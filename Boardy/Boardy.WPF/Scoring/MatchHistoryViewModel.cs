using Boardy.Application.Authentication.Admin;
using Boardy.Application.Scoring;
using Boardy.Application.Shared;
using Boardy.WPF.Home;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Boardy.WPF.Scoring;

public partial class MatchHistoryViewModel : ObservableObject
{
    private readonly GetMatchHistoryUseCase _getMatchHistoryUseCase;
    private readonly IDialogService _dialogService;
    private readonly DeleteMatchUseCase _deleteMatchUseCase;
    private readonly IUserSession _userSession;

    [ObservableProperty]
    private ObservableCollection<MatchHistoryListItem> _matches = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool IsAdminMode => _userSession.IsAdminMode;

    public MatchHistoryViewModel(
        GetMatchHistoryUseCase getMatchHistoryUseCase,
        IDialogService dialogService,
        DeleteMatchUseCase deleteMatchUseCase,
        IUserSession userSession)
    {
        _getMatchHistoryUseCase = getMatchHistoryUseCase;
        _dialogService = dialogService;
        _deleteMatchUseCase = deleteMatchUseCase;
        _userSession = userSession;

        _userSession.OnAdminStateChanged += UpdateAdminState;
    }

    public async Task LoadMatchesAsync()
    {
        if (!_userSession.IsAdminMode)
        {
            ErrorMessage = "Історія партій доступна лише адміністратору.";
            Matches.Clear();
            return;
        }

        var result = await _getMatchHistoryUseCase.ExecuteAsync();
        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося завантажити історію партій.";
            Matches.Clear();
            return;
        }

        ErrorMessage = string.Empty;
        Matches = new ObservableCollection<MatchHistoryListItem>(
            result.Data.Select((match, index) => new MatchHistoryListItem(
                index + 1,
                match.Id,
                match.GameName,
                match.Date.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
                match.Participants,
                string.IsNullOrWhiteSpace(match.Winners) ? "Немає переможця" : match.Winners)));
    }

    [RelayCommand]
    private void GoBack()
    {
        var homeViewModel = App.ServiceProvider.GetRequiredService<HomeViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(homeViewModel));
    }

    [RelayCommand]
    private void EditMatch(MatchHistoryListItem match)
    {
        var matchResultsViewModel = App.ServiceProvider.GetRequiredService<MatchResultsViewModel>();
        matchResultsViewModel.InitializeForEditing(match.Id, this);
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(matchResultsViewModel));
    }

    [RelayCommand]
    private async Task DeleteMatchAsync(MatchHistoryListItem match)
    {
        if (!IsAdminMode) return;

        bool isConfirmed = _dialogService.Confirm(
            "Видалення партії",
            $"Ви дійсно хочете назавжди видалити партію з гри «{match.GameName}» від {match.Date}?");

        if (isConfirmed)
        {
            var result = await _deleteMatchUseCase.ExecuteAsync(new DeleteMatchRequest(match.Id));

            if (result.IsSuccess)
            {
                await LoadMatchesAsync();
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Не вдалося видалити партію.";
            }
        }
    }

    private void UpdateAdminState()
    {
        OnPropertyChanged(nameof(IsAdminMode));
    }

}

public record MatchHistoryListItem(
    int Number,
    Guid Id,
    string GameName,
    string Date,
    string Participants,
    string Winners);
