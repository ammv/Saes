using Avalonia.Controls;
using Avalonia.Media;
using Grpc.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.KeyHolder
{
    public class KeyHolderFormViewModel : ViewModelFormBase<KeyHolderDto, KeyHolderDataRequest>
    {
        private CallInvoker _grpcChannel;

        public KeyHolderFormViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
            KeyHolderTypeCollection = new CollectionWithSelection<KeyHolderTypeDto>();
            UserCpiCollection = new CollectionWithSelection<BusinessEntityDto>();
        }

        protected override void _Configure(KeyHolderDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || CurrentMode == Core.Enums.FormMode.Edit)
            {
                DataRequest.KeyHolderID = dto.KeyHolderId;
                DataRequest.SerialNumber = dto.SerialNumber;
                DataRequest.UserCPI = dto.UserCPI;
                DataRequest.TypeID = dto.TypeId;
                DataRequest.SignFileId = dto.SignFileId;
            }
            else
            {
                DataRequest.KeyHolderID = 0;
            }
        }

        protected override void _ConfigureTitle()
        {
            switch (_currentMode)
            {
                case Core.Enums.FormMode.See:
                    Title = "Просмотр ключевого носителя";
                    break;
                case Core.Enums.FormMode.Edit:
                    Title = "Редактирование ключевого носителя";
                    break;
                case Core.Enums.FormMode.Add:
                    Title = "Добавление ключевого носителя";
                    break;
            }
        }

        [Reactive]
        public CollectionWithSelection<KeyHolderTypeDto> KeyHolderTypeCollection { get; set; }
        [Reactive]
        public CollectionWithSelection<BusinessEntityDto> UserCpiCollection { get; set; }

        protected override async Task _Loaded()
        {
            try
            {
                var client = new KeyHolderTypeService.KeyHolderTypeServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение записей типов ключевых носителей"));
                var response = await client.SearchAsync(new KeyHolderTypeLookup());
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                foreach (var item in response.Data)
                {
                    KeyHolderTypeCollection.Items.Add(item);
                }
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }

            try
            {
                var client = new BusinessEntityService.BusinessEntityServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение записей бизнес-сущностей"));
                var response = await client.SearchAsync(new BusinessEntityLookup());
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                foreach (var item in response.Data)
                {
                    UserCpiCollection.Items.Add(item);
                }
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }

        protected override async Task _OnAdd()
        {
            try
            {
                var service = new KeyHolderService.KeyHolderServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление ключевого носителя"));
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
                var service = new KeyHolderService.KeyHolderServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на редактирование ключевого носителя"));
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

        protected override Task _OnSee()
        {
            return Task.Delay(0);
        }

        protected override bool Validate()
        {
            return DataRequest.TypeID != null && !string.IsNullOrEmpty(DataRequest.SerialNumber);
        }
    }
}
