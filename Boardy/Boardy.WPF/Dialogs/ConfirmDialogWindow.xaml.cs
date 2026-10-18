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

namespace Boardy.WPF.Dialogs;

/// <summary>
/// Interaction logic for ConfirmDialogWindow.xaml
/// </summary>
public partial class ConfirmDialogWindow : Window
{
    public bool IsConfirmed { get; private set; } = false;
    public ConfirmDialogWindow(string title, string message)
    {
        InitializeComponent();
        TitleText.Text = title;
        MessageText.Text = message;
    }

    private void BtnYes_Click(object sender, RoutedEventArgs e)
    {
        IsConfirmed = true;
        Close();
    }

    private void BtnNo_Click(object sender, RoutedEventArgs e)
    {
        IsConfirmed = false;
        Close();
    }
}
