using Boardy.Application.Statistics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Boardy.WPF.Statistics;

public partial class PlayerStatisticsViewModel : ObservableObject
{
    private const string SortByWins = "Кількістю перемог";
    private const string SortByMatches = "Кількістю партій";
    private const string SortByName = "Назвою гри";

    private readonly GetPlayerStatisticsUseCase _getPlayerStatisticsUseCase;
    private OverallRatingViewModel? _previousViewModel;
    private List<PlayerGameStatisticsDto> _allGames = new();

    private string _playerName = string.Empty;
    private int _playedGamesCount;
    private int _playedMatchesCount;
    private int _winsCount;
    private string _searchQuery = string.Empty;
    private string _selectedSortOption = SortByWins;
    private string _emptyStateMessage = "Гравець ще не брав участі в партіях";
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

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                ApplyFiltersAndSorting();
            }
        }
    }

    public IReadOnlyList<string> SortOptions { get; } = new[]
    {
        SortByWins,
        SortByMatches,
        SortByName
    };

    public string SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            if (SetProperty(ref _selectedSortOption, value))
            {
                ApplyFiltersAndSorting();
            }
        }
    }

    public string EmptyStateMessage
    {
        get => _emptyStateMessage;
        private set => SetProperty(ref _emptyStateMessage, value);
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
        _searchQuery = string.Empty;
        OnPropertyChanged(nameof(SearchQuery));
        _selectedSortOption = SortByWins;
        OnPropertyChanged(nameof(SelectedSortOption));

        var result = await _getPlayerStatisticsUseCase.ExecuteAsync(playerId);
        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося завантажити статистику гравця.";
            PlayerName = string.Empty;
            PlayedGamesCount = 0;
            PlayedMatchesCount = 0;
            WinsCount = 0;
            _allGames = new List<PlayerGameStatisticsDto>();
            Games.Clear();
            EmptyStateMessage = "Гравець ще не брав участі в партіях";
            return;
        }

        ErrorMessage = string.Empty;
        PlayerName = result.Data.PlayerName;
        PlayedGamesCount = result.Data.PlayedGamesCount;
        PlayedMatchesCount = result.Data.PlayedMatchesCount;
        WinsCount = result.Data.WinsCount;
        _allGames = result.Data.Games.ToList();
        ApplyFiltersAndSorting();
    }

    private void ApplyFiltersAndSorting()
    {
        IEnumerable<PlayerGameStatisticsDto> filtered = _allGames;

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var query = SearchQuery.Trim();
            filtered = filtered.Where(game =>
                game.GameName.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        IEnumerable<PlayerGameStatisticsDto> sorted = SelectedSortOption switch
        {
            SortByMatches => filtered
                .OrderByDescending(game => game.PlayedMatchesCount)
                .ThenByDescending(game => game.WinsCount)
                .ThenBy(game => game.GameName),

            SortByName => filtered
                .OrderBy(game => game.GameName),

            _ => filtered
                .OrderByDescending(game => game.WinsCount)
                .ThenByDescending(game => game.PlayedMatchesCount)
                .ThenBy(game => game.GameName)
        };

        Games = new ObservableCollection<PlayerGameStatisticsListItem>(
            sorted.Select(game => new PlayerGameStatisticsListItem(
                game.GameName,
                game.PlayedMatchesCount,
                game.WinsCount)));

        EmptyStateMessage = _allGames.Count == 0
            ? "Гравець ще не брав участі в партіях"
            : "Ігор за вказаним запитом не знайдено";
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
