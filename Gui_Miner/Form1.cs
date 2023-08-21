﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
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
using Timer = System.Windows.Forms.Timer;

namespace Gui_Miner
{
    public partial class Form1 : Form
    {
        NotifyIcon notify_icon;
        SettingsForm settingsForm = new SettingsForm();
        CancellationTokenSource ctsRotatingPanel = new CancellationTokenSource();
        private RotatingPanel rotatingPanel;
        private double rotationAngle; // Current rotation angle
        List<Task> runningTasks = new List<Task>();
        CancellationTokenSource ctsRunningMiners = new CancellationTokenSource();
        public Form1()
        {
            InitializeComponent();

            // Create notify icon
            CreateNotifyIcon();

            // Create Settings Form
            settingsForm.Show();
            settingsForm.Visible = false;
            settingsForm.Form1 = this;

            outputPanel.BackgroundImage = null;

            // Rotating panel
            CreateRotatingPanel();
            
        }

        // Rotate image
        private async void CreateRotatingPanel()
        {
            outputPanel.Controls.Clear();

            ctsRotatingPanel = new CancellationTokenSource(); // Initialize the CancellationTokenSource

            rotatingPanel = new RotatingPanel();
            rotatingPanel.Size = outputPanel.Size;
            rotatingPanel.Location = new Point(0, 55);
            rotatingPanel.BackColor = Color.Transparent;
            rotatingPanel.Dock = DockStyle.Fill;
            rotatingPanel.Visible = true;

            outputPanel.Controls.Add(rotatingPanel);

            rotatingPanel.BringToFront();

            // Start a background task to rotate the panel asynchronously
            await RotatePanelAsync(ctsRotatingPanel.Token);
        }
        private void RemoveRotatingPanel()
        {
            if (rotatingPanel != null)
            {
                ctsRotatingPanel.Cancel();
                rotatingPanel.Dispose(); // Dispose of the rotating panel
                outputPanel.Controls.Remove(rotatingPanel); // Remove it from the outputPanel
                rotatingPanel = null; // Set the reference to null
            }
        }
        private async Task RotatePanelAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (rotatingPanel != null)
                    {
                        // Increment the rotation angle (adjust the speed by changing the increment)
                        rotationAngle += 0.5; // Adjust the angle increment for desired speed

                        // Apply the rotation
                        rotatingPanel.RotationAngle = rotationAngle;

                        // Redraw the Panel
                        rotatingPanel.Invalidate();

                        // Check for cancellation after each step
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break; // Exit the loop if cancellation is requested
                        }

