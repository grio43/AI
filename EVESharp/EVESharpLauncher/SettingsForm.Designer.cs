namespace EVESharpLauncher
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.textBoxEveLocation = new System.Windows.Forms.TextBox();
            this.exeFileLocationLabel = new System.Windows.Forms.Label();
            this.labelGmailUser = new System.Windows.Forms.Label();
            this.textBoxGmailUser = new System.Windows.Forms.TextBox();
            this.labelGmailPassword = new System.Windows.Forms.Label();
            this.textBoxGmailPassword = new System.Windows.Forms.TextBox();
            this.labelReceiverEmailAddress = new System.Windows.Forms.Label();
            this.textBoxReceiverEmailAddress = new System.Windows.Forms.TextBox();
            this.buttonSendTestEmail = new System.Windows.Forms.Button();
            this.labelSharpLogLite = new System.Windows.Forms.Label();
            this.checkBoxSharpLogLite = new System.Windows.Forms.CheckBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.label11 = new System.Windows.Forms.Label();
            this.trackBar3 = new System.Windows.Forms.TrackBar();
            this.labelGitHubURLBranchZip = new System.Windows.Forms.Label();
            this.textBoxGitHubUrlBranchZip = new System.Windows.Forms.TextBox();
            this.checkBoxAllowEveClientAutoUpdate = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxEveLocation
            // 
            this.textBoxEveLocation.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxEveLocation.Location = new System.Drawing.Point(14, 28);
            this.textBoxEveLocation.Name = "textBoxEveLocation";
            this.textBoxEveLocation.Size = new System.Drawing.Size(269, 21);
            this.textBoxEveLocation.TabIndex = 33;
            this.textBoxEveLocation.TextChanged += new System.EventHandler(this.TextBoxEveLocation_TextChanged);
            // 
            // exeFileLocationLabel
            // 
            this.exeFileLocationLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exeFileLocationLabel.Location = new System.Drawing.Point(11, 9);
            this.exeFileLocationLabel.Name = "exeFileLocationLabel";
            this.exeFileLocationLabel.Size = new System.Drawing.Size(152, 16);
            this.exeFileLocationLabel.TabIndex = 32;
            this.exeFileLocationLabel.Text = "ExeFile.exe location:";
            // 
            // labelGmailUser
            // 
            this.labelGmailUser.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGmailUser.Location = new System.Drawing.Point(12, 93);
            this.labelGmailUser.Name = "labelGmailUser";
            this.labelGmailUser.Size = new System.Drawing.Size(152, 17);
            this.labelGmailUser.TabIndex = 37;
            this.labelGmailUser.Text = "Gmail user:";
            // 
            // textBoxGmailUser
            // 
            this.textBoxGmailUser.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxGmailUser.Location = new System.Drawing.Point(14, 112);
            this.textBoxGmailUser.Name = "textBoxGmailUser";
            this.textBoxGmailUser.Size = new System.Drawing.Size(269, 21);
            this.textBoxGmailUser.TabIndex = 36;
            this.textBoxGmailUser.TextChanged += new System.EventHandler(this.TextBoxGmailUser_TextChanged);
            // 
            // labelGmailPassword
            // 
            this.labelGmailPassword.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGmailPassword.Location = new System.Drawing.Point(12, 135);
            this.labelGmailPassword.Name = "labelGmailPassword";
            this.labelGmailPassword.Size = new System.Drawing.Size(152, 17);
            this.labelGmailPassword.TabIndex = 39;
            this.labelGmailPassword.Text = "Gmail password:";
            // 
            // textBoxGmailPassword
            // 
            this.textBoxGmailPassword.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxGmailPassword.Location = new System.Drawing.Point(14, 154);
            this.textBoxGmailPassword.Name = "textBoxGmailPassword";
            this.textBoxGmailPassword.Size = new System.Drawing.Size(269, 21);
            this.textBoxGmailPassword.TabIndex = 38;
            this.textBoxGmailPassword.TextChanged += new System.EventHandler(this.TextBoxGmailPassword_TextChanged);
            // 
            // labelReceiverEmailAddress
            // 
            this.labelReceiverEmailAddress.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelReceiverEmailAddress.Location = new System.Drawing.Point(12, 177);
            this.labelReceiverEmailAddress.Name = "labelReceiverEmailAddress";
            this.labelReceiverEmailAddress.Size = new System.Drawing.Size(152, 17);
            this.labelReceiverEmailAddress.TabIndex = 41;
            this.labelReceiverEmailAddress.Text = "Receiver email address:";
            // 
            // textBoxReceiverEmailAddress
            // 
            this.textBoxReceiverEmailAddress.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxReceiverEmailAddress.Location = new System.Drawing.Point(14, 196);
            this.textBoxReceiverEmailAddress.Name = "textBoxReceiverEmailAddress";
            this.textBoxReceiverEmailAddress.Size = new System.Drawing.Size(269, 21);
            this.textBoxReceiverEmailAddress.TabIndex = 40;
            this.textBoxReceiverEmailAddress.TextChanged += new System.EventHandler(this.TextBoxReceiverEmailAddress_TextChanged);
            // 
            // buttonSendTestEmail
            // 
            this.buttonSendTestEmail.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSendTestEmail.Location = new System.Drawing.Point(14, 222);
            this.buttonSendTestEmail.Name = "buttonSendTestEmail";
            this.buttonSendTestEmail.Size = new System.Drawing.Size(270, 23);
            this.buttonSendTestEmail.TabIndex = 42;
            this.buttonSendTestEmail.Text = "Send test email";
            this.buttonSendTestEmail.UseVisualStyleBackColor = true;
            this.buttonSendTestEmail.Click += new System.EventHandler(this.ButtonSendTestEmail_Click);
            // 
            // labelSharpLogLite
            // 
            this.labelSharpLogLite.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSharpLogLite.Location = new System.Drawing.Point(289, 91);
            this.labelSharpLogLite.Name = "labelSharpLogLite";
            this.labelSharpLogLite.Size = new System.Drawing.Size(280, 16);
            this.labelSharpLogLite.TabIndex = 48;
            this.labelSharpLogLite.Text = "SharpLogLite / Severity:";
            // 
            // checkBoxSharpLogLite
            // 
            this.checkBoxSharpLogLite.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxSharpLogLite.Location = new System.Drawing.Point(292, 110);
            this.checkBoxSharpLogLite.Name = "checkBoxSharpLogLite";
            this.checkBoxSharpLogLite.Size = new System.Drawing.Size(104, 21);
            this.checkBoxSharpLogLite.TabIndex = 47;
            this.checkBoxSharpLogLite.Text = "Enabled";
            this.checkBoxSharpLogLite.UseVisualStyleBackColor = true;
            this.checkBoxSharpLogLite.CheckedChanged += new System.EventHandler(this.CheckBoxSharpLogLite_CheckedChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(292, 108);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(249, 21);
            this.comboBox1.TabIndex = 49;
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(292, 149);
            this.trackBar1.Maximum = 30;
            this.trackBar1.Minimum = 10;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(249, 45);
            this.trackBar1.TabIndex = 50;
            this.trackBar1.Value = 10;
            this.trackBar1.Scroll += new System.EventHandler(this.TrackBar1_Scroll);
            // 
            // label10
            // 
            this.label10.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(292, 136);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(280, 16);
            this.label10.TabIndex = 52;
            this.label10.Text = "Background FPS:";
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(292, 187);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(280, 16);
            this.label9.TabIndex = 54;
            this.label9.Text = "Min seconds between EVE launches:";
            // 
            // trackBar2
            // 
            this.trackBar2.LargeChange = 40;
            this.trackBar2.Location = new System.Drawing.Point(292, 200);
            this.trackBar2.Maximum = 240;
            this.trackBar2.Minimum = 20;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(249, 45);
            this.trackBar2.SmallChange = 15;
            this.trackBar2.TabIndex = 53;
            this.trackBar2.TickFrequency = 10;
            this.trackBar2.Value = 20;
            this.trackBar2.Scroll += new System.EventHandler(this.TrackBar2_Scroll);
            // 
            // label11
            // 
            this.label11.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(292, 238);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(280, 16);
            this.label11.TabIndex = 56;
            this.label11.Text = "Max seconds between EVE launches:";
            // 
            // trackBar3
            // 
            this.trackBar3.LargeChange = 20;
            this.trackBar3.Location = new System.Drawing.Point(292, 251);
            this.trackBar3.Maximum = 300;
            this.trackBar3.Minimum = 40;
            this.trackBar3.Name = "trackBar3";
            this.trackBar3.Size = new System.Drawing.Size(249, 45);
            this.trackBar3.SmallChange = 20;
            this.trackBar3.TabIndex = 55;
            this.trackBar3.TickFrequency = 10;
            this.trackBar3.Value = 40;
            this.trackBar3.Scroll += new System.EventHandler(this.TrackBar3_Scroll);
            // 
            // labelGitHubURLBranchZip
            // 
            this.labelGitHubURLBranchZip.Location = new System.Drawing.Point(0, 0);
            this.labelGitHubURLBranchZip.Name = "labelGitHubURLBranchZip";
            this.labelGitHubURLBranchZip.Size = new System.Drawing.Size(100, 23);
            this.labelGitHubURLBranchZip.TabIndex = 0;
            // 
            // textBoxGitHubUrlBranchZip
            // 
            this.textBoxGitHubUrlBranchZip.Location = new System.Drawing.Point(0, 0);
            this.textBoxGitHubUrlBranchZip.Name = "textBoxGitHubUrlBranchZip";
            this.textBoxGitHubUrlBranchZip.Size = new System.Drawing.Size(100, 20);
            this.textBoxGitHubUrlBranchZip.TabIndex = 0;
            // 
            // checkBoxAllowEveClientAutoUpdate
            // 
            this.checkBoxAllowEveClientAutoUpdate.AutoSize = true;
            this.checkBoxAllowEveClientAutoUpdate.Location = new System.Drawing.Point(14, 64);
            this.checkBoxAllowEveClientAutoUpdate.Name = "checkBoxAllowEveClientAutoUpdate";
            this.checkBoxAllowEveClientAutoUpdate.Size = new System.Drawing.Size(162, 17);
            this.checkBoxAllowEveClientAutoUpdate.TabIndex = 58;
            this.checkBoxAllowEveClientAutoUpdate.Text = "Allow Eve Client AutoUpdate";
            this.checkBoxAllowEveClientAutoUpdate.UseVisualStyleBackColor = true;
            this.checkBoxAllowEveClientAutoUpdate.CheckedChanged += new System.EventHandler(this.checkBoxAllowEveClientAutoUpdate_CheckedChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 295);
            this.Controls.Add(this.checkBoxAllowEveClientAutoUpdate);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.trackBar3);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.trackBar2);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.labelSharpLogLite);
            this.Controls.Add(this.checkBoxSharpLogLite);
            this.Controls.Add(this.buttonSendTestEmail);
            this.Controls.Add(this.labelReceiverEmailAddress);
            this.Controls.Add(this.textBoxReceiverEmailAddress);
            this.Controls.Add(this.labelGmailPassword);
            this.Controls.Add(this.textBoxGmailPassword);
            this.Controls.Add(this.labelGmailUser);
            this.Controls.Add(this.textBoxGmailUser);
            this.Controls.Add(this.textBoxEveLocation);
            this.Controls.Add(this.exeFileLocationLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxEveLocation;
        private System.Windows.Forms.Label exeFileLocationLabel;
        private System.Windows.Forms.Label labelGmailUser;
        private System.Windows.Forms.TextBox textBoxGmailUser;
        private System.Windows.Forms.Label labelGmailPassword;
        private System.Windows.Forms.TextBox textBoxGmailPassword;
        private System.Windows.Forms.Label labelReceiverEmailAddress;
        private System.Windows.Forms.TextBox textBoxReceiverEmailAddress;
        private System.Windows.Forms.Button buttonSendTestEmail;
        private System.Windows.Forms.Label labelSharpLogLite;
        private System.Windows.Forms.CheckBox checkBoxSharpLogLite;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label labelGitHubURLBranchZip;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.TextBox textBoxGitHubUrlBranchZip;
        //private System.Windows.Forms.Label labelEVELauncherLocation;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TrackBar trackBar3;
        private System.Windows.Forms.CheckBox checkBoxAllowEveClientAutoUpdate;
    }
}