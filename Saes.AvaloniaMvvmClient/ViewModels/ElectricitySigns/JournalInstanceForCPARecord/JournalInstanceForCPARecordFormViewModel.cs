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

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCPARecord
{
    public class JournalInstanceForCPARecordFormViewModel : ViewModelFormBase<JournalInstanceForCPARecordDto, JournalInstanceForCPARecordDataRequest>
    {
        private CallInvoker _grpcChannel;

        public JournalInstanceForCPARecordFormViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
            //JournalInstanceForCPARecordTypeCollection = new CollectionWithSelection<JournalInstanceForCPARecordTypeDto>();
            ReceiverCollection = new CollectionWithSelection<BusinessEntityDto>();
            OrganizationCollection = new CollectionWithSelection<OrganizationDto>();
        }

        protected override JournalInstanceForCPARecordDataRequest _Configure(JournalInstanceForCPARecordDto dto)
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
                    JournalInstanceForCPARecordID = 0
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

        [Reactive]
        public CollectionWithSelection<OrganizationDto> OrganizationCollection { get; set; }
        [Reactive]
        public CollectionWithSelection<BusinessEntityDto> ReceiverCollection { get; set; }

        protected override async Task _Loaded()
        {
            try
            {
                var client = new OrganizationService.OrganizationServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение организаций"));
                var response = await client.SearchAsync(new OrganizationLookup());
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                OrganizationCollection.Items.Add(null);
                foreach (var item in response.Data)
                {
                    OrganizationCollection.Items.Add(item);
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
                ReceiverCollection.Items.Add(null);
                foreach (var item in response.Data)
                {
                    ReceiverCollection.Items.Add(item);
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
                var service = new JournalInstanceForCPARecordService.JournalInstanceForCPARecordServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление новой записи журнала поэкземплярного учета СКЗИ для ОКЗ"));
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
                var service = new JournalInstanceForCPARecordService.JournalInstanceForCPARecordServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на редактирование записи журнала поэкземплярного учета СКЗИ для ОКЗ"));
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
            return true;
            //return DataRequest.TypeID != null && !string.IsNullOrEmpty(DataRequest.SerialNumber);
        }
    }
}
