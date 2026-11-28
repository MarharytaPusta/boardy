using Boardy.Application.GameCatalog.Catalog;
using Boardy.Application.Scoring;
using Boardy.Domain.Enums;
using Boardy.WPF.GameCatalog;
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
    private readonly GetGameDetailsUseCase _getGameDetailsUseCase;

    private SelectMatchPlayersViewModel? _callerVm;
    private List<Guid> _selectedPlayerIds = new();

    [ObservableProperty]
    private ObservableCollection<SelectableGameItem> _games = new();

    [ObservableProperty]
    private SelectableGameItem? _selectedGame;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty] private bool _isDetailsPopupOpen;
    [ObservableProperty] private bool _isTemplatesPopupOpen;
    [ObservableProperty] private bool _isSelectTemplatePopupOpen;
    [ObservableProperty] private string _popupGameName = string.Empty;
    [ObservableProperty] private string _popupGameDescription = string.Empty;
    [ObservableProperty] private string _popupGamePlayers = string.Empty;

    [ObservableProperty] private ObservableCollection<ScoreTemplateDisplayModel> _popupTemplates = new();
    [ObservableProperty] private ObservableCollection<ScoreTemplateDisplayModel> _selectableTemplates = new();
    [ObservableProperty] private ScoreTemplateDisplayModel? _selectedTemplateForMatch;

    public SelectMatchGameViewModel(GetGameCatalogUseCase getGameCatalog, CreateMatchUseCase createMatchUseCase, GetGameDetailsUseCase getGameDetailsUseCase)
    {
        _getGameCatalog = getGameCatalog;
        _createMatchUseCase = createMatchUseCase;
        _getGameDetailsUseCase = getGameDetailsUseCase;
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
                    result.Data
                        .Where(game => game.ScoreTemplates.Any() && game.Status == GameStatus.Active)
                        .Select((game, index) =>
                        {
                            var templates = game.ScoreTemplates
                                .Select(template =>
                                {
                                    var templateModel = new ScoreTemplateDisplayModel
                                    {
                                        Id = template.Id,
                                        TemplateName = template.Name
                                    };

                                    foreach (var criterion in template.Criteria)
                                    {
                                        templateModel.Criteria.Add(new CriterionItemViewModel
                                        {
                                            Name = criterion.Name,
                                            IsNegative = criterion.IsNegative
                                        });
                                    }

                                    return templateModel;
                                })
                                .ToList();

                            return new SelectableGameItem(
                                index + 1,
                                game.Id,
                                game.Name,
                                templates);
                        }));
    }

    [RelayCommand]
    private async Task StartMatchAsync()
    {
        if (SelectedGame == null)
        {
            ErrorMessage = "Оберіть гру для початку партії.";
            return;
        }

        ErrorMessage = string.Empty;

        if (SelectedGame.Templates.Count == 1)
        {
            await CreateAndNavigateToMatchAsync(SelectedGame.Templates[0].Id);
            return;
        }

        PopupGameName = SelectedGame.Name;
        SelectableTemplates = new ObservableCollection<ScoreTemplateDisplayModel>(SelectedGame.Templates);
        SelectedTemplateForMatch = SelectableTemplates.FirstOrDefault();
        IsSelectTemplatePopupOpen = true;
    }

    [RelayCommand]
    private async Task ConfirmTemplateAndStartMatchAsync()
    {
        if (SelectedTemplateForMatch == null)
        {
            ErrorMessage = "Оберіть шаблон підрахунку балів.";
            return;
        }

        IsSelectTemplatePopupOpen = false;
        await CreateAndNavigateToMatchAsync(SelectedTemplateForMatch.Id);
    }

    private async Task CreateAndNavigateToMatchAsync(Guid scoreTemplateId)
    {
        var request = new CreateMatchRequest(scoreTemplateId, _selectedPlayerIds);
        var result = await _createMatchUseCase.ExecuteAsync(request);

        if (result.IsSuccess && result.Data is Guid matchId)
        {
            var matchResultsVm = App.ServiceProvider.GetRequiredService<MatchResultsViewModel>();
            matchResultsVm.Initialize(matchId, this);
            WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(matchResultsVm));
        }
        else
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося створити партію.";
        }
    }

    [RelayCommand]
    private async Task ViewGameDetailsAsync(SelectableGameItem game)
    {
        var result = await _getGameDetailsUseCase.ExecuteAsync(new GetGameDetailsRequest(game.Id));
        if (!result.IsSuccess || result.Data == null) return;

        PopupGameName = result.Data.Name;
        PopupGameDescription = result.Data.Description;
        PopupGamePlayers = $"{result.Data.MinPlayers}-{result.Data.MaxPlayers}";

        IsDetailsPopupOpen = true;
    }

    [RelayCommand]
    private async Task ViewGameTemplatesAsync(SelectableGameItem game)
    {
        var result = await _getGameDetailsUseCase.ExecuteAsync(new GetGameDetailsRequest(game.Id));
        if (!result.IsSuccess || result.Data == null) return;

        PopupGameName = result.Data.Name;
        PopupTemplates.Clear();

        foreach (var template in result.Data.Templates)
        {
            var templateModel = new ScoreTemplateDisplayModel { TemplateName = template.Name };

            foreach (var criterion in template.Criteria)
            {
                templateModel.Criteria.Add(new CriterionItemViewModel { Name = criterion.Name, IsNegative = criterion.IsNegative });
            }

            PopupTemplates.Add(templateModel);
        }

        IsTemplatesPopupOpen = true;
    }

    [RelayCommand]
    private void ClosePopup()
    {
        IsDetailsPopupOpen = false;
        IsTemplatesPopupOpen = false;
        IsSelectTemplatePopupOpen = false;
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

public record SelectableGameItem(int Number, Guid Id, string Name, IReadOnlyList<ScoreTemplateDisplayModel> Templates);
