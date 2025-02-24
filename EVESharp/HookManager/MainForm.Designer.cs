/*
 * ---------------------------------------
 * User: duketwo
 * Date: 29.12.2013
 * Time: 21:20
 *
 * ---------------------------------------
 */
namespace HookManager
{
	partial class MainForm
	{



		private System.ComponentModel.IContainer components = null;





		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}






		private void InitializeComponent()
		{
            this.button1 = new System.Windows.Forms.Button();
            this.listViewLogs = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // button1
            //
            this.button1.BackColor = System.Drawing.Color.White;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(29, 349);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(173, 20);
            this.button1.TabIndex = 1;
            this.button1.Text = "Start/Restart";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.Button1Click);
            //
            // listViewLogs
            //
            this.listViewLogs.AutoArrange = false;
            this.listViewLogs.BackColor = System.Drawing.Color.Blue;
            this.listViewLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewLogs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listViewLogs.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewLogs.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewLogs.HideSelection = false;
            this.listViewLogs.Location = new System.Drawing.Point(28, 3);
            this.listViewLogs.Name = "listViewLogs";
            this.listViewLogs.Size = new System.Drawing.Size(770, 341);
            this.listViewLogs.TabIndex = 139;
            this.listViewLogs.UseCompatibleStateImageBehavior = false;
            this.listViewLogs.View = System.Windows.Forms.View.Details;
            //
            // columnHeader1
            //
            this.columnHeader1.Text = "Logbox";
            this.columnHeader1.Width = 1526;
            //
            // button3
            //
            this.button3.BackColor = System.Drawing.Color.White;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(210, 349);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(187, 20);
            this.button3.TabIndex = 140;
            this.button3.Text = "Compile/Start/Restart";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.Button3Click);
            //
            // button4
            //
            this.button4.BackColor = System.Drawing.Color.White;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.Location = new System.Drawing.Point(5, 135);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(16, 109);
            this.button4.TabIndex = 141;
            this.button4.Text = "+";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.Button4Click);
            //
            // button5
            //
            this.button5.BackColor = System.Drawing.Color.White;
            this.button5.Location = new System.Drawing.Point(416, 349);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(120, 20);
            this.button5.TabIndex = 9;
            this.button5.Text = "test";
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Visible = false;
            //this.button5.Click += new System.EventHandler(this.Button5Click);
            // 
            // button2
            //
            this.button2.BackColor = System.Drawing.Color.White;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(771, 349);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(35, 19);
            this.button2.TabIndex = 142;
            this.button2.Text = "py";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.Button2Click);
            //
            // button6
            // 
            this.button6.BackColor = System.Drawing.Color.White;
            this.button6.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button6.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button6.Location = new System.Drawing.Point(666, 349);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(99, 19);
            this.button6.TabIndex = 143;
            this.button6.Text = "packet viewer";
            this.button6.UseVisualStyleBackColor = false;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Blue;
            this.ClientSize = new System.Drawing.Size(819, 376);
            this.ControlBox = false;
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.listViewLogs);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(-15000, -15000);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "HookManager";
            this.TransparencyKey = System.Drawing.Color.Blue;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.Shown += new System.EventHandler(this.MainFormShown);
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.ColumnHeader columnHeader1;
	    private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ListView listViewLogs;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button6;
    }
}
