namespace Gui_Miner
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.restartsLabel = new System.Windows.Forms.Label();
            this.killAllMinersButtonPictureBox = new System.Windows.Forms.PictureBox();
            this.settingsButtonPictureBox = new System.Windows.Forms.PictureBox();
            this.startButtonPictureBox = new System.Windows.Forms.PictureBox();
            this.outputPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.killAllMinersButtonPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.settingsButtonPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.startButtonPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // restartsLabel
            // 
            this.restartsLabel.AutoSize = true;
            this.restartsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.restartsLabel.ForeColor = System.Drawing.SystemColors.Control;
            this.restartsLabel.Location = new System.Drawing.Point(159, 21);
            this.restartsLabel.Name = "restartsLabel";
            this.restartsLabel.Size = new System.Drawing.Size(73, 17);
            this.restartsLabel.TabIndex = 8;
            this.restartsLabel.Text = "Restarts 0";
            // 
            // killAllMinersButtonPictureBox
            // 
            this.killAllMinersButtonPictureBox.BackgroundImage = global::Gui_Miner.Properties.Resources.panic_attack;
            this.killAllMinersButtonPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.killAllMinersButtonPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.killAllMinersButtonPictureBox.Location = new System.Drawing.Point(116, 12);
            this.killAllMinersButtonPictureBox.Name = "killAllMinersButtonPictureBox";
            this.killAllMinersButtonPictureBox.Size = new System.Drawing.Size(37, 34);
            this.killAllMinersButtonPictureBox.TabIndex = 9;
            this.killAllMinersButtonPictureBox.TabStop = false;
            this.killAllMinersButtonPictureBox.Click += new System.EventHandler(this.killAllMinersButtonPictureBox_Click);
            // 
            // settingsButtonPictureBox
            // 
            this.settingsButtonPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsButtonPictureBox.BackgroundImage = global::Gui_Miner.Properties.Resources.settings;
            this.settingsButtonPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.settingsButtonPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.settingsButtonPictureBox.Location = new System.Drawing.Point(351, 3);
            this.settingsButtonPictureBox.Name = "settingsButtonPictureBox";
            this.settingsButtonPictureBox.Size = new System.Drawing.Size(64, 53);
            this.settingsButtonPictureBox.TabIndex = 7;
            this.settingsButtonPictureBox.TabStop = false;
            this.settingsButtonPictureBox.Click += new System.EventHandler(this.settingsButtonPictureBox_Click);
            // 
            // startButtonPictureBox
            // 
            this.startButtonPictureBox.BackgroundImage = global::Gui_Miner.Properties.Resources.play_button;
            this.startButtonPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.startButtonPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.startButtonPictureBox.Location = new System.Drawing.Point(2, 2);
            this.startButtonPictureBox.Name = "startButtonPictureBox";
            this.startButtonPictureBox.Size = new System.Drawing.Size(64, 53);
            this.startButtonPictureBox.TabIndex = 6;
            this.startButtonPictureBox.TabStop = false;
            this.startButtonPictureBox.Click += new System.EventHandler(this.startButtonPictureBox_Click);
            // 
            // outputPanel
            // 
            this.outputPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("outputPanel.BackgroundImage")));
            this.outputPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.outputPanel.Location = new System.Drawing.Point(2, 56);
            this.outputPanel.Name = "outputPanel";
            this.outputPanel.Size = new System.Drawing.Size(413, 415);
            this.outputPanel.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(20)))), ((int)(((byte)(52)))));
            this.ClientSize = new System.Drawing.Size(415, 471);
            this.Controls.Add(this.killAllMinersButtonPictureBox);
            this.Controls.Add(this.restartsLabel);
            this.Controls.Add(this.settingsButtonPictureBox);
            this.Controls.Add(this.startButtonPictureBox);
            this.Controls.Add(this.outputPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(431, 510);
            this.Name = "Form1";
            this.Text = "Gui Miner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.killAllMinersButtonPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.settingsButtonPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.startButtonPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel outputPanel;
        private System.Windows.Forms.PictureBox startButtonPictureBox;
        private System.Windows.Forms.PictureBox settingsButtonPictureBox;
        private System.Windows.Forms.Label restartsLabel;
        private System.Windows.Forms.PictureBox killAllMinersButtonPictureBox;
    }
}

