namespace Server.Forms
{
    partial class FormDownloadFile
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDownloadFile));
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelsize = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelfile = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(8, 58);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Downlad:";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // labelsize
            // 
            this.labelsize.AutoSize = true;
            this.labelsize.BackColor = System.Drawing.Color.Black;
            this.labelsize.ForeColor = System.Drawing.Color.White;
            this.labelsize.Location = new System.Drawing.Point(69, 58);
            this.labelsize.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelsize.Name = "labelsize";
            this.labelsize.Size = new System.Drawing.Size(13, 13);
            this.labelsize.TabIndex = 0;
            this.labelsize.Text = "..";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Black;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(8, 25);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "File:";
            // 
            // labelfile
            // 
            this.labelfile.AutoSize = true;
            this.labelfile.BackColor = System.Drawing.Color.Black;
            this.labelfile.ForeColor = System.Drawing.Color.White;
            this.labelfile.Location = new System.Drawing.Point(69, 25);
            this.labelfile.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelfile.Name = "labelfile";
            this.labelfile.Size = new System.Drawing.Size(13, 13);
            this.labelfile.TabIndex = 0;
            this.labelfile.Text = "..";
            // 
            // FormDownloadFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(331, 99);
            this.Controls.Add(this.labelfile);
            this.Controls.Add(this.labelsize);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormDownloadFile";
            this.Text = "Download";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SocketDownload_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.Timer timer1;
        public System.Windows.Forms.Label labelsize;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label labelfile;
        public System.Windows.Forms.Label label1;
    }
}