//using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using ServiceStack.OrmLite;
//using ServiceStack.OrmLite.Dapper;
using SharedComponents.EVE;
using SharedComponents.EVE.DatabaseSchemas;
//using SharedComponents.SQLite;
using SharedComponents.Utility;

namespace EVESharpLauncher
{
    public partial class DBViewer : Form
    {
        public DBViewer()
        {
            InitializeComponent();
        }

        private void DBViewer_Shown(object sender, EventArgs e)
        {
            //FormUtil.SetDoubleBuffered(this.dataGridView1);

            //DataTable dt;
            //using (var rc = ReadConn.Open())
            //{
            //    dt = Util.ConvertToDataTable(rc.DB.Select<StatisticsEntry>());

            //}
            //dataGridView1.DataSource = dt;
        }

        private void abyssStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //FormUtil.SetDoubleBuffered(this.dataGridView1);
            //DataTable dt;
            //using (var rc = ReadConn.Open())
            //{
            //    dt = Util.ConvertToDataTable(rc.DB.Select<AbyssStatEntry>());
            //}
            //dataGridView1.DataSource = dt;
        }

        private void missionStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //FormUtil.SetDoubleBuffered(this.dataGridView1);
            //
            //DataTable dt;
            //using (var rc = ReadConn.Open())
            //{
            //    dt = Util.ConvertToDataTable(rc.DB.Select<StatisticsEntry>());
            //}
            //dataGridView1.DataSource = dt;
        }
    }
}
