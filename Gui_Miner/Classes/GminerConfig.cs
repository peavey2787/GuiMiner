using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gui_Miner
{
    public class GminerConfig //: MinerConfig
    {
        #region Properties/Fields
        public List<Gpu> Gpus { get; set; }
        /*
        public string algo { get; set; } // --algo or -a
        public string server { get; set; } // --server or -s
        public int port { get; set; } = -1; // --port or -n
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
        public int watchdog_restart_delay { get; set; } = -1; // --watchdog_restart_delay
        public int watchdog_mode { get; set; } = -1;// --watchdog_mode
        public double min_rig_speed { get; set; } = -1; // --min_rig_speed
        public int report_interval { get; set; } = 30; // --report_interval
        public int api { get; set; } = -1; // --api
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
        public int dport { get; set; } = -1; // --dport
        public string duser { get; set; } // --duser
        public string dpass { get; set; } // --dpass
        public bool dssl { get; set; } // --dssl
        public string dworker { get; set; } // --dworker
        public string dual_intensity { get; set; } // --dual_intensity or -di
        public string zilserver { get; set; } // --zilserver
        public int zilport { get; set; } = -1;// --zilport
        public string ziluser { get; set; } // --ziluser
        public bool zilssl { get; set; } // --zilssl
        */
        #endregion
        public GminerConfig ()
        {
            /*Command_Prefix = "--";
            Command_Separator = ' ';
            List_Devices_Command = "--list_devices";
            cuda = true; // Enable nvidia gpus
            opencl = true; // Enable amd gpus
            electricity_cost = 0.15;
            report_interval = 30;*/
        }

        // Interface Required
        /*public MinerConfigType GetMinerConfigType()
        {
            return MinerConfigType.Gminer;
        }*/

        #region Property Overrides
        // Property Overrides
        /*
        public override string Worker_Name
        {
            get { return worker; }
            set
            {
                // Set the base class's var to the new value
                base.Worker_Name = value;
                // Set the sub-class's var to the same value
                worker = value;
            }
        }
        public override string Algo1
        {
            get { return algo; }
            set
            {
                base.Algo1 = value;
                algo = value;
            }
        }
        public override string Algo2
        {
            get { return dalgo; }
            set
            {
                base.Algo2 = value;
                dalgo = value;
            }
        }
        public override string Algo3
        {
            get { return "zilliqa"; }
            set
            {
                base.Algo3 = value;                
            }
        }
        public override string Wallet1
        {
            get { return user; }
            set
            {
                base.Wallet1 = value;
                user = value;
            }
        }
        public override string Wallet2
        {
            get { return duser; }
            set
            {
                base.Wallet2 = value;
                duser = value;
            }
        }
        public override string Wallet3
        {
            get { return ziluser; }
            set
            {
                base.Wallet3 = value;
                ziluser = value;
            }
        }
        public override string Pool1
        {
            get { return server; }
            set
            {
                base.Pool1 = value;
                server = value;
            }
        }
        public override string Pool2
        {
            get { return dserver; }
            set
            {
                base.Pool2 = value;
                dserver = value;
            }
        }
        public override string Pool3
        {
            get { return zilserver; }
            set
            {
                base.Pool3 = value;
                zilserver = value;
            }
        }
        public override int Port1
        {
            get { return port; }
            set
            {
                base.Port1 = value;
                port = value;
            }
        }
        public override int Port2
        {
            get { return dport; }
            set
            {
                base.Port2 = value;
                dport = value;
            }
        }
        public override int Port3
        {
            get { return zilport; }
            set
            {
                base.Port3 = value;
                zilport = value;
            }
        }
        public override bool SSL1
        {
            get { return ssl; }
            set
            {
                base.SSL1 = value;
                ssl = value;
            }
        }
        public override bool SSL2
        {
            get { return dssl; }
            set
            {
                base.SSL2 = value;
                dssl = value;
            }
        }
        public override bool SSL3
        {
            get { return zilssl; }
            set
            {
                base.SSL3 = value;
                zilssl = value;
            }
        }
        public override int Api
        {
            get { return api; }
            set
            {
                base.Api = value;
                api = value;
            }
        }
        public override List<int> Devices
        {
            get 
            {
                List<int> allDevicesToUse = ConvertStrToIntList(devices);  
                return allDevicesToUse; 
            }
            set
            {
                base.Devices = value;
                devices = ConvertListToStr(value);
            }
        }
        public override List<int> Power_Limit
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(pl);
                return allValues;
            }
            set
            {
                base.Power_Limit = value;
                pl = ConvertListToStr(value);
            }
        }
        public override List<int> Fan_Percent
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(fan);
                return allValues;
            }
            set
            {
                base.Fan_Percent = value;
                fan = ConvertListToStr(value);
            }
        }
        public override List<int> Temp_Limit_Core
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(templimit);
                return allValues;
            }
            set
            {
                base.Temp_Limit_Core = value;
                templimit = ConvertListToStr(value);
            }
        }
        public override List<int> Temp_Limit_Mem
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(templimit_mem);
                return allValues;
            }
            set
            {
                base.Temp_Limit_Mem = value;
                templimit_mem = ConvertListToStr(value);
            }
        }
        public override List<int> Core_Clock
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(lock_cclock);
                return allValues;
            }
            set
            {
                base.Core_Clock = value;
                lock_cclock = ConvertListToStr(value);
            }
        }
        public override List<int> Core_Offset
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(cclock);
                return allValues;
            }
            set
            {
                base.Core_Offset = value;
                cclock = ConvertListToStr(value);
            }
        }
        public override List<int> Mem_Clock
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(lock_mclock);
                return allValues;
            }
            set
            {
                base.Mem_Clock = value;
                lock_mclock = ConvertListToStr(value);
            }
        }
        public override List<int> Mem_Offset
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(mclock);
                return allValues;
            }
            set
            {
                base.Mem_Clock = value;
                mclock = ConvertListToStr(value);
            }
        }
        public override List<int> Core_Micro_Volts
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(lock_voltage);
                return allValues;
            }
            set
            {
                base.Core_Micro_Volts = value;
                lock_voltage = ConvertListToStr(value);
            }
        }
        // There's no Mem Volts in gminer yet
        */
        #endregion

        #region Function Overrides
        /*public override string GenerateBatFileArgs()
        {
            string args = "";

            // Add "" around file path
            args += $"\"{Miner_File_Path}\" ";            

            // 1st Algo
            args += $"--algo {Algo1} ";
            args += $"--ssl {SSL1} ";
            args += $"--server {Pool1} ";
            args += $"--port {Port1} ";            
            args += $"--user {Wallet1}.{Worker_Name} ";

            // 2nd Algo
            args += $"--dalgo {Algo2} ";
            args += $"--dssl {SSL2} ";
            args += $"--dserver {Pool2} ";
            args += $"--dport {Port2} ";
            args += $"--duser {Wallet2}.{Worker_Name} ";

            // 3rd Algo
            args += $"--zilssl {SSL3} ";
            args += $"--zilserver {Pool3} ";
            args += $"--zilport {Port3} ";
            args += $"--ziluser {Wallet3}.{Worker_Name} ";

            // Gpus
            bool overclocking = false;

            if (Devices.Any(item => item >= 0))
                args += $"--devices {ConvertListToStr(Devices)} ";

            // Clock
            if (Core_Clock.Any(item => item >= 0))
            {
                args += $"--lock_cclock {ConvertListToStr(Core_Clock)} ";
                overclocking = true;
            }
            else if (Core_Offset.Any(item => item >= 0))
            {
                args += $"--cclock {ConvertListToStr(Core_Offset)} ";
                overclocking = true;
            }
            if (Core_Micro_Volts.Any(item => item >= 0))
            {
                args += $"--lock_voltage {ConvertListToStr(Core_Micro_Volts)} ";
                overclocking = true;
            }

            // Mem
            if (Mem_Clock.Any(item => item >= 0))
            {
                args += $"--lock_mclock {ConvertListToStr(Mem_Clock)} ";
                overclocking = true;
            }
            else if (Mem_Offset.Any(item => item >= 0))
            {
                args += $"--mclock {ConvertListToStr(Mem_Offset)} ";
                overclocking = true;
            }

            // Required for Nvidia
            if (overclocking)
                args += "--nvml 1";

            if (Power_Limit.Any(item => item >= 0))
                args += $"--pl {ConvertListToStr(Power_Limit)} ";

            if (Fan_Percent.Any(item => item >= 0))
                args += $"--fan {ConvertListToStr(Fan_Percent)} ";

            if (Temp_Limit_Core.Any(item => item >= 0))
                args += $"--templimit {ConvertListToStr(Temp_Limit_Core)} ";

            if (Temp_Limit_Mem.Any(item => item >= 0))
                args += $"--templimit_mem {ConvertListToStr(Temp_Limit_Mem)} ";
            
            if (Intensity.Any(item => item >= 0))
                args += $"--intensity {ConvertListToStr(Intensity)} ";

            if (Dual_Intensity.Any(item => item >= 0))
                args += $"--dual_intensity {ConvertListToStr(Dual_Intensity)} ";           

            return args.Trim();
        }*/
        #endregion

    }
}
