using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gui_Miner
{
    public class TrmConfig
    {
        public string CommandPrefix { get; set; }
        public string CommandSeparator { get; set; }
        public string MinerFilePath { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool runAsAdmin { get; set; }
        public string ListDevicesCommand = "--list_devices";
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
            CommandPrefix = "--";
            CommandSeparator = ",";
        }
        public void ClearGpuSettings()
        {
            
            devices = "";
            clk_core_mhz = "";
            clk_core_mv = "";
            clk_mem_mhz = "";
            clk_mem_mv = "";
            fan_control = "";
            temp_limit = "";
            mem_temp_limit = "";
            dual_intensity = "";            
        }
        public void AddGpuSettings(List<Gpu> gpus)
        {
            ClearGpuSettings();

            foreach (Gpu gpu in gpus)
            {
                if (!gpu.Enabled) continue;
                
                // Add values
                devices += gpu.Device_Id.ToString();
                clk_core_mhz += gpu.Core_Clock.ToString();
                clk_core_mv += gpu.Core_Mv.ToString();
                clk_mem_mhz += gpu.Mem_Clock_Offset.ToString();
                clk_mem_mv += gpu.Mem_Mv.ToString();
                fan_control += ":::" + gpu.Fan_Percent.ToString();
                temp_limit += gpu.Max_Core_Temp.ToString();
                mem_temp_limit += gpu.Max_Mem_Temp.ToString();
                dual_intensity += gpu.Dual_Intensity.ToString();

                // Add separators
                devices += CommandSeparator;
                clk_core_mhz += CommandSeparator;
                clk_core_mv += CommandSeparator;
                clk_mem_mhz += CommandSeparator;
                clk_mem_mv += CommandSeparator;
                fan_control += CommandSeparator;
                temp_limit += CommandSeparator;
                mem_temp_limit += CommandSeparator;
                dual_intensity += CommandSeparator;                
            }
        }
        public int GetApiPort()
        {
            return api_listen;
        }
        public string GetPoolDomainName1()
        {
            var parts = url.Trim().Split('.');

            // mining url
            if (parts.Length == 3)
            {
                return parts[1] + "." + parts[2];
            }
            // pool url
            else if (parts.Length == 2)
            {
                return parts[0] + "." + parts[1];
            }
            return url.Trim();
        }
    }
}
