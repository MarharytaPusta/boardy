using Boardy.WPF.Home;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;

namespace Boardy.WPF.Scoring;

public partial class MatchCompletionViewModel : ObservableObject
{
    [ObservableProperty]
    private string _winnerNames = string.Empty;

    [ObservableProperty]
    private string _winnerTitle = string.Empty;

    [ObservableProperty]
    private string _winnerScoreText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOtherPlayers))]
    private ObservableCollection<MatchStandingItem> _otherPlayers = new();

    public bool HasOtherPlayers => OtherPlayers.Count > 0;

    public void Initialize(MatchResultsViewModel matchResultsViewModel)
    {
        var orderedParticipants = matchResultsViewModel.Participants
            .OrderByDescending(participant => participant.IsWinner)
            .ThenByDescending(participant => participant.TotalScore)
            .ThenBy(participant => participant.PlayerName)
            .ToList();

        var winners = orderedParticipants
            .Where(participant => participant.IsWinner)
            .ToList();

        WinnerTitle = winners.Count == 1 ? "Переможець" : "Переможці";
        WinnerNames = string.Join(", ", winners.Select(winner => winner.PlayerName));
        WinnerScoreText = winners.Count > 0
            ? $"Результат: {winners[0].TotalScore}"
            : string.Empty;

        var standings = new List<MatchStandingItem>();
        int currentPlace = 1;

        for (int i = 0; i < orderedParticipants.Count; i++)
        {
            var participant = orderedParticipants[i];
            if (i > 0 && participant.TotalScore < orderedParticipants[i - 1].TotalScore)
            {
                currentPlace = i + 1;
            }

            if (!participant.IsWinner)
            {
                standings.Add(new MatchStandingItem(
                    $"{currentPlace} місце",
                    participant.PlayerName,
                    participant.TotalScore));
            }
        }

        OtherPlayers = new ObservableCollection<MatchStandingItem>(standings);
    }

    [RelayCommand]
    private void GoHome()
    {
        var homeVm = App.ServiceProvider.GetRequiredService<HomeViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(homeVm));
    }

}

public record MatchStandingItem(string PlaceText, string PlayerName, int TotalScore);
