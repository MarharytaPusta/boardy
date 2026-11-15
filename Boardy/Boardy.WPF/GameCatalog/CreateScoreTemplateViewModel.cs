using Boardy.Application.Authentication.Admin;
using Boardy.Application.GameCatalog.ScoreTemplate;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.WPF.GameCatalog;

public partial class CreateScoreTemplateViewModel : ObservableObject
{
    private readonly CreateScoreTemplateUseCase _createUseCase;
    private readonly UpdateScoreTemplateUseCase _updateUseCase;
    private readonly IUserSession _userSession;
    private readonly ILogger<CreateScoreTemplateViewModel> _logger;

    private Guid _gameId;
    private Guid? _editingTemplateId;

    [ObservableProperty] private string _templateName = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    public ObservableCollection<CriterionItemViewModel> Criteria { get; } = new();

    public string PageTitle => _editingTemplateId == null ? "Новий шаблон оцінювання" : "Редагувати шаблон";
    public string SaveButtonText => _editingTemplateId == null ? "Створити" : "Зберегти";

    public CreateScoreTemplateViewModel(CreateScoreTemplateUseCase createUseCase, UpdateScoreTemplateUseCase updateUseCase, IUserSession userSession, ILogger<CreateScoreTemplateViewModel> logger)
    {
        _createUseCase = createUseCase;
        _updateUseCase = updateUseCase;
        _userSession = userSession;
        _logger = logger;
    }

    public void Initialize(Guid gameId)
    {
        _gameId = gameId;
        _editingTemplateId = null;
        TemplateName = string.Empty;
        Criteria.Clear();
        Criteria.Add(new CriterionItemViewModel());
        ErrorMessage = string.Empty;

        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    [RelayCommand]
    private void AddCriterion()
    {
        Criteria.Add(new CriterionItemViewModel());
    }

    [RelayCommand]
    private void RemoveCriterion(CriterionItemViewModel criterion)
    {
        if (Criteria.Count > 1)
        {
            Criteria.Remove(criterion);
        }
        else
        {
            ErrorMessage = "Шаблон повинен мати хоча б один критерій.";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;

        if (!_userSession.IsAdminMode)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(TemplateName))
        {
            ErrorMessage = "Назва шаблону не може бути порожньою.";
            return;
        }

        if (Criteria.Count == 0 || Criteria.Any(c => string.IsNullOrWhiteSpace(c.Name)))
        {
            ErrorMessage = "Усі критерії повинні мати назву. Заповніть порожні поля.";
            return;
        }

        var criteriaDtos = Criteria
            .Select(c => new CriterionDto(c.Name.Trim(), c.IsNegative))
            .ToList();

        try
        {
            var result = _editingTemplateId == null
                ? await _createUseCase.ExecuteAsync(new CreateScoreTemplateRequest(_gameId, TemplateName.Trim(), criteriaDtos))
                : await _updateUseCase.ExecuteAsync(new UpdateScoreTemplateRequest(_gameId, _editingTemplateId.Value, TemplateName.Trim(), criteriaDtos));

            if (result.IsSuccess)
            {
                GoBack();
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Помилка при збереженні шаблону.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving score template '{TemplateName}'", TemplateName);
            ErrorMessage = "Сталася критична помилка при збереженні.";
        }
    }

    [RelayCommand]
    private  void GoBack()
    {
        var catalogVm = App.ServiceProvider.GetRequiredService<GameCatalogViewModel>();
        _ = catalogVm.LoadGamesAsync();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(catalogVm));
    }

    public void InitializeForEditing(Guid gameId, Guid templateId, string currentName, IEnumerable<CriterionItemViewModel> existingCriteria)
    {
        _gameId = gameId;
        _editingTemplateId = templateId;
        TemplateName = currentName;

        Criteria.Clear();
        foreach (var c in existingCriteria)
        {
            Criteria.Add(new CriterionItemViewModel { Name = c.Name, IsNegative = c.IsNegative });
        }

        ErrorMessage = string.Empty;

        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }
}