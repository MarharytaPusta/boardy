using Boardy.Application.GameCatalog.Catalog;
using Boardy.Application.Scoring;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.WPF.Scoring;

public partial class SelectMatchGameViewModel : ObservableObject
{
    private readonly GetGameCatalogUseCase _getGameCatalog;
    private readonly CreateMatchUseCase _createMatchUseCase;

    private SelectMatchPlayersViewModel? _callerVm;


    private List<Guid> _selectedPlayerIds = new();

    [ObservableProperty]
    private ObservableCollection<SelectableGameItem> _games = new();

    [ObservableProperty]
    private SelectableGameItem? _selectedGame;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public SelectMatchGameViewModel(GetGameCatalogUseCase getGameCatalog, CreateMatchUseCase createMatchUseCase, SelectMatchPlayersViewModel callerVm)
    {
        _getGameCatalog = getGameCatalog;
        _createMatchUseCase = createMatchUseCase;
    }

    public void Initialize(List<Guid> playerIds, SelectMatchPlayersViewModel callerVm)
    {
        _selectedPlayerIds = playerIds;
        _callerVm = callerVm;
        _ = LoadGamesFilteredByPlayersAsync();
    }

    private async Task LoadGamesFilteredByPlayersAsync()
    {
        var request = new GetGameCatalogRequest(null, _selectedPlayerIds.Count);
        var result = await _getGameCatalog.ExecuteAsync(request);

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося завантажити ігри.";
            return;
        }

        Games = new ObservableCollection<SelectableGameItem>(
            result.Data.Select((g, i) => new SelectableGameItem(i + 1, g.Id, g.Name)));
    }

    [RelayCommand]
    private async Task StartMatchAsync()
    {
        if (SelectedGame == null)
        {
            ErrorMessage = "Оберіть гру для початку партії.";
            return;
        }

        var request = new CreateMatchRequest(SelectedGame.Id, _selectedPlayerIds);
        var result = await _createMatchUseCase.ExecuteAsync(request);

        if (result.IsSuccess && result.Data is Guid matchId)
        {
            var matchResultsVm = App.ServiceProvider.GetRequiredService<MatchResultsViewModel>();
            matchResultsVm.Initialize(matchId);
            WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(matchResultsVm));
        }
        else
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося створити партію.";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        if (_callerVm != null)
        {
            WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(_callerVm));
        }
    }
}

public record SelectableGameItem(int Number, Guid Id, string Name);