using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gui_Miner
{
    internal class GminerConfig : MinerConfig
    {
        public string Algo { get; set; } // --algo or -a
        public string Server { get; set; } // --server or -s
        public int Port { get; set; } // --port or -n
        public string User { get; set; } // --user or -u
        public string Worker { get; set; } // --worker
        public string Password { get; set; } // --pass or -p
        public bool Ssl { get; set; } // --ssl
        public string Proxy { get; set; } // --proxy
        public string Proto { get; set; } // --proto
        public string DagMode { get; set; } // --dag_mode
        public string SafeDag { get; set; } // --safe_dag
        public string DagLimit { get; set; } // --dag_limit
        public bool CacheDag { get; set; } // --cache_dag
        public string DagGenLimit { get; set; } // --dag_gen_limit
        public string Devices { get; set; } // --devices or -d
        public string Kernel { get; set; } // --kernel or -k
        public string Fan { get; set; } // --fan
        public string MemoryTweak { get; set; } // --mt
        public string PowerLimit { get; set; } // --pl
        public string CoreClock { get; set; } // --cclock
        public string MemoryClock { get; set; } // --mclock
        public string CoreVoltage { get; set; } // --cvddc
        public string LockVoltage { get; set; } // --lock_voltage
        public string LockCoreClock { get; set; } // --lock_cclock
        public string LockMemoryClock { get; set; } // --lock_mclock
        public string ZilMemoryTweak { get; set; } // --zilmt
        public string ZilPowerLimit { get; set; } // --zilpl
        public string ZilCoreClock { get; set; } // --zilcclock
        public string ZilMemoryClock { get; set; } // --zilmclock
        public string ZilCoreVoltage { get; set; } // --zilcvddc
        public string ZilLockVoltage { get; set; } // --zillock_voltage
        public string ZilLockCoreClock { get; set; } // --zillock_cclock
        public string ZilLockMemoryClock { get; set; } // --zillock_mclock
        public bool P2State { get; set; } // --p2state
        public string TargetFan { get; set; } // --tfan
        public string MinFanSpeed { get; set; } // --tfan_min
        public string MaxFanSpeed { get; set; } // --tfan_max
        public string Logfile { get; set; } // --logfile or -l
        public bool LogDate { get; set; } // --log_date
        public bool LogStratum { get; set; } // --log_stratum
        public bool LogNewJob { get; set; } // --log_newjob
        public string TempLimit { get; set; } // --templimit or -t
        public string TempLimitMem { get; set; } // --templimit_mem or -tm
        public bool ColorOutput { get; set; } // --color or -c
        public bool Watchdog { get; set; } // --watchdog or -w
        public int WatchdogRestartDelay { get; set; } // --watchdog_restart_delay
        public int WatchdogMode { get; set; } // --watchdog_mode
        public double MinRigSpeed { get; set; } // --min_rig_speed
        public int ReportInterval { get; set; } // --report_interval
        public int ApiPort { get; set; } // --api
        public string ConfigFile { get; set; } // --config
        public string PersonalizationString { get; set; } // --pers
        public bool PowerEfficiencyCalculator { get; set; } // --pec
        public double ElectricityCost { get; set; } // --electricity_cost
        public string Intensity { get; set; } // --intensity or -i
        public bool ShareCheck { get; set; } // --share_check
        public bool Nvml { get; set; } // --nvml
        public bool Cuda { get; set; } // --cuda
        public bool OpenCL { get; set; } // --opencl
        public bool SecureDns { get; set; } // --secure_dns
        public string MaintenanceServer { get; set; } // --maintenance_server
        public int MaintenancePort { get; set; } // --maintenance_port
        public string MaintenanceUser { get; set; } // --maintenance_user
        public string MaintenancePass { get; set; } // --maintenance_pass
        public bool MaintenanceSsl { get; set; } // --maintenance_ssl
        public string MaintenanceProto { get; set; } // --maintenance_proto
        public string MaintenanceWorker { get; set; } // --maintenance_worker
        public double MaintenanceFee { get; set; } // --maintenance_fee
        public string DualMiningAlgo { get; set; } // --dalgo
        public string DualServer { get; set; } // --dserver
        public int DualPort { get; set; } // --dport
        public string DualUser { get; set; } // --duser
        public string DualPass { get; set; } // --dpass
        public bool DualSsl { get; set; } // --dssl
        public string DualWorker { get; set; } // --dworker
        public string DualIntensity { get; set; } // --dual_intensity or -di
        public string ZilServer { get; set; } // --zilserver
        public int ZilPort { get; set; } // --zilport
        public string ZilUser { get; set; } // --ziluser
        public bool ZilSsl { get; set; } // --zilssl

        public GminerConfig ()
        {
            Name = "Gminer";
        }
    }
}
