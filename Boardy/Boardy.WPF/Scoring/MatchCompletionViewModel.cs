using Boardy.WPF.Home;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Boardy.WPF.Scoring;

public partial class MatchCompletionViewModel : ObservableObject
{
    private string _winnerNames = string.Empty;

    private string _winnerTitle = string.Empty;

    public string WinnerNames
    {
        get => _winnerNames;
        set => SetProperty(ref _winnerNames, value);
    }

    public string WinnerTitle
    {
        get => _winnerTitle;
        set => SetProperty(ref _winnerTitle, value);
    }

    public void Initialize(MatchResultsViewModel matchResultsViewModel)
    {
        var winners = matchResultsViewModel.Participants
            .Where(participant => participant.IsWinner)
            .Select(participant => participant.PlayerName)
            .ToList();

        WinnerTitle = winners.Count == 1 ? "Переможець" : "Переможці";
        WinnerNames = string.Join(", ", winners);
    }

    [RelayCommand]
    private void GoHome()
    {
        var homeVm = App.ServiceProvider.GetRequiredService<HomeViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(homeVm));
    }
}
