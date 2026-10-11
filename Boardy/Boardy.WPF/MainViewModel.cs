using Boardy.WPF.Home;
using Boardy.WPF.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Boardy.WPF;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object _currentViewModel;

    public MainViewModel(HomeViewModel homeViewModel)
    {
        CurrentViewModel = homeViewModel;

        WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
        {
            CurrentViewModel = m.ViewModel;
        });

    }
}
