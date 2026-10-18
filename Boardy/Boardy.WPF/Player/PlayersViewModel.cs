using Boardy.Application.Player;
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

    [ObservableProperty]
    private ObservableCollection<PlayerListItem> _players = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public PlayersViewModel(GetAllPlayers getAllPlayers)
    {
        _getAllPlayers = getAllPlayers;
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
}

public record PlayerListItem(int Number, Guid Id, string Name);
