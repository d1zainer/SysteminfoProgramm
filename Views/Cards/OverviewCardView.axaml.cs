using Avalonia.Controls;
using SystemProgramm.ViewModels.Cards;

namespace SystemProgramm.Views.Cards;

public partial class OverviewCardView : UserControl
{
    public OverviewCardView()
    {
        InitializeComponent();

        Tapped += (_, _) => (DataContext as OverviewCardViewModel)?.Activate();
    }
}