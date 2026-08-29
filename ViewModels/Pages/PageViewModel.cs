using CommunityToolkit.Mvvm.ComponentModel;
using SystemProgramm.Models;

namespace SystemProgramm.ViewModels.Pages;

public class PageViewModel(PageInfo info) : ObservableObject
{
    public PageInfo Info { get; } = info;
}