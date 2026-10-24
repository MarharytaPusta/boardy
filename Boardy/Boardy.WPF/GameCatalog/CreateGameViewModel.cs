using Boardy.Application.Authentication.Admin;
using Boardy.Application.GameCatalog.Game;
using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.WPF.GameCatalog;

public partial class CriterionItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isNegative;
}

public partial class CreateGameViewModel : ObservableObject
{
    private readonly CreateGameUseCase _createGameUseCase;
    private readonly CreateScoreTemplateUseCase _createScoreTemplateUseCase;
    private readonly ILogger<CreateGameViewModel> _logger;

    private readonly IUserSession _userSession;
    public bool IsAdminMode => _userSession.IsAdminMode;

    [ObservableProperty]
    private string _gameName = string.Empty;

    [ObservableProperty]
    private string _gameDescription = string.Empty;

    [ObservableProperty]
    private int _minPlayers = 2;

    [ObservableProperty]
    private int _maxPlayers = 4;

    [ObservableProperty]
    private bool _isActive = true;


    public ObservableCollection<CriterionItemViewModel> Criteria { get; } = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public CreateGameViewModel(CreateGameUseCase createGameUseCase, CreateScoreTemplateUseCase createScoreTemplateUseCase, ILogger<CreateGameViewModel> logger, IUserSession userSession)
    {
        _createGameUseCase = createGameUseCase;
        _createScoreTemplateUseCase = createScoreTemplateUseCase;
        _logger = logger;
        _userSession = userSession;

        Criteria.Add(new CriterionItemViewModel());
    }

    [RelayCommand]
    private void AddNewCriterionField()
    {
        Criteria.Add(new CriterionItemViewModel());
    }

    [RelayCommand]
    private async Task SaveGameAsync()
    {

        ErrorMessage = string.Empty;

        if (!ValidateAccess()) return;
        if (!ValidateGameData()) return;
        if (!ValidateCriteriaData()) return;

        var gameStatus = IsActive ? GameStatus.Active : GameStatus.Inactive;
        var gameRequest = new CreateGameRequest(GameName, GameDescription, MinPlayers, MaxPlayers, gameStatus);

        var gameResult = await _createGameUseCase.ExecuteAsync(gameRequest);

        if (!gameResult.IsSuccess)
        {
            ErrorMessage = gameResult.ErrorMessage ?? "Помилка при створенні гри.";
            return;
        }

        bool templateCreated = await CreateScoreTemplateAsync(gameResult.Data);

        if (templateCreated)
        {
            await NavigateToCatalogAsync();
        }
    }


    private bool ValidateAccess()
    {
        if (!IsAdminMode)
        {
            _logger.LogWarning("Unauthorized access: Non-admin attempted to create a game");
            ErrorMessage = "Тільки адміністратор може додавати нові ігри";
            return false;
        }
        return true;
    }

    private bool ValidateGameData()
    {
        if (string.IsNullOrWhiteSpace(GameName))
        {
            ErrorMessage = "Назва гри не може бути порожньою.";
            return false;
        }
        return true;
    }

    private bool ValidateCriteriaData()
    {
        if (Criteria.Count == 0 || Criteria.Any(c => string.IsNullOrWhiteSpace(c.Name)))
        {
            ErrorMessage = "Усі критерії повинні мати назву. Заповніть порожні поля.";
            return false;
        }
        return true;
    }

    private async Task<bool> CreateScoreTemplateAsync(Guid gameId)
    {
        var criteriaDtos = Criteria.Select(c => new CriterionDto(c.Name, c.IsNegative)).ToList();

        var templateRequest = new CreateScoreTemplateRequest(gameId, "Базовий шаблон", criteriaDtos);
        var templateResult = await _createScoreTemplateUseCase.ExecuteAsync(templateRequest);

        if (!templateResult.IsSuccess)
        {
            _logger.LogWarning("UseCase rejected ScoreTemplate creation. Reason: {Reason}", templateResult.ErrorMessage);
            ErrorMessage = templateResult.ErrorMessage ?? "Гра створена, але виникла помилка з шаблоном.";
            return false;
        }

        return true;
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await NavigateToCatalogAsync();
    }

    private static async Task NavigateToCatalogAsync()
    {
        var gameCatalogViewModel = App.ServiceProvider.GetRequiredService<GameCatalogViewModel>();
        await gameCatalogViewModel.LoadGamesAsync();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(gameCatalogViewModel));
    }
}