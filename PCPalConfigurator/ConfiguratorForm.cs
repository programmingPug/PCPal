using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using LibreHardwareMonitor.Hardware;
using PCPalConfigurator;

namespace PCPalConfigurator
{
    public partial class ConfiguratorForm : Form
    {
        private ConfigData config;
        private readonly string ConfigFile = GetConfigPath();
        private Computer computer;

        public ConfiguratorForm()
        {
            InitializeComponent();
            InitHardware();
            LoadConfig();
        }

        private void InitHardware()
        {
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsStorageEnabled = true
            };
            computer.Open();
            PopulateSensorOptions();
        }

        private static string GetConfigPath()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCPal");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "config.json");
        }

        private void LoadConfig()
        {
            if (File.Exists(ConfigFile))
            {
                string json = File.ReadAllText(ConfigFile);
                config = JsonConvert.DeserializeObject<ConfigData>(json) ?? new ConfigData();
                ApplyConfig();
            }
            else
            {
                config = new ConfigData();
            }
        }

        private void ApplyConfig()
        {
            cmbLine1.SelectedItem = config.Line1Selection;
            txtLine1.Text = config.Line1CustomText;
            txtLine1Post.Text = config.Line1PostText;
            cmbLine2.SelectedItem = config.Line2Selection;
            txtLine2.Text = config.Line2CustomText;
            txtLine2Post.Text = config.Line2PostText;
        }

        private void SaveConfig()
        {
            // ScreenType based on selected tab
            if (tabControl.SelectedTab == tab1602)
                config.ScreenType = "1602";
            else if (tabControl.SelectedTab == tabTFT)
                config.ScreenType = "TFT4_6";

            config.Line1Selection = cmbLine1.SelectedItem?.ToString();
            config.Line1CustomText = txtLine1.Text;
            config.Line1PostText = txtLine1Post.Text;
            config.Line2Selection = cmbLine2.SelectedItem?.ToString();
            config.Line2CustomText = txtLine2.Text;
            config.Line2PostText = txtLine2Post.Text;

            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigFile, json);
            MessageBox.Show("Settings saved!");
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void PopulateSensorOptions()
        {
            cmbLine1.Items.Clear();
            cmbLine2.Items.Clear();

            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load ||
                        sensor.SensorType == SensorType.Temperature ||
                        sensor.SensorType == SensorType.Data ||
                        sensor.SensorType == SensorType.Fan)
                    {
                        string option = $"{hardware.HardwareType}: {sensor.Name} ({sensor.SensorType})";
                        cmbLine1.Items.Add(option);
                        cmbLine2.Items.Add(option);
                    }
                }
            }

            cmbLine1.Items.Add("Custom Text");
            cmbLine2.Items.Add("Custom Text");
        }
    }
}
