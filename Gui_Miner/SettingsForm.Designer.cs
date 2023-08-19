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
            this.minerSettingsPanel = new System.Windows.Forms.Panel();
            this.gpuSettingsPanel = new System.Windows.Forms.Panel();
            this.gpuListBox = new System.Windows.Forms.ListBox();
            this.minerSettingsListBox = new System.Windows.Forms.ListBox();
            this.batLineTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // minerSettingsPanel
            // 
            this.minerSettingsPanel.Location = new System.Drawing.Point(12, 117);
            this.minerSettingsPanel.Name = "minerSettingsPanel";
            this.minerSettingsPanel.Size = new System.Drawing.Size(340, 316);
            this.minerSettingsPanel.TabIndex = 0;
            // 
            // gpuSettingsPanel
            // 
            this.gpuSettingsPanel.Location = new System.Drawing.Point(369, 117);
            this.gpuSettingsPanel.Name = "gpuSettingsPanel";
            this.gpuSettingsPanel.Size = new System.Drawing.Size(346, 315);
            this.gpuSettingsPanel.TabIndex = 1;
            // 
            // gpuListBox
            // 
            this.gpuListBox.FormattingEnabled = true;
            this.gpuListBox.Items.AddRange(new object[] {
            "Add GPU"});
            this.gpuListBox.Location = new System.Drawing.Point(369, 23);
            this.gpuListBox.Name = "gpuListBox";
            this.gpuListBox.Size = new System.Drawing.Size(345, 69);
            this.gpuListBox.TabIndex = 3;
            this.gpuListBox.SelectedIndexChanged += new System.EventHandler(this.gpuListBox_SelectedIndexChanged);
            this.gpuListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gpuListBox_KeyDown);
            // 
            // minerSettingsListBox
            // 
            this.minerSettingsListBox.FormattingEnabled = true;
            this.minerSettingsListBox.Items.AddRange(new object[] {
            "Add Miner Settings"});
            this.minerSettingsListBox.Location = new System.Drawing.Point(12, 23);
            this.minerSettingsListBox.Name = "minerSettingsListBox";
            this.minerSettingsListBox.Size = new System.Drawing.Size(340, 69);
            this.minerSettingsListBox.TabIndex = 4;
            this.minerSettingsListBox.SelectedIndexChanged += new System.EventHandler(this.minerSettingsListBox_SelectedIndexChanged);
            this.minerSettingsListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.minerSettingsListBox_KeyDown);
            // 
            // batLineTextBox
            // 
            this.batLineTextBox.Location = new System.Drawing.Point(12, 476);
            this.batLineTextBox.Multiline = true;
            this.batLineTextBox.Name = "batLineTextBox";
            this.batLineTextBox.Size = new System.Drawing.Size(702, 113);
            this.batLineTextBox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(86, 460);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = ".bat File:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(96, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Miner Settings";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(508, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "GPUs";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(726, 601);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.batLineTextBox);
            this.Controls.Add(this.minerSettingsListBox);
            this.Controls.Add(this.gpuListBox);
            this.Controls.Add(this.gpuSettingsPanel);
            this.Controls.Add(this.minerSettingsPanel);
            this.Name = "SettingsForm";
            this.Text = "SettingsForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}