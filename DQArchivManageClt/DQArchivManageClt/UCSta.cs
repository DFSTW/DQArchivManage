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
            this.dateTimePicker2.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 26);
            listView1.FullRowSelect = true;
            listView1.GridLines = true;

            listView1.View = View.Details;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string selector = "and u.plm_logid  = '" + ClientData.LogonUser.LogId + "' ";
            if (ClientData.LogonUser.LogId == "05700" || ClientData.LogonUser.LogId == "sysadmin" || ClientData.LogonUser.LogId == "01975")
                selector = string.Empty;
            listView1.Columns.Clear();
            listView1.Columns.Add("姓名", 120, HorizontalAlignment.Left);
            listView1.Columns.Add("托晒单数", 120, HorizontalAlignment.Left);
            listView1.Columns.Add("图纸张数", 120, HorizontalAlignment.Left);
            var dt = PLHelper.QuerySql(ClientData.Session, @"select u.plm_name 姓名, count(*) 托晒单数, sum(t.plm_mtzs) 图纸张数
from plm_psm_itemmaster_revision m, plm_cus_dqdossierprint t , plm_adm2_user u
where m.plm_m_lastrevision = m.plm_r_revision
and u.plm_oid = m.plm_r_creator
and m.plm_r_lastiteration = t.plm_iteration
and m.plm_r_oid = t.plm_revisionoid
and t.plm_checkintime >= to_date('" + dateTimePicker1.Value.ToString("yyyy-MM-dd") + @"','yyyy-MM-dd')
and t.plm_checkintime <= to_date('" + dateTimePicker2.Value.ToString("yyyy-MM-dd") + @"','yyyy-MM-dd')
and t.plm_tsstatus not in('未发打印') " + selector +
"group by u.plm_name");
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

        private void button2_Click(object sender, EventArgs e)
        {
            string selector = "and u.plm_logid  = '" + ClientData.LogonUser.LogId + "' ";
            if (ClientData.LogonUser.LogId == "05700" || ClientData.LogonUser.LogId == "sysadmin" || ClientData.LogonUser.LogId == "01975")
                selector = string.Empty;
            listView1.Columns.Clear();
            listView1.Columns.Add("姓名", 80, HorizontalAlignment.Left);
            listView1.Columns.Add("投图方式", 80, HorizontalAlignment.Left);
            listView1.Columns.Add("发图类型", 130, HorizontalAlignment.Left);
            listView1.Columns.Add("托晒单数", 80, HorizontalAlignment.Left);
            listView1.Columns.Add("图纸张数", 80, HorizontalAlignment.Left);

            var dt = PLHelper.QuerySql(ClientData.Session, @"select a.plm_name 姓名, a.plm_yct2 投图方式, a.plm_ftlx2 发图类型,count(*) 托晒单数, sum(a.plm_mtzs) 图纸张数
from (select u.plm_name, t.plm_wkflinfo, t.plm_mtzs, case  to_char(t.plm_yct) when '一次图' then '一次图' else '二次图' end plm_yct2, 
case to_char( t.plm_ftlx) when '标准' then '技术文件/标准' when '技术文件' then '技术文件/标准' else '图纸' end plm_ftlx2
from plm_psm_itemmaster_revision m, plm_cus_dqdossierprint t , plm_adm2_user u
where m.plm_m_lastrevision = m.plm_r_revision
and u.plm_oid = m.plm_r_creator
and m.plm_r_lastiteration = t.plm_iteration
and m.plm_r_oid = t.plm_revisionoid
and t.plm_checkintime >= to_date('" + dateTimePicker1.Value.ToString("yyyy-MM-dd") + @"','yyyy-MM-dd')
and t.plm_checkintime <= to_date('" + dateTimePicker2.Value.ToString("yyyy-MM-dd") + @"','yyyy-MM-dd')
and t.plm_tsstatus not in('未发打印')
" + selector +
@"
) a
/*order by u.plm_name*/
group by a.plm_name, a.plm_yct2, a.plm_ftlx2 
order by 姓名,投图方式 desc,发图类型");
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
