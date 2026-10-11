namespace Boardy.WPF.Messages;

public class NavigationMessage
{
    public object ViewModel { get; }
    public NavigationMessage(object viewModel)
    {
        ViewModel = viewModel;
    }
}
