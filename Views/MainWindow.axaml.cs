using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Material.Icons;
using Material.Icons.Avalonia;
using SystemProgramm.UI.Managers;

namespace SystemProgramm.Views;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; }
    private readonly List<Button> _toggleButtons;

    private MenuControl _menuControl;
    private HomeControl _homeControl;

    private bool _isMenuOpen;

    public MainWindow()
    {
        InitializeComponent();
        Instance = this; 

        _menuControl = new MenuControl
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 0),
            IsVisible = false
        };
        MainGrid.Children.Add(_menuControl);

        //RightGrid.Children.Add(_menuControl);
        // Set the border color of the window in code
        _homeControl = new HomeControl();
        ContentGrid.Children.Add(_homeControl);

        // ������������� ������ ToggleButton
        _toggleButtons = new List<Button>
        {
            ToggleButtonMenu,
            ToggleButtonHome,
            ToggleButtonSetting
        };

        ToggleButtonMenu.Click += ButtonMenuOnClick;
        ToggleButtonHome.Click += OnButtonClick;
        ToggleButtonSetting.Click += OnButtonClick;

        // ���������� ��������� ��������� ��� Home
        ToggleSwitch(ToggleButtonHome);

        ButtonExit.Click += (sender, args) => Close();
        ButtonResize.Click += (sender, args) => WindowState = WindowState.Minimized;
        PointerPressed += OnWindowPointerPressed;
    }

    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pointerPosition = e.GetPosition(this);
        var settingPanelBounds = Chrome.Bounds;
        // ���������, ���� �� ������ ����� ������ ����
        if (settingPanelBounds.Contains(pointerPosition)) BeginMoveDrag(e);
    }

    private void ButtonMenuOnClick(object? sender, RoutedEventArgs e)
    {
        // ����������� ��������� ����
        _isMenuOpen = !_isMenuOpen;
        // ��������� ��������� � ����������� �� ��������� ����
        UpdateMenuState();
    }

    public void MenuDisable()
    {
        // ������������� ��������� ����
        _isMenuOpen = false;
        // ��������� ��������� � ����������� �� ��������� ����
        UpdateMenuState();
    }

    private void UpdateMenuState()
    {
        _menuControl.IsVisible = _isMenuOpen;
        _menuControl.ShadowGrid.IsVisible = _isMenuOpen;

        // ��������� ����������� �������������� � ������ ���������
        ContentGrid.IsHitTestVisible = !_isMenuOpen;
    }


    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button clickedButton) ToggleSwitch(clickedButton);
    }

    private void ToggleSwitch(Button buttonToActivate)
    {
        foreach (var button in _toggleButtons)
            if (Equals(button, buttonToActivate))
            {
                button.Background = ThemeManager.Instance.BrushForegrBrush;
                button.Foreground = ThemeManager.Instance.BrushBackBrush;
            }
            else
            {
                button.Background = ThemeManager.Instance.BrushBackBrush;
                button.Foreground = ThemeManager.Instance.BrushForegrBrush;
            }
    }
}