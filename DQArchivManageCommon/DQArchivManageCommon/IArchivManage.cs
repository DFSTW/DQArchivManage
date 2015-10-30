namespace DQArchivManageCommon
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Runtime.InteropServices;
    using System.Text;
    using Thyt.TiPLM.DEL.Product;

    public interface IArchivManage
    {
        ArrayList CheckTsdRight(ArrayList lstItems, string action, out StringBuilder strInfo, string clslb);
        void GetAmCando(Guid userOid, out int canTs, out int canPrint, out int canSent, out string signerRoles);
        DataSet GetBpmNameByUserOid(Guid useroid);
        ArrayList GetDocClsById(string doccode);
        DataSet GetDrawingForTsOutPut(Guid iroid);
        DataSet GetSecondDocStandard();
        DataSet GetSentLst(string docCode, string wkinfo, string tsdId, string tUnit, string tstype, string sentstate, DateTime dFrom, DateTime dTo);
        DataSet GetSentResultForOutPut(DEBusinessItem item, string username);
        DataSet GetSentResultForOutPut(ArrayList lstUnit, ArrayList lstItems, out Hashtable hsTbIdx, string userName);
        DataSet GetTSD(string docId, string wkName);
        DataSet GetTSD(string docCode, string bpmInfo, string TsStatue, string TsType, string OrgPrintUser, string ftlx, string unit, DateTime dFromTime, DateTime dToTime, bool isPrint);
        DataSet GetTSDForPrint(string docId, string wkName, Guid useroid);
        DataSet GetTsdFsdwByDoc(Guid useroid, string doccode, string clsname, string docname, string yct, string ftlx);
        DataSet GetTsRes();
        Hashtable GetViewOfCol(string v_type, out ArrayList lstOrder, out Hashtable hswide);
        void PrintOrSentTsd(Guid useroid, DEBusinessItem item, string action, out StringBuilder strInfo);
    }
}

