using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;
using System;
using System.IO;
using System.Management;
using static System.Console;

namespace SystemProgramm.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        
        public MainWindowViewModel()
        {
            GetProcessorInfoAndTemperature();
        }

        static void GetProcessorInfoAndTemperature()
        {
            Computer computer = new Computer()
            {

                IsCpuEnabled = true
            };
            computer.Open();

            foreach (var hardwareItem in computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.Cpu)
                {
                    hardwareItem.Update();
                    Console.WriteLine("Processor Name: " + hardwareItem.Name);

                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            Console.WriteLine(sensor.Name + " Temperature: " + sensor.Value.GetValueOrDefault() + " °C");
                        }
                    }
                }
            }

            computer.Close();
        }



    }
}
