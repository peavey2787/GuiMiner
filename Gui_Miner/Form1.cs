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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gui_Miner
{
    public partial class Form1 : Form
    {
        SettingsForm settingsForm = new SettingsForm();
        public Form1()
        {
            InitializeComponent();
            settingsForm.Show();
            settingsForm.Visible = false;
            settingsForm.Form1 = this;
        }


        // Load/Close Form
        private void Form1_Load(object sender, EventArgs e)
        {
            startButtonPictureBox.Tag = "play";

            // Auto start
            bool autoStart = bool.TryParse(AppSettings.Load<string>(SettingsForm.AUTOSTARTMINING), out bool result) ? result : false;
            if(autoStart) startButtonPictureBox_Click(sender, EventArgs.Empty);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            KillAllActiveMiners();
        }


        // Start/Stop/Settings Buttons
        private void startButtonPictureBox_Click(object sender, EventArgs e)
        {
            if(startButtonPictureBox.Tag == "play")
            {
                // Play button pushed
                startButtonPictureBox.BackgroundImage = Properties.Resources.stop_button;
                startButtonPictureBox.Tag = "stop";

                // Remove background image
                outputPanel.BackgroundImage = null;

                CreateTabControlAndStartMiners();
            }
            else
            {
                // Stop button pushed
                startButtonPictureBox.BackgroundImage = Properties.Resources.play_button;
                startButtonPictureBox.Tag = "play";
                
                // Replace background image
                outputPanel.BackgroundImage = Properties.Resources.kas_world;

                KillAllActiveMiners();
            }
        }
        private void settingsButtonPictureBox_Click(object sender, EventArgs e)
        {
            settingsForm.Visible = true;
        }


        // Start Active Miners
        private void CreateTabControlAndStartMiners()
        {
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
                    PropertyInfo runAsAdminProperty = configType.GetProperty("runAsAdmin");
                    bool runAsAdmin = (bool)runAsAdminProperty.GetValue(configObject);

                    // Try to get api
                    MethodInfo getApiPortMethod = configType.GetMethod("GetApiPort");

                    int api = -1;
                    if (getApiPortMethod != null)
                    {
                        api = (int)getApiPortMethod.Invoke(configObject, null);
                    }

                    TabPage tabPage = new TabPage();
                    tabPage.Text = minerConfig.CurrentMinerConfig.ToString();
                    
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

                    RichTextBox tabPageRichTextBox = new RichTextBox();
                    tabPageRichTextBox.Dock = DockStyle.Fill;
                    tabPageRichTextBox.ReadOnly = true;
                    tabPageRichTextBox.ForeColor = Color.White;
                    tabPageRichTextBox.BackColor = Color.Black;
                    tabPage.Controls.Add(tabPageRichTextBox);

                    tabControl.TabPages.Add(tabPage);

                    if (minerConfig.batFileArguments.IndexOf(".exe") >= -1)
                    {
                        // Get miner path
                        string filePath = minerConfig.batFileArguments.Substring(1, minerConfig.batFileArguments.IndexOf(".exe") + 3);
                        string arguments = minerConfig.batFileArguments.Replace($"\"{filePath}\"", string.Empty).Trim();

                        // Start the miner in a separate thread with the miner-specific RichTextBox
                        Task.Run(() => StartMiner(filePath, arguments, runAsAdmin, tabPageRichTextBox));
                    }
                }
            }

            // Check if the outputPanel already has the tabControl
            if (outputPanel.Controls.Contains(tabControl))
            {
                outputPanel.Controls.Remove(tabControl);
            }

            // Add the TabControl to your form or container
            outputPanel.Controls.Add(tabControl);
        }
        private void StartMiner(string filePath, string arguments, bool runAsAdmin, RichTextBox richTextBox)
        {
            if (File.Exists(filePath))
            {
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
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        // This event is called whenever there is data available in the standard output
                        this.Invoke((MethodInvoker)delegate
                        {
                            if (e.Data.ToLower().Contains("failed"))
                            {
                                richTextBox.SelectionStart = richTextBox.TextLength;
                                richTextBox.SelectionLength = 0;
                                richTextBox.SelectionColor = Color.Red;
                                richTextBox.AppendText(Environment.NewLine + e.Data);
                                richTextBox.SelectionColor = richTextBox.ForeColor; // Reset to the default color
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
                                richTextBox.AppendText(Environment.NewLine + e.Data);
                            }
                            richTextBox.ScrollToCaret();
                        });
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

        public static bool IsRunningAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // Kill Active Miners
        private void KillAllActiveMiners()
        {
            // Clear Gui
            outputPanel.Controls.Clear();

            foreach (MinerConfig minerConfig in settingsForm.Settings.MinerSettings)
            {
                (Type configType, Object configObject) = minerConfig.GetSelectedMinerConfig();
                PropertyInfo activeProperty = configType.GetProperty("Active");
                bool isActive = (bool)activeProperty.GetValue(configObject);
                if (isActive)
                {
                    if (minerConfig.batFileArguments.IndexOf(".exe") >= -1)
                    {
                        // Get miner path
                        string filePath = minerConfig.batFileArguments.Substring(0, minerConfig.batFileArguments.IndexOf(".exe") + 4);

                        // Start the miner in a separate thread with the miner-specific RichTextBox
                        Task.Run(() =>
                        {
                            KillProcessesByFilename(filePath);
                        });
                    }
                }
            }
        }
        public static void KillProcessesByFilename(string filename)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "taskkill",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"/F /IM {filename}"
            };

            Process process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            process.Start();
            process.WaitForExit();
            process.Close();
        }





    }



}
