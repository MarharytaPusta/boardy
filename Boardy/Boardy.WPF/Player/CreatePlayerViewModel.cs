using Boardy.Application.Player;
using Boardy.WPF.Home;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Boardy.WPF.Player;

public partial class CreatePlayerViewModel : ObservableObject
{
    private readonly CreatePlayerUseCase _createPlayerUseCase;
    private readonly ILogger<CreatePlayerViewModel> _logger;


    [ObservableProperty]
    private string _playerName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public CreatePlayerViewModel(CreatePlayerUseCase createPlayerUseCase, ILogger<CreatePlayerViewModel> logger)
    {
        _createPlayerUseCase = createPlayerUseCase;
        _logger = logger;
    }

    partial void OnPlayerNameChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            ErrorMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task SavePlayerAsync()
    {
        ErrorMessage = string.Empty;

        if (EmptyField())
        {
            _logger.LogWarning("Invalid input: user attempted to create a player with an empty name");
            ErrorMessage = "Ім'я не може бути порожнім";
            return;
        }

        try
        {
            var request = new CreatePlayerRequest(PlayerName);
            await _createPlayerUseCase.ExecuteAsync(request);

            await GoBackAsync();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input: domain entity rejected the player name '{PlayerName}'.", PlayerName);
            ErrorMessage = "Введено некоректне ім'я гравця";
        }
    }

    private bool EmptyField()
    {
        return string.IsNullOrWhiteSpace(PlayerName);
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        var playersViewModel = App.ServiceProvider.GetRequiredService<PlayersViewModel>();
        await playersViewModel.LoadPlayersAsync();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(playersViewModel));
    }
}
