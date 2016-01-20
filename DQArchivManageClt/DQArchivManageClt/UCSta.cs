using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Thyt.TiPLM.UIL.Common;
using Thyt.TiPLM.PLL.Common;

namespace DQArchivManageClt
{
    public partial class UCSta : UserControl
    {
        public UCSta()
        {
            InitializeComponent();
            this.dateTimePicker1.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 26).AddMonths(-1);
            this.dateTimePicker2.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 25);
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Columns.Add("姓名", 80, HorizontalAlignment.Left);
            listView1.Columns.Add("托晒单数", 80, HorizontalAlignment.Left);
            listView1.Columns.Add("图纸张数", 80, HorizontalAlignment.Left);
            listView1.View = View.Details;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            var dt = PLHelper.QuerySql(ClientData.Session, @"select u.plm_name 姓名, count(*) 托晒单数, sum(t.plm_mtzs) 图纸张数
from plm_psm_itemmaster_revision m, plm_cus_dqdossierprint t , plm_adm2_user u
where m.plm_m_lastrevision = m.plm_r_revision
and u.plm_oid = m.plm_r_creator
and m.plm_r_lastiteration = t.plm_iteration
and m.plm_r_oid = t.plm_revisionoid
and m.plm_r_createtime >= to_date('" + dateTimePicker1.Value.ToString("yyyy-MM-dd") + @"','yyyy-MM-dd')
and m.plm_r_createtime <= to_date('" + dateTimePicker2.Value.ToString("yyyy-MM-dd") + @"','yyyy-MM-dd')
and t.plm_tsstatus not in('未发打印')
group by u.plm_name");
            this.listView1.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                ListViewItem item = new ListViewItem(row[0].ToString());
                for (int i = 1; i < dt.Columns.Count; i++)
                {
                    item.SubItems.Add(row[i].ToString());
                }
                this.listView1.Items.Add(item);
            }


        }
    }
}
