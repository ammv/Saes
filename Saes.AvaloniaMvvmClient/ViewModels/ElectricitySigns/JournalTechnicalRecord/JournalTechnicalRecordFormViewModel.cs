using DynamicData;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalTechnicalRecord
{
    public class JournalTechnicalRecordFormViewModel : ViewModelFormBase<JournalTechnicalRecordDto, JournalTechnicalRecordDataRequest>
    {
        private readonly CallInvoker _grpcChannel;

        public JournalTechnicalRecordFormViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
            OrganizationCollection = new CollectionWithSelection<OrganizationDto>();
            KeyDocumentTypeCollection = new CollectionWithSelection<KeyDocumentTypeDto>();
        }

        [Reactive]
        public CollectionWithSelection<OrganizationDto> OrganizationCollection { get; set; }
        [Reactive]
        public CollectionWithSelection<KeyDocumentTypeDto> KeyDocumentTypeCollection { get; set; }

        protected override bool Validate()
        {
            return true;
        }

        protected override JournalTechnicalRecordDataRequest _ConfigureDataRequest(JournalTechnicalRecordDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || CurrentMode == Core.Enums.FormMode.Edit)
            {

                return new JournalTechnicalRecordDataRequest
                {
                    JournalTechnicalRecordID = dto.JournalTechnicalRecordId,
                    OrganizationID = dto.OrganizationId,
                    Date = dto.Date,
                    TypeAndSerialUsedCPI = dto.TypeAndSerialUsedCPI,
                    RecordOnMaintenanceCPI = dto.RecordOnMaintenanceCPI,
                    KeyDocumentTypeID = dto.KeyDocumentTypeId,
                    SerialCPIAndKeyDocumentInstanceNumber = dto.SerialCPIAndKeyDocumentInstanceNumber,
                    NumberOneTimeKeyCarrierCPIZoneCryptoKeysInserted = dto.NumberOneTimeKeyCarrierCPIZoneCryptoKeysInserted,
                    DestructionDate = dto.DestructionDate,
                    ActNumber = dto.ActNumber,
                    Note = dto.Note
                };
            }
            else
            {
                return new JournalTechnicalRecordDataRequest
                {
                    OrganizationID = dto.OrganizationDto.OrganizationId
                };
            }
        }

        protected override void _ConfigureTitle()
        {
            switch (_currentMode)
            {
                case Core.Enums.FormMode.See:
                    Title = "Просмотр записи журнала технического (аппаратного)";
                    break;
                case Core.Enums.FormMode.Edit:
                    Title = "Изменение записи журнала технического (аппаратного)";
                    break;
                case Core.Enums.FormMode.Add:
                    Title = "Редактирование записи журнала технического (аппаратного)";
                    break;
            }
        }

        protected override async Task _Loaded()
        {
            try
            {
                await _OrganizationCollectionLoad();
                await _KeyDocumentTypeCollectionLoad();
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка во время загрузки формы", ex.Message);
                Close();
            }

        }

        private async Task _KeyDocumentTypeCollectionLoad()
        {
            var client = new KeyDocumentTypeService.KeyDocumentTypeServiceClient(_grpcChannel);
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение типов ключевых документов"));
            var response = await client.SearchAsync(new KeyDocumentTypeLookup { KeyDocumentTypeID = _dto.KeyDocumentTypeId });
            MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
            foreach (var item in response.Data)
            {
                KeyDocumentTypeCollection.Items.Add(item);
            }
            MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
        }

        private async Task _OrganizationCollectionLoad()
        {
            var client = new OrganizationService.OrganizationServiceClient(_grpcChannel);
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение организаций"));
            var response = await client.SearchAsync(new OrganizationLookup { OrganizationID = _dto.OrganizationId });
            MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
            foreach (var item in response.Data)
            {
                OrganizationCollection.Items.Add(item);
            }

            MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
        }

        protected override async Task _OnAdd()
        {
            try
            {
                var service = new JournalTechnicalRecordService.JournalTechnicalRecordServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление новой записи журнала технического(аппратного)"));
                var response = await service.AddAsync(DataRequest);

                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));

                await MessageBoxHelper.Success("Уведомление", "Успешно добавлено!");

                Callback?.Invoke(response.Data.FirstOrDefault());

            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время добавления произошла неизвестная ошибка: {ex.Message}");

            }
        }

        protected override async Task _OnEdit()
        {
            try
            {
                var service = new JournalTechnicalRecordService.JournalTechnicalRecordServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на изменение записи журнала технического(аппратного)"));
                var response = await service.EditAsync(DataRequest);

                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));

                await MessageBoxHelper.Success("Уведомление", "Успешно изменено!");
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время добавления произошла неизвестная ошибка: {ex.Message}");
            }
        }

        protected override async Task _OnSee()
        {
            await Task.CompletedTask;
        }
    }
}
