using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using EVESharpCore.Cache;
using EVESharpCore.Controllers.Abyssal;
using EVESharpCore.Controllers.ActionQueue.Actions.Base;
using EVESharpCore.Framework;

namespace EVESharpCore.Controllers
{
    public partial class AbyssalControllerForm : Form
    {
        #region Fields

        private AbyssalBaseController _controller;

        #endregion Fields

        #region Constructors

        public AbyssalControllerForm(AbyssalBaseController c)
        {
            this._controller = c;
            InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        public DataGridView GetDataGridView1 => this.dataGridView1;

        public Label IskPerHLabel => this.label2;

        public Label StageLabel => this.label3;

        public Label StageRemainingSeconds => this.label5;

        public Label EstimatedNpcKillTime => this.label7;

        public Label AbyssTotalTime => this.label13;

        public Label WreckLootTime => this.label15;

        public Label TimeNeededToGetToTheGate => this.label17;

        public Label TotalStageEhp => this.label11;

        public Label IgnoreAbyssEntities => this.label10;

        #endregion Methods

        private void label6_Click(object sender, System.EventArgs e)
        {

        }

        private void label10_Click(object sender, System.EventArgs e)
        {

        }

        private void label9_Click(object sender, System.EventArgs e)
        {

        }

        private void label12_Click(object sender, System.EventArgs e)
        {

        }

        private void label11_Click(object sender, System.EventArgs e)
        {

        }

        private void label5_Click(object sender, System.EventArgs e)
        {

        }

        private void button1_Click(object sender, System.EventArgs e)
        {

            ActionQueueAction actionQueueAction = null;
            actionQueueAction = new ActionQueueAction(new Action(() =>
            {
                try
                {
                    var droneBayItems = ESCache.Instance.DirectEve.GetShipsDroneBay()?.Items;
                    var inspaceDrones = ESCache.Instance.DirectEve.ActiveDrones;

                    if (!droneBayItems.Any())
                        return;

                    var droneTypeIdsToPutAlwaysOnLowLife = new List<int>() { 33681 };

                    foreach (var typeId in droneTypeIdsToPutAlwaysOnLowLife)
                    {
                        foreach (var item in droneBayItems.Where(e => e.TypeId == typeId))
                        {
                            DirectEve._entityHealthPercOverrides[item.ItemId] = (0.1d, 0.1d, 0.1d);
                        }

                        foreach (var item in inspaceDrones.Where(e => e.TypeId == typeId))
                        {
                            DirectEve._entityHealthPercOverrides[item.Id] = (0.1d, 0.1d, 0.1d);
                        }
                    }

                    // actionQueueAction.QueueAction();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            ));

            actionQueueAction.Initialize().QueueAction();
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            DirectEve._entityHealthPercOverrides = new System.Collections.Generic.Dictionary<long, (double, double, double)>();
        }
    }
}