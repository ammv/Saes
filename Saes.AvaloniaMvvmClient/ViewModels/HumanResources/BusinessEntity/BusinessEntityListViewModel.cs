﻿using Grpc.Core;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.Protos;
using Saes.Protos.ModelServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.ViewModels.HumanResources.BusinessEntity
{
    public class BusinessEntityListViewModel : ViewModelTabListBase<BusinessEntityDto, BusinessEntityLookup>
    {
        private CallInvoker _grpcChannel;

        public BusinessEntityListViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            TabTitle = "Бизнес-сущности";
            _grpcChannel = grpcChannelFactory.CreateChannel();
        }
        public override async Task<bool> CloseAsync()
        {
            return await MessageBoxHelper.Question("Вопрос", $"Вы уверены, что хотите закрыть вкладку \"{TabTitle}\"");
        }

        protected override Task OnAddCommand()
        {
            throw new NotImplementedException();
        }

        protected override Task OnCopyCommand()
        {
            throw new NotImplementedException();
        }

        protected override Task OnDeleteCommand()
        {
            throw new NotImplementedException();
        }

        protected override Task OnEditCommand()
        {
            throw new NotImplementedException();
        }

        protected override Task OnSeeCommand()
        {
            throw new NotImplementedException();
        }

        protected override Task _Export()
        {
            throw new NotImplementedException();
        }

        protected override async Task _Loaded()
        {
            await _Search();
        }

        protected override async Task _Search()
        {
            var client = new BusinessEntityService.BusinessEntityServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение записей"));
                var response = await client.SearchAsync(Lookup);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<BusinessEntityDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }
    }
}