//using EVESharpLauncher.SocksServer;
using SharedComponents.EVE;
using SharedComponents.Notifcations;
using SharedComponents.SharpLogLite.Model;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace EVESharpLauncher
{
    public partial class SettingsForm : Form
    {
        #region Constructors

        public SettingsForm()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        private void ButtonSendTestEmail_Click(object sender, EventArgs e)
        {
            Email.SendGmail(textBoxGmailPassword.Text, textBoxGmailUser.Text, textBoxReceiverEmailAddress.Text, "EVESharp Event: Test", "Test email.");
        }

        private void CheckBox1CheckedChanged(object sender, EventArgs e)
        {
        }

        private void CheckBoxSharpLogLite_CheckedChanged(object sender, EventArgs e)
        {
            Cache.Instance.EveSettings.SharpLogLite = checkBoxSharpLogLite.Checked;

            if (Cache.Instance.EveSettings.SharpLogLite)
                SharpLogLiteHandler.Instance.StartListening();
            else
                SharpLogLiteHandler.Instance.StopListening();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            textBoxEveLocation.Text = Cache.Instance.EveSettings.EveDirectory;
            textBoxGmailUser.Text = Cache.Instance.EveSettings.GmailUser;
            textBoxGmailPassword.Text = Cache.Instance.EveSettings.GmailPassword;
            textBoxReceiverEmailAddress.Text = Cache.Instance.EveSettings.ReceiverEmailAddress;
            checkBoxSharpLogLite.Checked = Cache.Instance.EveSettings.SharpLogLite;
            checkBoxAllowEveClientAutoUpdate.Checked = Cache.Instance.EveSettings.AutoUpdateEve;
            //trackBar1.Minimum = Cache.Instance.EveSettings.BackgroundFPSMin;
            //trackBar1.Maximum = Cache.Instance.EveSettings.BackgroundFPSMax;
            //if (Cache.Instance.EveSettings.BackgroundFPS < trackBar1.Minimum)
            //    Cache.Instance.EveSettings.BackgroundFPS = trackBar1.Minimum;
            //if (Cache.Instance.EveSettings.BackgroundFPS > trackBar1.Maximum)
            //    Cache.Instance.EveSettings.BackgroundFPS = trackBar1.Maximum;
            //trackBar1.Value = Cache.Instance.EveSettings.BackgroundFPS;

            if (Cache.Instance.EveSettings.TimeBetweenEVELaunchesMin < trackBar2.Minimum)
                Cache.Instance.EveSettings.TimeBetweenEVELaunchesMin = trackBar2.Minimum;
            if (Cache.Instance.EveSettings.TimeBetweenEVELaunchesMin > trackBar2.Maximum)
                Cache.Instance.EveSettings.TimeBetweenEVELaunchesMin = trackBar2.Maximum;
            trackBar2.Value = Cache.Instance.EveSettings.TimeBetweenEVELaunchesMin;

            if (Cache.Instance.EveSettings.TimeBetweenEVELaunchesMax < trackBar3.Minimum)
                Cache.Instance.EveSettings.TimeBetweenEVELaunchesMax = trackBar3.Minimum;
            if (Cache.Instance.EveSettings.TimeBetweenEVELaunchesMax > trackBar3.Maximum)
                Cache.Instance.EveSettings.TimeBetweenEVELaunchesMax = trackBar3.Maximum;
            trackBar3.Value = Cache.Instance.EveSettings.TimeBetweenEVELaunchesMax;

            comboBox1.DataSource = Enum.GetValues(typeof(LogSeverity));
            comboBox1.SelectedItem = Cache.Instance.EveSettings.SharpLogLiteLogSeverity;
            comboBox1.SelectedIndexChanged += delegate { Cache.Instance.EveSettings.SharpLogLiteLogSeverity = (LogSeverity)comboBox1.SelectedItem; };
        }

        private void TextBoxEveLocation_TextChanged(object sender, EventArgs e)
        {
            Cache.Instance.EveSettings.EveDirectory = textBoxEveLocation.Text;
        }

        private void TextBoxGmailPassword_TextChanged(object sender, EventArgs e)
        {
            Cache.Instance.EveSettings.GmailPassword = textBoxGmailPassword.Text;
        }

        private void TextBoxGmailUser_TextChanged(object sender, EventArgs e)
        {
            Cache.Instance.EveSettings.GmailUser = textBoxGmailUser.Text;
        }

        private void TextBoxReceiverEmailAddress_TextChanged(object sender, EventArgs e)
        {
            Cache.Instance.EveSettings.ReceiverEmailAddress = textBoxReceiverEmailAddress.Text;
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            //toolTip1.SetToolTip(trackBar1, trackBar1.Value.ToString());
            //Cache.Instance.EveSettings.BackgroundFPS = trackBar1.Value;
            //Debug.WriteLine($"Setting backgroundFPS to {trackBar1.Value}");
        }

        private void TrackBar2_Scroll(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(trackBar2, trackBar2.Value.ToString());
            Cache.Instance.EveSettings.TimeBetweenEVELaunchesMin = trackBar2.Value;
            Debug.WriteLine($"Setting TimeBetweenEVELaunchesMin to {trackBar2.Value}");
        }

        private void TrackBar3_Scroll(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(trackBar3, trackBar3.Value.ToString());
            Cache.Instance.EveSettings.TimeBetweenEVELaunchesMax = trackBar3.Value;
            Debug.WriteLine($"Setting TimeBetweenEVELaunchesMax to {trackBar3.Value}");
        }

        #endregion Methods

        private void checkBoxAllowEveClientAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            Cache.Instance.EveSettings.AutoUpdateEve = checkBoxAllowEveClientAutoUpdate.Checked;
            Cache.Instance.Log("Eve Client Autoupdate [" + Cache.Instance.EveSettings.AutoUpdateEve + "]");
        }
    }
}