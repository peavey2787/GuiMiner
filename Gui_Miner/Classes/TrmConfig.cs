using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Gui_Miner
{
    public class TrmConfig : MinerConfig
    {
        public string ListDevicesCommand = "--list_devices";
        private string secondAlgo = "";
        public string algo { get; set; }
        public bool benchmark { get; set; }
        public int api_listen { get; set; }
        public int api2_listen { get; set; }
        public int api3_listen { get; set; }
        public string cm_api_listen { get; set; }
        public string cm_api_password { get; set; }
        public string log_file { get; set; }
        public int log_interval { get; set; }
        public string log_rotate { get; set; }
        public bool short_stats { get; set; }
        public string dev_location { get; set; }
        public bool enable_compute { get; set; }
        public bool long_timestamps { get; set; }
        public bool restart_gpus { get; set; }
        public bool uac { get; set; }
        public bool high_score { get; set; }
        public bool allow_dup_bus_ids { get; set; }
        public string dns_https { get; set; }
        public string dns_https_sni { get; set; }
        public string kernel_vm_mode { get; set; }
        public string kernel_vm_mode_script { get; set; }
        public string gpu_sdma { get; set; }
        public string event_script { get; set; }
        public bool use_distro_features { get; set; }
        public bool no_distro_features { get; set; }
        public string url { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public bool pool_force_ensub { get; set; }
        public bool pool_broken_rpc { get; set; }
        public string pool_ratio { get; set; }
        public bool pool_debug { get; set; }
        public bool pool_comb_sub_auth { get; set; }
        public int pool_connect_TO { get; set; }
        public int pool_rpc_TO { get; set; }
        public int pool_max_rejects { get; set; }
        public int pool_share_limit_ms { get; set; }
        public string pool_strategy { get; set; }
        public bool no_ntime_roll { get; set; }
        public bool no_stale_submit { get; set; }
        public string devices { get; set; }
        public int init_style { get; set; }
        public string pcie_fmt { get; set; }
        public bool bus_reorder { get; set; }
        public bool opencl_order { get; set; }
        public string nr_cu_override { get; set; }
        public bool clk_debug { get; set; }
        public string clk_core_mhz { get; set; }
        public string clk_core_mv { get; set; }
        public string clk_mem_mhz { get; set; }
        public string clk_mem_mv { get; set; }
        public string clk_timing { get; set; }
        public string fan_control { get; set; }
        public string fan_default_polaris { get; set; }
        public string fan_default_vega { get; set; }
        public string fan_default_vega2 { get; set; }
        public string fan_default_navi { get; set; }
        public string fan_default_big_navi { get; set; }
        public bool fan_debug { get; set; }
        public bool fan_no_restore { get; set; }
        public bool no_gpu_monitor { get; set; }
        public string temp_limit { get; set; }
        public string temp_resume { get; set; }
        public string mem_temp_limit { get; set; }
        public string mem_temp_resume { get; set; }
        public string watchdog_script { get; set; }
        public bool watchdog_test { get; set; }
        public bool watchdog_disabled { get; set; }
        public string eth_config { get; set; }
        public string zilPool { get; set; }
        public int zilPort { get; set; }
        public string zilWallet { get; set; }
        public string kasPool { get; set; }
        public int kasPort { get; set; }
        public string kasWallet { get; set; }
        public string ironFishPool { get; set; }
        public int ironFishPort { get; set; }
        public string ironFishWallet { get; set; }
        public string dual_intensity { get; set; }
        public string dual_tuner_step { get; set; }
        public string dual_tuner_period { get; set; }
        public string dual_tuner_weights { get; set; }
        public string fpga_devices { get; set; }
        public string fpga_clk_core { get; set; }
        public string fpga_clk_core_init { get; set; }
        public string fpga_clk_mem { get; set; }
        public string fpga_vcc_int { get; set; }
        public string fpga_vcc_bram { get; set; }
        public string fpga_vcc_mem { get; set; }
        public string fpga_er_max { get; set; }
        public string fpga_er_auto { get; set; }
        public string fpga_tcore_limit { get; set; }
        public string fpga_tmem_limit { get; set; }
        public string fpga_ivccint_limit { get; set; }
        public string fpga_ivccbram_limit { get; set; }
        public string fpga_max_jtag_mhz { get; set; }
        public bool fpga_update_fw { get; set; }
        public string fpga_allow_unsafe { get; set; }
        public string fpga_eth_clk_dag { get; set; }
        public bool fpga_e300 { get; set; }
        public string fpga_jc_addr { get; set; }

        public TrmConfig() 
        {
            Command_Prefix = "--";
            Command_Separator = ',';
        }


        // Interface Required
        public MinerConfigType GetMinerConfigType()
        {
            return MinerConfigType.Gminer;
        }

        #region Property Overrides
        // Property Overrides
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
            get 
            {
                string algo = "";

                if(!string.IsNullOrWhiteSpace(secondAlgo))
                    algo = secondAlgo;
                else if (!string.IsNullOrWhiteSpace(kasWallet) || !string.IsNullOrWhiteSpace(kasPool))
                    algo = "kaspa";
                else if (!string.IsNullOrWhiteSpace(ironFishWallet) || !string.IsNullOrWhiteSpace(ironFishPool))
                    algo = "ironfish";

                return algo; 
            }
            set
            {
                base.Algo2 = value;
                secondAlgo = value;
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
            get 
            {
                string wallet = "";

                if (Algo2.Contains("kaspa"))
                    wallet = kasWallet;
                else if (Algo2.Contains("ironfish"))
                    wallet = ironFishWallet;

                return wallet; 
            }
            set
            {
                base.Wallet2 = value;

                if (Algo2.Contains("kaspa"))
                    kasWallet = value;
                else if (Algo2.Contains("ironfish"))
                    ironFishWallet = value;
            }
        }
        public override string Wallet3
        {
            get { return zilWallet; }
            set
            {
                base.Wallet3 = value;
                zilWallet = value;
            }
        }
        public override string Pool1
        {
            get { return url; }
            set
            {
                base.Pool1 = value;
                url = value;
            }
        }
        public override string Pool2
        {
            get 
            {
                string pool = "";

                if (Algo2.Contains("kaspa"))
                    pool = kasPool;
                else if (Algo2.Contains("ironfish"))
                    pool = ironFishPool;

                return pool;
            }
            set
            {
                base.Pool2 = value;

                if (Algo2.Contains("kaspa"))
                    kasPool = value;
                else if (Algo2.Contains("ironfish"))
                    ironFishPool = value;
            }
        }
        public override string Pool3
        {
            get { return zilWallet; }
            set
            {
                base.Pool3 = value;
                zilWallet = value;
            }
        }
        public override int Port2
        {
            get 
            {
                int port = -1;

                if (Algo2.Contains("kaspa"))
                    port = kasPort;
                else if (Algo2.Contains("ironfish"))
                    port = ironFishPort;

                return port; 
            }
            set
            {
                base.Port2 = value;

                if (Algo2.Contains("kaspa"))
                    kasPort = value;
                else if (Algo2.Contains("ironfish"))
                    ironFishPort = value;
            }
        }
        public override int Port3
        {
            get { return zilPort; }
            set
            {
                base.Port3 = value;
                zilPort = value;
            }
        }
        public override int Api
        {
            get { return api_listen; }
            set
            {
                base.Api = value;
                api_listen = value;
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
        public override List<int> Fan_Percent
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(fan_control);
                return allValues;
            }
            set
            {
                base.Fan_Percent = value;
                fan_control = ConvertListToStr(value);
            }
        }
        public override List<int> Temp_Limit_Core
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(temp_limit);
                return allValues;
            }
            set
            {
                base.Temp_Limit_Core = value;
                temp_limit = ConvertListToStr(value);
            }
        }
        public override List<int> Temp_Limit_Mem
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(mem_temp_limit);
                return allValues;
            }
            set
            {
                base.Temp_Limit_Mem = value;
                mem_temp_limit = ConvertListToStr(value);
            }
        }
        public override List<int> Core_Clock
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(clk_core_mhz);
                return allValues;
            }
            set
            {
                base.Core_Clock = value;
                clk_core_mhz = ConvertListToStr(value);
            }
        }
        public override List<int> Mem_Clock
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(clk_mem_mhz);
                return allValues;
            }
            set
            {
                base.Mem_Clock = value;
                clk_mem_mhz = ConvertListToStr(value);
            }
        }
        public override List<int> Core_Micro_Volts
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(clk_core_mv);
                return allValues;
            }
            set
            {
                base.Core_Micro_Volts = value;
                clk_core_mv = ConvertListToStr(value);
            }
        }
        public override List<int> Mem_Micro_Volts
        {
            get
            {
                List<int> allValues = ConvertStrToIntList(clk_mem_mv);
                return allValues;
            }
            set
            {
                base.Mem_Micro_Volts = value;
                clk_mem_mv = ConvertListToStr(value);
            }
        }

        #endregion

        #region Function Overrides
        public override void AddGpuSettings(List<Gpu> gpus)
        {
            base.AddGpuSettings(gpus);

            dual_intensity = "";
        }
        public override void ClearGpuSettings()
        {
            base.ClearGpuSettings();

            dual_intensity = "";
        }
        public string GenerateBatFileArgs()
        {
            string args = "";
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                // Skip base class items
                if (property.DeclaringType != type)
                    continue;

                string propertyName = property.Name;
                object propertyValue = property.GetValue(this);

                // skip
                if (propertyValue == null || String.IsNullOrWhiteSpace(propertyValue.ToString())
                    || propertyValue.ToString().Trim() == "-1") continue;


                // Add "" around file path
                if (propertyName.Equals("Miner_File_Path"))
                {
                    args += $"\"{propertyValue}\" ";
                }


                // ---------Trm Specific---------
                if (propertyName.Equals("fan_control"))
                    propertyValue = ":::" + propertyValue;
                // ---------End Trm Specific---------


                // List<int>
                if (propertyValue != null && propertyValue.GetType().IsGenericType &&
                    propertyValue.GetType().GetGenericTypeDefinition() == typeof(List<>) &&
                    propertyValue.GetType().GetGenericArguments()[0] == typeof(int))
                {
                    List<int> nums = (List<int>)propertyValue;

                    // Only add nums >= 0
                    if (nums.Count > 0 && nums.First() >= 0)
                        args += $"{Command_Prefix}{propertyName} {ConvertListToStr(nums)}";
                }
                else // Default name value
                {
                    // Replace any spaces/commas with separator
                    if (propertyValue.ToString().Contains(' ') || propertyValue.ToString().Contains(','))
                        propertyValue = propertyValue.ToString().Replace(' ', Command_Separator);

                    args += $"{Command_Prefix}{propertyName} {propertyValue} ";
                }
            }

            return args.Trim();
        }
        #endregion
    }
}
