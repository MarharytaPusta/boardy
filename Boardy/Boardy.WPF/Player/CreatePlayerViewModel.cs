using Boardy.Application.Player;
using Boardy.WPF.Home;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Boardy.WPF.Player;

public partial class CreatePlayerViewModel : ObservableObject
{
    private readonly CreatePlayerUseCase _createPlayerUseCase;

    [ObservableProperty]
    private string _playerName = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public CreatePlayerViewModel(CreatePlayerUseCase createPlayerUseCase)
    {
        _createPlayerUseCase = createPlayerUseCase;
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
            ErrorMessage = "Ім'я не може бути порожнім";
            return;
        }

        try
        {
            var request = new CreatePlayerRequest(PlayerName);
            await _createPlayerUseCase.ExecuteAsync(request);

            GoBack();
        }
        catch (ArgumentException)
        {
            ErrorMessage = "Введено некоректне ім'я гравця";
        }
    }

    private bool EmptyField()
    {
        return string.IsNullOrWhiteSpace(PlayerName);
    }

    [RelayCommand]
    private void GoBack()
    {
        var homeViewModel = App.ServiceProvider.GetRequiredService<HomeViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(homeViewModel));
    }
}