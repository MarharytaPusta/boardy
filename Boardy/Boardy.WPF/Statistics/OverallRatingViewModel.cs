using Boardy.Application.Authentication.Admin;
using Boardy.Application.Player;
using Boardy.Application.Shared;
using Boardy.Application.Statistics;
using Boardy.WPF.Home;
using Boardy.WPF.Player;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Boardy.WPF.Statistics;

public partial class OverallRatingViewModel : ObservableObject
{
    private readonly GetOverallRatingUseCase _getOverallRatingUseCase;
    private readonly DeletePlayerUseCase _deletePlayerUseCase;
    private readonly IDialogService _dialogService;
    private readonly IUserSession _userSession;
    private List<GetOverallRatingDto> _rating = new();

    private ObservableCollection<OverallRatingListItem> _players = new();

    private string _errorMessage = string.Empty;

    private int _sortIndex = 3;

    public bool IsAdminMode => _userSession.IsAdminMode;

    public ObservableCollection<OverallRatingListItem> Players
    {
        get => _players;
        private set => SetProperty(ref _players, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public int SortIndex
    {
        get => _sortIndex;
        set
        {
            if (SetProperty(ref _sortIndex, value))
            {
                ApplySorting();
            }
        }
    }

    public OverallRatingViewModel(
        GetOverallRatingUseCase getOverallRatingUseCase,
        DeletePlayerUseCase deletePlayerUseCase,
        IDialogService dialogService,
        IUserSession userSession)
    {
        _getOverallRatingUseCase = getOverallRatingUseCase;
        _deletePlayerUseCase = deletePlayerUseCase;
        _dialogService = dialogService;
        _userSession = userSession;
        _userSession.OnAdminStateChanged += UpdateAdminState;
    }

    public async Task LoadRatingAsync()
    {
        var result = await _getOverallRatingUseCase.ExecuteAsync();
        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося завантажити загальний рейтинг.";
            Players.Clear();
            return;
        }

        ErrorMessage = string.Empty;
        _rating = result.Data.ToList();
        ApplySorting();
    }

    [RelayCommand]
    private void GoBack()
    {
        var homeViewModel = App.ServiceProvider.GetRequiredService<HomeViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(homeViewModel));
    }

    [RelayCommand]
    private async Task NavigateToPlayerStatisticsAsync(OverallRatingListItem player)
    {
        var playerStatisticsViewModel = App.ServiceProvider.GetRequiredService<PlayerStatisticsViewModel>();
        await playerStatisticsViewModel.InitializeAsync(player.PlayerId, this);
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(playerStatisticsViewModel));
    }

    [RelayCommand]
    private void NavigateToCreatePlayer()
    {
        var createPlayerViewModel = App.ServiceProvider.GetRequiredService<CreatePlayerViewModel>();
        createPlayerViewModel.Initialize(this);
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(createPlayerViewModel));
    }

    [RelayCommand]
    private void NavigateToEditPlayer(OverallRatingListItem player)
    {
        var editPlayerViewModel = App.ServiceProvider.GetRequiredService<EditPlayerViewModel>();
        editPlayerViewModel.SetPlayer(new PlayerListItem(player.Number, player.PlayerId, player.Name));
        editPlayerViewModel.Initialize(this);
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(editPlayerViewModel));
    }

    [RelayCommand]
    private async Task DeletePlayerAsync(OverallRatingListItem player)
    {
        if (!IsAdminMode)
        {
            return;
        }

        var isConfirmed = _dialogService.Confirm(
            "Видалення гравця",
            $"Ви дійсно хочете назавжди видалити {player.Name}?");
        if (!isConfirmed)
        {
            return;
        }

        var result = await _deletePlayerUseCase.ExecuteAsync(new DeletePlayerRequest(player.PlayerId));
        if (result.IsSuccess)
        {
            await LoadRatingAsync();
        }
        else
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося видалити гравця.";
        }
    }

    private void UpdateAdminState()
    {
        OnPropertyChanged(nameof(IsAdminMode));
    }

    private void ApplySorting()
    {
        IEnumerable<GetOverallRatingDto> sortedRating = SortIndex switch
        {
            0 => _rating.OrderBy(player => player.PlayerName),
            1 => _rating.OrderByDescending(player => player.PlayedGamesCount)
                .ThenBy(player => player.PlayerName),
            2 => _rating.OrderByDescending(player => player.PlayedMatchesCount)
                .ThenBy(player => player.PlayerName),
            _ => _rating.OrderByDescending(player => player.WinsCount)
                .ThenByDescending(player => player.PlayedMatchesCount)
                .ThenByDescending(player => player.PlayedGamesCount)
                .ThenBy(player => player.PlayerName)
        };

        Players = new ObservableCollection<OverallRatingListItem>(
            sortedRating.Select((player, index) => new OverallRatingListItem(
                index + 1,
                player.PlayerId,
                player.PlayerName,
                player.PlayedGamesCount,
                player.PlayedMatchesCount,
                player.WinsCount)));
    }
}

public record OverallRatingListItem(
    int Number,
    Guid PlayerId,
    string Name,
    int PlayedGamesCount,
    int PlayedMatchesCount,
    int WinsCount);
