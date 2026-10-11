using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Boardy.WPF.Authentication;

public partial class AdminChangePasswordView : UserControl
{
    public AdminChangePasswordView()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetRequiredService<AdminChangePasswordViewModel>();
    }
}
