using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Core.Attributes;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.KeyHolder
{
    [RightScope("key_holder_see")]
    public class KeyHolderListViewModel : ViewModelTabListBase<KeyHolderDto, KeyHolderLookup>
    {
        private readonly IDialogService _dialogService;
        private CallInvoker _grpcChannel;

        public KeyHolderListViewModel(IGrpcChannelFactory grpcChannelFactory, IDialogService dialogService)
        {
            TabTitle = "Ключевые носители";
            _grpcChannel = grpcChannelFactory.CreateChannel();
            _dialogService = dialogService;
            KeyHolderTypeCollection = new CollectionWithSelection<KeyHolderTypeDto>();
        }

        protected override async Task OnAddCommand()
        {
            
            var vm = App.ServiceProvider.GetService<KeyHolderFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Add, null, SelectedEntity);

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task OnCopyCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task OnDeleteCommand()
        {
            if (SelectedEntity == null) return;

            if (!await MessageBoxHelper.Question("Вопрос",
                $"Вы уверены, что хотите удалить данную запись с № {_selectedEntity.KeyHolderId} ?")) return;

            try
            {
                var client = new KeyHolderService.KeyHolderServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на удаление типа ключевого носителя"));
                var response = await client.RemoveAsync(new KeyHolderLookup { KeyHolderID = SelectedEntity.KeyHolderId });
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));

                if (response.Result)
                {
                    MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
                }
                else
                {
                    MessageBus.Current.SendMessage(StatusData.Error("Ошибка"));
                }
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }

            await _Search();
        }

        protected override async Task OnEditCommand()
        {
            if (SelectedEntity == null) return;

            var vm = App.ServiceProvider.GetService<KeyHolderFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Edit, null, SelectedEntity);

            await _dialogService.ShowDialog(vm);
        }

        [Reactive]
        public CollectionWithSelection<KeyHolderTypeDto> KeyHolderTypeCollection { get; set; }

        protected override async Task OnSeeCommand()
        {
            if (SelectedEntity == null) return;

            var vm = App.ServiceProvider.GetService<KeyHolderFormViewModel>();

            vm.Configure(Core.Enums.FormMode.See, null, SelectedEntity);

            await _dialogService.ShowDialog(vm);
        }

        protected override async Task _Export()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task _Loaded()
        {
            await _Search();
            try
            {
                var client = new KeyHolderTypeService.KeyHolderTypeServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение записей типов ключевых носителей"));
                var response = await client.SearchAsync(new KeyHolderTypeLookup());
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                foreach(var item in response.Data)
                {
                    KeyHolderTypeCollection.Items.Add(item);
                }
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }

        protected override async Task _Search()
        {
            var client = new KeyHolderService.KeyHolderServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение записей"));
                var response = await client.SearchAsync(Lookup);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<KeyHolderDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }
    }
}
