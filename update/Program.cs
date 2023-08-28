using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Reflection;

class Program
{
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
            runAsAdmin = bool.TryParse(args[2], out bool parsedRunAs) ? parsedRunAs : false;

        Execute(command, version);

        // Restart App
        if(runAsAdmin)
            Console.WriteLine($"Restarting as Admin...");
        else
            Console.WriteLine($"Restarting...");

        RunExeAsAdmin(GetExeFilePath(), runAsAdmin);
        Environment.Exit(0);
    }
    static void Execute(string command, string version)
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
                    Update(url);
                }

                break;
            case "checkupdate":

                Console.WriteLine("Checking for updates...");

                if (AreUpdatesAvailable(version, out url))
                {
                    Console.WriteLine("Update Found!");
                    Environment.Exit(1); // return true
                }
                else
                {
                    Console.WriteLine($"Update Not Found for {version}");
                    Environment.Exit(0);
                }

                break;
            case "runas":

                break;
            default:
                break;
        }
    }
    static bool AreUpdatesAvailable(string version, out string url)
    {
        url = $"https://github.com/peavey2787/GuiMiner/archive/refs/tags/V{version}.zip";
              //https://github.com/peavey2787/GuiMiner/archive/refs/tags/V1.2.zip

        if (IsValidUrl(url))
            return true;

        return false;
    }
    static string GetExeFilePath()
    {
        // Get filename
        string exeFileName = "Gui_Miner.exe";      
        if(File.Exists(exeFileName))
            return exeFileName;

        // Return testing path
        exeFileName = "C:\\Users\\5800x\\source\\repos\\GuiMiner\\GuiMiner\\Gui_Miner\\bin\\Debug\\Gui_Miner.exe";
        if (File.Exists(exeFileName))
            return exeFileName;

        return "";
    }
    // Update App
    static bool Update(string url)
    {
        string fileName = GetExeFilePath();
        string currentFolder = Path.GetDirectoryName(fileName);
        string zipPath = Path.Combine(currentFolder, "update.zip");

        int retries = 3;
        while (retries > 0)
        {
            // Download the update
            using (WebClient client = new WebClient())
            {
                try
                {                    
                    client.DownloadFile(url, zipPath);
                    Console.WriteLine("Update file downloaded.");
                    break;                    
                }
                catch (Exception exc)
                {
                    retries--;
                    if (retries == 0)
                    {
                        Console.WriteLine($"Failed to Update because the Download Failed from {url}");
                        return false;
                    }
                    Thread.Sleep(500);
                }
            }
        }


        // Make sure the app isn't open, if so close it
        KillProcess("Gui_Miner.exe");

        // Unzip the update
        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".exe"))
                {
                    string outputPath = Path.Combine(currentFolder, fileName);
                    entry.ExtractToFile(outputPath, true);
                    break;
                }
            }
        }
        Console.WriteLine("Unzipping update files...");

        // Delete the .zip update
        File.Delete(zipPath);

        Console.WriteLine("Deleting update.zip file...");
        Console.WriteLine("Update completed successfully!");

        return true;
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
            Console.WriteLine("Error: " + ex.Message);
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
                Console.WriteLine($"Closed {processName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to kill {processName}: {ex.Message}");
            }
        }
    }
}