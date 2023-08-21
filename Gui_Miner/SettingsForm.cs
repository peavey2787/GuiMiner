using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Gui_Miner.Form1;
using static Gui_Miner.SettingsForm;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using Action = System.Action;
using ComboBox = System.Windows.Forms.ComboBox;
using Label = System.Windows.Forms.Label;
using Task = System.Threading.Tasks.Task;
using TextBox = System.Windows.Forms.TextBox;

namespace Gui_Miner
{
    public partial class SettingsForm : Form
    {
        #region Variables
        Settings _settings = new Settings();
        const string SETTINGSNAME = "Settings";
        const string MINERSETTINGSPANELNAME = "tableLayoutPanel";
        const string GPUSETTINGSPANELNAME = "gpuTableLayoutPanel";
        public const string AUTOSTARTMINING = "AutoStartMining";
        const string AUTOSTARTWITHWIN = "AutoStartWithWin";
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

            HideAllPanels();
            manageConfigPanel.Show();

            // Set form size
            Size = new Size(735, 700);

            // Position settings panels and resize
            int y = 55;
            Size panelSizes = new Size(720, 600);
            manageConfigPanel.Location = new Point(0, y);
            generalPanel.Location = new Point(0, y);
            walletsPanel.Location = new Point(0, y);
            poolsPanel.Location = new Point(0, y);
            manageConfigPanel.Size = panelSizes;
            generalPanel.Size = panelSizes;
            walletPanel.Size = panelSizes;
            poolsPanel.Size = panelSizes;

            // Load General settings
            autoStartMiningCheckBox.Checked = bool.TryParse(AppSettings.Load<string>(SettingsForm.AUTOSTARTMINING), out bool result) ? result : false;
            autoStartWithWinCheckBox.Checked = bool.TryParse(AppSettings.Load<string>(SettingsForm.AUTOSTARTWITHWIN), out bool winResult) ? winResult : false;

            // Tooltip text
            toolTip.SetToolTip(getAllGpusButton, "Add all available GPUs");
            toolTip.SetToolTip(addGpuSettingsButton, "Add all active GPU settings. Be sure to click generate .bat file after.");
            toolTip.SetToolTip(clearGpuSettingsButton, "Remove all GPU specific settings from the miner settings.");
            toolTip.SetToolTip(generateButton, "Use all miner settings above to generate the .bat file. Be sure to restart the miner to use the latest settings."); 
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
                (Type configType, Object configObject) = minerSetting.GetSelectedMinerConfig();
                PropertyInfo property = configType.GetProperty("Name");
                string name = (string)property.GetValue(configObject);

