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

namespace Saes.AvaloniaMvvmClient.ViewModels.ElectricitySigns.JournalTechnicalRecord
{
    public class JournalTechnicalRecordListViewModel : ViewModelTabListBase<JournalTechnicalRecordDto, JournalTechnicalRecordLookup>
    {
        private CallInvoker _grpcChannel;

        public JournalTechnicalRecordListViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            TabTitle = "Журнал технический (аппаратный)";
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
            var client = new JournalTechnicalRecordService.JournalTechnicalRecordServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение записей"));
                var response = await client.SearchAsync(Lookup);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<JournalTechnicalRecordDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }
    }
}
