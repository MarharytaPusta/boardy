using Boardy.Application.Authentication.Admin;
using Boardy.Application.Player;
using Boardy.Application.Shared;
using Boardy.WPF.Home;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.WPF.Player;

public partial class PlayersViewModel : ObservableObject
{
    private readonly GetAllPlayers _getAllPlayers;
    private readonly IDialogService _dialogService;
    private readonly DeletePlayerUseCase _deletePlayerUseCase;
    private readonly IUserSession _userSession;

    [ObservableProperty]
    private ObservableCollection<PlayerListItem> _players = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool IsAdminMode => _userSession.IsAdminMode;

    public PlayersViewModel(GetAllPlayers getAllPlayers, IDialogService dialogService, DeletePlayerUseCase deletePlayerUseCase, IUserSession userSession)
    {
        _getAllPlayers = getAllPlayers;
        _dialogService = dialogService;
        _deletePlayerUseCase = deletePlayerUseCase;
        _userSession = userSession;

        _userSession.OnAdminStateChanged += UpdateAdminState;
    }

    public async Task LoadPlayersAsync()
    {
        var result = await _getAllPlayers.ExecuteAsync();

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося завантажити гравців.";
            Players = new ObservableCollection<PlayerListItem>();
            return;
        }

        ErrorMessage = string.Empty;
        Players = new ObservableCollection<PlayerListItem>(
            result.Data.Select((player, index) => new PlayerListItem(index + 1, player.Id, player.Name)));
    }

    [RelayCommand]
    private void NavigateToCreatePlayer()
    {
        var createPlayerViewModel = App.ServiceProvider.GetRequiredService<CreatePlayerViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(createPlayerViewModel));
    }

    [RelayCommand]
    private void NavigateToEditPlayer(PlayerListItem player)
    {
        var editPlayerViewModel = App.ServiceProvider.GetRequiredService<EditPlayerViewModel>();
        editPlayerViewModel.SetPlayer(player);
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(editPlayerViewModel));
    }

    [RelayCommand]
    private void GoBack()
    {
        var homeViewModel = App.ServiceProvider.GetRequiredService<HomeViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(homeViewModel));
    }

    [RelayCommand]
    private async Task DeletePlayerAsync(PlayerListItem player)
    {

        if (!IsAdminMode) return;

        bool isConfirmed = _dialogService.Confirm(
            "Видалення гравця",
            $"Ви дійсно хочете назавжди видалити гравця {player.Name}?");

        if (isConfirmed)
        {
            var result = await _deletePlayerUseCase.ExecuteAsync(new DeletePlayerRequest(player.Id));

            if (result.IsSuccess)
            {
                await LoadPlayersAsync();
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Не вдалося видалити гравця.";
            }
        }
    }

    private void UpdateAdminState()
    {
        OnPropertyChanged(nameof(IsAdminMode));
    }
}

public record PlayerListItem(int Number, Guid Id, string Name);
