using Boardy.Application.Statistics;
using Boardy.WPF.GameCatalog;
using Boardy.WPF.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Boardy.WPF.Statistics;

public class GameStatisticsViewModel : ObservableObject
{
    private const string SortByWins = "Кількістю перемог";
    private const string SortByMatches = "Кількістю партій";
    private const string SortByBestScore = "Найкращим результатом";

    private readonly GetGameStatisticsUseCase _getGameStatisticsUseCase;

    private List<GamePlayerStatDto> _rawPlayers = new();

    private string _gameName = string.Empty;

    public string GameName
    {
        get => _gameName;
        set => SetProperty(ref _gameName, value);
    }

    private int _totalPlayedMatches;
    public int TotalPlayedMatches
    {
        get => _totalPlayedMatches;
        set => SetProperty(ref _totalPlayedMatches, value);
    }

    private string _highestScoreDisplay = "—";
    public string HighestScoreDisplay
    {
        get => _highestScoreDisplay;
        set => SetProperty(ref _highestScoreDisplay, value);
    }

    private string _topResultsSummary = "Ще немає зіграних партій";
    public string TopResultsSummary
    {
        get => _topResultsSummary;
        set => SetProperty(ref _topResultsSummary, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public List<string> SortOptions { get; } = new()
    {
        SortByWins,
        SortByMatches,
        SortByBestScore
    };

    private string _selectedSortOption = SortByWins;
    public string SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            if (SetProperty(ref _selectedSortOption, value))
            {
                ApplySorting();
            }
        }
    }

    public ObservableCollection<GamePlayerStatRowViewModel> Players { get; } = new();

    public ICommand BackCommand { get; }

    public GameStatisticsViewModel(GetGameStatisticsUseCase getGameStatisticsUseCase)
    {
        _getGameStatisticsUseCase = getGameStatisticsUseCase;
        BackCommand = new AsyncRelayCommand(NavigateBackAsync);
    }

    private static async Task NavigateBackAsync()
    {
        var gameCatalogViewModel = App.ServiceProvider.GetRequiredService<GameCatalogViewModel>();
        await gameCatalogViewModel.LoadGamesAsync();
        WeakReferenceMessenger.Default.Send(new NavigationMessage(gameCatalogViewModel));
    }

    public async Task LoadAsync(Guid gameId)
    {
        ErrorMessage = string.Empty;

        var result = await _getGameStatisticsUseCase.ExecuteAsync(gameId, topResultsLimit: 3);

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося завантажити статистику гри.";
            return;
        }

        var stats = result.Data;
        GameName = stats.GameName;
        TotalPlayedMatches = stats.TotalPlayedMatches;

        if (stats.TopResults.Count > 0)
        {
            var best = stats.TopResults[0];
            HighestScoreDisplay = $"{best.Score} ({FormatPlayerName(best.PlayerName, best.IsPlayerDeleted)})";

            TopResultsSummary = string.Join("   •   ", stats.TopResults.Select((r, idx) =>
                $"{idx + 1}. {FormatPlayerName(r.PlayerName, r.IsPlayerDeleted)} — {r.Score}"));
        }
        else
        {
            HighestScoreDisplay = "—";
            TopResultsSummary = "Ще немає зіграних партій";
        }

        _rawPlayers = stats.Players.ToList();
        ApplySorting();
    }

    private void ApplySorting()
    {
        IEnumerable<GamePlayerStatDto> sorted = SelectedSortOption switch
        {
            SortByMatches => _rawPlayers
                .OrderByDescending(p => p.MatchesPlayedInGame)
                .ThenByDescending(p => p.WinsInGame)
                .ThenByDescending(p => p.BestScoreInGame),

            SortByBestScore => _rawPlayers
                .OrderByDescending(p => p.BestScoreInGame)
                .ThenByDescending(p => p.WinsInGame)
                .ThenByDescending(p => p.MatchesPlayedInGame),

            _ => _rawPlayers
                .OrderByDescending(p => p.WinsInGame)
                .ThenByDescending(p => p.MatchesPlayedInGame)
                .ThenByDescending(p => p.BestScoreInGame)
        };

        Players.Clear();
        int index = 1;
        foreach (var player in sorted)
        {
            Players.Add(new GamePlayerStatRowViewModel(
                Index: index++,
                PlayerName: FormatPlayerName(player.PlayerName, player.IsPlayerDeleted),
                MatchesPlayed: player.MatchesPlayedInGame,
                WinsCount: player.WinsInGame,
                BestScore: player.BestScoreInGame));
        }
    }

    private static string FormatPlayerName(string name, bool isDeleted) =>
        isDeleted ? $"{name} (видалений)" : name;
}

public record GamePlayerStatRowViewModel(
    int Index,
    string PlayerName,
    int MatchesPlayed,
    int WinsCount,
    int BestScore
);