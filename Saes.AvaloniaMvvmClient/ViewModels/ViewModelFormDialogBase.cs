using Avalonia.Markup.Xaml.MarkupExtensions;

using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core.Enums;
using System;
using System.Windows.Input;

namespace Saes.AvaloniaMvvmClient.ViewModels;


public abstract class ViewModelFormDialogBase<TDto, TDataRequest> : ViewModelFormBase<TDto, TDataRequest>
    where TDto : class, new()
    where TDataRequest : class, new()

{

}
