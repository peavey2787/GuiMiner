# GuiMiner
Manage CLI miners with a GUI and manage all .bat files in one place along with gpu settings, wallets, and pools.

![main](https://github.com/peavey2787/GuiMiner/assets/11081113/4bb1a642-7a8c-405f-83fa-8e9078cffb13)


Tips: kaspa:qpfsh8feaq5evaum5auq9c29fvjnun0mrzj5ht6sz3sz09ptcdaj6qjx9fkug

# How To Use

1. Add your favorite miner.exe file to this app's folder or get the path to it
2. Open the app
3. Click Settings
4. Click Wallets and add each of your wallets
5. Click Pools and add each of your pools
6. Click Add Miner Settings
7. Choose a miner, check __active__, and edit the settings (add miner path here)
8. Click Get All GPUs
9. Edit each GPU settings and check __enabled__
10. Click Add Gpu Settings
11. Click Generate in the bottom left
12. Close Settings
13. Click Start and all __active__ miner settings will start on all __enabled__ GPUs

Make sure to click Generate .bat file after making any changes and stop then start the miner to use the latest settings. If you want to set GPU clock settings then the app must be run as admin by right clicking Gui_Miner.exe and clicking "Run as administrator".

# Settings
![settings](https://github.com/peavey2787/GuiMiner/assets/11081113/b7527cea-e34d-426f-9e3a-46f122c40acb)

# Wallets
![wallets](https://github.com/peavey2787/GuiMiner/assets/11081113/9fa29829-625f-4108-9ebc-7142f9c74330)

# Pools
![pools](https://github.com/peavey2787/GuiMiner/assets/11081113/e9a60136-65a2-47b3-8ea9-5505f37f59c8)

# Windows Settings
![windows-settings](https://github.com/peavey2787/GuiMiner/assets/11081113/311642a5-fb72-4699-8f52-95edafe3bfdd)

# Tips

If you use MSI Afterburner you can set short-cut keys to change profiles. Create a profile for mining and another for gaming. Use the same short-cut keys in this app to switch profiles when stopping/starting. For example, if MSI Afterburner's short-cut keys for mining are Ctrl+Alt+1 and and for gaming Ctrl+Alt+2. Then in this app set the start short-cut keys to Ctrl+Alt+1 and the stop short-cut keys to Ctrl+Alt+2.

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
