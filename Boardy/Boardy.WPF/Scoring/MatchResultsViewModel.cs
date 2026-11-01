using Boardy.Application.Scoring;
using Boardy.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.WPF.Scoring;

public partial class MatchResultsViewModel : ObservableObject
{
    private readonly GetMatchResultsUseCase _getMatchResultsUseCase;

    [ObservableProperty]
    private ObservableCollection<MatchParticipantResultItem> _participants = new();

    [ObservableProperty]
    private string _matchDate = string.Empty;

    [ObservableProperty]
    private string _matchStatus = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public MatchResultsViewModel(GetMatchResultsUseCase getMatchResultsUseCase)
    {
        _getMatchResultsUseCase = getMatchResultsUseCase;
    }

    public void Initialize(Guid matchId)
    {
        _ = LoadMatchResultsAsync(matchId);
    }

    private async Task LoadMatchResultsAsync(Guid matchId)
    {
        var result = await _getMatchResultsUseCase.ExecuteAsync(new GetMatchResultsRequest(matchId));

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося завантажити результати партії.";
            return;
        }

        ErrorMessage = string.Empty;
        MatchDate = result.Data.Date.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        MatchStatus = result.Data.Status == Boardy.Domain.Enums.MatchStatus.Completed ? "Завершена" : "У процесі";

        Participants = new ObservableCollection<MatchParticipantResultItem>(
            result.Data.Participants.Select(participant => new MatchParticipantResultItem(
                participant.Id,
                participant.PlayerName,
                participant.TotalScore,
                participant.IsWinner)));
    }
}

public class MatchParticipantResultItem
{
    public Guid Id { get; }
    public string PlayerName { get; }
    public int TotalScore { get; }
    public bool IsWinner { get; }

    public MatchParticipantResultItem(Guid id, string playerName, int totalScore, bool isWinner)
    {
        Id = id;
        PlayerName = playerName;
        TotalScore = totalScore;
        IsWinner = isWinner;
    }
}
