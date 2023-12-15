using Gui_Miner.Classes;
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
        public TaskManager GetTaskManager() { return taskManager; }
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
            if(stringLoaded.Success)
                autoStart = bool.TryParse(stringLoaded.Result, out bool result) ? result : false;
            
            if (autoStart) ClickStartButton();
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
        internal void ClickStartButton(bool shortcutOnly = false)
        {
            if (!shortcutOnly)
            {
                // Play button pushed
                startButtonPictureBox.SetBackgroundImageThreadSafe(Properties.Resources.stop_button);
                startButtonPictureBox.SetTagThreadSafe("stop");

                // Remove background
                RemoveRotatingPanel();

                // Reset restart counter
                restartsLabel.Text = $"Restarts 0";
                restartsLabel.ForeColor = Color.White;
                restartsLabel.Show();
            }

            CreateTabControlAndStartMiners(shortcutOnly);
        }
        internal async Task<bool> ClickStopButton(bool shortcutOnly = false)
        {
            if (!shortcutOnly)
            {
                // Replace background
                CreateRotatingPanel();

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
        private void settingsButtonPictureBox_Click(object sender, EventArgs e)
        {
            settingsForm.Visible = true;
            settingsForm.Focus();
        }


        // Start Active Miners
        private async void CreateTabControlAndStartMiners(bool shortcutOnly = false)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => CreateTabControlAndStartMiners(shortcutOnly)));
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
                if(stringLoaded.Success)
                    arotatingPanel.Image = GetBgImage(stringLoaded.Result);
                else
                    rotatingPanel.Image = GetBgImage("Kas - Globe");

                homeTabPage.Controls.Add(arotatingPanel);
                arotatingPanel.Start();

                homeTabPage.Controls.Add(arotatingPanel);
                tabControl.TabPages.Add(homeTabPage);
            }

            // Get all miner configs
            var minerConfigs = settingsForm.Settings.MinerSettings;
            if (minerConfigs == null) return;
            
            foreach (MinerConfig minerConfig in minerConfigs)
            {
                // If this config is active and we aren't using shortcut keys or we are using shortcut keys and this config also uses shortcut keys
                if (minerConfig.Active && !shortcutOnly || shortcutOnly && minerConfig.Use_Shortcut_Keys)
                {
                    TabPage tabPage = tabControl.TabPages.Cast<TabPage>().FirstOrDefault(tp => tp.Name.Equals(minerConfig.Name));

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
                        Label selectedSettingId = new Label();

                        selectedSettingId.Visible = false;
                        selectedSettingId.Text = minerConfig.Id.ToString();
                        topPanel.Controls.Add(selectedSettingId);

                        // Add stop button
                        PictureBox stopButton = new PictureBox();
                        stopButton.Location = new Point(outputPanel.Width - stopButton.Width, padding);
                        stopButton.Tag = "Stop";
                        stopButton.Cursor = Cursors.Hand;
                        stopButton.BackgroundImage = Properties.Resources.stop_button;
                        stopButton.BackgroundImageLayout = ImageLayout.Zoom;
                        stopButton.Click += (sender, e) =>
                        {
                            MinerConfig selectedConfig = (MinerConfig)quickChangeSettings.SelectedItem;
                            if ((string)stopButton.Tag == "Stop")
                            {
                                taskManager.StopTask(selectedSettingId.Text);
                                tabPageRichTextBox.AppendTextThreadSafe("\n\n\n\n\n\nStopping Miner...");

                                stopButton.Tag = "Start";
                                stopButton.BackgroundImage = Properties.Resources.play_button;
                            }
                            else if ((string)stopButton.Tag == "Start")
                            {
                                if (selectedConfig.Redirect_Console_Output)
                                    taskManager.StartTask(selectedConfig.Miner_File_Path, selectedConfig.Bat_File_Arguments, selectedConfig.Run_As_Admin, selectedConfig.Id.ToString(), tabPageRichTextBox);
                                else
                                    taskManager.StartTask(selectedConfig.Miner_File_Path, selectedConfig.Bat_File_Arguments, selectedConfig.Run_As_Admin, selectedConfig.Id.ToString());

                                stopButton.Tag = "Stop";
                                stopButton.BackgroundImage = Properties.Resources.stop_button;
                            }
                        };
                        topPanel.Controls.Add(stopButton);


                        // Quick change settings
                        quickChangeLabel.Location = new Point(0, 10);
                        quickChangeLabel.Text = "Active Setting";
                        topPanel.Controls.Add(quickChangeLabel);

                        quickChangeSettings.Location = new Point(quickChangeLabel.Location.X + quickChangeLabel.Width, quickChangeLabel.Location.Y - 3);
                        foreach (MinerConfig config in minerConfigs)
                            quickChangeSettings.Items.Add(config);
                        quickChangeSettings.SelectedItem = minerConfig;

                        quickChangeSettings.SelectedIndexChanged += async (sender, e) =>
                        {
                            MinerConfig selectedConfig = (MinerConfig)quickChangeSettings.SelectedItem;

                            // Get the current setting from Master settings list and update its active state

                            var settings = new Settings();
                            var settingsLoaded = await AppSettings.LoadAsync<Settings>(SettingsForm.SETTINGSNAME);
                            if (settingsLoaded.Success)
                                settings = settingsLoaded.Result;

                            for (int x = 0; x < settings.MinerSettings.Count; x++)
                            {
                                // Deactivate the old, activate the new
                                if (settings.MinerSettings[x].Id == minerConfig.Id)
                                    settings.MinerSettings[x].Active = false;
                                else if (settings.MinerSettings[x].Id == selectedConfig.Id)
                                    settings.MinerSettings[x].Active = true;
                            }
                            await AppSettings.SaveAsync<Settings>(SettingsForm.SETTINGSNAME, settings);

                            // Stop this miner setting
                            if ((string)stopButton.Tag == "Stop")
                            {
                                taskManager.StopTask(selectedSettingId.Text);
                                tabPageRichTextBox.AppendTextThreadSafe("\n\n\n\n\n\nStopping Miner...");

                                stopButton.Tag = "Start";
                                stopButton.BackgroundImage = Properties.Resources.play_button;
                            }

                            // Start the new miner setting
                            if (selectedConfig.Redirect_Console_Output)
                                taskManager.StartTask(selectedConfig.Miner_File_Path, selectedConfig.Bat_File_Arguments, selectedConfig.Run_As_Admin, selectedConfig.Id.ToString(), tabPageRichTextBox);
                            else
                                taskManager.StartTask(selectedConfig.Miner_File_Path, selectedConfig.Bat_File_Arguments, selectedConfig.Run_As_Admin, selectedConfig.Id.ToString());

                            stopButton.Tag = "Stop";
                            stopButton.BackgroundImage = Properties.Resources.stop_button;

                            // Update selectedSettingId
                            selectedSettingId.Text = selectedConfig.Id.ToString();
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

                        tabPageRichTextBox.Text = "Starting miner. It may take up to 5 minutes before you see any updates, depending on which miner you use. If you don't have Redirect_Console_Output check marked in miner settings then you wont see any updates here.";

                        tabControl.TabPages.Add(tabPage);
                    }
                    else
                        tabPageRichTextBox = tabPage.Controls.OfType<RichTextBox>().FirstOrDefault();

                    // Start the miner 
                    if(minerConfig.Redirect_Console_Output)
                        taskManager.StartTask(minerConfig.Miner_File_Path, minerConfig.Bat_File_Arguments, minerConfig.Run_As_Admin, minerConfig.Id.ToString(), tabPageRichTextBox);                       
                    else
                        taskManager.StartTask(minerConfig.Miner_File_Path, minerConfig.Bat_File_Arguments, minerConfig.Run_As_Admin, minerConfig.Id.ToString());
                }
            }

            // Add the TabControl if there isn't one yet
            if (!outputPanel.Controls.OfType<TabControl>().Any())
            {
                if (InvokeRequired)
                    outputPanel.Invoke(new Action(() => outputPanel.Controls.Add(tabControl)));                
                else
                    outputPanel.Controls.Add(tabControl);                
            }            
        }
        private LinkLabel CreatePoolLinkLabel(Pool pool)
        {
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

                    if (!shortcutOnly || (shortcutOnly && matchingConfig.Use_Shortcut_Keys))
                    {
                        // Stop the miner
                        taskManager.StopTask(matchingConfig.Id.ToString());

                        // Remove the tab page
                        RemoveTabPage(matchingConfig.Id.ToString());
                    }
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
            outputPanel.Controls.Clear();

            rotatingPanel = RotatingPanel.Create();

            // Add image
            var stringLoaded = await AppSettings.LoadAsync<string>(SettingsForm.BGIMAGE);
            if (stringLoaded.Success)
                rotatingPanel.Image = GetBgImage(stringLoaded.Result);
            else
                rotatingPanel.Image = GetBgImage("Kas - Globe");

            outputPanel.Controls.Add(rotatingPanel);

            rotatingPanel.Start();
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




