using Boardy.Application.Authentication.Admin;
using Boardy.Application.GameCatalog.Catalog;
using Boardy.Application.GameCatalog.Game;
using Boardy.Application.Shared;
using Boardy.Domain.Enums;
using Boardy.WPF.Home;
using Boardy.WPF.Scoring;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.WPF.GameCatalog;


public partial class ScoreTemplateDisplayModel : ObservableObject
{
    public Guid Id { get; set; }

    [ObservableProperty]
    private string _templateName = string.Empty;

    public ObservableCollection<CriterionItemViewModel> Criteria { get; } = new();
}

public partial class GameCatalogViewModel : ObservableObject
{
    private readonly GetGameCatalogUseCase _getGameCatalogUseCase;
    private readonly GetGameDetailsUseCase _getGameDetailsUseCase;
    private readonly GetGamesStatisticsUseCase _getGamesStatisticsUseCase;
    private readonly DeleteGameUseCase _deleteGameUseCase;
    private readonly IDialogService _dialogService;
    private readonly ToggleGameStatusUseCase _toggleGameStatusUseCase;
    private readonly IUserSession _userSession;

    [ObservableProperty]
    private ObservableCollection<GameCatalogListItem> _games = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _playersCountText = string.Empty;

    [ObservableProperty] private bool _isDetailsPopupOpen;

    [ObservableProperty] private bool _isTemplatesPopupOpen;

    [ObservableProperty] private string _popupGameName = string.Empty;

    [ObservableProperty] private string _popupGameDescription = string.Empty;

    [ObservableProperty] private string _popupGamePlayers = string.Empty;

    [ObservableProperty] private ObservableCollection<ScoreTemplateDisplayModel> _popupTemplates = new();

    [ObservableProperty]
    private string _mostPlayedGame = "Немає даних";

    [ObservableProperty]
    private string _leastPlayedGame = "Немає даних";

    [ObservableProperty]
    private int _sortIndex;

    private List<GameCatalogListItem> _loadedGames = new();

    private Guid _currentPopupGameId;

    public bool IsAdminMode => _userSession.IsAdminMode;

    public GameCatalogViewModel(
        GetGameCatalogUseCase getGameCatalogUseCase,
        GetGameDetailsUseCase getGameDetailsUseCase,
        GetGamesStatisticsUseCase getGamesStatisticsUseCase,
        ToggleGameStatusUseCase toggleGameStatusUseCase,
        DeleteGameUseCase deleteGameUseCase,
        IDialogService dialogService,
        IUserSession userSession)
    {
        _getGameCatalogUseCase = getGameCatalogUseCase;
        _getGameDetailsUseCase = getGameDetailsUseCase;
        _getGamesStatisticsUseCase = getGamesStatisticsUseCase;
        _toggleGameStatusUseCase = toggleGameStatusUseCase;
        _deleteGameUseCase = deleteGameUseCase;
        _dialogService = dialogService;
        _userSession = userSession;
        _userSession.OnAdminStateChanged += UpdateAdminState;
    }

    [RelayCommand]
    public async Task LoadGamesAsync()
    {
        int? playersCount = null;

        if (!string.IsNullOrWhiteSpace(PlayersCountText))
        {
            if (!int.TryParse(PlayersCountText, out var parsedPlayersCount))
            {
                ErrorMessage = "Кількість гравців має бути числом.";
                return;
            }

            playersCount = parsedPlayersCount;
        }

        var catalogResult = await _getGameCatalogUseCase.ExecuteAsync(new GetGameCatalogRequest(SearchQuery, playersCount));

        if (!catalogResult.IsSuccess || catalogResult.Data is null)
        {
            ErrorMessage = catalogResult.ErrorMessage ?? "Не вдалося завантажити каталог ігор.";
            return;
        }

        var statsResult = await _getGamesStatisticsUseCase.ExecuteAsync(IsAdminMode);
        IReadOnlyDictionary<Guid, int> matchesCountByGame;

        if (statsResult.IsSuccess && statsResult.Data is not null)
        {
            MostPlayedGame = statsResult.Data.MostPlayedGame;
            LeastPlayedGame = statsResult.Data.LeastPlayedGame;
            matchesCountByGame = statsResult.Data.MatchesCountByGame;
        }
        else
        {
            MostPlayedGame = "Немає даних";
            LeastPlayedGame = "Немає даних";
            matchesCountByGame = new Dictionary<Guid, int>();
        }

        ErrorMessage = string.Empty;

        var filteredGames = IsAdminMode
            ? catalogResult.Data
            : catalogResult.Data.Where(g => g.Status == GameStatus.Active);

        _loadedGames = filteredGames
                    .Select((game, index) => new GameCatalogListItem(
                        game.Id,
                        index + 1,
                        game.Name,
                        $"{game.MinPlayers}–{game.MaxPlayers}",
                        matchesCountByGame.GetValueOrDefault(game.Id, 0),
                        game.Status == GameStatus.Active))
                    .ToList();

        ApplySorting();
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchQuery = string.Empty;
        PlayersCountText = string.Empty;
        SortIndex = 0;
        await LoadGamesAsync();
    }


