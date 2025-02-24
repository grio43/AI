using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using HookManager.Win32Hooks;
using SharedComponents.Controls;
using SharedComponents.Utility;

namespace HookManager
{
    public partial class PacketViewerForm : Form
    {
        public PacketViewerForm()
        {
            InitializeComponent();
        }

        private void PacketViewerForm_Load(object sender, EventArgs e)
        {
            try
            {
                SymmetricEncryptionController.AsyncLogQueue.OnMessage += AddLogS;
                SymmetricDecryptionController.AsyncLogQueue.OnMessage += AddLogR;
                SymmetricDecryptionController.AsyncLogQueue.StartWorker();
                SymmetricEncryptionController.AsyncLogQueue.StartWorker();
                //outgoingPacketsListView.OwnerDraw = true;
                //outgoingPacketsListView.DrawColumnHeader += DrawColumnHeader;
                //FormUtil.Color = outgoingPacketsListView.BackColor;
                //FormUtil.Font = outgoingPacketsListView.Font;
                //outgoingPacketsListView.DrawItem += FormUtil.DrawItem;
                //outgoingPacketsListView.AutoArrange = false;

                incomingPacketsListView.ItemActivate += delegate (object s, EventArgs _args)
                {
                    var i = incomingPacketsListView.SelectedIndices[0];
                    new ListViewItemForm(Regex.Replace(incomingPacketsListView.Items[i].Text, @"\r\n|\n\r|\n|\r", Environment.NewLine)).Show();
                };

                outgoingPacketsListView.ItemActivate += delegate (object s, EventArgs _args)
                {
                    var i = outgoingPacketsListView.SelectedIndices[0];
                    new ListViewItemForm(Regex.Replace(outgoingPacketsListView.Items[i].Text, @"\r\n|\n\r|\n|\r", Environment.NewLine)).Show();
                };
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void AddLogS(string msg, Color? col = null)
        {
            try
            {
                col = col ?? Color.Black;
                var item = new ListViewItem();
                item.Text = msg;
                item.ForeColor = (Color)col;
                outgoingPacketsListView.Items.Add(item);
            }
            catch (Exception)
            {
            }
        }

        private void AddLogR(string msg, Color? col = null)
        {
            try
            {
                col = col ?? Color.Black;
                var item = new ListViewItem();
                item.Text = msg;
                item.ForeColor = (Color)col;
                incomingPacketsListView.Items.Add(item);
            }
            catch (Exception)
            {
            }
        }

        private void PacketViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SymmetricEncryptionController.AsyncLogQueue.OnMessage -= AddLogS;
            SymmetricDecryptionController.AsyncLogQueue.OnMessage -= AddLogR;
        }

        private void outgoingPacketsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.richTextBox1.Text = Regex.Replace(outgoingPacketsListView.SelectedItems[0].Text, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            }
            catch (Exception exception)
            {
                //Console.WriteLine(exception.ToString());
            }
        }

        private void incomingPacketsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.richTextBox2.Text = Regex.Replace(incomingPacketsListView.SelectedItems[0].Text, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            }
            catch (Exception exception)
            {
                //Console.WriteLine(exception.ToString());
            }
        }
    }
}
