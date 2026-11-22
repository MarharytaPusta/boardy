using Boardy.Application.Statistics;
using Boardy.WPF.Home;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Boardy.WPF.Statistics;

public partial class OverallRatingViewModel : ObservableObject
{
    private readonly GetOverallRatingUseCase _getOverallRatingUseCase;
    private List<GetOverallRatingDto> _rating = new();

    private ObservableCollection<OverallRatingListItem> _players = new();

    private string _errorMessage = string.Empty;

    private int _sortIndex = 3;

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

    public OverallRatingViewModel(GetOverallRatingUseCase getOverallRatingUseCase)
    {
        _getOverallRatingUseCase = getOverallRatingUseCase;
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
                player.PlayerName,
                player.PlayedGamesCount,
                player.PlayedMatchesCount,
                player.WinsCount)));
    }
}

public record OverallRatingListItem(
    int Number,
    string Name,
    int PlayedGamesCount,
    int PlayedMatchesCount,
    int WinsCount);
