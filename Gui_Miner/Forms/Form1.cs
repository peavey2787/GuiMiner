using Gui_Miner.Classes;
using Gui_Miner.Properties;
using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Management.Instrumentation;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using Action = System.Action;
using Application = System.Windows.Forms.Application;
using Image = System.Drawing.Image;
using Task = System.Threading.Tasks.Task;
using Timer = System.Windows.Forms.Timer;

namespace Gui_Miner
{
    public partial class Form1 : Form
    {
        NotifyIcon notify_icon;
        SettingsForm settingsForm = new SettingsForm();
        internal RotatingPanel rotatingPanel;
        CancellationTokenSource ctsRunningMiners = new CancellationTokenSource();
        private GlobalKeyboardHook globalKeyboardHook = new GlobalKeyboardHook();
        private TaskManager taskManager;
        bool switchingMinerSettings = false;
        public TaskManager GetTaskManager() { return taskManager; }
        public Dictionary<string, Process> GetRunningMiners() { return taskManager.GetRunningTasks(); }
        public Form1()
        {
            InitializeComponent();

            taskManager = new TaskManager(this);

            // Create notify icon
            CreateNotifyIcon();

            // Create Settings Form
            settingsForm.Show();
            settingsForm.MainForm = this;
            settingsForm.Visible = false;

            outputPanel.BackgroundImage = null;

            // Rotating panel
            CreateRotatingPanel();

            // Listen for short-cut keys
            LoadShortcutKeys();

            Task.Run(() =>
            {
                NServer nServer = new NServer();
                nServer.Form1 = this;
                nServer.Start();
            });
        }
        public async void LoadShortcutKeys()
        {
            // Run the entire method on a background thread
            await Task.Run(async () =>
            {
                globalKeyboardHook.SetMainForm(this);

                var startShortKeys = new List<Keys>();
                var stopShortKeys = new List<Keys>();

                var keysLoaded = await AppSettings.LoadAsync<List<Keys>>(SettingsForm.STARTSHORTKEYS);
                if (keysLoaded.Success)
                    startShortKeys = keysLoaded.Result;

                keysLoaded = await AppSettings.LoadAsync<List<Keys>>(SettingsForm.STOPSHORTKEYS);
                if (keysLoaded.Success)
                    stopShortKeys = keysLoaded.Result;

                if (stopShortKeys != null && startShortKeys != null)
                {
                    globalKeyboardHook.SetStartKeys(startShortKeys.ToArray());
                    globalKeyboardHook.SetStopKeys(stopShortKeys.ToArray());
                }
                else if (startShortKeys != null)
                {
                    globalKeyboardHook.SetStartKeys(startShortKeys.ToArray());
                }
                else if (stopShortKeys != null)
                {
                    globalKeyboardHook.SetStopKeys(stopShortKeys.ToArray());
                }

                // Start the keyboard hook on the main thread
                try
                {
                    Invoke((MethodInvoker)delegate
                    {
                        globalKeyboardHook.Start();
                    });
                }
                catch { }
            });
        }


        // Load/Close Form
        private async void Form1_Load(object sender, EventArgs e)
        {
            // Load last window location
            var lastLocationLoaded = await AppSettings.LoadAsync<Point>("windowLocation");
            if (lastLocationLoaded.Success)
                this.Location = lastLocationLoaded.Result;

            startButtonPictureBox.SetTagThreadSafe("play");

            // Auto start
            bool autoStart = false;
            var stringLoaded = await AppSettings.LoadAsync<string>(SettingsForm.AUTOSTARTMINING);
            if (stringLoaded.Success)
                autoStart = bool.TryParse(stringLoaded.Result, out bool result) ? result : false;

            if (autoStart)
            {
                await Task.Delay(1000);
                ClickStartButton();
            }

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            CloseApp();
            // Minimize to tray instead
            /*
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                this.ShowInTaskbar = false;
                this.WindowState = FormWindowState.Minimized;
            }*/
        }
        internal async void CloseApp()
        {
            // Hide main form and remove notify icon
            this.Visible = false;
            notify_icon.Dispose();

            // Close all running miners
            await Task.Run(() => taskManager.ToggleAllTasks(false));

            // Close settings form
            if (settingsForm.InvokeRequired)
                settingsForm.Invoke(new Action(() => this.Close()));
            else
                settingsForm.Close();

            // Close the main form
            this.Close();
            this.Dispose();

            // Exit the application
            Environment.Exit(0);
            Application.Exit();
            Application.ExitThread();

        }


