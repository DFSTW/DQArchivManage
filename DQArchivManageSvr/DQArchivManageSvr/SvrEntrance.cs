namespace DQArchivManageSvr
{
    using DQArchivManageCommon;
    using System;
    using System.Collections;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Text;
    using System.Xml;
    using Thyt.TiPLM.BRL.Admin.NewResponsibility;
    using Thyt.TiPLM.Common;
    using Thyt.TiPLM.Common.Interface.Addin;
    using Thyt.TiPLM.DEL.Admin.NewResponsibility;
    using Thyt.TiPLM.DEL.Product;

    public class SvrEntrance : PLMBFRoot, IAddinServiceEntry, IArchivManage
    {
        private BrArchivManager brArchiv = new BrArchivManager();
        private Hashtable hsGetResType;

        public SvrEntrance()
        {
            this.hsGetResType = this.GetResType();
        }

        public ArrayList CheckTsdRight(ArrayList lstItems, string action, out StringBuilder strInfo, string clslb)
        {
            return this.brArchiv.CheckTsdRight(lstItems, action, out strInfo, clslb);
        }

        public void GetAmCando(Guid userOid, out int canTs, out int canPrint, out int canSent, out string signrolename)
        {
            BRUser user = new BRUser(userOid);
            bool flag = user.IsAdministrator();
            signrolename = "";
            canTs = 0;
            canPrint = 0;
            canSent = 0;
            ArrayList allAssignedRoles = user.GetAllAssignedRoles();
            ArrayList list2 = new ArrayList();
            ArrayList list3 = new ArrayList();
            ArrayList list4 = new ArrayList();
            ArrayList list5 = new ArrayList();
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dq_daconfig.xml");
            if (File.Exists(path))
            {
                XmlDocument document = new XmlDocument();
                document.Load(path);
                foreach (XmlElement element in document.DocumentElement.ChildNodes)
                {
                    if (element.Name == "Roles")
                    {
                        foreach (XmlElement element2 in element.ChildNodes)
                        {
                            foreach (XmlElement element3 in element2.ChildNodes)
                            {
                                if (element2.Name == "TS")
                                {
                                    list2.Add(element3.InnerText);
                                }
                                else if (element2.Name == "Print")
                                {
                                    list3.Add(element3.InnerText);
                                }
                                else if (element2.Name == "Suiji")
                                {
                                    list5.Add(element3.InnerText);
                                }
                                else if (element2.Name == "Signer")
                                {
                                    signrolename = element3.InnerText;
                                }
                                else
                                {
                                    list4.Add(element3.InnerText);
                                }
                            }
                        }
                    }
                }
                if (flag)
                {
                    int num;
                    canTs = num = 1;
                    canPrint = canSent = num;
                }
                else
                {
                    foreach (DERole role in allAssignedRoles)
                    {
                        if (list2.Contains(role.Name))
                        {
                            canTs = 1;
                        }
                        if (list3.Contains(role.Name))
                        {
                            canPrint = 1;
                        }
                        if (list4.Contains(role.Name))
                        {
                            canSent = 1;
                        }
                        if (list5.Contains(role.Name))
                        {
                            canSent = 2;
                        }
                    }
                }
            }
        }

        public DataSet GetBpmNameByUserOid(Guid useroid)
        {
            return this.brArchiv.GetBpmNameByUserOid(useroid);
        }

        public ArrayList GetDocClsById(string doccode)
        {
            return this.brArchiv.GetDocClsById(doccode);
        }

        public DataSet GetDrawingForTsOutPut(Guid iroid)
        {
            return this.brArchiv.GetDrawingForTsOutput(iroid);
        }

        private Hashtable GetResType()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "dq_daconfig.xml");
            Hashtable hashtable = new Hashtable();
            if (File.Exists(path))
            {
                XmlDocument document = new XmlDocument();
                document.Load(path);
                foreach (XmlElement element in document.DocumentElement.ChildNodes)
                {
                    if (element.Name == "TSRes")
                    {
                        foreach (XmlElement element2 in element.ChildNodes)
                        {
                            string attribute = element2.GetAttribute("tbName");
                            string str3 = element2.GetAttribute("lb");
                            if (!string.IsNullOrEmpty(attribute))
                            {
                                hashtable[str3] = attribute;
                            }
                        }
                    }
                }
            }
            return hashtable;
        }

        public DataSet GetSecondDocStandard()
        {
            return this.brArchiv.GetSecondDocStandard();
        }

        public DataSet GetSentLst(string docCode, string wkinfo, string tsdId, string tUnit, string tstype, string sentstate, DateTime dFrom, DateTime dTo)
        {
            return this.brArchiv.GetSentLst(docCode, wkinfo, tsdId, tUnit, tstype, sentstate, dFrom, dTo);
        }

        public DataSet GetSentResultForOutPut(DEBusinessItem item, string username)
        {
            return this.brArchiv.GetSentResultForOutPut(item, username);
        }

        public DataSet GetSentResultForOutPut(ArrayList lstUnit, ArrayList lstItems, out Hashtable hsTbIdx, string username)
        {
            return this.brArchiv.GetSentResultForOutPut(lstUnit, lstItems, out hsTbIdx, username);
        }

        public DataSet GetTSD(string docId, string wkName)
        {
            return this.brArchiv.GetTSD(docId, wkName);
        }

        public DataSet GetTSD(string docCode, string bpmInfo, string TsStatue, string TsType, string OrgPrintUser, string ftlx, string unit, DateTime dFromTime, DateTime dToTime, bool isPrint)
        {
            return this.brArchiv.GetTSD(docCode, bpmInfo, TsStatue, TsType, OrgPrintUser, ftlx, unit, dFromTime, dToTime, isPrint);
        }

        public DataSet GetTSDForPrint(string docId, string wkName, Guid useroid)
        {
            return this.brArchiv.GetTSDForPrint(docId, useroid, wkName);
        }

        public DataSet GetTsdFsdwByDoc(Guid useroid, string doccode, string clsname, string yct, string docname, string ftlx)
        {
            return this.brArchiv.GetTsdFsdwByDoc(useroid, doccode, clsname, docname, yct, ftlx);
        }

        public DataSet GetTsRes()
        {
            return this.brArchiv.GetTsRes(this.hsGetResType);
        }

        public Hashtable GetViewOfCol(string v_type, out ArrayList lstOrder, out Hashtable hswide)
        {
            return this.brArchiv.GetViewOfCol(v_type, out lstOrder, out hswide);
        }

        public void PrintOrSentTsd(Guid useroid, DEBusinessItem item, string action, out StringBuilder strInfo)
        {
            this.brArchiv.PrintOrSentTsd(useroid, item, action, out strInfo);
        }

        public WellKnownServiceTypeEntry[] RemoteTypes
        {
            get
            {
                return new WellKnownServiceTypeEntry[] { new WellKnownServiceTypeEntry(typeof(SvrEntrance), "TiPLM/DQArchivManage/SVR/BFEntrance.rem", WellKnownObjectMode.SingleCall) };
            }
        }


        public void SignSentList(ArrayList lisItems, string unit, string signer, string sm)
        {
            this.brArchiv.SignSentList(lisItems, unit, signer, sm);
        }


        public DataSet GetSentLstSuiJi(string docCode, string wkinfo, string tsdId, string tUnit, string tstype, string sentstate, DateTime dFrom, DateTime dTo)
        {
            return this.brArchiv.GetSentLstSuiJi(docCode, wkinfo, tsdId, tUnit, tstype, sentstate, dFrom, dTo);
        }
    }
}

