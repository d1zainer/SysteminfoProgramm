using CommunityToolkit.Mvvm.ComponentModel;

namespace SystemProgramm.ViewModels.Pages;

public class PageViewModel(string title) : ObservableObject
{
    public string Title { get; } = title;
}