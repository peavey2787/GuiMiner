using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gui_Miner
{
    public class GminerConfig
    {
        public string CommandPrefix { get; set; }
        public string CommandSeparator { get; set; }
        public string MinerFilePath { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool runAsAdmin { get; set; }
        public string ListDevicesCommand = "--list_devices";
        public string algo { get; set; } // --algo or -a
        public string server { get; set; } // --server or -s
        public int port { get; set; } // --port or -n
        public string user { get; set; } // --user or -u
        public string worker { get; set; } // --worker
        public string pass { get; set; } // --pass or -p
        public bool ssl { get; set; } // --ssl
        public string proxy { get; set; } // --proxy
        public string proto { get; set; } // --proto
        public string dag_mode { get; set; } // --dag_mode
        public string safe_dag { get; set; } // --safe_dag
        public string dag_limit { get; set; } // --dag_limit
        public bool cache_dag { get; set; } // --cache_dag
        public string dag_gen_limit { get; set; } // --dag_gen_limit
        public string devices { get; set; } // --devices or -d
        public string kernel { get; set; } // --kernel or -k
        public string fan { get; set; } // --fan
        public string mt { get; set; } // --mt
        public string pl { get; set; } // --pl
        public string cclock { get; set; } // --cclock
        public string mclock { get; set; } // --mclock
        public string cvddc { get; set; } // --cvddc
        public string lock_voltage { get; set; } // --lock_voltage
        public string lock_cclock { get; set; } // --lock_cclock
        public string lock_mclock { get; set; } // --lock_mclock
        public string zilmt { get; set; } // --zilmt
        public string zilpl { get; set; } // --zilpl
        public string zilcclock { get; set; } // --zilcclock
        public string zilmclock { get; set; } // --zilmclock
        public string zilcvddc { get; set; } // --zilcvddc
        public string zillock_voltage { get; set; } // --zillock_voltage
        public string zillock_cclock { get; set; } // --zillock_cclock
        public string zillock_mclock { get; set; } // --zillock_mclock
        public bool p2state { get; set; } // --p2state
        public string tfan { get; set; } // --tfan
        public string tfan_min { get; set; } // --tfan_min
        public string tfan_max { get; set; } // --tfan_max
        public string logfile { get; set; } // --logfile or -l
        public bool log_date { get; set; } // --log_date
        public bool log_stratum { get; set; } // --log_stratum
        public bool log_newjob { get; set; } // --log_newjob
        public string templimit { get; set; } // --templimit or -t
        public string templimit_mem { get; set; } // --templimit_mem or -tm
        public bool watchdog { get; set; } // --watchdog or -w
        public int watchdog_restart_delay { get; set; } // --watchdog_restart_delay
        public int watchdog_mode { get; set; } // --watchdog_mode
        public double min_rig_speed { get; set; } // --min_rig_speed
        public int report_interval { get; set; } // --report_interval
        public int api { get; set; } // --api
        public string config { get; set; } // --config
        public string pers { get; set; } // --pers
        public bool pec { get; set; } // --pec
        public double electricity_cost { get; set; } // --electricity_cost
        public string intensity { get; set; } // --intensity or -i
        public bool share_check { get; set; } // --share_check
        public bool nvml { get; set; } // --nvml
        public bool cuda { get; set; } // --cuda
        public bool opencl { get; set; } // --opencl
        public bool secure_dns { get; set; } // --secure_dns
        public string dalgo { get; set; } // --dalgo
        public string dserver { get; set; } // --dserver
        public int dport { get; set; } // --dport
        public string duser { get; set; } // --duser
        public string dpass { get; set; } // --dpass
        public bool dssl { get; set; } // --dssl
        public string dworker { get; set; } // --dworker
        public string dual_intensity { get; set; } // --dual_intensity or -di
        public string zilserver { get; set; } // --zilserver
        public int zilport { get; set; } // --zilport
        public string ziluser { get; set; } // --ziluser
        public bool zilssl { get; set; } // --zilssl

        public GminerConfig ()
        {
            CommandPrefix = "--";
            CommandSeparator = " ";
            cuda = true;
            opencl = true;
            electricity_cost = 0.15;
            report_interval = 30;
        }

        public void ClearGpuSettings()
        {
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
        }
        public void AddGpuSettings(List<Gpu> gpus)
        {
            ClearGpuSettings();

            // Required for Nvidia overclocking
            nvml = true;

            foreach (Gpu gpu in gpus)
            {
                if (!gpu.Enabled) continue;

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
            }
        }

        public int GetApiPort()
        {
            return api;
        }
    }
}
