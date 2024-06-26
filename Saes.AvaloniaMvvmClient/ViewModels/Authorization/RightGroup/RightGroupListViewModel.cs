using Grpc.Core;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core;
using Saes.AvaloniaMvvmClient.Core.Attributes;
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

namespace Saes.AvaloniaMvvmClient.ViewModels.Authorization.RightGroup
{
    [RightScope("right_group_see")]
    public class RightGroupListViewModel : ViewModelTabListBase<RightGroupDto, RightGroupLookup>
    {
        private CallInvoker _grpcChannel;

        public RightGroupListViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            TabTitle = "Группы прав";
            _grpcChannel = grpcChannelFactory.CreateChannel();
        }
        protected override async Task OnAddCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task OnCopyCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task OnDeleteCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task OnEditCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task OnSeeCommand()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task _Export()
        {
            await MessageBoxHelper.NotImplementedError();
        }

        protected override async Task _Loaded()
        {
            await _Search();
        }

        protected override async Task _Search()
        {
            var client = new RightGroupService.RightGroupServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение записей"));
                var response = await client.SearchAsync(Lookup);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<RightGroupDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }
    }
}
