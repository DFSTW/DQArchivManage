using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DQ.Common.RouteProcess;
using Thyt.TiPLM.DEL.Product;
using Thyt.TiPLM.BRL.Common;
using Thyt.TiPLM.BRL.Product;
using System.Collections;

namespace RouteProcessTest
{
    class RouteP : QRBase
    {
        static void Main(string[] args)
        {
            RouteP p = new RouteP();
            p.GetPSItems("000T", 
                new Guid( PLM.PLMCommon.OracleToDotNet("A5A1DF150C8D104EB7240011A7A09363")),
                10070552, null, 0, false,
                new Guid(PLM.PLMCommon.OracleToDotNet("5421861F3FF4504EB5116DD68EB873DD")));
        }
        public DEFullRelationBizItem1[] GetPSItems(string prjId, Guid rootMasterOid, long child_pos_id, DEPSOption option, int level, bool isLight, Guid userOid)
        {
            //try
            //{
            if (!this.isInTrans)
                this.dbParam.Open();

            // 获取用户的全局有效性选项
            if (option == null)
            {
                PRPSOption prOption = new PRPSOption(this.dbParam);
                option = prOption.GetUserGlobalOption(userOid);
            }
            //if (option == null)
            //    throw new PLMException(ExceptionManager.E_PDT_CANNOT_GET_USERGLOBALOPTION);
            string msg = "";
            DateTime start = DateTime.Now;
            DARouteProcess daRouteProcess = new DARouteProcess(this.dbParam);
            List<Guid> masterOids = null;
            List<int> revs = null;
            List<DEFullRelationBizItem1> l_fullItems = daRouteProcess.GetPSItems(prjId, rootMasterOid, child_pos_id, level, out masterOids, out revs);
            msg += "GetPSItems:" + l_fullItems.Count.ToString() + ":" + DateTime.Now.Subtract(start).ToString();
            start = DateTime.Now;
            //if (l_fullItems == null && l_fullItems.Count > 0)
            //{
            //    Guid masterOid = Guid.Empty;
            //    int rev = 0;
            //    DEFullRelationBizItem1 fullItem1 = daRouteProcess.GetPSRootItem(prjId, l_fullItems[0].child_posid, out masterOid, out rev);
            //    msg += "GetPSRootItem1:" + DateTime.Now.Subtract(start).ToString();
            //    start = DateTime.Now;
            //    if (fullItem1 != null)
            //    {
            //        l_fullItems.Insert(0, fullItem1);
            //        if (!masterOids.Contains(masterOid))
            //        {
            //            masterOids.Insert(0, masterOid);
            //            revs.Insert(0, rev);
            //        }
            //    }
            //    msg += "GetPSRootItem2:" + DateTime.Now.Subtract(start).ToString();
            //    start = DateTime.Now;
            //}
            //    ArrayList bizitems = new ArrayList();
            //    if (isLight)
            //    {
            //        bizitems = daRouteProcess.GetRoutePartItem(l_fullItems);
            //        msg += "GetRoutePartItem:" + bizitems.Count.ToString() + ":" +DateTime.Now.Subtract(start).ToString();
            //        start = DateTime.Now;
            //    }
            //    else
            //    {
            //        QRItem qrItem = new QRItem(this.dbParam);
            //        List<int> iters = new List<int>();
            //        for (int i = 0; i < masterOids.Count; i++)
            //            iters.Add(0);
            //        bizitems = qrItem.GetBizItems(masterOids.ToArray(), revs.ToArray(), iters.ToArray(), option.CurView, userOid, BizItemMode.BizItem);
            //        msg += "GetBizItems:" + bizitems .Count.ToString()+ ":" + DateTime.Now.Subtract(start).ToString();
            //        start = DateTime.Now;
            //    }
            //    foreach (DEFullRelationBizItem1 f in l_fullItems)
            //    {
            //        foreach (DEBusinessItem bizitem in bizitems)
            //        {
            //            if (bizitem.MasterOid == f.partMasterOid)
            //            {
            //                f.RightItem = bizitem;
            //                break;
            //            }
            //        }
            //    }
            //    msg += "foreach:" + DateTime.Now.Subtract(start).ToString();
            //    try
            //    {
            //        System.Diagnostics.EventLog.WriteEntry("TiPLM",msg);
            //    }
            //    catch { }
            //    return l_fullItems.ToArray();
            //}
            //catch (Exception ex)
            //{
            //    //if (ex is PLMException)
            //    //    throw ex;
            //    //else
            //    //{
            //    //    PLMEventLog.WriteExceptionLog(ex);
            //    //    throw new PLMException(ExceptionManager.E_PDT_CANNOT_GET_PSTREE, ex);
            //    //}
            //}
            //finally
            //{
            //    if (!this.isInTrans)
            //        this.dbParam.Close();
            //}
            return null;
        }
    }
}
