using Boardy.Application.GameCatalog.ScoreTemplate;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.WPF.GameCatalog;

public partial class CreateScoreTemplateViewModel : ObservableObject
{
    private readonly CreateScoreTemplateUseCase _createUseCase;
    private Guid _gameId;

    [ObservableProperty] private string _templateName = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    public ObservableCollection<CriterionItemViewModel> Criteria { get; } = new();

    public CreateScoreTemplateViewModel(CreateScoreTemplateUseCase createUseCase)
    {
        _createUseCase = createUseCase;
        Criteria.Add(new CriterionItemViewModel());
    }

    public void Initialize(Guid gameId)
    {
        _gameId = gameId;
        TemplateName = string.Empty;
        Criteria.Clear();
        Criteria.Add(new CriterionItemViewModel());
        ErrorMessage = string.Empty;
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

        var request = new CreateScoreTemplateRequest(_gameId, TemplateName.Trim(), criteriaDtos);
        var result = await _createUseCase.ExecuteAsync(request);

        if (result.IsSuccess)
        {
            GoBack();
        }
        else
        {
            ErrorMessage = result.ErrorMessage ?? "Помилка при збереженні.";
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        var catalogVm = App.ServiceProvider.GetRequiredService<GameCatalogViewModel>();
        _ = catalogVm.LoadGamesAsync();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(catalogVm));
    }
}