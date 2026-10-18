using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Boardy.WPF.Player
{
    public partial class CreatePlayerView : UserControl
    {
        public CreatePlayerView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<CreatePlayerViewModel>();
        }
    }
}
