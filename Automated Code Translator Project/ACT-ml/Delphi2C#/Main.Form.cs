
namespace MyHorseApp
{
    partial class frmMain
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox txtKey;
        private System.Windows.Forms.Label lblKey;
        private System.Windows.Forms.TextBox txtCrt;
        private System.Windows.Forms.Label lblCrt;
        private System.Windows.Forms.Button btnSelectKey;
        private System.Windows.Forms.Button btnSelectCrt;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tslStatus;

       
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.btnStart = new System.Windows.Forms.Button();
            this.txtKey = new System.Windows.Forms.TextBox();
            this.lblKey = new System.Windows.Forms.Label();
            this.txtCrt = new System.Windows.Forms.TextBox();
            this.lblCrt = new System.Windows.Forms.Label();
            this.btnSelectKey = new System.Windows.Forms.Button();
            this.btnSelectCrt = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            
            this.numPort.Location = new System.Drawing.Point(12, 12);
            this.numPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(120, 20);
            this.numPort.TabIndex = 0;
            this.numPort.Value = new decimal(new int[] {
            8443,
            0,
            0,
            0});
            
            this.btnStart.Location = new System.Drawing.Point(12, 38);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(120, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start Horse Server";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            
            this.txtKey.Location = new System.Drawing.Point(12, 80);
            this.txtKey.Name = "txtKey";
            this.txtKey.Size = new System.Drawing.Size(300, 20);
            this.txtKey.TabIndex = 2;
            
            this.lblKey.AutoSize = true;
            this.lblKey.Location = new System.Drawing.Point(12, 64);
            this.lblKey.Name = "lblKey";
            this.lblKey.Size = new System.Drawing.Size(49, 13);
            this.lblKey.TabIndex = 3;
            this.lblKey.Text = "Key File:";
            
            this.txtCrt.Location = new System.Drawing.Point(12, 120);
            this.txtCrt.Name = "txtCrt";
            this.txtCrt.Size = new System.Drawing.Size(300, 20);
            this.txtCrt.TabIndex = 4;
            
            this.lblCrt.AutoSize = true;
            this.lblCrt.Location = new System.Drawing.Point(12, 104);
            this.lblCrt.Name = "lblCrt";
            this.lblCrt.Size = new System.Drawing.Size(62, 13);
            this.lblCrt.TabIndex = 5;
            this.lblCrt.Text = "Certificate:";
            
            this.btnSelectKey.Location = new System.Drawing.Point(318, 78);
            this.btnSelectKey.Name = "btnSelectKey";
            this.btnSelectKey.Size = new System.Drawing.Size(75, 23);
            this.btnSelectKey.TabIndex = 6;
            this.btnSelectKey.Text = "Browse...";
            this.btnSelectKey.UseVisualStyleBackColor = true;
            this.btnSelectKey.Click += new System.EventHandler(this.btnSelectKey_Click);
            
            this.btnSelectCrt.Location = new System.Drawing.Point(318, 118);
            this.btnSelectCrt.Name = "btnSelectCrt";
            this.btnSelectCrt.Size = new System.Drawing.Size(75, 23);
            this.btnSelectCrt.TabIndex = 7;
            this.btnSelectCrt.Text = "Browse...";
            this.btnSelectCrt.UseVisualStyleBackColor = true;
            this.btnSelectCrt.Click += new System.EventHandler(this.btnSelectCrt_Click);
            
            this.txtPassword.Location = new System.Drawing.Point(12, 160);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(300, 20);
            this.txtPassword.TabIndex = 8;
            
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(12, 144);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.TabIndex = 9;
            this.lblPassword.Text = "Password:";
            
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 240);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(420, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
             
            this.tslStatus.Name = "tslStatus";
            this.tslStatus.Size = new System.Drawing.Size(118, 17);
            this.tslStatus.Text = "Status: Not running...";
            
            this.ClientSize = new System.Drawing.Size(420, 262);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.btnSelectCrt);
            this.Controls.Add(this.btnSelectKey);
            this.Controls.Add(this.lblCrt);
            this.Controls.Add(this.txtCrt);
            this.Controls.Add(this.lblKey);
            this.Controls.Add(this.txtKey);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.numPort);
            this.Name = "frmMain";
            this.Text = "Horse SSL Server Demo";
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}
