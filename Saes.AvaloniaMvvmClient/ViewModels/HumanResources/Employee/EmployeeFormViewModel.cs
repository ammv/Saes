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

namespace Saes.AvaloniaMvvmClient.ViewModels.HumanResources.Employee
{
    public class EmployeeFormViewModel : ViewModelFormBase<EmployeeDto, EmployeeDataRequest>
    {
        private readonly CallInvoker _grpcChannel;

        public EmployeeFormViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
            OrganizationCollection = new CollectionWithSelection<OrganizationDto>();
            EmployeePositionCollection = new CollectionWithSelection<EmployeePositionDto>();
        }

        [Reactive]
        public CollectionWithSelection<OrganizationDto> OrganizationCollection { get; set; }
        [Reactive]
        public CollectionWithSelection<EmployeePositionDto> EmployeePositionCollection { get; set; }

        protected override bool Validate()
        {
            return OrganizationCollection.Selected != null && EmployeePositionCollection.Selected != null;
        }

        protected override EmployeeDataRequest _ConfigureDataRequest(EmployeeDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || CurrentMode == Core.Enums.FormMode.Edit)
            {
                return new EmployeeDataRequest
                {
                    EmployeeID = dto.EmployeeId,
                    OrganizationID = dto.OrganizationId,
                    EmployeePositionID = dto.EmployeePositionId,
                    FirstName = dto.FirstName,
                    MiddleName = dto.MiddleName,
                    LastName = dto.LastName
                };

            }
            else
            {
                return new EmployeeDataRequest
                {
                    EmployeeID = 0
                };
            }
        }

        protected override void _ConfigureTitle()
        {
            switch (_currentMode)
            {
                case Core.Enums.FormMode.See:
                    Title = "Просмотр сотрудника";
                    break;
                case Core.Enums.FormMode.Edit:
                    Title = "Редактирование сотрудника";
                    break;
                case Core.Enums.FormMode.Add:
                    Title = "Добавление сотрудника";
                    break;
            }
        }

        protected override async Task _Loaded()
        {
            try
            {
                await _OrganizationCollectionLoad();
                await _EmployeePositionCollectionLoad();

            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка во время загрузки формы", ex.Message);
                Close();
            }
            
        }

        private async Task _EmployeePositionCollectionLoad()
        {
            try
            {
                var service = new EmployeePositionService.EmployeePositionServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение должностей"));
                var response = await service.SearchAsync(new EmployeePositionLookup());
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                EmployeePositionCollection.Items.Clear();
                foreach (var item in response.Data)
                {
                    EmployeePositionCollection.Items.Add(item);
                }
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }

        private async Task _OrganizationCollectionLoad()
        {
            try
            {
                var service = new OrganizationService.OrganizationServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение организаций"));
                var response = await service.SearchAsync(new OrganizationLookup());
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                OrganizationCollection.Items.Clear();
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

        }

        protected override async Task _OnAdd()
        {

            if(!Validate())
            {
                await MessageBoxHelper.Exception("Ошибка", "Надо обязательно выбрать организацию и должность сотрудника");
            }
            try
            {
                var service = new EmployeeService.EmployeeServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление сотрудника"));
                var response = await service.AddAsync(DataRequest);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
                await MessageBoxHelper.Success("Уведомление", "Сотрудник успешно добавлен!");
                Callback?.Invoke(response.Data.FirstOrDefault());

            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", "Во время добавления сотрудника возникла ошибка");
            }
        }

        protected override async Task _OnEdit()
        {
            try
            {
                var service = new EmployeeService.EmployeeServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на редактирование сотрудника"));
                var response = await service.EditAsync(DataRequest);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
                await MessageBoxHelper.Success("Уведомление", "Сотрудник успешно изменен!");

            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", "Во время изменения сотрудника возникла ошибка");
            }
        }

        protected override async Task _OnSee()
        {
            await Task.CompletedTask;
        }
    }
}
