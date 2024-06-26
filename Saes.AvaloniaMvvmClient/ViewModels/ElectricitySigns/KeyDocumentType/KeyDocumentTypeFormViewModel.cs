using Avalonia.Controls;
using Avalonia.Media;
using Grpc.Core;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.KeyDocumentType
{
    public class KeyDocumentTypeFormViewModel : ViewModelFormBase<KeyDocumentTypeDto, KeyDocumentTypeDataRequest>
    {
        private CallInvoker _grpcChannel;

        public KeyDocumentTypeFormViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
        }

        protected override bool Validate()
        {
            return !string.IsNullOrEmpty(DataRequest.Name);
        }

        protected override KeyDocumentTypeDataRequest _ConfigureDataRequest(KeyDocumentTypeDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || CurrentMode == Core.Enums.FormMode.Edit)
            {
                return new KeyDocumentTypeDataRequest
                {
                    Name = dto.Name,
                    KeyDocumentTypeID = dto.KeyDocumentTypeId
                };
                
            }
            else
            {
                return new KeyDocumentTypeDataRequest
                {
                    KeyDocumentTypeID = 0
                };
            }
        }

        protected override void _ConfigureTitle()
        {
            switch (_currentMode)
            {
                case Core.Enums.FormMode.See:
                    Title = "Просмотр типа ключевого документа";
                    break;
                case Core.Enums.FormMode.Edit:
                    Title = "Редактирование типа ключевого документа";
                    break;
                case Core.Enums.FormMode.Add:
                    Title = "Добавление типа ключевого документа";
                    break;
            }
        }

        protected override async Task _Loaded()
        {
            await Task.Delay(0);
        }

        protected override async Task _OnAdd()
        {
            try
            {
                var service = new KeyDocumentTypeService.KeyDocumentTypeServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление типа ключевого документа"));
                var response = await service.AddAsync(DataRequest);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Callback(response.Data.FirstOrDefault());

            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }

        protected override async Task _OnEdit()
        {
            try
            {
                var service = new KeyDocumentTypeService.KeyDocumentTypeServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на редактирование типа ключевого документа"));
                var response = await service.EditAsync(DataRequest);
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
        }

        protected override async Task _OnPreFormCommand()
        {
            await Task.CompletedTask;
        }

        protected override async Task _OnSee()
        {
            await Task.CompletedTask;
        }
    }
}
