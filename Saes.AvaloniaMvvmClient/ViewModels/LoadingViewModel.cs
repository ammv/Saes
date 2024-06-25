using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Fody.Helpers;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Impementations;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication;
using Saes.AvaloniaMvvmClient.ViewModels.MainMenu;
using Saes.Protos;
using Saes.Protos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels
{
    public class LoadingViewModel: ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IGrpcChannelFactory _grpcChannelFactory;
        private readonly ISessionKeyService _sessionKeyService;
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;

        [Reactive]
        public string LoadingStatus { get; set; }

        public LoadingViewModel(INavigationServiceFactory navigationServiceFactory, ISessionKeyService sessionKeyService, IGrpcChannelFactory grpcChannelFactory, IUserService userService, IServiceProvider serviceProvider)
        {
            _navigationService = navigationServiceFactory.Singleton;
            _grpcChannelFactory = grpcChannelFactory;
            _sessionKeyService = sessionKeyService;
            _userService = userService;
            _serviceProvider = serviceProvider;
        }
        
        public async Task LoadedAsync()
        {
            LoadingStatus = "Попытка подключиться к серверу";
            // Выполнение асинхронной операции в фоновом потоке
            bool isConnected = await Task.Run(async () =>
            {
                return await GrpcHelper.CheckConnection(_grpcChannelFactory.CreateChannel(), delay: 1000, attempts: 30);
            });

            if (!isConnected)
            {
                await WindowManager.CloseWithException(MainViewModel.Selector, "Ошибка при подключении к серверу", $"Сервер недоступен. Обратитесь к системному администратору или повторите попытку позже.");
            }

            LoadingStatus = "Проверка наличия существующей сессии";
            if (_sessionKeyService.GetSessionKey() == null)
            {
                LoadingStatus = "Навигация к окну аутенфикации";
                _navigationService.NavigateTo(_serviceProvider.GetService<AuthenticationMainViewModel>());
                return;
            }

            try
            {
                LoadingStatus = "Проверка сессии";
                var authService = new Protos.Auth.Authentication.AuthenticationClient(_grpcChannelFactory.CreateChannel());

                var request = new ValidateSessionKeyRequest { SessionKey = _sessionKeyService.GetSessionKey() };

                var response = await authService.ValidateSessionKeyAsync(request);

                if (response.Result)
                {
                    LoadingStatus = "Загрузка прав пользователя";
                    _userService.LoadRights();
                    LoadingStatus = "Навигация к главному окну";
                    _navigationService.NavigateTo(_serviceProvider.GetService<MainMenuViewModel>());
                }
                else
                {
                    LoadingStatus = "Навигация к окну аутенфикации";
                    _navigationService.NavigateTo(_serviceProvider.GetService<AuthenticationMainViewModel>());
                }

            }
            catch (RpcException ex)
            {
                if (ex.Status.StatusCode == StatusCode.Unavailable)
                {
                    await WindowManager.CloseWithException(MainViewModel.Selector,"Ошибка при подключении к серверу", $"Сервер не доступен в данный момент, обратитесь к системному администратору или повторите попытку позже:\n{ex.Status.Detail}");
                }
                else
                {
                    await WindowManager.CloseWithException(MainViewModel.Selector,"Ошибка при подключении к серверу", $"Неизвестная ошибка при попытке подключения к серверу, обратитесь к системному администратору или повторите попытку позже:\n{ex.Status.Detail}");
                }
            }
            catch (Exception ex)
            {
                await WindowManager.CloseWithException(MainViewModel.Selector,"Ошибка при подключении к серверу", $"Во время загрузки программы произошла ошибка:\n{ex.Message}");
            }
        }
    }

    
}
