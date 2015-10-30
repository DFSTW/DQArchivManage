namespace DQArchivManageClt
{
    using System;
    using System.Windows.Forms;
    using Thyt.TiPLM.Common.Interface.Addin;
    using Thyt.TiPLM.UIL.Common;

    public class UiEntrance : IAddinClientEntry, ISystemClientAddin
    {
        public void Activate()
        {
            int num;
            int num2;
            int num3;
            string str;
            PlArchivManage.Agent.GetAmCando(ClientData.LogonUser.Oid, out num, out num2, out num3, out str);
            PlArchivManage.SetCanSignUserName = str;
            foreach (Form form in ClientData.mainForm.MdiChildren)
            {
                if (form is FrmArchivManage)
                {
                    form.Activate();
                    form.Show();
                    return;
                }
            }
            FrmArchivManage.frmMian = new FrmArchivManage();
            FrmArchivManage.frmMian.InitFrm(num, num2, num3);
            FrmArchivManage.frmMian.MdiParent = ClientData.mainForm;
            FrmArchivManage.frmMian.Show();
        }

        public void Config()
        {
        }
    }
}

