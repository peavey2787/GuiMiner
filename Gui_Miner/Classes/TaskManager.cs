using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gui_Miner.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Security.Principal;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    // Custom EventArgs class to hold output data received
    public class OutputDataReceivedEventArgs : EventArgs
    {
        public string OutputData { get; }

        public OutputDataReceivedEventArgs(string outputData)
        {
            OutputData = outputData;
        }
    }

    public class TaskManager
    {
        Random randomId = new Random();
        Form1 form1;
        public void SetForm1(Form1 form1) { this.form1 = form1; }

        // TaskId - Process
        private Dictionary<string, Process> runningTasks = new Dictionary<string, Process>();
        public Dictionary<string, Process> GetRunningTasks () { return runningTasks; }

        public TaskManager (Form1 form1) 
        {
            this.form1 = form1;
            runningTasks = new Dictionary<string, Process>(); 
        }
        // Custom event for output data received
        public delegate void OutputDataReceivedEventHandler(object sender, OutputDataReceivedEventArgs e);
        public event OutputDataReceivedEventHandler OutputDataReceivedEvent;


        public bool StartTask(string filePath, string arguments = "", bool runAsAdmin = false, string taskId = null, RichTextBox richTextBox = null)
        {
            if (!string.IsNullOrEmpty(taskId) && runningTasks.ContainsKey(taskId) && !runningTasks[taskId].HasExited)
            {
                return true; // Task is already running
            }
            else if (string.IsNullOrEmpty(taskId))
            {
                taskId = randomId.Next(1000, 40001).ToString();
            }

            if (!File.Exists(filePath))
            {
                if (richTextBox != null)
                {
                    richTextBox.AppendTextThreadSafe($"\nUnable to locate miner at {filePath}");
                    richTextBox.ForeColorSetThreadSafe(Color.FromArgb((int)(0.53 * 255), 58, 221, 190));
                }
                return false;
            }

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (runAsAdmin)
            {
                // Start the process as an administrator
                process.StartInfo.Verb = "runas";
            }

            process.OutputDataReceived += (sender, e) =>
            {
                OutputDataReceivedEvent?.Invoke(sender, new OutputDataReceivedEventArgs(e.Data));
                if (richTextBox != null)
                    UpdateOutputConsole(e.Data, richTextBox);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                OutputDataReceivedEvent?.Invoke(sender, new OutputDataReceivedEventArgs(e.Data));
                if (richTextBox != null)
                    UpdateOutputConsole(e.Data, richTextBox);
            };

            if (richTextBox != null)
            {
                richTextBox.AppendTextThreadSafe("\nSTARTING MINER...");
                richTextBox.ForeColorSetThreadSafe(Color.FromArgb((int)(0.53 * 255), 58, 221, 190));
            }

            try
            {
                process.Start();

                runningTasks[taskId] = process;

                Task.Run( () =>
                {
                    // Begin asynchronously reading the output
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Wait for the process to exit
                    process.WaitForExit();

                    // Close the standard output stream
                    process.Close();
                    process.Dispose();
                });
            }
            catch (Exception ex)
            {
                if (richTextBox != null)
                    richTextBox.AppendTextThreadSafe(Environment.NewLine + "Error starting miner " + ex.Message);
                return false;
            }

            return true;
        }

        public void StopTask(string taskId)
        {
            if (runningTasks.ContainsKey(taskId))
            {
                KillProcess(runningTasks[taskId]);
                runningTasks.Remove(taskId);
            }
        }

        public void ToggleAllTasks(bool startTasks)
        {
            if (startTasks)
            {
                foreach (var task in runningTasks)
                {
                    task.Value.Start();
                }
            }
            else
            {
                foreach (var task in runningTasks)
                {
                    KillProcess(task.Value);
                }
            }
        }
        

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
                    string updateProjectPath = Directory.GetCurrentDirectory() + "\\update.exe";
                    string command = "runas";

                    // Create a process start info
                    ProcessStartInfo restartInfo = new ProcessStartInfo(updateProjectPath);
                    restartInfo.Verb = "runas";
                    restartInfo.Arguments = $"-{command} -{version} -{true}";

                    // Start the "update" project as a separate process
                    try
                    {
                        Process.Start(restartInfo);

                        form1.CloseApp();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error restarting app as admin " + ex.Message);
                    }
                }
            }

            return isAdmin;
        }

        public void KillAllMiners()
        {
            KillAllProcessesContainingName("miner");
            KillAllProcessesContainingName("Miner");
        }


        // Helpers
        private void KillAllProcessesContainingName(string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName.Contains(name))
                {
                    KillProcess(process);
                }
            }
        }

        private void KillProcess(Process process)
        {
            try
            {
                // Use PowerShell to find and kill the associated cmd window                
                RunPowerShellCommand(process.Id.ToString());

                // Kill the process
                process.Kill();
                process = null;
            }
            catch { }
        }        

        private static bool RunPowerShellCommand(string processId)
        {
            // Use PowerShell to find and kill the associated cmd window
            string command = $"Get-WmiObject Win32_Process | Where-Object {{ $_.ParentProcessId -eq {processId} }} | ForEach-Object {{ $_.Terminate() }}";

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

        private void UpdateOutputConsole(string output, RichTextBox richTextBox)
        {
            if (output == null) return;

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
                    form1.ClickStopButton();
                    Thread.Sleep(3500);
                    form1.ClickStartButton();

                    // Assuming you have a reference to the Form1 instance named form1
                    Label restartsLabel = form1.Controls.Find("restartsLabel", true).FirstOrDefault() as Label;

                    if (restartsLabel != null)
                    {
                        // Extract the number from the restarts label text
                        int restarts;
                        if (int.TryParse(Regex.Match(restartsLabel.GetTextThreadSafe(), @"\d+").Value, out restarts))
                        {
                            restarts++;

                            // Update the label text with the new number
                            restartsLabel.SetTextThreadSafe($"Restarts {restarts}");
                            restartsLabel.ForeColorThreadSafe(Color.Red);
                            restartsLabel.ShowThreadSafe();
                        }
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
    }


}
