using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gui_Miner
{
    public partial class Form1 : Form
    {
        SettingsForm settingsForm = new SettingsForm();
        MinerConfig minerConfig;
        public Form1()
        {
            InitializeComponent();
            settingsForm.Show();
            settingsForm.Visible = false;
            settingsForm.Form1 = this;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MinerConfig minerConfig = GetWhichMinerConfigToUse();
        }

        internal MinerConfig GetWhichMinerConfigToUse()
        {
            if (DoesExeFileExist("miner"))
                minerConfig = new GminerConfig();

            return minerConfig;
        }

        public static bool DoesExeFileExist(string fileName)
        {
            // Get the current directory where the application is running
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Combine the current directory with the file name to create the full path
            string fullPath = Path.Combine(currentDirectory, fileName + ".exe");

            // Check if the file exists
            return File.Exists(fullPath);
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            settingsForm.Visible = true;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            TabControl tabControl = new TabControl();
            tabControl.Name = "outputTabControl";
            tabControl.Dock = DockStyle.Fill;

            foreach (MinerConfig minerConfig in settingsForm.Settings.MinerSettings)
            {
                if (minerConfig.Active)
                {
                    TabPage tabPage = new TabPage();
                    tabPage.Text = minerConfig.Name;

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
                        string filePath = minerConfig.batFileArguments.Substring(0, minerConfig.batFileArguments.IndexOf(".exe") + 4);
                        string arguments = minerConfig.batFileArguments.Replace(filePath, string.Empty).Trim();

                        // Start the miner in a separate thread with the miner-specific RichTextBox
                        Task.Run(() => StartMiner(filePath, arguments, tabPageRichTextBox));
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

        private void StartMiner(string filePath, string arguments, RichTextBox richTextBox)
        {
            if (File.Exists(filePath))
            {
                // Create a new process
                Process process = new Process();

                string path = Directory.GetCurrentDirectory() + "\\" + filePath;
                // Set the process start information
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
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
                            richTextBox.AppendText(Environment.NewLine + e.Data);
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

        private void KillAllActiveMiners()
        {
            // Clear Gui
            outputPanel.Controls.Clear();

            foreach (MinerConfig minerConfig in settingsForm.Settings.MinerSettings)
            {
                if (minerConfig.Active)
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            KillAllActiveMiners();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            KillAllActiveMiners();
        }
    }

    public class MinerConfig
    {
        Random random = new Random();
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string Worker_Name { get; set; }
        public string Wallet1 { get; set; }
        public string Wallet2 { get; set; }
        public string Wallet3 { get; set; }
        public string Coin1 { get; set; }
        public string Coin2 { get; set; }
        public string Coin3 { get; set; }
        public string Algo1 { get; set; }
        public string Algo2 { get; set; }
        public string Algo3 { get; set; }
        public string Pool1 { get; set; }
        public string Pool2 { get; set; }
        public string Pool3 { get; set; }
        public int Port1 { get; set; }
        public int Port2 { get; set; }
        public int Port3 { get; set; }
        public bool SSL1 { get; set; }
        public bool SSL2 { get; set; }
        public bool SSL3 { get; set; }
        public string batFileArguments { get; set; }

        public MinerConfig()
        {
            Id = random.Next(2303, 40598);
        }
    }

    }
