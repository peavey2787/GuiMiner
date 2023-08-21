using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gui_Miner
{
    public class UnknownConfig
    {
        public string CommandPrefix { get; set; }
        public string CommandSeparator { get; set; }
        public string MinerFilePath { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool runAsAdmin { get; set; }
        public string ListDevicesCommand = "--list_devices";
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
        public int Api { get; set; }
        public UnknownConfig()
        {
            CommandPrefix = "--";
            CommandSeparator = " ";
        }

        public void ClearGpuSettings()
        {
            /*
            devices = "";
            cclock = "";
            lock_cclock = "";
            mclock = "";
            lock_mclock = "";
            pl = "";
            fan = "";
            templimit = "";
            templimit_mem = "";
            intensity = "";
            dual_intensity = "";
            nvml = false;
            */
        }
        public void AddGpuSettings(List<Gpu> gpus)
        {
            ClearGpuSettings();

            foreach (Gpu gpu in gpus)
            {
                if (!gpu.Enabled) continue;
                /*
                // Add values
                devices += gpu.Device_Id.ToString();
                lock_cclock += gpu.Core_Clock.ToString();
                lock_mclock += gpu.Mem_Clock_Offset.ToString();
                pl += gpu.Power_Limit.ToString();
                fan += gpu.Fan_Percent.ToString();
                templimit += gpu.Max_Core_Temp.ToString();
                templimit_mem += gpu.Max_Mem_Temp.ToString();
                intensity += gpu.Intensity.ToString();
                dual_intensity += gpu.Dual_Intensity.ToString();

                // Add separators
                devices += CommandSeparator;
                lock_cclock += CommandSeparator;
                lock_mclock += CommandSeparator;
                pl += CommandSeparator;
                fan += CommandSeparator;
                templimit += CommandSeparator;
                templimit_mem += CommandSeparator;
                intensity += CommandSeparator;
                dual_intensity += CommandSeparator;
                */
            }
        }
        public int GetApiPort()
        {
            return Api;
        }
        public string GetPoolDomainName1()
        {
            var parts = Pool1.Trim().Split('.');
            
            // mining url
            if(parts.Length == 3)
            {
                return parts[1] + "." + parts[2];
            }
            // pool url
            else if(parts.Length == 2)
            {
                return parts[0] + "." + parts[1];    
            }
            return Pool1.Trim();
        }
    }
}
