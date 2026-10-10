using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

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
    }
}
