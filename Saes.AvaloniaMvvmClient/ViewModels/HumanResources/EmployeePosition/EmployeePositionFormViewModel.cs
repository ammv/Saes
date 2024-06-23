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

namespace Saes.AvaloniaMvvmClient.ViewModels.HumanResources.EmployeePosition
{
    public class EmployeePositionFormViewModel : ViewModelFormBase<EmployeePositionDto, EmployeePositionDataRequest>
    {
        private readonly CallInvoker _grpcChannel;

        public EmployeePositionFormViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            _grpcChannel = grpcChannelFactory.CreateChannel();
        }

        protected override bool Validate()
        {
            return true;
        }

        protected override EmployeePositionDataRequest _ConfigureDataRequest(EmployeePositionDto dto)
        {
            if (_currentMode == Core.Enums.FormMode.See || CurrentMode == Core.Enums.FormMode.Edit)
            {
                return new EmployeePositionDataRequest
                {
                    EmployeePositionID = dto.EmployeePositionId,
                    Name = dto.Name,
                    Note = dto.Note
                };

            }
            else
            {
                return new EmployeePositionDataRequest
                {
                    EmployeePositionID = 0
                };
            }
        }

        protected override void _ConfigureTitle()
        {
            switch (_currentMode)
            {
                case Core.Enums.FormMode.See:
                    Title = "Просмотр должности сотрудника";
                    break;
                case Core.Enums.FormMode.Edit:
                    Title = "Редактирование должности сотрудника";
                    break;
                case Core.Enums.FormMode.Add:
                    Title = "Добавление должности сотрудника";
                    break;
            }
        }

        protected override async Task _Loaded()
        {

        }

        protected override async Task _OnAdd()
        {

            if(!Validate())
            {
                await MessageBoxHelper.Exception("Ошибка", "Надо обязательно выбрать организацию и должность сотрудника");
            }
            try
            {
                var service = new EmployeePositionService.EmployeePositionServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на добавление сотрудника"));
                var response = await service.AddAsync(DataRequest);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
                await MessageBoxHelper.Success("Уведомление", "Сотрудник успешно добавлен!");
                Callback?.Invoke(response.Data.FirstOrDefault());

            }
            catch (RpcException ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время добавления должности сотрудника возникла ошибка:\n{ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время добавления должности сотрудника возникла неизвестная ошибка:\n{ex.Message}");
            }
        }

        protected override async Task _OnEdit()
        {
            try
            {
                var service = new EmployeePositionService.EmployeePositionServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на редактирование должности сотрудника"));
                var response = await service.EditAsync(DataRequest);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
                await MessageBoxHelper.Success("Уведомление", "Должность сотрудника успешно изменена!");

            }
            catch (RpcException ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время изменения должности сотрудника возникла ошибка:\n{ex.Status.Detail}");
            }
            catch(Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
                await MessageBoxHelper.Exception("Ошибка", $"Во время изменения должности сотрудника неизвестная ошибка:\n{ex.Message}");
            }
        }

        protected override Task _OnSee()
        {
            return Task.CompletedTask;
        }
    }
}
