> **Warning** Windows' anti-virus erroneously claims this app as potentially containing malware, but you can see every line of code for yourself to verify that it does not.

# GuiMiner
Manage CLI miners with a GUI and manage all .bat files in one place along with gpu settings, wallets, and pools.

![screenshot](https://github.com/peavey2787/GuiMiner/assets/11081113/85c4b90c-fbe6-480e-a676-266787db3613)

Tips: kaspa:qpfsh8feaq5evaum5auq9c29fvjnun0mrzj5ht6sz3sz09ptcdaj6qjx9fkug

# How To Use

1. Open the app
2. Click Settings
3. Click Wallets and add each of your wallets
4. Click Pools and add each of your pools
5. Go to Miner Settings and click Add Miner Settings
7. Choose a miner and click the textbox for the Miner File Path to open a pop-up to select the miner's .exe file
8. Select the algos, wallets, and pools to use
9. Click Get All GPUs
10. Edit each GPUs settings
11. Click Add Gpu Settings
12. Click Generate in the bottom left
13. Close Settings
14. Click Start and all __active__ miner settings will start

Make sure to click Generate .bat file after making any changes and stop then start the miner to use the latest settings. If you want to set GPU clock settings then the app must be run as admin by right clicking Gui_Miner.exe and clicking "Run as administrator".

# Settings
![settings](https://github.com/peavey2787/GuiMiner/assets/11081113/b7527cea-e34d-426f-9e3a-46f122c40acb)

# Wallets
![wallets](https://github.com/peavey2787/GuiMiner/assets/11081113/9fa29829-625f-4108-9ebc-7142f9c74330)

# Pools
![pools](https://github.com/peavey2787/GuiMiner/assets/11081113/df4b6b8b-56d5-4287-872e-004459dd6622)

# Windows Settings
![windows-settings](https://github.com/peavey2787/GuiMiner/assets/11081113/311642a5-fb72-4699-8f52-95edafe3bfdd)

# Tips

If you use MSI Afterburner you can set short-cut keys to change profiles. Create a profile for mining and another for gaming. Use the same short-cut keys in this app to switch profiles when stopping/starting. For example, if MSI Afterburner's short-cut keys for mining are Ctrl+Alt+1 and and for gaming Ctrl+Alt+2. Then in this app set the start short-cut keys to Ctrl+Alt+1 and the stop short-cut keys to Ctrl+Alt+2.

# Web Control

Navigate to the LAN IP for the PC running the app. (Note: you must be on the same LAN network) 
Click the link and change the IP: [http://192.168.1.5:46543](http://192.168.1.5:46543/)
All PC's running this app, and also on the same LAN network, will show up on this page I've dubbed the Dashboard page. Click on one and it will take you to the Controls page where you can control the app.

The name in the dashboard next to the IP is the PC's name. In order to change this name it must be done on the computer in the Windows settings.

Clicking The Checkmart will make the miner setting active or inactive depending on the color, i.e. red is inactive and green is active. Clicking this will not stop or start an active miner setting.
![screenshot](https://github.com/peavey2787/GuiMiner/assets/11081113/c7ca52cb-cfe5-43c1-9149-e89ff220e77f)


# FAQ

## Why is the miner not starting?

1. Make sure you have checked the box for "Active" in Miner Settings.
2. Ensure your anti-virus isn't blocking the miner.
3. Check your firewall settings to allow the miner to communicate.
4. Read the documentation for your miner to double check the syntax as it could have been updated.


## Why does it keep failing to set the overclock and asking me to run as admin?

In order for the miner to set the GPU settings it must be run as admin and in order for this app to run the miner as admin this app must also be run as admin. Otherwise the app will continue to run without admin privliges therefore so will the miner and that is commonly why it is failing.


## Why are the GPUs not getting added when I click "Add GPU Settings"?

Ensure the "Enabled" check-box is checked for each GPU.
