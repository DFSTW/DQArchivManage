namespace DQArchivManageClt
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    public class FrmBar : Form
    {
        private BackgroundWorker _bkwMain;
        private IContainer components = null;
        private bool isPower = false;
        public ProgressBar progressBar1;
        private readonly System.Windows.Forms.Timer timer1;

        public FrmBar(BackgroundWorker bkwMain)
        {
            this.InitializeComponent();
            this._bkwMain = bkwMain;
            this.progressBar1 = new ProgressBar();
            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = 100;
            this.progressBar1.Step = 1;
            this.progressBar1.Dock = DockStyle.Fill;
            base.Controls.Add(this.progressBar1);
            this.timer1 = new System.Windows.Forms.Timer();
            this.timer1.Tick += new EventHandler(this.timer1_Tick);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void FrmBar_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = this._bkwMain.IsBusy && !this.isPower;
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            base.ClientSize = new Size(0x1ab, 0x29);
            base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "FrmBar";
            base.StartPosition = FormStartPosition.CenterScreen;
            base.FormClosing += new FormClosingEventHandler(this.FrmBar_FormClosing);
            base.ResumeLayout(false);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.progressBar1.Value == this.progressBar1.Maximum)
            {
                this.progressBar1.Value = 0;
            }
            this.progressBar1.Increment(1);
        }

        public void ToAStart()
        {
            base.Activate();
            base.Show();
            this.timer1.Start();
        }

        public void ToClose()
        {
            this.isPower = true;
            base.Close();
        }

        public void ToClose(bool isEnd)
        {
            this.timer1.Stop();
            this.progressBar1.Value = isEnd ? this.progressBar1.Maximum : 0;
            base.Activate();
            Thread.Sleep(0x3e8);
            base.Close();
        }
    }
}

