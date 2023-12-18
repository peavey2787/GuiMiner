using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Web;

namespace Gui_Miner.Classes
{
    public class NServer
    {
        #region Variables
        // Public
        public const int Port = 46543;
        public Form1 Form1;

        // Private
        string Ip;
        ConcurrentQueue<TcpClient> Clients_to_serve;
        RichTextBox ConsoleTextBox;
        TcpListener _Server;
        public void SetConsoleTextBox(ref RichTextBox textbox) { ConsoleTextBox = textbox; }
        #endregion

        public NServer()
        {
            Ip = Get_Local_IP_Address()?.ToString();
        }

        #region Start/Stop
        public async void Start()
        {
            try
            {
                Clients_to_serve = new ConcurrentQueue<TcpClient>();
                _Server = IPAddress.TryParse(Ip, out IPAddress ipAddress)
                    ? new TcpListener(ipAddress, Port)
                    : null;

                _Server?.Start();

                //ConsoleTextBox.AddTextThreadSafe($"N server started, listening for LAN commands on {Ip}:{Port}");

                while (true)
                {
                    TcpClient client = _Server.Pending() ? await _Server.AcceptTcpClientAsync() : null;

                    if (client == null)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 0));

                    Clients_to_serve.Enqueue(client);

                    if (Clients_to_serve.TryDequeue(out client))
                    {
                        //ConsoleTextBox.AddTextThreadSafe($"Client connected from {client.Client.RemoteEndPoint}");
                        var task = Serve_Client(client);
                        //task.ContinueWith(_ => ConsoleTextBox.AddTextThreadSafe("Client has been served"), TaskContinuationOptions.OnlyOnRanToCompletion);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Only one usage of each socket address (protocol/network address/port) is normally permitted"))
                {
                    // TODO: figure this out 
                }

                string message = "";

                if (ex.Message == "Not listening. You must call the Start() method before calling this method.")
                    message = "N server stopped";
                else
                    message = ex.Message;
                //ConsoleTextBox.AddTextThreadSafe(message);
            }

        }
        private Task Serve_Client(TcpClient client)
        {
            return Task.Run(() =>
            {
                var stream = client.GetStream();
                var writer = new StreamWriter(stream) { AutoFlush = true };
                var reader = new StreamReader(stream);
                bool closeConnection = false;
                string command = "";

                while (client.Connected && !closeConnection)
                {
                    try
                    {
                        command = reader.ReadLine();
                        if (command != null & command == "command=closeConnection")
                            closeConnection = true;
                        else if (command != null)
                            Execute(command, writer);
                        else
                            Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }

                if (writer.BaseStream != null)
                {
                    //Console.WriteLine("The StreamWriter is in use.");
                    Thread.Sleep(2000);
                }

                stream.Close();
                writer.Close();
                reader.Close();
            });
        }
        public void Stop()
        {
            _Server?.Stop();
        }
        #endregion

        #region Execute command
        void Execute(string command, StreamWriter writer)
        {
            string extra = "";
            command = Uri.UnescapeDataString(command);

            var imgName = ExtractFilename(command);

            if (command == "GET / HTTP/1.1")
            {
                // Serve index file
                command = "index";
            }
            else if (command == "GET /controls HTTP/1.1")
            {
                command = "controls";
            }
            else if (command == "GET /settings HTTP/1.1")
            {
                command = "settings";
            }
            else if (command == "GET /styles.css HTTP/1.1")
            {
                command = "styles";
            }
            else if (imgName != null && !imgName.StartsWith("command="))
            {
                command = "images";
                extra = imgName;
            }
            else if (command.IndexOf("command=") > -1)
            {
                // Split the command=actualCommandHere
                string[] parts = command.Split(new[] { '=' }, 2);
                command = parts[1].Split(' ')[0]; // Remove extra junk


                if (parts.Length >= 2 && parts[1].Contains('='))
                {
                    var extraParts = command.Split('=');
                    if (extraParts.Length >= 2)
                    {
                        command = extraParts[0];
                        extra = extraParts[1];
                    }
                }
            }

            HandleCommand(command, extra, writer);
        }

        private async void HandleCommand(string command, string extra, StreamWriter writer)
        {
            string resp = "";
            string httpResponse = "";
            string decodedJson = "";
            MinerConfig incomingMinerConfig = new MinerConfig();
            Wallet incomingWallet = new Wallet();
            Pool incomingPool = new Pool();
            Settings settings = new Settings();

            switch (command)
            {
                // General server commands
                #region General Server
                case "ping":
                    string name = Environment.MachineName;
                    resp = "pong:" + name;
                    httpResponse = CreateHttpResponse(resp);
                    writer.Write(httpResponse);
                    break;

                case "index":
                    var indexFile = Directory.GetCurrentDirectory() + "\\web\\index.html";
                    string indexHtml = "";

                    if (File.Exists(indexFile))
                        indexHtml = LoadHtmlFile(indexFile);
                    else
                        indexHtml = "<h1>Uh Oh! The index file is missing!</h1>";

                    httpResponse = CreateHttpResponse(indexHtml, "text/html");
                    writer.Write(httpResponse);
                    break;

                case "controls":
                    var controlsFile = Directory.GetCurrentDirectory() + "\\web\\controls.html";
                    string controlsHtml = "";

                    if (File.Exists(controlsFile))
                        controlsHtml = LoadHtmlFile(controlsFile);

                    httpResponse = CreateHttpResponse(controlsHtml, "text/html");
                    writer.Write(httpResponse);
                    break;

                case "settings":
                    var settingsFile = Directory.GetCurrentDirectory() + "\\web\\settings.html";
                    string settingsHtml = "";

                    if (File.Exists(settingsFile))
                        settingsHtml = LoadHtmlFile(settingsFile);

                    httpResponse = CreateHttpResponse(settingsHtml, "text/html");
                    writer.Write(httpResponse);
                    break;

                case "styles":
                    var stylesFile = Directory.GetCurrentDirectory() + "\\web\\styles.css";
                    string css = "";

                    if (File.Exists(stylesFile))
                        css = LoadHtmlFile(stylesFile);

                    httpResponse = CreateHttpResponse(css, "text/css");
                    writer.Write(httpResponse);
                    break;

                case "images":
                    var imageFile = Directory.GetCurrentDirectory() + "\\web\\" + extra;

                    if (File.Exists(imageFile))
                    {
                        byte[] imageData;

                        using (FileStream fileStream = new FileStream(imageFile, FileMode.Open, FileAccess.Read))
                        {
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                fileStream.CopyTo(memoryStream);
                                imageData = memoryStream.ToArray();
                            }
                        }

                        CreateHttpResponse(imageData, writer);
                    }
                    break;
                #endregion

                // Remote Actions
                #region Remove Commands

                case "getSettings":

                    settings = Form1.GetSettings();
                    resp = JsonConvert.SerializeObject(settings);
                    httpResponse = CreateHttpResponse(resp);
                    writer.Write(httpResponse);

                    break;

                case "getMinerSettings":

                    settings = Form1.GetSettings();
                    resp = JsonConvert.SerializeObject(settings.MinerSettings);
                    httpResponse = CreateHttpResponse(resp);
                    writer.Write(httpResponse);

                    break;

                case "startAllActive":
                    Form1.ClickStartButton();
                    break;

                case "stopAllActive":
                    await Form1.ClickStopButton();
                    break;

                case "startMinerId":
                    Form1.ClickStartButton(extra);
                    break;

                case "stopMinerId":
                    Form1.ClickStopButton(extra);
                    break;

                case "switchToMinerId":
                    string[] pieces = extra.Split('-');
                    if (pieces.Length == 2)
                    {
                        string oldId = pieces[0];
                        string newId = pieces[1];
                        Form1.SwitchActiveMinerSetting(oldId, newId);
                    }
                    break;

                case "getAllRunningMiners":                    
                    var runningMiners = Form1.GetRunningMiners();
                    resp = JsonConvert.SerializeObject(runningMiners);
                    httpResponse = CreateHttpResponse(resp);
                    writer.Write(httpResponse);
                    break;

                case "updateMinerSetting":

                    decodedJson = HttpUtility.UrlDecode(extra);
                    incomingMinerConfig = JsonConvert.DeserializeObject<MinerConfig>(decodedJson);

                    settings = Form1.GetSettings();

                    for (int x = 0; x < settings.MinerSettings.Count(); x++)
                    {
                        if (settings.MinerSettings[x].Id == incomingMinerConfig.Id)
                        {
                            settings.MinerSettings[x] = incomingMinerConfig; // Overwrite old config
                            Form1.SaveSettings(settings);
                            break;
                        }
                    }                   

                    break;

                case "addMinerSetting":
                    
                    decodedJson = HttpUtility.UrlDecode(extra);
                    incomingMinerConfig = JsonConvert.DeserializeObject<MinerConfig>(decodedJson);

                    settings = Form1.GetSettings();
                    settings.MinerSettings.Add(incomingMinerConfig);

                    Form1.SaveSettings(settings);

                    break;

                case "removeMinerSetting":
                    
                    decodedJson = HttpUtility.UrlDecode(extra);
                    incomingMinerConfig = JsonConvert.DeserializeObject<MinerConfig>(decodedJson);

                    settings = Form1.GetSettings();

                    for (int x = 0; x < settings.MinerSettings.Count(); x++)
                    {
                        if (settings.MinerSettings[x].Id == incomingMinerConfig.Id)
                        {
                            settings.MinerSettings.Remove(settings.MinerSettings[x]);
                            Form1.SaveSettings(settings);
                            break;
                        }
                    }                    

                    break;

                case "getWallets":
                    settings = Form1.GetSettings();

                    resp = JsonConvert.SerializeObject(settings.Wallets);
                    httpResponse = CreateHttpResponse(resp);
                    writer.Write(httpResponse);

                    break;

                case "updateWallet":

                    decodedJson = HttpUtility.UrlDecode(extra);
                    incomingWallet = JsonConvert.DeserializeObject<Wallet>(decodedJson);

                    settings = Form1.GetSettings();

                    for (int x = 0; x < settings.Wallets.Count(); x++)
                    {
                        if (settings.Wallets[x].Id == incomingWallet.Id)
                        {
                            settings.Wallets[x] = incomingWallet;
                            Form1.SaveSettings(settings);
                            break;
                        }
                    }                    
                    break;

                case "addWallet":

                    decodedJson = HttpUtility.UrlDecode(extra);
                    incomingWallet = JsonConvert.DeserializeObject<Wallet>(decodedJson);

                    settings = Form1.GetSettings();
                    settings.Wallets.Add(incomingWallet);
                    Form1.SaveSettings(settings);
                    break;

                case "removeWallet":
                    decodedJson = HttpUtility.UrlDecode(extra);
                    incomingWallet = JsonConvert.DeserializeObject<Wallet>(decodedJson);

                    settings = Form1.GetSettings();

                    for (int x = 0; x < settings.Wallets.Count(); x++)
                    {
                        if (settings.Wallets[x].Id == incomingWallet.Id)
                        {
                            settings.Wallets.Remove(settings.Wallets[x]);
                            Form1.SaveSettings(settings);
                            break;
                        }
                    }                    
                    break;

                case "getPools":
                    settings = Form1.GetSettings();

                    resp = JsonConvert.SerializeObject(settings.Pools);
                    httpResponse = CreateHttpResponse(resp);
                    writer.Write(httpResponse);
                    break;

                case "updatePool":
                    decodedJson = HttpUtility.UrlDecode(extra);
                    incomingPool = JsonConvert.DeserializeObject<Pool>(decodedJson);

                    settings = Form1.GetSettings();

                    for (int x = 0; x < settings.Pools.Count(); x++)
                    {
                        if (settings.Pools[x].Id == incomingPool.Id)
                        {
                            settings.Pools[x] = incomingPool;
                            Form1.SaveSettings(settings);
                            break;
                        }
                    }                    
                    break;

                case "addPool":
                    decodedJson = HttpUtility.UrlDecode(extra);
                    incomingPool = JsonConvert.DeserializeObject<Pool>(decodedJson);

                    settings = Form1.GetSettings();
                    settings.Pools.Add(incomingPool);
                    Form1.SaveSettings(settings);
                    break;

                case "removePool":
                    decodedJson = HttpUtility.UrlDecode(extra);
                    incomingPool = JsonConvert.DeserializeObject<Pool>(decodedJson);

                    settings = Form1.GetSettings();

                    for (int x = 0; x < settings.Pools.Count(); x++)
                    {
                        if (settings.Pools[x].Id == incomingPool.Id)
                        {
                            settings.Pools.Remove(settings.Pools[x]);
                            Form1.SaveSettings(settings);
                            break;
                        }
                    }
                    
                    break;
                #endregion

                default:
                    break;
            }
        }
        #endregion


