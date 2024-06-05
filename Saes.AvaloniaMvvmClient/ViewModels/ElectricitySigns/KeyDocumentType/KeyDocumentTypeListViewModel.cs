using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.KeyDocumentType;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.KeyDocumentType
{
    public class KeyDocumentTypeListViewModel : ViewModelTabListBase<KeyDocumentTypeDto, KeyDocumentTypeLookup>
    {
        private readonly IDialogService _dialogService;
        private CallInvoker _grpcChannel;

        public KeyDocumentTypeListViewModel(IGrpcChannelFactory grpcChannelFactory, IDialogService dialogService)
        {
            TabTitle = "Типы ключевых документов";
            _grpcChannel = grpcChannelFactory.CreateChannel();
            _dialogService = dialogService;
        }
        public override async Task<bool> CloseAsync()
        {
            return await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите закрыть вкладку \"{TabTitle}\"");
        }

        protected override async Task OnAddCommand()
        {
            var vm = App.ServiceProvider.GetService<KeyDocumentTypeFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Add, async (f) => {
                await MessageBoxHelper.Question("Вопрос", $"{f.Name} - Вы довольны результатом?");
            }, SelectedEntity);

            _dialogService.ShowDialog(vm);

            await _Search();
        }

        protected override async Task OnCopyCommand()
        {
            if (SelectedEntity == null) return;

            await _Search();
        }

        protected override async Task OnDeleteCommand()
        {
            if (SelectedEntity == null) return;

            if (!await MessageBoxHelper.Question("Вопрос", 
                $"Вы уверены, что хотите удалить данную запись с № {_selectedEntity.KeyDocumentTypeId} ?")) return;

            try
            {
                var client = new KeyDocumentTypeService.KeyDocumentTypeServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на удаление типа ключевого документа"));
                var response = await client.RemoveAsync(new KeyDocumentTypeLookup { KeyDocumentTypeID = SelectedEntity.KeyDocumentTypeId});
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));

                if(response.Result)
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

            var vm = App.ServiceProvider.GetService<KeyDocumentTypeFormViewModel>();

            vm.Configure(Core.Enums.FormMode.Edit, async (f) => {
                await MessageBoxHelper.Question("Вопрос", $"{f.Name} - Вы довольны результатом?");
            }, SelectedEntity);

            _dialogService.ShowDialog(vm);

            await _Search();
        }

        protected override async Task OnSeeCommand()
        {
            var vm = App.ServiceProvider.GetService<KeyDocumentTypeFormViewModel>();

            vm.Configure(Core.Enums.FormMode.See, async (f) => {
                await MessageBoxHelper.Question("Вопрос", $"{f.Name} - Вы довольны результатом?");
            }, SelectedEntity);

            _dialogService.ShowDialog(vm);

            await _Search();
        }

        protected override async Task _Export()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task _Loaded()
        {
            await _Search();
        }

        protected override async Task _Search()
        {
            var client = new KeyDocumentTypeService.KeyDocumentTypeServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение типов ключевых документов"));
                var response = await client.SearchAsync(Lookup);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<KeyDocumentTypeDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }
    }
}
