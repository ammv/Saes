using Avalonia.Controls;
using Avalonia.Media;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCPARecord
{
    public class JournalInstanceForCPARecordFormViewModel : ViewModelFormBase<JournalInstanceForCPARecordDto, JournalInstanceForCPARecordDataRequest>
    {
        private CallInvoker _grpcChannel;

        public JournalInstanceForCPARecordFormViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
            //JournalInstanceForCPARecordTypeCollection = new CollectionWithSelection<JournalInstanceForCPARecordTypeDto>();
            ReceivedFromCollection = new CollectionWithSelection<BusinessEntityDto>();
            OrganizationCollection = new CollectionWithSelection<OrganizationDto>();
            RecipientCollection = new CollectionWithSelection<OrganizationDto>();
        }

        protected override JournalInstanceForCPARecordDataRequest _ConfigureDataRequest(JournalInstanceForCPARecordDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || CurrentMode == Core.Enums.FormMode.Edit)
            {
                return new JournalInstanceForCPARecordDataRequest
                {
                    JournalInstanceForCPARecordID = dto.JournalInstanceForCPARecordId,
                    OrganizationID = dto.OrganizationId,
                    NameCPI = dto.NameCPI,
                    SerialCPI = dto.SerialCPI,
                    InstanceNumber = dto.InstanceNumber,
                    ReceivedFromID = dto.ReceivedFromId,
                    DateAndNumberCoverLetterReceive = dto.DateAndNumberCoverLetterReceive,
                    DateAndNumberCoverLetterSend = dto.DateAndNumberCoverLetterSend,
                    DateAndNumberConfirmationSend = dto.DateAndNumberConfirmationSend,
                    DateAndNumberCoverLetterReturn = dto.DateAndNumberCoverLetterReturn,
                    DateAndNumberConfirmationReturn = dto.DateAndNumberConfirmationReturn,
                    CommissioningDate = dto.CommissioningDate,
                    DecommissioningDate = dto.DecommissioningDate,
                    DestructionDate = dto.DestructionDate,
                    DestructionActNumber = dto.DestructionActNumber,
                    Note = dto.Note
                };
            }
            else
            {
                return new JournalInstanceForCPARecordDataRequest
                {
                    JournalInstanceForCPARecordID = 0,
                    OrganizationID = dto.OrganizationDto.OrganizationId
                };
            }
        }

        protected override void _ConfigureTitle()
        {
            switch (_currentMode)
            {
                case Core.Enums.FormMode.See:
                    Title = "Просмотр записи журнала поэкземплярного учета СКЗИ для ОКЗ";
                    break;

                case Core.Enums.FormMode.Edit:
                    Title = "Редактирование записи журнала поэкземплярного учета СКЗИ для ОКЗ";
                    break;

                case Core.Enums.FormMode.Add:
                    Title = "Добавление записи журнала поэкземплярного учета СКЗИ для ОКЗ";
                    break;
            }
        }

        private void InitiliazeCommands()
        {
            AddRecipientCommand = ReactiveCommand.Create(
                () =>
                {
                    LinkedRecipients.Add(RecipientCollection.Selected);
                    RecipientCollection.Items.Remove(RecipientCollection.Selected);
                },
                RecipientCollection.WhenAnyValue(x => x.Selected, x => !LinkedRecipients.Contains(x) && RecipientCollection.Selected != null)
                );
            DeleteRecipientCommand = ReactiveCommand.Create(
                () =>
                {
                    RecipientCollection.Items.Add(SelectedLinkedRecipient);
                    RecipientCollection.SelectedIndex = 0;
                    LinkedRecipients.Remove(SelectedLinkedRecipient);
                },
                this.WhenAnyValue(x => x.SelectedLinkedRecipient, LinkedRecipients.Contains));
        }

        public ReactiveCommand<Unit, Unit> AddRecipientCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteRecipientCommand { get; set; }

        private async Task _ConfigureCollections()
        {

            var organizationClient = new OrganizationService.OrganizationServiceClient(_grpcChannel);

            var client = new JournalInstanceCPAReceiverService.JournalInstanceCPAReceiverServiceClient(_grpcChannel);
            var requestInstaller = new JournalInstanceCPAReceiverLookup { RecordID = _dto.JournalInstanceForCPARecordId };

            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение получателей СКЗИ"));
            var responseReceivers = await client.SearchAsync(requestInstaller);

            MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка полученных данных"));
            foreach (var receiver in responseReceivers.Data)
            {
                var organizationResponse = await organizationClient.SearchAsync(new OrganizationLookup { BusinessEntityID = receiver.ReceiverId });

                foreach (var organization in organizationResponse.Data)
                {
                    LinkedRecipients.Add(organization);
                }

            }
        }

        [Reactive]
        public CollectionWithSelection<OrganizationDto> OrganizationCollection { get; set; }

        [Reactive]
        public CollectionWithSelection<BusinessEntityDto> ReceivedFromCollection { get; set; }

        [Reactive]
        public CollectionWithSelection<OrganizationDto> RecipientCollection { get; set; }

        [Reactive]
        public ObservableCollection<OrganizationDto> LinkedRecipients { get; set; }

        [Reactive]
        public OrganizationDto SelectedLinkedRecipient { get; set; }

        protected override async Task _Loaded()
        {
            try
            {
                if (_currentMode == Core.Enums.FormMode.See || _currentMode == Core.Enums.FormMode.Edit)
                {
                    await _ConfigureCollections();
                }
                await OrganizationCollectionLoad();
                await ReceivedFromCollectionLoad();
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка во время загрузки формы", ex.Message);
                Close();
            }
        }

        private async Task ReceivedFromCollectionLoad()
        {
            var client = new BusinessEntityService.BusinessEntityServiceClient(_grpcChannel);
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение записей бизнес-сущностей"));
            var response = await client.SearchAsync(new BusinessEntityLookup());
            MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
            ReceivedFromCollection.Items.Add(null);
            foreach (var item in response.Data)
            {
                ReceivedFromCollection.Items.Add(item);
            }

            MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
        }

        private async Task OrganizationCollectionLoad()
        {
            var client = new OrganizationService.OrganizationServiceClient(_grpcChannel);
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение организаций"));
            var response = await client.SearchAsync(new OrganizationLookup());
            MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
            OrganizationCollection.Items.Add(null);
            foreach (var item in response.Data)
            {
                OrganizationCollection.Items.Add(item);

                if (!LinkedRecipients.Contains(item) && item.OrganizationId != _dto.OrganizationId)
                {
                    RecipientCollection.Items.Add(item);
                }

            }

            MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
        }

        protected override async Task _OnAdd()
        {
            try
            {
                var service = new JournalInstanceForCPARecordService.JournalInstanceForCPARecordServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление новой записи журнала поэкземплярного учета СКЗИ для ОКЗ"));
                var response = await service.AddAsync(DataRequest);

                await UpdateBulkRecordData(response.Data.First().JournalInstanceForCPARecordId);

                MessageBus.Current.SendMessage(StatusData.Ok("Успешный успех"));

                await MessageBoxHelper.Success("Уведомление", "Успешно добавлено!");

                Callback?.Invoke(response.Data.FirstOrDefault());

            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));

                await MessageBoxHelper.Exception("Ошибка", $"Во время добавления произошла неизвестная ошибка: {ex.Message}");
            }
        }

        private async Task UpdateBulkRecordData(int journalInstanceForCPARecordId)
        {
            var receiverService = new JournalInstanceCPAReceiverService.JournalInstanceCPAReceiverServiceClient(_grpcChannel);
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на изменение получателей в записи журнала поэкземплярного учета СКЗИ для ОКЗ"));
            var installerRequest = new JournalInstanceCPAReceiverBulkUpdateRequest
            {
                RecordID = journalInstanceForCPARecordId,
            };
            installerRequest.ReceiversIds.AddRange(LinkedRecipients.Select(x => x.BusinessEntityId));
            await receiverService.BulkUpdateAsync(installerRequest);
        }

        protected override async Task _OnEdit()
        {
            try
            {
                var service = new JournalInstanceForCPARecordService.JournalInstanceForCPARecordServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на редактирование записи журнала поэкземплярного учета СКЗИ для ОКЗ"));
                var response = await service.EditAsync(DataRequest);
                await UpdateBulkRecordData(DataRequest.JournalInstanceForCPARecordID.Value);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                if (response.Result)
                {

                    MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
                    await MessageBoxHelper.Success("Уведомление", "Успешно изменено!");
                }
                else
                {
                    MessageBus.Current.SendMessage(StatusData.Error("Ошибка"));
                    await MessageBoxHelper.Exception("Ошибка", "Не удалось измезменить");
                }

            }
            catch (Exception ex)
            {
                await MessageBoxHelper.Exception("Ошибка", $"Во время изменения произошла неизвестная ошибка: {ex.Message}");
            }
        }

        protected override async Task _OnSee()
        {
            await Task.CompletedTask;
        }

        protected override bool Validate()
        {
            return true;
            //return DataRequest.TypeID != null && !string.IsNullOrEmpty(DataRequest.SerialNumber);
        }
    }
}