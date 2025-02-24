extern alias SC;

using EVESharpCore.Cache;
using EVESharpCore.Controllers.Base;
using EVESharpCore.Logging;
using SC::SharedComponents.Controls;
using SC::SharedComponents.EVE;
using SC::SharedComponents.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace EVESharpCore
{
    extern alias SC;

    public partial class EVESharpCoreForm : Form
    {
        #region Constructors

        public EVESharpCoreForm()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Log.WriteLine("Exception [" + ex + "]");
            }
        }

        #endregion Constructors

        #region Fields

        private const int MAXIMIZED_HEIGHT = 327;
        private const int MINIMIZED_HEIGHT = 16;

        #endregion Fields

        #region Methods

        public void AddControllerTab(IController controller)
        {
            // just use invoke regardless
            Invoke(new Action(() =>
            {
                Form frm = controller.Form;
                if (frm != null && !tabControlMain.TabPages.ContainsKey(frm.Text))
                {
                    Log.WriteLine($"Adding controller form {frm.Text}");
                    frm.FormBorderStyle = FormBorderStyle.None;
                    TabPage tab = new TabPage(frm.Text);
                    controller.TabPage = tab;
                    tab.BackColor = Color.Blue;
                    tab.Name = frm.Text;
                    frm.TopLevel = false;
                    frm.Parent = tab;
                    frm.Visible = true;
                    frm.Dock = DockStyle.Fill;
                    tabControlMain.TabPages.Add(tab);
                    //tabControlMain.SelectedTab = tab;
                }
            }));
        }

        public void RemoveControllerTab(IController controller)
        {
            // just use invoke regardless
            Invoke(new Action(() =>
            {
                if (controller.Form != null && controller.TabPage != null)
                {
                    Log.WriteLine($"Removing controller form {controller.Form.Text}");
                    tabControlMain.TabPages.Remove(controller.TabPage);
                }
            }));
        }

        private void AddLog(string msg, Color? col = null)
        {
            try
            {
                col = col ?? Color.White;
                ListViewItem item = new ListViewItem
                {
                    Text = msg
                };
                label11.Text = ESCache.Instance.CharName + "-" + msg;
                item.ForeColor = (Color)col;

                if (listViewLogs.Items.Count >= 1000)
                    listViewLogs.Items.Clear();
                listViewLogs.Items.Add(item);

                if (listViewLogs.Items.Count > 1)
                    listViewLogs.Items[listViewLogs.Items.Count - 1].EnsureVisible();
            }
            catch (Exception) { }
        }

        private void AddLogInvoker(string msg, Color? col)
        {
            try
            {
                if (!IsHandleCreated)
                    return;

                Invoke((MethodInvoker) delegate { AddLog(msg, col); });
            }
            catch (Exception)
            {
            }
        }

        private void Button2Click(object sender, EventArgs e)
        {
            if (Height == MINIMIZED_HEIGHT)
            {
                Height = MAXIMIZED_HEIGHT;
                tabControlMain.Visible = true;
            }
            else
            {
                Height = MINIMIZED_HEIGHT;
                tabControlMain.Visible = false;
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonAddController_Click(object sender, EventArgs e)
        {
            ControllerManager.Instance.AddController(((Type) comboBoxControllers.SelectedItem).Name);
        }

        private void ButtonOpenLogDirectory_Click(object sender, EventArgs e)
        {
            Process.Start(Log.BotLogpath);
        }

        private void ButtonRemoveController_Click(object sender, EventArgs e)
        {
            ControllerManager.Instance.RemoveController((Type) comboBoxControllers.SelectedItem);
        }

        private void DataGridControllers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Method intentionally left empty.
        }

        private void DataGridControllers_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Method intentionally left empty.
        }

        private void DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void EVESharpCoreFormLoad(object sender, EventArgs e)
        {
            try
            {
                Log.AsyncLogQueue.OnMessage += AddLogInvoker;
                Log.AsyncLogQueue.StartWorker();
                listViewLogs.OwnerDraw = true;
                listViewLogs.DrawColumnHeader += DrawColumnHeader;
                FormUtil.Color = listViewLogs.BackColor;
                FormUtil.Font = listViewLogs.Font;
                listViewLogs.DrawItem += FormUtil.DrawItem;
                listViewLogs.AutoArrange = false;

                listViewLogs.ItemActivate += delegate
                {
                    int i = listViewLogs.SelectedIndices[0];
                    new ListViewItemForm(Regex.Replace(listViewLogs.Items[i].Text, @"\r\n|\n\r|\n|\r", Environment.NewLine)).Show();
                };
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void EVESharpCoreFormShown(object sender, EventArgs e)
        {
            try
            {
                // Late init here because new controllers can have a form as attribute, which will be added as tab page
                ControllerManager.Instance.Initialize();

                label11.Text = string.Empty;
                Height = MINIMIZED_HEIGHT;
                tabControlMain.Visible = false;
                ESCache.Instance.TaskSetEveAccountAttribute(nameof(EveAccount.EVESharpCoreFormHWnd), (long) Handle);

                IEnumerable<Type> controllers = new List<Type>();
                try
                {
                    controllers = Assembly.GetAssembly(typeof(BaseController)).GetTypes().Where(t => t.IsSubclassOf(typeof(BaseController)));
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // now look at ex.LoaderExceptions - this is an Exception[], so:
                    foreach (Exception inner in ex.LoaderExceptions)
                    {
                        // write details of "inner", in particular inner.Message
                        Log.WriteLine(inner.Message);
                        Log.WriteLine(inner.ToString());
                    }
                }

                comboBoxControllers.DataSource = controllers.Where(k => !ControllerManager.DEFAULT_CONTROLLERS.Contains(k)).OrderBy(k => k.Name).ToList();
                comboBoxControllers.DisplayMember = "Name";
                dataGridControllers.DataSource = ControllerManager.Instance.ControllerList;
                //dataGridControllers.Columns[nameof(BaseController.LocalPulse)].DefaultCellStyle.Format = "HH:mm:ss";
                //dataGridControllers.Columns[nameof(BaseController.Avg)].DefaultCellStyle.Format = "N0";
            }
            catch (Exception exception)
            {
                Log.WriteLine(exception.ToString());
            }
        }

        private void EVESharpFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Log.WriteLine("Closing EveSharp Bot");
                ESCache.Instance.TaskSetEveAccountAttribute(nameof(EveAccount.LastInteractedWithEVE), DateTime.UtcNow.AddDays(1));
                Program.IsShuttingDown = true;
                Log.AsyncLogQueue.OnMessage -= AddLogInvoker;
                ControllerManager.Instance.RemoveAllControllers();
                ControllerManager.Instance.Dispose();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void PauseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ControllerManager.Instance.SetPause(((CheckBox) sender).Checked);
            Log.WriteLine("ManuallyPausedViaUI = [" + ((CheckBox) sender).Checked + "]");
        }

        #endregion Methods
    }
}