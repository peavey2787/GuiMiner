using Gui_Miner.Classes;
using Newtonsoft.Json.Linq;
using System;
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
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
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
        List<Task> runningTasks = new List<Task>();
        CancellationTokenSource ctsRunningMiners = new CancellationTokenSource();
        private GlobalKeyboardHook globalKeyboardHook = new GlobalKeyboardHook(); 
        public Form1()
        {
            InitializeComponent();

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

            globalKeyboardHook.SetMainForm(this);

            // Listen for short-cut keys
            UpdateShortcutKeys();
 
        }
        public void UpdateShortcutKeys()
        {
            var stopShortKeys = AppSettings.Load<List<Keys>>(SettingsForm.STOPSHORTKEYS);
            var startShortKeys = AppSettings.Load<List<Keys>>(SettingsForm.STARTSHORTKEYS);

            if (stopShortKeys != null && startShortKeys != null)
            {
                globalKeyboardHook.SetStartKeys(startShortKeys.ToArray());
                globalKeyboardHook.SetStopKeys(stopShortKeys.ToArray());
            }
            else if(startShortKeys != null)
            {
                globalKeyboardHook.SetStartKeys(startShortKeys.ToArray());
            }
            else if(stopShortKeys != null)
            {
                globalKeyboardHook.SetStopKeys(stopShortKeys.ToArray());
            }

        }

        internal Image GetBgImage(string  bgImage)
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
            ClickStopButton();

            // Save window location
            AppSettings.Save<Point>("windowLocation", this.Location);
        }
        private void CloseApp()
        {
            // Hide main form and remove notify icon
            this.Visible = false;
            notify_icon.Dispose();

            // Stop mining
            ClickStopButton();

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
            if (startButtonPictureBox.Tag == "play")
            {
                ClickStartButton();
            }
            else if (startButtonPictureBox.Tag == "stop")
            {
                ClickStopButton();
            }
        }
        internal async void ClickStartButton()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ClickStartButton));
                return;
            }

            await CancelAllTasksAsync();
            Task.Run(() => { KillAllActiveMiners(); });

            // Play button pushed
            startButtonPictureBox.BackgroundImage = Properties.Resources.stop_button;
            startButtonPictureBox.Tag = "stop";

            // Remove background
            RemoveRotatingPanel();
            outputPanel.Controls.Clear();

            // Reset restart counter
            restartsLabel.Text = $"Restarts 0";
            restartsLabel.Show();

            CreateTabControlAndStartMiners();
        }
        internal async void ClickStopButton()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ClickStopButton));
                return;
            }

            // Stop button pushed
            startButtonPictureBox.BackgroundImage = Properties.Resources.play_button;
            startButtonPictureBox.Tag = "play";

            // Replace background
            CreateRotatingPanel();

            // Reset restart counter
            restartsLabel.Text = $"Restarts 0";
            restartsLabel.Hide();

            await Task.Run(() => { KillAllActiveMiners(); });
        }
        private void settingsButtonPictureBox_Click(object sender, EventArgs e)
        {
            settingsForm.Visible = true;
            settingsForm.Focus();
        }




        // Start Active Miners
        private void CreateTabControlAndStartMiners()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)CreateTabControlAndStartMiners);
            }

            TabControl tabControl = new TabControl();
            tabControl.Name = "outputTabControl";
            tabControl.Dock = DockStyle.Fill;

            // Create the "Home" tab page
            TabPage homeTabPage = new TabPage();
            homeTabPage.BackColor = Color.FromArgb(12, 20, 52);
            homeTabPage.Text = "Home";

            var arotatingPanel = RotatingPanel.Create();

            // Add image
            string bgImage = AppSettings.Load<string>(SettingsForm.BGIMAGE);
            arotatingPanel.Image = GetBgImage(bgImage);

            outputPanel.Controls.Add(arotatingPanel);
            arotatingPanel.Start();

            homeTabPage.Controls.Add(arotatingPanel);
            tabControl.TabPages.Add(homeTabPage);

            foreach (MinerConfig minerConfig in settingsForm.Settings.MinerSettings)
            {                
                if (minerConfig.Active)
                {
                    TabPage tabPage = new TabPage();
                    tabPage.Text = minerConfig.Current_Miner_Config_Type.ToString();

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
                    if(minerConfig.Api > 0)
                        tabPage.Controls.Add(linkLabel);

                    // Try to get pool link
                    string poolDomainName1 = minerConfig.GetPool1DomainName();
                    string poolLink1 = "";
                    if (poolDomainName1 != null)
                    {
                        // Get matching pool and return its link
                        if (settingsForm.Settings.Pools != null && settingsForm.Settings.Pools.Count > 0)
                        {
                            foreach (Pool pool in settingsForm.Settings.Pools)
                            {
                                if (pool.Address.Contains(poolDomainName1) || pool.Link.Contains(poolDomainName1))
                                {
                                    poolLink1 = pool.Link;
                                }
                            }
                        }

                    }

                    // Set link to pool
                    LinkLabel poolLinkLabel = new LinkLabel();
                    poolLinkLabel.Text = "Pool Link: " + poolLink1;
                    poolLinkLabel.Dock = DockStyle.Top;
                    poolLinkLabel.LinkClicked += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(poolLink1) && Uri.IsWellFormedUriString(poolLink1, UriKind.Absolute))
                        {
                            // Open the default web browser with the specified URL
                            Process.Start(poolLink1);
                        }
                    };
                    
                    if(!string.IsNullOrWhiteSpace(poolLink1))
                        tabPage.Controls.Add(poolLinkLabel);


                    RichTextBox tabPageRichTextBox = new RichTextBox();
                    tabPageRichTextBox.Dock = DockStyle.Fill;
                    tabPageRichTextBox.ReadOnly = true;
                    tabPageRichTextBox.ForeColor = Color.White;
                    tabPageRichTextBox.BackColor = Color.Black;
                    tabPage.Controls.Add(tabPageRichTextBox);

                    tabControl.TabPages.Add(tabPage);

                    // Start the miner in a separate thread with the miner-specific RichTextBox
                    Task minerTask = Task.Run(() => StartMiner(minerConfig.Miner_File_Path, minerConfig.Bat_File_Arguments, minerConfig.Run_As_Admin, tabPageRichTextBox, ctsRunningMiners.Token));

                    runningTasks.Add(minerTask);
                }
            }

            // Clear previous tab control
            outputPanel.Controls.Clear();
            
            // Add the TabControl
            outputPanel.Controls.Add(tabControl);
        }
        private void StartMiner(string filePath, string arguments, bool runAsAdmin, RichTextBox richTextBox, CancellationToken token)
        {
            if (File.Exists(filePath))
            {
                richTextBox.AppendTextThreadSafe("STARTING MINER...");
                richTextBox.ForeColorThreadSafe(Color.FromArgb((int)(0.53 * 255), 58, 221, 190));

                // Create a new process
                Process process = new Process();

                string verb = "";
                if (runAsAdmin)
                {
                    verb = "runas";

                    if (!IsRunningAsAdmin())
                    {
                        Task.Run(()=>MessageBox.Show("App is not running as admin! Please restart the app and run as admin."));
                    }
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
                process.OutputDataReceived += async (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data) && !token.IsCancellationRequested)
                    {
                        try
                        {
                            // This event is called whenever there is data available in the standard output
                            this.BeginInvoke((MethodInvoker)async delegate
                            {
                                if (e.Data.ToLower().Contains("failed") || e.Data.ToLower().Contains("error") || e.Data.ToLower().Contains("miner terminated"))
                                {
                                        richTextBox.SelectionStartThreadSafe(richTextBox.TextLength);
                                        richTextBox.SelectionLengthThreadSafe(0);
                                        richTextBox.SelectionColorThreadSafe(Color.Red);
                                        richTextBox.AppendText(Environment.NewLine + e.Data);
                                        richTextBox.SelectionColorThreadSafe(richTextBox.ForeColor); // Reset to the default color                                    

                                    // Errors
                                    if (e.Data.ToLower().Contains("error on gpu") || e.Data.ToLower().Contains("miner terminated"))
                                    {
                                        // Restart miner
                                        richTextBox.AppendTextThreadSafe("Gpu failed/Miner terminated. Restarting...");
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
                                            restartsLabel.Show();
                                        }
                                    }
                                }
                                else if (e.Data.ToLower().Contains("accepted"))
                                {
                                    richTextBox.SelectionStartThreadSafe(richTextBox.TextLength);
                                    richTextBox.SelectionLengthThreadSafe(0);
                                    richTextBox.SelectionColorThreadSafe(Color.ForestGreen);
                                    richTextBox.AppendText(Environment.NewLine + e.Data);
                                    richTextBox.SelectionColorThreadSafe(richTextBox.ForeColor); // Reset to the default color  
                                }
                                else
                                {
                                    richTextBox.BeginInvoke((MethodInvoker)delegate
                                    {
                                        richTextBox.SelectionStartThreadSafe(richTextBox.TextLength);
                                        richTextBox.SelectionLengthThreadSafe(0);

                                        // Check if e.Data starts with a number
                                        if (e.Data.Length > 0 && char.IsDigit(e.Data[0]))
                                        {
                                            richTextBox.SelectionColorThreadSafe(Color.Yellow);
                                        }
                                        else
                                        {
                                            richTextBox.SelectionColorThreadSafe(richTextBox.ForeColor); // Reset to the default color
                                        }

                                        richTextBox.AppendText(Environment.NewLine + e.Data);
                                    });
                                }
                                richTextBox.ScrollToCaretThreadSafe();
                            });
                        }
                        catch (Exception ex) { }
                    }
                };

                try
                {
                    // Start the process
                    process.Start();

                    // Begin asynchronously reading the output
                    process.BeginOutputReadLine();

                    // Wait for the process to exit
                    process.WaitForExit();

                    // Close the standard output stream
                    process.Close();
                }
                catch (Exception ex)
                {
                    richTextBox.AppendTextThreadSafe(Environment.NewLine + "Error starting miner " + ex.Message);
                }
            }
        }


        // Helpers
        public static bool IsRunningAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public async Task CancelAllTasksAsync()
        {
            var tasks = runningTasks;

            if (tasks == null || tasks.Count == 0) return;

            // Create a CancellationTokenSource to cancel tasks
            ctsRunningMiners = new CancellationTokenSource();

            // Cancel each task in the list
            foreach (var task in tasks)
            {
                if (task.Status == TaskStatus.Running || task.Status == TaskStatus.WaitingForActivation)
                {
                    // Try to cancel the task
                    try
                    {
                        ctsRunningMiners.Cancel();
                        // Await the task to allow it to complete (including cancellation)
                        await task;
                    }
                    catch (AggregateException)
                    {
                        // Handle any exceptions if needed
                    }
                }
            }
        }


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


        // Kill Active Miners
        private void KillAllActiveMiners()
        {
            var minerConfigs = new List<MinerConfig>(settingsForm.Settings.MinerSettings);
            foreach (MinerConfig minerConfig in minerConfigs)
            {
                if (minerConfig.Active)
                {
                    if (!String.IsNullOrWhiteSpace(minerConfig.Miner_File_Path))
                    {
                        KillProcesses(Path.GetFileNameWithoutExtension(minerConfig.Miner_File_Path));
                    }
                    else
                    {
                        KillProcesses("miner"); 
                    }
                }
            }
        }
        static void KillProcesses(string processName)
        {
            Process[] processes = Process.GetProcesses();
            bool procExists = false;

            foreach (Process process in processes)
            {
                try
                {
                    if (process.ProcessName == Path.GetFileNameWithoutExtension(processName))
                    {
                        procExists = true;

                        // Kill the process
                        process.Kill();

                        // Use PowerShell to find and kill the associated cmd window
                        string command = $"Get-WmiObject Win32_Process | Where-Object {{ $_.ParentProcessId -eq {process.Id} }} | ForEach-Object {{ $_.Terminate() }}";
                        RunPowerShellCommand(command);
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show($"Error killing {processName} process: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }

            Thread.Sleep(1000);
            processes = Process.GetProcesses();
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




    }

}




