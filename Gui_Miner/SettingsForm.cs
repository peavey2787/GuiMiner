﻿using Gui_Miner.Classes;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Gui_Miner.Form1;
using static Gui_Miner.MinerConfig;
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
        public Form1 MainForm { get; set; }
        internal RotatingPanel rotatingPanel;
        const string SETTINGSNAME = "Settings";
        const string MINERSETTINGSPANELNAME = "tableLayoutPanel";
        const string GPUSETTINGSPANELNAME = "gpuTableLayoutPanel";
        public const string AUTOSTARTMINING = "AutoStartMining";
        const string AUTOSTARTWITHWIN = "AutoStartWithWin";
        public const string STOPSHORTKEYS = "StopShortKeys";
        public const string STARTSHORTKEYS = "StartShortKeys";
        public const string BGIMAGE = "BackgroundImage";
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
            Size = new Size(735, 725);

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
            LoadGeneralSettings();


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

            MainForm.UpdateShortcutKeys();
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
        private void LoadGeneralSettings()
        {
            autoStartMiningCheckBox.Checked = bool.TryParse(AppSettings.Load<string>(AUTOSTARTMINING), out bool result) ? result : false;
            autoStartWithWinCheckBox.Checked = bool.TryParse(AppSettings.Load<string>(AUTOSTARTWITHWIN), out bool winResult) ? winResult : false;
            bgComboBox.Text = AppSettings.Load<string>(BGIMAGE);

            var keys = AppSettings.Load<List<Keys>>(STOPSHORTKEYS);
            if (keys != null)
            {
                foreach (Keys key in keys)
                    stopShortKeysTextBox.Text += key.ToString() + " + ";

                // Remove the trailing " + "
                if (stopShortKeysTextBox.Text.EndsWith(" + "))
                    stopShortKeysTextBox.Text = stopShortKeysTextBox.Text.Substring(0, stopShortKeysTextBox.Text.Length - 3);
            }

            keys = AppSettings.Load<List<Keys>>(STARTSHORTKEYS);
            if (keys != null)
            {
                foreach (Keys key in keys)
                    startShortKeysTextBox.Text += key.ToString() + " + ";

                // Remove the trailing " + "
                if (startShortKeysTextBox.Text.EndsWith(" + "))
                    startShortKeysTextBox.Text = startShortKeysTextBox.Text.Substring(0, startShortKeysTextBox.Text.Length - 3);
            }
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
            if (_settings.Gpus.Count > 0)
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
            newMinerSetting.Bat_File_Arguments = batLineTextBox.Text;

            // Set selected miner config
            var minerComboBox = tableLayoutPanel.Controls.Find("ChooseMinerComboBox", true).FirstOrDefault();
            MinerConfig.MinerConfigType matchedEnumValue = (MinerConfig.MinerConfigType)Enum.Parse(typeof(MinerConfig.MinerConfigType), minerComboBox.Text);
            newMinerSetting.Current_Miner_Config_Type = matchedEnumValue;

            string propertyName = "";
            foreach (Control control in tableLayoutPanel.Controls)
            {
                if (control is Label label)
                {
                    propertyName = label.Text;
                }

                PropertyInfo property = newMinerSetting.GetType().GetProperty(propertyName);

                if (property != null)
                {
                    if (control is TextBox textbox)
                    {
                        string propertyValue = textbox.Text;

                        if (propertyName.ToLower() == "id" && propertyValue != null)
                            id = int.Parse(propertyValue);

                        // Check if the property is of type List<int>
                        if (property.PropertyType == typeof(List<int>))
                        {
                            var newValue = newMinerSetting.ConvertStrToIntList(propertyValue);
                            //object convertedValue = Convert.ChangeType(propertyValue, property.PropertyType);
                            property.SetValue(newMinerSetting, newValue);
                        }
                        // Check if the property is of type List<float>
                        else if (property.PropertyType == typeof(List<float>))
                        {
                            // It's a List<float>
                            var newValue = newMinerSetting.ConvertStrToFloatList(propertyValue);
                            //object convertedValue = Convert.ChangeType(propertyValue, property.PropertyType);
                            property.SetValue(newMinerSetting, newValue);
                        }
                        else
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
                    else if (control is ComboBox comboBox)
                    {
                        // Get Wallet/Pool
                        if (comboBox.Text.Contains('/'))
                        {
                            int propertyValue = int.Parse(comboBox.Text.Split('/')[1].Trim());

                            // Get wallet
                            var wallet = _settings.Wallets.Find(w => w.Id.Equals(propertyValue));

                            if (wallet == null)
                            {
                                // Get Pool
                                var pool = _settings.Pools.Find(p => p.Id.Equals(propertyValue));
                                property.SetValue(newMinerSetting, pool.Address);
                            }
                            else
                                property.SetValue(newMinerSetting, wallet.Address);
                        }
                        else
                        {
                            // Get Algo
                            property.SetValue(newMinerSetting, comboBox.Text);
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

                // Get miner config
                PropertyInfo[] properties = minerSetting.GetType().GetProperties();

                // CUSTOM SETTINGS
                // Set .bat file textbox
                batLineTextBox.Text = minerSetting.Bat_File_Arguments;

                // Choose Miner Label
                Label minerLabel = new Label();
                minerLabel.Text = "Choose Miner:";
                minerLabel.Anchor = AnchorStyles.None;
                minerLabel.TextAlign = ContentAlignment.MiddleCenter;

                // Create a dropdown menu
                ComboBox minerComboBox = new ComboBox();
                minerComboBox.Name = "ChooseMinerComboBox";
                minerComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                minerComboBox.ForeColor = Color.White;
                minerComboBox.BackColor = Color.FromArgb(12, 20, 52);

                foreach (MinerConfig.MinerConfigType value in Enum.GetValues(typeof(MinerConfig.MinerConfigType)))
                    minerComboBox.Items.Add(value);

                minerComboBox.Text = minerSetting.Current_Miner_Config_Type.ToString();

                // Add event handler for when an item is selected
                minerComboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
                minerComboBox.Anchor = AnchorStyles.None;
                tableLayoutPanel.Controls.Add(minerComboBox, 0, tableLayoutPanel.RowCount);
                tableLayoutPanel.Controls.Add(minerLabel, 0, tableLayoutPanel.RowCount);
                tableLayoutPanel.RowCount++;

                int port = 0;
                foreach (PropertyInfo property in properties)
                {
                    // Skip default properties
                    if (property.Name.StartsWith("Default")
                        || property.Name.Equals("Id")
                        || property.Name.Equals("Current_Miner_Config_Type")
                        || property.Name.Equals("Bat_File_Arguments")) continue;                    

                    Label nameLabel = new Label();
                    nameLabel.Text = property.Name;
                    nameLabel.AutoSize = true;
                    nameLabel.Anchor = AnchorStyles.None;
                    nameLabel.TextAlign = ContentAlignment.MiddleCenter;

                    Control inputControl; // This will be either a TextBox/CheckBox/Label

                    if (property.PropertyType == typeof(bool))
                    {
                        // Create a CheckBox for boolean properties
                        CheckBox checkBox = new CheckBox();
                        checkBox.BackColor = Color.FromArgb(12, 20, 52);

                        if (property.Name == "Active")
                            checkBox.Name = "activeCheckBox";
                        else
                            checkBox.Name = property.Name;

                        // Use saved value
                        checkBox.Checked = (bool)property.GetValue(minerSetting);                        

                        checkBox.CheckedChanged += (sender, e) =>
                        {
                            property.SetValue(minerSetting, checkBox.Checked);
                        };

                        inputControl = checkBox;
                    }
                    else if (property.Name.StartsWith("Wallet"))
                    {
                        ComboBox walletComboBox = new ComboBox();
                        walletComboBox.BackColor = Color.FromArgb(12, 20, 52);
                        walletComboBox.ForeColor = Color.White;
                        walletComboBox.Name = property.Name;
                        var propertyValue = (string)property.GetValue(minerSetting);

                        int i = 0;
                        int selectedIndex = 0;
                        if (_settings.Wallets != null)
                        {
                            foreach (Wallet wallet in _settings.Wallets)
                            {
                                walletComboBox.Items.Add(wallet.Name + " - " + wallet.Coin + " / " + wallet.Id);
                                if (wallet.Address == propertyValue)
                                    selectedIndex = i;
                                i++;
                            }

                            walletComboBox.SelectedIndex = selectedIndex;
                        }

                        walletComboBox.SelectedIndexChanged += (sender, e) =>
                        {
                            SaveSettings();
                        };

                        inputControl = walletComboBox;
                    }
                    else if (property.Name.StartsWith("Pool"))
                    {
                        ComboBox poolComboBox = new ComboBox();
                        poolComboBox.BackColor = Color.FromArgb(12, 20, 52);
                        poolComboBox.ForeColor = Color.White;
                        poolComboBox.Name = property.Name;
                        string propertyValue = "";
                        if (property.GetValue(minerSetting) != null)
                        {
                            propertyValue = property.GetValue(minerSetting).ToString();
                        }

                        int i = 0;
                        int selectedIndex = 0;
                        if (_settings.Pools != null)
                        {
                            foreach (Pool pool in _settings.Pools)
                            {
                                poolComboBox.Items.Add(pool.Name + " - SSL: " + pool.SSL + " / " + pool.Id);
                                if (pool.Address == propertyValue)
                                {
                                    selectedIndex = i;
                                    port = pool.Port;
                                }
                                i++;
                            }

                            poolComboBox.SelectedIndex = selectedIndex;
                        }

                        poolComboBox.SelectedIndexChanged += (sender, e) =>
                        {
                            SaveSettings();
                            var currentComboBox = (ComboBox)sender;

                            // Get the selected pool
                            var parts = currentComboBox.Text.Split('/');
                            int id = int.Parse(parts[1].Trim());
                            Pool selectedPool = _settings.Pools.Find(p => p.Id.Equals(id));

                            // Extract the single-digit number from end of name
                            int prevNumber = int.Parse(property.Name.Substring(property.Name.Length - 1));

                            // Change Port
                            Control matchingTextBox = minerSettingsPanel.Controls.Find($"Port{prevNumber}", true).First();
                            if (matchingTextBox != null && matchingTextBox is TextBox)
                                matchingTextBox.Text = selectedPool.Port.ToString();

                            // Change SSL
                            CheckBox matchingCheckBox = minerSettingsPanel.Controls.Find($"SSL{prevNumber}", true).OfType<CheckBox>().FirstOrDefault();
                            if (matchingCheckBox != null)
                                matchingCheckBox.Checked = selectedPool.SSL;

                        };

                        inputControl = poolComboBox;
                    }
                    else if (property.Name.StartsWith("Algo"))
                    {
                        ComboBox algoComboBox = new ComboBox();
                        algoComboBox.BackColor = Color.FromArgb(12, 20, 52);
                        algoComboBox.ForeColor = Color.White;
                        algoComboBox.Name = property.Name;
                        string propertyValue = "";
                        if (property.GetValue(minerSetting) != null)
                        {
                            propertyValue = property.GetValue(minerSetting).ToString();
                        }

                        int i = 0;
                        int selectedIndex = 0;
                        var algos = minerSetting.GetAlgos();
                        if (algos != null && algos.Count > 0)
                        {
                            foreach (string algo in minerSetting.Algos)
                            {
                                algoComboBox.Items.Add(algo);
                            }
                        }

                        string selectedAlgo = "";
                        if(property.Name == "Algo1")
                            selectedAlgo = minerSetting.Algo1;
                        if (property.Name == "Algo2")
                            selectedAlgo = minerSetting.Algo2;
                        if (property.Name == "Algo3")
                            selectedAlgo = minerSetting.Algo3;

                        algoComboBox.Text = selectedAlgo;

                        algoComboBox.SelectedIndexChanged += (sender, e) =>
                        {
                            SaveSettings();
                        };

                        inputControl = algoComboBox;
                    }
                    else
                    {
                        // Create a TextBox 
                        TextBox textbox = new TextBox();
                        textbox.Name = property.Name;
                        textbox.BackColor = Color.FromArgb(12, 20, 52);
                        textbox.ForeColor = Color.White;

                        // Extract value
                        string value = "";
                        object propertyValue = property.GetValue(minerSetting);

                        if (propertyValue is List<int>)
                        {
                            // Property is a List<int>
                            List<int> intValue = (List<int>)propertyValue;
                            value = minerSetting.ConvertListToStr(intValue);
                        }
                        else if (propertyValue is List<float>)
                        {
                            // Property is a List<float>
                            List<float> floatValue = (List<float>)propertyValue;
                            value = minerSetting.ConvertListToStr(floatValue);
                        }
                        else
                        {
                            value = property.GetValue(minerSetting)?.ToString();
                        }
                        textbox.Text = value;

                        if (property.Name == "MinerFilePath")
                        {
                            textbox.ReadOnly = true;

                            textbox.Click += (sender, e) =>
                            {
                                // Have user select miner.exe file
                                OpenFileDialog openFileDialog = new OpenFileDialog();
                                openFileDialog.Filter = "Executable Files (*.exe)|*.exe";

                                if (openFileDialog.ShowDialog() == DialogResult.OK)
                                {
                                    textbox.Text = openFileDialog.FileName;

                                    UpdateStatusLabel("Remove the .exe from the .bat File");
                                    SaveSettings();
                                }

                            };
                        }
                        else
                        {
                            textbox.KeyUp += (sender, e) =>
                            {
                                if (e.KeyCode == Keys.Enter)
                                {
                                    SaveSettings();
                                    UpdateMinerSettingsListBox(minerSettingsListBox.SelectedIndex);
                                }
                            };
                        }

                        inputControl = textbox;
                    }

                    // Hide Id's
                    if (property.Name.ToLower().Equals("id"))
                    {
                        inputControl.Enabled = false;
                    }

                    tableLayoutPanel.Controls.Add(nameLabel, 0, tableLayoutPanel.RowCount);
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
            selectedMinerSettings.Current_Miner_Config_Type = matchedEnumValue;
            DisplayMinerSettings();
        }
        private void minerSettingsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (minerSettingsListBox.SelectedIndex == -1) return;

            bool setDefaults = false;

            // Adding new item
            if (minerSettingsListBox.SelectedIndex == 0)
            {
                setDefaults = true;

                // Create and add new miner setting
                if (_settings.MinerSettings == null)
                    _settings.MinerSettings = new List<MinerConfig>();
                else
                    _settings.MinerSettings.Add(new MinerConfig());

                UpdateMinerSettingsListBox(_settings.MinerSettings.Count);
            }

            DisplayMinerSettings();

            if (setDefaults)
            {
                CheckBox activeCheckBox = minerSettingsPanel.Controls.Find("activeCheckBox", true).OfType<CheckBox>().FirstOrDefault();
                if (activeCheckBox != null)
                {
                    activeCheckBox.Checked = true;
                }

            }
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
        private void generateButton_Click(object sender, EventArgs e)
        {
            var minerSetting = GetSelectedMinerSettings();
            batLineTextBox.Text = minerSetting.GenerateBatFile();

            UpdateStatusLabel("");

            SaveSettings();
        }
        private void importBatButton_Click(object sender, EventArgs e)
        {
            // Add new miner setting if there aren't any yet
            if (minerSettingsListBox.Items.Count == 1)
            {
                minerSettingsListBox.SelectedIndex = 0;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Batch Files (*.bat)|*.bat";
                openFileDialog.FilterIndex = 1;

                DialogResult result = openFileDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    string selectedBatFile = openFileDialog.FileName;
                    string batContents = ReadBatFile(selectedBatFile);
                    batLineTextBox.Text = batContents;
                    SaveSettings();
                }
            }
        }
        public static string ReadBatFile(string filePath)
        {
            try
            {
                // Check if the file exists
                if (File.Exists(filePath))
                {
                    // Read the contents of the .bat file and return as a string
                    string fileContents = File.ReadAllText(filePath);
                    return fileContents;
                }
                else
                {
                    // Handle the case where the file does not exist
                    return "File does not exist.";
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during file reading
                return "Error reading file: " + ex.Message;
            }
        }
        #endregion


        #region GPUs
        // Gpus
        private Gpu GetSelectedGpu()
        {
            Gpu gpu = new Gpu();

            if (gpuListBox.SelectedIndex <= 0) return gpu;

            int id = int.Parse(gpuListBox.SelectedItem.ToString().Split('/')[1].Trim());

            foreach (Gpu savedGpu in _settings.Gpus)
            {
                if (savedGpu.Id == id)
                    return savedGpu;
            }

            return gpu;
        }
        private Gpu GetGpuSettingsFromUI()
        {
            var newGpu = GetSelectedGpu();
            TableLayoutPanel tableLayoutPanel = this.Controls.Find(GPUSETTINGSPANELNAME, true).FirstOrDefault() as TableLayoutPanel;

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

                        if (property.Name == "Enabled")
                            checkBox.Name = "gpuEnabledCheckBox";

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

            bool setDefaults = false;

            // Adding new item
            if (gpuListBox.SelectedIndex == 0)
            {
                setDefaults = true;

                // Create and add new gpu
                if (_settings.Gpus == null)
                    _settings.Gpus = new List<Gpu> { new Gpu("New Gpu") };
                else
                    _settings.Gpus.Add(new Gpu("New Gpu"));

                UpdateGpusListBox(_settings.Gpus.Count);
            }

            DisplayGpuSettings();

            UpdateStatusLabel("Edit Gpu Then Click Add GPUs");

            if (setDefaults)
            {
                CheckBox activeCheckBox = gpuSettingsPanel.Controls.Find("gpuEnabledCheckBox", true).OfType<CheckBox>().FirstOrDefault();
                if (activeCheckBox != null)
                {
                    activeCheckBox.Checked = true;
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
                    newGpu.Enabled = true;
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

            if (minerSettings == null)
            {
                UpdateStatusLabel("Please select/create a miner config.");
                return new List<string>();
            }

            // Create a new process
            Process process = new Process();

            string path = minerSettings.Miner_File_Path;

            if (!File.Exists(path) && File.Exists("miner.exe"))
                path = "miner.exe";

            // Set the process start information
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = minerSettings.List_Devices_Command,
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
            minerSettings.AddGpuSettings(_settings.Gpus);

            DisplayMinerSettings();
            UpdateStatusLabel();
        }
        private void clearGpuSettingsButton_Click(object sender, EventArgs e)
        {
            var minerSettings = GetSelectedMinerSettings();
            minerSettings.ClearGpuSettings();

            DisplayMinerSettings();
            UpdateStatusLabel();
        }
        #endregion





        private void UpdateStatusLabel(string message = "Click Generate to Update")
        {
            if (statusLabel.InvokeRequired)
            {
                // Use a delegate to update the UI thread
                this.Invoke(new Action(() => UpdateStatusLabel(message)));
                return;
            }

            if (!string.IsNullOrWhiteSpace(message))
                statusLabel.Text = "Hint: " + message;
            else
                statusLabel.Text = "";
        }


        #region Navigation Buttons
        // Navigation buttons
        private void generalButton_Click(object sender, EventArgs e)
        {
            HideAllPanels();
            CreateRotatingPanel();
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
            RemoveRotatingPanel();
            manageConfigPanel.Hide();
            generalPanel.Hide();
            walletsPanel.Hide();
            poolsPanel.Hide();
        }
        #endregion


        #region General Settings
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
        List<Keys> keysPressed = new List<Keys>();
        private void startShortKeysTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if it has focus
            if (startShortKeysTextBox.Focused)
            {
                if (startShortKeysTextBox.Tag != null &&
                    startShortKeysTextBox.Tag.ToString() == "false")
                { startShortKeysTextBox.Text += " + "; }


                if (e.KeyCode == Keys.Delete)
                {
                    // Clear short-cut keys
                    startShortKeysTextBox.Text = "";
                    startShortKeysTextBox.Tag = "true";
                    keysPressed = new List<Keys>();
                }
                else
                {
                    // Add key
                    startShortKeysTextBox.Text += e.KeyCode.ToString();
                    startShortKeysTextBox.Tag = "false";
                    keysPressed.Add(e.KeyCode);
                }

                AppSettings.Save<List<Keys>>(STARTSHORTKEYS, keysPressed);


                e.SuppressKeyPress = true; // Prevent the key press from being entered into textBox
            }
        }
        private void stopShortKeysTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if it has focus
            if (stopShortKeysTextBox.Focused)
            {
                if (stopShortKeysTextBox.Tag != null &&
                    stopShortKeysTextBox.Tag.ToString() == "false")
                { stopShortKeysTextBox.Text += " + "; }


                if (e.KeyCode == Keys.Delete)
                {
                    // Clear short-cut keys
                    stopShortKeysTextBox.Text = "";
                    stopShortKeysTextBox.Tag = "true";
                    keysPressed = new List<Keys>();
                }
                else
                {
                    // Add key
                    stopShortKeysTextBox.Text += e.KeyCode.ToString();
                    stopShortKeysTextBox.Tag = "false";
                    keysPressed.Add(e.KeyCode);
                }

                AppSettings.Save<List<Keys>>(STOPSHORTKEYS, keysPressed);


                e.SuppressKeyPress = true; // Prevent the key press from being entered into textBox
            }
        }
        private void bgComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            AppSettings.Save<string>(BGIMAGE, bgComboBox.Text);
            if (MainForm != null)
            {
                if (MainForm.rotatingPanel != null)
                    MainForm.rotatingPanel.Image = MainForm.GetBgImage(bgComboBox.Text);
                if (rotatingPanel != null)
                    rotatingPanel.Image = MainForm.GetBgImage(bgComboBox.Text);
            }
        }
        private void tipLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText("kaspa:qpfsh8feaq5evaum5auq9c29fvjnun0mrzj5ht6sz3sz09ptcdaj6qjx9fkug");
            copiedLabel.ShowTextForDuration("", 3000);
        }

        // Rotate image
        private void CreateRotatingPanel()
        {
            bgImagePanel.Controls.Clear();

            rotatingPanel = RotatingPanel.Create();

            // Add image
            string bgImage = AppSettings.Load<string>(SettingsForm.BGIMAGE);
            rotatingPanel.Image = MainForm.GetBgImage(bgImage);

            bgImagePanel.Controls.Add(rotatingPanel);

            rotatingPanel.Start();
        }
        private void RemoveRotatingPanel()
        {
            if (rotatingPanel != null)
            {
                rotatingPanel.Dispose();
                bgImagePanel.Controls.Remove(rotatingPanel);
                rotatingPanel = null;
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
                if (IsRunningAsAdmin())
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

        #endregion


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
            // Show panel
            walletPanel.Visible = true;

            Wallet selectedWallet = GetSelectedWallet();
            if (selectedWallet == null) return;

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
            if (walletsListBox.SelectedIndex <= 0) return null;

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
            // Show panel
            poolPanel.Visible = true;

            Pool selectedPool = GetSelectedPool();
            if (selectedPool == null) return;

            poolNameTextBox.Text = selectedPool.Name;
            poolAddressTextBox.Text = selectedPool.Address;
            poolPortTextBox.Text = selectedPool.Port.ToString();
            poolLinkTextBox.Text = selectedPool.Link;
            poolSsslCheckBox.Checked = selectedPool.SSL;
        }
        private Pool GetPoolFromUI()
        {
            Pool pool = new Pool();

            pool.Name = poolNameTextBox.Text;
            pool.Address = poolAddressTextBox.Text;
            pool.Port = int.TryParse(poolPortTextBox.Text, out int port) ? port : -1;
            pool.Link = poolLinkTextBox.Text;
            pool.SSL = poolSsslCheckBox.Checked;

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
            savedPool.SSL = updatedPool.SSL;
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
    /*public interface IMinerConfig
    {
        int Id { get; set; }
        string Name { get; set; }
        string Miner_File_Path { get; set; }
        bool Active { get; set; }
        bool Run_As_Admin { get; set; }
        string Bat_File_Arguments { get; set; }
        MinerConfigType Current_Miner_Config_Type { get; set; }
        IMinerConfig Current_Miner_Config { get; set; }
        List<IMinerConfig> Sub_Configs { get; set; }

        void AddGpuSettings(List<Gpu> gpus);
        string GenerateBatFileArgs();
    }*/

    public class MinerConfig //: IMinerConfig
    {
        public enum MinerConfigType
        {
            Unknown,
            Gminer,
            Trm
        }
        Random random = new Random();
        private string _batFileArguments;
        private string _batFilePath;

        public List<Gpu> Gpus;
        public int Id { get; set; }
        public string Command_Prefix;
        public char Command_Separator;
        public string List_Devices_Command;
        public List<string> Algos;
        public string Name { get; set; }
        public string Miner_File_Path
        {
            get
            {
                (string filePath, string arguments) = GetBatFilePathAndArguments();
                _batFilePath = filePath;
                return _batFilePath;
            }
            set
            {
                _batFilePath = value;
            }
        }
        public bool Active { get; set; }
        public bool Run_As_Admin { get; set; }
        public string Bat_File_Arguments
        {
            get
            {
                (string filePath, string arguments) = GetBatFilePathAndArguments();
                _batFileArguments = arguments;
                return _batFileArguments;
            }
            set
            {
                _batFileArguments = value;
            }
        }
        public MinerConfigType Current_Miner_Config_Type { get; set; }
        //public IMinerConfig Current_Miner_Config { get; set; }
        //public List<IMinerConfig> Sub_Configs { get; set; }
        public virtual string Worker_Name { get; set; }
        public virtual string Algo1 { get; set; }
        public virtual string Algo2 { get; set; }
        public virtual string Algo3 { get; set; }
        public virtual string Wallet1 { get; set; }
        public virtual string Wallet2 { get; set; }
        public virtual string Wallet3 { get; set; }
        public virtual string Pool1 { get; set; }
        public virtual string Pool2 { get; set; }
        public virtual string Pool3 { get; set; }
        public virtual int Port1 { get; set; }
        public virtual int Port2 { get; set; }
        public virtual int Port3 { get; set; }
        public virtual bool SSL1 { get; set; }
        public virtual bool SSL2 { get; set; }
        public virtual bool SSL3 { get; set; }
        public virtual int Api { get; set; }
        public virtual List<int> Devices { get; set; }
        public virtual List<int> Fan_Percent { get; set; }
        public virtual List<int> Power_Limit { get; set; }
        public virtual List<int> Temp_Limit_Core { get; set; }
        public virtual List<int> Temp_Limit_Mem { get; set; }        
        public virtual List<int> Core_Clock { get; set; }
        public virtual List<int> Core_Offset { get; set; }
        public virtual List<int> Mem_Clock { get; set; }
        public virtual List<int> Mem_Offset { get; set; }
        public virtual List<int> Core_Micro_Volts { get; set; }
        public virtual List<int> Mem_Micro_Volts { get; set; }
        public virtual List<float> Intensity { get; set; }
        public virtual List<float> Dual_Intensity { get; set; }


        public MinerConfig()
        {
            // Interface
            Id = random.Next(2303, 40598);
            Command_Prefix = "--";
            Command_Separator = ' ';
            List_Devices_Command = "--list_devices";
            Name = "New Miner Config";
            Miner_File_Path = Directory.GetCurrentDirectory() + "\\miner.exe";
            Active = true;
            Run_As_Admin = false;
            Bat_File_Arguments = "";
            Current_Miner_Config_Type = MinerConfigType.Unknown;
            Api = -1;
            Port1 = -1;
            Port2 = -1;
            Port3 = -1;

            ClearGpuSettings();
        }
        internal List<string> GetAlgos()
        {
            ChangeCurrentMinerConfig(Current_Miner_Config_Type);
            return Algos;
        }
        // Miner Specific Setup
        private void GminerSetup()
        {
            Command_Prefix = "--";
            Command_Separator = ' ';
            List_Devices_Command = "--list_devices";
            Algos = new List<string> { "ethash", "etchash", "kawpow",
                "cortex", "autolykos2", "kheavyhash",
                "aeternity", "beamhash", "octopus",
                "ironfish", "radiant", "zilliqa",
                "firo", "125_4", "cuckatoo32",
                "sero", "vds", "210_9"};
        }
        private void TrmSetup()
        {
            Command_Prefix = "--";
            Command_Separator = ',';
            List_Devices_Command = "--list_devices";
            Algos = new List<string> { "ethash", "etchash", "kawpow",
                "autolykos2", "kheavyhash", "verthash",
                "ironfish", "radiant", "zilliqa",
                "mtp_firopow"};
        }
        /*internal void AddOrUpdateSubConfig(IMinerConfig config)
        {
            bool added = false;

            for(int i = 0; i < Sub_Configs.Count(); i++)
            {
                if (Sub_Configs[i].Id == config.Id)
                {
                    // Update found config
                    Sub_Configs[i] = config;
                    added = true;
                }
            }

            if (!added)
            {
                // Add new config
                Sub_Configs.Add(config);
            }
        }*/
        internal void ChangeCurrentMinerConfig(MinerConfigType minerConfigType)
        {
            Current_Miner_Config_Type = minerConfigType;

            if(minerConfigType == MinerConfigType.Gminer || minerConfigType == MinerConfigType.Unknown)
            {
                GminerSetup();
            }
            else if (minerConfigType == MinerConfigType.Trm)
            {
                TrmSetup();
            }
            /*
            bool changed = false;

            foreach (IMinerConfig minerConfig in Sub_Configs)
            {
                if (minerConfig.Current_Miner_Config_Type == minerConfigType)
                {
                    Current_Miner_Config = minerConfig;
                    changed = true;
                }
            }

            if (!changed)
            {
                var newConfig = CreateConfigInstance(Current_Miner_Config_Type);
                AddOrUpdateSubConfig(newConfig);
            }*/
        }




        // Interface required
        /*private IMinerConfig CreateConfigInstance(MinerConfigType configType)
        {
            switch (configType)
            {
                case MinerConfigType.Gminer:
                    return new GminerConfig();
                case MinerConfigType.Trm:
                    return new TrmConfig();
                default:
                    return new UnknownConfig();
            }
        }*/
        public virtual void ClearGpuSettings()
        {
            Devices = new List<int>();
            Devices = new List<int>();
            Fan_Percent = new List<int>();
            Power_Limit = new List<int>();
            Temp_Limit_Core = new List<int>();
            Temp_Limit_Mem = new List<int>();
            Core_Clock = new List<int>();
            Core_Offset = new List<int>();
            Mem_Clock = new List<int>();
            Mem_Offset = new List<int>();
            Core_Micro_Volts = new List<int>();
            Mem_Micro_Volts = new List<int>();
            Intensity = new List<float>();
            Dual_Intensity = new List<float>();
        }
        public virtual void AddGpuSettings(List<Gpu> gpus)
        {
            ClearGpuSettings();

            bool overclocking = false;

            foreach (Gpu gpu in gpus)
            {
                if (!gpu.Enabled) continue;

                // Add values
                Devices.Add(gpu.Device_Id);
                Core_Clock.Add(gpu.Core_Clock);
                Core_Offset.Add(gpu.Core_Offset);
                Mem_Clock.Add(gpu.Mem_Clock);
                Mem_Offset.Add(gpu.Mem_Clock_Offset);
                Power_Limit.Add(gpu.Power_Limit);
                Core_Micro_Volts.Add(gpu.Core_Mv);
                Mem_Micro_Volts.Add(gpu.Mem_Mv);
                Fan_Percent.Add(gpu.Fan_Percent);
                Temp_Limit_Core.Add(gpu.Max_Core_Temp);
                Temp_Limit_Mem.Add(gpu.Max_Mem_Temp);
                Intensity.Add(gpu.Intensity);
                Dual_Intensity.Add(gpu.Dual_Intensity);

                // Check if over/under clocking gpu
                bool anyGreaterThanZero = new List<List<int>>
                {
                    Core_Clock, Core_Offset, Mem_Clock, Mem_Offset
                }
                .SelectMany(list => list)
                .Any(value => value > 0);

                if (anyGreaterThanZero)
                    overclocking = true;
            }

            if (overclocking)
            {
                Run_As_Admin = true;
            }
        }


        private (string filePath, string args) GetBatFilePathAndArguments()
        {
            string filePath = "";
            string defaultPath = Directory.GetCurrentDirectory() + "\\miner.exe";

            // Check if file path has been supplied, if not extract from .bat file if .exe is preesnt or set to default
            if (!string.IsNullOrWhiteSpace(_batFilePath))
                filePath = _batFilePath;

            if (_batFileArguments.IndexOf(".exe") >= 0)
            {
                // Get miner path
                if (_batFileArguments.StartsWith("\""))
                {
                    // Quotes around path
                    filePath = _batFileArguments.Substring(1, _batFileArguments.IndexOf(".exe") + 3);
                    _batFileArguments = _batFileArguments.Replace($"\"{filePath}\"", string.Empty).Trim();
                }
                else
                {
                    // No quotes around path
                    filePath = _batFileArguments.Substring(0, _batFileArguments.IndexOf(".exe") + 4);
                    _batFileArguments = _batFileArguments.Replace($"{filePath}", string.Empty).Trim();
                }

                if (!File.Exists(filePath)) filePath = defaultPath;
            }


            // Remove any trailing new lines
            string pattern = @"[\r\n]+$";
            _batFileArguments = Regex.Replace(_batFileArguments, pattern, String.Empty);

            // Remove any 'pause' keywords
            string lastNineChars = _batFileArguments.Substring(Math.Max(0, _batFileArguments.Length - 9));
            if (lastNineChars.Contains("pause"))
            {
                lastNineChars = lastNineChars.Replace("pause", "").TrimEnd();
                _batFileArguments = _batFileArguments.Substring(0, _batFileArguments.Length - 9) + lastNineChars;
            }

            return (filePath, _batFileArguments.Trim());
        }


        // Getter/Setter Helpers
        internal List<int> ConvertStrToIntList(string str)
        {
            List<int> listOfInts = new List<int>();
            List<string> listOfParts = str.Split(Command_Separator).ToList();

            foreach (string device in listOfParts)
                if (int.TryParse(device, out int deviceNum))
                    listOfInts.Add(deviceNum);

            return listOfInts;
        }
        internal List<float> ConvertStrToFloatList(string str)
        {
            List<float> listOfInts = new List<float>();
            List<string> listOfParts = str.Split(Command_Separator).ToList();

            foreach (string device in listOfParts)
                if (int.TryParse(device, out int deviceNum))
                    listOfInts.Add(deviceNum);

            return listOfInts;
        }
        internal string ConvertListToStr<T>(IEnumerable<T> items)
        {
            string str = "";

            int count = 0;

            foreach (T item in items)
            {
                str += item.ToString();

                // Add separator if not the last item
                if (count < items.Count() - 1)
                {
                    str += Command_Separator;
                }

                count++;
            }

            return str;
        }

        #region Generate Bat Files
        // Miner Specific Generate Bat Files
        public string GenerateBatFile()
        {
            if (Current_Miner_Config_Type == MinerConfigType.Gminer || Current_Miner_Config_Type == MinerConfigType.Unknown)
                return GenerateGminerBatFile();
            else if (Current_Miner_Config_Type == MinerConfigType.Trm)
                return GenerateTrmBatFile();
            else
                return GenerateUnknownBatFileArgs();
        }
        private string GenerateGminerBatFile()
        {
            string args = "";

            // Add "" around file path
            args += $"\"{Miner_File_Path}\" ";

            // 1st Algo
            args += $"--algo {Algo1} ";
            args += $"--ssl {SSL1} ";
            args += $"--server {Pool1} ";
            args += $"--port {Port1} ";
            args += $"--user {Wallet1}.{Worker_Name} ";

            // 2nd Algo
            if (!string.IsNullOrWhiteSpace(Algo2))
            {
                args += $"--dalgo {Algo2} ";
                args += $"--dssl {SSL2} ";
                args += $"--dserver {Pool2} ";
                args += $"--dport {Port2} ";
                args += $"--duser {Wallet2}.{Worker_Name} ";
            }

            // 3rd Algo
            if (!string.IsNullOrWhiteSpace(Algo3))
            {
                args += $"--zilssl {SSL3} ";
                args += $"--zilserver {Pool3} ";
                args += $"--zilport {Port3} ";
                args += $"--ziluser {Wallet3}.{Worker_Name} ";
            }

            // Gpus
            bool overclocking = false;

            if (Devices.Any(item => item >= 0))
                args += $"--devices {ConvertListToStr(Devices)} ";

            // Clock
            if (Core_Clock.Any(item => item >= 0))
            {
                args += $"--lock_cclock {ConvertListToStr(Core_Clock)} ";
                overclocking = true;
            }
            else if (Core_Offset.Any(item => item >= 0))
            {
                args += $"--cclock {ConvertListToStr(Core_Offset)} ";
                overclocking = true;
            }
            if (Core_Micro_Volts.Any(item => item >= 0))
            {
                args += $"--lock_voltage {ConvertListToStr(Core_Micro_Volts)} ";
                overclocking = true;
            }

            // Mem
            if (Mem_Clock.Any(item => item >= 0))
            {
                args += $"--lock_mclock {ConvertListToStr(Mem_Clock)} ";
                overclocking = true;
            }
            else if (Mem_Offset.Any(item => item >= 0))
            {
                args += $"--mclock {ConvertListToStr(Mem_Offset)} ";
                overclocking = true;
            }

            // Required for Nvidia
            if (overclocking)
                args += "--nvml 1 ";

            if (Power_Limit.Any(item => item >= 0))
                args += $"--pl {ConvertListToStr(Power_Limit)} ";

            if (Fan_Percent.Any(item => item >= 0))
                args += $"--fan {ConvertListToStr(Fan_Percent)} ";

            if (Temp_Limit_Core.Any(item => item >= 0))
                args += $"--templimit {ConvertListToStr(Temp_Limit_Core)} ";

            if (Temp_Limit_Mem.Any(item => item >= 0))
                args += $"--templimit_mem {ConvertListToStr(Temp_Limit_Mem)} ";

            if (Intensity.Any(item => item >= 0))
                args += $"--intensity {ConvertListToStr(Intensity)} ";

            if (Dual_Intensity.Any(item => item >= 0))
                args += $"--dual_intensity {ConvertListToStr(Dual_Intensity)} ";

            return args.Trim();
        }
        private string GenerateTrmBatFile()
        {
            string args = "";

            // Add "" around file path
            args += $"\"{Miner_File_Path}\" ";

            // 1st Algo
            args += $"--algo {Algo1} ";
            args += $"--ssl {SSL1} ";
            args += $"--server {Pool1} ";
            args += $"--port {Port1} ";
            args += $"--user {Wallet1}.{Worker_Name} ";

            // 2nd Algo
            args += $"--dalgo {Algo2} ";
            args += $"--dssl {SSL2} ";
            args += $"--dserver {Pool2} ";
            args += $"--dport {Port2} ";
            args += $"--duser {Wallet2}.{Worker_Name} ";

            // 3rd Algo
            args += $"--zilssl {SSL3} ";
            args += $"--zilserver {Pool3} ";
            args += $"--zilport {Port3} ";
            args += $"--ziluser {Wallet3}.{Worker_Name} ";

            // Gpus
            bool overclocking = false;

            if (Devices.Any(item => item >= 0))
                args += $"--devices {ConvertListToStr(Devices)} ";

            // Clock
            if (Core_Clock.Any(item => item >= 0))
            {
                args += $"--lock_cclock {ConvertListToStr(Core_Clock)} ";
                overclocking = true;
            }
            else if (Core_Offset.Any(item => item >= 0))
            {
                args += $"--cclock {ConvertListToStr(Core_Offset)} ";
                overclocking = true;
            }
            if (Core_Micro_Volts.Any(item => item >= 0))
            {
                args += $"--lock_voltage {ConvertListToStr(Core_Micro_Volts)} ";
                overclocking = true;
            }

            // Mem
            if (Mem_Clock.Any(item => item >= 0))
            {
                args += $"--lock_mclock {ConvertListToStr(Mem_Clock)} ";
                overclocking = true;
            }
            else if (Mem_Offset.Any(item => item >= 0))
            {
                args += $"--mclock {ConvertListToStr(Mem_Offset)} ";
                overclocking = true;
            }

            // Required for Nvidia
            if (overclocking)
                args += "--nvml 1";

            if (Power_Limit.Any(item => item >= 0))
                args += $"--pl {ConvertListToStr(Power_Limit)} ";

            if (Fan_Percent.Any(item => item >= 0))
                args += $"--fan {ConvertListToStr(Fan_Percent)} ";

            if (Temp_Limit_Core.Any(item => item >= 0))
                args += $"--templimit {ConvertListToStr(Temp_Limit_Core)} ";

            if (Temp_Limit_Mem.Any(item => item >= 0))
                args += $"--templimit_mem {ConvertListToStr(Temp_Limit_Mem)} ";

            if (Intensity.Any(item => item >= 0))
                args += $"--intensity {ConvertListToStr(Intensity)} ";

            if (Dual_Intensity.Any(item => item >= 0))
                args += $"--dual_intensity {ConvertListToStr(Dual_Intensity)} ";

            return args.Trim();
        }
        private string GenerateUnknownBatFileArgs()
        {
            string args = "WARNING: YOU MUST MANUALLY EDIT THIS OR PICK YOUR DESIRED MINER AND GENERATE AGAIN OR ASK THE DEVELOPER TO ADD YOUR MINER";
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name;
                object propertyValue = property.GetValue(this);

                // skip
                if (propertyValue == null || String.IsNullOrWhiteSpace(propertyValue.ToString())) continue;
                if (propertyName.StartsWith("Command")) continue;
                if (propertyName.Equals("Name")) continue;
                if (propertyName.Equals("Active")) continue;
                if (propertyName.Equals("Run_As_Admin")) continue;
                if (propertyValue.ToString().Trim() == "-1") continue;


                // Add "" around file path
                if (propertyName.Equals("Miner_File_Path"))
                {
                    args += $"\"{propertyValue}\" ";
                }

                // List<int>
                if (propertyValue != null && propertyValue.GetType().IsGenericType &&
                    propertyValue.GetType().GetGenericTypeDefinition() == typeof(List<>) &&
                    propertyValue.GetType().GetGenericArguments()[0] == typeof(int))
                {
                    List<int> nums = (List<int>)propertyValue;

                    // Only add nums >= 0
                    if (nums.Count > 0 && nums.First() >= 0)
                        args += $"{Command_Prefix}{propertyName} {ConvertListToStr(nums)}";
                }
                else // Default name value
                {
                    args += $"{Command_Prefix}{propertyName} {propertyValue} ";
                }
            }

            return args.Trim();
        }
        #endregion

        public string GetPool1DomainName()
        {
            if (string.IsNullOrWhiteSpace(Pool1)) return "";
            var parts = Pool1.Trim().Split('.');

            // mining url
            if (parts.Length == 3)
            {
                return parts[1] + "." + parts[2];
            }
            // pool url
            else if (parts.Length == 2)
            {
                return parts[0] + "." + parts[1];
            }
            return Pool1.Trim();
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
        public int Core_Offset { get; set; }
        public int Mem_Clock { get; set; }
        public int Mem_Clock_Offset { get; set; }
        public int Power_Limit { get; set; }
        public int Core_Mv { get; set; }
        public int Mem_Mv { get; set; } 
        public float Intensity { get; set; }
        public float Dual_Intensity { get; set; }
        public int Max_Core_Temp { get; set; }
        public int Max_Mem_Temp { get; set; }
        public int Fan_Percent { get; set; }


        public Gpu()
        {
            Id = random.Next(2303, 40598);
            Device_Id = -1;
            Name = "";
            Enabled = false;
            Core_Clock = 1200;
            Core_Offset = -1;
            Mem_Clock = -1;
            Mem_Clock_Offset = 1000;
            Power_Limit = -1;
            Core_Mv = -1;
            Mem_Mv = -1;
            Intensity = -1;
            Dual_Intensity = -1;
            Max_Core_Temp = 85;
            Max_Mem_Temp = 110;
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
            Name = "";
            Address = "";
            Coin = "";
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
        public bool SSL { get; set; }
        public Pool()
        {
            Id = random.Next(2303, 40598);
            Name = "";
            Address = "";
            Port = -1;
            Link = "";
            SSL = false;
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
