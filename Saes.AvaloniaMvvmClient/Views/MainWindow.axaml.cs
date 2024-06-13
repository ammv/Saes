using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Helpers;
using Saes.AvaloniaMvvmClient.ViewModels;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication.User;
using Saes.AvaloniaMvvmClient.Views.Authentication.User;
using Saes.Protos;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        WindowManager.Add(this);
        //this.WhenActivated(action =>
        //        action(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        int x = (int)(Screens.Primary.WorkingArea.Width - this.Bounds.Width) / 2;
        int y = (int)(Screens.Primary.WorkingArea.Height - this.Bounds.Height) / 2;

        this.Position = new Avalonia.PixelPoint(x,y);
    }

    //private async Task DoShowDialogAsync(InteractionContext<UserFormViewModel,
    //                                            UserDto?> interaction)
    //{
    //    var dialog = new UserFormView();
    //    dialog.DataContext = interaction.Input;

    //    var result = await dialog.ShowDialog<UserDto?>(this);
    //    interaction.SetOutput(result);
    //}

    ~MainWindow()
    {
        WindowManager.Remove(this);
    }
}
