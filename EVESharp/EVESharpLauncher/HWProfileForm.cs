/*
 * ---------------------------------------
 * User: duketwo
 * Date: 21.06.2014
 * Time: 11:10
 *
 * ---------------------------------------
 */

using SharedComponents.EVE;
using SharedComponents.Utility;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EVESharpLauncher
{
    /// <summary>
    ///     Description of HWProfileForm.
    /// </summary>
    public partial class HWProfileForm : Form
    {
        #region Fields

        private EveAccount EA;

        #endregion Fields

        #region Constructors

        public HWProfileForm(EveAccount eA)
        {
            EA = eA;
            InitializeComponent();
            Text = string.Format("HWProfile [{0}]", EA.CharacterName);
            if (EA.HWSettings != null && string.IsNullOrEmpty(EA.HWSettings.MachineGuid))
            {
                EA.HWSettings.MachineGuid = Guid.NewGuid().ToString();
            }

            if (EA.HWSettings != null && string.IsNullOrEmpty(EA.HWSettings.LauncherMachineHash))
            {
                EA.HWSettings.LauncherMachineHash = LauncherHash.GetRandomLauncherHash().Item1;
            }

            if (EA.HWSettings != null && EA.HWSettings.SystemReservedMemory == 0)
            {
                EA.HWSettings.SystemReservedMemory = (ulong)(2 * new Random().Next(8 / 2, 100 / 2));
        }

        }

        #endregion Constructors

        #region Methods

        private void Button1Click(object sender, EventArgs e)
        {

        }

        private void Button2Click(object sender, EventArgs e)
        {
            if (EA.HWSettings != null)
            {
                var content = Clipboard.GetText();
                EA.HWSettings.ParseDXDiagFile(content);
                LoadSettings();
            }
        }

        private void Button3Click(object sender, EventArgs e)
        {
            GenerateRandom();
        }

        private void Button5Click(object sender, EventArgs e)
        {
            button5.Enabled = false;
            Task.Run(() =>
            {
                try
                {
            if (EA.HWSettings.Proxy == null)
            {
                        this.Invoke((MethodInvoker)delegate { MessageBox.Show("Error: Proxy is null"); });
            }
            else
            {
                        var response = EA.HWSettings.Proxy.GetExternalIp(3);
                        this.Invoke((MethodInvoker)delegate { MessageBox.Show("Whoer.net/ip response: " + response); });
                Cache.Instance.Log("Whoer.net/ip response: " + response);
            }
        }
                catch (Exception exception)
                {

                }
                finally
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        button5.Enabled = true;
                    });
                }

            });

        }

        private void ComboBoxProxiesOnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            var c = (ComboBox)sender;
            var p = (Proxy)c.SelectedItem;

            if (p != null && EA.HWSettings != null)
            {
                EA.HWSettings.Proxy = p;
                textBoxProxyIP.Text = p.Ip + ":" + p.Socks5Port;
                textBoxProxyUsername.Text = p.Username;
                textBoxProxyPassword.Text = p.Password;
            }
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(CompressUtil.Compress(Util.XmlSerialize(EA.HWSettings)));
        }

        private void GenerateRandom()
        {
            Proxy p = null;
            if (EA.HWSettings != null && EA.HWSettings.Proxy != null)
                p = EA.HWSettings.Proxy;

            EA.HWSettings = new HWSettings();

            if (p != null)
                EA.HWSettings.Proxy = p;

            EA.HWSettings.GenerateRandomProfile();
            LoadSettings();
        }

        private void HWProfileForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            comboBoxProxies.SelectedIndexChanged -= ComboBoxProxiesOnSelectedIndexChanged;
            SaveSettings();
        }

        private void HWProfileFormFormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void HWProfileFormLoad(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string clipboard = Clipboard.GetText();
            try
            {
                var result = Util.XmlDeserialize(CompressUtil.DecompressText(clipboard), typeof(HWSettings));
                if (result is HWSettings setting)
                {
                    EA.HWSettings = setting;
                    Cache.Instance.Log($"HWProfile imported.");
                    LoadSettings();
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log($"Unable to import hardware profile from clipboard, are you sure it is a base64 Hardware profile exported string?" + Environment.NewLine +
                    $"Clipboard text:\"{clipboard}\"");
            }
        }

        private void LoadSettings()
        {
            comboBoxProxies.DataSource = Cache.Instance.EveSettings.Proxies;
            comboBoxProxies.DisplayMember = "Description";
            comboBoxProxies.SelectedIndex = -1;
            try
            {
                if (EA.HWSettings != null)
                {
                    textBoxTotalPhysRam.Text = EA.HWSettings.TotalPhysRam.ToString();
                    textBoxSystemReservedRam.Text = EA.HWSettings.SystemReservedMemory.ToString();
                    textBoxWindowsUserLogin.Text = EA.HWSettings.WindowsUserLogin;
                    textBoxComputername.Text = EA.HWSettings.Computername;
                    textBoxWindowsKey.Text = EA.HWSettings.WindowsKey;
                    textBoxMachineGuid.Text = EA.HWSettings.MachineGuid;

                    textBoxNetworkAdapterGuid.Text = EA.HWSettings.NetworkAdapterGuid.ToString();
                    textBoxNetworkAddress.Text = EA.HWSettings.NetworkAddress.ToString();
                    textBoxMacAddress.Text = EA.HWSettings.MacAddress.ToString();

                    textBoxProcessorIdent.Text = EA.HWSettings.ProcessorIdent.ToString();
                    textBoxProcessorRev.Text = EA.HWSettings.ProcessorRev.ToString();
                    textBoxProcessorCoreAmount.Text = EA.HWSettings.ProcessorCoreAmount.ToString();
                    textBoxProcessorLevel.Text = EA.HWSettings.ProcessorLevel.ToString();

                    textBoxGpuDescription.Text = EA.HWSettings.GpuDescription.ToString();
                    textBoxGpuDeviceId.Text = EA.HWSettings.GpuDeviceId.ToString();
                    textBoxGpuVendorId.Text = EA.HWSettings.GpuVendorId.ToString();
                    textBoxGpuRevision.Text = EA.HWSettings.GpuRevision.ToString();
                    textBoxGpuDriverversion.Text = EA.HWSettings.GpuDriverversion.ToString();
                    textBoxGpuDriverversionInt.Text = EA.HWSettings.GpuDriverversionInt.ToString();
                    textBoxGpuIdentifier.Text = EA.HWSettings.GpuIdentifier.ToString();
                    textBoxGPUDedicatedMemoryMB.Text = EA.HWSettings.GpuDedicatedMemoryMB.ToString();
                    textBoxLauncherMachineHash.Text = EA.HWSettings.LauncherMachineHash.ToString();

                    try
                    {
                        textBoxGpuManufacturer.Text = EA.HWSettings.GpuManufacturer.ToString();
                        textBoxGpuDriverDate.Text = EA.HWSettings.GpuDriverDate.ToString();
                        textBoxNetworkAdapterName.Text = EA.HWSettings.NetworkAdapterName.ToString();

                        textBoxMonitorHeight.Text = EA.HWSettings.MonitorHeight.ToString();
                        textBoxMonitorWidth.Text = EA.HWSettings.MonitorWidth.ToString();
                        textBoxMonitorName.Text = EA.HWSettings.MonitorName.ToString();
                        textBoxMonitorRefreshRate.Text = EA.HWSettings.MonitorRefreshrate.ToString();
                    }
                    catch (Exception)
                    {
                    }

                    if (EA.HWSettings.Proxy != null)
                    {
                        comboBoxProxies.SelectedItem = EA.HWSettings.Proxy;
                        textBoxProxyIP.Text = EA.HWSettings.Proxy.Ip + ":" + EA.HWSettings.Proxy.Socks5Port;
                        textBoxProxyUsername.Text = EA.HWSettings.Proxy.Username;
                        textBoxProxyPassword.Text = EA.HWSettings.Proxy.Password;
                    }
                    else
                    {
                        comboBoxProxies.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("HWProfile could not be loaded. Exception: " + ex);
            }

            comboBoxProxies.SelectedIndexChanged += ComboBoxProxiesOnSelectedIndexChanged;
        }

        private void SaveSettings()
        {
            try
            {
                DateTime? gpuDriverDate = Util.ParseDateTime(textBoxGpuDriverDate.Text);
                if (!gpuDriverDate.HasValue)
                {
                    MessageBox.Show("Invalid GpuDriverDate (probably in unparsable format)");
                    return;
                }

                EA.HWSettings = new HWSettings(
                    ulong.Parse(textBoxTotalPhysRam.Text),
                    textBoxWindowsUserLogin.Text,
                    textBoxComputername.Text,
                    textBoxWindowsKey.Text,
                    textBoxNetworkAdapterGuid.Text,
                    textBoxNetworkAddress.Text,
                    textBoxMacAddress.Text,
                    textBoxProcessorIdent.Text,
                    textBoxProcessorRev.Text,
                    textBoxProcessorCoreAmount.Text,
                    textBoxProcessorLevel.Text,
                    textBoxGpuDescription.Text,
                    uint.Parse(textBoxGpuDeviceId.Text),
                    uint.Parse(textBoxGpuVendorId.Text),
                    uint.Parse(textBoxGpuRevision.Text),
                    long.Parse(textBoxGpuDriverversion.Text),
                    textBoxGpuDriverversionInt.Text,
                    textBoxGpuIdentifier.Text,
                    EA.HWSettings.ProxyId,
                    textBoxMachineGuid.Text,
                    ulong.Parse(textBoxSystemReservedRam.Text),
                    textBoxLauncherMachineHash.Text,
                    gpuDriverDate ?? DateTime.Now,
                    textBoxGpuManufacturer.Text,
                    uint.Parse(textBoxGPUDedicatedMemoryMB.Text),
                    textBoxNetworkAdapterName.Text,
                    textBoxMonitorName.Text,
                    int.Parse(textBoxMonitorWidth.Text),
                    int.Parse(textBoxMonitorHeight.Text),
                    int.Parse(textBoxMonitorRefreshRate.Text)
                );
            }
            catch (Exception e)
            {
                Cache.Instance.Log("Exception: " + e.ToString());
            }
        }

        private void TextBoxGpuDriverversionIntTextChanged(object sender, EventArgs e)
        {
            if (EA.HWSettings != null)
            {
                EA.HWSettings.GpuDriverversionInt = textBoxGpuDriverversionInt.Text;
                textBoxGpuDriverversion.Text = EA.HWSettings.GpuDriverversion.ToString();
            }
        }

        #endregion Methods

        private void button4_Click(object sender, EventArgs e)
        {
            EA.HWSettings.GenerateRandomGpuProfile();
            this.LoadSettings();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            EA.HWSettings.GenerateNetworkProfile();
            this.LoadSettings();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            EA.HWSettings.GenerateNewMonitorProfile();
            this.LoadSettings();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            EA.HWSettings.GenerateNewLauncherMachineHash();
            this.LoadSettings();
        }
    }
}