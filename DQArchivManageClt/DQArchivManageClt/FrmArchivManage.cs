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
        private TabPage tbSta;
        private UcTs ucts = null;
        private UCSta ucsta = null;

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
                this.ucSent.IsSUIJI = false;
                this.ucSent.Dock = DockStyle.Fill;
                this.tPSent.Controls.Add(this.ucSent);
            }
            if (cansent == 2)
            {
                this.tbCtrlMain.TabPages.Add(this.tPSent);
                this.ucSent = new UcSent();
                this.ucSent.IsSUIJI = true;
                this.ucSent.Dock = DockStyle.Fill;
                this.tPSent.Controls.Add(this.ucSent);
            }
            this.ucsta = new UCSta();
            this.ucsta.Dock = DockStyle.Fill;
            this.tbSta.Controls.Add(this.ucsta);

            this.tbCtrlMain.TabPages.Add(this.tpInfo);
            this.tbCtrlMain.TabPages.Add(this.tbSta);
        }

        private void InitializeComponent()
        {
            this.tbCtrlMain = new System.Windows.Forms.TabControl();
            this.tPTs = new System.Windows.Forms.TabPage();
            this.tPPrint = new System.Windows.Forms.TabPage();
            this.tPSent = new System.Windows.Forms.TabPage();
            this.tpInfo = new System.Windows.Forms.TabPage();
            this.txtInfo = new System.Windows.Forms.RichTextBox();
            this.tbSta = new System.Windows.Forms.TabPage();
            this.tbCtrlMain.SuspendLayout();
            this.tpInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbCtrlMain
            // 
            this.tbCtrlMain.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tbCtrlMain.Controls.Add(this.tPTs);
            this.tbCtrlMain.Controls.Add(this.tPPrint);
            this.tbCtrlMain.Controls.Add(this.tPSent);
            this.tbCtrlMain.Controls.Add(this.tpInfo);
            this.tbCtrlMain.Controls.Add(this.tbSta);
            this.tbCtrlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbCtrlMain.Location = new System.Drawing.Point(0, 0);
            this.tbCtrlMain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbCtrlMain.Name = "tbCtrlMain";
            this.tbCtrlMain.SelectedIndex = 0;
            this.tbCtrlMain.Size = new System.Drawing.Size(1213, 602);
            this.tbCtrlMain.TabIndex = 1;
            // 
            // tPTs
            // 
            this.tPTs.Location = new System.Drawing.Point(4, 28);
            this.tPTs.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tPTs.Name = "tPTs";
            this.tPTs.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tPTs.Size = new System.Drawing.Size(1205, 570);
            this.tPTs.TabIndex = 0;
            this.tPTs.Text = "托晒单管理";
            this.tPTs.UseVisualStyleBackColor = true;
            // 
            // tPPrint
            // 
            this.tPPrint.Location = new System.Drawing.Point(4, 28);
            this.tPPrint.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tPPrint.Name = "tPPrint";
            this.tPPrint.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tPPrint.Size = new System.Drawing.Size(1205, 570);
            this.tPPrint.TabIndex = 1;
            this.tPPrint.Text = "打印管理";
            this.tPPrint.UseVisualStyleBackColor = true;
            // 
            // tPSent
            // 
            this.tPSent.Location = new System.Drawing.Point(4, 28);
            this.tPSent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tPSent.Name = "tPSent";
            this.tPSent.Size = new System.Drawing.Size(1205, 570);
            this.tPSent.TabIndex = 2;
            this.tPSent.Text = "收发管理";
            this.tPSent.UseVisualStyleBackColor = true;
            // 
            // tpInfo
            // 
            this.tpInfo.Controls.Add(this.txtInfo);
            this.tpInfo.Location = new System.Drawing.Point(4, 28);
            this.tpInfo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpInfo.Name = "tpInfo";
            this.tpInfo.Size = new System.Drawing.Size(1205, 570);
            this.tpInfo.TabIndex = 3;
            this.tpInfo.Text = "详细信息";
            this.tpInfo.UseVisualStyleBackColor = true;
            // 
            // txtInfo
            // 
            this.txtInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInfo.Location = new System.Drawing.Point(0, 0);
            this.txtInfo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(1205, 570);
            this.txtInfo.TabIndex = 0;
            this.txtInfo.Text = "";
            // 
            // tbSta
            // 
            this.tbSta.Location = new System.Drawing.Point(4, 28);
            this.tbSta.Name = "tbSta";
            this.tbSta.Padding = new System.Windows.Forms.Padding(3);
            this.tbSta.Size = new System.Drawing.Size(1205, 570);
            this.tbSta.TabIndex = 4;
            this.tbSta.Text = "统计信息";
            this.tbSta.UseVisualStyleBackColor = true;
            // 
            // FrmArchivManage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1213, 602);
            this.Controls.Add(this.tbCtrlMain);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FrmArchivManage";
            this.Text = "东汽档案管理系统";
            this.Load += new System.EventHandler(this.FrmArchivManage_Load);
            this.tbCtrlMain.ResumeLayout(false);
            this.tpInfo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        public void ShowRs()
        {
            this.tbCtrlMain.SelectedTab = this.tpInfo;
        }
    }
}

