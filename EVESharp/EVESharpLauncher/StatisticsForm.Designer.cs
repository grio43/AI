/*
 * Created by SharpDevelop.
 * User: duketwo
 * Date: 07.05.2016
 * Time: 16:17
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace EVESharpLauncher
{
	partial class StatisticsForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		public OxyPlot.WindowsForms.PlotView plot1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem statisticsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem selectCharacterToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem iskPerHourMissionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem tOTALISKChartTypeColumnSeriesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem eARNINGSDAILYChartTypeColumnSeriesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem timeDomainToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem sTANDINGSDAILYChartTypeColumnSeriesToolStripMenuItem;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatisticsForm));
            this.plot1 = new OxyPlot.WindowsForms.PlotView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.statisticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iskPerHourMissionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tOTALISKChartTypeColumnSeriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eARNINGSHOURLYChartTypeColumnSeriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eARNINGSDAILYChartTypeColumnSeriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eARNINGSMONTHLYChartTypeColumnSeriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eARNINGSYEARLYChartTypeColumnSeriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sTANDINGSDAILYChartTypeColumnSeriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tOTALISKBYCHARNAMEChartTypeColumnSeriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectCharacterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timeDomainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonReload = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // plot1
            // 
            this.plot1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plot1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.plot1.Location = new System.Drawing.Point(0, 24);
            this.plot1.Name = "plot1";
            this.plot1.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plot1.Size = new System.Drawing.Size(1268, 576);
            this.plot1.TabIndex = 0;
            this.plot1.Text = "plot1";
            this.plot1.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plot1.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plot1.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statisticsToolStripMenuItem,
            this.selectCharacterToolStripMenuItem,
            this.timeDomainToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1268, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // statisticsToolStripMenuItem
            // 
            this.statisticsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.iskPerHourMissionsToolStripMenuItem,
            this.tOTALISKChartTypeColumnSeriesToolStripMenuItem,
            this.eARNINGSHOURLYChartTypeColumnSeriesToolStripMenuItem,
            this.eARNINGSDAILYChartTypeColumnSeriesToolStripMenuItem,
            this.eARNINGSMONTHLYChartTypeColumnSeriesToolStripMenuItem,
            this.eARNINGSYEARLYChartTypeColumnSeriesToolStripMenuItem,
            this.sTANDINGSDAILYChartTypeColumnSeriesToolStripMenuItem,
            this.tOTALISKBYCHARNAMEChartTypeColumnSeriesToolStripMenuItem});
            this.statisticsToolStripMenuItem.Name = "statisticsToolStripMenuItem";
            this.statisticsToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.statisticsToolStripMenuItem.Text = "Statistics";
            this.statisticsToolStripMenuItem.DropDownOpening += new System.EventHandler(this.StatisticsToolStripMenuItemDropDownOpening);
            // 
            // iskPerHourMissionsToolStripMenuItem
            // 
            this.iskPerHourMissionsToolStripMenuItem.Name = "iskPerHourMissionsToolStripMenuItem";
            this.iskPerHourMissionsToolStripMenuItem.Size = new System.Drawing.Size(384, 22);
            this.iskPerHourMissionsToolStripMenuItem.Text = "MISSIONS_ISK_PER_HOUR - ChartType [BarSeries]";
            this.iskPerHourMissionsToolStripMenuItem.Click += new System.EventHandler(this.IskPerHourMissionsToolStripMenuItemClick);
            // 
            // tOTALISKChartTypeColumnSeriesToolStripMenuItem
            // 
            this.tOTALISKChartTypeColumnSeriesToolStripMenuItem.Name = "tOTALISKChartTypeColumnSeriesToolStripMenuItem";
            this.tOTALISKChartTypeColumnSeriesToolStripMenuItem.Size = new System.Drawing.Size(384, 22);
            this.tOTALISKChartTypeColumnSeriesToolStripMenuItem.Text = "TOTAL_ISK_DAILY - ChartType [ColumnSeries]";
            this.tOTALISKChartTypeColumnSeriesToolStripMenuItem.Click += new System.EventHandler(this.TOTALISKChartTypeColumnSeriesToolStripMenuItemClick);
            // 
            // eARNINGSHOURLYChartTypeColumnSeriesToolStripMenuItem
            // 
            this.eARNINGSHOURLYChartTypeColumnSeriesToolStripMenuItem.Name = "eARNINGSHOURLYChartTypeColumnSeriesToolStripMenuItem";
            this.eARNINGSHOURLYChartTypeColumnSeriesToolStripMenuItem.Size = new System.Drawing.Size(384, 22);
            this.eARNINGSHOURLYChartTypeColumnSeriesToolStripMenuItem.Text = "EARNINGS_HOURLY - ChartType [ColumnSeries]";
            this.eARNINGSHOURLYChartTypeColumnSeriesToolStripMenuItem.Click += new System.EventHandler(this.EARNINGSHOURLYChartTypeColumnSeriesToolStripMenuItem_Click);
            // 
            // eARNINGSDAILYChartTypeColumnSeriesToolStripMenuItem
            // 
            this.eARNINGSDAILYChartTypeColumnSeriesToolStripMenuItem.Name = "eARNINGSDAILYChartTypeColumnSeriesToolStripMenuItem";
            this.eARNINGSDAILYChartTypeColumnSeriesToolStripMenuItem.Size = new System.Drawing.Size(384, 22);
            this.eARNINGSDAILYChartTypeColumnSeriesToolStripMenuItem.Text = "EARNINGS_DAILY - ChartType [ColumnSeries]";
            this.eARNINGSDAILYChartTypeColumnSeriesToolStripMenuItem.Click += new System.EventHandler(this.EARNINGSDAILYChartTypeColumnSeriesToolStripMenuItemClick);
            // 
            // eARNINGSMONTHLYChartTypeColumnSeriesToolStripMenuItem
            // 
            this.eARNINGSMONTHLYChartTypeColumnSeriesToolStripMenuItem.Name = "eARNINGSMONTHLYChartTypeColumnSeriesToolStripMenuItem";
            this.eARNINGSMONTHLYChartTypeColumnSeriesToolStripMenuItem.Size = new System.Drawing.Size(384, 22);
            this.eARNINGSMONTHLYChartTypeColumnSeriesToolStripMenuItem.Text = "EARNINGS_MONTHLY - ChartType [ColumnSeries]";
            this.eARNINGSMONTHLYChartTypeColumnSeriesToolStripMenuItem.Click += new System.EventHandler(this.EARNINGSMONTHLYChartTypeColumnSeriesToolStripMenuItem_Click);
            // 
            // eARNINGSYEARLYChartTypeColumnSeriesToolStripMenuItem
            // 
            this.eARNINGSYEARLYChartTypeColumnSeriesToolStripMenuItem.Name = "eARNINGSYEARLYChartTypeColumnSeriesToolStripMenuItem";
            this.eARNINGSYEARLYChartTypeColumnSeriesToolStripMenuItem.Size = new System.Drawing.Size(384, 22);
            this.eARNINGSYEARLYChartTypeColumnSeriesToolStripMenuItem.Text = "EARNINGS_YEARLY - ChartType [ColumnSeries]";
            this.eARNINGSYEARLYChartTypeColumnSeriesToolStripMenuItem.Click += new System.EventHandler(this.EARNINGSYEARLYChartTypeColumnSeriesToolStripMenuItem_Click);
            // 
            // sTANDINGSDAILYChartTypeColumnSeriesToolStripMenuItem
            // 
            this.sTANDINGSDAILYChartTypeColumnSeriesToolStripMenuItem.Name = "sTANDINGSDAILYChartTypeColumnSeriesToolStripMenuItem";
            this.sTANDINGSDAILYChartTypeColumnSeriesToolStripMenuItem.Size = new System.Drawing.Size(384, 22);
            this.sTANDINGSDAILYChartTypeColumnSeriesToolStripMenuItem.Text = "STANDINGS_DAILY - ChartType [ColumnSeries]";
            this.sTANDINGSDAILYChartTypeColumnSeriesToolStripMenuItem.Click += new System.EventHandler(this.STANDINGSDAILYChartTypeColumnSeriesToolStripMenuItemClick);
            // 
            // tOTALISKBYCHARNAMEChartTypeColumnSeriesToolStripMenuItem
            // 
            this.tOTALISKBYCHARNAMEChartTypeColumnSeriesToolStripMenuItem.Name = "tOTALISKBYCHARNAMEChartTypeColumnSeriesToolStripMenuItem";
            this.tOTALISKBYCHARNAMEChartTypeColumnSeriesToolStripMenuItem.Size = new System.Drawing.Size(384, 22);
            this.tOTALISKBYCHARNAMEChartTypeColumnSeriesToolStripMenuItem.Text = "TOTAL_ISK_BY_CHARNAME - ChartType [ColumnSeries]";
            this.tOTALISKBYCHARNAMEChartTypeColumnSeriesToolStripMenuItem.Click += new System.EventHandler(this.tOTALISKBYCHARNAMEChartTypeColumnSeriesToolStripMenuItem_Click);
            // 
            // selectCharacterToolStripMenuItem
            // 
            this.selectCharacterToolStripMenuItem.Name = "selectCharacterToolStripMenuItem";
            this.selectCharacterToolStripMenuItem.Size = new System.Drawing.Size(109, 20);
            this.selectCharacterToolStripMenuItem.Text = "Select character";
            this.selectCharacterToolStripMenuItem.DropDownOpening += new System.EventHandler(this.SelectCharacterToolStripMenuItemDropDownOpening);
            // 
            // timeDomainToolStripMenuItem
            // 
            this.timeDomainToolStripMenuItem.Name = "timeDomainToolStripMenuItem";
            this.timeDomainToolStripMenuItem.Size = new System.Drawing.Size(90, 20);
            this.timeDomainToolStripMenuItem.Text = "Time domain";
            this.timeDomainToolStripMenuItem.DropDownOpening += new System.EventHandler(this.TimeDomainToolStripMenuItemDropDownOpening);
            // 
            // buttonReload
            // 
            this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReload.FlatAppearance.BorderSize = 0;
            this.buttonReload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReload.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonReload.Location = new System.Drawing.Point(1247, 0);
            this.buttonReload.Margin = new System.Windows.Forms.Padding(0);
            this.buttonReload.Name = "buttonReload";
            this.buttonReload.Size = new System.Drawing.Size(21, 24);
            this.buttonReload.TabIndex = 3;
            this.buttonReload.Text = "↻";
            this.buttonReload.UseVisualStyleBackColor = true;
            this.buttonReload.Click += new System.EventHandler(this.ButtonReload_Click);
            // 
            // StatisticsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1268, 600);
            this.Controls.Add(this.buttonReload);
            this.Controls.Add(this.plot1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "StatisticsForm";
            this.Text = "StatisticsForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.StatisticsForm_FormClosed);
            this.Load += new System.EventHandler(this.StatisticsFormLoad);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}


	    private System.Windows.Forms.Button buttonReload;
        private System.Windows.Forms.ToolStripMenuItem eARNINGSMONTHLYChartTypeColumnSeriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eARNINGSYEARLYChartTypeColumnSeriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eARNINGSHOURLYChartTypeColumnSeriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tOTALISKBYCHARNAMEChartTypeColumnSeriesToolStripMenuItem;
    }
}
