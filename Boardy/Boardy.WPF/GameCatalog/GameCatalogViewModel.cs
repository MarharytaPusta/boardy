using Boardy.Application.Authentication.Admin;
using Boardy.Application.GameCatalog.Catalog;
using Boardy.Application.GameCatalog.Game;
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
    [ObservableProperty]
    private string _templateName = string.Empty;

    public ObservableCollection<CriterionItemViewModel> Criteria { get; } = new();
}

public partial class GameCatalogViewModel : ObservableObject
{
    private readonly GetGameCatalogUseCase _getGameCatalogUseCase;
    private readonly GetGameDetailsUseCase _getGameDetailsUseCase;
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

    private Guid _currentPopupGameId;

    public bool IsAdminMode => _userSession.IsAdminMode;

    public GameCatalogViewModel(GetGameCatalogUseCase getGameCatalogUseCase, GetGameDetailsUseCase getGameDetailsUseCase, ToggleGameStatusUseCase toggleGameStatusUseCase, IUserSession userSession)
    {
        _getGameCatalogUseCase = getGameCatalogUseCase;
        _getGameDetailsUseCase = getGameDetailsUseCase;
        _toggleGameStatusUseCase = toggleGameStatusUseCase;
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

        var result = await _getGameCatalogUseCase.ExecuteAsync(new GetGameCatalogRequest(SearchQuery, playersCount));

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося завантажити каталог ігор.";
            return;
        }

        ErrorMessage = string.Empty;

        var filteredGames = IsAdminMode
            ? result.Data
            : result.Data.Where(g => g.Status == GameStatus.Active);

        Games = new ObservableCollection<GameCatalogListItem>(filteredGames.Select((game, index) =>
            new GameCatalogListItem(
                game.Id,
                index + 1,
                game.Name,
                $"{game.MinPlayers}–{game.MaxPlayers}",
                game.Status == GameStatus.Active)));
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchQuery = string.Empty;
        PlayersCountText = string.Empty;
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
            var templateModel = new ScoreTemplateDisplayModel { TemplateName = template.Name };

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
    private void AddScoreTemplate(GameCatalogListItem selectedGame)
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

}

public record GameCatalogListItem(Guid Id, int Number, string Name, string PlayersCount, bool IsActive);
