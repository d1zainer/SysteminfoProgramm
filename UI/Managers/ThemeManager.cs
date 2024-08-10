using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using SystemProgramm.Views;

namespace SystemProgramm.UI.Managers
{
    public class ThemeManager
    {
     

        public static ThemeManager Instance { get; private set; }

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new ThemeManager();
            }
        }

        public IBrush GetActualBrush()
        {
            // Если фон окна установлен как SolidColorBrush, возвращаем его
            if (MainWindow.Instance.Background is SolidColorBrush solidColorBrush)
            {
                return solidColorBrush;
            }
            if (Application.Current.ActualThemeVariant == ThemeVariant.Dark)
                return Brushes.Black;
            else
                return Brushes.White;

        }
    }
}
