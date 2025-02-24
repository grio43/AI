namespace HookManager
{
    partial class PacketViewerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.outgoingPacketsListView = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.incomingPacketsListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.outgoingPacketsListView, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.incomingPacketsListView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.richTextBox1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.richTextBox2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1342, 468);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // outgoingPacketsListView
            // 
            this.outgoingPacketsListView.AutoArrange = false;
            this.outgoingPacketsListView.BackColor = System.Drawing.Color.DarkGray;
            this.outgoingPacketsListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.outgoingPacketsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.outgoingPacketsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outgoingPacketsListView.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outgoingPacketsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.outgoingPacketsListView.HideSelection = false;
            this.outgoingPacketsListView.Location = new System.Drawing.Point(674, 3);
            this.outgoingPacketsListView.Name = "outgoingPacketsListView";
            this.outgoingPacketsListView.Size = new System.Drawing.Size(665, 134);
            this.outgoingPacketsListView.TabIndex = 143;
            this.outgoingPacketsListView.UseCompatibleStateImageBehavior = false;
            this.outgoingPacketsListView.View = System.Windows.Forms.View.Details;
            this.outgoingPacketsListView.SelectedIndexChanged += new System.EventHandler(this.outgoingPacketsListView_SelectedIndexChanged);
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Logbox";
            this.columnHeader2.Width = 1526;
            // 
            // incomingPacketsListView
            // 
            this.incomingPacketsListView.AutoArrange = false;
            this.incomingPacketsListView.BackColor = System.Drawing.Color.DarkGray;
            this.incomingPacketsListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.incomingPacketsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.incomingPacketsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.incomingPacketsListView.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.incomingPacketsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.incomingPacketsListView.HideSelection = false;
            this.incomingPacketsListView.Location = new System.Drawing.Point(3, 3);
            this.incomingPacketsListView.Name = "incomingPacketsListView";
            this.incomingPacketsListView.Size = new System.Drawing.Size(665, 134);
            this.incomingPacketsListView.TabIndex = 142;
            this.incomingPacketsListView.UseCompatibleStateImageBehavior = false;
            this.incomingPacketsListView.View = System.Windows.Forms.View.Details;
            this.incomingPacketsListView.SelectedIndexChanged += new System.EventHandler(this.incomingPacketsListView_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Logbox";
            this.columnHeader1.Width = 1526;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(674, 143);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(665, 322);
            this.richTextBox1.TabIndex = 144;
            this.richTextBox1.Text = "";
            // 
            // richTextBox2
            // 
            this.richTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox2.Location = new System.Drawing.Point(3, 143);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.Size = new System.Drawing.Size(665, 322);
            this.richTextBox2.TabIndex = 145;
            this.richTextBox2.Text = "";
            // 
            // PacketViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1342, 468);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PacketViewerForm";
            this.Text = "PacketViewerForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PacketViewerForm_FormClosing);
            this.Load += new System.EventHandler(this.PacketViewerForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListView outgoingPacketsListView;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ListView incomingPacketsListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.RichTextBox richTextBox2;
    }
}