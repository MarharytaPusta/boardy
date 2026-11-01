using Boardy.Application.Player;
using Boardy.Domain.Entities;
using Boardy.WPF.Home;
using Boardy.WPF.Player;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.WPF.Scoring;

public partial class SelectMatchPlayersViewModel : ObservableObject
{
    private readonly GetAllPlayers _getAllPlayers;

    [ObservableProperty]
    private ObservableCollection<SelectablePlayerItem> _players = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private int _sortIndex;

    public SelectMatchPlayersViewModel(GetAllPlayers getAllPlayers)
    {
        _getAllPlayers = getAllPlayers;
    }

    public async Task LoadPlayersAsync()
    {
        var result = await _getAllPlayers.ExecuteAsync();

        if (!result.IsSuccess || result.Data is null)
        {
            ErrorMessage = result.ErrorMessage ?? "Не вдалося завантажити гравців.";
            return;
        }

        ErrorMessage = string.Empty;

        var selectedIds = Players.Where(p => p.IsSelected).Select(p => p.Id).ToList();

        Players.Clear();

        var playersList = result.Data.ToList();

        for (int i = 0; i < playersList.Count; i++)
        {
            bool isSelected = selectedIds.Contains(playersList[i].Id);

            Players.Add(new SelectablePlayerItem(i + 1, playersList[i].Id, playersList[i].Name)
            {
                IsSelected = isSelected
            });
        }
    }

    [RelayCommand]
    private void NextStep()
    {
        var selectedIds = Players.Where(p => p.IsSelected).Select(p => p.Id).ToList();

        if (!selectedIds.Any())
        {
            ErrorMessage = "Оберіть хоча б одного гравця!";
            return;
        }

        var selectGameVm = App.ServiceProvider.GetRequiredService<SelectMatchGameViewModel>();
        selectGameVm.Initialize(selectedIds, this);
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(selectGameVm));
    }

    [RelayCommand]
    private void NavigateToCreatePlayer() {
        var createPlayerVm = App.ServiceProvider.GetRequiredService<CreatePlayerViewModel>();
        createPlayerVm.Initialize(this);
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(createPlayerVm));
    }

    [RelayCommand]
    private void GoBack()
    {
        var homeVm = App.ServiceProvider.GetRequiredService<HomeViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(homeVm));
    }

    partial void OnSortIndexChanged(int value)
    {
        ApplySorting();
    }

    private void ApplySorting()
    {
        if (!Players.Any()) return;

        List<SelectablePlayerItem> sortedList;

        switch (SortIndex)
        {
            case 1: 
                sortedList = Players.OrderBy(p => p.Name).ToList();
                break;
            case 2:
                sortedList = Players.OrderBy(p => p.Number).ToList();
                break;
            default:
                sortedList = Players.OrderBy(p => p.Number).ToList();
                break;
        }

        Players.Clear();
        foreach (var item in sortedList)
        {
            Players.Add(item);
        }
    }
}

public partial class SelectablePlayerItem : ObservableObject
{
    public int Number { get; }
    public Guid Id { get; }
    public string Name { get; }

    [ObservableProperty]
    private bool _isSelected;

    public SelectablePlayerItem(int number, Guid id, string name)
    {
        Number = number;
        Id = id;
        Name = name;
    }
}