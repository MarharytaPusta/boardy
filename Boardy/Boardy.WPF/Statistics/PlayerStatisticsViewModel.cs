using Boardy.Application.Statistics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Boardy.WPF.Statistics;

public partial class PlayerStatisticsViewModel : ObservableObject
{
    private readonly GetPlayerStatisticsUseCase _getPlayerStatisticsUseCase;
    private OverallRatingViewModel? _previousViewModel;

    private string _playerName = string.Empty;
    private int _playedGamesCount;
    private int _playedMatchesCount;
    private int _winsCount;
    private ObservableCollection<PlayerGameStatisticsListItem> _games = new();
    private string _errorMessage = string.Empty;

    public string PlayerName
    {
        get => _playerName;
        private set => SetProperty(ref _playerName, value);
    }

    public int PlayedGamesCount
    {
        get => _playedGamesCount;
        private set => SetProperty(ref _playedGamesCount, value);
    }

    public int PlayedMatchesCount
    {
        get => _playedMatchesCount;
        private set => SetProperty(ref _playedMatchesCount, value);
    }

    public int WinsCount
    {
        get => _winsCount;
        private set => SetProperty(ref _winsCount, value);
    }

    public ObservableCollection<PlayerGameStatisticsListItem> Games
    {
        get => _games;
        private set => SetProperty(ref _games, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public PlayerStatisticsViewModel(GetPlayerStatisticsUseCase getPlayerStatisticsUseCase)
    {
        _getPlayerStatisticsUseCase = getPlayerStatisticsUseCase;
    }

    public async Task InitializeAsync(Guid playerId, OverallRatingViewModel previousViewModel)
    {
        _previousViewModel = previousViewModel;
        await LoadStatisticsAsync(playerId);
    }

    private async Task LoadStatisticsAsync(Guid playerId)
    {
        var result = await _getPlayerStatisticsUseCase.ExecuteAsync(playerId);
        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося завантажити статистику гравця.";
            PlayerName = string.Empty;
            PlayedGamesCount = 0;
            PlayedMatchesCount = 0;
            WinsCount = 0;
            Games.Clear();
            return;
        }

        ErrorMessage = string.Empty;
        PlayerName = result.Data.PlayerName;
        PlayedGamesCount = result.Data.PlayedGamesCount;
        PlayedMatchesCount = result.Data.PlayedMatchesCount;
        WinsCount = result.Data.WinsCount;
        Games = new ObservableCollection<PlayerGameStatisticsListItem>(
            result.Data.Games.Select(game => new PlayerGameStatisticsListItem(
                game.GameName,
                game.PlayedMatchesCount,
                game.WinsCount)));
    }

    [RelayCommand]
    private void GoBack()
    {
        if (_previousViewModel is not null)
        {
            WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(_previousViewModel));
        }
    }
}

public record PlayerGameStatisticsListItem(
    string GameName,
    int PlayedMatchesCount,
    int WinsCount);