        // Start/Stop/Settings Buttons
        private async void startButtonPictureBox_Click(object sender, EventArgs e)
        {
            // Disable the button immediately
            startButtonPictureBox.Enabled = false;

            if ((string)startButtonPictureBox.Tag == "play")
            {
                ClickStartButton();
            }
            else if ((string)startButtonPictureBox.Tag == "stop")
            {
                await ClickStopButton();
            }
            startButtonPictureBox.Enabled = true;
        }
        private void settingsButtonPictureBox_Click(object sender, EventArgs e)
        {
            settingsForm.ShowForm();
        }
        internal void ClickStartButton(bool shortcutOnly = false)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ClickStartButton(shortcutOnly)));
                return;
            }

            if (!shortcutOnly)
            {
                // Play button pushed
                startButtonPictureBox.SetBackgroundImageThreadSafe(Properties.Resources.stop_button);
                startButtonPictureBox.SetTagThreadSafe("stop");

                // Remove background
                RemoveRotatingPanel();

                // Reset restart counter
                restartsLabel.SetTextThreadSafe("Restarts 0");
                restartsLabel.ForeColorThreadSafe(Color.White);
                restartsLabel.Show();
            }

            CreateTabControlAndStartMiners(shortcutOnly);
        }
        internal async Task<bool> ClickStopButton(bool shortcutOnly = false)
        {
            if (!shortcutOnly)
            {
                // Replace background
                await Task.Run(() => CreateRotatingPanel());

                // Reset restart counter
                restartsLabel.SetTextThreadSafe($"Restarts 0");
                restartsLabel.HideThreadSafe();
            }

            // Close all running miners
            bool response = await Task<bool>.Run(() => KillAllRunningMiners(shortcutOnly));

            // Stop button pushed
            startButtonPictureBox.SetBackgroundImageThreadSafe(Properties.Resources.play_button);
            startButtonPictureBox.SetTagThreadSafe("play");

            return response;
        }


        #region Remote Actions
        // Remote Actions
        public async Task<Settings> GetSettings()
        {
            var settings = new Settings();
            var settingsLoaded = await AppSettings.LoadAsync<Settings>(SettingsForm.SETTINGSNAME);
            if (settingsLoaded.Success)
                settings = settingsLoaded.Result;
            return settings;
        }
        public Task SaveSettings(Settings settings)
        {
            return AppSettings.SaveAsync<Settings>(SettingsForm.SETTINGSNAME, settings);
        }
        public async void SwitchActiveMinerSetting(string oldMinerSettingsId, string newMinerSettingsId)
        {
            if (switchingMinerSettings) return;
            
            switchingMinerSettings = true;
            
            TabPage tabPage = outputPanel.Controls.OfType<TabControl>().SelectMany(tc => tc.TabPages.Cast<TabPage>()).FirstOrDefault(tp => tp.Name.Equals(oldMinerSettingsId));

            if (tabPage == null)
                return;

            Panel panel = tabPage.Controls.OfType<Panel>().FirstOrDefault();

            if (panel == null)
                return;

            RichTextBox textBox = tabPage.Controls.OfType<RichTextBox>().FirstOrDefault();
            textBox.AppendTextThreadSafe("Switching Active Miner Settings...");

            // Get the start button and click it if it is in the stop state
            PictureBox toggleButton = panel.Controls.OfType<PictureBox>().FirstOrDefault(pb => (string)pb.Name == "toggleButton");

            // Access the ComboBox named "quickChangeSettings" within the TabPage
            ComboBox quickChangeSettings = panel.Controls.OfType<ComboBox>().FirstOrDefault(c => c.Name == "quickChangeSettings");

            // Access the Label named "selectedSettingId" within the TabPage
            Label selectedSettingIdLabel = panel.Controls.OfType<Label>().FirstOrDefault(label => label.Name == "selectedSettingId");

            // Get the current setting from Master settings list and update its active state
            var settings = await GetSettings();

            var newMinerSettings = new MinerConfig();
            int check = 0;
            for (int x = 0; x < settings.MinerSettings.Count; x++)
            {
                // Deactivate the old, activate the new
                if (settings.MinerSettings[x].Id.ToString() == oldMinerSettingsId)
                {
                    settings.MinerSettings[x].Active = false;
                    check++;
                }
                else if (settings.MinerSettings[x].Id.ToString() == newMinerSettingsId)
                {
                    settings.MinerSettings[x].Active = true;
                    newMinerSettings = settings.MinerSettings[x];
                    check++;

                    // Update quick change dropdown
                    quickChangeSettings.SetTextThreadSafe(newMinerSettings.Name);
                }
            }

            if (check != 2) return;

            await AppSettings.SaveAsync<Settings>(SettingsForm.SETTINGSNAME, settings);

            // Stop this miner if it is running
            if (toggleButton != null && (string)toggleButton.GetTagThreadSafe() == "Stop")
            {
                // Simulate a click on the stop button
                toggleMinerButton_Click(toggleButton, EventArgs.Empty);
            }

            // Simulate a click on the play button
            toggleMinerButton_Click(toggleButton, EventArgs.Empty);

            // Update selectedSettingId
            selectedSettingIdLabel.SetTextThreadSafe(newMinerSettingsId);

            // Update tabpage
            tabPage.SetNameThreadSafe(newMinerSettingsId);
            tabPage.SetTextThreadSafe(newMinerSettings.Name);

            switchingMinerSettings = false;
        }
        public void ClickStartButton(string minerSettingsId)
        {
            // Get tab page with given miner settings id
            TabPage tabPage = outputPanel.Controls.OfType<TabControl>().SelectMany(tc => tc.TabPages.Cast<TabPage>()).FirstOrDefault(tp => tp.Name.Equals(minerSettingsId));

            if (tabPage == null)
            {
                // New Miner starting

                return;
            }

            Panel panel = tabPage.Controls.OfType<Panel>().FirstOrDefault();

            if (panel == null)
                return;

            // Get the stop button and click it if it is in the play state
            PictureBox toggleButton = panel.Controls.OfType<PictureBox>().FirstOrDefault(pb => (string)pb.Name == "toggleButton");

            if (toggleButton != null && (string)toggleButton.Tag == "Start")
            {
                // Simulate a click on the stop button
                toggleMinerButton_Click(toggleButton, EventArgs.Empty);
            }
        }
        public void ClickStopButton(string minerSettingsId)
        {
            // Get tab page with given miner settings id
            TabPage tabPage = outputPanel.Controls.OfType<TabControl>().SelectMany(tc => tc.TabPages.Cast<TabPage>()).FirstOrDefault(tp => tp.Name.Equals(minerSettingsId));

            if (tabPage == null)
                return;

            Panel panel = tabPage.Controls.OfType<Panel>().FirstOrDefault();

            if (panel == null)
                return;

            // Get the stop button and click it if it is in the play state
            PictureBox toggleButton = panel.Controls.OfType<PictureBox>().FirstOrDefault(pb => (string)pb.Name == "toggleButton");

            if (toggleButton != null && (string)toggleButton.Tag == "Stop")
            {
                // Simulate a click on the stop button
                toggleMinerButton_Click(toggleButton, EventArgs.Empty);
            }
        }
        public async void StartInactiveMiner(string minerSettingsId)
        {
            // Get the current setting from Master settings list and update its active state
            var settings = await GetSettings();

            for (int x = 0; x < settings.MinerSettings.Count; x++)
            {
                // Activate the setting
                if (settings.MinerSettings[x].Id.ToString() == minerSettingsId)
                {
                    settings.MinerSettings[x].Active = true;
                    await SaveSettings(settings);

                    CreateTabControlAndStartMiners(false);
                }
            }
        }
        public List<Pool> GetDefaultPools()
        {
            return settingsForm.GetDefaultPools();
        }
        
        #endregion


        // Individual miner actions
        private void toggleMinerButton_Click(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => toggleMinerButton_Click(sender, e)));
                return;
            }

            // Assuming sender is the stop button clicked
            PictureBox stopButton = (PictureBox)sender;

            // Get the parent control, which should be the TopPanel
            Control topPanel = stopButton.Parent;

            // Get the parent control of the TopPanel, which should be the TabPage
            TabPage tabPage = (TabPage)topPanel.Parent;

            // Access the RichTextBox within the TabPage
            RichTextBox textBox = tabPage.Controls.OfType<RichTextBox>().FirstOrDefault();

            // Access the ComboBox named "quickChangeSettings" within the TabPage
            ComboBox quickChangeSettings = topPanel?.Controls.OfType<ComboBox>().FirstOrDefault(c => c.Name == "quickChangeSettings");

            // Access the Label named "selectedSettingId" within the TabPage
            Label selectedSettingIdLabel = topPanel?.Controls.OfType<Label>().FirstOrDefault(label => label.Name == "selectedSettingId");
            string selectedSettingId = selectedSettingIdLabel.Text;

            MinerConfig selectedConfig = (MinerConfig)quickChangeSettings.SelectedItem;
            if ((string)stopButton.Tag == "Stop")
            {
                taskManager.StopTask(selectedSettingId);
                textBox.AppendTextThreadSafe("\n\n\n\n\n\nStopping Miner...");

                stopButton.Tag = "Start";
                stopButton.BackgroundImage = Properties.Resources.play_button;
            }
            else if ((string)stopButton.Tag == "Start")
            {
                 StartAMiner(selectedConfig, textBox);  
            }
        }
        private async void StartAMiner(MinerConfig minerConfig, RichTextBox textBox = null)
        {
            if (textBox != null && textBox.InvokeRequired)
            {
                textBox.BeginInvoke(new Action(() => StartAMiner(minerConfig, textBox)));
                return;
            }

            bool started = true;

            if (minerConfig.Redirect_Console_Output && textBox != null)
                started = await Task.Run(() => taskManager.StartTask(minerConfig, settingsForm.Settings.GlobalWorkerName, textBox));
            else
                started = await Task.Run(() => taskManager.StartTask(minerConfig, settingsForm.Settings.GlobalWorkerName));

            // Change stop button back to play button if it didn't start
            TabControl tabControl = outputPanel.Controls.Find("outputTabControl", true).FirstOrDefault() as TabControl;            
            TabPage tabPage = tabControl.TabPages.Cast<TabPage>().FirstOrDefault(tp => tp.Text.Equals(minerConfig.Name));
            PictureBox stopButton = tabPage?.Controls.OfType<PictureBox>().FirstOrDefault(c => c.Name == "toggleButton");


            // Change GUI
            if (!started)
            {
                if (stopButton != null)
                {
                    stopButton.Tag = "Start";
                    stopButton.BackgroundImage = Properties.Resources.play_button;
                }
            }
            else
            {
                if (stopButton != null)
                {
                    stopButton.Tag = "Stop";
                    stopButton.BackgroundImage = Properties.Resources.stop_button;
                }
            }
        }


        // Start Active Miners
        private async void CreateTabControlAndStartMiners(bool shortcutOnly = false)
        {
            if (outputPanel.InvokeRequired)
            {
                BeginInvoke(new Action(() => CreateTabControlAndStartMiners(shortcutOnly)));
                return;
            }

            // Get current tab control
            TabControl tabControl = outputPanel.Controls.Find("outputTabControl", true).FirstOrDefault() as TabControl;

            if (tabControl == null) // Or create a new tab control
            {
                tabControl = new TabControl();
                tabControl.Name = "outputTabControl";
                tabControl.Dock = DockStyle.Fill;
                tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

                // Create the "Home" tab page
                TabPage homeTabPage = new TabPage();
                homeTabPage.BackColor = Color.FromArgb(12, 20, 52);
                homeTabPage.Text = "Home";

                var arotatingPanel = RotatingPanel.Create();

                // Add image
                var stringLoaded = await AppSettings.LoadAsync<string>(SettingsForm.BGIMAGE);
                if (stringLoaded.Success)
                    arotatingPanel.Image = GetBgImage(stringLoaded.Result);
                else
                    rotatingPanel.Image = GetBgImage("Kas - Globe");

                homeTabPage.Controls.Add(arotatingPanel);
                //arotatingPanel.Start();                    

                homeTabPage.Controls.Add(arotatingPanel);
                tabControl.TabPages.Add(homeTabPage);
            }

            // Get all miner configs
            var settings = await GetSettings();
            var minerConfigs = settings.MinerSettings;
            if (minerConfigs == null) return;

            foreach (MinerConfig minerConfig in minerConfigs)
            {
                // If this config is active and we aren't using shortcut keys or we are using shortcut keys and this config also uses shortcut keys
                if (minerConfig.Active && !shortcutOnly || shortcutOnly && minerConfig.Use_Shortcut_Keys && minerConfig.Active)
                {
                    TabPage tabPage = tabControl.TabPages.Cast<TabPage>().FirstOrDefault(tp => tp.Text.Equals(minerConfig.Name));

                    RichTextBox tabPageRichTextBox = new RichTextBox();

                    // If this config doesn't have a tab page yet 
                    if (tabPage == null)
                    {
                        // Create one
                        tabPage = new TabPage();
                        tabPage.Name = minerConfig.Id.ToString();
                        tabPage.Text = minerConfig.Name;

                        #region Top Panel
                        // Top panel
                        Panel topPanel = new Panel();
                        topPanel.Dock = DockStyle.Top;
                        int padding = 10;
                        topPanel.Size = new Size(outputPanel.Width, restartsLabel.Height * 4 + padding);
                        topPanel.Padding = new Padding(padding, padding, 0, 0);

                        Label quickChangeLabel = new Label();
                        ComboBox quickChangeSettings = new ComboBox();
                        quickChangeSettings.Name = "quickChangeSettings";

                        Label selectedSettingId = new Label();
                        selectedSettingId.Name = "selectedSettingId";
                        selectedSettingId.Visible = false;
                        selectedSettingId.Text = minerConfig.Id.ToString();
                        topPanel.Controls.Add(selectedSettingId);

                        // Add stop button
                        PictureBox stopButton = new PictureBox();
                        stopButton.Name = "toggleButton";
                        stopButton.Location = new Point(outputPanel.Width - stopButton.Width, padding);
                        stopButton.Tag = "Stop";
                        stopButton.Cursor = Cursors.Hand;
                        stopButton.BackgroundImage = Properties.Resources.stop_button;
                        stopButton.BackgroundImageLayout = ImageLayout.Zoom;
                        stopButton.Click += toggleMinerButton_Click;
                        topPanel.Controls.Add(stopButton);


                        // Quick change settings
                        quickChangeLabel.Location = new Point(0, 10);
                        quickChangeLabel.Text = "Active Setting";
                        topPanel.Controls.Add(quickChangeLabel);

                        quickChangeSettings.Location = new Point(quickChangeLabel.Location.X + quickChangeLabel.Width, quickChangeLabel.Location.Y - 3);
                        foreach (MinerConfig config in minerConfigs)
                            quickChangeSettings.Items.Add(config);
                        quickChangeSettings.SelectedItem = minerConfig;

                        quickChangeSettings.GotFocus += async (sender, e) =>
                        {
                            MinerConfig selectedConfig = (MinerConfig)quickChangeSettings.SelectedItem;

                            // Ensure most up-to-date list is loaded for user to pick from
                            var loadedSettings = await AppSettings.LoadAsync<Settings>(SettingsForm.SETTINGSNAME);
                            if (loadedSettings.Success)
                            {
                                quickChangeSettings.Items.Clear();
                                var configs = loadedSettings.Result.MinerSettings;
                                foreach (MinerConfig config in configs)
                                    quickChangeSettings.Items.Add(config);
                                quickChangeSettings.SelectedItem = selectedConfig;
                            }
                        };

                        quickChangeSettings.SelectedIndexChanged += (sender, e) =>
                        {
                            MinerConfig selectedConfig = (MinerConfig)quickChangeSettings.SelectedItem;

                            // Only switch if user selects a different setting
                            if (selectedSettingId.Text != selectedConfig.Id.ToString())
                                SwitchActiveMinerSetting(selectedSettingId.Text, selectedConfig.Id.ToString());
                        };

                        topPanel.Controls.Add(quickChangeSettings);


                        // Set link to api stats
                        string url = "http://localhost:" + minerConfig.Api;

                        LinkLabel linkLabel = new LinkLabel();
                        linkLabel.Text = "Api Stats: " + url;
                        linkLabel.Dock = DockStyle.Top;
                        linkLabel.LinkClicked += (sender, e) =>
                        {
                            // Open the default web browser with the specified URL
                            Process.Start(url);
                        };

                        // Add api link
                        if (minerConfig.Api > 0)
                            topPanel.Controls.Add(linkLabel);

                        // Try to get pool link 1
                        string poolDomainName1 = minerConfig.GetPool1DomainName();
                        if (!string.IsNullOrWhiteSpace(poolDomainName1))
                        {
                            Pool pool = settingsForm.Settings.Pools.Find(p => p.Address.Contains(poolDomainName1));
                            topPanel.Controls.Add(CreatePoolLinkLabel(pool));
                        }

                        // Try to get pool link 2
                        string poolDomainName2 = minerConfig.GetPool2DomainName();
                        if (!string.IsNullOrWhiteSpace(poolDomainName2))
                        {
                            Pool pool = settingsForm.Settings.Pools.Find(p => p.Address.Contains(poolDomainName2));
                            CreatePoolLinkLabel(pool);
                            topPanel.Controls.Add(CreatePoolLinkLabel(pool));
                        }

                        // Try to get pool link 3
                        string poolDomainName3 = minerConfig.GetPool3DomainName();
                        if (!string.IsNullOrWhiteSpace(poolDomainName3))
                        {
                            Pool pool = settingsForm.Settings.Pools.Find(p => p.Address.Contains(poolDomainName3));
                            CreatePoolLinkLabel(pool);
                            topPanel.Controls.Add(CreatePoolLinkLabel(pool));
                        }

                        tabPage.Controls.Add(topPanel);
                        // End of Top Panel
                        #endregion

                        // Console output                    
                        tabPageRichTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                        tabPageRichTextBox.Location = new Point(0, topPanel.Height);
                        tabPageRichTextBox.Size = new Size(tabPage.Width - padding, tabPage.Height - topPanel.Height - padding);
                        tabPageRichTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
                        tabPageRichTextBox.ReadOnly = true;
                        tabPageRichTextBox.ForeColor = Color.White;
                        tabPageRichTextBox.BackColor = Color.Black;
                        tabPage.Controls.Add(tabPageRichTextBox);

                        tabPageRichTextBox.Text = "Starting miner. \n\nIt may take up to 5 minutes before you see any updates, depending on which miner you use. \n\nIf you don't have Redirect_Console_Output check marked in miner settings then you wont see any updates here.";

                        tabControl.TabPages.Add(tabPage);
                    }
                    else
                        continue;


                    // Start the miner 
                    StartAMiner(minerConfig, tabPageRichTextBox);
                }
            }


            // Add the TabControl if there isn't one yet
            // TODO only add it if this specific one isn't added yet
            if (!outputPanel.Controls.OfType<TabControl>().Any())
            {
                outputPanel.Invoke((MethodInvoker)delegate
                {
                    outputPanel.Controls.Add(tabControl);
                });
            }
        }
        private LinkLabel CreatePoolLinkLabel(Pool pool)
        {
            if (pool == null) return null; 

            LinkLabel linkLabel = new LinkLabel();

            var parts = pool.Link.Split('.');
            if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                linkLabel.Text = parts[0] + '.' + parts[1];

            linkLabel.Text = "Pool Link: " + linkLabel.Text;
            linkLabel.Dock = DockStyle.Top;
            linkLabel.LinkClicked += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(pool.Link) && Uri.IsWellFormedUriString(pool.Link, UriKind.Absolute))
                {
                    // Open the default web browser with the specified URL
                    Process.Start(pool.Link);
                }
                else if (!string.IsNullOrEmpty(pool.Link))
                    Process.Start(AddHttpsIfNeeded(pool.Link));
            };

            return linkLabel;
        }
        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tabControl = (TabControl)sender;

            // Check if there are any tab pages in the control
            if (tabControl.TabPages.Count > 0)
            {
                TabPage selectedTabPage = tabControl.SelectedTab;
                RichTextBox selectedRichTextBox = selectedTabPage.Controls.OfType<RichTextBox>().FirstOrDefault();

                if (selectedRichTextBox != null)
                {
                    // Inform user of the switch
                    selectedRichTextBox.SelectionColor = Color.White;
                    selectedRichTextBox.AppendText("\n\n----------- SWITCHED PAGE --- WAITING FOR NEW STATS -----------");
                    selectedRichTextBox.ScrollToCaret();
                }
            }
        }


        #region Notify Icon
        // Notify Icon
        private void CreateNotifyIcon()
        {
            // Create the notify icon
            notify_icon = new NotifyIcon();
            notify_icon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notify_icon.Text = "Gui Miner"; // Set the tooltip text
            notify_icon.Visible = true;

            notify_icon.MouseDoubleClick += (sender, e) =>
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.Show();
                    this.ShowInTaskbar = true;
                }
                else if (this.WindowState != FormWindowState.Minimized)
                    this.WindowState = FormWindowState.Minimized;                
            };

            // Add a context menu to the notify icon
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            // Create menu items with the desired text and image size
            int imageSize = 32;
            contextMenu.Items.Add(CreateMenuItem("Show Gui", Properties.Resources.icon, OnShowGui, imageSize));
            contextMenu.Items.Add(CreateMenuItem("Settings", Properties.Resources.settings, OnSettings, imageSize));
            contextMenu.Items.Add(CreateMenuItem("Start Mining", Properties.Resources.play_button, OnStartMining, imageSize));
            contextMenu.Items.Add(CreateMenuItem("Stop Mining", Properties.Resources.stop_button, OnStopMining, imageSize));
            contextMenu.Items.Add(CreateMenuItem("Check for Updates", Properties.Resources.check_for_updates, OnCheckForUpdates, imageSize));
            contextMenu.Items.Add(CreateMenuItem("Exit", Properties.Resources.exit_button, OnExit, imageSize));            

            // Set the image scaling to None to prevent automatic resizing
            contextMenu.ImageScalingSize = new Size(imageSize, imageSize); // Set the desired size

            notify_icon.ContextMenuStrip = contextMenu;
        }
        private ToolStripMenuItem CreateMenuItem(string text, Image image, EventHandler onClick, int imageSize)
        {
            var resizedImage = new Bitmap(image, new Size(imageSize, imageSize));
            var item = new ToolStripMenuItem(text, resizedImage, onClick);
            item.Font = new Font("Arial", 14, FontStyle.Regular);
            return item;
        }
        private void OnExit(object sender, EventArgs e)
        {
            CloseApp();
        }
        private async void OnStopMining(object sender, EventArgs e)
        {
            await ClickStopButton();
        }
        private void OnStartMining(object sender, EventArgs e)
        {
            ClickStartButton();
        }
        private void OnSettings(object sender, EventArgs e)
        {
            settingsForm.Show();
            settingsForm.Focus();
        }
        private void OnShowGui(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Show();
            this.Focus();
        }
        private void OnCheckForUpdates(object sender, EventArgs e)
        {
            settingsForm.CheckForUpdates();
        }
        #endregion


        #region Kill Miners
        // Kill Active Miners
        private bool KillAllRunningMiners(bool shortcutOnly = false)
        {
            var tasks = taskManager.GetRunningTasks().ToList();
            if (tasks != null && tasks.Count > 0)
            {
                // Kill all configs or only Use Shortcut Keys = true
                foreach (var taskDictItem in tasks)
                {
                    string minerId = taskDictItem.Key;

                    var matchingConfig = settingsForm.Settings.MinerSettings.Find(m => m.Id.ToString().Equals(minerId));
                    if (matchingConfig == null) return false;

                    // Stop the miner
                    taskManager.StopTask(matchingConfig.Id.ToString());

                    // Remove the tab page
                    RemoveTabPage(matchingConfig.Id.ToString());

                    // Kill all tasks with this miner name incase app crashed and didn't get closed
                    taskManager.KillAllProcessesContainingName(Path.GetFileNameWithoutExtension(matchingConfig.Miner_File_Path));
                }
            }

            
            return true;
        }
        private void killAllMinersButtonPictureBox_Click(object sender, EventArgs e)
        {
            Task.Run(() => KillAllRunningMiners());

            // Kill any miners that might have been started but never stopped from a crash/etc.
            taskManager.KillAllMiners(); 
        }
        private void RemoveTabPage(string minerId)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => RemoveTabPage(minerId)));
                return;
            }
            TabControl foundTabControl = outputPanel.Controls.OfType<TabControl>().FirstOrDefault();

            if (foundTabControl != null)
            {
                TabPage tabPageToRemove = foundTabControl.TabPages.OfType<TabPage>().FirstOrDefault(tabPage => tabPage.Name.Equals(minerId));

                if (tabPageToRemove != null)
                {
                    foundTabControl.TabPages.Remove(tabPageToRemove);
                }
            }
        }

        #endregion


        #region Helpers
        // Helpers
        internal Image GetBgImage(string bgImage)
        {
            Image image = null;

            if (bgImage == "Kas - Globe")
                image = Properties.Resources.kas_world;
            else if (bgImage == "Kaspa")
                image = Properties.Resources.kaspa;
            else if (bgImage == "Ergo")
                image = Properties.Resources.ergo;
            else if (bgImage == "Bitcoin")
                image = Properties.Resources.bitcoin;
            else if (bgImage == "Zilliqa")
                image = Properties.Resources.zilliqa;
            else
                image = Properties.Resources.bitcoin;

            return image;
        }
        static string AddHttpsIfNeeded(string url)
        {
            // Check if the URL starts with "https://" or "http://"
            if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                // If not, add "https://"
                url = "https://" + url;
            }

            return url;
        }

        // Rotating image
        private async void CreateRotatingPanel()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => CreateRotatingPanel()));
                return;
            }

            outputPanel.Controls.Clear();

            rotatingPanel = RotatingPanel.Create();

            // Add image
            var stringLoaded = await AppSettings.LoadAsync<string>(SettingsForm.BGIMAGE);
            if (stringLoaded.Success)
                rotatingPanel.Image = GetBgImage(stringLoaded.Result);
            else
                rotatingPanel.Image = GetBgImage("Kas - Globe");

            outputPanel.AddControlThreadSafe(rotatingPanel);

            //rotatingPanel.Start();
        }
        private void RemoveRotatingPanel()
        {
            if (rotatingPanel != null)
            {
                rotatingPanel.Dispose(); // Dispose of the rotating panel
                outputPanel.Controls.Remove(rotatingPanel); // Remove it from the outputPanel
                rotatingPanel = null; // Set the reference to null
            }
        }
        #endregion

    }

}




