namespace DQArchivManageClt
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Windows.Forms;

    public class FrmTsdQuickSch : Form
    {
        private bool _isPrint;
        private Button btnCancel;
        private Button btnOK;
        private IContainer components;
        public DataSet ds;
        private DateTimePicker dTFromTime;
        private DateTimePicker dTToTime;
        private GroupBox groupBox2;
        private Label label1;
        private Label label10;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Panel panel1;
        private Panel pnlBtn;
        private ResWkInfo resWkTsd;
        private TextBox txtDocCode;
        private ComboBox txtFtlx;
        private TextBox txtOrgPrintUser;
        private ComboBox txtTsStatue;
        private ComboBox txtTsType;
        private ComboBox txtUnit;

        public FrmTsdQuickSch()
        {
            this._isPrint = false;
            this.components = null;
            this.InitializeComponent();
            this.Init();
        }

        public FrmTsdQuickSch(bool isPrint)
        {
            this._isPrint = false;
            this.components = null;
            this.InitializeComponent();
            this._isPrint = isPrint;
            this.Init();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            base.Close();
            base.DialogResult = DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.dTFromTime.Value > this.dTToTime.Value)
            {
                MessageBox.Show("截止时间的起始日期存在问题");
            }
            else
            {
                this.ds = PlArchivManage.Agent.GetTSD(this.txtDocCode.Text, this.resWkTsd.ResValue, this.txtTsStatue.Text, this.txtTsType.Text, this.txtOrgPrintUser.Text, this.txtFtlx.Text, this.txtUnit.Text, this.dTFromTime.Value, this.dTToTime.Value, this._isPrint);
                base.Close();
                base.DialogResult = DialogResult.OK;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void FrmTsdQuickSch_Load(object sender, EventArgs e)
        {
            this.resWkTsd = new ResWkInfo();
            this.resWkTsd.Dock = DockStyle.Fill;
            this.panel1.Controls.Add(this.resWkTsd);
        }

        private void Init()
        {
            if (!this._isPrint)
            {
                PlArchivManage.SetComBoxItem("托晒方式", this.txtTsType, "", this._isPrint);
                PlArchivManage.SetComBoxItem("托晒打印状态", this.txtTsStatue, "", this._isPrint);
            }
            else
            {
                PlArchivManage.SetComBoxItem("托晒方式", this.txtTsType, "", this._isPrint);
                PlArchivManage.SetComBoxItem("托晒打印状态", this.txtTsStatue, "", this._isPrint);
            }
            PlArchivManage.SetComBoxItem("发图类型", this.txtFtlx, null, this._isPrint);
            PlArchivManage.SetComBoxItem("路线部门", this.txtUnit, "", this._isPrint);
            DateTime time = DateTime.Now.AddDays(-7.0);
            this.dTFromTime.Value = new DateTime(time.Year, time.Month, time.Day);
            DateTime time2 = time.AddDays(15.0).AddSeconds(-1.0);
            this.dTToTime.Value = time2;
        }

        private void InitializeComponent()
        {
            this.pnlBtn = new Panel();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.label1 = new Label();
            this.label2 = new Label();
            this.txtDocCode = new TextBox();
            this.label4 = new Label();
            this.label5 = new Label();
            this.txtFtlx = new ComboBox();
            this.label6 = new Label();
            this.label7 = new Label();
            this.txtOrgPrintUser = new TextBox();
            this.dTToTime = new DateTimePicker();
            this.txtTsType = new ComboBox();
            this.label10 = new Label();
            this.groupBox2 = new GroupBox();
            this.dTFromTime = new DateTimePicker();
            this.label8 = new Label();
            this.label3 = new Label();
            this.txtTsStatue = new ComboBox();
            this.txtUnit = new ComboBox();
            this.panel1 = new Panel();
            this.pnlBtn.SuspendLayout();
            this.groupBox2.SuspendLayout();
            base.SuspendLayout();
            this.pnlBtn.Controls.Add(this.btnOK);
            this.pnlBtn.Controls.Add(this.btnCancel);
            this.pnlBtn.Dock = DockStyle.Bottom;
            this.pnlBtn.Location = new Point(0, 0x99);
            this.pnlBtn.Name = "pnlBtn";
            this.pnlBtn.Size = new Size(0x1c6, 0x29);
            this.pnlBtn.TabIndex = 0;
            this.btnOK.Location = new Point(0x10d, 10);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4b, 0x17);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            this.btnCancel.Location = new Point(0x173, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4b, 0x17);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0x17, 0x7e);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x35, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "发放单位";
            this.label2.AutoSize = true;
            this.label2.Location = new Point(5, 15);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x47, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "图号_文件号";
            this.txtDocCode.Location = new Point(0x52, 12);
            this.txtDocCode.Name = "txtDocCode";
            this.txtDocCode.Size = new Size(0x99, 0x15);
            this.txtDocCode.TabIndex = 4;
            this.label4.AutoSize = true;
            this.label4.Location = new Point(0x17, 0x4b);
            this.label4.Name = "label4";
            this.label4.Size = new Size(0x35, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "流程信息";
            this.label5.AutoSize = true;
            this.label5.Location = new Point(0x17, 0x65);
            this.label5.Name = "label5";
            this.label5.Size = new Size(0x35, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "发图类型";
            this.txtFtlx.FormattingEnabled = true;
            this.txtFtlx.Location = new Point(0x52, 0x5d);
            this.txtFtlx.Name = "txtFtlx";
            this.txtFtlx.Size = new Size(0x99, 20);
            this.txtFtlx.TabIndex = 10;
            this.label6.AutoSize = true;
            this.label6.Location = new Point(0xf3, 0x65);
            this.label6.Name = "label6";
            this.label6.Size = new Size(0x4d, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "托晒打印状态";
            this.label7.AutoSize = true;
            this.label7.Location = new Point(0xf3, 0x80);
            this.label7.Name = "label7";
            this.label7.Size = new Size(0x4d, 12);
            this.label7.TabIndex = 13;
            this.label7.Text = "被委托打印人";
            this.txtOrgPrintUser.Location = new Point(0x146, 0x7d);
            this.txtOrgPrintUser.Name = "txtOrgPrintUser";
            this.txtOrgPrintUser.Size = new Size(0x79, 0x15);
            this.txtOrgPrintUser.TabIndex = 15;
            this.dTToTime.Location = new Point(0x56, 50);
            this.dTToTime.Name = "dTToTime";
            this.dTToTime.Size = new Size(0x76, 0x15);
            this.dTToTime.TabIndex = 0x10;
            this.txtTsType.FormattingEnabled = true;
            this.txtTsType.Location = new Point(0x52, 40);
            this.txtTsType.Name = "txtTsType";
            this.txtTsType.Size = new Size(0x99, 20);
            this.txtTsType.TabIndex = 0x13;
            this.label10.AutoSize = true;
            this.label10.Location = new Point(0x17, 0x2b);
            this.label10.Name = "label10";
            this.label10.Size = new Size(0x35, 12);
            this.label10.TabIndex = 20;
            this.label10.Text = "托晒方式";
            this.groupBox2.Controls.Add(this.dTFromTime);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.dTToTime);
            this.groupBox2.Location = new Point(0xf3, 11);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new Size(210, 0x4b);
            this.groupBox2.TabIndex = 0x16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "截止日期";
            this.dTFromTime.Location = new Point(0x56, 0x17);
            this.dTFromTime.Name = "dTFromTime";
            this.dTFromTime.Size = new Size(0x76, 0x15);
            this.dTFromTime.TabIndex = 0x13;
            this.label8.AutoSize = true;
            this.label8.Location = new Point(60, 0x31);
            this.label8.Name = "label8";
            this.label8.Size = new Size(0x11, 12);
            this.label8.TabIndex = 0x12;
            this.label8.Text = "至";
            this.label3.AutoSize = true;
            this.label3.Location = new Point(60, 0x1a);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x11, 12);
            this.label3.TabIndex = 0x11;
            this.label3.Text = "从";
            this.txtTsStatue.FormattingEnabled = true;
            this.txtTsStatue.Location = new Point(0x146, 0x5d);
            this.txtTsStatue.Name = "txtTsStatue";
            this.txtTsStatue.Size = new Size(0x79, 20);
            this.txtTsStatue.TabIndex = 12;
            this.txtUnit.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.txtUnit.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.txtUnit.FormattingEnabled = true;
            this.txtUnit.Location = new Point(0x52, 120);
            this.txtUnit.Name = "txtUnit";
            this.txtUnit.Size = new Size(0x99, 20);
            this.txtUnit.TabIndex = 0x18;
            this.panel1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.panel1.Location = new Point(0x52, 0x3d);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0x99, 0x20);
            this.panel1.TabIndex = 0x22;
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            //base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x1c6, 0xc2);
            base.Controls.Add(this.panel1);
            base.Controls.Add(this.txtUnit);
            base.Controls.Add(this.groupBox2);
            base.Controls.Add(this.label10);
            base.Controls.Add(this.txtTsType);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.txtOrgPrintUser);
            base.Controls.Add(this.label7);
            base.Controls.Add(this.txtTsStatue);
            base.Controls.Add(this.label6);
            base.Controls.Add(this.txtFtlx);
            base.Controls.Add(this.label5);
            base.Controls.Add(this.label4);
            base.Controls.Add(this.txtDocCode);
            base.Controls.Add(this.label2);
            base.Controls.Add(this.pnlBtn);
            base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "FrmTsdQuickSch";
            base.ShowIcon = false;
            this.Text = "快速查询托晒单";
            base.Load += new EventHandler(this.FrmTsdQuickSch_Load);
            this.pnlBtn.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }
    }
}

