using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Reflection;

class Program
{
    const string _appName = "Gui_Miner.exe";
    static void Main(string[] args)
    {
        string command = "";
        bool runAsAdmin = false;
        string version = "0.1";

        if (args.Length >= 1)
            command = args[0].Replace("-", "").Trim();
        if (args.Length >= 2)
            version = args[1].Replace("-", "").Trim();
        if (args.Length >= 3)
            runAsAdmin = bool.TryParse(args[2].Replace("-", "").Trim(), out bool parsedRunAs) ? parsedRunAs : false;

        Execute(command, version);

        // Restart App
        if(runAsAdmin)
            Console.WriteLine($"Restarting as Admin...");
        else
            Console.WriteLine($"Restarting...");

        RunExeAsAdmin(GetExeFilePath(), runAsAdmin);
        CloseGuiMinerUpdateApp(0);
    }
    static async void Execute(string command, string version)
    {
        Console.WriteLine($"Incoming command: {command} {version}");
        string url = "";

        switch (command.ToLower())
        {
            case "update":

                Console.WriteLine("Checking for Updates...");

                if (AreUpdatesAvailable(version, out url))
                {
                    Console.WriteLine("Update found! Updating...");
                    await Update(url);
                }
                else
                {
                    Console.WriteLine($"Update Not Found for {version}");
                    CloseGuiMinerUpdateApp(0);
                }

                break;

            case "checkupdate":

                Console.WriteLine("Checking for updates...");

                if (AreUpdatesAvailable(version, out url))
                {
                    Console.WriteLine("Update Found!");
                    CloseGuiMinerUpdateApp(1); // return true
                }
                else
                {
                    Console.WriteLine($"Update Not Found for {version}");
                    CloseGuiMinerUpdateApp(0);
                }

                break;

            case "runas":
                KillProcess(_appName);
                break;

            default:
                break;
        }
    }
    static async Task<bool> Update(string url)
    {
        string zipPath = "";
        string fileName = GetExeFilePath();
        string? currentFolder = Path.GetDirectoryName(fileName);
        if (currentFolder != null)
            zipPath = Path.Combine(currentFolder, "update.zip");
        else
            currentFolder = "";

        Console.WriteLine("Update file downloading...");
        if (await DownloadedUpdateFile(url, zipPath))
            Console.WriteLine("Update file downloaded.");
        else
        {
            Console.WriteLine($"Failed to Update because the Download Failed from {url}");
            return false;
        }

        // Make sure the app isn't open, if so close it
        KillProcess(_appName);
        Thread.Sleep(1500);

        // Unzip the update
        string outputPath = Path.Combine(currentFolder, fileName);
        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                bool extracted = false;
                int retries = 5;
                while (!extracted || retries > 0)
                {
                    try
                    {
                        entry.ExtractToFile(outputPath, true);
                        extracted = true;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("The process cannot access the file") && ex.Message.Contains("being used by another process"))
                        {
                            try { File.Delete(outputPath); }
                            catch
                            {
                                Console.WriteLine("Unable to delete file as it is being used by another process");
                            }
                        }
                        Thread.Sleep(1000);
                    }
                }
                if (!extracted)
                    Console.WriteLine("Unable to extract all files!");
            }
        }
        Console.WriteLine("Unzipping update files...");

        // Delete the .zip update
        File.Delete(zipPath);

        Console.WriteLine("Deleting update.zip file...");
        Console.WriteLine("Update completed successfully!");

        return true;
    }
    static async Task<bool> DownloadedUpdateFile(string url, string zipPath)
    {
        int retries = 3;
        using (var httpClient = new HttpClient())
        {
            while (retries > 0)
            {
                try
                {
                    var response = httpClient.GetAsync(url).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        using (var fileStream = File.Create(zipPath))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }

                        return true;
                    }
                }
                catch (HttpRequestException)
                {
                    retries--;

                    if (retries == 0)
                    {
                        return false;
                    }

                    await Task.Delay(500);
                }
            }
        }
        return false;
    }
    static string GetExeFilePath()
    {
        // Get filename
        string exeFilePath = Directory.GetCurrentDirectory() + "\\" + _appName;
        if (File.Exists(exeFilePath))
            return exeFilePath;

        return "";
    }
    static bool AreUpdatesAvailable(string version, out string url)
    {
        url = $"https://github.com/peavey2787/GuiMiner/archive/refs/tags/V{version}.zip";
        //https://github.com/peavey2787/GuiMiner/archive/refs/tags/V1.2.zip

        if (IsValidUrl(url))
            return true;

        return false;
    }
    static bool IsValidUrl(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception)
            {

            }

            return false;
        }
    }
    static void RunExeAsAdmin(string exeFilePath, bool asAdmin = false)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo(exeFilePath);

        if(asAdmin)
            startInfo.Verb = "runas";

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to run exe as admin " + ex.Message);
        }
    }
    static void KillProcess(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);
        foreach (Process process in processes)
        {
            try
            {
                process.Kill();
                // Use PowerShell to find and kill the associated cmd window
                string command = $"Get-WmiObject Win32_Process | Where-Object {{ $_.ParentProcessId -eq {process.Id} }} | ForEach-Object {{ $_.Terminate() }}";
                RunPowerShellCommand(command);
                Console.WriteLine($"Closed {processName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to kill {processName}: {ex.Message}");
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
    static void CloseGuiMinerUpdateApp(int exitCode)
    {
        for (int i = 5; i >= 1; i--)
        {
            Console.WriteLine($"This window will self destruct in {i}");
            Thread.Sleep(1000);
        }

        Environment.Exit(exitCode);
    }
}