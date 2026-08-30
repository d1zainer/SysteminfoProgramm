using CommunityToolkit.Mvvm.ComponentModel;

namespace SystemProgramm.ViewModels.Pages;

public sealed partial class DetailRowViewModel(string name) : ObservableObject
{
    [ObservableProperty]
    private string? _value;

    public string Name { get; } = name;
}