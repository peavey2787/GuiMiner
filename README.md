# GuiMiner
 Manage CLI miners with a GUI and manage all .bat files in one place along with gpu settings

# How To Use

1. Add your favorite miner.exe file to this app's folder
2. Click Settings in the top right then Get All GPUs in the bottom right
3. Edit each GPU settings
4. Click Add Miner Settings in the top left
5. Choose a miner and edit the settings
6. Click Add Gpu Settings
7. Click Generate in the bottom left
8. Close Settings
9. Click Start and all Active miners will start

Make sure to click Generate .bat file after making any changes and stop then start the miner to use the latest settings. If you want to set GPU clock settings then the app must be run as admin by right clicking Gui_Miner.exe and clicking "Run as administrator".

# FAQ
## Why is the miner not starting?

1. Make sure you have checked the box for "Active".
2. Either check-mark "cuda" or "opencl" to use Nvidia/Amd devices.
3. Ensure your anti-virus isn't blocking the miner.
4. Check your firewall settings to allow the miner to communicate.
5. Read the documentation for your miner to double check the syntax as it could have been updated.

## Why does it keep failing to set the overclock and asking me to run as admin?

In order for the miner to set the GPU settings it must be run as admin and in order for this app to run the miner as admin this app must also be run as admin. Otherwise the app will continue to run without admin privliges therefore so will the miner and that is commonly why it is failing.

## Why are the GPUs not getting added when I click "Add GPU Settings"?

Ensure the "Enabled" check-box is checked and that there are no typos or unwanted characters/numbers.
   
## What are CommandPrefix/CommandSeparator used for?

CommandPrefix is what will be put in front of each miner command and CommandSeparator is what is used to separate multiple command parameters.

## Why are the stats not showing on Gminer?

Make sure "report_interval" is greater than 0. Default Gminer uses 30 for 30 seconds.
