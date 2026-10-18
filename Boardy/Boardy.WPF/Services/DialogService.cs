using Boardy.Application.Shared;
using Boardy.WPF.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.WPF.Services;

public class DialogService : IDialogService
{
    public bool Confirm(string title, string message)
    {
        var dialog = new ConfirmDialogWindow(title, message);

        dialog.Owner = System.Windows.Application.Current.MainWindow;

        dialog.ShowDialog();

        return dialog.IsConfirmed;
    }
}
