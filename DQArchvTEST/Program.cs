using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DQArchivManageSvr;
using System.Collections;
using Thyt.TiPLM.BRL.Product;
using Thyt.TiPLM.DEL.Product;
using System.Windows.Forms;
using Thyt.TiPLM.UIL.Common;
using Thyt.TiPLM.UIL.Product;
using Thyt.TiPLM.CLT.Admin.BPM;
using DQArchivManageClt;

namespace DQArchvTEST
{
    class Program
    {
         [STAThread]
        static void Main(string[] args)
        {
            //TestSVR();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //   Application.Run();


            bool isLoged = Login();
            if (!isLoged) return;
            int num;
            int num2;
            int num3;
            string str;
            PlArchivManage.Agent.GetAmCando(ClientData.LogonUser.Oid, out num, out num2, out num3, out str);
            PlArchivManage.SetCanSignUserName = str;
           
            FrmArchivManage.frmMian = new FrmArchivManage();
            FrmArchivManage.frmMian.InitFrm(num, num2, num3);
            FrmArchivManage.frmMian.MdiParent = ClientData.mainForm;
            FrmArchivManage.frmMian.Show();
            //FrmArchivManage frm = new FrmArchivManage();
            //if (ClientData.mainForm != null)
            //    frm.MdiParent = ClientData.mainForm;
            Application.Run(FrmArchivManage.frmMian);

        }
        /// <summary>
        /// 初始PLM公共数据
        /// </summary>
        /// <param name="user">登陆用户</param>
        /// <returns></returns>
        private static bool Init()
        {
            try
            {
                //this.curUser = user;
                PSInit.InitPS(ClientData.LogonUser, false);
                BPMEventInit.InitBPMEvent();
                Thyt.TiPLM.UIL.TiMessage.UIMessage.Instance.InitilizeMessage(null);
                //Thyt.TiPLM.UIL.Addin.AddinDeployment.Instance.SyncAddinsWithServer();
                return true;
            }
            catch (Exception ex)
            {
                //System.Diagnostics.EventLog.WriteEntry("PLM集成控件",ex.ToString(),System.Diagnostics.EventLogEntryType.Error);
                PrintException.Print(ex);
                return false;
            }
        }
        
        /// <returns>
        /// 登录成功返回true
        /// </returns>
        public static bool Login()
        {
            string product = "TiDesk";
            bool isLogin = false;
            try
            {
                //product += "Unknown";
                //   MessageBox.Show("1");
                if (FrmLogon.Logon(product, true))
                {

                    if (Init())
                    {
                        isLogin = true;
                    }
                }
            }
            catch (Exception ex)
            {
                PrintException.Print(ex);
            }
            return isLogin;
        }
        /// <summary>
        /// 注销登录
        /// </summary>
        
        private static void TestSVR()
        {
            BrArchivManager br = new BrArchivManager();
            Hashtable hashtable;
            ArrayList unit = new ArrayList();
            unit.Add("07焊接备料");
            ArrayList items = new ArrayList();
            items.Add(GetItem("201508100055"));
            // items.Add(GetItem("201508110083"));
            //items.Add(GetItem("201507210101"));
            //items.Add(GetItem("201508110083"));
            br.GetSentResultForOutPut(unit, items, out hashtable, "sysadmin");
        }

        private static DEBusinessItem GetItem(string id)
        {
            QRItem qritem = new QRItem();
            var master = qritem.GetItemMaster(id, "DQDOSSIRSENT", PLM.PLMCommon.Sysadmin);
            var item = qritem.GetBizItemByMaster(master.Oid, 0, Guid.Empty, PLM.PLMCommon.Sysadmin,
                Thyt.TiPLM.DEL.Product.BizItemMode.BizItem) as DEBusinessItem;
            return item;
        }
    }
}
