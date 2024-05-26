using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ReactiveUI;
using Saes.Protos.Auth;
using System;
using System.Reactive.Linq;

namespace Saes.AvaloniaMvvmClientTest.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel(GrpcChannel grpcChannel)
    {
        _grpcChannel = grpcChannel;
        UserAuthentication = new UserAuthenticationViewModel(_grpcChannel);
        ContentViewModel = UserAuthentication;

        UserAuthentication.AuthCommand.Subscribe(UserAuthentication_AuthCommandExecuted);

    }

    private ViewModelBase _contentViewModel;
    private readonly GrpcChannel _grpcChannel;

    public ViewModelBase ContentViewModel
    {
        get { return _contentViewModel; }
        private set { this.RaiseAndSetIfChanged(ref _contentViewModel, value); }
    }

    private async void InputTotpPasswordViewModel_SuccessCommandExecuted((string otpPassword, FirstFactorAuthenticateResponse response) data)
    {
        var authService = new Authentication.AuthenticationClient(_grpcChannel);

        var secondFactorAuthenticateRequest = new SecondFactorAuthenticateRequest
        {
            Token = data.response.Token,
            OtpPassword = data.otpPassword,
            SendTime = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        // First response

        SecondFactorAuthenticateResponse secondFactorAuthenticateResponse;

        try
        {
            UserAuthentication.Status = "secondFactorAuthenticate requesting";
            secondFactorAuthenticateResponse = await authService.SecondFactorAuthenticateAsync(secondFactorAuthenticateRequest);
            UserAuthentication.Status = "secondFactorAuthenticate response";
        }
        catch (Exception ex)
        {
            UserAuthentication.Status = ex.Message;
            goto Finish;
        }

        UserAuthentication.Status = secondFactorAuthenticateResponse.SessionKey;

        Finish:

        ContentViewModel = UserAuthentication;

        //return firstFactorAuthenticateResponse;
    }

    private void UserAuthentication_AuthCommandExecuted(FirstFactorAuthenticateResponse response)
    {
        if (response == null) return;
        if (response.Has2FA)
        {
            var inputTotpPasswordviewModel = new InputTotpPasswordViewModel();

            inputTotpPasswordviewModel.SuccessCommand.Select(x => (x, response)).Subscribe(InputTotpPasswordViewModel_SuccessCommandExecuted);

            ContentViewModel = inputTotpPasswordviewModel;
        }
        else
        {
            UserAuthentication.Status = response.SessionKey;

        }
    }

    public UserAuthenticationViewModel UserAuthentication { get; set; }

}