    [RelayCommand]
    private async Task ViewGameDetailsAsync(GameCatalogListItem selectedGame)
    {
        var result = await _getGameDetailsUseCase.ExecuteAsync(new GetGameDetailsRequest(selectedGame.Id));
        if (!result.IsSuccess || result.Data == null) return;

        PopupGameName = result.Data.Name;
        PopupGameDescription = result.Data.Description;
        PopupGamePlayers = $"{result.Data.MinPlayers}-{result.Data.MaxPlayers}";

        IsDetailsPopupOpen = true;
    }


    [RelayCommand]
    private async Task ViewGameTemplatesAsync(GameCatalogListItem selectedGame)
    {
        _currentPopupGameId = selectedGame.Id;
        var result = await _getGameDetailsUseCase.ExecuteAsync(new GetGameDetailsRequest(selectedGame.Id));
        if (!result.IsSuccess || result.Data == null) return;

        PopupGameName = result.Data.Name;
        PopupTemplates.Clear();

        foreach (var template in result.Data.Templates)
        {
            var templateModel = new ScoreTemplateDisplayModel
            {
                Id = template.Id,
                TemplateName = template.Name
            };

            foreach (var criterion in template.Criteria)
            {
                templateModel.Criteria.Add(new CriterionItemViewModel { Name = criterion.Name, IsNegative = criterion.IsNegative });
            }

            PopupTemplates.Add(templateModel);
        }

        IsTemplatesPopupOpen = true;
    }


    [RelayCommand]
    private void ClosePopup()
    {
        IsDetailsPopupOpen = false;
        IsTemplatesPopupOpen = false;
    }


    [RelayCommand]
    private void AddScoreTemplate()
    {
        if (!IsAdminMode) return;

        var createTemplateVm = App.ServiceProvider.GetRequiredService<CreateScoreTemplateViewModel>();
        createTemplateVm.Initialize(_currentPopupGameId);
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(createTemplateVm));
    }


    [RelayCommand]
    private void EditGame(GameCatalogListItem selectedGame)
    {
        if (!IsAdminMode)
        {
            return;
        }

        var updateVm = App.ServiceProvider.GetRequiredService<UpdateGameViewModel>();
        _ = updateVm.InitializeAsync(selectedGame.Id);
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(updateVm));
    }


    [RelayCommand]
    private void EditScoreTemplate(ScoreTemplateDisplayModel selectedTemplate)
    {
        if (!IsAdminMode || selectedTemplate == null) return;

        var editTemplateVm = App.ServiceProvider.GetRequiredService<CreateScoreTemplateViewModel>();

        var criteriaItems = selectedTemplate.Criteria
            .Select(c => new CriterionItemViewModel
            {
                Name = c.Name,
                IsNegative = c.IsNegative
            });

        editTemplateVm.InitializeForEditing(
            _currentPopupGameId,
            selectedTemplate.Id,
            selectedTemplate.TemplateName,
            criteriaItems
        );

        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(editTemplateVm));
    }


    [RelayCommand]
    private void NavigateToCreateGame()
    {
        if (!IsAdminMode)
        {
            return;
        }

        var createGameViewModel = App.ServiceProvider.GetRequiredService<CreateGameViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(createGameViewModel));
    }

    [RelayCommand]
    private async Task DeleteGameAsync(GameCatalogListItem game)
    {
        if (!IsAdminMode)
        {
            return;
        }

        var isConfirmed = _dialogService.Confirm(
            "Видалення гри",
            $"Ви дійсно хочете назавжди видалити {game.Name}?");

        if (!isConfirmed)
        {
            return;
        }

        var result = await _deleteGameUseCase.ExecuteAsync(new DeleteGameRequest(game.Id));

        if (result.IsSuccess)
        {
            await LoadGamesAsync();
        }
        else
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося видалити гру.";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        var homeViewModel = App.ServiceProvider.GetRequiredService<HomeViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(homeViewModel));
    }

    private void UpdateAdminState()
    {
        OnPropertyChanged(nameof(IsAdminMode));
    }

    [RelayCommand]
    private async Task ToggleGameStatusAsync(GameCatalogListItem selectedGame)
    {
        var result = await _toggleGameStatusUseCase.ExecuteAsync(new ToggleGameStatusRequest(selectedGame.Id));
        if (result.IsSuccess)
        {
            await LoadGamesAsync();
        }
        else
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося змінити статус гри.";
        }
    }

    partial void OnSortIndexChanged(int value)
    {
        ApplySorting();
    }

    private void ApplySorting()
    {
        IEnumerable<GameCatalogListItem> sorted = SortIndex switch
        {
            1 => _loadedGames.OrderBy(g => g.Name),
            2 => _loadedGames.OrderByDescending(g => g.MatchesCount).ThenBy(g => g.Name),
            3 => _loadedGames.OrderBy(g => g.MatchesCount).ThenBy(g => g.Name),
            _ => _loadedGames
        };

        Games = new ObservableCollection<GameCatalogListItem>(
            sorted.Select((game, index) => game with { Number = index + 1 }));
    }

}

public record GameCatalogListItem(Guid Id, int Number, string Name, string PlayersCount, int MatchesCount, bool IsActive);
