using Avalonia.Reactive;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.DataModel;
using Saes.AvaloniaMvvmClient.Services;
using System;
using System.Reactive.Linq;

namespace Saes.AvaloniaMvvmClient.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _contentViewModel;



    public MainViewModel()
    {
        var service = new ToDoListService();
        ToDoList = new ToDoListViewModel(service.GetItems());
        _contentViewModel = ToDoList;
    }

    public ToDoListViewModel ToDoList { get; }
    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

    public void AddItem()
    {
        AddItemViewModel addItemViewModel = new();

        Observable.Merge(
            addItemViewModel.OkCommand,
            addItemViewModel.CancelCommand.Select(_ => (ToDoItem?)null))
            .Take(1)
            .Subscribe(OnAddItem);  

        ContentViewModel = addItemViewModel;
    }

    public void ClearItems()
    {
        ToDoList.ListItems.Clear();
    }

    private void OnAddItem(ToDoItem? newItem)
    {
        if (newItem != null)
        {
            ToDoList.ListItems.Add(newItem);
        }
        ContentViewModel = ToDoList;
    }
}
