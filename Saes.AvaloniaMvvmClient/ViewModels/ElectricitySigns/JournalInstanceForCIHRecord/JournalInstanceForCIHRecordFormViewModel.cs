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

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalInstanceForCIHRecord
{
    public class JournalInstanceForCIHRecordFormViewModel : ViewModelFormBase<JournalInstanceForCIHRecordDto, JournalInstanceForCIHRecordDataRequest>
    {
        private CallInvoker _grpcChannel;

        public JournalInstanceForCIHRecordFormViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
            //JournalInstanceForCIHRecordTypeCollection = new CollectionWithSelection<JournalInstanceForCIHRecordTypeDto>();
            ReceivedFromCollection = new CollectionWithSelection<BusinessEntityDto>();
            UserCpiCollection = new CollectionWithSelection<EmployeeDto>();

            OrganizationCollection = new CollectionWithSelection<OrganizationDto>();
            HardwareCollection = new CollectionWithSelection<HardwareDto>();
            InstallerCollection = new CollectionWithSelection<EmployeeDto>();
            DestructorCollection = new CollectionWithSelection<EmployeeDto>();

            LinkedInstallers = new ObservableCollection<EmployeeDto>();
            LinkedDestructors = new ObservableCollection<EmployeeDto>();
            LinkedHardwares = new ObservableCollection<HardwareDto>();

            InitiliazeCommands();
        }

        protected override JournalInstanceForCIHRecordDataRequest _ConfigureDataRequest(JournalInstanceForCIHRecordDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || CurrentMode == Core.Enums.FormMode.Edit)
            {

                return new JournalInstanceForCIHRecordDataRequest
                {
                    JournalInstanceForCIHRecordId = dto.JournalInstanceForCIHRecordId,
                    OrganizationID = dto.OrganizationId,
                    NameCPI = dto.NameCPI,
                    SerialCPI = dto.SerialCPI,
                    InstanceNumber = dto.InstanceNumber,
                    ReceivedFromID = dto.ReceivedFromId,
                    DateAndNumberCoverLetterReceive = dto.DateAndNumberCoverLetterReceive,
                    CPIUserID = dto.CPIUserId,
                    DateAndNumberConfirmationIssue = dto.DateAndNumberConfirmationIssue,
                    InstallationDateAndConfirmation = dto.InstallationDateAndConfirmation,
                    DestructionDate = dto.DestructionDate,
                    DestructionActNumber = dto.DestructionActNumber,
                    Note = dto.Note
                };

            }
            else
            {
                return new JournalInstanceForCIHRecordDataRequest
                {
                    JournalInstanceForCIHRecordId = 0,
                    OrganizationID = dto.OrganizationDto.OrganizationId
                };
            }
        }

        private async Task _ConfigureCollections()
        {

            var employeeClient = new EmployeeService.EmployeeServiceClient(_grpcChannel);

            var installerClient = new JournalInstanceForCIHInstallerService.JournalInstanceForCIHInstallerServiceClient(_grpcChannel);
            var requestInstaller = new JournalInstanceForCIHInstallerLookup { RecordID = _dto.JournalInstanceForCIHRecordId };

            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение установщиков"));
            var responseInstaller = await installerClient.SearchAsync(requestInstaller);

            MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка полученных данных"));
            foreach (var installer in responseInstaller.Data)
            {
                var employeeRequest = new EmployeeLookup { BusinessEntityID = installer.InstallerId };
                var responseEmployee = await employeeClient.SearchAsync(employeeRequest);

                if (responseEmployee.Data.Count > 0)
                {

                    LinkedInstallers.Add(responseEmployee.Data.Single());
                }
            }

            var destructorClient = new JournalInstanceForCIHDestructorService.JournalInstanceForCIHDestructorServiceClient(_grpcChannel);
            var requestDestructor = new JournalInstanceForCIHDestructorLookup { RecordID = _dto.JournalInstanceForCIHRecordId };

            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение изъятелей"));
            var responseDestructor = await destructorClient.SearchAsync(requestDestructor);

            MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка полученных данных"));
            foreach (var destructor in responseDestructor.Data)
            {
                var employeeRequest = new EmployeeLookup { BusinessEntityID = destructor.DestructorId };
                var responseEmployee = await employeeClient.SearchAsync(employeeRequest);
                LinkedDestructors.Add(responseEmployee.Data.Single());
            }


            var connectedhardwareClient = new JournalInstanceForCIHConnectedHardwareService.JournalInstanceForCIHConnectedHardwareServiceClient(_grpcChannel);
            var requestConnectedHardware = new JournalInstanceForCIHConnectedHardwareLookup { RecordID = _dto.JournalInstanceForCIHRecordId };

            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение аппаратуры"));
            var responseConnectedHardware = await connectedhardwareClient.SearchAsync(requestConnectedHardware);

            MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка полученных данных"));
            foreach (var connectedhardware in responseConnectedHardware.Data)
            {
                LinkedHardwares.Add(connectedhardware.HardwareDto);
            }
            MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));

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
            AddInstallerCommand = ReactiveCommand.Create(
                () =>
                {
                    LinkedInstallers.Add(InstallerCollection.Selected);
                    InstallerCollection.Items.Remove(InstallerCollection.Selected);
                },
                InstallerCollection.WhenAnyValue(x => x.Selected, x => !LinkedInstallers.Contains(x) && InstallerCollection.Selected != null)
                );
            DeleteInstallerCommand = ReactiveCommand.Create(
                () =>
                {
                    InstallerCollection.Items.Add(SelectedLinkedInstaller);
                    InstallerCollection.SelectedIndex = 0;
                    LinkedInstallers.Remove(SelectedLinkedInstaller);
                },
                this.WhenAnyValue(x => x.SelectedLinkedInstaller, LinkedInstallers.Contains));

            AddDestructorCommand = ReactiveCommand.Create(
                () =>
                {
                    LinkedDestructors.Add(DestructorCollection.Selected);
                    DestructorCollection.Items.Remove(DestructorCollection.Selected);
                },
                DestructorCollection.WhenAnyValue(x => x.Selected, x => !LinkedDestructors.Contains(x) && DestructorCollection.Selected != null)
                );
            DeleteDestructorCommand = ReactiveCommand.Create(
                () =>
                {
                    DestructorCollection.Items.Add(SelectedLinkedDestructor);
                    DestructorCollection.SelectedIndex = 0;
                    LinkedDestructors.Remove(SelectedLinkedDestructor);
                },
                this.WhenAnyValue(x => x.SelectedLinkedDestructor, LinkedDestructors.Contains));

            AddHardwareCommand = ReactiveCommand.Create(
                () =>
                {
                    LinkedHardwares.Add(HardwareCollection.Selected);
                    HardwareCollection.Items.Remove(HardwareCollection.Selected);
                },
                HardwareCollection.WhenAnyValue(x => x.Selected, x => !LinkedHardwares.Contains(x) && HardwareCollection.Selected != null)
                );
            DeleteHardwareCommand = ReactiveCommand.Create(
                () =>
                {
                    HardwareCollection.Items.Add(SelectedLinkedHardware);
                    HardwareCollection.SelectedIndex = 0;
                    LinkedHardwares.Remove(SelectedLinkedHardware);
                },
                this.WhenAnyValue(x => x.SelectedLinkedHardware, LinkedHardwares.Contains));
        }

        public ReactiveCommand<Unit, Unit> AddInstallerCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteInstallerCommand { get; set; }

        public ReactiveCommand<Unit, Unit> AddHardwareCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteHardwareCommand { get; set; }

        public ReactiveCommand<Unit, Unit> AddDestructorCommand { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteDestructorCommand { get; set; }

        [Reactive]
        public CollectionWithSelection<EmployeeDto> UserCpiCollection { get; set; }
        [Reactive]
        public CollectionWithSelection<OrganizationDto> OrganizationCollection { get; set; }
        [Reactive]
        public CollectionWithSelection<BusinessEntityDto> ReceivedFromCollection { get; set; }
        [Reactive]
        public CollectionWithSelection<EmployeeDto> InstallerCollection { get; set; }
        [Reactive]
        public CollectionWithSelection<HardwareDto> HardwareCollection { get; set; }
        [Reactive]
        public CollectionWithSelection<EmployeeDto> DestructorCollection { get; set; }



        [Reactive]
        public ObservableCollection<EmployeeDto> LinkedInstallers { get; set; }
        [Reactive]
        public ObservableCollection<HardwareDto> LinkedHardwares { get; set; }
        [Reactive]
        public ObservableCollection<EmployeeDto> LinkedDestructors { get; set; }

        [Reactive]
        public EmployeeDto SelectedLinkedInstaller { get; set; }
        [Reactive]
        public EmployeeDto SelectedLinkedDestructor { get; set; }
        [Reactive]
        public HardwareDto SelectedLinkedHardware { get; set; }

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
                await HardwareCollectionLoad();
                await DestructorAndInstallerAndUserCpiCollectionLoad();
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка во время загрузки формы", ex.Message);
                Close();
            }
        }

        private async Task HardwareCollectionLoad()
        {

            var client = new HardwareService.HardwareServiceClient(_grpcChannel);
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение аппаратуры"));
            var response = await client.SearchAsync(new HardwareLookup { OrganizationID = _dto.OrganizationId });
            MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
            foreach (var item in response.Data)
            {
                if (!LinkedHardwares.Contains(item))
                {
                    HardwareCollection.Items.Add(item);
                }
            }

            MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
        }

        private async Task DestructorAndInstallerAndUserCpiCollectionLoad()
        {
            var client = new EmployeeService.EmployeeServiceClient(_grpcChannel);
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение сотрудников"));
            var response = await client.SearchAsync(new EmployeeLookup { OrganizationID = _dto.OrganizationId });
            MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
            foreach (var item in response.Data)
            {
                if (!LinkedDestructors.Contains(item))
                {
                    DestructorCollection.Items.Add(item);
                }

                if (!LinkedInstallers.Contains(item))
                {
                    InstallerCollection.Items.Add(item);
                }

                UserCpiCollection.Items.Add(item);
            }

            MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));

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
                var service = new JournalInstanceForCIHRecordService.JournalInstanceForCIHRecordServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление новой записи журнала поэкземплярного учета СКЗИ для ОКИ"));
                var response = await service.AddAsync(DataRequest);

                await UpdateBulkRecordData(response.Data.First().JournalInstanceForCIHRecordId);

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

        private async Task UpdateBulkRecordData(int journalInstanceForCIHRecordId)
        {
            var installerService = new JournalInstanceForCIHInstallerService.JournalInstanceForCIHInstallerServiceClient(_grpcChannel);
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на изменение установщиков в записи журнала поэкземплярного учета СКЗИ для ОКИ"));
            var installerRequest = new JournalInstanceForCIHInstallerBulkUpdateRequest
            {
                RecordID = journalInstanceForCIHRecordId,
            };
            installerRequest.InstallersIds.AddRange(LinkedInstallers.Select(x => x.BusinessEntityId));
            await installerService.BulkUpdateAsync(installerRequest);


            var destructorService = new JournalInstanceForCIHDestructorService.JournalInstanceForCIHDestructorServiceClient(_grpcChannel);
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на изменение изъятелей в записи журнала поэкземплярного учета СКЗИ для ОКИ"));
            var destructorRequest = new JournalInstanceForCIHDestructorBulkUpdateRequest
            {
                RecordID = journalInstanceForCIHRecordId,
            };
            destructorRequest.DestructorsIds.AddRange(LinkedDestructors.Select(x => x.BusinessEntityId));
            await destructorService.BulkUpdateAsync(destructorRequest);

            var connectedhardwareService = new JournalInstanceForCIHConnectedHardwareService.JournalInstanceForCIHConnectedHardwareServiceClient(_grpcChannel);
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на изменение подключенных устройств в записи журнала поэкземплярного учета СКЗИ для ОКИ"));
            var connectedhardwareRequest = new JournalInstanceForCIHConnectedHardwareBulkUpdateRequest
            {
                RecordID = journalInstanceForCIHRecordId,
            };
            connectedhardwareRequest.ConnectedHardwaresIds.AddRange(LinkedHardwares.Select(x => x.HardwareId));
            await connectedhardwareService.BulkUpdateAsync(connectedhardwareRequest);
        }

        protected override async Task _OnEdit()
        {
            try
            {
                var service = new JournalInstanceForCIHRecordService.JournalInstanceForCIHRecordServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на редактирование записи журнала поэкземплярного учета СКЗИ для ОКИ"));
                var response = await service.EditAsync(DataRequest);
                await UpdateBulkRecordData(DataRequest.JournalInstanceForCIHRecordId.Value);
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
