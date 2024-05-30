using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Saes.AvaloniaMvvmClient.Helpers;

namespace Saes.AvaloniaMvvmClient.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        WindowManager.Add(this);
    }
    ~MainWindow()
    {
        WindowManager.Remove(this);
    }
}
