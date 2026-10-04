using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Serilog;

namespace Boardy.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Log.Information("Main window opened");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string playerName = PlayerNameTextBox.Text;

            if (!int.TryParse(PointsTextBox.Text, out int points) ||
                !int.TryParse(PenaltyTextBox.Text, out int penalty))
            {
                Log.Warning(
                    "Invalid score input for player {PlayerName}",
                    playerName);

                MessageBox.Show("Points and penalty must be numbers.");
                return;
            }

            int total = points - penalty;

            if (total < 0)
            {
                total = 0;
            }

            Log.Information(
                "Score calculated for player {PlayerName}. Points: {Points}, Penalty: {Penalty}, Total: {Total}",
                playerName,
                points,
                penalty,
                total);

            MessageBox.Show($"Result: {total}");
        }
    }
}