using SharedComponents.EVE;
using SharedComponents.EVE.ClientSettings;
using SharedComponents.EVE.ClientSettings.SharedComponents.EVE.ClientSettings;
using SharedComponents.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace EVESharpLauncher
{
    public partial class ClientSettingForm : Form
    {
        /**
        #region Constructors

        public ClientSettingForm(EveAccount eveAccount)
        {
            _eA = eveAccount;

            InitializeComponent();
            Text = string.Format("ClientSetting [{0}]", _eA.CharacterName);

            if (_eA.ClientSetting == null)
                _eA.ClientSetting = new ClientSetting();
            Height = 500;
        }

        #endregion Constructors

        #region Methods

        private void ClientSettingForm_Load(object sender, EventArgs e)
        {
            SetupControls();
        }

        private void ClientSettingForm_Shown(object sender, EventArgs e)
        {
        }

        #endregion Methods

        #region Fields

        private const int CONTROL_WIDTH = 600;
        private readonly EveAccount _eA;
        private int _questorSettingGroupChangedCounter;
        private List<EveAccount> _questorSettingSyncList;

        #endregion Fields

        #region Methods

        public void AddBoolProperty(Panel pa, object ds, PropertyInfo p, int pWidth = CONTROL_WIDTH)
        {
            TableLayoutPanel panel = new TableLayoutPanel
            {
                RowCount = 1,
                ColumnCount = 1,
                Width = pWidth
            };
            CheckBox checkbox = new CheckBox
            {
                Text = p.Name,
                AutoSize = true,
                Padding = new Padding(3, 0, 0, 0)
            };
            Binding binding = new Binding("Checked", ds, p.Name);
            panel.Controls.Add(checkbox, 0, 0);
            checkbox.DataBindings.Add(binding);
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(checkbox, GetDescriptionAttributeValue(p));

            panel.Height = checkbox.Height;
            pa.Controls.Add(panel);
        }

        public void AddEnumProperty(Panel pa, object ds, PropertyInfo p, int pWidth = CONTROL_WIDTH)
        {
            GroupBox groupbox1 = new GroupBox
            {
                Text = p.Name
            };
            ComboBox combobox = new ComboBox();
            groupbox1.Width = pWidth;
            combobox.Dock = DockStyle.Fill;
            groupbox1.Height = combobox.Height + 20;
            groupbox1.Controls.Add(combobox);
            combobox.DropDownStyle = ComboBoxStyle.DropDownList;
            combobox.DataSource = Enum.GetValues(p.PropertyType);
            combobox.DataBindings.Add(new Binding("SelectedItem", ds, p.Name));
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(combobox, GetDescriptionAttributeValue(p));
            pa.Controls.Add(groupbox1);
        }

        public void AddIntOrStringProperty(Panel pa, object ds, PropertyInfo p, ConvertEventHandler con, int pWidth = CONTROL_WIDTH)
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.RowCount = 1;
            panel.ColumnCount = 2;
            TextBox textbox = new TextBox();
            panel.Height = textbox.Height + 4;
            panel.Width = pWidth;
            Label label = new Label
            {
                Text = p.Name,
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            textbox.Dock = DockStyle.Fill;

            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(label, GetDescriptionAttributeValue(p));
            toolTip.SetToolTip(textbox, GetDescriptionAttributeValue(p));

            panel.Controls.Add(label, 0, 0);
            panel.Controls.Add(textbox, 1, 0);
            pa.Controls.Add(panel);
            Binding binding = new Binding("Text", ds, p.Name);
            if (con != null)
                binding.Parse += con;
            textbox.DataBindings.Add(binding);
            textbox.TextChanged += delegate
            {
                if (p.Name.Equals(nameof(_eA.ClientSetting.QuestorMainSetting.QuestorSettingGroup)))
                {
                    if (_questorSettingGroupChangedCounter > 0)
                    {
                        p.SetValue(ds,
                            Convert.ChangeType(textbox.Text, p.PropertyType),
                            null);
                        ReloaderQuestorGroupSnyc();
                    }

                    _questorSettingGroupChangedCounter++; // counter to allow ignoring the inital change
                }
            };
        }

        public void AddListProperty(Panel pa, object ds, PropertyInfo p, int pWidth = CONTROL_WIDTH)
        {
            GroupBox groupbox1 = new GroupBox
            {
                Text = p.Name
            };
            DataGridView datagrid1 = new DataGridView
            {
                Dock = DockStyle.Fill,
                SelectionMode = DataGridViewSelectionMode.CellSelect
            };
            datagrid1.CellMouseDown += delegate(object sender, DataGridViewCellMouseEventArgs args)
            {
                if (args.RowIndex != -1 && args.ColumnIndex != -1)
                    if (args.Button == MouseButtons.Right)
                    {
                        DataGridViewCell clickedCell = (sender as DataGridView).Rows[args.RowIndex].Cells[args.ColumnIndex];
                        datagrid1.CurrentCell = clickedCell;
                        Point relativeMousePosition = datagrid1.PointToClient(Cursor.Position);
                        ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
                        contextMenuStrip.Items.Add("Delete", null, delegate
                        {
                            try
                            {
                                datagrid1.Rows.RemoveAt(args.RowIndex);
                            }
                            catch (Exception)
                            {
                            }
                        });
                        contextMenuStrip.Show(datagrid1, relativeMousePosition);
                    }
            };
            groupbox1.Width = pWidth;
            groupbox1.Height = 300;
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(groupbox1, GetDescriptionAttributeValue(p));
            groupbox1.Controls.Add(datagrid1);
            pa.Controls.Add(groupbox1);
            datagrid1.DataSource = p.GetValue(ds);

            // Below code for DataGridViewComboBoxColumns
            // Enum Types are automatically converted to DataGridViewComboBoxColumns, set [Browsable(false)] attribute!
            IEnumerable list = p.GetValue(ds) as IEnumerable;
            List<object> objList = new List<object>();
            foreach (object obj in list)
                objList.Add(obj);

            Type listType = GetListType(list);
            List<PropertyInfo> propInfosAdded = new List<PropertyInfo>();
            if (listType != null)
            {
                int n = 0;
                foreach (PropertyInfo propInfo in listType.GetProperties())
                {
                    if (propInfo.PropertyType.IsEnum && !IsBrowsableAttribute(propInfo))
                        //if (propInfo.PropertyType.IsEnum)
                    {
                        propInfosAdded.Add(propInfo);
                        DataGridViewComboBoxColumn cmb = new DataGridViewComboBoxColumn();
                        List<string> items = Enum.GetNames(propInfo.PropertyType).ToList();
                        cmb.DataSource = items;
                        cmb.Name = propInfo.Name;
                        datagrid1.Columns.Insert(n, cmb);
                    }
                    n++;
                }
            }

            datagrid1.CellValueChanged += delegate(object sender, DataGridViewCellEventArgs args)
            {
                DataGridView dgv = (DataGridView) sender;
                DataGridViewCell c = datagrid1.SelectedCells[0];
                int index = c.OwningRow.Index;
                if (propInfosAdded.Any(px => px.Name == c.OwningColumn.Name) && c.OwningColumn.GetType() == typeof(DataGridViewComboBoxColumn))
                {
                    PropertyInfo propInfo = propInfosAdded.FirstOrDefault(px => px.Name == c.OwningColumn.Name);
                    try
                    {
                        if (index < objList.Count)
                        {
                            DataGridViewComboBoxCell k = c as DataGridViewComboBoxCell;
                            object item = objList[index];
                            object enumValue = Enum.Parse(propInfo.PropertyType, k.Value.ToString());
                            item.GetType().GetProperty(propInfo.Name).SetValue(item, enumValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            };

            foreach (DataGridViewRow r in datagrid1.Rows) // load value
            foreach (DataGridViewCell c in r.Cells)
                if (propInfosAdded.Any(px => px.Name == c.OwningColumn.Name) && c.OwningColumn.GetType() == typeof(DataGridViewComboBoxColumn))
                {
                    PropertyInfo propInfo = propInfosAdded.FirstOrDefault(px => px.Name == c.OwningColumn.Name);
                    int index = c.OwningRow.Index;
                    try
                    {
                        if (index < objList.Count)
                        {
                            DataGridViewComboBoxCell k = c as DataGridViewComboBoxCell;
                            object item = objList[index];
                            object itemVal = item.GetType().GetProperty(propInfo.Name).GetValue(item);
                            k.Value = itemVal.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
        }

        public void EnumeratePropertiesAndAddControlsToPanel(Panel pa, object ds, int pWidth = CONTROL_WIDTH)
        {
            foreach (PropertyInfo property in ds.GetType().GetProperties())
            {
                if (!IsBrowsableAttribute(property))
                    continue;

                if (property.PropertyType == typeof(bool))
                    AddBoolProperty(pa, ds, property, pWidth);

                if (property.PropertyType == typeof(int))
                    AddIntOrStringProperty(pa, ds, property, delegate(object sender, ConvertEventArgs args)
                    {
                        int.TryParse((string) args.Value, out var result);
                        args.Value = result;
                    }, pWidth);

                if (property.PropertyType == typeof(string))
                    AddIntOrStringProperty(pa, ds, property, null, pWidth);

                if (IsBindingList(property.GetValue(ds)))
                    AddListProperty(pa, ds, property, pWidth);

                if (property.PropertyType.IsEnum)
                    AddEnumProperty(pa, ds, property, pWidth);
            }
        }

        public string GetDescriptionAttributeValue(PropertyInfo p)
        {
            object[] attributes = p.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length == 0 ? "No description." : ((DescriptionAttribute) attributes[0]).Description;
        }

        public string GetTabAttributeValue(PropertyInfo p)
        {
            object[] a = p.GetCustomAttributes(typeof(TabPageAttribute), false);
            object[] b = p.GetCustomAttributes(typeof(TabRootAttribute), false);
            return a.Length == 0 ? b.Length == 0 ? null : ((TabRootAttribute) b[0]).Name : ((TabPageAttribute) a[0]).Name;
        }

        public bool HasTabPageAttribute(PropertyInfo p)
        {
            object[] b = p.GetCustomAttributes(typeof(TabPageAttribute), false);
            return b.Length != 0;
        }

        public bool HasTabRootAttribute(PropertyInfo p)
        {
            object[] b = p.GetCustomAttributes(typeof(TabRootAttribute), false);
            return b.Length != 0;
        }

        public bool IsBindingList(object o)
        {
            return o is IBindingList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(BindingList<>));
        }

        public bool IsBrowsableAttribute(PropertyInfo p)
        {
            object[] attributes = p.GetCustomAttributes(typeof(BrowsableAttribute), false);
            return attributes.Length == 0 ? true : ((BrowsableAttribute) attributes[0]).Browsable;
        }

        public bool IsList(object o)
        {
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public void ReloaderQuestorGroupSnyc()
        {
            _questorSettingSyncList = new List<EveAccount>();
            if (!string.IsNullOrEmpty(_eA.ClientSetting.QuestorMainSetting.QuestorSettingGroup))
            {
                string group = _eA.ClientSetting.QuestorMainSetting.QuestorSettingGroup;
                foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.ToList().Where(e =>
                    e != _eA && e.ClientSetting != null && !string.IsNullOrEmpty(e.ClientSetting.QuestorMainSetting.QuestorSettingGroup) &&
                    e.ClientSetting.QuestorMainSetting.QuestorSettingGroup.Equals(group)))
                    _questorSettingSyncList.Add(eA);
            }

            Text = string.Format("ClientSetting [{0}]", _eA.CharacterName);
            if (_questorSettingSyncList.Count > 0)
                Text += $" Sync [{string.Join(",", _questorSettingSyncList.Select(e => e.CharacterName))}]";

            if (_questorSettingSyncList.Any(ev => ev.ClientSetting != null && ev != _eA))
                _eA.CS.QMS.QuestorSetting = _questorSettingSyncList.FirstOrDefault(ev => ev.ClientSetting != null && ev != _eA).CS.QMS.QS;

            foreach (EveAccount eA in _questorSettingSyncList.Where(ev => ev.ClientSetting != null))
                eA.CS.QMS.QuestorSetting = _eA.ClientSetting.QuestorMainSetting.QuestorSetting;

            foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.ToList().Where(e =>
                e != _eA && e.ClientSetting != null && !_questorSettingSyncList.Contains(e)
                && _eA.ClientSetting.QuestorMainSetting.QuestorSetting == e.ClientSetting.QuestorMainSetting.QuestorSetting))
                eA.ClientSetting = eA.ClientSetting.Clone();
        }

        public void SetupControls()
        {
            tabControl1.TabPages.Clear();
            ReloaderQuestorGroupSnyc();
            TraversePropertiesRecursive(new List<Tuple<PropertyInfo, int>>(), _eA.ClientSetting, -1, tabControl1);
        }

        public List<Tuple<PropertyInfo, int>> TraversePropertiesRecursive(List<Tuple<PropertyInfo, int>> list, object obj, int depth,
            TabControl parentTabControl)
        {
            depth++;
            foreach (PropertyInfo property in obj.GetType().GetProperties().Where(p => GetTabAttributeValue(p) != null))
            {
                if (!parentTabControl.TabPages.ContainsKey(GetTabAttributeValue(property)))
                {
                    TabPage page = new TabPage(GetTabAttributeValue(property));
                    parentTabControl.TabPages.Add(page);

                    Console.WriteLine($"Selecting tab (1) {page.Text}");
                    parentTabControl.SelectTab(page);
                }

                FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
                flowLayoutPanel.Dock = DockStyle.Fill;
                flowLayoutPanel.AutoScroll = true;

                list.Add(new Tuple<PropertyInfo, int>(property, depth));
                TabControl tabControl = parentTabControl;
                if (HasTabRootAttribute(property))
                {
                    TabControl tabc = new TabControl();
                    TabPage page = new TabPage("Main");
                    tabc.TabPages.Add(page);

                    tabc.Dock = DockStyle.Fill;
                    page.Controls.Add(flowLayoutPanel);
                    parentTabControl.TabPages[parentTabControl.TabPages.Count - 1].Controls.Add(tabc);
                    tabControl = tabc;
                }

                if (HasTabPageAttribute(property))
                    parentTabControl.TabPages[parentTabControl.TabPages.Count - 1].Controls.Add(flowLayoutPanel);

                TabPage flowPanelTabPageParent = flowLayoutPanel.Parent as TabPage;
                TabControl flowPanelTabControlParent = flowPanelTabPageParent.Parent as TabControl;

                flowPanelTabControlParent.SelectTab(flowPanelTabPageParent);
                Console.WriteLine($"Selecting tab (2) {flowPanelTabPageParent.Text}");

                EnumeratePropertiesAndAddControlsToPanel(flowLayoutPanel, property.GetValue(obj));
                TraversePropertiesRecursive(list, property.GetValue(obj), depth, tabControl);

                //var tabList = parentTabControl.TabPages.Cast<TabPage>().ToList();
                //tabList.Sort((x, y) => string.Compare(x.Text, y.Text));
                //parentTabControl.TabPages.Clear();
                //parentTabControl.TabPages.AddRange(tabList.ToArray());
            }

            return list;
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(CompressUtil.Compress(Util.XmlSerialize(_eA.ClientSetting)));
        }

        private Type GetListType(IEnumerable enumerable)
        {
            try
            {
                Type type = enumerable.GetType();
                Type enumerableType = type
                    .GetInterfaces()
                    .Where(x => x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .First();
                return enumerableType.GetGenericArguments()[0];
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                object result = Util.XmlDeserialize(CompressUtil.DecompressText(Clipboard.GetText()), _eA.ClientSetting.GetType());
                if (result is ClientSetting setting)
                {
                    _eA.ClientSetting = setting;
                    Cache.Instance.Log($"Settings imported.");
                    SetupControls();
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log($"Exception {ex}");
            }
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetupControls();
        }

        #endregion Methods
    **/
    }
}