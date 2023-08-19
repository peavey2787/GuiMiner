# GuiMiner
 Manage CLI miners with a GUI and manage all .bat files in one place along with gpu settings

# FAQ
## Why is the miner not starting?

1. Make sure you have checked the box for "Active".
2. Either check-mark "cuda" or "opencl" to use Nvidia/Amd devices.
3. Ensure your anti-virus isn't blocking the miner.
4. Check your firewall settings to allow the miner to communicate.
5. Read the documentation for your miner to double check the syntax as it could have been updated.
   
## What are CommandPrefix/CommandSeparator used for?

CommandPrefix is what will be put in front of each miner command and CommandSeparator is what is used to separate multiple command parameters.

## Why are the stats not showing on Gminer?

Make sure "report_interval" is greater than 0. Default Gminer uses 30 for 30 seconds.
