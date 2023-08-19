using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Gui_Miner.Form1;
using static Gui_Miner.SettingsForm;

namespace Gui_Miner
{
    public partial class SettingsForm : Form
    {
        #region Variables
        Settings _settings = new Settings();
        const string SETTINGSNAME = "Settings";
        const string MINERSETTINGSPANELNAME = "tableLayoutPanel";
        const string GPUSETTINGSPANELNAME = "gpuTableLayoutPanel";
        public Settings Settings { get { return _settings; } }
        public Form1 Form1 { get; set; }
        public void SetSettings(Settings settings) { _settings = settings; }
        
        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }
        #endregion

        // Start/Stop Load/Save
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (_settings == null) return;

            UpdateMinerSettingsListBox();
            UpdateGpusListBox();

            DisplayMinerSettings();
            DisplayGpuSettings();
        }
        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            SaveSettings();
            this.Visible = false;
        }
        private void LoadSettings()
        {
            _settings = AppSettings.Load<Settings>(SETTINGSNAME);
            if (_settings == null)
            {
                _settings = new Settings();
            }
        }
        public void SaveSettings()
        {
            var uiMinerSetting = GetMinerSettingsFromUI();
            if (_settings.MinerSettings != null && _settings.MinerSettings.Count > 0)
            {
                for (int i = 0; i < _settings.MinerSettings.Count; i++)
                {
                    if (_settings.MinerSettings[i].Id == uiMinerSetting.Id)
                        _settings.MinerSettings[i] = uiMinerSetting;
                }
            }
            else if (uiMinerSetting != null)
                _settings.MinerSettings = new List<MinerConfig> { uiMinerSetting };

            var uiGpuSetting = GetGpuSettingsFromUI();
            if (_settings.Gpus != null && _settings.Gpus.Count > 0)
            {
                for (int i = 0; i < _settings.Gpus.Count; i++)
                {
                    if (_settings.Gpus[i].Id == uiGpuSetting.Id)
                        _settings.Gpus[i] = uiGpuSetting;
                }
            }
            else if (uiGpuSetting != null && !uiGpuSetting.isEquals(new Gpu()))
                _settings.Gpus = new List<Gpu> { uiGpuSetting };

            AppSettings.Save<Settings>(SETTINGSNAME, _settings);
        }


        // Update listboxes
        private void UpdateMinerSettingsListBox(int selectedIndex = 1)
        {
            if (_settings.MinerSettings == null) return;

            minerSettingsListBox.Items.Clear();
            minerSettingsListBox.Items.Add("Add Miner Settings");

            foreach (MinerConfig minerSetting in _settings.MinerSettings)
            {
                minerSettingsListBox.Items.Add(minerSetting.Name + " / " + minerSetting.Id);
            }
            minerSettingsListBox.SelectedIndex = selectedIndex;
        }
        private void UpdateGpusListBox(int selectedIndex = 1)
        {
            if (_settings.Gpus == null) return;

            gpuListBox.Items.Clear();
            gpuListBox.Items.Add("Add GPU");

            foreach (Gpu gpu in _settings.Gpus)
            {
                gpuListBox.Items.Add(gpu.Name + " / " + gpu.Id);
            }
            gpuListBox.SelectedIndex = selectedIndex;
        }


        // Miner Settings
        private MinerConfig GetSelectedMinerSettings()
        {
            MinerConfig minerSetting = null;

            if (minerSettingsListBox.SelectedIndex <= 0) return minerSetting;

            int id = int.Parse(minerSettingsListBox.SelectedItem.ToString().Split('/')[1].Trim());

            foreach (MinerConfig savedMinerSetting in _settings.MinerSettings)
            {
                if (savedMinerSetting.Id == id)
                    return savedMinerSetting;
            }

            return minerSetting;
        }
        private MinerConfig GetMinerSettingsFromUI()
        {
            MinerConfig newMinerSetting = new MinerConfig();
            TableLayoutPanel tableLayoutPanel = this.Controls.Find(MINERSETTINGSPANELNAME, true).FirstOrDefault() as TableLayoutPanel;
            int id = 0;

            if (tableLayoutPanel == null) return newMinerSetting;

            // CUSTOM SETTINGS
            newMinerSetting.batFileArguments = batLineTextBox.Text;

            foreach (Control control in tableLayoutPanel.Controls)
            {
                if (control is Label label)
                {
                    string propertyName = label.Text;                    
                    PropertyInfo property = typeof(MinerConfig).GetProperty(propertyName); // Use MinerSetting type

                    if (property != null)
                    {
                        if (control is TextBox textbox)
                        {
                            string propertyValue = textbox.Text;

                            if (propertyName.ToLower() == "id" && propertyValue != null)
                                id = int.Parse(propertyValue);

                            if (!string.IsNullOrEmpty(propertyValue))
                            {
                                object convertedValue = Convert.ChangeType(propertyValue, property.PropertyType);
                                property.SetValue(newMinerSetting, convertedValue);
                            }
                        }
                        else if (control is CheckBox checkBox)
                        {
                            bool propertyValue = checkBox.Checked;
                            property.SetValue(newMinerSetting, propertyValue);
                        }
                    }
                }
            }

            return newMinerSetting;
        }
        private void DisplayMinerSettings()
        {
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Name = MINERSETTINGSPANELNAME;
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.AutoScroll = true;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            if (_settings.MinerSettings != null)
            {
                MinerConfig minerSetting = GetSelectedMinerSettings();
                PropertyInfo[] properties = typeof(MinerConfig).GetProperties();

                // CUSTOM SETTINGS
                // Set .bat file textbox
                batLineTextBox.Text = minerSetting.batFileArguments;

                foreach (PropertyInfo property in properties)
                {
                    // Skip default properties
                    if (property.Name.StartsWith("Default") 
                        || property.Name.Equals("batFileArguments")) continue;

                    Label label = new Label();
                    label.Text = property.Name;
                    label.AutoSize = true;
                    label.Anchor = AnchorStyles.None; // Center the text horizontally
                    label.TextAlign = ContentAlignment.MiddleCenter; // Center the text vertically

                    Control inputControl; // This will be either a TextBox or CheckBox

                    if (property.PropertyType == typeof(bool))
                    {
                        // Create a CheckBox for boolean properties
                        CheckBox checkBox = new CheckBox();
                        checkBox.Checked = (bool)property.GetValue(minerSetting);
                        checkBox.CheckedChanged += (sender, e) =>
                        {
                            property.SetValue(minerSetting, checkBox.Checked);
                        };

                        inputControl = checkBox;
                    }
                    else
                    {
                        // Create a TextBox for non-boolean properties
                        TextBox textbox = new TextBox();
                        textbox.Text = property.GetValue(minerSetting)?.ToString();
                        textbox.KeyUp += (sender, e) =>
                        {
                            if (e.KeyCode == Keys.Enter)
                            {
                                SaveSettings();
                                UpdateMinerSettingsListBox(minerSettingsListBox.SelectedIndex);
                            }
                        };

                        inputControl = textbox;
                    }


                    // Hide Id's
                    if (property.Name.ToLower().Equals("id"))
                    {
                        inputControl.Enabled = false;
                    }

                    tableLayoutPanel.Controls.Add(label, 0, tableLayoutPanel.RowCount);
                    tableLayoutPanel.Controls.Add(inputControl, 1, tableLayoutPanel.RowCount);

                    tableLayoutPanel.RowCount++;
                }

            }

            // Remove existing panel if there is one
            TableLayoutPanel existingPanel = minerSettingsPanel.Controls.Find(MINERSETTINGSPANELNAME, true).FirstOrDefault() as TableLayoutPanel;
            if (existingPanel != null) { minerSettingsPanel.Controls.Remove(existingPanel); }

            // Add panel
            minerSettingsPanel.Controls.Add(tableLayoutPanel);
        }
        private void minerSettingsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (minerSettingsListBox.SelectedIndex == -1) return;

            // Adding new item
            if (minerSettingsListBox.SelectedIndex == 0)
            {
                // Create and add new miner setting
                if (_settings.MinerSettings == null)
                    _settings.MinerSettings = new List<MinerConfig>();
                else
                    _settings.MinerSettings.Add(Form1.GetWhichMinerConfigToUse());

                UpdateMinerSettingsListBox(_settings.MinerSettings.Count);
            }

            DisplayMinerSettings();
        }


        // Gpus
        private Gpu GetSelectedGpu()
        {
            Gpu gpu = new Gpu();

            if(gpuListBox.SelectedIndex <= 0) return gpu;

            int id = int.Parse(gpuListBox.SelectedItem.ToString().Split('/')[1].Trim());

            foreach(Gpu savedGpu in _settings.Gpus)
            {
                if(savedGpu.Id == id)
                    return savedGpu;
            }

            return gpu;
        }      
        private Gpu GetGpuSettingsFromUI()
        {
            var newGpu = new Gpu();
            TableLayoutPanel tableLayoutPanel = this.Controls.Find(GPUSETTINGSPANELNAME, true).FirstOrDefault() as TableLayoutPanel;
            int id = 0;

            if (tableLayoutPanel == null) return newGpu;

            foreach (Control control in tableLayoutPanel.Controls)
            {
                if (control is Label label)
                {
                    string propertyName = label.Text;
                    PropertyInfo property = typeof(Gpu).GetProperty(propertyName); // Use Gpu type

                    if (property != null)
                    {
                        if (control is TextBox textBox)
                        {
                            string propertyValue = textBox.Text;

                            if (propertyName.ToLower() == "id" && propertyValue != null)
                                id = int.Parse(propertyValue);

                            if (!string.IsNullOrEmpty(propertyValue))
                            {
                                object convertedValue = Convert.ChangeType(propertyValue, property.PropertyType);
                                property.SetValue(newGpu, convertedValue);
                            }
                        }
                        else if (control is CheckBox checkBox)
                        {
                            bool propertyValue = checkBox.Checked;
                            property.SetValue(newGpu, propertyValue);
                        }
                    }
                }
            }

            return newGpu;
        }
        private void DisplayGpuSettings()
        {
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Name = GPUSETTINGSPANELNAME;
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.AutoScroll = true;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            if (_settings.Gpus != null)
            {
                Gpu gpu = GetSelectedGpu();
                PropertyInfo[] properties = typeof(Gpu).GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    // Skip default properties
                    if (property.Name.StartsWith("Default")) continue;

                    Label label = new Label();
                    label.Text = property.Name;
                    label.AutoSize = true;
                    label.Anchor = AnchorStyles.None; // Center the text horizontally
                    label.TextAlign = ContentAlignment.MiddleCenter; // Center the text vertically

                    Control inputControl; // This will be either a TextBox or CheckBox

                    if (property.PropertyType == typeof(bool))
                    {
                        // Create a CheckBox for boolean properties
                        CheckBox checkBox = new CheckBox();
                        checkBox.Checked = (bool)property.GetValue(gpu);
                        checkBox.CheckedChanged += (sender, e) =>
                        {
                            property.SetValue(gpu, checkBox.Checked);
                        };

                        inputControl = checkBox;
                    }
                    else
                    {
                        // Create a TextBox for non-boolean properties
                        TextBox textbox = new TextBox();
                        textbox.Text = property.GetValue(gpu)?.ToString();
                        textbox.KeyUp += (sender, e) =>
                        {
                            if (e.KeyCode == Keys.Enter)
                            {
                                SaveSettings();
                                UpdateGpusListBox(gpuListBox.SelectedIndex);
                            }
                        };

                        inputControl = textbox;
                    }


                    // Hide Id's
                    if (property.Name.ToLower().Equals("id"))
                    {
                        inputControl.Enabled = false;
                    }

                    tableLayoutPanel.Controls.Add(label, 0, tableLayoutPanel.RowCount);
                    tableLayoutPanel.Controls.Add(inputControl, 1, tableLayoutPanel.RowCount);

                    tableLayoutPanel.RowCount++;
                }

            }

            // Remove existing panel if there is one
            TableLayoutPanel existingPanel = gpuSettingsPanel.Controls.Find(GPUSETTINGSPANELNAME, true).FirstOrDefault() as TableLayoutPanel;
            if (existingPanel != null) { gpuSettingsPanel.Controls.Remove(existingPanel); }

            // Add panel
            gpuSettingsPanel.Controls.Add(tableLayoutPanel);
        }
        private void gpuListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gpuListBox.SelectedIndex == -1) return;

            // Adding new item
            if(gpuListBox.SelectedIndex == 0)
            {
                // Create and add new gpu
                if (_settings.Gpus == null)
                    _settings.Gpus = new List<Gpu> { new Gpu("New Gpu") };
                else
                    _settings.Gpus.Add(new Gpu("New Gpu"));

                UpdateGpusListBox(_settings.Gpus.Count);
            }

            DisplayGpuSettings();
        }

        private void minerSettingsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                minerSettingsPanel.Controls.Clear();
                
                if (minerSettingsListBox.SelectedIndex >= 1)
                {
                    MinerConfig minerSettings = GetSelectedMinerSettings();

                    // Remove from settings
                    for(int i = 0; i < _settings.MinerSettings.Count; i++)
                    {
                        if(_settings.MinerSettings[i].Id == minerSettings.Id) 
                            _settings.MinerSettings.RemoveAt(i);
                    }

                    // Remove the selected item from the ListBox
                    minerSettingsListBox.Items.RemoveAt(minerSettingsListBox.SelectedIndex);
                }
            }
        }

        private void gpuListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                gpuSettingsPanel.Controls.Clear();

                if (gpuListBox.SelectedIndex >= 1)
                {
                    Gpu gpu = GetSelectedGpu();

                    // Remove from settings
                    for (int i = 0; i < _settings.Gpus.Count; i++)
                    {
                        if (_settings.Gpus[i].Id == gpu.Id)
                            _settings.Gpus.RemoveAt(i);
                    }

                    // Remove the selected item from the ListBox
                    gpuListBox.Items.RemoveAt(gpuListBox.SelectedIndex);
                }
            }
        }
    }

    public class Settings
    {
        public List<MinerConfig> MinerSettings { get; set; }
        public List<Gpu> Gpus { get; set; }
        public Settings()
        {
        }
    }
    public class Gpu
    {
        Random random = new Random();
        public int Id { get; set; }
        public int Device_Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public int Core_Clock { get; set; }
        public int Mem_Clock_Offset { get; set; }
        public int Power_Limit { get; set; }
        public double Core_Mv { get; set; }
        public double Intensity { get; set; }
        public double Dual_Intensity { get; set; }
        public int Max_Core_Temp { get; set; }
        public int Max_Mem_Temp { get; set; }
        public int Fan_Percent { get; set; }


        public Gpu()
        {
            Id = random.Next(2303, 40598);
            Name = "";
            Enabled = false;
            Core_Clock = 1200;
            Mem_Clock_Offset = 1000;
            Power_Limit = -1;
            Core_Mv = -1;
            Intensity = -1;
            Dual_Intensity = -1;
            Max_Core_Temp = 85;
            Max_Mem_Temp = 115;
            Fan_Percent = 100;
        }
        public Gpu(string name)
        {
            Id = random.Next(2303, 40598);
            Name = name;
            Enabled = false;
            Core_Clock = 1200;
            Mem_Clock_Offset = 1000;
            Power_Limit = -1;
            Core_Mv = -1;
            Intensity = -1;
            Dual_Intensity = -1;
            Max_Core_Temp = 85;
            Max_Mem_Temp = 115;
            Fan_Percent = 100;
        }

        public bool isEquals(Gpu gpu)
        {
            if (gpu is null)
                return false;            
            
            if (gpu.Id == Id)
                return true;
            
            return false;
        }
    }

    public static class AppSettings
    {
        public static void Save<T>(string key, T value)
        {
            string serializedValue = JsonConvert.SerializeObject(value);
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Check if the key already exists in AppSettings
            KeyValueConfigurationElement key_config = config.AppSettings.Settings[key];

            // If the key is not found, add it to AppSettings
            if (key_config == null)
            {
                config.AppSettings.Settings.Add(key, serializedValue);
            }
            else
            {
                key_config.Value = serializedValue;
            }

            config.Save(ConfigurationSaveMode.Modified);
        }

        public static T Load<T>(string key)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Check if the key exists in AppSettings
            KeyValueConfigurationElement key_config = config.AppSettings.Settings[key];

            if (key_config == null)
            {
                return default(T);
            }
            else
            {
                string serializedValue = key_config.Value;
                var deserialized = JsonConvert.DeserializeObject<T>(serializedValue);
                return deserialized;
            }
        }
    }
}