                        // Sleep to control the rotation speed
                        Task.Delay(50).Wait(); // Adjust the interval for rotation speed (milliseconds)
                    }
                    else
                    {
                        Task.Delay(250).Wait();
                    }
                }
            });
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
        private void ClickStartButton()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ClickStartButton));
                return;
            }

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
        private void ClickStopButton()
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

            Task.Run(() => { KillAllActiveMiners(); });
        }
        private void settingsButtonPictureBox_Click(object sender, EventArgs e)
        {
            settingsForm.Visible = true;
        }


        // Start Active Miners
        private void CreateTabControlAndStartMiners()
        {
            CancelAllTasks();

            TabControl tabControl = new TabControl();
            tabControl.Name = "outputTabControl";
            tabControl.Dock = DockStyle.Fill;

            foreach (MinerConfig minerConfig in settingsForm.Settings.MinerSettings)
            {
                (Type configType, Object configObject) = minerConfig.GetSelectedMinerConfig();
                PropertyInfo activeProperty = configType.GetProperty("Active");
                bool isActive = (bool)activeProperty.GetValue(configObject);
                if (isActive)
                {
                    TabPage tabPage = new TabPage();
                    tabPage.Text = minerConfig.CurrentMinerConfig.ToString();

                    PropertyInfo runAsAdminProperty = configType.GetProperty("runAsAdmin");
                    bool runAsAdmin = (bool)runAsAdminProperty.GetValue(configObject);

                    // Try to get api
                    MethodInfo getApiPortMethod = configType.GetMethod("GetApiPort");

                    int api = -1;
                    if (getApiPortMethod != null)
                    {
                        api = (int)getApiPortMethod.Invoke(configObject, null);
                    }

                    if(api <= 0)
                    {
                        // Try getting api port from .bat file
                        if(minerConfig.batFileArguments.Contains("api "))
                        {
                            string args = minerConfig.batFileArguments + " ";
                            int start = args.IndexOf("api ") + 4;
                            int end = args.IndexOf(" ", start);
                            api = int.TryParse(args.Substring(start, end - start), out int portResult) ? portResult : 0;
                        }
                    }

                    // Set link to api stats
                    string url = "http://localhost:" + api;

                    LinkLabel linkLabel = new LinkLabel();
                    linkLabel.Text = "Api Stats: " + url;
                    linkLabel.Dock = DockStyle.Top;
                    linkLabel.LinkClicked += (sender, e) =>
                    {
                        // Open the default web browser with the specified URL
                        Process.Start(url);
                    };
                    tabPage.Controls.Add(linkLabel);

                    // Try to get pool link
                    MethodInfo getPoolDomainNameMethod = configType.GetMethod("GetPoolDomainName1");
                    string poolDomainName1 = (string)getPoolDomainNameMethod.Invoke(configObject, null);
                    string poolLink1 = "";
                    if (poolDomainName1 != null)
                    {
                        // Get matching pool and return its link
                        foreach (Pool pool in settingsForm.Settings.Pools)
                        {
                            if (pool.Address.Contains(poolDomainName1) || pool.Link.Contains(poolDomainName1))
                            {
                                poolLink1 = pool.Link;
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
                    tabPage.Controls.Add(poolLinkLabel);


                    RichTextBox tabPageRichTextBox = new RichTextBox();
                    tabPageRichTextBox.Dock = DockStyle.Fill;
                    tabPageRichTextBox.ReadOnly = true;
                    tabPageRichTextBox.ForeColor = Color.White;
                    tabPageRichTextBox.BackColor = Color.Black;
                    tabPage.Controls.Add(tabPageRichTextBox);

                    tabControl.TabPages.Add(tabPage);

                    if (minerConfig.batFileArguments.IndexOf(".exe") >= -1)
                    {
                        string filePath = "";
                        string arguments = "";

                        // Get miner path
                        if (minerConfig.batFileArguments.StartsWith("\""))
                        {
                            // Quotes around path
                            filePath = minerConfig.batFileArguments.Substring(1, minerConfig.batFileArguments.IndexOf(".exe") + 3);
                            arguments = minerConfig.batFileArguments.Replace($"\"{filePath}\"", string.Empty).Trim();
                        }
                        else
                        {
                            // No quotes around path
                            filePath = minerConfig.batFileArguments.Substring(0, minerConfig.batFileArguments.IndexOf(".exe") + 4);
                            arguments = minerConfig.batFileArguments.Replace($"{filePath}", string.Empty).Trim();
                        }                        

                        // Start the miner in a separate thread with the miner-specific RichTextBox
                        Task minerTask = Task.Run(() => StartMiner(filePath, arguments, runAsAdmin, tabPageRichTextBox, ctsRunningMiners.Token));

                        runningTasks.Add(minerTask);
                    }
                }
            }

            // Check if the outputPanel already has the tabControl
            TabControl tabControlToRemove = null;
            foreach (Control control in outputPanel.Controls)
            {
                if (control is TabControl)
                {
                    tabControlToRemove = (TabControl)control;
                    break; // Exit the loop once the first TabControl is found
                }
            }

            if (tabControlToRemove != null)
            {
                outputPanel.Controls.Remove(tabControlToRemove);
                tabControlToRemove.Dispose();
            }

            // Add the TabControl
            outputPanel.Controls.Add(tabControl);
        }
        private void StartMiner(string filePath, string arguments, bool runAsAdmin, RichTextBox richTextBox, CancellationToken token)
        {
            if (File.Exists(filePath))
            {
                richTextBox.Clear();
                richTextBox.ForeColor = Color.FromArgb((int)(0.53 * 255), 58, 221, 190);

                // Create a new process
                Process process = new Process();

                string path = Directory.GetCurrentDirectory() + "\\" + filePath;

                string verb = "";
                if (runAsAdmin)
                {
                    verb = "runas";

                    if (!IsRunningAsAdmin())
                    {
                        MessageBox.Show("App is not running as admin! Please restart the app and run as admin.");
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
                                if (e.Data.ToLower().Contains("failed") || e.Data.ToLower().Contains("error"))
                                {
                                    richTextBox.SelectionStart = richTextBox.TextLength;
                                    richTextBox.SelectionLength = 0;
                                    richTextBox.SelectionColor = Color.Red;
                                    richTextBox.AppendText(Environment.NewLine + e.Data);
                                    richTextBox.SelectionColor = richTextBox.ForeColor; // Reset to the default color

                                    // Errors
                                    if (e.Data.ToLower().Contains("error on gpu"))
                                    {
                                        // Restart miner
                                        richTextBox.AppendText("Gpu failed, Restarting...");
                                        ClickStopButton();
                                        await Task.Delay(2500);// Thread.Sleep(2500);                                        
                                        ClickStartButton();

                                        // Extract the number from the restarts label text
                                        int restarts;
                                        if (int.TryParse(Regex.Match(restartsLabel.Text, @"\d+").Value, out restarts))
                                        {
                                            restarts++;

                                            // Update the label text with the new number
                                            restartsLabel.Text = $"Restarts {restarts}";
                                            restartsLabel.ForeColor = Color.Red;
                                            restartsLabel.Show();
                                        }
                                    }
                                }
                                else if (e.Data.ToLower().Contains("accepted"))
                                {
                                    richTextBox.SelectionStart = richTextBox.TextLength;
                                    richTextBox.SelectionLength = 0;
                                    richTextBox.SelectionColor = Color.ForestGreen;
                                    richTextBox.AppendText(Environment.NewLine + e.Data);
                                    richTextBox.SelectionColor = richTextBox.ForeColor; // Reset to the default color
                                }
                                else
                                {
                                    richTextBox.SelectionStart = richTextBox.TextLength;
                                    richTextBox.SelectionLength = 0;

                                    // Check if e.Data starts with a number
                                    if (e.Data.Length > 0 && char.IsDigit(e.Data[0]))
                                    {
                                        richTextBox.SelectionColor = Color.Yellow;
                                    }
                                    else
                                    {
                                        richTextBox.SelectionColor = richTextBox.ForeColor; // Reset to the default color
                                    }

                                    richTextBox.AppendText(Environment.NewLine + e.Data);
                                }
                                richTextBox.ScrollToCaret();
                            });
                        }
                        catch (Exception ex) { }
                    }
                };

                // Start the process
                process.Start();

                // Begin asynchronously reading the output
                process.BeginOutputReadLine();

                // Wait for the process to exit
                process.WaitForExit();

                // Close the standard output stream
                process.Close();
            }
        }


        // Helpers
        public static bool IsRunningAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public void CancelAllTasks()
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
            foreach (MinerConfig minerConfig in settingsForm.Settings.MinerSettings)
            {
                (Type configType, Object configObject) = minerConfig.GetSelectedMinerConfig();
                PropertyInfo activeProperty = configType.GetProperty("Active");
                bool isActive = (bool)activeProperty.GetValue(configObject);
                if (isActive)
                {                    
                    PropertyInfo filePathProperty = configType.GetProperty("MinerFilePath");
                    string filePath = (string)filePathProperty.GetValue(configObject);

                    if (minerConfig.batFileArguments.IndexOf(".exe") >= -1)
                    {
                        // Get miner path
                        if (minerConfig.batFileArguments.StartsWith("\""))
                        {
                            // Quotes around path
                            filePath = minerConfig.batFileArguments.Substring(1, minerConfig.batFileArguments.IndexOf(".exe") + 3);
                        }
                        else
                        {
                            // No quotes around path
                            filePath = minerConfig.batFileArguments.Substring(0, minerConfig.batFileArguments.IndexOf(".exe") + 4);
                        }
                        KillProcesses(filePath);
                    }
                    else if(!String.IsNullOrWhiteSpace(filePath))
                        KillProcesses(filePath);
                    else
                        KillProcesses("miner.exe");
                    
                }
            }
        }
        static void KillProcesses(string processName)
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));

            while (processes.Count() > 0)
            {
                processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));

                foreach (Process process in processes)
                {
                    try
                    {
                        // First, kill the process
                        process.Kill();

                        // Then, use PowerShell to find and kill the associated cmd window
                        string command = $"Get-WmiObject Win32_Process | Where-Object {{ $_.ParentProcessId -eq {process.Id} }} | ForEach-Object {{ $_.Terminate() }}";
                        RunPowerShellCommand(command);
                    }
                    catch (Exception ex)
                    {

                    }
                }
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




    }



    // Custom panel for rotating
    public class RotatingPanel : Panel
    {
        private double rotationAngle;

        public double RotationAngle
        {
            get { return rotationAngle; }
            set
            {
                rotationAngle = value;
                Invalidate(); // Trigger a repaint when the rotation angle changes
            }
        }

        public RotatingPanel()
        {
            DoubleBuffered = true; // Enable double buffering
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Rotate the image and draw it on the panel
            using (Image rotatedImage = RotateImage(Properties.Resources.kas_world, (float)rotationAngle, ClientRectangle.Size))
            {
                e.Graphics.DrawImage(rotatedImage, Point.Empty);
            }
        }

        // Function to rotate and resize an image by a specified angle while maintaining aspect ratio
        private Image RotateImage(Image image, float angle, Size newSize)
        {
            // Calculate the new dimensions while maintaining the aspect ratio
            int newWidth, newHeight;
            float aspectRatio = (float)image.Width / image.Height;

            if (newSize.Width / aspectRatio <= newSize.Height)
            {
                // Lock to height and calculate width based on aspect ratio
                newHeight = newSize.Height;
                newWidth = (int)(newHeight * aspectRatio);
            }
            else
            {
                // Lock to width and calculate height based on aspect ratio
                newWidth = newSize.Width;
                newHeight = (int)(newWidth / aspectRatio);
            }

            Bitmap rotatedImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // Enable anti-aliasing
                g.TranslateTransform(newWidth / 2, newHeight / 2); // Set the rotation point at the center of the image
                g.RotateTransform(angle);
                g.TranslateTransform(-newWidth / 2, -newHeight / 2); // Reset the translation

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; // Set interpolation mode

                g.DrawImage(image, new RectangleF(Point.Empty, new Size(newWidth, newHeight)), new Rectangle(Point.Empty, image.Size), GraphicsUnit.Pixel);
            }
            return rotatedImage;
        }

    }

}




