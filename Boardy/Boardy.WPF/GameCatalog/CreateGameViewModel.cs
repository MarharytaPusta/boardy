using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.WPF.Home;
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
}

public partial class CreateGameViewModel : ObservableObject
{
    private readonly CreateScoreTemplateUseCase _createScoreTemplateUseCase;
    private readonly ILogger<CreateGameViewModel> _logger;

    public ObservableCollection<CriterionItemViewModel> Criteria { get; } = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public CreateGameViewModel(
        CreateScoreTemplateUseCase createScoreTemplateUseCase,
        ILogger<CreateGameViewModel> logger)
    {
        _createScoreTemplateUseCase = createScoreTemplateUseCase;
        _logger = logger;

        Criteria.Add(new CriterionItemViewModel());
    }

    [RelayCommand]
    private void AddNewCriterionField()
    {
        Criteria.Add(new CriterionItemViewModel());
    }

    [RelayCommand]
    private async Task SaveTemplateAsync()
    {
        ErrorMessage = string.Empty;

        if (Criteria.Count == 0 || Criteria.Any(c => string.IsNullOrWhiteSpace(c.Name)))
        {
            _logger.LogWarning("Invalid input: Attempted to save template with missing or empty criteria.");
            ErrorMessage = "Усі критерії повинні мати назву. Заповніть порожні поля.";
            return;
        }

        var criteriaDtos = Criteria.Select(c => new CriterionDto(c.Name)).ToList();

        var request = new CreateScoreTemplateRequest("Базовий шаблон", criteriaDtos);
        var result = await _createScoreTemplateUseCase.ExecuteAsync(request);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("UseCase rejected ScoreTemplate creation. Reason: {Reason}", result.ErrorMessage);
            ErrorMessage = result.ErrorMessage ?? string.Empty;
            return;
        }

        GoBack();
    }

    [RelayCommand]
    private void GoBack()
    {
        var homeViewModel = App.ServiceProvider.GetRequiredService<HomeViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(homeViewModel));
    }
}