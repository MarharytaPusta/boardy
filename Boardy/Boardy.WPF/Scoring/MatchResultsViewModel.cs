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

public partial class MatchResultsViewModel : ObservableObject
{
    private readonly GetMatchResultsUseCase _getMatchResultsUseCase;
    private readonly SaveMatchResultsUseCase _saveMatchResultsUseCase;

    private Guid _matchId;
    private SelectMatchGameViewModel? _callerViewModel;

    [ObservableProperty]
    private ObservableCollection<MatchParticipantResultItem> _participants = new();

    [ObservableProperty]
    private string _matchDate = string.Empty;

    [ObservableProperty]
    private string _gameName = string.Empty;

    [ObservableProperty]
    private string _matchStatus = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;


    private bool _canSaveResults;

    public bool CanSaveResults
    {
        get => _canSaveResults;
        set => SetProperty(ref _canSaveResults, value);
    }

    public MatchResultsViewModel(
        GetMatchResultsUseCase getMatchResultsUseCase,
        SaveMatchResultsUseCase saveMatchResultsUseCase)
    {
        _getMatchResultsUseCase = getMatchResultsUseCase;
        _saveMatchResultsUseCase = saveMatchResultsUseCase;
    }

    public void Initialize(Guid matchId, SelectMatchGameViewModel callerViewModel)
    {
        _matchId = matchId;
        _callerViewModel = callerViewModel;
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
        GameName = result.Data.GameName;
        MatchDate = result.Data.Date.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        MatchStatus = result.Data.Status == Boardy.Domain.Enums.MatchStatus.Completed ? "Завершена" : "У процесі";
        CanSaveResults = result.Data.Status == Boardy.Domain.Enums.MatchStatus.InProgress;

        Participants = new ObservableCollection<MatchParticipantResultItem>(
            result.Data.Participants.Select(participant => new MatchParticipantResultItem(
                participant.Id,
                participant.PlayerName,
                participant.TotalScore,
                participant.IsWinner,
                result.Data.Criteria,
                participant.CriterionResults)));
    }

    [RelayCommand]
    private async Task SaveResultsAsync()
    {
        if (!CanSaveResults)
        {
            return;
        }

        var participantScores = new List<MatchParticipantScoreRequest>();

        foreach (var participant in Participants)
        {
            var criterionScores = new List<CriterionScoreRequest>();
            foreach (var criterion in participant.Criteria)
            {
                if (!int.TryParse(criterion.ScoreText, out var score))
                {
                    ErrorMessage = $"Значення критерію «{criterion.Name}» для гравця {participant.PlayerName} має бути цілим числом.";
                    return;
                }

                criterionScores.Add(new CriterionScoreRequest(criterion.Id, score));
            }

            participantScores.Add(new MatchParticipantScoreRequest(participant.Id, criterionScores));
        }

        var request = new SaveMatchResultsRequest(_matchId, participantScores);

        var result = await _saveMatchResultsUseCase.ExecuteAsync(request);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося зберегти результати партії.";
            return;
        }

        await LoadMatchResultsAsync(_matchId);

        var matchCompletionVm = App.ServiceProvider.GetRequiredService<MatchCompletionViewModel>();
        matchCompletionVm.Initialize(this);
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(matchCompletionVm));
    }

    [RelayCommand]
    private void GoBack()
    {
        if (_callerViewModel is not null)
        {
            WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(_callerViewModel));
        }
    }
}

public partial class MatchParticipantResultItem : ObservableObject
{
    public Guid Id { get; }
    public string PlayerName { get; }

    public ObservableCollection<CriterionScoreItem> Criteria { get; } = new();
    public string TotalScoreText => CalculateTotal().ToString();

    public bool IsWinner { get; }

    public MatchParticipantResultItem(
        Guid id,
        string playerName,
        int totalScore,
        bool isWinner,
        IEnumerable<ScoreCriterionDto> criteria,
        IEnumerable<CriterionResultDto> criterionResults)
    {
        Id = id;
        PlayerName = playerName;
        IsWinner = isWinner;

        var scoresByCriterion = criterionResults.ToDictionary(result => result.CriterionId, result => result.Score);
        foreach (var criterion in criteria)
        {
            var hasSavedScore = scoresByCriterion.TryGetValue(criterion.Id, out var score);
            var item = new CriterionScoreItem(criterion.Id, criterion.Name, criterion.IsNegative,
                hasSavedScore ? score.ToString() : string.Empty);
            item.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(CriterionScoreItem.ScoreText))
                {
                    OnPropertyChanged(nameof(TotalScoreText));
                }
            };
            Criteria.Add(item);
        }
    }

    private int CalculateTotal()
    {
        return Criteria.Sum(criterion =>
        {
            if (!int.TryParse(criterion.ScoreText, out var score))
            {
                return 0;
            }

            return criterion.IsNegative ? -score : score;
        });
    }
}

public partial class CriterionScoreItem : ObservableObject
{
    public Guid Id { get; }
    public string Name { get; }
    public bool IsNegative { get; }

    [ObservableProperty]
    private string _scoreText;

    public CriterionScoreItem(Guid id, string name, bool isNegative, string scoreText)
    {
        Id = id;
        Name = isNegative ? $"{name} (штраф)" : name;
        IsNegative = isNegative;
        _scoreText = scoreText;
    }
}
