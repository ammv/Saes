using Grpc.Core;
using Saes.Protos.Auth;
using OtpNet;
using Saes.Models;
using Microsoft.EntityFrameworkCore;
using Saes.GrpcServer.Services.Interfaces;
using Azure;
using Saes.Models.Schemas;
using System.Security.Authentication;
using Google.Protobuf.WellKnownTypes;

namespace Saes.GrpcServer.ProtoServices.AuthService
{
    public class AuthenticationService: Authentication.AuthenticationBase
    {
        private static Dictionary<string, int> _userFirstAuthTokensMap = new Dictionary<string, int>();

        private readonly SaesContext _ctx;
        private readonly ITokenService _tokenService;
        private readonly ILogAuthenticationService _logAuthenticationService;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(SaesContext ctx, ITokenService tokenService, ILogAuthenticationService logAuthenticationService, ILogger<AuthenticationService> logger)
        {
            _ctx = ctx;
            _tokenService = tokenService;
            _logAuthenticationService = logAuthenticationService;
            _logger = logger;
            _logger.LogInformation($"Context Id: {0}", _ctx.ContextId);
        }
        public override async Task<FirstFactorAuthenticateResponse> FirstFactorAuthenticate(FirstFactorAuthenticateRequest request, ServerCallContext context)
        {
            FirstFactorAuthenticateResponse response = new FirstFactorAuthenticateResponse();

            // 1. Find user
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Login == request.Login);

            string answer;

            if (user == null)
            {
                answer = $"User with login '{request.Login}' not found";
                await _logAuthenticationService.AddLogAsync(request.Login, false, false, answer);
                throw new RpcException(new Status(StatusCode.NotFound, "Неправильный логин или пароль"));
            }

            // 2. Check password
            bool checkResult = await _ctx.udfVerifyUserAsync(user.Login, request.Password);

            if (!checkResult)
            {
                answer = $"Incorrect password";
                await _logAuthenticationService.AddLogAsync(request.Login, false, false, answer);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Неправильный логин или пароль"));
            }

            // 3. Check user has2Fa

            answer = $"Success First Factor Authenticate";

            await _logAuthenticationService.AddLogAsync(request.Login, true, user.TwoFactorEnabled == false, answer);

            if (user.TwoFactorEnabled == true)
            {
                response.Has2FA = true;
                response.Token = _tokenService.GenerateToken(64);
                _userFirstAuthTokensMap[response.Token] = user.UserId;
  
            }
            else
            {
                string sessionKey = await _ctx.uspCreateSessionAsync(user.UserId, DateTime.Now.AddHours(Configuration.Cofiguration.SessionExpiredHours));
                response.SessionKey = sessionKey;
            }   

            return response;
        }

        public override async Task<SecondFactorAuthenticateResponse> SecondFactorAuthenticate(SecondFactorAuthenticateRequest request, ServerCallContext context)
        {
            string answer;
            if (request.Token == null || string.IsNullOrEmpty(request.OtpPassword))
            {
                answer = "Invalid request data.";
                await _logAuthenticationService.AddLogAsync(null, true, false, answer);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "В запросе не был указан токен или одноразовый пароль"));
            }
            if (!_userFirstAuthTokensMap.TryGetValue(request.Token, out int userId))
            {
                answer = "Invalid token.";
                await _logAuthenticationService.AddLogAsync(null, true, false, answer);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "В запросе был указан неккоректный токен"));
            }

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                answer = "Invalid token.";
                await _logAuthenticationService.AddLogAsync(null, true, false, answer);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Неккоректный токен"));
            }

            // Проверьте, как хранится TotpSecretKey и используйте соответствующую кодировку
            byte[] userOtpTokenBytes = Base32Encoding.ToBytes(user.TotpSecretKey);
            var totp = new Totp(userOtpTokenBytes, timeCorrection: new TimeCorrection(request.SendTime.ToDateTime()));

            //totp.VerifyTotp(request.OtpPassword, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);

            if (!totp.VerifyTotp(request.OtpPassword, out long timeStepMatched, new VerificationWindow(previous: 3, future: 3)))
            {
                answer = "Bad TOTP password";
                await _logAuthenticationService.AddLogAsync(user.Login, true, false, answer);
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Одноразовый пароль неправильный"));
            }

            //if (!totp.VerifyTotp(request.SendTime.ToDateTime(), request.OtpPassword, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay))
            //{
            //    answer = "Bad TOTP password";
            //    await _logAuthenticationService.AddLogAsync(user.Login, true, false, answer);
            //    throw new RpcException(new Status(StatusCode.InvalidArgument, "Одноразовый пароль неправильный"));
            //}


            answer = $"Success Second Factor Authenticate";
            await _logAuthenticationService.AddLogAsync(user.Login, true, true, answer);

            string sessionKey = await _ctx.uspCreateSessionAsync(userId, DateTime.Now.AddHours(Configuration.Cofiguration.SessionExpiredHours));

            if (sessionKey == null)
            {
                answer = $"uspCreateSession returned null value";
                await _logAuthenticationService.AddLogAsync(user.Login, true, true, answer);
                throw new RpcException(new Status(StatusCode.Internal, "Не удалось создать сессию"));
            }

            var response = new SecondFactorAuthenticateResponse
            {
                SessionKey = sessionKey
            };

            await _ctx.SaveChangesAsync();

            return response;
        }

        public override async Task<ValidateSessionKeyResponse> ValidateSessionKey(ValidateSessionKeyRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.SessionKey))
            {
                return new ValidateSessionKeyResponse { Result = false, Message = "Invalid session key" };
            }

            var userSession = await _ctx.UserSessions.FirstOrDefaultAsync(x => x.SessionKey == request.SessionKey);
            if (userSession == null)
            {
                return new ValidateSessionKeyResponse { Result = false, Message = "Invalid session key" };
            }

            if (userSession.IsExpired == true)
            {
                return new ValidateSessionKeyResponse { Result = false, Message = "Session key was expired" };
            }

            return new ValidateSessionKeyResponse { Result = true };
        }
    }
}
