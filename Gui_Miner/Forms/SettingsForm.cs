using Gui_Miner.Classes;
using Gui_Miner.Properties;
using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using static Gui_Miner.Form1;
using static Gui_Miner.MinerConfig;
using static Gui_Miner.SettingsForm;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using Action = System.Action;
using CheckBox = System.Windows.Forms.CheckBox;
using ComboBox = System.Windows.Forms.ComboBox;
using Label = System.Windows.Forms.Label;
using Panel = System.Windows.Forms.Panel;
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
        public const string SETTINGSNAME = "Settings";
        const string MINERSETTINGSPANELNAME = "tableLayoutPanel";
        const string GPUSETTINGSPANELNAME = "gpuTableLayoutPanel";
        public const string AUTOSTARTMINING = "AutoStartMining";
        const string AUTOSTARTWITHWIN = "AutoStartWithWin";
        public const string STOPSHORTKEYS = "StopShortKeys";
        public const string STARTSHORTKEYS = "StartShortKeys";
        public const string BGIMAGE = "BackgroundImage";
        const double AppVersion = 1.8;
        const double VersionIncrement = 0.1;
        double NextAppVersion = AppVersion + VersionIncrement;
        bool progMakingChanges = false;
        public Settings Settings { get { return _settings; } }

        public SettingsForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Load/Close
        // Start/Stop Load/Save
        private async void SettingsForm_Load(object sender, EventArgs e)
        {
            await LoadSettings();

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
            HideForm();
        }
        public async void ShowForm()
        {
            await LoadSettings();

            manageMinerConfigsButton.PerformClick();

            Visible = true;
            Focus();
        }
        public void HideForm()
        {
            SaveSettings();
            this.Visible = false;

            MainForm.LoadShortcutKeys();
        }
        private async Task<bool> LoadSettings()
        {
            var loadResult = await AppSettings.LoadAsync<Settings>(SETTINGSNAME);
            if (loadResult.Success)
                _settings = loadResult.Result;
            else
                _settings = new Settings();
            return true;
        }
        public async void SaveSettings()
        {
            var uiMinerSetting = GetMinerSettingsFromUI();
            if (_settings.MinerSettings != null && _settings.MinerSettings.Count > 0)
            {
                for (int i = 0; i < _settings.MinerSettings.Count; i++)
                {
                    // Update with the new miner setting
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

            await AppSettings.SaveAsync<Settings>(SETTINGSNAME, _settings);
        }
        bool settingSettings = true; // Change auto start checkbox w/o deleting scheduled task
        private async void LoadGeneralSettings()
        {
            var stringLoaded = await AppSettings.LoadAsync<string>(AUTOSTARTMINING);
            if (stringLoaded.Success)
                autoStartMiningCheckBox.Checked = bool.TryParse(stringLoaded.Result, out bool result) ? result : false;
            
            stringLoaded = await AppSettings.LoadAsync<string>(AUTOSTARTWITHWIN);
            if (stringLoaded.Success)
                autoStartWithWinCheckBox.Checked = bool.TryParse(stringLoaded.Result, out bool winResult) ? winResult : false;
            
            stringLoaded = await AppSettings.LoadAsync<string>(BGIMAGE);
            if (stringLoaded.Success)
                bgComboBox.Text = stringLoaded.Result;
            else
                bgComboBox.Text = "Kas - Globe";

            var keysLoaded = await AppSettings.LoadAsync<List<Keys>>(STOPSHORTKEYS);
            if (stringLoaded.Success)
            {
                List<Keys> keys = keysLoaded.Result;
                if (keys != null)
                {
                    foreach (Keys key in keys)
                        stopShortKeysTextBox.Text += key.ToString() + " + ";

                    // Remove the trailing " + "
                    if (stopShortKeysTextBox.Text.EndsWith(" + "))
                        stopShortKeysTextBox.Text = stopShortKeysTextBox.Text.Substring(0, stopShortKeysTextBox.Text.Length - 3);
                }
            }

            keysLoaded = await AppSettings.LoadAsync<List<Keys>>(STARTSHORTKEYS);
            if (stringLoaded.Success)
            {
                List<Keys> keys = keysLoaded.Result;
                if (keys != null)
                {
                    foreach (Keys key in keys)
                        startShortKeysTextBox.Text += key.ToString() + " + ";

                    // Remove the trailing " + "
                    if (startShortKeysTextBox.Text.EndsWith(" + "))
                        startShortKeysTextBox.Text = startShortKeysTextBox.Text.Substring(0, startShortKeysTextBox.Text.Length - 3);
                }
            }

            versionLabel.Text = "V " + AppVersion;
        }
        #endregion


        // Update listboxes
        private void UpdateMinerSettingsListBox(int selectedIndex = 1)
        {
            if (_settings.MinerSettings == null)
                _settings.MinerSettings = new List<MinerConfig>();

            minerSettingsListBox.Items.Clear();
            var newMinerSetting = new MinerConfig();
            newMinerSetting.Name = "*Add Miner Settings";
            minerSettingsListBox.Items.Add(newMinerSetting);

            foreach (MinerConfig minerSetting in _settings.MinerSettings)
            {
                minerSettingsListBox.Items.Add(minerSetting);
            }

            if (minerSettingsListBox.Items.Count == selectedIndex)
                selectedIndex--;
            minerSettingsListBox.SelectedIndex = selectedIndex;
        }
        private void UpdateGpusListBox(int selectedIndex = 1)
        {
            if (_settings.Gpus == null)
                _settings.Gpus = new List<Gpu>();

            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateGpusListBox(selectedIndex)));
                return;
            }

            gpuListBox.Items.Clear();
            gpuListBox.Items.Add(new Gpu("*Add GPU"));

            foreach (Gpu gpu in _settings.Gpus)
            {
                gpuListBox.Items.Add(gpu);
            }
            if (_settings.Gpus.Count > 0)
                gpuListBox.SelectedIndex = selectedIndex;
        }
        private void UpdateWalletsListBox(int selectedIndex = 1)
        {
            if (_settings.Wallets == null)
                _settings.Wallets = new List<Wallet>();

            walletsListBox.Items.Clear();
            Wallet newWallet = new Wallet();
            newWallet.Name = "*Add Wallet";
            walletsListBox.Items.Add(newWallet);

            foreach (Wallet wallet in _settings.Wallets)
            {
                walletsListBox.Items.Add(wallet);
            }

            if (_settings.Wallets.Count >= 1)
            {
                // Select last item if out of bounds
                if (selectedIndex >= walletsListBox.Items.Count)
                    walletsListBox.SelectedIndex = walletsListBox.Items.Count - 1;
                else
                    walletsListBox.SelectedIndex = selectedIndex;
            }
            else
            {
                walletNameTextBox.Text = string.Empty;
                walletAddressTextBox.Text = string.Empty;
                walletCoinTextBox.Text = string.Empty;
            }
        }
        private void UpdatePoolsListBox(int selectedIndex = 1)
        {
            if (_settings.Pools == null)
            {
                _settings.Pools = new List<Pool>();
            }

            poolsListBox.Items.Clear();
            // Create and add new pool
            Pool newPool = new Pool();
            newPool.Name = "*Add Pool";
            poolsListBox.Items.Add(newPool);

            foreach (Pool pool in _settings.Pools)
                poolsListBox.Items.Add(pool);

            var defaultPools = GetDefaultPools();
            foreach (Pool defaultPool in defaultPools)
                poolsListBox.Items.Add(defaultPool);

            if (_settings.Pools.Count >= 1)
            {
                progMakingChanges = true;

                // Select last item if out of bounds
                if (selectedIndex >= poolsListBox.Items.Count)
                    poolsListBox.SelectedIndex = poolsListBox.Items.Count - 1;
                else
                    poolsListBox.SelectedIndex = selectedIndex;

                progMakingChanges = false;
            }
            else
            {
                progMakingChanges = true;

                poolNameTextBox.Text = string.Empty;
                poolAddressTextBox.Text = string.Empty;                                
                poolPortTextBox.Text = string.Empty;
                poolLinkTextBox.Text = string.Empty;
                poolSsslCheckBox.Checked = false;

                progMakingChanges = false;
            }

        }




        #region MinerConfigs
        // Miner Settings
        private MinerConfig GetSelectedMinerSettings()
        {
            if (minerSettingsListBox.SelectedIndex <= 0) 
                return null;

            return minerSettingsListBox.SelectedItem as MinerConfig;
        }
        private MinerConfig GetMinerSettingsFromUI()
        {
            if (minerSettingsListBox.SelectedIndex == 0) return new MinerConfig();

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
                            property.SetValue(newMinerSetting, newValue);
                        }
                        // Check if the property is of type List<float>
                        else if (property.PropertyType == typeof(List<float>))
                        {
                            // It's a List<float>
                            var newValue = newMinerSetting.ConvertStrToFloatList(propertyValue);
                            property.SetValue(newMinerSetting, newValue);
                        }
                        else
                        {
                            try 
                            {
                                object convertedValue = Convert.ChangeType(propertyValue, property.PropertyType);
                                property.SetValue(newMinerSetting, convertedValue);
                            }
                            catch 
                            {
                                if (string.IsNullOrWhiteSpace(propertyValue.ToString()))
                                    property.SetValue(newMinerSetting, -1); ;
                            }
                            
                        }          
                    }
                    else if (control is CheckBox checkBox)
                    {
                        bool propertyValue = checkBox.Checked;
                        property.SetValue(newMinerSetting, propertyValue);
                    }
                    else if (control is ComboBox comboBox)
                    {
                        // Set Wallet/Pool
                        if (comboBox.SelectedItem != null && !String.IsNullOrWhiteSpace(comboBox.SelectedItem.ToString()))
                        {
                            if (comboBox.SelectedItem is Wallet)
                            {
                                var wallet = comboBox.SelectedItem as Wallet;
                                property.SetValue(newMinerSetting, wallet.Address);
                            }
                            else if (comboBox.SelectedItem is Pool)
                            {
                                var pool = comboBox.SelectedItem as Pool;
                                property.SetValue(newMinerSetting, pool.Address);
                            }
                            else if (propertyName.StartsWith("Algo"))
                            {
                                // Set Algo
                                property.SetValue(newMinerSetting, comboBox.Text);
                            }
                        }
                    }
                }

            }

            return newMinerSetting;
        }
        private void DisplayMinerSettings()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => DisplayMinerSettings()));
                return;
            }

            SuspendLayout();

            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Name = MINERSETTINGSPANELNAME;
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.AutoScroll = true;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            if (_settings.MinerSettings != null)
            {
                MinerConfig minerSetting = GetSelectedMinerSettings();
                if (minerSetting == null) return;

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
                            SaveSettings();
                        };

                        inputControl = checkBox;
                    }
                    else if (property.Name.StartsWith("Wallet"))
                    {
                        ComboBox walletComboBox = new ComboBox();                        

                        walletComboBox.BackColor = Color.FromArgb(12, 20, 52);
                        walletComboBox.ForeColor = Color.White;
                        walletComboBox.Name = property.Name;

                        var walAddress = "";
                        if (property.GetValue(minerSetting) != null)
                        {
                            walAddress = property.GetValue(minerSetting).ToString();
                        }

                        int i = 0;
                        int selectedIndex = -1;
                        if (_settings.Wallets != null)
                        {
                            foreach (Wallet wallet in _settings.Wallets)
                            {
                                walletComboBox.Items.Add(wallet);
                                if (wallet.Address == walAddress)
                                    selectedIndex = i;
                                i++;
                            }
                            
                            walletComboBox.SelectedIndex = selectedIndex;
                        }

                        walletComboBox.SelectedIndexChanged += (sender, e) =>
                        {
                            if (string.IsNullOrWhiteSpace(walletComboBox.Text) || walletComboBox.Text == "none")
                            { return; }

                            SaveSettings();                            
                        };

                        // Check if Algo is empty, if so set this wallet to none
                        int prevNumber = int.Parse(property.Name.Substring(property.Name.Length - 1));

                        inputControl = walletComboBox;
                    }
                    else if (property.Name.StartsWith("Pool"))
                    {
                        ComboBox poolComboBox = new ComboBox();
                        poolComboBox.BackColor = Color.FromArgb(12, 20, 52);
                        poolComboBox.ForeColor = Color.White;
                        poolComboBox.Name = property.Name;

                        var poolAddress = "";
                        if (property.GetValue(minerSetting) != null)
                        {
                            poolAddress = property.GetValue(minerSetting).ToString();
                        }

                        int i = 0;
                        int selectedIndex = -1;
                        if (_settings.Pools != null)
                        {
                            foreach (Pool pool in _settings.Pools)
                            {
                                poolComboBox.Items.Add(pool);
                                if (pool.Address == poolAddress)
                                {
                                    selectedIndex = i;
                                    port = pool.Port;
                                }
                                i++;
                            }

                            // Add default pools
                            var defaultPools = GetDefaultPools();
                            foreach (Pool pool in defaultPools)
                            {
                                poolComboBox.Items.Add(pool);
                                if (pool.Address == poolAddress)
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
                            var currentComboBox = (ComboBox)sender;

                            // Get the selected pool
                            if (!string.IsNullOrWhiteSpace(currentComboBox.Text))
                            {                                
                                var pool = currentComboBox.SelectedItem as Pool;

                                var layoutPanel = minerSettingsPanel.Controls[0] as TableLayoutPanel;
                                int prevNumber = int.Parse(property.Name.Substring(property.Name.Length - 1));

                                // Change Port
                                Control matchingTextBox = layoutPanel.Controls.Find($"Port{prevNumber}", true).First();
                                if (matchingTextBox != null && matchingTextBox is TextBox)
                                    matchingTextBox.Text = pool.Port.ToString();

                                // Change SSL
                                CheckBox matchingCheckBox = layoutPanel.Controls.Find($"SSL{prevNumber}", true).OfType<CheckBox>().FirstOrDefault();
                                if (matchingCheckBox != null)
                                    matchingCheckBox.Checked = pool.SSL;

                                SaveSettings();
                            }
                        };

                        // Check if Algo is empty, if so set this wallet to none
                        int prevPoolNumber = int.Parse(property.Name.Substring(property.Name.Length - 1));

                        // Show none if Algo empty
                        if (minerSettingsPanel.Controls.Count > 0)
                        {
                            if ((prevPoolNumber == 1 && string.IsNullOrWhiteSpace(minerSetting.Algo1))
                                || (prevPoolNumber == 2 && string.IsNullOrWhiteSpace(minerSetting.Algo2))
                                || prevPoolNumber == 3 && string.IsNullOrWhiteSpace(minerSetting.Algo3))
                            {
                                poolComboBox.Text = "";
                                poolComboBox.SelectedIndex = -1;
                            }
                        }

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
                            if (string.IsNullOrWhiteSpace(algoComboBox.Text) || algoComboBox.Text == "none")
                            { return; }

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

                        // Show "" instead of -1
                        if (value == null || value.Trim() == "-1")
                            textbox.Text = "";
                        else
                            textbox.Text = value;

                        if (property.Name == "Miner_File_Path")
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

                        if (property.Name == "Extra_Args")
                        {
                            textbox.Multiline = true;
                            textbox.Height = 100;
                        }

                        inputControl = textbox;
                    }

                    // Hide Id's
                    if (property.Name.ToLower().Equals("id"))
                    {
                        inputControl.Enabled = false;
                    }


                    // Prevent mouse wheel from changing drop-down selections
                    if (inputControl is ComboBox updateComboBox)
                    {
                        updateComboBox.MouseWheel += (sender, e) =>
                        {
                            ((HandledMouseEventArgs)e).Handled = true;

                            // Get the parent panel
                            Panel parentPanel = updateComboBox.Parent as Panel;

                            if (parentPanel != null)
                            {
                                // Scroll the parent panel in the direction of the mouse wheel
                                if (e.Delta > 0)
                                {
                                    // Scroll up
                                    parentPanel.AutoScrollPosition = new Point(0, parentPanel.VerticalScroll.Value - SystemInformation.MouseWheelScrollDelta);
                                }
                                else
                                {
                                    // Scroll down
                                    parentPanel.AutoScrollPosition = new Point(0, parentPanel.VerticalScroll.Value + SystemInformation.MouseWheelScrollDelta);
                                }
                            }
                        };
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

            ResumeLayout(true);
            Refresh();
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

            // Adding new item
            if (minerSettingsListBox.SelectedIndex == 0)
            {
                // Create and add new miner setting
                if (_settings.MinerSettings == null)
                    _settings.MinerSettings = new List<MinerConfig>();
                else
                    _settings.MinerSettings.Add(new MinerConfig());

                UpdateMinerSettingsListBox(_settings.MinerSettings.Count);
                return;
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
            if (gpuListBox.SelectedIndex <= 0) return null;

            return gpuListBox.SelectedItem as Gpu;
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
                        
                        textbox.KeyUp += (sender, e) =>
                        {
                            if (e.KeyCode == Keys.Enter)
                            {
                                SaveSettings();
                                UpdateGpusListBox(gpuListBox.SelectedIndex);
                            }
                        };

                        // Show "" instead of -1
                        string value = property.GetValue(gpu)?.ToString();
                        if (value.Trim() == "-1")
                            textbox.Text = "";
                        else
                            textbox.Text = value;

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
                    string deviceName = device.Replace("  ", " "); // remove double spaces
                    var parts = deviceName.Split(' ');
                    string vram = parts[parts.Length - 4];
                    string fullName = "";
                    for(int i = 0; i < parts.Length - 4; i++)
                    {
                        fullName += parts[i];
                        if(i < parts.Length - 4) fullName += " ";
                    }
                    newGpu.Name = $"{fullName} {vram}";
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
        private async void generalButton_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            HideAllPanels();
            
            await LoadSettings();
            CreateRotatingPanel();
            
            successLabel.Text = "";
            generalPanel.Show();
            generalPanel.BringToFront();
            
            ResumeLayout();
            Refresh();
        }
        private async void manageMinerConfigsButton_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            HideAllPanels();
            
            await LoadSettings();

            await Task.Run(() => DisplayMinerSettings());
            
            manageConfigPanel.Show();
            manageConfigPanel.BringToFront();
            
            ResumeLayout(true);
            Refresh();
        }
        private async void manageWalletsButton_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            HideAllPanels();

            await LoadSettings();
            
            walletsPanel.Show();
            walletsPanel.BringToFront();

            UpdateWalletsListBox();

            ResumeLayout();
            Refresh();
        }
        private async void managePoolsButton_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            HideAllPanels();

            await LoadSettings();

            poolsPanel.Show();
            poolsPanel.BringToFront();

            UpdatePoolsListBox();

            ResumeLayout();
            Refresh();
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
        private async void autoStartMiningCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            await AppSettings.SaveAsync<string>(AUTOSTARTMINING, autoStartMiningCheckBox.Checked.ToString());
        }
        private async void autoStartWithWinCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            await AppSettings.SaveAsync<string>(AUTOSTARTWITHWIN, autoStartWithWinCheckBox.Checked.ToString());

            if (autoStartWithWinCheckBox.Checked)
            {
                if (MainForm != null && MainForm.GetTaskManager().IsRunningAsAdmin())
                {
                    string assemblyPath = Assembly.GetEntryAssembly().Location;
                    if (CreateSchedulerTask("GuiMiner", assemblyPath))
                        successLabel.Text = "Successfully added auto start task";
                    else
                        successLabel.Text = "Unable to add auto start task, please try restarting your PC and try again.";
                }
                else
                {
                    successLabel.Text = "Please restart the app as admin in order to start with Windows";
                }
            }
            else if(!settingSettings)
            {
                DeleteSchedulerTask("GuiMiner");
            }
        }
        List<Keys> keysPressed = new List<Keys>();
        private async void startShortKeysTextBox_KeyDown(object sender, KeyEventArgs e)
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

                await AppSettings.SaveAsync<List<Keys>>(STARTSHORTKEYS, keysPressed);

                e.SuppressKeyPress = true; // Prevent the key press from being entered into textBox
            }
        }
        private async void stopShortKeysTextBox_KeyDown(object sender, KeyEventArgs e)
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

                await AppSettings.SaveAsync<List<Keys>>(STOPSHORTKEYS, keysPressed);


                e.SuppressKeyPress = true; // Prevent the key press from being entered into textBox
            }
        }
        private void stopShortKeysTextBox_Enter(object sender, EventArgs e)
        {
            keysPressed = new List<Keys>();
        }
        private void startShortKeysTextBox_Enter(object sender, EventArgs e)
        {
            keysPressed = new List<Keys>();
        }

        // Change rotating image
        private async void bgComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await AppSettings.SaveAsync<string>(BGIMAGE, bgComboBox.Text);
            if (MainForm != null)
            {
                if (MainForm.rotatingPanel != null)
                    MainForm.rotatingPanel.Image = MainForm.GetBgImage(bgComboBox.Text);
                if (rotatingPanel != null)
                    rotatingPanel.Image = MainForm.GetBgImage(bgComboBox.Text);

                // Change home page bg image
                foreach (Control mainFormControl in MainForm.Controls)
                {
                    if (mainFormControl is Panel outputPanel && outputPanel.Name == "outputPanel")
                    {
                        SetImageForRotatingPanels(outputPanel);
                    }
                }
            }
        }
        private void SetImageForRotatingPanels(Control parentControl)
        {
            foreach (Control control in parentControl.Controls)
            {
                if (control is RotatingPanel rotatingPanel)
                {
                    // Set the Image property of the RotatingPanel
                    rotatingPanel.Image = MainForm.GetBgImage(bgComboBox.Text);
                }
                else if (control.HasChildren)
                {
                    SetImageForRotatingPanels(control);
                }
            }
        }

        // Tip link
        private void tipLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try { Clipboard.SetText("kaspa:qpfsh8feaq5evaum5auq9c29fvjnun0mrzj5ht6sz3sz09ptcdaj6qjx9fkug"); }
            catch { }
            copiedLabel.ShowTextForDuration("", 3000);
        }

        // Rotate image
        private async void CreateRotatingPanel()
        {
            bgImagePanel.Controls.Clear();

            rotatingPanel = RotatingPanel.Create();

            // Add image
            var stringLoaded = await AppSettings.LoadAsync<string>(SettingsForm.BGIMAGE);
            if(stringLoaded.Success)
                rotatingPanel.Image = MainForm.GetBgImage(stringLoaded.Result);
            else
                rotatingPanel.Image = MainForm.GetBgImage("Kas - Globe");

            bgImagePanel.Controls.Add(rotatingPanel);

            //rotatingPanel.Start();
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
        public bool CreateSchedulerTask(string taskName, string applicationPath)
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
                if (MainForm.GetTaskManager().IsRunningAsAdmin())
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
        int lastSelectedWalletIndex = -1;
        private void walletsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (walletsListBox.SelectedIndex == -1 || progMakingChanges) return;

            if(unsavedChanges)
                SaveWallet();

            // Adding new item
            if (walletsListBox.SelectedIndex == 0)
            {
                // Prevent duplicates
                foreach (var item in walletsListBox.Items)                
                    if (item.ToString().StartsWith("*Add"))
                        return;

                // Create and add new setting
                Wallet newWallet = new Wallet();
                newWallet.Name = "*Add Wallet";

                if (_settings.Wallets == null)
                    _settings.Wallets = new List<Wallet> { newWallet };
                else
                    _settings.Wallets.Add(newWallet);

                UpdateWalletsListBox(_settings.Wallets.Count);
            }

            DisplayWalletSettings();

            lastSelectedWalletIndex = walletsListBox.SelectedIndex;
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
            if (walletsListBox.SelectedIndex == -1) return null;

            return walletsListBox.SelectedItem as Wallet;
        }
        private void SaveWallet()
        {
            progMakingChanges = true;

            if (walletsListBox.SelectedIndex >= 0)
            {
                // Remove the old *Add Wallet
                walletsListBox.Items.RemoveAt(walletsListBox.SelectedIndex);
            }

            var wallet = GetWalletFromUI(); // Get the new pool
            walletsListBox.Items.Add(wallet); // Add it to UI
            walletsListBox.SelectedItem = wallet; // and select it

            SaveWallets();

            unsavedChanges = false;
            progMakingChanges = false;
        }
        private async void SaveWallets()
        {
            var wallets = new List<Wallet>();

            foreach (var item in walletsListBox.Items)
            {
                var wallet = item as Wallet;
                if (!wallet.Name.StartsWith("*"))
                    wallets.Add(wallet);
            }

            _settings.Wallets = wallets;

            await AppSettings.SaveAsync<Settings>(SETTINGSNAME, _settings);

            walletsListBox.Refresh();

            unsavedChanges = false;
        }
        private void walletNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveWallet();
            }
            unsavedChanges = true;
        }
        private void walletAddressTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveWallet();
            }
            unsavedChanges = true;
        }
        private void walletCoinTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveWallet();
            }
            unsavedChanges = true;
        }
        private void walletsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var selectedWallet = GetSelectedWallet();

                // Display a confirmation dialog
                DialogResult result = MessageBox.Show($"Are you sure you want to delete this wallet {selectedWallet}?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // User clicked "Yes"
                    int indexToRemove = _settings.Wallets.FindIndex(w => w.Id.Equals(selectedWallet.Id));

                    // Check if a matching wallet was found
                    if (indexToRemove != -1)
                    {
                        // Remove the wallet from the list
                        _settings.Wallets.RemoveAt(indexToRemove);
                        walletsListBox.Items.RemoveAt(walletsListBox.SelectedIndex);

                        SaveWallets();

                        UpdateWalletsListBox(walletsListBox.Items.Count);
                    }
                }
            }
        }
        #endregion


        #region Pools
        // Manage Pools
        int lastSelectedPoolIndex = -1;
        bool unsavedChanges = false;
        private void poolsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (poolsListBox.SelectedIndex == -1 || progMakingChanges) 
                return;
                        
            if (unsavedChanges) 
                SavePool();

            var selectedPool = GetSelectedPool();

            // Adding new item
            if (poolsListBox.SelectedIndex == 0)
            {
                // Prevent duplicates
                foreach (var item in poolsListBox.Items)
                    if (item.ToString().StartsWith("*Add"))
                        return;

                // Create and add new pool
                Pool pool = new Pool();
                pool.Name = "*Add Pool";

                if (_settings.Pools == null)
                    _settings.Pools = new List<Pool> { pool };
                else
                    _settings.Pools.Add(pool);
                
                UpdatePoolsListBox(1);
            }

            DisplayPoolSettings();

            lastSelectedPoolIndex = poolsListBox.SelectedIndex;
        }
        private void DisplayPoolSettings()
        {
            progMakingChanges = true;

            // Show panel
            poolPanel.Visible = true;

            Pool selectedPool = GetSelectedPool();
            if (selectedPool == null) return;

            poolNameTextBox.Text = selectedPool.Name;
            poolAddressTextBox.Text = selectedPool.Address;
            poolPortTextBox.Text = selectedPool.Port.ToString();
            poolLinkTextBox.Text = selectedPool.Link;
            poolSsslCheckBox.Checked = selectedPool.SSL;

            // Reset ping
            pingLabel.Text = "";

            progMakingChanges = false;
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
            if (poolsListBox.SelectedIndex <= -1) return null;

            var selectedPool = poolsListBox.SelectedItem as Pool;

            return selectedPool;
        }
        private void SavePool()
        {
            progMakingChanges = true;

            if (poolsListBox.SelectedIndex >= 0)
            {
                // Remove the old *Add Pool
                poolsListBox.Items.RemoveAt(poolsListBox.SelectedIndex);
            }
            
            var pool = GetPoolFromUI(); // Get the new pool
            poolsListBox.Items.Add(pool); // Add it to UI
            poolsListBox.SelectedItem = pool; // and select it

            SavePools();

            unsavedChanges = false;
            progMakingChanges = false;
        }
        private async void SavePools()
        {
            var pools = new List<Pool>();

            foreach(var item in poolsListBox.Items)
            {
                var pool = item as Pool;
                if (!pool.Name.StartsWith("*"))
                    pools.Add(pool);
            }
            
            _settings.Pools = pools;

            await AppSettings.SaveAsync<Settings>(SETTINGSNAME, _settings);
                
            poolsListBox.Refresh();

            unsavedChanges = false;
        }

        private void poolNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SavePool();
            }
            unsavedChanges = true;
        }
        private void poolAddressTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SavePool();
            }
            unsavedChanges = true;
        }
        private void poolPortTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SavePool();
            }
            unsavedChanges = true;
        }
        private void poolLinkTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SavePool();
            }
            unsavedChanges = true;
        }
        private void poolSsslCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!progMakingChanges)
                SavePool();
        }
        private void poolsListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var selectedPool = GetSelectedPool();

                // Display a confirmation dialog
                DialogResult result = MessageBox.Show($"Are you sure you want to delete the pool named {selectedPool}?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // User clicked "Yes"
                    int indexToRemove = _settings.Pools.FindIndex(pool => pool.Id.Equals(selectedPool.Id));

                    // Check if a matching pool was found
                    if (indexToRemove != -1)
                    {
                        // Remove the pool from the list
                        _settings.Pools.RemoveAt(indexToRemove);
                        poolsListBox.Items.RemoveAt(poolsListBox.SelectedIndex);

                        SavePools();

                        UpdatePoolsListBox(1);
                    }
                }                
            }
        }
        public List<Pool> GetDefaultPools()
        {
            var pools = new List<Pool>();

            // UnMineable SSL
            Pool pool = new Pool();
            pool.Port = 443;
            pool.SSL = true;
            pool.Name = "*UnMineable - Nexa SSL";
            pool.Address = "nexapow.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 443;
            pool.SSL = true;
            pool.Name = "*UnMineable - Octopus SSL";
            pool.Address = "octopus.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 443;
            pool.SSL = true;
            pool.Name = "*UnMineable - Karlsen SSL";
            pool.Address = "karlsenhash.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 443;
            pool.SSL = true;
            pool.Name = "*UnMineable - IronFish SSL";
            pool.Address = "ironfish.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 443;
            pool.SSL = true;
            pool.Name = "*UnMineable - Alephium SSL";
            pool.Address = "blake3.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 443;
            pool.SSL = true;
            pool.Name = "*UnMineable - Firo SSL";
            pool.Address = "firopow.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 443;
            pool.SSL = true;
            pool.Name = "*UnMineable - Ergo SSL";
            pool.Address = "autolykos.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 443;
            pool.SSL = true;
            pool.Name = "*UnMineable - Kawpow SSL";
            pool.Address = "kp.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 443;
            pool.SSL = true;
            pool.Name = "*UnMineable - Etc SSL";
            pool.Address = "etchash.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 443;
            pool.SSL = true;
            pool.Name = "*UnMineable - Ethash SSL";
            pool.Address = "ethash.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 443;
            pool.SSL = true;
            pool.Name = "*UnMineable - Xmr (cpu) SSL";
            pool.Address = "rx.unmineable.com";
            pools.Add(pool);

            // Non-SSL
            pool = new Pool();
            pool.Port = 3333;
            pool.SSL = false;
            pool.Name = "*UnMineable - Nexa";
            pool.Address = "nexapow.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 3333;
            pool.SSL = false;
            pool.Name = "*UnMineable - Octopus";
            pool.Address = "octopus.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 3333;
            pool.SSL = false;
            pool.Name = "*UnMineable - Karlsen";
            pool.Address = "karlsenhash.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 3333;
            pool.SSL = false;
            pool.Name = "*UnMineable - IronFish";
            pool.Address = "ironfish.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 3333;
            pool.SSL = false;
            pool.Name = "*UnMineable - Alephium";
            pool.Address = "blake3.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 3333;
            pool.SSL = false;
            pool.Name = "*UnMineable - Firo";
            pool.Address = "firopow.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 3333;
            pool.SSL = false;
            pool.Name = "*UnMineable - Ergo";
            pool.Address = "autolykos.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 3333;
            pool.SSL = false;
            pool.Name = "*UnMineable - Kawpow";
            pool.Address = "kp.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 3333;
            pool.SSL = false;
            pool.Name = "*UnMineable - Etc";
            pool.Address = "etchash.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 3333;
            pool.SSL = false;
            pool.Name = "*UnMineable - Ethash";
            pool.Address = "ethash.unmineable.com";
            pools.Add(pool);

            pool = new Pool();
            pool.Port = 3333;
            pool.SSL = false;
            pool.Name = "*UnMineable - Xmr (cpu)";
            pool.Address = "rx.unmineable.com";
            pools.Add(pool);

            return pools;
        }

        // Ping
        private async void pingPictureBox_Click(object sender, EventArgs e)
        {

            // Make sure user supplied a url+port
            if (string.IsNullOrWhiteSpace(poolAddressTextBox.Text))
            {
                pingLabel.Text = "Please enter a valid pool address";
                return;
            }
            if (string.IsNullOrWhiteSpace(poolPortTextBox.Text))
            {
                pingLabel.Text = "Please enter a valid port";
                return;
            }

            // Change bg image
            pingPictureBox.BackgroundImage = Properties.Resources.pinging;
            pingPictureBox.Enabled = false;
            pingLabel.Text = "pinging...";

            // Ping pool
            string url = $"{poolAddressTextBox.Text}:{poolPortTextBox.Text}";
            if (poolSsslCheckBox.Checked)
                url = $"-tls {url}";            
            pingLabel.Text = await PingPool(url) + " ms";

            // Change bg image back
            pingPictureBox.BackgroundImage = Properties.Resources.ping;
            pingPictureBox.Enabled = true;

        }
        private async Task<int> PingPool(string url)
        {
            int latency = -1;
            Process process = new Process();
            string stratumPingPath = Directory.GetCurrentDirectory() + "\\stratum-ping.exe";

            // Set the process start information
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = stratumPingPath,
                Arguments = url,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;

            List<string> outputLines = new List<string>();

            // Subscribe to the OutputDataReceived event
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputLines.Add(e.Data);
                };
            };

            try
            {
                await Task.Run(() =>
                {
                    // Start the process
                    process.Start();

                    // Begin asynchronously reading the output
                    process.BeginOutputReadLine();

                    // Asynchronously wait for the process to exit
                    process.WaitForExit();

                    // Close the standard output stream
                    process.Close();
                    process.Dispose();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("\nError starting stratum-ping " + ex.Message);
            }

            // Get avg latency
            string latencyStr = outputLines.Last();
            var parts = latencyStr.Split(',');
            if(parts.Length == 3)
                latency = int.TryParse(parts[1].Substring(0, parts[1].IndexOf('.')), out int parsedAvg) ? parsedAvg : -1;
           
            return latency;
        }
        #endregion


        #region Update App
        // Update App
        private void checkUpdatesButton_Click(object sender, EventArgs e)
        {
            CheckForUpdates();
        }
        public void CheckForUpdates()
        {
            if (UpdatesAvailable())
            {
                // Updates found
                DialogResult result = MessageBox.Show("Update Found! Close the app and update?", "Update Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Check if we need to run as admin
                    bool runAs = AppIsRunningAsAdmin();
                    UpdateApp(runAs);
                }
            }
            else
            {
                // No updates found
                UpdateStatusLabel("No updates found at this time");
            }
        }
        static bool AppIsRunningAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        internal string GetUpdateAppPath()
        {
            string updateProjectPath = Directory.GetCurrentDirectory() + "\\update.exe";

            if(File.Exists(updateProjectPath))
                return updateProjectPath;

            return "";
        }
        private bool UpdatesAvailable()
        {
            string updateProjectPath = GetUpdateAppPath();            

            // Create a process start info
            ProcessStartInfo startInfo = new ProcessStartInfo(updateProjectPath);
            startInfo.Arguments = $"-checkupdate -{NextAppVersion} -false";

            // Start the "update" project as a separate process
            try
            {
                Process proc = Process.Start(startInfo);                
                proc.WaitForExit();
                int exitCode = proc.ExitCode;
                proc.Close();
                proc.Dispose();

                if (exitCode == 1)
                    return true;
                else if (exitCode == 0)
                    return false;

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating " + ex.Message);
                return false;
            }
        }
        private void UpdateApp(bool runAsAdmin)
        {
            string updateProjectPath = GetUpdateAppPath();
            string command = "update";            

            // Create a process start info
            ProcessStartInfo startInfo = new ProcessStartInfo(updateProjectPath);
            if(runAsAdmin)
                startInfo.Verb = "runas";
            startInfo.Arguments = $"-{command} -{NextAppVersion} {runAsAdmin}";

            // Start the "update" project as a separate process
            try
            {
                Process.Start(startInfo);
                MainForm.CloseApp();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating to version " + NextAppVersion + " -> " + ex.Message);
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
        private string _batFileArguments;

        public List<Gpu> Gpus;
        public int Id { get; set; }
        public string Command_Prefix;
        public char Command_Separator;
        public string List_Devices_Command;
        public List<string> Algos;
        public string Name { get; set; }
        public string Miner_File_Path { get; set; }
        public bool Active { get; set; }
        public bool Run_As_Admin { get; set; }
        public bool Use_Shortcut_Keys { get; set; }
        public bool Redirect_Console_Output { get; set; }
        public string Bat_File_Arguments
        {
            get
            {
                _batFileArguments = GetBatFileArguments();
                return _batFileArguments;
            }
            set
            {
                _batFileArguments = value;
            }
        }
        public MinerConfigType Current_Miner_Config_Type { get; set; }
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
        public virtual string Extra_Args { get; set; }

        public override string ToString()
        {
            return Name;
        }

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
            Use_Shortcut_Keys = true;
            Redirect_Console_Output = true;
            Run_As_Admin = false;
            Bat_File_Arguments = "";
            Current_Miner_Config_Type = MinerConfigType.Unknown;
            Algo1 = ""; Algo2 = ""; Algo3 = "";
            Wallet1 = ""; Wallet2 = ""; Wallet3 = "";            
            Port1 = -1; Port2 = -1; Port3 = -1;
            SSL1 = false; SSL2 = false; SSL3 = false;
            Api = -1;
            Extra_Args = "";

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
            Algos = new List<string> { "none",
                "autolykos2", "radiant", "zilliqa",
                "cortex", "ethash", "kheavyhash",
                "aeternity", "beamhash", "octopus",
                "ironfish", "etchash", "kawpow",
                "firo", "125_4", "cuckatoo32",
                "sero", "vds", "210_9"};
        }
        private void TrmSetup()
        {
            Command_Prefix = "--";
            Command_Separator = ',';
            List_Devices_Command = "--list_devices";
            Algos = new List<string> { "none",
                "ethash", "etchash", "kawpow",
                "autolykos2", "kheavyhash", "verthash",
                "ironfish", "radiant", "zilliqa",
                "mtp_firopow"};
        }
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
        }

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

        private string GetBatFileArguments()
        {            
            // Remove filepath if there is one
            if (_batFileArguments.IndexOf(".exe") >= 0)
            {
                string filePath = "";

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

            return _batFileArguments.Trim();
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

            // 1st Algo
            if (!string.IsNullOrWhiteSpace(Algo1) && Algo1 != "none")
            {
                args += $"--algo {Algo1} ";
                args += $"--ssl {SSL1} ";
                args += $"--server {Pool1} ";
                args += $"--port {Port1} ";
                args += $"--user {Wallet1}.{Worker_Name} ";
            }

            // 2nd Algo
            if (!string.IsNullOrWhiteSpace(Algo2) && Algo2 != "none")
            {
                args += $"--dalgo {Algo2} ";
                args += $"--dssl {SSL2} ";
                args += $"--dserver {Pool2} ";
                args += $"--dport {Port2} ";
                args += $"--duser {Wallet2}.{Worker_Name} ";
            }

            // 3rd Algo
            if (!string.IsNullOrWhiteSpace(Algo3) && Algo3 != "none")
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

            if(!string.IsNullOrWhiteSpace(Extra_Args))
                args += Extra_Args.Trim();

            return args.Trim();
        }
        private string GenerateTrmBatFile()
        {
            string args = "";

            if (!string.IsNullOrWhiteSpace(Algo1) && Algo1 != "none")
            {
                // 1st Algo
                args += $"--algo {Algo1} ";
                args += $"--ssl {SSL1} ";
                args += $"--server {Pool1} ";
                args += $"--port {Port1} ";
                args += $"--user {Wallet1}.{Worker_Name} ";
            }

            if (!string.IsNullOrWhiteSpace(Algo2) && Algo2 != "none")
            {
                // 2nd Algo
                args += $"--dalgo {Algo2} ";
                args += $"--dssl {SSL2} ";
                args += $"--dserver {Pool2} ";
                args += $"--dport {Port2} ";
                args += $"--duser {Wallet2}.{Worker_Name} ";
            }

            if (!string.IsNullOrWhiteSpace(Algo3) && Algo3 != "none")
            {
                // 3rd Algo
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

            if (!string.IsNullOrWhiteSpace(Extra_Args))
                args += Extra_Args.Trim();

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

            if (!string.IsNullOrWhiteSpace(Extra_Args))
                args += Extra_Args.Trim();

            return args.Trim();
        }
        #endregion

        public string GetPool1DomainName()
        {
            return ExtractUrl(Pool1);
        }
        public string GetPool2DomainName()
        {
            return ExtractUrl(Pool2);
        }
        public string GetPool3DomainName()
        {
            return ExtractUrl(Pool3);
        }
        private string ExtractUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            var parts = text.Trim().Split('.');

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

            return text.Trim();
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
            Dual_Intensity = 3;
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
        public override string ToString()
        {
            return Name;
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
        public override string ToString()
        {
            return Name;
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
        public override string ToString()
        {
            return Name;
        }
    }
    #endregion

    internal static class AppSettings
    {
        private static readonly string SettingsFilePath = "AppSettings.json";
        private static readonly SemaphoreSlim FileLock = new SemaphoreSlim(1, 1);

        public static async Task<bool> SaveAsync<T>(string key, T value)
        {
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                if (await TrySaveAsync(key, value))
                {
                    return true;
                }

                // Wait for a short delay before retrying
                //await Task.Delay(500);
            }

            return false;
        }

        public static async Task<(bool Success, T Result)> LoadAsync<T>(string key)
        {
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                var result = await TryLoadAsync<T>(key);
                if (result.Success)
                {
                    return result;
                }

                // Wait for a short delay before retrying
                //await Task.Delay(500);
            }

            return (false, default);
        }

        private static async Task<bool> TrySaveAsync<T>(string key, T value)
        {
            await FileLock.WaitAsync();
            try
            {
                var settings = await LoadSettingsAsync();
                settings[key] = JsonConvert.SerializeObject(value);
                await SaveSettingsAsync(settings);

                return true;
            }
            finally
            {
                FileLock.Release();
            }
        }

        private static async Task<(bool Success, T Result)> TryLoadAsync<T>(string key)
        {
            await FileLock.WaitAsync();
            try
            {
                var settings = await LoadSettingsAsync();
                if (settings.ContainsKey(key))
                {
                    string serializedValue = settings[key];
                    T result = JsonConvert.DeserializeObject<T>(serializedValue);
                    return (true, result);
                }

                return (false, default);
            }
            finally
            {
                FileLock.Release();
            }
        }

        private static async Task SaveSettingsAsync(Dictionary<string, string> settings)
        {
            using (var fileStream = new FileStream(SettingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            using (var writer = new StreamWriter(fileStream))
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                await writer.WriteAsync(json);
            }
        }

        private static async Task<Dictionary<string, string>> LoadSettingsAsync()
        {
            if (File.Exists(SettingsFilePath))
            {
                using (var fileStream = new FileStream(SettingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                using (var reader = new StreamReader(fileStream))
                {
                    string json = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
            }
            else
            {
                return new Dictionary<string, string>();
            }
        }
    }




}
