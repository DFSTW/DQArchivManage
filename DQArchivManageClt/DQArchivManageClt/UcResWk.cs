namespace DQArchivManageClt
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    public class UcResWk : UserControl
    {
        private IContainer components = null;
        private DataGridView dgView;
        private DataTable tbZ;

        public event SelectResHandler2 ResTextChanged;

        public UcResWk(DataSet ds)
        {
            this.InitializeComponent();
            this.SetDataSource(ds);
        }

        private void dgView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.dgView.CurrentCell != null)
            {
                int rowIndex = this.dgView.CurrentCell.RowIndex;
                string str = this.dgView.Rows[rowIndex].Cells["流程名称"].Value.ToString();
                if (!(string.IsNullOrEmpty(str) || (this.ResTextChanged == null)))
                {
                    this.ResTextChanged(str);
                }
                if (base.Parent != null)
                {
                    base.Parent.Hide();
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

        public void Filter(string str)
        {
            string[] c = new string[] { "*", "%", "[", "]" };
            ArrayList list = new ArrayList(c);
            bool flag = false;
            foreach (string str2 in list)
            {
                if (str.IndexOf(str2) != -1)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                DataTable table = this.tbZ.Clone();
                foreach (DataRow row in this.tbZ.Rows)
                {
                    flag = false;
                    foreach (DataColumn column in this.tbZ.Columns)
                    {
                        if (column.DataType == typeof(string))
                        {
                            string str3 = (row[column] == DBNull.Value) ? "" : row[column].ToString();
                            if (str3.IndexOf(str) != -1)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (flag)
                    {
                        DataRow row2 = table.NewRow();
                        row2.ItemArray = row.ItemArray;
                        table.Rows.Add(row2);
                    }
                }
                this.dgView.DataSource = table.DefaultView;
                this.dgView.ReadOnly = true;
                this.dgView.AllowUserToAddRows = this.dgView.AllowUserToDeleteRows = false;
                this.dgView.Sort(this.dgView.Columns[0], ListSortDirection.Ascending);
            }
            else
            {
                DataView defaultView = this.tbZ.DefaultView;
                defaultView.RowFilter = this.GetFilterstr(str);
                this.dgView.DataSource = defaultView;
            }
        }

        private string GetFilterstr(string str)
        {
            if (string.IsNullOrEmpty(str) || (str == "(无)"))
            {
                return "";
            }
            DataView dataSource = this.dgView.DataSource as DataView;
            DataTable table = dataSource.Table;
            StringBuilder builder = new StringBuilder();
            foreach (DataColumn column in table.Columns)
            {
                if (column.DataType == typeof(string))
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(" or ");
                    }
                    builder.Append(column.ColumnName + " like '%" + str + "%' ");
                }
            }
            return builder.ToString();
        }

        private void InitializeComponent()
        {
            this.dgView = new DataGridView();
            ((ISupportInitialize) this.dgView).BeginInit();
            base.SuspendLayout();
            this.dgView.AllowUserToAddRows = false;
            this.dgView.AllowUserToDeleteRows = false;
            this.dgView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dgView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgView.Dock = DockStyle.Fill;
            this.dgView.Location = new Point(0, 0);
            this.dgView.Name = "dgView";
            this.dgView.ReadOnly = true;
            this.dgView.RowTemplate.Height = 0x17;
            this.dgView.Size = new Size(0x29a, 0x163);
            this.dgView.TabIndex = 0;
            this.dgView.MouseDoubleClick += new MouseEventHandler(this.dgView_MouseDoubleClick);
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            base.Controls.Add(this.dgView);
            base.Name = "UcResWk";
            base.Size = new Size(0x29a, 0x163);
            ((ISupportInitialize) this.dgView).EndInit();
            base.ResumeLayout(false);
        }

        public void SetDataSource(DataSet ds)
        {
            this.tbZ = ds.Tables[0].Copy();
            this.dgView.ReadOnly = false;
            this.dgView.AllowUserToAddRows = this.dgView.AllowUserToDeleteRows = true;
            this.dgView.DataSource = this.tbZ.DefaultView;
            this.dgView.ReadOnly = true;
            this.dgView.AllowUserToAddRows = this.dgView.AllowUserToDeleteRows = false;
            this.dgView.Sort(this.dgView.Columns[0], ListSortDirection.Ascending);
        }
    }
}

