using Boardy.Application.Authentication.Admin;
using Boardy.Application.GameCatalog.Catalog;
using Boardy.Domain.Enums;
using Boardy.WPF.Home;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.WPF.GameCatalog;

public partial class GameCatalogViewModel : ObservableObject
{
    private readonly GetGameCatalogUseCase _getGameCatalogUseCase;
    private readonly IUserSession _userSession;

    [ObservableProperty]
    private ObservableCollection<GameCatalogListItem> _games = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public bool IsAdminMode => _userSession.IsAdminMode;

    public GameCatalogViewModel(GetGameCatalogUseCase getGameCatalogUseCase, IUserSession userSession)
    {
        _getGameCatalogUseCase = getGameCatalogUseCase;
        _userSession = userSession;
        _userSession.OnAdminStateChanged += UpdateAdminState;
    }

    public async Task LoadGamesAsync()
    {
        await LoadGamesAsync(SearchQuery);
    }

    [RelayCommand]
    private async Task SearchGamesAsync()
    {
        await LoadGamesAsync(SearchQuery);
    }

    private async Task LoadGamesAsync(string? searchQuery)
    {
        var result = await _getGameCatalogUseCase.ExecuteAsync(new GetGameCatalogRequest(searchQuery, null));

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося завантажити каталог ігор.";
            Games = new ObservableCollection<GameCatalogListItem>();
            return;
        }

        ErrorMessage = string.Empty;
        Games = new ObservableCollection<GameCatalogListItem>(result.Data.Select((game, index) =>
            new GameCatalogListItem(
                index + 1,
                game.Name,
                $"{game.MinPlayers}–{game.MaxPlayers}",
                game.Status == GameStatus.Active)));
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
}

public record GameCatalogListItem(int Number, string Name, string PlayersCount, bool IsActive);
