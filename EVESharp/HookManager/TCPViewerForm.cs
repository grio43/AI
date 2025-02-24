
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HookManager.Win32Hooks;
using SharedComponents.Utility;

namespace HookManager
{
    public partial class TCPViewerForm : Form
    {
        private TcpConnectionManager _manager;
        public TCPViewerForm()
        {
            _manager = new TcpConnectionManager();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {   /**
            var connections = _manager.GetTcpConnections();
            var currentPid = Process.GetCurrentProcess().Id;
            var connectionsForCurrentProcess = connections.Where(c => c.ProcessId == currentPid).ToList();
            var socketInfoValues = SocketInfo.SocketsInfoDictionary.Values.ToList();

            List<SocketInfo> socketInfoList = new List<SocketInfo>();
            foreach (var info in connectionsForCurrentProcess)
            {
                var socketInfo = socketInfoValues.FirstOrDefault(s => Equals(s.SourceIP, info.LocalIPAddress) && s.SPort == info.LocalPort && Equals(s.DestIP, info.RemoteIPAddress) && s.DPort == info.RemotePort);
                if (socketInfo == null)
                {
                    socketInfo = new SocketInfo(info.LocalIPAddress, info.LocalPort, info.RemoteIPAddress,
                           info.RemotePort, IPAddress.Parse("0.0.0.0"), 0, "Unknown", IntPtr.Zero);
                }

                if (GetAddrInfoController.Hosts.ContainsKey(socketInfo.SocksDestIP.ToString()))
                {
                    var kvp = GetAddrInfoController.Domains.FirstOrDefault(e => e.Value == socketInfo.SocksDestIP.ToString());
                    if (kvp.Equals(default(KeyValuePair<string, string>)))
                    {
                        socketInfo.SocksDestDomain = "Unknown";
                    }
                    else
                    {
                        socketInfo.SocksDestDomain = kvp.Key;
                    }
                }

                socketInfoList.Add(socketInfo);
            }
            dataGridView1.DataSource = socketInfoList;

            dataGridView2.Columns.Clear();
            DataGridViewTextBoxColumn valueColumn = new DataGridViewTextBoxColumn();
            valueColumn.HeaderText = "Domains";
            dataGridView2.Columns.Add(valueColumn);
            DataGridViewTextBoxColumn valueColumnIps = new DataGridViewTextBoxColumn();
            valueColumnIps.HeaderText = "IPs";
            dataGridView2.Columns.Add(valueColumnIps);
            //dataGridView2.DataSource = new BindingSource(GetAddrInfoController.Hosts.Values, "Domains");

            foreach (var d in GetAddrInfoController.Domains)
            {
                dataGridView2.Rows.Add(d.Key, d.Value);
            }
            **/
        }

        private void TCPViewerForm_Shown(object sender, EventArgs e)
        {
            button1.PerformClick();
        }


        //dataGridView1.DataSource = connectionsForCurrentProcess;
        //dataGridView1.DataSource = SocketInfo.SocketsInfoDictionary.Values.ToList();
    }
}
