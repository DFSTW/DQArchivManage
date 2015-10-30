namespace DQArchivManageClt
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class FrmUnit : Form
    {
        private Button btnCancel;
        private Button btnOK;
        private CheckBox chkAll;
        private CheckBox chkAllNull;
        private ColumnHeader columnHeader1;
        private IContainer components = null;
        public ArrayList LstUnit;
        private ListView lvwUnit;
        private Panel panel1;

        public FrmUnit()
        {
            this.InitializeComponent();
            this.LstUnit = new ArrayList();
            this.init();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.lvwUnit.CheckedItems.Count == 0)
            {
                MessageBox.Show("必须选择一个或多个单位");
            }
            else
            {
                foreach (ListViewItem item in this.lvwUnit.CheckedItems)
                {
                    this.LstUnit.Add(item.Text);
                }
                base.DialogResult = DialogResult.OK;
                base.Close();
            }
        }

        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkAll.Checked)
            {
                this.chkAllNull.Checked = false;
                foreach (ListViewItem item in this.lvwUnit.Items)
                {
                    item.Checked = true;
                }
            }
        }

        private void chkAllNull_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkAllNull.Checked)
            {
                this.chkAllNull.Checked = false;
                foreach (ListViewItem item in this.lvwUnit.Items)
                {
                    item.Checked = false;
                }
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

        private void init()
        {
            ArrayList tsRes = PlArchivManage.GetTsRes("路线部门") as ArrayList;
            foreach (string str in tsRes)
            {
                ListViewItem item = new ListViewItem(str) {
                    Text = str
                };
                this.lvwUnit.Items.Add(item);
            }
        }

        private void InitializeComponent()
        {
            this.panel1 = new Panel();
            this.btnCancel = new Button();
            this.btnOK = new Button();
            this.lvwUnit = new ListView();
            this.columnHeader1 = new ColumnHeader();
            this.chkAll = new CheckBox();
            this.chkAllNull = new CheckBox();
            this.panel1.SuspendLayout();
            base.SuspendLayout();
            this.panel1.Controls.Add(this.chkAllNull);
            this.panel1.Controls.Add(this.chkAll);
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Dock = DockStyle.Bottom;
            this.panel1.Location = new Point(0, 0x135);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0x125, 0x27);
            this.panel1.TabIndex = 0;
            this.btnCancel.Anchor = AnchorStyles.Right;
            this.btnCancel.Location = new Point(220, 7);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x3d, 0x17);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.btnOK.Anchor = AnchorStyles.Right;
            this.btnOK.Location = new Point(0x97, 7);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(0x3f, 0x17);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);
            this.lvwUnit.CheckBoxes = true;
            this.lvwUnit.Columns.AddRange(new ColumnHeader[] { this.columnHeader1 });
            this.lvwUnit.Dock = DockStyle.Fill;
            this.lvwUnit.FullRowSelect = true;
            this.lvwUnit.Location = new Point(0, 0);
            this.lvwUnit.Name = "lvwUnit";
            this.lvwUnit.Size = new Size(0x125, 0x135);
            this.lvwUnit.Sorting = SortOrder.Ascending;
            this.lvwUnit.TabIndex = 1;
            this.lvwUnit.UseCompatibleStateImageBehavior = false;
            this.lvwUnit.View = View.Details;
            this.lvwUnit.ItemChecked += new ItemCheckedEventHandler(this.lvwUnit_ItemChecked);
            this.columnHeader1.Text = "单位简称";
            this.columnHeader1.Width = 0x11b;
            this.chkAll.AutoSize = true;
            this.chkAll.Location = new Point(12, 13);
            this.chkAll.Name = "chkAll";
            this.chkAll.Size = new Size(0x30, 0x10);
            this.chkAll.TabIndex = 2;
            this.chkAll.Text = "全选";
            this.chkAll.UseVisualStyleBackColor = true;
            this.chkAll.CheckedChanged += new EventHandler(this.chkAll_CheckedChanged);
            this.chkAllNull.AutoSize = true;
            this.chkAllNull.Location = new Point(0x42, 13);
            this.chkAllNull.Name = "chkAllNull";
            this.chkAllNull.Size = new Size(60, 0x10);
            this.chkAllNull.TabIndex = 3;
            this.chkAllNull.Text = "全不选";
            this.chkAllNull.UseVisualStyleBackColor = true;
            this.chkAllNull.CheckedChanged += new EventHandler(this.chkAllNull_CheckedChanged);
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            base.ClientSize = new Size(0x125, 0x15c);
            base.Controls.Add(this.lvwUnit);
            base.Controls.Add(this.panel1);
            base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            base.Name = "FrmUnit";
            this.Text = "选择需要输出的单位";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            base.ResumeLayout(false);
        }

        private void lvwUnit_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Checked && this.chkAllNull.Checked)
            {
                this.chkAllNull.Checked = false;
            }
            if (!(e.Item.Checked || !this.chkAll.Checked))
            {
                this.chkAll.Checked = false;
            }
        }
    }
}

