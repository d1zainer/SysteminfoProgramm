using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using SystemProgramm.UI.Managers;

namespace SystemProgramm.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        private readonly List<ToggleButton> _toggleButtons;
        private IBrush _brush => ThemeManager.Instance.GetActualBrush();

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            ThemeManager.Initialize();

            // Инициализация списка ToggleButton
            _toggleButtons = new List<ToggleButton>
            {
                ToggleButtonGPU,
                ToggleButtonHome,
                ToggleButtonCPU,
                ToggleButtonMemory,
                ToggleButtonNet,
                ToggleButtonSetting
            };

            // Назначение обработчиков событий
            ToggleButtonGPU.Click += OnToggleButtonClick;
            ToggleButtonHome.Click += OnToggleButtonClick;
            ToggleButtonCPU.Click += OnToggleButtonClick;
            ToggleButtonMemory.Click += OnToggleButtonClick;
            ToggleButtonNet.Click += OnToggleButtonClick;
            ToggleButtonSetting.Click += OnToggleButtonClick;   

            // Установить начальное состояние для Home
            ToggleSwitch(ToggleButtonHome);

            ButtonExit.Click += (sender, args) => this.Close();
            ButtonResize.Click += (sender, args) => this.WindowState = WindowState.Minimized;

        }


        private void OnToggleButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton clickedButton)
            {
                ToggleSwitch(clickedButton);
            }
        }

        private void ToggleSwitch(ToggleButton toggleButtonToActivate)
        {
            foreach (var toggleButton in _toggleButtons)
            {
                if (Equals(toggleButton, toggleButtonToActivate))
                {
                    toggleButton.Background = Brushes.Lime;
                    toggleButton.Foreground = _brush;
                }
                else
                {
                    toggleButton.ClearValue(BackgroundProperty);
                    toggleButton.Foreground = Brushes.Lime;

                }

                toggleButton.IsChecked = (Equals(toggleButton, toggleButtonToActivate));
            }
        }

       


    }
}
