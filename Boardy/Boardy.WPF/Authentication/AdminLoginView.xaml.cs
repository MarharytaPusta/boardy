using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Boardy.WPF.Authentication
{
    /// <summary>
    /// Interaction logic for AdminLoginView.xaml
    /// </summary>
    public partial class AdminLoginView : UserControl
    {
        public AdminLoginView()
        {
            InitializeComponent();

            DataContext = App.ServiceProvider.GetRequiredService<AdminLoginViewModel>();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GetNavigationService(this)?.Navigate(new AdminChangePasswordView());
        }
    }
}
