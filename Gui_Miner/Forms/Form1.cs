using Gui_Miner.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using Application = System.Windows.Forms.Application;
using Image = System.Drawing.Image;
using Timer = System.Windows.Forms.Timer;

namespace Gui_Miner
{
    public partial class Form1 : Form
    {
        NotifyIcon notify_icon;
        SettingsForm settingsForm = new SettingsForm();
        internal RotatingPanel rotatingPanel;
        Dictionary<int,int> runningTasks = new Dictionary<int, int>();
        CancellationTokenSource ctsRunningMiners = new CancellationTokenSource();
        private GlobalKeyboardHook globalKeyboardHook = new GlobalKeyboardHook();
        public Form1()
        {
            InitializeComponent();

            KillAllRunningMiners();

            // Create notify icon
            CreateNotifyIcon();

            // Create Settings Form
            settingsForm.Show();
            settingsForm.MainForm = this;
            settingsForm.Visible = false;
            settingsForm.Form1 = this;

            outputPanel.BackgroundImage = null;

            // Rotating panel
            CreateRotatingPanel();

            // Listen for short-cut keys
            LoadShortcutKeys();
        }
        public void LoadShortcutKeys()
        {
            // Run the entire method on a background thread
            Task.Run(() =>
            {
                globalKeyboardHook.SetMainForm(this);

                var stopShortKeys = AppSettings.Load<List<Keys>>(SettingsForm.STOPSHORTKEYS);
                var startShortKeys = AppSettings.Load<List<Keys>>(SettingsForm.STARTSHORTKEYS);

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
                Invoke((MethodInvoker)delegate
                {
                    globalKeyboardHook.Start();
                });
            });
        }



        // Rotate image
        private void CreateRotatingPanel()
        {
            outputPanel.Controls.Clear();

            rotatingPanel = RotatingPanel.Create();

            // Add image
            string bgImage = AppSettings.Load<string>(SettingsForm.BGIMAGE);
            rotatingPanel.Image = GetBgImage(bgImage);

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


        // Load/Close Form
        private void Form1_Load(object sender, EventArgs e)
        {
            // Load last window location
            var lastLocation = AppSettings.Load<Point>("windowLocation");
            if (lastLocation != null)
                this.Location = lastLocation;

            startButtonPictureBox.Tag = "play";

            // Auto start
            bool autoStart = bool.TryParse(AppSettings.Load<string>(SettingsForm.AUTOSTARTMINING), out bool result) ? result : false;
            if (autoStart) ClickStartButton();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Close all running miners
            KillAllRunningMiners();

            // Save window location
            AppSettings.Save<Point>("windowLocation", this.Location);
        }
        internal void CloseApp()
        {
            // Hide main form and remove notify icon
            this.Visible = false;
            notify_icon.Dispose();

            // Close all running miners
            Task.Run(() => KillAllRunningMiners());

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
        private void startButtonPictureBox_Click(object sender, EventArgs e)
        {
            // Disable the button immediately
            startButtonPictureBox.Enabled = false;

            if ((string)startButtonPictureBox.Tag == "play")
            {
                ClickStartButton();
            }
            else if ((string)startButtonPictureBox.Tag == "stop")
            {
                ClickStopButton();
            }
            startButtonPictureBox.Enabled = true;
        }
        internal void ClickStartButton(bool shortcutOnly = false)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClickStartButton(shortcutOnly)));
                return;
            }

            if (!shortcutOnly)
            {
                // Play button pushed
                startButtonPictureBox.BackgroundImage = Properties.Resources.stop_button;
                startButtonPictureBox.Tag = "stop";

                // Remove background
                RemoveRotatingPanel();

                // Reset restart counter
                restartsLabel.Text = $"Restarts 0";
                restartsLabel.ForeColor = Color.White;
                restartsLabel.Show();
            }

