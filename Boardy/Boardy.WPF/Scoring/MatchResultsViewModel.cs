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
        MatchDate = result.Data.Date.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        MatchStatus = result.Data.Status == Boardy.Domain.Enums.MatchStatus.Completed ? "Завершена" : "У процесі";
        CanSaveResults = result.Data.Status == Boardy.Domain.Enums.MatchStatus.InProgress;

        Participants = new ObservableCollection<MatchParticipantResultItem>(
            result.Data.Participants.Select(participant => new MatchParticipantResultItem(
                participant.Id,
                participant.PlayerName,
                participant.TotalScore,
                participant.IsWinner)));
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
            if (!int.TryParse(participant.TotalScoreText, out var totalScore))
            {
                ErrorMessage = $"Бали гравця {participant.PlayerName} мають бути цілим числом.";
                return;
            }

            participantScores.Add(new MatchParticipantScoreRequest(participant.Id, totalScore));
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

    private string _totalScoreText;

    public string TotalScoreText
    {
        get => _totalScoreText;
        set => SetProperty(ref _totalScoreText, value);
    }

    public bool IsWinner { get; }

    public MatchParticipantResultItem(Guid id, string playerName, int totalScore, bool isWinner)
    {
        Id = id;
        PlayerName = playerName;
        _totalScoreText = totalScore.ToString();
        IsWinner = isWinner;
    }
}
