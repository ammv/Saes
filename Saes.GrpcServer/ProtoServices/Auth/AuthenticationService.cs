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
                throw new RpcException(new Status(StatusCode.NotFound, answer));
            }

            // 2. Check password
            bool checkResult = await _ctx.udfVerifyUserAsync(user.Login, request.Password);

            if (!checkResult)
            {
                answer = $"Incorrect password";
                await _logAuthenticationService.AddLogAsync(request.Login, false, false, answer);
                throw new RpcException(new Status(StatusCode.InvalidArgument, answer));
            }

            // 3. Check user has2Fa

            if(user.TwoFactorEnabled == true)
            {
                response.Has2FA = true;
                response.Token = _tokenService.GenerateToken(64);
                _userFirstAuthTokensMap[response.Token] = user.UserId;
  
            }
            else
            {
                response.SessionKey = _tokenService.GenerateToken(128);
            }

            answer = $"Success First Factor Authenticate";
            await _logAuthenticationService.AddLogAsync(request.Login, true, false, answer);

            return response;
        }

        public override async Task<SecondFactorAuthenticateResponse> SecondFactorAuthenticate(SecondFactorAuthenticateRequest request, ServerCallContext context)
        {
            string answer;
            if (request.Token == null || string.IsNullOrEmpty(request.OtpPassword))
            {
                answer = "Invalid request data.";
                await _logAuthenticationService.AddLogAsync(null, true, false, answer);
                throw new AuthenticationException(answer);
            }
            if (!_userFirstAuthTokensMap.TryGetValue(request.Token, out int userId))
            {
                answer = "Invalid token.";
                await _logAuthenticationService.AddLogAsync(null, true, false, answer);
                throw new AuthenticationException(answer);
            }

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                answer = "Invalid token.";
                await _logAuthenticationService.AddLogAsync(null, true, false, answer);
                throw new AuthenticationException(answer);
            }

            // Проверьте, как хранится TotpSecretKey и используйте соответствующую кодировку
            byte[] userOtpTokenBytes = Base32Encoding.ToBytes(user.TotpSecretKey);
            var totp = new Totp(userOtpTokenBytes);

            string computedTotp = totp.ComputeTotp(request.SendTime.ToDateTime());

            if (!totp.VerifyTotp(request.SendTime.ToDateTime(), request.OtpPassword, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay))
            {
                answer = "Bad TOTP password";
                await _logAuthenticationService.AddLogAsync(user.Login, true, false, answer);
                throw new RpcException(new Status(StatusCode.InvalidArgument, answer));
            }

            string sessionKey = await _ctx.uspCreateSessionAsync(userId, request.SendTime.ToDateTime().ToLocalTime().AddHours(Configuration.Cofiguration.SessionExpiredHours));

            answer = $"Success Second Factor Authenticate";
            await _logAuthenticationService.AddLogAsync(user.Login, true, true, answer);

            if (sessionKey == null)
            {
                answer = $"uspCreateSession returned null value";
                await _logAuthenticationService.AddLogAsync(user.Login, true, true, answer);
                throw new RpcException(new Status(StatusCode.Internal, answer));
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