            CreateTabControlAndStartMiners(shortcutOnly);
        }
        internal async void ClickStopButton(bool shortcutOnly = false)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClickStopButton(shortcutOnly)));
                return;
            }

            if (!shortcutOnly)
            {
                // Stop button pushed
                startButtonPictureBox.BackgroundImage = Properties.Resources.play_button;
                startButtonPictureBox.Tag = "play";

                // Replace background
                CreateRotatingPanel();

                // Reset restart counter
                restartsLabel.Text = $"Restarts 0";
                restartsLabel.Hide();
            }

            // Close all running miners
            await Task.Run(() => KillAllRunningMiners(shortcutOnly));
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
                string bgImage = AppSettings.Load<string>(SettingsForm.BGIMAGE);
                arotatingPanel.Image = GetBgImage(bgImage);

                homeTabPage.Controls.Add(arotatingPanel);
                arotatingPanel.Start();

                homeTabPage.Controls.Add(arotatingPanel);
                tabControl.TabPages.Add(homeTabPage);
            }

            // Get all miner configs
            var minerConfigs = settingsForm.Settings.MinerSettings;

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

                        // Add stop button
                        PictureBox stopButton = new PictureBox();
                        stopButton.Location = new Point(outputPanel.Width - stopButton.Width, padding);
                        stopButton.Tag = "Stop";
                        stopButton.Cursor = Cursors.Hand;
                        stopButton.BackgroundImage = Properties.Resources.stop_button;
                        stopButton.BackgroundImageLayout = ImageLayout.Zoom;
                        stopButton.Click += (sender, e) =>
                        {
                            PictureBox button = (PictureBox)sender;
                            if ((string)button.Tag == "Stop")
                            {
                                KillMinerById(minerConfig.Id);
                                tabPageRichTextBox.SetTextThreadSafe("");

                                button.Tag = "Start";
                                stopButton.BackgroundImage = Properties.Resources.play_button;
                            }
                            else if ((string)button.Tag == "Start")
                            {
                                StartMinerById(minerConfig.Id, tabPageRichTextBox);
                                button.Tag = "Stop";
                                stopButton.BackgroundImage = Properties.Resources.stop_button;
                            }
                        };
                        topPanel.Controls.Add(stopButton);

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

                        // Add link
                        if (minerConfig.Api > 0)
                            topPanel.Controls.Add(linkLabel);

                        // Try to get pool link 1
                        string poolDomainName1 = minerConfig.GetPool1DomainName();
                        string poolLink1 = settingsForm.Settings.Pools.Find(p => p.Address.Contains(poolDomainName1)).Link;

                        LinkLabel poolLinkLabel1 = new LinkLabel();
                        string poolLinkText = poolLink1;
                        var poolLinkTextParts = poolLink1.Split('.');
                        if (poolLinkTextParts.Length > 0 && !string.IsNullOrWhiteSpace(poolLinkTextParts[0]))
                            poolLinkText = poolLinkTextParts[0] + '.' + poolLinkTextParts[1];
                        poolLinkLabel1.Text = "Pool Link1: " + poolLinkText;
                        poolLinkLabel1.Dock = DockStyle.Top;
                        poolLinkLabel1.LinkClicked += (sender, e) =>
                        {
                            if (!string.IsNullOrEmpty(poolLink1) && Uri.IsWellFormedUriString(poolLink1, UriKind.Absolute))
                            {
                                // Open the default web browser with the specified URL
                                Process.Start(AddHttpsIfNeeded(poolLink1));
                            }
                        };

                        // Try to get pool link 2
                        string poolDomainName2 = minerConfig.GetPool2DomainName();
                        string poolLink2 = settingsForm.Settings.Pools.Find(p => p.Address.Contains(poolDomainName2)).Link;
                        poolLinkText = poolLink2;
                        poolLinkTextParts = poolLink2.Split('.');
                        if (poolLinkTextParts.Length > 0)
                            poolLinkText = poolLinkTextParts[0] + '.' + poolLinkTextParts[1];
                        LinkLabel poolLinkLabel2 = new LinkLabel();
                        poolLinkLabel2.Text = "Pool Link2: " + poolLinkText;
                        poolLinkLabel2.Dock = DockStyle.Top;
                        poolLinkLabel2.LinkClicked += (sender, e) =>
                        {
                            if (!string.IsNullOrEmpty(poolLink2) && Uri.IsWellFormedUriString(poolLink2, UriKind.Absolute))
                            {
                                // Open the default web browser with the specified URL
                                Process.Start(AddHttpsIfNeeded(poolLink2));
                            }
                        };

                        // Try to get pool link 3
                        string poolDomainName3 = minerConfig.GetPool3DomainName();
                        string poolLink3 = settingsForm.Settings.Pools.Find(p => p.Address.Contains(poolDomainName3)).Link;
                        poolLinkText = poolLink3;
                        poolLinkTextParts = poolLink3.Split('.');
                        if (poolLinkTextParts.Length > 0)
                            poolLinkText = poolLinkTextParts[0] + '.' + poolLinkTextParts[1];
                        LinkLabel poolLinkLabel3 = new LinkLabel();
                        poolLinkLabel3.Text = "Pool Link3: " + poolLinkText;
                        poolLinkLabel3.Dock = DockStyle.Top;
                        poolLinkLabel3.LinkClicked += (sender, e) =>
                        {
                            if (!string.IsNullOrEmpty(poolLink3) && Uri.IsWellFormedUriString(poolLink3, UriKind.Absolute))
                            {
                                // Open the default web browser with the specified URL
                                Process.Start(AddHttpsIfNeeded(poolLink3));
                            }
                        };

                        // Add pool links if they're not empty
                        if (!string.IsNullOrWhiteSpace(poolLink1))
                            topPanel.Controls.Add(poolLinkLabel1);

                        if (!string.IsNullOrWhiteSpace(poolLink2))
                            topPanel.Controls.Add(poolLinkLabel2);

                        if (!string.IsNullOrWhiteSpace(poolLink3))
                            topPanel.Controls.Add(poolLinkLabel3);

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

                        tabControl.TabPages.Add(tabPage);
                    }
                    else
                        tabPageRichTextBox = tabPage.Controls.OfType<RichTextBox>().FirstOrDefault();

                    // Start the miner in a separate thread with the miner-specific RichTextBox
                    int processId = await Task.Run(() =>
                    {
                        return StartMiner(minerConfig.Miner_File_Path, minerConfig.Bat_File_Arguments, minerConfig.Run_As_Admin, tabPageRichTextBox, ctsRunningMiners.Token);
                    });

                    if (!runningTasks.ContainsKey(minerConfig.Id))
                        runningTasks.Add(minerConfig.Id, processId);
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
        private int StartMiner(string filePath, string arguments, bool runAsAdmin, RichTextBox richTextBox, CancellationToken token)
        {
            int procId = -1;
            if (InvokeRequired)
            {
                return (int)Invoke(new Func<int>(() => StartMiner(filePath, arguments, runAsAdmin, richTextBox, token)));
            }

            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Unable to locate miner at {filePath}");
                return procId;
            }

            richTextBox.AppendTextThreadSafe("\nSTARTING MINER...");
            richTextBox.ForeColorSetThreadSafe(Color.FromArgb((int)(0.53 * 255), 58, 221, 190));

            // Create a new process
            Process process = new Process();

            string verb = "";
            if (runAsAdmin)
            {
                verb = "runas";

                IsRunningAsAdmin();
            }

            // Set the process start information
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = verb
            };
            process.StartInfo = startInfo;

            // Subscribe to the OutputDataReceived event
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data) && !token.IsCancellationRequested)
                {
                     UpdateOutputConsole(e.Data, richTextBox);
                };
            };

            try
            {
                // Start the process
                process.Start();
                procId = process.Id;

                Task.Run(() =>
                {
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
                richTextBox.AppendTextThreadSafe(Environment.NewLine + "Error starting miner " + ex.Message);
            }

            return procId;
        }
        private void UpdateOutputConsole(string output, RichTextBox richTextBox)
        {
            if (output.ToLower().Contains("failed") || output.ToLower().Contains("error") || output.ToLower().Contains("miner terminated"))
            {
                richTextBox.SelectionStartThreadSafe(richTextBox.TextLengthGetThreadSafe());
                richTextBox.SelectionLengthThreadSafe(0);
                richTextBox.SelectionColorThreadSafe(Color.Red);
                richTextBox.AppendTextThreadSafe(Environment.NewLine + output);
                richTextBox.SelectionColorThreadSafe(richTextBox.ForeColorGetThreadSafe()); // Reset to the default color                                    

                // Errors
                if (output.ToLower().Contains("error on gpu"))
                {
                    // Restart miner
                    richTextBox.AppendTextThreadSafe("Gpu failed. Restarting...");
                    ClickStopButton();
                    Thread.Sleep(3500);
                    ClickStartButton();

                    // Extract the number from the restarts label text
                    int restarts;
                    if (int.TryParse(Regex.Match(restartsLabel.Text, @"\d+").Value, out restarts))
                    {
                        restarts++;

                        // Update the label text with the new number
                        restartsLabel.TextThreadSafe($"Restarts {restarts}");
                        restartsLabel.ForeColorThreadSafe(Color.Red);
                        restartsLabel.ShowThreadSafe();
                    }
                }
            }
            else if (output.ToLower().Contains("accepted"))
            {
                richTextBox.SelectionStartThreadSafe(richTextBox.TextLengthGetThreadSafe());
                richTextBox.SelectionLengthThreadSafe(0);
                richTextBox.SelectionColorThreadSafe(Color.ForestGreen);
                richTextBox.AppendTextThreadSafe(Environment.NewLine + output);
                richTextBox.SelectionColorThreadSafe(richTextBox.ForeColorGetThreadSafe()); // Reset to the default color  
            }
            else
            {

                richTextBox.SelectionStartThreadSafe(richTextBox.TextLengthGetThreadSafe());
                richTextBox.SelectionLengthThreadSafe(0);

                // Check if output starts with a number
                if (output.Length > 0 && char.IsDigit(output[0]))
                {
                    richTextBox.SelectionColorThreadSafe(Color.Yellow);
                }
                else
                {
                    richTextBox.SelectionColorThreadSafe(richTextBox.ForeColorGetThreadSafe()); // Reset to the default color
                }

                richTextBox.AppendTextThreadSafe(Environment.NewLine + output);

            }
            richTextBox.ScrollToCaretThreadSafe();
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
                    selectedRichTextBox.AppendText("\n\n----------- SWITCHED MINERS...WAITING FOR NEW STATS... -----------");
                    selectedRichTextBox.ScrollToCaret();
                }
            }
        }


        // Helpers
        public bool IsRunningAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!isAdmin)
            {
                DialogResult result = MessageBox.Show("This miner setting requires admin privliges. Do you want to restart the app as admin?", "Restart App as Admin?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                // Check the user's response
                if (result == DialogResult.Yes)
                {
                    string version = "0.0";
                    string updateProjectPath = settingsForm.GetUpdateAppPath();
                    string command = "runas";

                    // Create a process start info
                    ProcessStartInfo restartInfo = new ProcessStartInfo(updateProjectPath);
                    restartInfo.Verb = "runas";
                    restartInfo.Arguments = $"-{command} -{version} -{true}";

                    // Start the "update" project as a separate process
                    try
                    {
                        Process.Start(restartInfo);

                        CloseApp();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error restarting app as admin " + ex.Message);
                    }
                }
            }

            return isAdmin;
        }
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


        #region Notify Icon
        // Notify Icon
        private void CreateNotifyIcon()
        {
            // Create the notify icon
            notify_icon = new NotifyIcon();
            notify_icon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notify_icon.Text = "Gui Miner"; // Set the tooltip text
            notify_icon.Visible = true;

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
        private void OnStopMining(object sender, EventArgs e)
        {
            ClickStopButton();
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
        private void KillAllRunningMiners(bool shortcutOnly = false)
        {
            var tasks = runningTasks;
            if (tasks != null && tasks.Count > 0)
            {
                // Kill all configs or only Use Shortcut Keys = true
                foreach (var taskDictItem in tasks)
                {
                    int procId = taskDictItem.Value;
                    int minerId = taskDictItem.Key;

                    var matchingConfig = settingsForm.Settings.MinerSettings.Find(m => m.Id.Equals(minerId));
                    if (matchingConfig == null) return;

                    if (!shortcutOnly || (shortcutOnly && matchingConfig.Use_Shortcut_Keys))
                    {
                        try
                        {
                            // Kill process
                            Process process = Process.GetProcesses().ToList().Find(p => p.Id.Equals(procId));
                            if (process != null)
                            {
                                process.Kill();

                                // Use PowerShell to find and kill the associated cmd window
                                string command = $"Get-WmiObject Win32_Process | Where-Object {{ $_.ParentProcessId -eq {process.Id} }} | ForEach-Object {{ $_.Terminate() }}";
                                RunPowerShellCommand(command);
                            }
                        }
                        catch (AggregateException)
                        {
                            // Handle any exceptions if needed
                        }

                        // Remove the tab page
                        RemoveTabPage(matchingConfig.Id.ToString());

                        // Remove from runningTasks
                        runningTasks.Remove(procId);
                    }

                }
            }
            // Kill all known miners
            if (!shortcutOnly && settingsForm.Settings.MinerSettings != null)
            {
                var minerConfigs = new List<MinerConfig>(settingsForm.Settings.MinerSettings);
                foreach (MinerConfig minerConfig in minerConfigs)
                {
                    if (!string.IsNullOrWhiteSpace(minerConfig.Miner_File_Path))
                    {
                        KillProcessByName(Path.GetFileNameWithoutExtension(minerConfig.Miner_File_Path));
                    }
                }
            }
        }
        private void KillMinerById(int id, bool removeTabPage = false)
        {
            if (runningTasks.ContainsKey(id))
            {
                var procId = runningTasks[id];

                var matchingConfig = settingsForm.Settings.MinerSettings.Find(m => m.Id.Equals(id));
                if (matchingConfig == null) return;

                if (removeTabPage)
                    RemoveTabPage(matchingConfig.Id.ToString());// Remove the tab page                

                // If no more miners running change the play/stop button back to play
                if (runningTasks.Count == 0)
                    ClickStopButton();

                // Kill the process
                try
                {
                    var runningProc = Process.GetProcessById(procId);
                    runningProc.Kill();

                    // Use PowerShell to find and kill the associated cmd window
                    string command = $"Get-WmiObject Win32_Process | Where-Object {{ $_.ParentProcessId -eq {procId} }} | ForEach-Object {{ $_.Terminate() }}";
                    RunPowerShellCommand(command);
                }
                catch { }

                // Remove task
                runningTasks.Remove(id);
            }
        }
        private async void StartMinerById(int id, RichTextBox richTextBox)
        {
            // If running task already exists, kill it
            KillMinerById(id);

            MinerConfig minerConfig = settingsForm.Settings.MinerSettings.Find(m => m.Id.Equals(id));
            if (minerConfig == null) return;

            // Create a CancellationTokenSource to cancel tasks
            ctsRunningMiners = new CancellationTokenSource();

            // Start the miner in a separate thread with the miner-specific RichTextBox
            int processId = await Task.Run(() =>
            {
                return StartMiner(minerConfig.Miner_File_Path, minerConfig.Bat_File_Arguments, minerConfig.Run_As_Admin, richTextBox, ctsRunningMiners.Token);
            });

            if (!runningTasks.ContainsKey(minerConfig.Id))
                runningTasks.Add(minerConfig.Id, processId);
        }
        private void KillProcessByName(string name)
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                try
                {
                    if (process.ProcessName == Path.GetFileNameWithoutExtension(name)
                        || process.ProcessName == "miner")
                    {
                        // Kill the process
                        process.Kill();

                        // Use PowerShell to find and kill the associated cmd window
                        string command = $"Get-WmiObject Win32_Process | Where-Object {{ $_.ParentProcessId -eq {process.Id} }} | ForEach-Object {{ $_.Terminate() }}";
                        RunPowerShellCommand(command);
                    }
                }
                catch { }
            }
        }
        static bool RunPowerShellCommand(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy ByPass -Command {command}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
            };

            using (Process powershell = new Process())
            {
                powershell.StartInfo = psi;
                powershell.Start();
                string output = powershell.StandardOutput.ReadToEnd();
                powershell.WaitForExit();

                return powershell.ExitCode == 0 && !output.Contains("Error") && !output.Contains("error");
            }
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


    }

}




