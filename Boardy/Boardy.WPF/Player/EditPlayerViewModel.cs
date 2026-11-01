using Boardy.Application.Player;
using Boardy.WPF.Scoring;
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


    private ObservableObject? _callerViewModel;

    [ObservableProperty]
    private string _playerName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public EditPlayerViewModel(UpdatePlayerUseCase updatePlayerUseCase)
    {
        _updatePlayerUseCase = updatePlayerUseCase;
    }

    public void Initialize(ObservableObject callerViewModel)
    {
        _callerViewModel = callerViewModel;
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
        if (_callerViewModel is PlayersViewModel pvm)
        {
            await pvm.LoadPlayersAsync();
            WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(pvm));
        }
        else if (_callerViewModel is SelectMatchPlayersViewModel smpvm)
        {
            await smpvm.LoadPlayersAsync();
            WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(smpvm));
        }
        else
        {
            var fallbackVm = App.ServiceProvider.GetRequiredService<PlayersViewModel>();
            await fallbackVm.LoadPlayersAsync();
            WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(fallbackVm));
        }
    }
}
