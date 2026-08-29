using System.Collections.ObjectModel;
using SystemProgramm.Models;
using SystemProgramm.ViewModels.Cards;

namespace SystemProgramm.ViewModels.Pages;

public sealed class OverviewViewModel : PageViewModel
{
    public OverviewViewModel() : base(new PageInfo(Localization.PageOverview, IconKind.Overview))
    {
        // Временная заглушка, чтобы посмотреть вёрстку.
        // Значения сняты пробником с этой машины, дальше их заменит HardwareMonitor.
        Cards =
        [
            new OverviewCardViewModel(Localization.CardCpu, IconKind.Cpu)
            {
                Headline = "13th Gen Intel Core i5-13400F",
                Detail = "10 ядер, 16 потоков",
                Load = 16,
                LoadText = "Загрузка 16 %"
            },

            new OverviewCardViewModel(Localization.CardGpu, IconKind.Gpu)
            {
                Headline = "NVIDIA GeForce RTX 4060 Ti",
                Detail = "1134 / 8188 МБ, 53 °C",
                Load = 14,
                LoadText = "Загрузка 14 %"
            },

            new OverviewCardViewModel(Localization.CardMemory, IconKind.Memory)
            {
                Headline = "31,8 ГБ",
                Detail = "Занято 20,8 ГБ, свободно 11,0 ГБ",
                Load = 65,
                LoadText = "Занято 65 %"
            },

            new OverviewCardViewModel(Localization.CardStorage, IconKind.Storage)
            {
                Headline = "ADATA LEGEND 960",
                Detail = "Занято 780 ГБ из 1,86 ТБ",
                Load = 41,
                LoadText = "Занято 41 %"
            },

            new OverviewCardViewModel(Localization.CardNetwork, IconKind.Network)
            {
                Headline = "Беспроводная сеть",
                Detail = "Wi-Fi, 1,2 Гбит/с"
            },

            new OverviewCardViewModel(Localization.CardDevices, IconKind.Devices)
            {
                Headline = "3 устройства",
                Detail = "Клавиатура, мышь, USB-накопитель"
            }
        ];
    }

    public ObservableCollection<OverviewCardViewModel> Cards { get; }
}
