namespace Gui_Miner
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.minerSettingsPanel = new System.Windows.Forms.Panel();
            this.gpuSettingsPanel = new System.Windows.Forms.Panel();
            this.gpuListBox = new System.Windows.Forms.ListBox();
            this.minerSettingsListBox = new System.Windows.Forms.ListBox();
            this.batLineTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.generateButton = new System.Windows.Forms.Button();
            this.getAllGpusButton = new System.Windows.Forms.Button();
            this.addGpuSettingsButton = new System.Windows.Forms.Button();
            this.clearGpuSettingsButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.manageConfigPanel = new System.Windows.Forms.Panel();
            this.topPanel = new System.Windows.Forms.Panel();
            this.managePoolsButton = new System.Windows.Forms.Button();
            this.manageWalletsButton = new System.Windows.Forms.Button();
            this.manageMinerConfigsButton = new System.Windows.Forms.Button();
            this.generalButton = new System.Windows.Forms.Button();
            this.generalPanel = new System.Windows.Forms.Panel();
            this.successLabel = new System.Windows.Forms.Label();
            this.autoStartWithWinCheckBox = new System.Windows.Forms.CheckBox();
            this.autoStartMiningCheckBox = new System.Windows.Forms.CheckBox();
            this.walletsPanel = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.walletPanel = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.walletNameTextBox = new System.Windows.Forms.TextBox();
            this.walletAddressTextBox = new System.Windows.Forms.TextBox();
            this.walletsListBox = new System.Windows.Forms.ListBox();
            this.poolsPanel = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.walletCoinTextBox = new System.Windows.Forms.TextBox();
            this.manageConfigPanel.SuspendLayout();
            this.topPanel.SuspendLayout();
            this.generalPanel.SuspendLayout();
            this.walletsPanel.SuspendLayout();
            this.walletPanel.SuspendLayout();
            this.poolsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // minerSettingsPanel
            // 
            this.minerSettingsPanel.Location = new System.Drawing.Point(27, 112);
            this.minerSettingsPanel.Name = "minerSettingsPanel";
            this.minerSettingsPanel.Size = new System.Drawing.Size(299, 316);
            this.minerSettingsPanel.TabIndex = 0;
            // 
            // gpuSettingsPanel
            // 
            this.gpuSettingsPanel.Location = new System.Drawing.Point(395, 113);
            this.gpuSettingsPanel.Name = "gpuSettingsPanel";
            this.gpuSettingsPanel.Size = new System.Drawing.Size(299, 315);
            this.gpuSettingsPanel.TabIndex = 1;
            // 
            // gpuListBox
            // 
            this.gpuListBox.FormattingEnabled = true;
            this.gpuListBox.Items.AddRange(new object[] {
            "Add GPU"});
            this.gpuListBox.Location = new System.Drawing.Point(395, 21);
            this.gpuListBox.Name = "gpuListBox";
            this.gpuListBox.Size = new System.Drawing.Size(299, 69);
            this.gpuListBox.TabIndex = 3;
            this.gpuListBox.SelectedIndexChanged += new System.EventHandler(this.gpuListBox_SelectedIndexChanged);
            this.gpuListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gpuListBox_KeyDown);
            // 
            // minerSettingsListBox
            // 
            this.minerSettingsListBox.FormattingEnabled = true;
            this.minerSettingsListBox.Items.AddRange(new object[] {
            "Add Miner Settings"});
            this.minerSettingsListBox.Location = new System.Drawing.Point(27, 21);
            this.minerSettingsListBox.Name = "minerSettingsListBox";
            this.minerSettingsListBox.Size = new System.Drawing.Size(299, 69);
            this.minerSettingsListBox.TabIndex = 4;
            this.minerSettingsListBox.SelectedIndexChanged += new System.EventHandler(this.minerSettingsListBox_SelectedIndexChanged);
            this.minerSettingsListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.minerSettingsListBox_KeyDown);
            // 
            // batLineTextBox
            // 
            this.batLineTextBox.Location = new System.Drawing.Point(10, 500);
            this.batLineTextBox.Multiline = true;
            this.batLineTextBox.Name = "batLineTextBox";
            this.batLineTextBox.Size = new System.Drawing.Size(702, 96);
            this.batLineTextBox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(94, 481);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = ".bat File:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(111, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Miner Settings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(534, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "GPUs";
            // 
            // generateButton
            // 
            this.generateButton.Location = new System.Drawing.Point(10, 477);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(67, 21);
            this.generateButton.TabIndex = 9;
            this.generateButton.Text = "Generate";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // getAllGpusButton
            // 
            this.getAllGpusButton.Location = new System.Drawing.Point(615, 433);
            this.getAllGpusButton.Name = "getAllGpusButton";
            this.getAllGpusButton.Size = new System.Drawing.Size(79, 21);
            this.getAllGpusButton.TabIndex = 10;
            this.getAllGpusButton.Text = "Get All GPUs";
            this.getAllGpusButton.UseVisualStyleBackColor = true;
            this.getAllGpusButton.Click += new System.EventHandler(this.getAllGpusButton_Click);
            // 
            // addGpuSettingsButton
            // 
            this.addGpuSettingsButton.Location = new System.Drawing.Point(224, 433);
            this.addGpuSettingsButton.Name = "addGpuSettingsButton";
            this.addGpuSettingsButton.Size = new System.Drawing.Size(102, 21);
            this.addGpuSettingsButton.TabIndex = 11;
            this.addGpuSettingsButton.Text = "Add GPU Settings";
            this.addGpuSettingsButton.UseVisualStyleBackColor = true;
            this.addGpuSettingsButton.Click += new System.EventHandler(this.addGpuSettingsButton_Click);
            // 
            // clearGpuSettingsButton
            // 
            this.clearGpuSettingsButton.Location = new System.Drawing.Point(27, 433);
            this.clearGpuSettingsButton.Name = "clearGpuSettingsButton";
            this.clearGpuSettingsButton.Size = new System.Drawing.Size(106, 21);
            this.clearGpuSettingsButton.TabIndex = 12;
            this.clearGpuSettingsButton.Text = "Clear GPU Settings";
            this.clearGpuSettingsButton.UseVisualStyleBackColor = true;
            this.clearGpuSettingsButton.Click += new System.EventHandler(this.clearGpuSettingsButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(296, 472);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 13);
            this.statusLabel.TabIndex = 13;
            // 
            // manageConfigPanel
            // 
            this.manageConfigPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.manageConfigPanel.Controls.Add(this.statusLabel);
            this.manageConfigPanel.Controls.Add(this.clearGpuSettingsButton);
            this.manageConfigPanel.Controls.Add(this.addGpuSettingsButton);
            this.manageConfigPanel.Controls.Add(this.getAllGpusButton);
            this.manageConfigPanel.Controls.Add(this.generateButton);
            this.manageConfigPanel.Controls.Add(this.label3);
            this.manageConfigPanel.Controls.Add(this.label2);
            this.manageConfigPanel.Controls.Add(this.label1);
            this.manageConfigPanel.Controls.Add(this.batLineTextBox);
            this.manageConfigPanel.Controls.Add(this.minerSettingsListBox);
            this.manageConfigPanel.Controls.Add(this.gpuListBox);
            this.manageConfigPanel.Controls.Add(this.gpuSettingsPanel);
            this.manageConfigPanel.Controls.Add(this.minerSettingsPanel);
            this.manageConfigPanel.Location = new System.Drawing.Point(534, 60);
            this.manageConfigPanel.Name = "manageConfigPanel";
            this.manageConfigPanel.Size = new System.Drawing.Size(722, 604);
            this.manageConfigPanel.TabIndex = 14;
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.managePoolsButton);
            this.topPanel.Controls.Add(this.manageWalletsButton);
            this.topPanel.Controls.Add(this.manageMinerConfigsButton);
            this.topPanel.Controls.Add(this.generalButton);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(722, 54);
            this.topPanel.TabIndex = 15;
            // 
            // managePoolsButton
            // 
            this.managePoolsButton.Location = new System.Drawing.Point(618, 13);
            this.managePoolsButton.Name = "managePoolsButton";
            this.managePoolsButton.Size = new System.Drawing.Size(78, 26);
            this.managePoolsButton.TabIndex = 3;
            this.managePoolsButton.Text = "Pools";
            this.managePoolsButton.UseVisualStyleBackColor = true;
            this.managePoolsButton.Click += new System.EventHandler(this.managePoolsButton_Click);
            // 
            // manageWalletsButton
            // 
            this.manageWalletsButton.Location = new System.Drawing.Point(426, 12);
            this.manageWalletsButton.Name = "manageWalletsButton";
            this.manageWalletsButton.Size = new System.Drawing.Size(78, 26);
            this.manageWalletsButton.TabIndex = 2;
            this.manageWalletsButton.Text = "Wallets";
            this.manageWalletsButton.UseVisualStyleBackColor = true;
            this.manageWalletsButton.Click += new System.EventHandler(this.manageWalletsButton_Click);
            // 
            // manageMinerConfigsButton
            // 
            this.manageMinerConfigsButton.Location = new System.Drawing.Point(213, 12);
            this.manageMinerConfigsButton.Name = "manageMinerConfigsButton";
            this.manageMinerConfigsButton.Size = new System.Drawing.Size(91, 26);
            this.manageMinerConfigsButton.TabIndex = 1;
            this.manageMinerConfigsButton.Text = "Miner Configs";
            this.manageMinerConfigsButton.UseVisualStyleBackColor = true;
            this.manageMinerConfigsButton.Click += new System.EventHandler(this.manageMinerConfigsButton_Click);
            // 
            // generalButton
            // 
            this.generalButton.Location = new System.Drawing.Point(28, 13);
            this.generalButton.Name = "generalButton";
            this.generalButton.Size = new System.Drawing.Size(76, 26);
            this.generalButton.TabIndex = 0;
            this.generalButton.Text = "General";
            this.generalButton.UseVisualStyleBackColor = true;
            this.generalButton.Click += new System.EventHandler(this.generalButton_Click);
            // 
            // generalPanel
            // 
            this.generalPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.generalPanel.Controls.Add(this.successLabel);
            this.generalPanel.Controls.Add(this.autoStartWithWinCheckBox);
            this.generalPanel.Controls.Add(this.autoStartMiningCheckBox);
            this.generalPanel.Location = new System.Drawing.Point(1, 57);
            this.generalPanel.Name = "generalPanel";
            this.generalPanel.Size = new System.Drawing.Size(722, 93);
            this.generalPanel.TabIndex = 16;
            // 
            // successLabel
            // 
            this.successLabel.AutoSize = true;
            this.successLabel.Location = new System.Drawing.Point(343, 26);
            this.successLabel.Name = "successLabel";
            this.successLabel.Size = new System.Drawing.Size(0, 13);
            this.successLabel.TabIndex = 2;
            // 
            // autoStartWithWinCheckBox
            // 
            this.autoStartWithWinCheckBox.AutoSize = true;
            this.autoStartWithWinCheckBox.Location = new System.Drawing.Point(30, 21);
            this.autoStartWithWinCheckBox.Name = "autoStartWithWinCheckBox";
            this.autoStartWithWinCheckBox.Size = new System.Drawing.Size(170, 17);
            this.autoStartWithWinCheckBox.TabIndex = 1;
            this.autoStartWithWinCheckBox.Text = "Auto Start App with Windows?";
            this.autoStartWithWinCheckBox.UseVisualStyleBackColor = true;
            this.autoStartWithWinCheckBox.CheckedChanged += new System.EventHandler(this.autoStartWithWinCheckBox_CheckedChanged);
            // 
            // autoStartMiningCheckBox
            // 
            this.autoStartMiningCheckBox.AutoSize = true;
            this.autoStartMiningCheckBox.Location = new System.Drawing.Point(30, 55);
            this.autoStartMiningCheckBox.Name = "autoStartMiningCheckBox";
            this.autoStartMiningCheckBox.Size = new System.Drawing.Size(113, 17);
            this.autoStartMiningCheckBox.TabIndex = 0;
            this.autoStartMiningCheckBox.Text = "Auto Start Mining?";
            this.autoStartMiningCheckBox.UseVisualStyleBackColor = true;
            this.autoStartMiningCheckBox.CheckedChanged += new System.EventHandler(this.autoStartMiningCheckBox_CheckedChanged);
            // 
            // walletsPanel
            // 
            this.walletsPanel.Controls.Add(this.label6);
            this.walletsPanel.Controls.Add(this.walletPanel);
            this.walletsPanel.Controls.Add(this.walletsListBox);
            this.walletsPanel.Location = new System.Drawing.Point(30, 156);
            this.walletsPanel.Name = "walletsPanel";
            this.walletsPanel.Size = new System.Drawing.Size(722, 604);
            this.walletsPanel.TabIndex = 17;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 18.25F);
            this.label6.Location = new System.Drawing.Point(249, 12);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(195, 29);
            this.label6.TabIndex = 2;
            this.label6.Text = "Manage Wallets";
            // 
            // walletPanel
            // 
            this.walletPanel.Controls.Add(this.walletCoinTextBox);
            this.walletPanel.Controls.Add(this.label8);
            this.walletPanel.Controls.Add(this.label5);
            this.walletPanel.Controls.Add(this.label4);
            this.walletPanel.Controls.Add(this.walletNameTextBox);
            this.walletPanel.Controls.Add(this.walletAddressTextBox);
            this.walletPanel.Location = new System.Drawing.Point(192, 167);
            this.walletPanel.Name = "walletPanel";
            this.walletPanel.Size = new System.Drawing.Size(301, 182);
            this.walletPanel.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(108, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Wallet Address";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(113, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Wallet Name";
            // 
            // walletNameTextBox
            // 
            this.walletNameTextBox.Location = new System.Drawing.Point(62, 33);
            this.walletNameTextBox.Name = "walletNameTextBox";
            this.walletNameTextBox.Size = new System.Drawing.Size(169, 20);
            this.walletNameTextBox.TabIndex = 1;
            this.walletNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.walletNameTextBox_KeyDown);
            // 
            // walletAddressTextBox
            // 
            this.walletAddressTextBox.Location = new System.Drawing.Point(0, 89);
            this.walletAddressTextBox.Name = "walletAddressTextBox";
            this.walletAddressTextBox.Size = new System.Drawing.Size(301, 20);
            this.walletAddressTextBox.TabIndex = 0;
            this.walletAddressTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.walletAddressTextBox_KeyDown);
            // 
            // walletsListBox
            // 
            this.walletsListBox.FormattingEnabled = true;
            this.walletsListBox.Items.AddRange(new object[] {
            "Add Wallet"});
            this.walletsListBox.Location = new System.Drawing.Point(192, 56);
            this.walletsListBox.Name = "walletsListBox";
            this.walletsListBox.Size = new System.Drawing.Size(301, 95);
            this.walletsListBox.TabIndex = 0;
            this.walletsListBox.SelectedIndexChanged += new System.EventHandler(this.walletsListBox_SelectedIndexChanged);
            this.walletsListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.walletsListBox_KeyDown);
            // 
            // poolsPanel
            // 
            this.poolsPanel.Controls.Add(this.label7);
            this.poolsPanel.Location = new System.Drawing.Point(12, 477);
            this.poolsPanel.Name = "poolsPanel";
            this.poolsPanel.Size = new System.Drawing.Size(722, 604);
            this.poolsPanel.TabIndex = 18;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 18.25F);
            this.label7.Location = new System.Drawing.Point(248, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(175, 29);
            this.label7.TabIndex = 3;
            this.label7.Text = "Manage Pools";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(132, 129);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(28, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Coin";
            // 
            // walletCoinTextBox
            // 
            this.walletCoinTextBox.Location = new System.Drawing.Point(62, 145);
            this.walletCoinTextBox.Name = "walletCoinTextBox";
            this.walletCoinTextBox.Size = new System.Drawing.Size(169, 20);
            this.walletCoinTextBox.TabIndex = 5;
            this.walletCoinTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.walletCoinTextBox_KeyDown);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(722, 661);
            this.Controls.Add(this.poolsPanel);
            this.Controls.Add(this.walletsPanel);
            this.Controls.Add(this.manageConfigPanel);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.generalPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "SettingsForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.manageConfigPanel.ResumeLayout(false);
            this.manageConfigPanel.PerformLayout();
            this.topPanel.ResumeLayout(false);
            this.generalPanel.ResumeLayout(false);
            this.generalPanel.PerformLayout();
            this.walletsPanel.ResumeLayout(false);
            this.walletsPanel.PerformLayout();
            this.walletPanel.ResumeLayout(false);
            this.walletPanel.PerformLayout();
            this.poolsPanel.ResumeLayout(false);
            this.poolsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel minerSettingsPanel;
        private System.Windows.Forms.Panel gpuSettingsPanel;
        private System.Windows.Forms.ListBox gpuListBox;
        private System.Windows.Forms.ListBox minerSettingsListBox;
        private System.Windows.Forms.TextBox batLineTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.Button getAllGpusButton;
        private System.Windows.Forms.Button addGpuSettingsButton;
        private System.Windows.Forms.Button clearGpuSettingsButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Panel manageConfigPanel;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Button manageMinerConfigsButton;
        private System.Windows.Forms.Button generalButton;
        private System.Windows.Forms.Panel generalPanel;
        private System.Windows.Forms.CheckBox autoStartWithWinCheckBox;
        private System.Windows.Forms.CheckBox autoStartMiningCheckBox;
        private System.Windows.Forms.Label successLabel;
        private System.Windows.Forms.Panel walletsPanel;
        private System.Windows.Forms.Panel walletPanel;
        private System.Windows.Forms.TextBox walletAddressTextBox;
        private System.Windows.Forms.ListBox walletsListBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox walletNameTextBox;
        private System.Windows.Forms.Button managePoolsButton;
        private System.Windows.Forms.Button manageWalletsButton;
        private System.Windows.Forms.Panel poolsPanel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox walletCoinTextBox;
    }
}