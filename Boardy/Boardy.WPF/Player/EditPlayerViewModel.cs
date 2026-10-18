using Boardy.Application.Player;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Boardy.WPF.Player;

public partial class EditPlayerViewModel : ObservableObject
{
    private readonly UpdatePlayerUseCase _updatePlayerUseCase;
    private Guid _playerId;

    [ObservableProperty]
    private string _playerName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public EditPlayerViewModel(UpdatePlayerUseCase updatePlayerUseCase)
    {
        _updatePlayerUseCase = updatePlayerUseCase;
    }

    public void SetPlayer(PlayerListItem player)
    {
        _playerId = player.Id;
        PlayerName = player.Name;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SavePlayerAsync()
    {
        var request = new UpdatePlayerRequest(_playerId, PlayerName);
        var result = await _updatePlayerUseCase.ExecuteAsync(request);

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage ?? string.Empty;
            return;
        }

        await GoBackAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        var playersViewModel = App.ServiceProvider.GetRequiredService<PlayersViewModel>();
        await playersViewModel.LoadPlayersAsync();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(playersViewModel));
    }
}
