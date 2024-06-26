using Avalonia.Controls.Primitives;
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
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.HumanResources.Organization
{
    public class OrganizationFormViewModel : ViewModelFormBase<OrganizationDto, OrganizationDataRequest>
    {
        private readonly CallInvoker _grpcChannel;

        [Reactive]
        public OrganizationContactDto OrganizationContactTemp { get; set; } = new OrganizationContactDto();

        public OrganizationFormViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
            OrganizationContactCollection = new CollectionWithSelection<OrganizationContactDto>();
            ContactTypeCollection = new CollectionWithSelection<ContactTypeDto>();
            _InitializeCommands();
        }
        protected override bool Validate()
        {
            return false;
        }

        private async Task<bool> _Validate()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(_dataRequest.INN) && !(_dataRequest.INN.Length == 10 || _dataRequest.INN.Length == 12))
            {
                sb.Append("ИНН должен состоять из 10 или 12 цифр (Если ИП)\n");
            }
            if (!string.IsNullOrEmpty(_dataRequest.KPP) && _dataRequest.KPP.Length != 9)
            {
                sb.Append("КПП должен состоять из 9 цифр\n");
            }
            if (!string.IsNullOrEmpty(_dataRequest.OKPO) && _dataRequest.OKPO.Length != 8)
            {
                sb.Append("ОКПО должен состоять из 8 цифр\n");
            }
            if (!string.IsNullOrEmpty(_dataRequest.OGRN) && !(_dataRequest.OGRN.Length == 13 || _dataRequest.OGRN.Length == 15))
            {
                sb.Append("ОГРН должен состоять из 13 или 15 цифр (Если ИП)\n");
            }

            if (sb.Length == 0)
            {
                return true;
            }

            await MessageBoxHelper.Exception("Ошибка заполнения", sb.ToString());

            return false;
        }


        private void _InitializeCommands()
        {
            var selectedObservable = OrganizationContactCollection.WhenAny(x => x.Selected, x => x != null);
            AddOrganizationContanctCommand = ReactiveCommand.Create(OnAddOrganizationContanctCommand);
            DeleteOrganizationContanctCommand = ReactiveCommand.Create(OnDeleteOrganizationContanctCommand, selectedObservable);
            StartEditOrganizationContanctCommand = ReactiveCommand.Create<FlyoutBase>(OnStartEditOrganizationContanctCommand);
            EditFinalOrganizationContanctCommand = ReactiveCommand.Create(OnEditFinalOrganizationContanctCommand, selectedObservable);
            CloseEditOrganizationContanctCommand = ReactiveCommand.Create(OnCloseEditOrganizationContanctCommand, selectedObservable);
        }

        protected override OrganizationDataRequest _ConfigureDataRequest(OrganizationDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || _currentMode == Core.Enums.FormMode.Edit)
            {
                return new OrganizationDataRequest
                {
                    OrganizationID = dto.OrganizationId,
                    DirectorFullName = dto.DirectorFullName,
                    ChiefAccountantFullName = dto.ChiefAccountantFullName,
                    INN = dto.INN,
                    KPP = dto.KPP,
                    OKPO = dto.OKPO,
                    OKVED = dto.OKVED,
                    FullName = dto.FullName,
                    ShortName = dto.ShortName,
                    BusinessAddressDto = dto.BusinessAddressDto,
                    DateOfAssignmentOGRN = dto.DateOfAssignmentOGRN,
                    // TODO: fix 
                    IsOwnerJournalAccountingCPI = true
                };
            }
            else
            {
                return new OrganizationDataRequest { IsOwnerJournalAccountingCPI = true };
            }
        }

        private void OnAddOrganizationContanctCommand()
        {
            OrganizationContactCollection.Items.Add(OrganizationContactTemp);
            OrganizationContactCollection.Selected = OrganizationContactTemp;
           // OrganizationContactCollection.Selected.ContactTypeId = OrganizationContactTemp.ContactTypeDto.ContactTypeId;
            OrganizationContactTemp = new OrganizationContactDto();
        }

        private void OnDeleteOrganizationContanctCommand()
        {
            OrganizationContactCollection.Items.Remove(OrganizationContactCollection.Selected);
        }

        private void OnStartEditOrganizationContanctCommand(FlyoutBase flyout)
        {
            if (OrganizationContactCollection.Selected == null)
            {
                flyout?.Hide();
            }
            else
            {
                OrganizationContactTemp = OrganizationContactCollection.Selected.Clone();
            }
        }

        private void OnEditFinalOrganizationContanctCommand()
        {
            OrganizationContactCollection.Items.Replace(OrganizationContactCollection.Selected, OrganizationContactTemp);
            OrganizationContactCollection.Selected = OrganizationContactTemp;
           // OrganizationContactCollection.Selected.ContactTypeId = OrganizationContactTemp.ContactTypeDto.ContactTypeId;

        }

        private void OnCloseEditOrganizationContanctCommand()
        {
            OrganizationContactTemp = new OrganizationContactDto();
        }

        [Reactive]
        public CollectionWithSelection<OrganizationContactDto> OrganizationContactCollection { get; set; }
        [Reactive]
        public CollectionWithSelection<ContactTypeDto> ContactTypeCollection { get; set; }
        public ReactiveCommand<Unit, Unit> AddOrganizationContanctCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteOrganizationContanctCommand { get; set; }
        public ReactiveCommand<FlyoutBase, Unit> StartEditOrganizationContanctCommand { get; set; }
        public ReactiveCommand<Unit, Unit> EditFinalOrganizationContanctCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CloseEditOrganizationContanctCommand { get; set; }

        protected override void _ConfigureTitle()
        {
            switch (_currentMode)
            {
                case Core.Enums.FormMode.See:
                    Title = "Просмотр организации";
                    break;
                case Core.Enums.FormMode.Edit:
                    Title = "Редактирование организации";
                    break;
                case Core.Enums.FormMode.Add:
                    Title = "Добавление организации";
                    break;
            }
        }

        protected override async Task _Loaded()
        {
            try
            {
                if (_currentMode == Core.Enums.FormMode.See || _currentMode == Core.Enums.FormMode.Edit)
                {
                    await _ConfigureCollections();
                }
                await _ContactTypeCollectionLoad();
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка во время загрузки формы", ex.Message);
                Close();
            }

        }

        private async Task _ContactTypeCollectionLoad()
        {
            try
            {
                var client = new ContactTypeService.ContactTypeServiceClient(_grpcChannel);
                var response = await client.SearchAsync(new ContactTypeLookup());
                foreach (var item in response.Data)
                {
                    ContactTypeCollection.Items.Add(item);
                }
            }
            catch (RpcException ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время загрузки типов контактов возникла ошибка:\n{ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время загрузки типов контактов неизвестная ошибка:\n{ex.Message}");
            }
        }

        private async Task _ConfigureCollections()
        {
            try
            {
                var client = new OrganizationContactService.OrganizationContactServiceClient(_grpcChannel);
                var response = await client.SearchAsync(new OrganizationContactLookup { OrganizationID = _dto.OrganizationId });

                foreach (var item in response.Data)
                {
                    OrganizationContactCollection.Items.Add(item);
                }
            }
            catch (RpcException ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время загрузки контактов возникла ошибка:\n{ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время загрузки контактов неизвестная ошибка:\n{ex.Message}");
            }
        }

        

        protected override async Task _OnAdd()
        {
            if (!await _Validate()) return;
            try
            {
                var client = new OrganizationService.OrganizationServiceClient(_grpcChannel);

                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление организации"));
                var response = await client.AddAsync(DataRequest);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                var orgId = response.Data.FirstOrDefault().OrganizationId;

                foreach (var item in OrganizationContactCollection.Items)
                {
                    item.OrganizationId = orgId;
                }

                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляются запросы на добавление или изменение контактов организации"));
                await _AddOrUpdateContacts();
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
                await MessageBoxHelper.Success("Уведомление", "Организация успешна добавлена!");

            }
            catch (RpcException ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка со стороны сервера", $"Во время добавления организации или её контактов возникла ошибка:\n{ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время добавления организации или её контактов возникла неизвестная ошибка:\n{ex.Message}");
            }
        }

        protected override async Task _OnEdit()
        {
            if (!await _Validate()) return;
            try
            {
                var client = new OrganizationService.OrganizationServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на изменение организации"));
                var response = await client.EditAsync(DataRequest);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));

                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляются запросы на добавление или изменение контактов организации"));

                await _AddOrUpdateContacts();

                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));

                await MessageBoxHelper.Success("Уведомление", "Организация успешно изменена!");

            }
            catch (RpcException ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время изменения организации или её контактов возникла ошибка:\n{ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время изменения организации или её контактов возникла неизвестная ошибка:\n{ex.Message}");
            }
        }

        private async Task _AddOrUpdateContacts()
        {
            var orgContactClient = new OrganizationContactService.OrganizationContactServiceClient(_grpcChannel);
            foreach (var item in OrganizationContactCollection.Items)
            {
                var dataRequest = new OrganizationContactDataRequest
                {
                    OrganizationContactID = item.OrganizationContactId,
                    ContactTypeID = item.ContactTypeDto.ContactTypeId,
                    Note = item.Note,
                    Value = item.Value,
                    OrganizationID = item.OrganizationId
                };

                if (item.OrganizationContactId == 0)
                {
                    MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление контакта организации"));
                    await orgContactClient.AddAsync(dataRequest);
                }
                else
                {
                    MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на изменение контакта организации"));
                    await orgContactClient.EditAsync(dataRequest);
                }
            }

            MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
        }

        protected override async Task _OnSee()
        {
            await Task.CompletedTask;
        }
    }
}
