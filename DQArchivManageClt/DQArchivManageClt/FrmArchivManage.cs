namespace DQArchivManageClt
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class FrmArchivManage : Form
    {
        private IContainer components = null;
        public static FrmArchivManage frmMian;
        private TabControl tbCtrlMain;
        private TabPage tpInfo;
        private TabPage tPPrint;
        private TabPage tPSent;
        private TabPage tPTs;
        private RichTextBox txtInfo;
        private UcPrint ucPrint = null;
        private UcSent ucSent = null;
        private UcTs ucts = null;

        public FrmArchivManage()
        {
            this.InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        public void DisplayTextInRichtBox(string txt, int InfoName, bool isShow)
        {
            if (isShow)
            {
                this.tbCtrlMain.SelectedTab = this.tpInfo;
            }
            switch (InfoName)
            {
                case 0:
                    this.txtInfo.SelectionColor = Color.Red;
                    this.txtInfo.SelectionFont = new Font("宋体", 9f, FontStyle.Bold);
                    break;

                case 1:
                    this.txtInfo.SelectionColor = Color.Black;
                    this.txtInfo.SelectionFont = new Font("宋体", 9f, FontStyle.Bold);
                    break;

                case 2:
                    this.txtInfo.SelectionColor = Color.Blue;
                    this.txtInfo.SelectionFont = new Font("宋体", 9f, FontStyle.Bold);
                    break;
            }
            this.txtInfo.AppendText("\r\n" + txt);
            this.txtInfo.Focus();
            this.txtInfo.Select(this.txtInfo.Text.Length, 0);
            if (this.txtInfo.SelectionFont != null)
            {
                this.txtInfo.SelectionFont.Dispose();
            }
            this.txtInfo.Refresh();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void FrmArchivManage_Load(object sender, EventArgs e)
        {
            this.tbCtrlMain.SelectedIndex = 0;
            if ((this.tbCtrlMain.TabPages.Count == 1) && (this.tbCtrlMain.SelectedTab == this.tpInfo))
            {
                this.DisplayTextInRichtBox("没有任何档案管理权限，请与管理员联系，确认自己是可以进行档案管理的有关角色成员", 2, true);
            }
        }

        public void InitFrm(int cants, int canprint, int cansent)
        {
            this.tbCtrlMain.TabPages.Clear();
            if (cants == 1)
            {
                this.tbCtrlMain.TabPages.Add(this.tPTs);
                this.ucts = new UcTs();
                this.ucts.Dock = DockStyle.Fill;
                this.tPTs.Controls.Add(this.ucts);
            }
            if (canprint == 1)
            {
                this.tbCtrlMain.TabPages.Add(this.tPPrint);
                this.ucPrint = new UcPrint();
                this.ucPrint.Dock = DockStyle.Fill;
                this.tPPrint.Controls.Add(this.ucPrint);
            }
            if (cansent == 1)
            {
                this.tbCtrlMain.TabPages.Add(this.tPSent);
                this.ucSent = new UcSent();
                this.ucSent.Dock = DockStyle.Fill;
                this.tPSent.Controls.Add(this.ucSent);
            }
            this.tbCtrlMain.TabPages.Add(this.tpInfo);
        }

        private void InitializeComponent()
        {
            this.tbCtrlMain = new TabControl();
            this.tPTs = new TabPage();
            this.tPPrint = new TabPage();
            this.tPSent = new TabPage();
            this.tpInfo = new TabPage();
            this.txtInfo = new RichTextBox();
            this.tbCtrlMain.SuspendLayout();
            this.tpInfo.SuspendLayout();
            base.SuspendLayout();
            this.tbCtrlMain.Appearance = TabAppearance.FlatButtons;
            this.tbCtrlMain.Controls.Add(this.tPTs);
            this.tbCtrlMain.Controls.Add(this.tPPrint);
            this.tbCtrlMain.Controls.Add(this.tPSent);
            this.tbCtrlMain.Controls.Add(this.tpInfo);
            this.tbCtrlMain.Dock = DockStyle.Fill;
            this.tbCtrlMain.Location = new Point(0, 0);
            this.tbCtrlMain.Name = "tbCtrlMain";
            this.tbCtrlMain.SelectedIndex = 0;
            this.tbCtrlMain.Size = new Size(910, 0x1e2);
            this.tbCtrlMain.TabIndex = 1;
            this.tPTs.Location = new Point(4, 0x19);
            this.tPTs.Name = "tPTs";
            this.tPTs.Padding = new Padding(3);
            this.tPTs.Size = new Size(0x386, 0x1c5);
            this.tPTs.TabIndex = 0;
            this.tPTs.Text = "托晒单管理";
            this.tPTs.UseVisualStyleBackColor = true;
            this.tPPrint.Location = new Point(4, 0x19);
            this.tPPrint.Name = "tPPrint";
            this.tPPrint.Padding = new Padding(3);
            this.tPPrint.Size = new Size(0x386, 0x1c5);
            this.tPPrint.TabIndex = 1;
            this.tPPrint.Text = "打印管理";
            this.tPPrint.UseVisualStyleBackColor = true;
            this.tPSent.Location = new Point(4, 0x19);
            this.tPSent.Name = "tPSent";
            this.tPSent.Size = new Size(0x386, 0x1c5);
            this.tPSent.TabIndex = 2;
            this.tPSent.Text = "收发管理";
            this.tPSent.UseVisualStyleBackColor = true;
            this.tpInfo.Controls.Add(this.txtInfo);
            this.tpInfo.Location = new Point(4, 0x19);
            this.tpInfo.Name = "tpInfo";
            this.tpInfo.Size = new Size(0x386, 0x1c5);
            this.tpInfo.TabIndex = 3;
            this.tpInfo.Text = "详细信息";
            this.tpInfo.UseVisualStyleBackColor = true;
            this.txtInfo.Dock = DockStyle.Fill;
            this.txtInfo.Location = new Point(0, 0);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new Size(0x386, 0x1c5);
            this.txtInfo.TabIndex = 0;
            this.txtInfo.Text = "";
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            base.ClientSize = new Size(910, 0x1e2);
            base.Controls.Add(this.tbCtrlMain);
            base.Name = "FrmArchivManage";
            this.Text = "东汽档案管理系统";
            base.Load += new EventHandler(this.FrmArchivManage_Load);
            this.tbCtrlMain.ResumeLayout(false);
            this.tpInfo.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        public void ShowRs()
        {
            this.tbCtrlMain.SelectedTab = this.tpInfo;
        }
    }
}

