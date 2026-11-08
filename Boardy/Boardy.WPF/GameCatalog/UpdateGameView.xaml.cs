using Boardy.Application.GameCatalog.Catalog;
using Boardy.Application.GameCatalog.Game;
using Boardy.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Boardy.WPF.GameCatalog;

public partial class UpdateGameViewModel : ObservableObject
{
    private readonly UpdateGameUseCase _updateGameUseCase;
    private readonly GetGameDetailsUseCase _getGameDetailsUseCase;
    private Guid _gameId;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _minPlayersText = string.Empty;
    [ObservableProperty] private string _maxPlayersText = string.Empty;
    [ObservableProperty] private bool _isActive;

    [ObservableProperty] private string _errorMessage = string.Empty;

    public UpdateGameViewModel(
        UpdateGameUseCase updateGameUseCase,
        GetGameDetailsUseCase getGameDetailsUseCase)
    {
        _updateGameUseCase = updateGameUseCase;
        _getGameDetailsUseCase = getGameDetailsUseCase;
    }

    public async Task InitializeAsync(Guid gameId)
    {
        _gameId = gameId;
        var result = await _getGameDetailsUseCase.ExecuteAsync(new GetGameDetailsRequest(gameId));

        if (result.IsSuccess && result.Data != null)
        {
            Name = result.Data.Name;
            Description = result.Data.Description;
            MinPlayersText = result.Data.MinPlayers.ToString();
            MaxPlayersText = result.Data.MaxPlayers.ToString();
            IsActive = result.Data.Status == GameStatus.Active;
        }
        else
        {
            ErrorMessage = "Не вдалося завантажити деталі гри.";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!int.TryParse(MinPlayersText, out var minPlayers) || !int.TryParse(MaxPlayersText, out var maxPlayers))
        {
            ErrorMessage = "Кількість гравців має бути числом.";
            return;
        }

        var status = IsActive ? GameStatus.Active : GameStatus.Inactive;

        var request = new UpdateGameRequest(_gameId, Name, Description, minPlayers, maxPlayers, status);
        var result = await _updateGameUseCase.ExecuteAsync(request);

        if (result.IsSuccess)
        {
            GoBack();
        }
        else
        {
            ErrorMessage = result.ErrorMessage ?? "Помилка при збереженні.";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        var catalogViewModel = App.ServiceProvider.GetRequiredService<GameCatalogViewModel>();
        _ = catalogViewModel.LoadGamesAsync();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(catalogViewModel));
    }
}