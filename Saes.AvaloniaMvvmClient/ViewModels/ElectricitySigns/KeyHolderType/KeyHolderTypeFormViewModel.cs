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

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.KeyHolderType
{
    public class KeyHolderTypeFormViewModel : ViewModelFormBase<KeyHolderTypeDto, KeyHolderTypeDataRequest>
    {
        private CallInvoker _grpcChannel;

        public KeyHolderTypeFormViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
        }

        protected override bool Validate()
        {
            return !string.IsNullOrEmpty(DataRequest.Name);
        }

        protected override KeyHolderTypeDataRequest _ConfigureDataRequest(KeyHolderTypeDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || CurrentMode == Core.Enums.FormMode.Edit)
            {
                return new KeyHolderTypeDataRequest
                {
                    Name = dto.Name,
                    KeyHolderTypeID = dto.KeyHolderTypeId
                };
                
            }
            else
            {
                return new KeyHolderTypeDataRequest
                {
                    KeyHolderTypeID = 0
                };
            }
        }

        protected override void _ConfigureTitle()
        {
            switch (_currentMode)
            {
                case Core.Enums.FormMode.See:
                    Title = "Просмотр типа ключевого носителя";
                    break;
                case Core.Enums.FormMode.Edit:
                    Title = "Редактирование типа ключевого носителя";
                    break;
                case Core.Enums.FormMode.Add:
                    Title = "Добавление типа ключевого носителя";
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
                var service = new KeyHolderTypeService.KeyHolderTypeServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление типа ключевого носителя"));
                var response = await service.AddAsync(DataRequest);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Callback(response.Data.FirstOrDefault());
                MessageBus.Current.SendMessage(StatusData.Ok("Обработка результатов"));

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
                var service = new KeyHolderTypeService.KeyHolderTypeServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на редактирование типа ключевого носителя"));
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
            
        }

        protected override async Task _OnSee()
        {
            await Task.CompletedTask;
        }
    }
}
