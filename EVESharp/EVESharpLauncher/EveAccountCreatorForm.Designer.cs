namespace EVESharpLauncher
{
    partial class EveAccountCreatorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EveAccountCreatorForm));
            this.comboBoxProxies = new System.Windows.Forms.ComboBox();
            this.buttonOpenBrowser = new System.Windows.Forms.Button();
            this.buttonStartAlphaCreation = new System.Windows.Forms.Button();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonAbortAlphaCreation = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.button4 = new System.Windows.Forms.Button();
            this.buttonAbortEmailValidation = new System.Windows.Forms.Button();
            this.buttonValidateEveAccount = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxEvePassword = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxEveAccountName = new System.Windows.Forms.TextBox();
            this.buttonCreateEveAccount = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.buttonCreateEmailAccount = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.radioButtonOutlook = new System.Windows.Forms.RadioButton();
            this.radioButtonYandex = new System.Windows.Forms.RadioButton();
            this.radioButtonGmail = new System.Windows.Forms.RadioButton();
            this.textBoxEmailPassword = new System.Windows.Forms.TextBox();
            this.textBoxIMAPHost = new System.Windows.Forms.TextBox();
            this.textBoxEmailAddress = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonGenerateRandom = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxProxies
            // 
            this.comboBoxProxies.Dock = System.Windows.Forms.DockStyle.Top;
            this.comboBoxProxies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxProxies.FormattingEnabled = true;
            this.comboBoxProxies.Location = new System.Drawing.Point(3, 17);
            this.comboBoxProxies.Name = "comboBoxProxies";
            this.comboBoxProxies.Size = new System.Drawing.Size(438, 21);
            this.comboBoxProxies.TabIndex = 0;
            this.comboBoxProxies.SelectedIndexChanged += new System.EventHandler(this.ComboBoxProxies_SelectedIndexChanged);
            // 
            // buttonOpenBrowser
            // 
            this.buttonOpenBrowser.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonOpenBrowser.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOpenBrowser.Location = new System.Drawing.Point(3, 50);
            this.buttonOpenBrowser.Name = "buttonOpenBrowser";
            this.buttonOpenBrowser.Size = new System.Drawing.Size(438, 25);
            this.buttonOpenBrowser.TabIndex = 1;
            this.buttonOpenBrowser.Text = "Open Browser: All traffic will use Proxy";
            this.buttonOpenBrowser.UseVisualStyleBackColor = true;
            this.buttonOpenBrowser.Click += new System.EventHandler(this.ButtonOpenBrowser_Click);
            // 
            // buttonStartAlphaCreation
            // 
            this.buttonStartAlphaCreation.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonStartAlphaCreation.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStartAlphaCreation.Location = new System.Drawing.Point(3, 74);
            this.buttonStartAlphaCreation.Name = "buttonStartAlphaCreation";
            this.buttonStartAlphaCreation.Size = new System.Drawing.Size(438, 25);
            this.buttonStartAlphaCreation.TabIndex = 2;
            this.buttonStartAlphaCreation.Text = "Start";
            this.buttonStartAlphaCreation.UseVisualStyleBackColor = true;
            this.buttonStartAlphaCreation.Click += new System.EventHandler(this.ButtonAddTrial_Click);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Dock = System.Windows.Forms.DockStyle.Top;
            this.numericUpDown1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericUpDown1.Location = new System.Drawing.Point(3, 17);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(438, 21);
            this.numericUpDown1.TabIndex = 3;
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numericUpDown1);
            this.groupBox1.Controls.Add(this.buttonAbortAlphaCreation);
            this.groupBox1.Controls.Add(this.buttonStartAlphaCreation);
            this.groupBox1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 639);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(444, 102);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "2.4) [Optional] Alpha account creation";
            // 
            // buttonAbortAlphaCreation
            // 
            this.buttonAbortAlphaCreation.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonAbortAlphaCreation.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAbortAlphaCreation.Location = new System.Drawing.Point(3, 49);
            this.buttonAbortAlphaCreation.Name = "buttonAbortAlphaCreation";
            this.buttonAbortAlphaCreation.Size = new System.Drawing.Size(438, 25);
            this.buttonAbortAlphaCreation.TabIndex = 4;
            this.buttonAbortAlphaCreation.Text = "Abort";
            this.buttonAbortAlphaCreation.UseVisualStyleBackColor = true;
            this.buttonAbortAlphaCreation.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboBoxProxies);
            this.groupBox2.Controls.Add(this.buttonOpenBrowser);
            this.groupBox2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(12, 40);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(444, 78);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "1) Select proxy to work with";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button5);
            this.groupBox3.Controls.Add(this.groupBox7);
            this.groupBox3.Controls.Add(this.groupBox5);
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(12, 124);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(444, 509);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "2) EVE account creation";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.button4);
            this.groupBox7.Controls.Add(this.buttonAbortEmailValidation);
            this.groupBox7.Controls.Add(this.buttonValidateEveAccount);
            this.groupBox7.Location = new System.Drawing.Point(6, 373);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(432, 90);
            this.groupBox7.TabIndex = 4;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "2.3) Validate EVE account";
            // 
            // button4
            // 
            this.button4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.Location = new System.Drawing.Point(3, 41);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(426, 23);
            this.button4.TabIndex = 14;
            this.button4.Text = ".";  //"Login to Email (IMAP): Find Email from CCP: Click Validation Link";
            this.button4.UseVisualStyleBackColor = true;
            //this.button4.Click += new System.EventHandler(this.ButtonValidateEveAccount_Click);
            // 
            // buttonAbortEmailValidation
            // 
            this.buttonAbortEmailValidation.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonAbortEmailValidation.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonAbortEmailValidation.Location = new System.Drawing.Point(3, 17);
            this.buttonAbortEmailValidation.Name = "buttonAbortEmailValidation";
            this.buttonAbortEmailValidation.Size = new System.Drawing.Size(426, 25);
            this.buttonAbortEmailValidation.TabIndex = 13;
            this.buttonAbortEmailValidation.Text = "Abort";
            this.buttonAbortEmailValidation.UseVisualStyleBackColor = true;
            this.buttonAbortEmailValidation.Click += new System.EventHandler(this.ButtonAbortEmailValidation_Click);
            // 
            // buttonValidateEveAccount
            // 
            this.buttonValidateEveAccount.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonValidateEveAccount.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonValidateEveAccount.Location = new System.Drawing.Point(3, 64);
            this.buttonValidateEveAccount.Name = "buttonValidateEveAccount";
            this.buttonValidateEveAccount.Size = new System.Drawing.Size(426, 23);
            this.buttonValidateEveAccount.TabIndex = 12;
            this.buttonValidateEveAccount.Text = "Visit Email WebSite";
            this.buttonValidateEveAccount.UseVisualStyleBackColor = true;
            this.buttonValidateEveAccount.Click += new System.EventHandler(this.ButtonOpenEmail_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.button1);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Controls.Add(this.textBoxEvePassword);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.textBoxEveAccountName);
            this.groupBox5.Controls.Add(this.buttonCreateEveAccount);
            this.groupBox5.Location = new System.Drawing.Point(6, 262);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(432, 105);
            this.groupBox5.TabIndex = 3;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "2.2) Create EVE account";
            // 
            // button1
            // 
            this.button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(198, 71);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(228, 22);
            this.button1.TabIndex = 20;
            this.button1.Text = "Visit EVE Account Creation WebSite";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ButtonCreateEveAccountManually_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(3, 57);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "EVE Password";
            // 
            // textBoxEvePassword
            // 
            this.textBoxEvePassword.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxEvePassword.Location = new System.Drawing.Point(3, 73);
            this.textBoxEvePassword.Name = "textBoxEvePassword";
            this.textBoxEvePassword.Size = new System.Drawing.Size(179, 21);
            this.textBoxEvePassword.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Top;
            this.label5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(3, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "EVE account name";
            // 
            // textBoxEveAccountName
            // 
            this.textBoxEveAccountName.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxEveAccountName.Location = new System.Drawing.Point(3, 33);
            this.textBoxEveAccountName.Name = "textBoxEveAccountName";
            this.textBoxEveAccountName.Size = new System.Drawing.Size(179, 21);
            this.textBoxEveAccountName.TabIndex = 13;
            // 
            // buttonCreateEveAccount
            // 
            this.buttonCreateEveAccount.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonCreateEveAccount.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCreateEveAccount.Location = new System.Drawing.Point(198, 31);
            this.buttonCreateEveAccount.Name = "buttonCreateEveAccount";
            this.buttonCreateEveAccount.Size = new System.Drawing.Size(228, 23);
            this.buttonCreateEveAccount.TabIndex = 13;
            this.buttonCreateEveAccount.Text = "Create EVE Account";
            this.buttonCreateEveAccount.UseVisualStyleBackColor = true;
            this.buttonCreateEveAccount.Click += new System.EventHandler(this.ButtonCreateEveAccount_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.button3);
            this.groupBox4.Controls.Add(this.button2);
            this.groupBox4.Controls.Add(this.buttonCreateEmailAccount);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.groupBox6);
            this.groupBox4.Controls.Add(this.textBoxEmailPassword);
            this.groupBox4.Controls.Add(this.textBoxIMAPHost);
            this.groupBox4.Controls.Add(this.textBoxEmailAddress);
            this.groupBox4.Location = new System.Drawing.Point(6, 19);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(432, 245);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "2.1) Create Email account";
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(9, 200);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(417, 25);
            this.button3.TabIndex = 14;
            this.button3.Text = "Create new Alias using another existing Email Account";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.ButtonCreateEmailAlias_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(9, 169);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(417, 25);
            this.button2.TabIndex = 13;
            this.button2.Text = "Visit New Email Account WebSite";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ButtonCreateEmailAccountManually_Click);
            // 
            // buttonCreateEmailAccount
            // 
            this.buttonCreateEmailAccount.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCreateEmailAccount.Location = new System.Drawing.Point(9, 138);
            this.buttonCreateEmailAccount.Name = "buttonCreateEmailAccount";
            this.buttonCreateEmailAccount.Size = new System.Drawing.Size(417, 25);
            this.buttonCreateEmailAccount.TabIndex = 12;
            this.buttonCreateEmailAccount.Text = "Create Email Account";
            this.buttonCreateEmailAccount.UseVisualStyleBackColor = true;
            this.buttonCreateEmailAccount.Click += new System.EventHandler(this.ButtonCreateEmailAccount_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(6, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Email Password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Email address";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "IMAPHost";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.radioButtonOutlook);
            this.groupBox6.Controls.Add(this.radioButtonYandex);
            this.groupBox6.Controls.Add(this.radioButtonGmail);
            this.groupBox6.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox6.Location = new System.Drawing.Point(298, 21);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(128, 109);
            this.groupBox6.TabIndex = 6;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Select Email provider";
            // 
            // radioButtonOutlook
            // 
            this.radioButtonOutlook.AutoSize = true;
            this.radioButtonOutlook.Checked = true;
            this.radioButtonOutlook.Location = new System.Drawing.Point(16, 65);
            this.radioButtonOutlook.Name = "radioButtonOutlook";
            this.radioButtonOutlook.Size = new System.Drawing.Size(62, 17);
            this.radioButtonOutlook.TabIndex = 6;
            this.radioButtonOutlook.TabStop = true;
            this.radioButtonOutlook.Text = "Outlook";
            this.radioButtonOutlook.UseVisualStyleBackColor = true;
            this.radioButtonOutlook.CheckedChanged += new System.EventHandler(this.RadioButtonOutlook_CheckedChanged);
            // 
            // radioButtonYandex
            // 
            this.radioButtonYandex.AutoSize = true;
            this.radioButtonYandex.Enabled = false;
            this.radioButtonYandex.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonYandex.Location = new System.Drawing.Point(16, 42);
            this.radioButtonYandex.Name = "radioButtonYandex";
            this.radioButtonYandex.Size = new System.Drawing.Size(61, 17);
            this.radioButtonYandex.TabIndex = 5;
            this.radioButtonYandex.Text = "Yandex";
            this.radioButtonYandex.UseVisualStyleBackColor = true;
            this.radioButtonYandex.CheckedChanged += new System.EventHandler(this.RadioButtonYandex_CheckedChanged);
            // 
            // radioButtonGmail
            // 
            this.radioButtonGmail.AutoSize = true;
            this.radioButtonGmail.Enabled = false;
            this.radioButtonGmail.Location = new System.Drawing.Point(16, 19);
            this.radioButtonGmail.Name = "radioButtonGmail";
            this.radioButtonGmail.Size = new System.Drawing.Size(50, 17);
            this.radioButtonGmail.TabIndex = 4;
            this.radioButtonGmail.Text = "Gmail";
            this.radioButtonGmail.UseVisualStyleBackColor = true;
            this.radioButtonGmail.CheckedChanged += new System.EventHandler(this.RadioButtonGmail_CheckedChanged);
            // 
            // textBoxEmailPassword
            // 
            this.textBoxEmailPassword.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxEmailPassword.Location = new System.Drawing.Point(9, 71);
            this.textBoxEmailPassword.Name = "textBoxEmailPassword";
            this.textBoxEmailPassword.Size = new System.Drawing.Size(266, 21);
            this.textBoxEmailPassword.TabIndex = 2;
            // 
            // textBoxIMAPHost
            // 
            this.textBoxIMAPHost.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxIMAPHost.Location = new System.Drawing.Point(9, 111);
            this.textBoxIMAPHost.Name = "textBoxIMAPHost";
            this.textBoxIMAPHost.Size = new System.Drawing.Size(266, 21);
            this.textBoxIMAPHost.TabIndex = 1;
            this.textBoxIMAPHost.Text = "outlook.office365.com";
            // 
            // textBoxEmailAddress
            // 
            this.textBoxEmailAddress.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxEmailAddress.Location = new System.Drawing.Point(9, 32);
            this.textBoxEmailAddress.Name = "textBoxEmailAddress";
            this.textBoxEmailAddress.Size = new System.Drawing.Size(266, 21);
            this.textBoxEmailAddress.TabIndex = 0;
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 778);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(467, 24);
            this.progressBar1.TabIndex = 4;
            // 
            // buttonGenerateRandom
            // 
            this.buttonGenerateRandom.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGenerateRandom.Location = new System.Drawing.Point(15, 9);
            this.buttonGenerateRandom.Name = "buttonGenerateRandom";
            this.buttonGenerateRandom.Size = new System.Drawing.Size(435, 25);
            this.buttonGenerateRandom.TabIndex = 12;
            this.buttonGenerateRandom.Text = "Generate Random Account Info";
            this.buttonGenerateRandom.UseVisualStyleBackColor = true;
            this.buttonGenerateRandom.Click += new System.EventHandler(this.ButtonGenerateRandom_Click);
            // 
            // button5
            // 
            this.button5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button5.Location = new System.Drawing.Point(3, 483);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(438, 23);
            this.button5.TabIndex = 13;
            this.button5.Text = "Create entry in Launcher for thie new EveAccount ";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.ButtonCreateLauncherEveAccountEntry_Click);
            // 
            // EveAccountCreatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 802);
            this.Controls.Add(this.buttonGenerateRandom);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.progressBar1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EveAccountCreatorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EveAccountCreator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EveAccountCreatorForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EveAccountCreatorForm_FormClosed);
            this.Load += new System.EventHandler(this.EveAccountCreatorForm_Load);
            this.Shown += new System.EventHandler(this.EveAccountCreatorForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxProxies;
        private System.Windows.Forms.Button buttonOpenBrowser;
        private System.Windows.Forms.Button buttonStartAlphaCreation;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button buttonAbortAlphaCreation;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RadioButton radioButtonYandex;
        private System.Windows.Forms.RadioButton radioButtonGmail;
        private System.Windows.Forms.TextBox textBoxEmailPassword;
        private System.Windows.Forms.TextBox textBoxIMAPHost;
        private System.Windows.Forms.TextBox textBoxEmailAddress;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button buttonValidateEveAccount;
        private System.Windows.Forms.Button buttonCreateEveAccount;
        private System.Windows.Forms.Button buttonCreateEmailAccount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxEveAccountName;
        private System.Windows.Forms.RadioButton radioButtonOutlook;
        private System.Windows.Forms.Button buttonAbortEmailValidation;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxEvePassword;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button buttonGenerateRandom;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
    }
}