        #region Helpers
        // Helpers

        string LoadHtmlFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string htmlContent = File.ReadAllText(filePath);
                    return htmlContent;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private string CreateHttpResponse(string resp, string type = "text/plain")
        {
            StringBuilder sb = new StringBuilder();

            // Write the HTTP response header
            sb.AppendLine("HTTP/1.1 200 OK");
            sb.AppendLine($"Content-Type: {type}; charset=utf-8");
            sb.AppendLine("Content-Length: " + Encoding.UTF8.GetByteCount(resp));
            sb.AppendLine("Access-Control-Allow-Origin: *"); // Allow requests from any origin
            sb.AppendLine(); // Empty line to indicate the end of headers

            // Write the response content
            sb.AppendLine(resp);

            return sb.ToString();
        }
        private void CreateHttpResponse(byte[] imageData, StreamWriter writer)
        {
            // Write the HTTP response header
            writer.WriteLine("HTTP/1.1 200 OK");
            writer.WriteLine($"Content-Type: image/jpeg");
            writer.WriteLine("Content-Length: " + imageData.Length);
            writer.WriteLine("Access-Control-Allow-Origin: *"); // Allow requests from any origin
            writer.WriteLine(); // Empty line to indicate the end of headers
            writer.Flush(); // Flush the headers to the stream

            // Write the response content (image data)
            writer.BaseStream.Write(imageData, 0, imageData.Length);
            writer.Flush(); // Flush the image data to the stream
        }
        public string ExtractFilename(string httpRequest)
        {
            // Use a regular expression to find the image filename in the HTTP request string.
            // This pattern matches a string starting with "GET /", followed by any non-whitespace characters (the filename),
            // and ending with " HTTP/1.1" (assuming it's an HTTP/1.1 request). The filename is captured in a named group "filename".
            string pattern = @"GET /\s*(?<filename>\S+)\s*HTTP/1.1";

            // Perform the regular expression match on the httpRequest string.
            Match match = Regex.Match(httpRequest, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                // Retrieve the value of the named group "filename".
                string filename = match.Groups["filename"].Value;

                // Decode the URL-encoded filename, if present.
                filename = Uri.UnescapeDataString(filename);

                return filename;
            }

            // If no match is found, return null or an empty string, depending on your preference.
            return null;
        }
        public static IPAddress Get_Local_IP_Address()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var networkInterface in interfaces)
            {
                // Check if the interface is up and connected
                if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                    networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var properties = networkInterface.GetIPProperties();

                    foreach (var address in properties.UnicastAddresses)
                    {
                        // Check for IPv4 address
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            // Check if it has internet access
                            var ping = new Ping();
                            var reply = ping.Send("8.8.8.8", 1000); // Ping Google DNS server
                            if (reply.Status == IPStatus.Success)
                            {
                                return address.Address;
                            }
                        }
                    }
                }
            }

            return null; // No network interface with internet access found
        }
        #endregion
    }
}
