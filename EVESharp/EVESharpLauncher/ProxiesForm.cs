//using SharedComponents.CurlUtil;
using SharedComponents.EVE;
using SharedComponents.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EVESharpLauncher
{
    public partial class ProxiesForm : Form
    {
        #region Constructors

        public ProxiesForm()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        private void Button2_Click(object sender, EventArgs e)
        {
            foreach (Proxy p in Cache.Instance.EveSettings.Proxies)
                new Task(() =>
                {
                    bool result = false;
                    try
                    {
                        result = p.CheckSocks5InternetConnectivity(SampleEveAccount);
                        //if (result)
                        //    result = p.CheckHttpProxyInternetConnectivity(SampleEveAccount);

                        Invoke((MethodInvoker)delegate
                        {
                           p.IsAlive = result;
                           p.LastCheck = DateTime.UtcNow;
                           if (!p.IsAlive)
                               p.LastFail = DateTime.UtcNow;
                        });

                        if (p.IsAlive)
                        {
                            string ip = p.GetExternalIp();
                            ip = string.IsNullOrEmpty(ip) ? p.GetExternalIp() : ip;

                            Invoke((MethodInvoker)delegate { p.ExtIp = ip; });
                        }
                        Invoke((MethodInvoker)delegate { UpdateLinks(); });
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("Exception: " + ex);
                    }
                }).Start();
        }

        private EveAccount SampleEveAccount
        {
            get
            {
                return new EveAccount("TestAccount", "TestChar", "TestPassword", DateTime.UtcNow, DateTime.UtcNow, "test@gmail.com", "password");
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            foreach (Proxy p in Cache.Instance.EveSettings.Proxies)
                p.Clear();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            try
            {
                Cache.Instance.EveSettings.Proxies.Clear();
                foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List)
                    eA.HWSettings.ProxyId = -1;
            }
            catch (Exception exception)
            {
                Cache.Instance.Log("Exception: " + exception);
            }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            Cache.Instance.EveSettings.Proxies.Add(new Proxy("", "", "", "", Cache.Instance.EveSettings.Proxies));
        }

        private void DeleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DataGridView dgv = ActiveControl as DataGridView;
            if (dgv == null) return;
            int selected = dgv.SelectedCells[0].OwningRow.Index;

            if (selected >= 0)
            {
                Proxy p = Cache.Instance.EveSettings.Proxies[selected];
                foreach (
                    EveAccount eA in
                    Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(
                        a => a.HWSettings != null && a.HWSettings.Proxy != null && a.HWSettings.Proxy.Id == p.Id))
                {
                    HWSettings hw = eA.HWSettings;
                    hw.ProxyId = -1;
                    eA.HWSettings = hw;
                }
                Cache.Instance.EveSettings.Proxies.RemoveAt(selected);
            }
        }

        private void ProxiesForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Cache.Instance.EveSettingsSerializeableSortableBindingList.List.XmlSerialize(
                Cache.Instance.EveSettingsSerializeableSortableBindingList.FilePathName);
        }

        private void ProxiesForm_Load(object sender, EventArgs e)
        {
            if (Cache.Instance.EveSettings.Proxies == null)
                Cache.Instance.EveSettings.Proxies = new ConcurrentBindingList<Proxy>();

            dataGridProxies.DataSource = Cache.Instance.EveSettings.Proxies;
            foreach (DataGridViewColumn col in dataGridProxies.Columns)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem
                {
                    Text = col.Name
                };

                if (menuItem.Text == "Description")
                {
                    col.DividerWidth = 2;
                    col.Frozen = true;
                }
            }
        }

        private void ProxiesForm_Shown(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)delegate { UpdateLinks(); });
        }

        private void UpdateLinks()
        {
            foreach (Proxy p in Cache.Instance.EveSettings.Proxies)
            {
                p.LinkedAccounts = string.Empty;
                p.LinkedCharacters = string.Empty;
                IEnumerable<EveAccount> eAs = Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(
                    a => a.HWSettings != null && a.HWSettings.Proxy != null && a.HWSettings.Proxy == p);
                if (eAs.Any())
                {
                    p.LinkedAccounts = string.Empty;
                    p.LinkedCharacters = string.Empty;
                    int i = 0;
                    foreach (EveAccount eA in eAs)
                    {
                        if (i == 0)
                        {
                            p.LinkedAccounts += eA.AccountName;
                            p.LinkedCharacters += eA.CharacterName;
                        }
                        else
                        {
                            p.LinkedAccounts += ", " + eA.AccountName;
                            p.LinkedCharacters += ", " + eA.CharacterName;
                        }
                        i++;
                    }
                }
            }
        }

        #endregion Methods
    }
}