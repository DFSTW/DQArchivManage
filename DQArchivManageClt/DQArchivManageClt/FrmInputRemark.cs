namespace DQArchivManageClt
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class FrmInputRemark : Form
    {
        private bool _canNull;
        private Button btnCancel;
        private Button btnOK;
        private CheckBox chkPrintBomAll;
        private CheckBox chkSameReason;
        private IContainer components = null;
        public bool IsSameReason = false;
        private Panel panel1;
        public string StrMarkup;
        private RichTextBox txtMarkup;

        public FrmInputRemark(string tp, bool canNull, bool isJustOne)
        {
            this.InitializeComponent();
            this.Text = tp;
            this._canNull = canNull;
            this.chkSameReason.Visible = !isJustOne;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.IsSameReason = this.chkSameReason.Checked;
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!(this._canNull || (this.txtMarkup.Text.Trim().Length != 0)))
            {
                MessageBox.Show("必须填写原因或说明！");
            }
            else
            {
                this.IsSameReason = this.chkSameReason.Checked;
                this.StrMarkup = this.txtMarkup.Text;
                base.DialogResult = DialogResult.OK;
                base.Close();
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

        private void FrmInputRemark_Load(object sender, EventArgs e)
        {
            this.txtMarkup.Select();
            this.txtMarkup.Focus();
        }

        private void InitializeComponent()
        {
            this.panel1 = new Panel();
            this.chkPrintBomAll = new CheckBox();
            this.chkSameReason = new CheckBox();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.txtMarkup = new RichTextBox();
            this.panel1.SuspendLayout();
            base.SuspendLayout();
            this.panel1.Controls.Add(this.chkPrintBomAll);
            this.panel1.Controls.Add(this.chkSameReason);
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Dock = DockStyle.Bottom;
            this.panel1.Location = new Point(0, 0xf5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0x235, 0x37);
            this.panel1.TabIndex = 0;
            this.chkPrintBomAll.AutoSize = true;
            this.chkPrintBomAll.Checked = true;
            this.chkPrintBomAll.CheckState = CheckState.Checked;
            this.chkPrintBomAll.Location = new Point(0x1a, 20);
            this.chkPrintBomAll.Name = "chkPrintBomAll";
            this.chkPrintBomAll.Size = new Size(90, 0x10);
            this.chkPrintBomAll.TabIndex = 3;
            this.chkPrintBomAll.Text = "打印完成BOM";
            this.chkPrintBomAll.UseVisualStyleBackColor = true;
            this.chkSameReason.AutoSize = true;
            this.chkSameReason.Location = new Point(0x8b, 20);
            this.chkSameReason.Name = "chkSameReason";
            this.chkSameReason.Size = new Size(0x60, 0x10);
            this.chkSameReason.TabIndex = 2;
            this.chkSameReason.Text = "其余原因相同";
            this.chkSameReason.UseVisualStyleBackColor = true;
            this.btnOK.Location = new Point(0x1de, 0x10);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x4b, 0x17);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            this.btnCancel.Location = new Point(0x179, 0x10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4b, 0x17);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.txtMarkup.Dock = DockStyle.Fill;
            this.txtMarkup.Location = new Point(0, 0);
            this.txtMarkup.Name = "txtMarkup";
            this.txtMarkup.Size = new Size(0x235, 0xf5);
            this.txtMarkup.TabIndex = 1;
            this.txtMarkup.Text = "";
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            //base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x235, 300);
            base.Controls.Add(this.txtMarkup);
            base.Controls.Add(this.panel1);
            //base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "FrmInputRemark";
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "FrmInputRemark";
            base.Load += new EventHandler(this.FrmInputRemark_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            base.ResumeLayout(false);
        }
    }
}

