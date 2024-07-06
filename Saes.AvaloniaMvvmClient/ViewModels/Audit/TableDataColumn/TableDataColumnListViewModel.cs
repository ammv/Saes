using Grpc.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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

namespace Saes.AvaloniaMvvmClient.ViewModels.Audit.TableDataColumn
{
    [RightScope("table_column_data_see")]
    public class TableDataColumnListViewModel : ViewModelTabListBase<TableColumnDataDto, TableColumnDataLookup>
    {
        private CallInvoker _grpcChannel;

        public TableDataColumnListViewModel(IGrpcChannelFactory grpcChannelFactory)
        {
            TabTitle = "Данные столбцов таблиц";
            _grpcChannel = grpcChannelFactory.CreateChannel();
            TableDataCollection = new();
        }

        [Reactive]
        public CollectionWithSelection<TableDataDto> TableDataCollection { get; private set; }
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
            try
            {
                await _Search();
                var service = new TableDataService.TableDataServiceClient(_grpcChannel);
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение таблиц"));
                var response = await service.SearchAsync(new TableDataLookup());
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                TableDataCollection.Items.Clear();
                foreach (var item in response.Data)
                {
                    TableDataCollection.Items.Add(item);
                }
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }

        protected override async Task _Search()
        {
            var client = new TableColumnDataService.TableColumnDataServiceClient(_grpcChannel);

            try
            {
                MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest("Отправляется запрос на получение записей"));
                var response = await client.SearchAsync(Lookup);
                MessageBus.Current.SendMessage(StatusData.HandlingGrpcResponse("Обработка результатов"));
                Entities = new ObservableCollection<TableColumnDataDto>(response.Data);
                MessageBus.Current.SendMessage(StatusData.Ok("Успешно"));
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(StatusData.Error(ex));
            }
        }
    }
}