                minerSettingsListBox.Items.Add(name + " / " + minerSetting.Id);
            }
            minerSettingsListBox.SelectedIndex = selectedIndex;
        }
        private void UpdateGpusListBox(int selectedIndex = 1)
        {
            if (_settings.Gpus == null) return;

            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateGpusListBox(selectedIndex)));
                return;
            }

            gpuListBox.Items.Clear();
            gpuListBox.Items.Add("Add GPU");

            foreach (Gpu gpu in _settings.Gpus)
            {
                gpuListBox.Items.Add(gpu.Name + " / " + gpu.Id);
            }
            if(_settings.Gpus.Count > 0)
                gpuListBox.SelectedIndex = selectedIndex;
        }
        private void UpdateWalletsListBox(int selectedIndex = 1)
        {
            if (_settings.Wallets == null) return;

            walletsListBox.Items.Clear();
            walletsListBox.Items.Add("Add Wallet");

            foreach (Wallet wallet in _settings.Wallets)
            {
                walletsListBox.Items.Add(wallet.Name + " / " + wallet.Id);
            }

            if (_settings.Wallets.Count >= 1)
            {
                // Select last item if out of bounds
                if (selectedIndex >= walletsListBox.Items.Count)
                    walletsListBox.SelectedIndex = walletsListBox.Items.Count - 1;
                else
                    walletsListBox.SelectedIndex = selectedIndex;
            }
        }
        private void UpdatePoolsListBox(int selectedIndex = 1)
        {
            if (_settings.Pools == null) return;

            poolsListBox.Items.Clear();
            poolsListBox.Items.Add("Add Pool");

            foreach (Pool pool in _settings.Pools)
            {
                poolsListBox.Items.Add(pool.Name + " / " + pool.Id);
            }

            if (_settings.Pools.Count >= 1)
            {
                // Select last item if out of bounds
                if (selectedIndex >= poolsListBox.Items.Count)
                    poolsListBox.SelectedIndex = poolsListBox.Items.Count - 1;
                else
                    poolsListBox.SelectedIndex = selectedIndex;
            }
        }


        #region MinerConfigs
        // Miner Settings
        private MinerConfig GetSelectedMinerSettings()
        {
            if (InvokeRequired)
            {
                return (MinerConfig)Invoke(new Func<MinerConfig>(GetSelectedMinerSettings));
            }

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
            MinerConfig newMinerSetting = GetSelectedMinerSettings();
            if (newMinerSetting == null)
            {
                UpdateStatusLabel("Please select/create a miner config");
                return new MinerConfig();
            }

            TableLayoutPanel tableLayoutPanel = this.Controls.Find(MINERSETTINGSPANELNAME, true).FirstOrDefault() as TableLayoutPanel;
            int id = 0;

            if (tableLayoutPanel == null) return newMinerSetting;

            // CUSTOM SETTINGS
            newMinerSetting.batFileArguments = batLineTextBox.Text;

            // Set selected miner config
            var comboBox = tableLayoutPanel.Controls.Find("ChooseMinerComboBox", true).FirstOrDefault();
            MinerConfig.MinerConfigType matchedEnumValue = (MinerConfig.MinerConfigType)Enum.Parse(typeof(MinerConfig.MinerConfigType), comboBox.Text);
            newMinerSetting.CurrentMinerConfig = matchedEnumValue;

            (Type configType, Object configObject) = newMinerSetting.GetSelectedMinerConfig();

            string propertyName = "";
            foreach (Control control in tableLayoutPanel.Controls)
            {
                if (control is Label label)
                {
                    propertyName = label.Text;
                }

                PropertyInfo property = configType.GetProperty(propertyName);

                if (property != null)
                {
                    if (control is TextBox textbox)
                    {
                        string propertyValue = textbox.Text;

                        if (propertyName.ToLower() == "id" && propertyValue != null)
                            id = int.Parse(propertyValue);

                        object convertedValue = Convert.ChangeType(propertyValue, property.PropertyType);
                        property.SetValue(configObject, convertedValue);

                    }
                    else if (control is CheckBox checkBox)
                    {
                        bool propertyValue = checkBox.Checked;
                        property.SetValue(configObject, propertyValue);
                    }
                    else if (control is ComboBox walletComboBox)
                    {
                        int propertyValue = int.Parse(walletComboBox.Text.Split('/')[1].Trim());
                        
                        // Get wallet
                        var wallet = _settings.Wallets.Find(w => w.Id.Equals(propertyValue));
                        
                        if(wallet == null)
                        {
                            // Get Pool
                            var pool = _settings.Pools.Find(p => p.Id.Equals(propertyValue));
                            property.SetValue(configObject, pool.Address);
                        }
                        else
                            property.SetValue(configObject, wallet.Address);
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

                // Get miner config
                (Type configType, Object configObject) = minerSetting.GetSelectedMinerConfig();
                PropertyInfo[] properties = configType.GetProperties();

                // CUSTOM SETTINGS
                // Set .bat file textbox
                batLineTextBox.Text = minerSetting.batFileArguments;

                // Choose Miner Label
                Label minerLabel = new Label();
                minerLabel.Text = "Choose Miner:";
                minerLabel.Anchor = AnchorStyles.None;
                minerLabel.TextAlign = ContentAlignment.MiddleCenter;                

                // Create a dropdown menu
                ComboBox comboBox = new ComboBox();
                comboBox.Name = "ChooseMinerComboBox";
                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox.ForeColor = Color.White;
                comboBox.BackColor = Color.FromArgb(12, 20, 52);

                foreach (MinerConfig.MinerConfigType value in Enum.GetValues(typeof(MinerConfig.MinerConfigType)))
                    comboBox.Items.Add(value);

                comboBox.Text = minerSetting.CurrentMinerConfig.ToString();
                
                // Add event handler for when an item is selected
                comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
                comboBox.Anchor = AnchorStyles.None;              
                tableLayoutPanel.Controls.Add(comboBox, 0, tableLayoutPanel.RowCount);
                tableLayoutPanel.Controls.Add(minerLabel, 0, tableLayoutPanel.RowCount);
                tableLayoutPanel.RowCount++;

                int port = 0;
                foreach (PropertyInfo property in properties)
                {
                    // Skip default properties
                    if (property.Name.StartsWith("Default") 
                        || property.Name.Equals("batFileArguments")) continue;

                    Label label = new Label();
                    label.Text = property.Name;
                    label.AutoSize = true;
                    label.Anchor = AnchorStyles.None;
                    label.TextAlign = ContentAlignment.MiddleCenter; 

                    Control inputControl; // This will be either a TextBox or CheckBox

                    if (property.PropertyType == typeof(bool))
                    {
                        // Create a CheckBox for boolean properties
                        CheckBox checkBox = new CheckBox();
                        checkBox.BackColor = Color.FromArgb(12, 20, 52);
                        checkBox.Checked = (bool)property.GetValue(configObject);
                        checkBox.CheckedChanged += (sender, e) =>
                        {
                            property.SetValue(configObject, checkBox.Checked);
                        };

                        inputControl = checkBox;
                    }
                    else if(property.Name.Contains("user") || property.Name.Contains("wallet"))
                    {
                        ComboBox walletComboBox = new ComboBox();
                        walletComboBox.BackColor = Color.FromArgb(12, 20, 52);
                        walletComboBox.ForeColor = Color.White;
                        walletComboBox.Name = property.Name;                        
                        var propertyValue = (string)property.GetValue(configObject);

                        int i = 0;
                        int selectedIndex = 0;
                        foreach (Wallet wallet in _settings.Wallets)
                        {
                            walletComboBox.Items.Add(wallet.Name + " - " + wallet.Coin + " / " + wallet.Id);
                            if (wallet.Address == propertyValue)
                                selectedIndex = i;
                            i++;
                        }

                        walletComboBox.SelectedIndex = selectedIndex;

                        walletComboBox.SelectedIndexChanged += (sender, e) =>
                        {
                            SaveSettings();
                        };

                        inputControl = walletComboBox;
                    }
                    else if(property.Name.Contains("server") || property.Name.Contains("pool"))
                    {
                        ComboBox poolComboBox = new ComboBox();
                        poolComboBox.BackColor = Color.FromArgb(12, 20, 52);
                        poolComboBox.ForeColor = Color.White;
                        poolComboBox.Name = property.Name;
                        var propertyValue = (string)property.GetValue(configObject);

                        int i = 0;
                        int selectedIndex = 0;

                        foreach (Pool pool in _settings.Pools)
                        {
                            poolComboBox.Items.Add(pool.Name + " - SSL: " + pool.Ssl + " / " + pool.Id);
                            if (pool.Address == propertyValue)
                            {
                                selectedIndex = i;
                                port = pool.Port;
                            }
                            i++;
                        }

                        poolComboBox.SelectedIndex = selectedIndex;

                        poolComboBox.SelectedIndexChanged += (sender, e) =>
                        {
                            SaveSettings();
                            var currentComboBox = (ComboBox)sender;                            

                            // Get the selected pool
                            var parts = currentComboBox.Text.Split('/');
                            int id = int.Parse(parts[1].Trim());
                            Pool selectedPool = _settings.Pools.Find(p => p.Id.Equals(id));                            

                            // Find the next TextBox control
                            Control nextControl = this.GetNextControl(currentComboBox, true);

                            // Loop until we find a TextBox control
                            while (nextControl != null && !(nextControl is TextBox))
                            {
                                nextControl = this.GetNextControl(nextControl, true);
                            }

                            // Set the next textbox to the port number
                            nextControl.Text = selectedPool.Port.ToString();
                        };

                        inputControl = poolComboBox;
                    }
                    else
                    {
                        // Create a TextBox for non-boolean properties
                        TextBox textbox = new TextBox();
                        textbox.BackColor = Color.FromArgb(12, 20, 52);
                        textbox.ForeColor = Color.White;
                        textbox.Text = property.GetValue(configObject)?.ToString();
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

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            var selectedMinerSettings = GetSelectedMinerSettings();
            MinerConfig.MinerConfigType matchedEnumValue = (MinerConfig.MinerConfigType)Enum.Parse(typeof(MinerConfig.MinerConfigType), comboBox.Text);
            selectedMinerSettings.CurrentMinerConfig = matchedEnumValue;
            DisplayMinerSettings();
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
                    _settings.MinerSettings.Add(new MinerConfig());

                UpdateMinerSettingsListBox(_settings.MinerSettings.Count);
            }

            DisplayMinerSettings();
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
                    for (int i = 0; i < _settings.MinerSettings.Count; i++)
                    {
                        if (_settings.MinerSettings[i].Id == minerSettings.Id)
                            _settings.MinerSettings.RemoveAt(i);
                    }

                    // Remove the selected item from the ListBox
                    minerSettingsListBox.Items.RemoveAt(minerSettingsListBox.SelectedIndex);
                }
            }
        }
        #endregion


        #region GPUs
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

            string propertyName = "";
            foreach (Control control in tableLayoutPanel.Controls)
            {
                if (control is Label label)
                {
                    propertyName = label.Text;
                }

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
                    if (property.Name.Equals("Id")) continue;

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
                        textbox.BackColor = Color.FromArgb(12, 20, 52);
                        textbox.ForeColor = Color.White;
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

            UpdateStatusLabel("Edit Gpu Then Click Add GPUs");
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

        // Get list of devices
        private void getAllGpusButton_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var devices = GetDevicesFromMiner();
                
                foreach (var device in devices)
                {
                    // Create new gpu
                    Gpu newGpu = new Gpu();
                    var parts = device.Split(' ');
                    string vram = parts[parts.Length - 3];
                    string name = parts[parts.Length - 4];
                    newGpu.Name = $"{name} {vram}";
                    var deviceId = parts[0].Substring(3);
                    deviceId = deviceId.Replace(':', ' ').Trim();
                    newGpu.Device_Id = int.Parse(deviceId);

                    // Check if a GPU with the same Device_ID and Name already exists
                    if (_settings.Gpus == null)
                        _settings.Gpus = new List<Gpu> { newGpu };
                    else
                    {
                        Gpu existingGpu = _settings.Gpus.FirstOrDefault(gpu => gpu.Device_Id == newGpu.Device_Id && gpu.Name == newGpu.Name);

                        if (existingGpu == null)
                        {
                            // If no matching GPU was found, add the new GPU
                            if (_settings.Gpus == null)
                                _settings.Gpus = new List<Gpu> { newGpu };
                            else
                                _settings.Gpus.Add(newGpu);
                        }
                    }

                    UpdateGpusListBox(_settings.Gpus.Count);

                    UpdateStatusLabel("Edit Gpus Then Click Add GPU Settings");
                }
            });

        }
        private List<string> GetDevicesFromMiner()
        {
            var minerSettings = GetSelectedMinerSettings();

            if(minerSettings == null)
            {
                UpdateStatusLabel("Please select/create a miner config.");
                return new List<string>();
            }

            (Type configType, Object configObject) = minerSettings.GetSelectedMinerConfig();
            PropertyInfo minerFilePathProperty = configType.GetProperty("MinerFilePath");
            string minerFilePath = (string)minerFilePathProperty.GetValue(configObject);

            FieldInfo listDevicesCommandField = configType.GetField("ListDevicesCommand", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            string listDevicesCommand = (string)listDevicesCommandField.GetValue(configObject);

            // Create a new process
            Process process = new Process();

            string path = minerFilePath;
            if (!File.Exists(minerFilePath))
                path = Directory.GetCurrentDirectory() + "\\" + minerFilePath;

            if (!File.Exists(path) && File.Exists("miner.exe"))
                path = "miner.exe";

            // Set the process start information
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = listDevicesCommand,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;

            // Subscribe to the OutputDataReceived event
            List<string> devices = new List<string>();
            process.OutputDataReceived += (senderr, ee) =>
            {
                if (!string.IsNullOrEmpty(ee.Data))
                {
                    // This event is called whenever there is data available in the standard output
                    this.Invoke((MethodInvoker)delegate
                    {
                        devices.Add(ee.Data);
                    });
                }
            };

            // Start the process
            try 
            { 
                process.Start();

                // Begin asynchronously reading the output
                process.BeginOutputReadLine();
                process.WaitForExit();
                process.Close();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Access is denied") && File.Exists(path))
                    UpdateStatusLabel("Please restart the app as administrator");
                else if (!File.Exists(path))
                    UpdateStatusLabel("Please add miner.exe to folder or set path");
            }

            return devices;
        }

        // Add/Clear Gpu Settings Buttons
        private void addGpuSettingsButton_Click(object sender, EventArgs e)
        {
            var minerSettings = GetSelectedMinerSettings();
            (Type configType, Object configObject) = minerSettings.GetSelectedMinerConfig();
            MethodInfo methodInfo = configType.GetMethod("AddGpuSettings");
            methodInfo.Invoke(configObject, new object[] { _settings.Gpus });
            DisplayMinerSettings();

            UpdateStatusLabel();
        }
        private void clearGpuSettingsButton_Click(object sender, EventArgs e)
        {
            var minerSettings = GetSelectedMinerSettings();
            (Type configType, Object configObject) = minerSettings.GetSelectedMinerConfig();
            MethodInfo methodInfo = configType.GetMethod("ClearGpuSettings");
            methodInfo.Invoke(configObject, null);
            DisplayMinerSettings();

            UpdateStatusLabel();
        }
        #endregion


        // Generate bat file arguments
        private void generateButton_Click(object sender, EventArgs e)
        {
            var minerSetting = GetSelectedMinerSettings();
            batLineTextBox.Text = minerSetting.GeneratebatFileArguments();

            UpdateStatusLabel("");

            SaveSettings();
        }       


        private void UpdateStatusLabel(string message = "Click Generate to Update")
        {
            if (statusLabel.InvokeRequired)
            {
                // Use a delegate to update the UI thread
                this.Invoke(new Action(() => UpdateStatusLabel(message)));
                return;
            }

            statusLabel.Text = message;
        }

        
        // Navigation buttons
        private void generalButton_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            generalPanel.Show();
            generalPanel.BringToFront();
        }
        private void manageMinerConfigsButton_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            manageConfigPanel.Show();
            manageConfigPanel.BringToFront();
        }
        private void manageWalletsButton_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            walletsPanel.Show();
            walletsPanel.BringToFront();

            // Load saved wallets
            UpdateWalletsListBox();
        }
        private void managePoolsButton_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            poolsPanel.Show();
            poolsPanel.BringToFront();

            UpdatePoolsListBox();
        }
        private void HideAllPanels()
        {
            manageConfigPanel.Hide();
            generalPanel.Hide();
            walletsPanel.Hide();
            poolsPanel.Hide();
        }


        // General Settings
        private void autoStartMiningCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            AppSettings.Save<string>(AUTOSTARTMINING, autoStartMiningCheckBox.Checked.ToString());
        }
        private void autoStartWithWinCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            AppSettings.Save<string>(AUTOSTARTWITHWIN, autoStartWithWinCheckBox.Checked.ToString());

            if (autoStartWithWinCheckBox.Checked)
            {
                if (IsRunningAsAdmin())
                {
                    string assemblyPath = Assembly.GetEntryAssembly().Location;
                    if (CreateSchedulerTask("GuiMiner", assemblyPath))
                        successLabel.Text = "Successfully added auto start task";
                    else
                        successLabel.Text = "Unable to add auto start task";
                }
                else
                    successLabel.Text = "Please restart the app as admin in order to start with Windows";
            }
            else
            {
                DeleteSchedulerTask("GuiMiner");
            }
        }


        // Create/Delete Scheduler Task
        public static bool CreateSchedulerTask(string taskName, string applicationPath)
        {
            try
            {
                using (TaskService taskService = new TaskService())
                {
                    TaskDefinition taskDefinition = taskService.NewTask();

                    // Set the task properties
                    taskDefinition.RegistrationInfo.Description = "Auto Start Gui Miner on Windows Startup";
                    taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;  // Run with highest privileges

                    // Set the task settings
                    taskDefinition.Settings.ExecutionTimeLimit = TimeSpan.Zero; // Do not stop the task if it runs longer than 3 days
                    taskDefinition.Settings.StartWhenAvailable = true; // Run the task as soon as possible after a scheduled start is missed

                    // Create a trigger to run the task on system startup
                    LogonTrigger trigger = new LogonTrigger();
                    taskDefinition.Triggers.Add(trigger);

                    // Create an action to start the application
                    string actionPath = applicationPath;
                    string actionArguments = "";  // You can specify additional arguments here if needed
                    taskDefinition.Actions.Add(new ExecAction(actionPath, actionArguments, Path.GetDirectoryName(applicationPath)));

                    // Register the new task
                    taskService.RootFolder.RegisterTaskDefinition(taskName, taskDefinition, TaskCreation.CreateOrUpdate, null, null, TaskLogonType.None, null);
                }

                return true;
            }
            catch (Exception ex)
            {
                if(IsRunningAsAdmin())
                    MessageBox.Show($"Failed to add auto start task to windows task scheduler: {ex.Message}");
                
                return false;
            }
        }
        public static bool DeleteSchedulerTask(string taskName)
        {
            try
            {
                using (TaskService taskService = new TaskService())
                {
                    // Delete the task if it exists
                    if (taskService.GetTask(taskName) != null)
                    {
                        taskService.RootFolder.DeleteTask(taskName, false);
                        return true;
                    }
                    else
                    {
                        // The task with the specified name does not exist
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete task from Windows Task Scheduler: {ex.Message}");
                return false;
            }
        }


        #region Wallets
        // Manage Wallets
        private void walletsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (walletsListBox.SelectedIndex == -1) return;

            // Adding new item
            if (walletsListBox.SelectedIndex == 0)
            {
                // Create and add new miner setting
                Wallet newWallet = new Wallet();
                newWallet.Name = "New Wallet";

                if (_settings.Wallets == null)
                    _settings.Wallets = new List<Wallet> { newWallet };
                else
                    _settings.Wallets.Add(newWallet);

                UpdateWalletsListBox(_settings.Wallets.Count);
            }

            DisplayWalletSettings();
        }
        private void DisplayWalletSettings()
        {
            Wallet selectedWallet = GetSelectedWallet();
            if(selectedWallet == null) return;

            walletNameTextBox.Text = selectedWallet.Name;
            walletAddressTextBox.Text = selectedWallet.Address;
            walletCoinTextBox.Text = selectedWallet.Coin;
        }
        private Wallet GetWalletFromUI()
        {
            Wallet wallet = new Wallet();
            wallet.Name = walletNameTextBox.Text;
            wallet.Address = walletAddressTextBox.Text;
            wallet.Coin = walletCoinTextBox.Text;

            return wallet;
        }
        private Wallet GetSelectedWallet()
        {
            if(walletsListBox.SelectedIndex <= 0) return null;

            int id = int.Parse(walletsListBox.Text.Split('/')[1]);

            Wallet existingWallet = _settings.Wallets.Find(w => w.Id.Equals(id));
            
            return existingWallet;            
        }
        private void SaveWallets()
        {
            var updatedWallet = GetWalletFromUI();
            var savedWallet = GetSelectedWallet();
            savedWallet.Name = updatedWallet.Name;
            savedWallet.Address = updatedWallet.Address;
            savedWallet.Coin = updatedWallet.Coin;

            AppSettings.Save<Settings>(SETTINGSNAME, _settings);

            UpdateWalletsListBox(walletsListBox.SelectedIndex);
            DisplayMinerSettings();
        }
        private void walletNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveWallets();
            }
        }
        private void walletAddressTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveWallets();
            }
        }
        private void walletCoinTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveWallets();
            }
        }
        private void walletsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var selectedWallet = GetSelectedWallet();

                // Display a confirmation dialog
                DialogResult result = MessageBox.Show($"Are you sure you want to delete the wallet named {selectedWallet.Name}?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // User clicked "Yes"
                    _settings.Wallets.Remove(selectedWallet);
                    AppSettings.Save<Settings>(SETTINGSNAME, _settings);

                    UpdateWalletsListBox(walletsListBox.Items.Count);
                    DisplayMinerSettings();
                }
            }
        }
        #endregion

        #region Pools
        // Manage Pools
        private void poolsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (poolsListBox.SelectedIndex == -1) return;

            // Adding new item
            if (poolsListBox.SelectedIndex == 0)
            {
                // Create and add new miner setting
                Pool pool = new Pool();
                pool.Name = "New Pool";

                if (_settings.Pools == null)
                    _settings.Pools = new List<Pool> { pool };
                else
                    _settings.Pools.Add(pool);

                UpdatePoolsListBox(_settings.Pools.Count);
            }

            DisplayPoolSettings();
        }
        private void DisplayPoolSettings()
        {
            Pool selectedPool = GetSelectedPool();
            if (selectedPool == null) return;

            poolNameTextBox.Text = selectedPool.Name;
            poolAddressTextBox.Text = selectedPool.Address;
            poolPortTextBox.Text = selectedPool.Port.ToString();
            poolLinkTextBox.Text = selectedPool.Link;
            poolSsslCheckBox.Checked = selectedPool.Ssl;
        }
        private Pool GetPoolFromUI()
        {
            Pool pool = new Pool();

            pool.Name = poolNameTextBox.Text;
            pool.Address = poolAddressTextBox.Text;
            pool.Port = int.TryParse(poolPortTextBox.Text, out int port) ? port : -1;
            pool.Link = poolLinkTextBox.Text;
            pool.Ssl = poolSsslCheckBox.Checked;

            return pool;
        }
        private Pool GetSelectedPool()
        {
            if (poolsListBox.SelectedIndex <= 0) return null;

            int id = int.Parse(poolsListBox.Text.Split('/')[1]);

            Pool pool = _settings.Pools.Find(w => w.Id.Equals(id));

            return pool;
        }
        private void SavePools()
        {
            var updatedPool = GetPoolFromUI();
            var savedPool = GetSelectedPool();
            savedPool.Name = updatedPool.Name;
            savedPool.Address = updatedPool.Address;
            savedPool.Port = updatedPool.Port;
            savedPool.Ssl = updatedPool.Ssl;
            savedPool.Link = updatedPool.Link;

            AppSettings.Save<Settings>(SETTINGSNAME, _settings);

            UpdatePoolsListBox(poolsListBox.SelectedIndex);
            DisplayMinerSettings();
        }
        private void poolNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SavePools();
            }
        }
        private void poolAddressTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SavePools();
            }
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SavePools();
            }
        }
        private void poolLinkTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SavePools();
            }
        }
        private void poolSsslCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SavePools();
        }
        private void poolsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var selectedPool = GetSelectedPool();

                // Display a confirmation dialog
                DialogResult result = MessageBox.Show($"Are you sure you want to delete the pool named {selectedPool.Name}?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // User clicked "Yes"
                    _settings.Pools.Remove(selectedPool);
                    AppSettings.Save<Settings>(SETTINGSNAME, _settings);

                    UpdatePoolsListBox(poolsListBox.Items.Count);
                    DisplayMinerSettings();
                }
            }
        }
        #endregion

    }

    #region Class Structure
    public class Settings
    {
        public List<MinerConfig> MinerSettings { get; set; }
        public List<Gpu> Gpus { get; set; }
        public List<Wallet> Wallets { get; set; }
        public List<Pool> Pools { get; set; }
        public Settings()
        {

        }
    }
    public class MinerConfig
    {
        public enum MinerConfigType
        {
            Unknown,
            Gminer,
            Trm
        }

        Random random = new Random();
        public int Id { get; set; }
        public MinerConfigType CurrentMinerConfig { get; set; }
        public UnknownConfig UnknownConfig { get; set; }
        public GminerConfig GminerConfig { get; set; }
        public TrmConfig TrmConfig { get; set; }
        public string batFileArguments { get; set; }

        public MinerConfig()
        {
            Id = random.Next(2303, 40598);
            CurrentMinerConfig = MinerConfigType.Unknown;
            UnknownConfig = new UnknownConfig();
            GminerConfig = new GminerConfig();
            TrmConfig = new TrmConfig();
            batFileArguments = "";
        }

        internal (Type, object) GetSelectedMinerConfig()
        {
            Type configType;
            object configObject;

            if (CurrentMinerConfig == MinerConfigType.Gminer)
            {
                configType = typeof(GminerConfig);
                configObject = GminerConfig;
            }
            else if (CurrentMinerConfig == MinerConfigType.Trm)
            {
                configType = typeof(TrmConfig);
                configObject = TrmConfig;
            }
            else
            {
                configType = typeof(UnknownConfig);
                configObject = UnknownConfig;
            }

            return (configType, configObject);
        }

        internal string GeneratebatFileArguments()
        {
            (Type configType, object configObject) = GetSelectedMinerConfig();
            PropertyInfo[] properties = configType.GetProperties();

            StringBuilder configString = new StringBuilder();

            string commandPrefix = (string)configType.GetProperty("CommandPrefix")?.GetValue(configObject);
            string commandSeparator = (string)configType.GetProperty("CommandSeparator")?.GetValue(configObject);

            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name;
                object propertyValue = property.GetValue(configObject);
                
                if (propertyValue == null || String.IsNullOrWhiteSpace(propertyValue.ToString())) continue; // skip empty properties
                if (propertyName.StartsWith("Command")) continue; // skip 
                if (propertyName.Equals("Name")) continue; // skip 
                if (propertyName.Equals("Active")) continue; // skip 
                if (propertyName.Equals("runAsAdmin")) continue; // skip 
                if (propertyName.Equals("pl") && propertyValue.ToString().Trim().Equals("-1")) continue; // skip 
                

                if (propertyName.Equals("MinerFilePath"))
                {
                    configString.Append($"\"{propertyValue}\" ");
                }
                // Special gminer
                else if(propertyName.Equals("nvml"))
                {
                    propertyValue.ToString().Replace(" ", commandSeparator);
                    if (propertyValue.ToString().ToLower() == "true")
                        propertyValue = "1";

                    configString.Append($"{commandPrefix}{propertyName} {propertyValue} ");
                }
                else
                {
                    propertyValue.ToString().Replace(" ", commandSeparator);
                    configString.Append($"{commandPrefix}{propertyName} {propertyValue} ");
                }
            }

            return configString.ToString().Trim();
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
        public double Mem_Mv { get; set; } 
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
    public class Wallet
    {
        Random random = new Random();
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Coin { get; set; }
        public Wallet() 
        {
            Id = random.Next(2303, 40598);
        }
    }
    public class Pool
    {
        Random random = new Random();
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Link { get; set; }
        public bool Ssl { get; set; }
        public Pool()
        {
            Id = random.Next(2303, 40598);
        }
    }
    #endregion

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
