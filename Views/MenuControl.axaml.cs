using Avalonia.Controls;
using Avalonia.Input;

namespace SystemProgramm.Views;

public partial class MenuControl : UserControl
{
    public MenuControl()
    {
        InitializeComponent();
        this.PointerPressed += SecondGrid_OnPointerPressed;
    }
    /// <summary>
    /// нажатие на тень
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SecondGrid_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pointerPosition = e.GetPosition(this);
        var settingPanelBounds = ShadowGrid.Bounds;
        if (settingPanelBounds.Contains(pointerPosition))
        {
            MainWindow.Instance.MenuDisable();
        }
    }
}