namespace DQArchivManageClt
{
    using DQArchivManageCommon;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using Thyt.TiPLM.CLT.Admin.BPM;
    using Thyt.TiPLM.Common;
    using Thyt.TiPLM.DEL.Admin.BPM;
    using Thyt.TiPLM.DEL.Admin.DataModel;
    using Thyt.TiPLM.DEL.Admin.NewResponsibility;
    using Thyt.TiPLM.DEL.Product;
    using Thyt.TiPLM.PLL.Admin.BPM;
    using Thyt.TiPLM.PLL.Admin.DataModel;
    using Thyt.TiPLM.PLL.Admin.NewResponsibility;
    using Thyt.TiPLM.PLL.Common;
    using Thyt.TiPLM.PLL.Product2;
    using Thyt.TiPLM.UIL.Common;
    using Thyt.TiPLM.UIL.Product.Common;

    public class PlArchivManage
    {
        private static IArchivManage _agent;
        private static Hashtable _hsRes;
        private static DataSet dsBpm;
        private static DataSet dsSecondNum;
        private static ArrayList lstSigners = null;

        internal static void AddLvwRelValues(ListView lv, ArrayList lstOrder, DERelationBizItem dItem, string unit)
        {
            DEMetaRelation relation = ModelContext.MetaModel.GetRelation(dItem.Relation.RelationName);
            ArrayList relationAttributes = ModelContext.MetaModel.GetRelationAttributes(relation.Oid, 1);
            if (string.IsNullOrEmpty(unit))
            {
                SetLvwRelValues(lv, lstOrder, dItem, relationAttributes, "");
            }
            else
            {
                object attrValue = dItem.Relation.GetAttrValue("JSDW");
                if (attrValue != null)
                {
                    foreach (string str in attrValue.ToString().Replace("；", ";").Split(new char[] { ';' }))
                    {
                        if (string.IsNullOrEmpty(unit) || (str.IndexOf(unit + "(") == 0))
                        {
                            SetLvwRelValues(lv, lstOrder, dItem, relationAttributes, str);
                        }
                    }
                }
            }
        }

        public static DERelationBizItem AddNewRelation(DEBusinessItem item, string relName, DEBusinessItem cItem)
        {
            DERelation2 relation = new DERelation2 {
                LeftClass = item.Master.ClassName,
                LeftObj = item.Iteration.Oid,
                RelationName = relName,
                RightClass = cItem.Master.ClassName,
                RightObj = cItem.Master.Oid,
                RightObjRev = 0,
                CreatorName = ClientData.LogonUser.Name,
                CreateTime = DateTime.Now,
                View = PSStart.GetLinkView(item.Master, relName, ClientData.UserGlobalOption, ClientData.LogonUser.Oid)
            };
            DERelationBizItemList relationBizItemList = item.Iteration.LinkRelationSet.GetRelationBizItemList(relName);
            if (relationBizItemList == null)
            {
                try
                {
                    relationBizItemList = PLItem.Agent.GetLinkRelationItems(item.Iteration.Oid, item.Master.ClassName, relName, ClientData.LogonUser.Oid, ClientData.UserGlobalOption);
                    item.Iteration.LinkRelationSet.AddRelationList(relName, relationBizItemList);
                }
                catch
                {
                }
            }
            if (relationBizItemList != null)
            {
                relation.Order = GetPossibleOrder(relationBizItemList);
                item.Iteration.LinkRelationSet.GetRelationBizItemList(relName).AddLinkRelationItem(relation, cItem, RevOccurrenceType.Effective);
            }
            ArrayList relationBizItems = item.Iteration.LinkRelationSet.GetRelationBizItemList(relName).RelationBizItems;
            foreach (DERelationBizItem item2 in relationBizItems)
            {
                if (item2.Id == cItem.Id)
                {
                    return item2;
                }
            }
            return null;
        }

        public static DERelationBizItem AddNewRelItem(DEBusinessItem docItem, string relName, DEBusinessItem parentItem)
        {
            DERelationBizItem item = AddNewRelation(parentItem, relName, docItem);
            if (item == null)
            {
                return null;
            }
            item.Relation.SetAttrValue("DOCREV", docItem.RevNum + "." + docItem.IterNum);
            item.Relation.SetAttrValue("DOCCODE", docItem.Id);
            item.Relation.SetAttrValue("DOCNAME", docItem.Name);
            item.Relation.SetAttrValue("MTZS", 1);
            return item;
        }

        public static void BackPrint(DEBusinessItem item, string sm)
        {
            item.Iteration.SetAttrValue("TSSTATUS", "打印回退");
            string str = "";
            object attrValue = item.Iteration.GetAttrValue("FFSM");
            if (attrValue != null)
            {
                str = attrValue.ToString();
            }
            if (!string.IsNullOrEmpty(str))
            {
                str = str + "\r\n 打印回退：" + (string.IsNullOrEmpty(sm) ? ("(" + ClientData.LogonUser.Name + ")") : sm);
            }
            else
            {
                str = str + " 打印回退：" + (string.IsNullOrEmpty(sm) ? ("(" + ClientData.LogonUser.Name + ")") : sm);
            }
            item.Iteration.SetAttrValue("FFSM", str);
            DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(item, ConstAm.TDSBOM_RELCLASS);
            foreach (DERelationBizItem item2 in relListOfDEBizItem.RelationBizItems)
            {
                if (GetTsStatue(item2) != "未打印")
                {
                    item2.Relation.SetAttrValue(ConstAm.TDSBOM_ATTR_TSSTATUE, "未打印");
                    if (!string.IsNullOrEmpty(sm))
                    {
                        item2.Relation.SetAttrValue("FFSM", sm);
                    }
                }
            }
        }

        public static void CancelPrint(DEBusinessItem item, string sm)
        {
            item.Iteration.SetAttrValue("TSSTATUS", "打印取消");
            string str = "";
            object attrValue = item.Iteration.GetAttrValue("FFSM");
            if (attrValue != null)
            {
                str = attrValue.ToString();
            }
            if (!string.IsNullOrEmpty(str))
            {
                str = str + "\r\n 打印取消：" + (string.IsNullOrEmpty(sm) ? ("(" + ClientData.LogonUser.Name + ")") : sm);
            }
            else
            {
                str = str + " 打印取消：" + (string.IsNullOrEmpty(sm) ? ("(" + ClientData.LogonUser.Name + ")") : sm);
            }
            item.Iteration.SetAttrValue("FFSM", str);
            DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(item, ConstAm.TDSBOM_RELCLASS);
            foreach (DERelationBizItem item2 in relListOfDEBizItem.RelationBizItems)
            {
                if (GetTsStatue(item2) != "已取消")
                {
                    item2.Relation.SetAttrValue(ConstAm.TDSBOM_ATTR_TSSTATUE, "已取消");
                    if (!string.IsNullOrEmpty(sm))
                    {
                        item2.Relation.SetAttrValue("FFSM", sm);
                    }
                }
            }
        }

        public static void CancelPrint(DERelationBizItem relItem, string sm)
        {
            relItem.Relation.SetAttrValue(ConstAm.TDSBOM_ATTR_TSSTATUE, "已取消");
            relItem.Relation.SetAttrValue("FFSM", sm);
        }

        internal static void CancelSent(DEBusinessItem theItem, string mark)
        {
            DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(theItem, ConstAm.SENTBOM_RELCLS);
            foreach (DERelationBizItem item in relListOfDEBizItem.RelationBizItems)
            {
                CancelSent(item, mark);
            }
            relListOfDEBizItem = GetRelListOfDEBizItem(theItem, ConstAm.SENTRBOM_RELCLS);
            foreach (DERelationBizItem item in relListOfDEBizItem.RelationBizItems)
            {
                CancelSent(item, mark);
            }
            string attrValue = (theItem.Iteration.GetAttrValue("SM") == null) ? "" : theItem.Iteration.GetAttrValue("SM").ToString();
            attrValue = attrValue + "\r\n取消收发：" + mark;
            theItem.Iteration.SetAttrValue("SM", attrValue);
            theItem.Iteration.SetAttrValue(ConstAm.SENT_ATTR_SENTSTATUS, "已取消");
            if ((theItem.State == ItemState.CheckOut) && (theItem.Holder == ClientData.LogonUser.Oid))
            {
                theItem.Iteration = PLItem.UpdateItemIteration(theItem.Iteration, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption);
                theItem = PLItem.Agent.CheckIn(theItem.MasterOid, theItem.ClassName, ClientData.LogonUser.Oid, "取消收发");
            }
            else
            {
                theItem.Iteration = PLItem.UpdateItemIterationDirectly(theItem, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption, false);
            }
        }

        private static void CancelSent(DERelationBizItem relItem, string mark)
        {
            string str = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW).ToString();
            string attrs = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_JSR) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_JSR).ToString();
            string str3 = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFSM) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFSM).ToString();
            string str4 = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SENTSTATUS) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SENTSTATUS).ToString();
            ArrayList list = new ArrayList(str.Split(new char[] { ';' }));
            ArrayList lsts = new ArrayList();
            foreach (string str5 in list)
            {
                string unitName = GetUnitName(str5);
                string str7 = GetAttrByUnit(str4, unitName, false, out lsts, true);
                if (string.IsNullOrEmpty(str7) || (str7 == "未收发"))
                {
                    lsts.Add(unitName + "(已取消)");
                    str7 = ResetAttrs(lsts);
                    relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_SENTSTATUS, str7);
                    string str8 = GetAttrByUnit(str3, unitName, true, out lsts, true);
                    lsts.Add(unitName + "(" + mark + ")");
                    str3 = ResetAttrs(lsts);
                    relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_FFSM, str3);
                    string str9 = GetAttrByUnit(attrs, unitName, true, out lsts);
                    lsts.Add(unitName + "(" + ClientData.LogonUser.Name + ")");
                    attrs = ResetAttrs(lsts);
                    relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_JSR, attrs);
                }
            }
        }

        public static bool CheckItemCanPrintOrSent(bool bToPrint, DEBusinessItem item, out StringBuilder strErr)
        {
            string action = bToPrint ? "ToPrint" : "ToSent";
            string str2 = bToPrint ? "发送打印" : "直接回收";
            ArrayList lstItems = new ArrayList();
            lstItems.Add(item);
            Agent.CheckTsdRight(lstItems, action, out strErr, "托晒");
            if (strErr.Length > 0)
            {
                return false;
            }
            return true;
        }

        internal static void CheckPrintItem(DEBusinessItem item, out int iEnd, out int iUndo, out int iCancel)
        {
            iEnd = 0;
            iUndo = 0;
            iCancel = 0;
            DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(item, ConstAm.TDSBOM_RELCLASS);
            foreach (DERelationBizItem item2 in relListOfDEBizItem.RelationBizItems)
            {
                string str = "";
                object attrValue = item2.Relation.GetAttrValue(ConstAm.TDSBOM_ATTR_TSSTATUE);
                if (attrValue != null)
                {
                    str = attrValue.ToString();
                }
                if (str == "未打印")
                {
                    iUndo++;
                }
                if (str == "已打印")
                {
                    iEnd++;
                }
                if (str == "已取消")
                {
                    iCancel++;
                }
            }
        }

        private static string CheckSentRelItemRight(DERelationBizItem relItem, bool isEnd)
        {
            StringBuilder builder = new StringBuilder();
            string str = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW).ToString();
            string attrs = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFSM) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFSM).ToString();
            string str3 = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SENTSTATUS) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SENTSTATUS).ToString();
            string str4 = relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_TH).ToString();
            if (string.IsNullOrEmpty(str))
            {
                return (relItem.Id + "单位为空");
            }
            ArrayList list = new ArrayList(str.Split(new char[] { ';' }));
            foreach (string str5 in list)
            {
                ArrayList list2;
                int num;
                int num2;
                string unitName = GetUnitName(str5);
                string str7 = GetAttrByUnit(str3, unitName, false, out list2, true);
                string str8 = GetAttrByUnit(attrs, unitName, false, out list2, true);
                GetSentFs(relItem, unitName, out num, out num2);
                if (isEnd)
                {
                    if ((str7 == "未收发") || string.IsNullOrEmpty(str7))
                    {
                        builder.Append(unitName + " 状态为未收发,不能完成处理\t");
                    }
                    if ((str7 == "已收发") && ((num2 != num) && string.IsNullOrEmpty(str8)))
                    {
                        builder.Append(unitName + " 实际数量与预期数量不符，且没有说明原因\t");
                    }
                }
                else if (str7 == "已收发")
                {
                    builder.Append(unitName + " 已收发，无法取消\t");
                }
            }
            if (builder.Length > 0)
            {
                builder.Insert(0, str4 + ":");
            }
            return builder.ToString();
        }

        internal static StringBuilder CheckSentRight(DEBusinessItem item, bool isEnd)
        {
            string str;
            StringBuilder builder = new StringBuilder();
            DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(item, ConstAm.SENTBOM_RELCLS);
            foreach (DERelationBizItem item2 in relListOfDEBizItem.RelationBizItems)
            {
                str = CheckSentRelItemRight(item2, true);
                if (!string.IsNullOrEmpty(str))
                {
                    builder.Append("\r" + item2.Id + "\n" + str);
                }
            }
            relListOfDEBizItem = GetRelListOfDEBizItem(item, ConstAm.SENTRBOM_RELCLS);
            foreach (DERelationBizItem item2 in relListOfDEBizItem.RelationBizItems)
            {
                str = CheckSentRelItemRight(item2, false);
                if (!string.IsNullOrEmpty(str))
                {
                    builder.Append("\r" + item2.Id + "\n" + str);
                }
            }
            return builder;
        }

        internal static void CommitWorkItem(DEBusinessItem item)
        {
            BPMProcessor processor = new BPMProcessor();
            ArrayList processInstanceListByObject = processor.GetProcessInstanceListByObject(item.MasterOid, item.RevNum);
            if ((processInstanceListByObject != null) && (processInstanceListByObject.Count != 0))
            {
                ArrayList list2 = new ArrayList();
                DELProcessInsProperty property = null;
                foreach (DELProcessInsProperty property2 in processInstanceListByObject)
                {
                    if (!(property2.State != "Running"))
                    {
                        property = property2;
                        break;
                    }
                }
                if (property != null)
                {
                    DELBPMEntityList theActivityInstanceList = new DELBPMEntityList();
                    BPMClient client = new BPMClient();
                    processor.GetActivityInstancesList(item.Revision.Creator, property.ID, out theActivityInstanceList, false);
                    foreach (DELActivityInstance instance in theActivityInstanceList)
                    {
                        DELBPMEntityList theWorkItemList = new DELBPMEntityList();
                        processor.GetWorkItemList(item.Revision.Creator, instance.ID, out theWorkItemList, false);
                        if (instance.DState == "Activated")
                        {
                            foreach (DELWorkItem item2 in theWorkItemList)
                            {
                                if ((item2.State == "Running") || (item2.State == "Accepted"))
                                {
                                    client.CompleteWorkItem(item2);
                                }
                                else if (item2.State == "Assigned")
                                {
                                    client.CompleteWorkItem(item2);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Hashtable ConvertItem(Hashtable hsCols, DEBusinessItem item)
        {
            Hashtable hashtable = new Hashtable();
            IDictionaryEnumerator enumerator = hsCols.GetEnumerator();
            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                string attrName = enumerator.Key.ToString();
                string str2 = enumerator.Value.ToString();
                if (attrName == "ID")
                {
                    hashtable[str2] = item.Id;
                }
                else
                {
                    object attrValue = item.Iteration.GetAttrValue(attrName);
                    if (attrValue != null)
                    {
                        DEMetaAttribute attribute = ModelContext.MetaModel.GetAttribute(item.Master.ClassName, attrName);
                        if (attribute != null)
                        {
                            if ((attribute.DataType2 == PLMDataType.Guid) && (attribute.Label.IndexOf("人") != -1))
                            {
                                Guid userId = (Guid) attrValue;
                                if (!userId.Equals(Guid.Empty))
                                {
                                    DEUser userByOid = PLUser.Agent.GetUserByOid(userId);
                                    hashtable[str2] = (userByOid != null) ? userByOid.Name : "";
                                }
                                else
                                {
                                    hashtable[str2] = "";
                                }
                            }
                            else if (attribute.DataType2 == PLMDataType.DateTime)
                            {
                                hashtable[str2] = Convert.ToDateTime(attrValue).ToString("yyyy.MM.dd HH:mm");
                            }
                            else
                            {
                                hashtable[str2] = attrValue.ToString();
                            }
                        }
                    }
                }
            }
            return hashtable;
        }

        public static void DelteSentBomDw(DERelationBizItem relItem, string unit)
        {
            string str = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW).ToString();
            string attrs = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SINGNER) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SINGNER).ToString();
            string str3 = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SIGNTIME) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SIGNTIME).ToString();
            string str4 = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_JSR) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_JSR).ToString();
            string str5 = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFSM) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFSM).ToString();
            if (!(string.IsNullOrEmpty(str) || (str.IndexOf(unit + "(") == -1)))
            {
                ArrayList list;
                GetAttrByUnit(str, unit, true, out list);
                str = ResetAttrs(list);
                relItem.Relation.SetAttrValue("JSDW", str);
                GetAttrByUnit(attrs, unit, true, out list);
                attrs = ResetAttrs(list);
                relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_SINGNER, attrs);
                GetAttrByUnit(str3, unit, true, out list);
                str3 = ResetAttrs(list);
                relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_SIGNTIME, str3);
                GetAttrByUnit(str4, unit, true, out list);
                str4 = ResetAttrs(list);
                relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_JSR, str4);
                GetAttrByUnit(str5, unit, true, out list);
                str5 = ResetAttrs(list);
                relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_FFSM, str5);
            }
        }

        public static void DelteTsdDw(DERelationBizItem relItem, string unit)
        {
            string str = (relItem.Relation.GetAttrValue("JSDW") == null) ? "" : relItem.Relation.GetAttrValue("JSDW").ToString();
            if (!string.IsNullOrEmpty(str) && (str.IndexOf(unit + "(") != -1))
            {
                ArrayList list = new ArrayList(str.Split(new char[] { ';' }));
                string str2 = "";
                foreach (string str3 in list)
                {
                    if (str3.IndexOf(unit + "(") == 0)
                    {
                        str2 = str3;
                        break;
                    }
                }
                list.Remove(str2);
                str = "";
                foreach (string str3 in list)
                {
                    str = str + ";" + str3;
                }
                if (str.IndexOf(";") != -1)
                {
                    str = str.Substring(1);
                }
                relItem.Relation.SetAttrValue("JSDW", str);
            }
        }

        internal static void EndOrCancelSent(string doccode, string unit, string sm, string singuser, ArrayList relItemList, bool isEnd, out StringBuilder strErr, out StringBuilder strSuc)
        {
            strErr = new StringBuilder();
            strSuc = new StringBuilder();
            if (relItemList.Count != 0)
            {
                DERelationBizItem item = relItemList[0] as DERelationBizItem;
                bool flag = item.Relation.RelationName == ConstAm.SENTBOM_RELCLS;
                foreach (DERelationBizItem item2 in relItemList)
                {
                    bool flag2 = false;
                    if (string.IsNullOrEmpty(doccode) || (item2.Id == doccode))
                    {
                        string str = (item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW) == null) ? "" : item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW).ToString();
                        if (string.IsNullOrEmpty(str))
                        {
                            strErr.Append("\t" + item2.Id + "单位为空");
                        }
                        else
                        {
                            string attrs = (item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SINGNER) == null) ? "" : item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SINGNER).ToString();
                            string str3 = (item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SIGNTIME) == null) ? "" : item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SIGNTIME).ToString();
                            string str4 = (item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_JSR) == null) ? "" : item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_JSR).ToString();
                            string str5 = (item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SENTSTATUS) == null) ? "" : item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SENTSTATUS).ToString();
                            string str6 = (item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFSM) == null) ? "" : item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFSM).ToString();
                            string str7 = (item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFFS) == null) ? "" : item2.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFFS).ToString();
                            ArrayList lst = new ArrayList(str.Split(new char[] { ';' }));
                            ArrayList lsts = new ArrayList();
                            for (int i = 0; i < lst.Count; i++)
                            {
                                string unitName = GetUnitName(lst[i].ToString());
                                if (string.IsNullOrEmpty(unit) || (unitName == unit))
                                {
                                    int num2;
                                    int num3;
                                    GetSentFs(item2, unitName, out num3, out num2);
                                    string str9 = GetAttrByUnit(str5, unitName, false, out lsts, true);
                                    if ((!isEnd || (str9 != "已收发")) && (isEnd || (str9 != "已取消")))
                                    {
                                        string str10 = GetAttrByUnit(str6, unit, false, out lsts);
                                        if (string.IsNullOrEmpty(sm))
                                        {
                                            sm = str10;
                                        }
                                        if (num3 != num2)
                                        {
                                            if ((flag && isEnd) && (num2 == 0))
                                            {
                                                GetAttrByUnit(str7, unitName, true, out lsts);
                                                lsts.Add(string.Concat(new object[] { unitName, "(", num3, ")" }));
                                                str7 = ResetAttrs(lst);
                                                item2.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_FFFS, str7);
                                            }
                                            else if (!(isEnd || (num2 <= 0)))
                                            {
                                                GetAttrByUnit(str7, unitName, true, out lsts);
                                                lsts.Add(unitName + "(0)");
                                                str7 = ResetAttrs(lst);
                                                item2.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_FFFS, str7);
                                            }
                                            else if (string.IsNullOrEmpty(sm))
                                            {
                                                flag2 = true;
                                                strErr.Append("\r\t" + item2.Id + "单位[" + unitName + "]实际" + (isEnd ? "发放" : "回收") + "数量与预期不一致，没有填写原因");
                                                continue;
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(sm))
                                        {
                                            GetAttrByUnit(str6, unitName, true, out lsts);
                                            lsts.Add(unitName + "(" + sm + ")");
                                            str6 = ResetAttrs(lsts);
                                            item2.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_FFSM, str6);
                                        }
                                        GetAttrByUnit(attrs, unitName, true, out lsts);
                                        if (isEnd)
                                        {
                                            lsts.Add(unitName + "(" + singuser + ")");
                                        }
                                        attrs = ResetAttrs(lsts);
                                        item2.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_SINGNER, attrs);
                                        GetAttrByUnit(str3, unit, true, out lsts);
                                        if (isEnd)
                                        {
                                            lsts.Add(unitName + "(" + DateTime.Now.ToString("yyyy.MM.dd HH:mm") + ")");
                                        }
                                        str3 = ResetAttrs(lsts);
                                        item2.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_SIGNTIME, str3);
                                        GetAttrByUnit(str4, unit, true, out lsts);
                                        lsts.Add(unitName + "(" + ClientData.LogonUser.Name + ")");
                                        str4 = ResetAttrs(lsts);
                                        item2.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_JSR, str4);
                                        GetAttrByUnit(str5, unit, true, out lsts);
                                        lsts.Add(unitName + "(" + (isEnd ? "已收发" : "已取消") + ")");
                                        str5 = ResetAttrs(lsts);
                                        item2.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_SENTSTATUS, str5);
                                    }
                                }
                            }
                            if (!flag2)
                            {
                                strSuc.Append("\t" + item2.Id);
                            }
                        }
                    }
                }
            }
        }

        public static void EndPrint(DERelationBizItem relItem, string sm)
        {
            object attrValue = null;
            string str = "";
            attrValue = relItem.Relation.GetAttrValue(ConstAm.TDSBOM_ATTR_TSSTATUE);
            if (attrValue != null)
            {
                str = attrValue.ToString();
            }
            if ((string.IsNullOrEmpty(str) || (str == "未打印")) || (str == "已取消"))
            {
                relItem.Relation.SetAttrValue(ConstAm.TDSBOM_ATTR_TSSTATUE, "已打印");
                relItem.Relation.SetAttrValue(ConstAm.TDSBOM_ATTR_PRINTER, ClientData.LogonUser.Name);
                relItem.Relation.SetAttrValue(ConstAm.TDSBOM_ATTR_PRINTERTIME, DateTime.Now);
                relItem.Relation.SetAttrValue("FFSM", sm);
            }
        }

        public static void EndPrint(DEBusinessItem item, string sm, bool isPowerPrintBomAll)
        {
            string str = "";
            DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(item, ConstAm.TDSBOM_RELCLASS);
            object attrValue = null;
            attrValue = item.Iteration.GetAttrValue("FFSM");
            if (attrValue != null)
            {
                str = attrValue.ToString();
            }
            foreach (DERelationBizItem item2 in relListOfDEBizItem.RelationBizItems)
            {
                string str2 = "";
                attrValue = item2.Relation.GetAttrValue(ConstAm.TDSBOM_ATTR_TSSTATUE);
                if (attrValue != null)
                {
                    str2 = attrValue.ToString();
                }
                if ((string.IsNullOrEmpty(str2) || (str2 == "未打印")) || ((str2 == "已取消") && isPowerPrintBomAll))
                {
                    item2.Relation.SetAttrValue(ConstAm.TDSBOM_ATTR_TSSTATUE, "已打印");
                    item2.Relation.SetAttrValue(ConstAm.TDSBOM_ATTR_PRINTER, ClientData.LogonUser.Name);
                    item2.Relation.SetAttrValue(ConstAm.TDSBOM_ATTR_PRINTERTIME, DateTime.Now);
                    if (!string.IsNullOrEmpty(sm))
                    {
                        item2.Relation.SetAttrValue("FFSM", sm);
                    }
                }
            }
            if (!string.IsNullOrEmpty(str))
            {
                str = str + "\r\n 打印完成：" + sm;
            }
            else
            {
                str = str + " 打印完成：" + sm;
            }
            item.Iteration.SetAttrValue("FFSM", str);
            item.Iteration.SetAttrValue("PRINTER", ClientData.LogonUser.Name);
            item.Iteration.SetAttrValue("PRINTTIME", DateTime.Now);
        }

        public static void EndSent(DEBusinessItem theItem, string mark, out StringBuilder strErr)
        {
            strErr = CheckSentRight(theItem, true);
            if (strErr.Length <= 0)
            {
                string attrValue = (theItem.Iteration.GetAttrValue("SM") == null) ? "" : theItem.Iteration.GetAttrValue("SM").ToString();
                attrValue = attrValue + "\r\n收发结束：" + mark;
                theItem.Iteration.SetAttrValue("SM", attrValue);
                theItem.Iteration.SetAttrValue(ConstAm.SENT_ATTR_SENTSTATUS, "已收发");
                ResetRealfsOfSent(theItem);
                if ((theItem.State == ItemState.CheckOut) && (theItem.Holder == ClientData.LogonUser.Oid))
                {
                    theItem.Iteration = PLItem.UpdateItemIteration(theItem.Iteration, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption);
                    theItem = PLItem.Agent.CheckIn(theItem.MasterOid, theItem.ClassName, ClientData.LogonUser.Oid, "结束收发");
                }
                else
                {
                    theItem.Iteration = PLItem.UpdateItemIterationDirectly(theItem, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption, false);
                }
                theItem = PLItem.Agent.Release(theItem.MasterOid, theItem.ClassName, ClientData.LogonUser.Oid, "结束收发");
                if (BizItemHandlerEvent.Instance.D_AfterReleased != null)
                {
                    BizItemHandlerEvent.Instance.D_AfterReleased(BizOperationHelper.ConvertPLMBizItemDelegateParam(theItem));
                }
            }
        }

        internal static ArrayList FindTSDByDocCode(string docId, ListView lvwTsd, ArrayList lstSchTsd)
        {
            lvwTsd.Items.Clear();
            CompareTsd comparer = new CompareTsd();
            ArrayList list = new ArrayList();
            if (string.IsNullOrWhiteSpace(docId))
            {
                return lstSchTsd;
            }
            foreach (DEBusinessItem item in lstSchTsd)
            {
                string str = (item.Iteration.GetAttrValue("DOCCODE") == null) ? "" : item.Iteration.GetAttrValue("DOCCODE").ToString();
                if (str.IndexOf(docId.ToUpper()) != -1)
                {
                    list.Add(item);
                }
                else
                {
                    DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(item, ConstAm.TDSBOM_RELCLASS);
                    if (relListOfDEBizItem != null)
                    {
                        foreach (IBizItem item2 in relListOfDEBizItem.BizItems)
                        {
                            if (item2.Id.IndexOf(docId.ToUpper()) != -1)
                            {
                                list.Add(item);
                                break;
                            }
                        }
                    }
                }
            }
            list.Sort(comparer);
            return list;
        }

        internal static ArrayList GetAllUnitByQuickSign(ArrayList lstPrintItems)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < lstPrintItems.Count; i++)
            {
                DEBusinessItem item = lstPrintItems[i] as DEBusinessItem;
                DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(item, ConstAm.SENTBOM_RELCLS);
                bool flag = false;
                foreach (DERelationBizItem item2 in relListOfDEBizItem.RelationBizItems)
                {
                    string getRelAttrValue = GetGetRelAttrValue(item2, ConstAm.SENTBOM_ATTR_FFDW);
                    if (!string.IsNullOrEmpty(getRelAttrValue))
                    {
                        flag = true;
                        ArrayList list3 = new ArrayList(getRelAttrValue.Split(new char[] { ';' }));
                        foreach (string str2 in list3)
                        {
                            string unitName = GetUnitName(str2);
                            if (!list.Contains(unitName))
                            {
                                list.Add(unitName);
                            }
                        }
                    }
                }
                if (!flag)
                {
                    lstPrintItems.Remove(item);
                    i--;
                }
            }
            list.Sort();
            return list;
        }

        private static string GetAttrByUnit(string attrs, string unit, bool isDel, out ArrayList lsts)
        {
            string[] c = attrs.Split(new char[] { ';' });
            lsts = new ArrayList(c);
            string str = "";
            foreach (string str2 in lsts)
            {
                if (str2.IndexOf(unit + "(") == 0)
                {
                    str = str2;
                    break;
                }
            }
            if (isDel)
            {
                lsts.Remove(str);
            }
            return str;
        }

        private static string GetAttrByUnit(string attrs, string unit, bool isDel, out ArrayList lsts, bool isOnlycontext)
        {
            string str = GetAttrByUnit(attrs, unit, isDel, out lsts);
            if (!(string.IsNullOrEmpty(str) || !isOnlycontext))
            {
                str = str.Replace(unit, "").Replace("(", "").Replace(")", "");
            }
            return str;
        }

        private static string GetGetAttrValue(DEBusinessItem item, string attrName)
        {
            return ((item.Iteration.GetAttrValue(attrName) == null) ? "" : item.Iteration.GetAttrValue(attrName).ToString());
        }

        private static string GetGetRelAttrValue(DERelationBizItem relItem, string attrName)
        {
            return ((relItem.Relation.GetAttrValue(attrName) == null) ? "" : relItem.Relation.GetAttrValue(attrName).ToString());
        }

        private static Hashtable GetHsRes()
        {
            if (_hsRes == null)
            {
                _hsRes = new Hashtable();
                DataSet tsRes = Agent.GetTsRes();
                if (tsRes != null)
                {
                    foreach (DataTable table in tsRes.Tables)
                    {
                        _hsRes[table.TableName] = table;
                    }
                }
            }
            return _hsRes;
        }

        public static DEBusinessItem GetItem(object obj)
        {
            DEBusinessItem bizItem = null;
            if (obj is DEBusinessItem)
            {
                return (DEBusinessItem) obj;
            }
            if (obj is DESmartBizItem)
            {
                return PSConvert.ToBizItem((IBizItem) obj, ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid);
            }
            if (obj is DERelationBizItem)
            {
                bizItem = ((DERelationBizItem) obj).BizItem;
            }
            return bizItem;
        }

        public static ListViewItem GetListViewItem(ListView lv, string docId, string unit, bool isHs)
        {
            string str = isHs ? "回收单位" : "接收单位";
            foreach (ListViewItem item in lv.Items)
            {
                DERelationBizItem tag = item.Tag as DERelationBizItem;
                if ((tag.Id == docId) && (item.SubItems[lv.Columns[str].Index].Text == unit))
                {
                    return item;
                }
            }
            return null;
        }

        internal static DEBusinessItem GetNewItem(string clsName)
        {
            DEBusinessItem item = PLItem.CreateBizItem(PLItem.Agent.AutoGenerateID(clsName), clsName, 1, "1", ClientData.UserGlobalOption, ClientData.LogonUser.Oid);
            if (clsName == "DQDOSSIERPRINT")
            {
                item.Iteration.SetAttrValue("YCT", "一次图");
                item.Iteration.SetAttrValue("TSSTATUS", "未发打印");
                item.Iteration.SetAttrValue("TSTYPE", "新发");
                item.Iteration.SetAttrValue("ZS", 0);
                item.Iteration.SetAttrValue("FS", 0);
                DateTime time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                item.Iteration.SetAttrValue("YQWCSJ", time.AddDays(7.0));
            }
            return item;
        }

        private static int GetPossibleOrder(DERelationBizItemList ri)
        {
            if (ri == null)
            {
                return 1;
            }
            int order = 0;
            foreach (DERelation2 relation in ri.RelationList)
            {
                if (relation.Order > order)
                {
                    order = relation.Order;
                }
            }
            return (order + 1);
        }

        public static DELProcessInsProperty GetProcessItem(Guid moid, int revNum)
        {
            ArrayList processInstanceListByObject = new BPMProcessor().GetProcessInstanceListByObject(moid, revNum);
            ArrayList list2 = new ArrayList();
            DateTime minValue = DateTime.MinValue;
            if ((processInstanceListByObject != null) && (processInstanceListByObject.Count > 0))
            {
                foreach (DELProcessInsProperty property in processInstanceListByObject)
                {
                    if (((property.State != "Aborted") && (property.State != "Closed")) && (property.State != "Deleted"))
                    {
                        if (property.CreationDate > minValue)
                        {
                            property.TerminationDate = property.CreationDate;
                            list2.Insert(0, property);
                        }
                        else
                        {
                            list2.Add(property);
                        }
                    }
                }
            }
            if (list2.Count > 0)
            {
                return (DELProcessInsProperty) list2[0];
            }
            return null;
        }

        private static ArrayList GetQuickSentRelItems(DEBusinessItem item, string unit)
        {
            DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(item, ConstAm.SENTBOM_RELCLS);
            ArrayList list2 = new ArrayList();
            foreach (DERelationBizItem item2 in relListOfDEBizItem.RelationBizItems)
            {
                bool flag = false;
                string getRelAttrValue = GetGetRelAttrValue(item2, ConstAm.SENTBOM_ATTR_FFDW);
                if (!string.IsNullOrEmpty(getRelAttrValue))
                {
                    ArrayList list3 = new ArrayList(getRelAttrValue.Split(new char[] { ';' }));
                    foreach (string str2 in list3)
                    {
                        if (GetUnitName(str2) == unit)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        ArrayList list4;
                        string str3 = GetAttrByUnit(GetGetRelAttrValue(item2, ConstAm.SENTBOM_ATTR_SENTSTATUS), unit, false, out list4, true);
                        if (string.IsNullOrEmpty(str3) || (str3 == "未收发"))
                        {
                            list2.Add(item2);
                        }
                    }
                }
            }
            return list2;
        }

        public static DERelationBizItem GetRelItemById(string id, DEBusinessItem item, string relName)
        {
            DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(item, relName);
            foreach (DERelationBizItem item2 in relListOfDEBizItem.RelationBizItems)
            {
                if (item2.Id == id)
                {
                    return item2;
                }
            }
            return null;
        }

        internal static ArrayList GetRelItems(ListView lv)
        {
            ArrayList list = new ArrayList();
            foreach (ListViewItem item in lv.Items)
            {
                DERelationBizItem tag = item.Tag as DERelationBizItem;
                if (tag != null)
                {
                    list.Add(tag);
                }
            }
            return list;
        }

        public static DERelationBizItemList GetRelListOfDEBizItem(DEBusinessItem item, string relName)
        {
            DERelationBizItemList relationBizItemList = item.Iteration.LinkRelationSet.GetRelationBizItemList(relName);
            if (relationBizItemList == null)
            {
                relationBizItemList = PLItem.Agent.GetLinkRelationItems(item.IterOid, item.ClassName, relName, ClientData.LogonUser.Oid, ClientData.UserGlobalOption);
                item.Iteration.LinkRelationSet.AddRelationList(relName, relationBizItemList);
            }
            return relationBizItemList;
        }

        public static int GetSedNum(string dw, string ftly)
        {
            if (dsSecondNum == null)
            {
                dsSecondNum = _agent.GetSecondDocStandard();
            }
            DataTable table = dsSecondNum.Tables["DW"];
            DataTable table2 = dsSecondNum.Tables["NUM"];
            DataRow[] rowArray = table.Select(" DWNAME = '" + dw + "'");
            DataRow[] rowArray2 = table2.Select("PLM_ID ='" + ftly + "'");
            if ((rowArray.Length != 0) && (rowArray2.Length != 0))
            {
                foreach (DataRow row in rowArray)
                {
                    string str = row["DWID"].ToString().Replace("(", "").Replace(")", "");
                    string name = "PLM_" + str;
                    DataRow row2 = rowArray2[0];
                    if (table2.Columns.Contains(name))
                    {
                        object obj2 = row2[name];
                        if (obj2 != DBNull.Value)
                        {
                            try
                            {
                                return Convert.ToInt32(obj2);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            return 0;
        }

        public static void GetSentBomAllFs(DERelationBizItem relItem, out int yqfs, out int mtzs, out int realfs)
        {
            yqfs = 0;
            realfs = 0;
            string str = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW).ToString();
            mtzs = (relItem.Relation.GetAttrValue("MTZS") == null) ? 0 : Convert.ToInt32(relItem.Relation.GetAttrValue("MTZS"));
            string str2 = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFFS) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFFS).ToString();
            string attrs = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SENTSTATUS) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SENTSTATUS).ToString();
            if (!string.IsNullOrEmpty(str))
            {
                string str4 = "";
                string[] strArray = str.Split(new char[] { ';' });
                foreach (string str5 in strArray)
                {
                    ArrayList list;
                    string unit = str5.Substring(0, str5.LastIndexOf("("));
                    str4 = GetAttrByUnit(str, unit, false, out list, true);
                    yqfs += Convert.ToInt32(str4);
                    if (!string.IsNullOrEmpty(str2) && (GetAttrByUnit(attrs, unit, false, out list, true) == "已收发"))
                    {
                        str4 = GetAttrByUnit(str2, unit, false, out list, true);
                        if (!string.IsNullOrEmpty(str4))
                        {
                            realfs += Convert.ToInt32(str4);
                        }
                    }
                }
            }
        }

        private static void GetSentFs(DERelationBizItem relItem, string unit, out int yqfs, out int realfs)
        {
            yqfs = 0;
            realfs = 0;
            string str = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW).ToString();
            if (!string.IsNullOrEmpty(str))
            {
                ArrayList list;
                string attrs = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFFS) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFFS).ToString();
                string str3 = GetAttrByUnit(str, unit, false, out list, true);
                if (!string.IsNullOrEmpty(str3))
                {
                    yqfs = Convert.ToInt32(str3);
                    str3 = GetAttrByUnit(attrs, unit, false, out list, true);
                    if (!string.IsNullOrEmpty(str3))
                    {
                        realfs = Convert.ToInt32(str3);
                    }
                }
            }
        }

        public static int GetTsdBomAllFs(DERelationBizItem relItem)
        {
            string str = (relItem.Relation.GetAttrValue("JSDW") == null) ? "" : relItem.Relation.GetAttrValue("JSDW").ToString();
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }
            string str2 = "";
            string[] strArray = str.Split(new char[] { ';' });
            int num = 0;
            foreach (string str3 in strArray)
            {
                str2 = str3.Substring(str3.LastIndexOf("(") + 1).Replace(")", "");
                num += Convert.ToInt32(str2);
            }
            return num;
        }

        public static int GetTsdBomFs(DERelationBizItem relItem, string unit)
        {
            string str = (relItem.Relation.GetAttrValue("JSDW") == null) ? "" : relItem.Relation.GetAttrValue("JSDW").ToString();
            if (!string.IsNullOrEmpty(str))
            {
                string str2 = "";
                string[] strArray = str.Split(new char[] { ';' });
                foreach (string str3 in strArray)
                {
                    if (str3.IndexOf(unit + "(") == 0)
                    {
                        str2 = str3.Substring(str3.LastIndexOf("(") + 1).Replace(")", "");
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(str2))
                {
                    return Convert.ToInt32(str2);
                }
            }
            return 0;
        }

        internal static object GetTsRes(string lb)
        {
            if (_hsRes == null)
            {
                _hsRes = GetHsRes();
            }
            if (_hsRes.ContainsKey(lb))
            {
                string str;
                DataTable table = _hsRes[lb] as DataTable;
                string str2 = lb;
                if ((str2 != null) && (str2 == "路线部门"))
                {
                    ArrayList list = new ArrayList();
                    foreach (DataRow row in table.Rows)
                    {
                        str = (row["PLM_NAME"] == DBNull.Value) ? "" : row["PLM_NAME"].ToString();
                        if (!(string.IsNullOrWhiteSpace(str) || list.Contains(str)))
                        {
                            list.Add(str);
                        }
                    }
                    list.Sort();
                    return list;
                }
                ArrayList list2 = new ArrayList();
                foreach (DataRow row in table.Rows)
                {
                    str = (row["PLM_ID"] == DBNull.Value) ? "" : row["PLM_ID"].ToString();
                    if (!(string.IsNullOrWhiteSpace(str) || list2.Contains(str)))
                    {
                        list2.Add(str);
                    }
                }
                list2.Sort();
                return list2;
            }
            return null;
        }

        public static string GetTsStatue(object obj)
        {
            object attrValue;
            DEBusinessItem item = obj as DEBusinessItem;
            if (item != null)
            {
                attrValue = item.Iteration.GetAttrValue("TSSTATUS");
                if (attrValue == null)
                {
                    return "";
                }
                return attrValue.ToString();
            }
            DERelationBizItem item2 = obj as DERelationBizItem;
            if (item2 == null)
            {
                return "";
            }
            attrValue = item2.Relation.GetAttrValue(ConstAm.TDSBOM_ATTR_TSSTATUE);
            if (attrValue == null)
            {
                return "";
            }
            return attrValue.ToString();
        }

        public static string GetUnitName(string unit)
        {
            int length = unit.LastIndexOf("(");
            if (length != -1)
            {
                return unit.Substring(0, length);
            }
            return "";
        }

        public static bool IsUnSent(DEBusinessItem item, string unit)
        {
            DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(item, ConstAm.SENTBOM_RELCLS);
            foreach (DERelationBizItem item2 in relListOfDEBizItem.RelationBizItems)
            {
                bool flag = false;
                string getRelAttrValue = GetGetRelAttrValue(item2, ConstAm.SENTBOM_ATTR_FFDW);
                if (!string.IsNullOrEmpty(getRelAttrValue))
                {
                    ArrayList list2 = new ArrayList(getRelAttrValue.Split(new char[] { ';' }));
                    foreach (string str2 in list2)
                    {
                        if (GetUnitName(str2) == unit)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        ArrayList list3;
                        string str3 = GetAttrByUnit(GetGetRelAttrValue(item2, ConstAm.SENTBOM_ATTR_SENTSTATUS), unit, false, out list3, true);
                        if (string.IsNullOrEmpty(str3) || (str3 == "未收发"))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static DataSet LstBpmInfo(bool isGetNew)
        {
            ArrayList list = new ArrayList();
            if (isGetNew || (dsBpm == null))
            {
                dsBpm = Agent.GetBpmNameByUserOid(ClientData.LogonUser.Oid);
            }
            return dsBpm;
        }

        internal static void QuickSign(DEBusinessItem item, string unit, string signer, string sm, out StringBuilder strErr)
        {
            ArrayList quickSentRelItems = GetQuickSentRelItems(item, unit);
            strErr = new StringBuilder();
            if (quickSentRelItems.Count == 0)
            {
                strErr.Append(unit + "不需要签收或已经签收");
            }
            else
            {
                StringBuilder builder;
                StringBuilder builder2;
                EndOrCancelSent("", unit, sm, signer, quickSentRelItems, true, out builder, out builder2);
                if (builder.Length > 0)
                {
                    strErr.Append(builder.ToString());
                }
                else
                {
                    ResetRealfsOfSent(item);
                }
            }
        }

        public static void RePrint(DEBusinessItem item)
        {
            item.Iteration.SetAttrValue("TSSTATUS", "开始打印");
            string attrValue = "";
            object obj2 = item.Iteration.GetAttrValue("FFSM");
            if (obj2 != null)
            {
                attrValue = obj2.ToString();
                int index = attrValue.IndexOf("打印取消：");
                if (index != -1)
                {
                    attrValue = attrValue.Substring(0, index);
                    item.Iteration.SetAttrValue("FFSM", attrValue);
                }
            }
            DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(item, ConstAm.TDSBOM_RELCLASS);
            foreach (DERelationBizItem item2 in relListOfDEBizItem.RelationBizItems)
            {
                item2.Relation.SetAttrValue(ConstAm.TDSBOM_ATTR_TSSTATUE, "未打印");
                item2.Relation.SetAttrValue("FFSM", "");
            }
        }

        private static string ResetAttrs(ArrayList lst)
        {
            if (lst.Count == 0)
            {
                return "";
            }
            lst.Sort();
            string str = "";
            foreach (string str2 in lst)
            {
                str = str + ";" + str2;
            }
            return str.Substring(1);
        }

        internal static void ResetRealfsOfSent(DEBusinessItem sentItemItem)
        {
            int num;
            int num2;
            int num3;
            int attrValue = 0;
            DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(sentItemItem, ConstAm.SENTBOM_RELCLS);
            if (relListOfDEBizItem.Count > 0)
            {
                foreach (DERelationBizItem item in relListOfDEBizItem.RelationBizItems)
                {
                    if (item.Relation.State != RelationState.Deleted)
                    {
                        GetSentBomAllFs(item, out num, out num2, out num3);
                        item.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_REALFS, num3);
                        item.Relation.SetAttrValue("FS", num3);
                        attrValue += num3;
                    }
                }
                sentItemItem.Iteration.SetAttrValue("REALFFFS", attrValue);
            }
            if (GetRelListOfDEBizItem(sentItemItem, ConstAm.SENTRBOM_RELCLS).Count > 0)
            {
                attrValue = 0;
                foreach (DERelationBizItem item in relListOfDEBizItem.RelationBizItems)
                {
                    if (item.Relation.State != RelationState.Deleted)
                    {
                        GetSentBomAllFs(item, out num, out num2, out num3);
                        item.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_REALFS, num3);
                        item.Relation.SetAttrValue("FS", num3);
                        attrValue += num3;
                    }
                }
                sentItemItem.Iteration.SetAttrValue("REALHSFS", attrValue);
            }
        }

        public static void ResetZsAndFsOfTsd(DEBusinessItem tsdItem)
        {
            DERelationBizItemList relListOfDEBizItem = GetRelListOfDEBizItem(tsdItem, ConstAm.TDSBOM_RELCLASS);
            int attrValue = 0;
            int num2 = 0;
            int num3 = 0;
            if (relListOfDEBizItem.Count != 0)
            {
                foreach (DERelationBizItem item in relListOfDEBizItem.RelationBizItems)
                {
                    if (item.Relation.State != RelationState.Deleted)
                    {
                        num3 += (item.Relation.GetAttrValue(ConstAm.TDS_ATTR_MTZS) == null) ? 0 : ((int) item.Relation.GetAttrValue(ConstAm.TDS_ATTR_MTZS));
                        attrValue += GetTsdBomAllFs(item);
                        if (item.Relation.GetAttrValue("ZS") == null)
                        {
                            ResetZSofTdsBom(item);
                        }
                        object obj2 = item.Relation.GetAttrValue("ZS");
                        if (obj2 != null)
                        {
                            num2 += Convert.ToInt32(obj2);
                        }
                    }
                }
                tsdItem.Iteration.SetAttrValue("FS", attrValue);
                tsdItem.Iteration.SetAttrValue("ZS", num2);
                tsdItem.Iteration.SetAttrValue(ConstAm.TDS_ATTR_MTZS, num3);
            }
        }

        public static void ResetZSofTdsBom(DERelationBizItem relItem)
        {
            int tsdBomAllFs = GetTsdBomAllFs(relItem);
            relItem.Relation.SetAttrValue("FS", tsdBomAllFs);
            int num2 = (relItem.Relation.GetAttrValue("MTZS") == null) ? 1 : Convert.ToInt32(relItem.Relation.GetAttrValue("MTZS"));
            relItem.Relation.SetAttrValue("ZS", tsdBomAllFs * num2);
        }

        public static void SetBpmInfo(ResWkInfo wk, bool isNew)
        {
            DataSet ds = LstBpmInfo(isNew);
            wk.SetDataSource(ds);
        }

        public static void SetCol(Hashtable hsCols, ListView lvw, string tp, ArrayList lstOrd, Hashtable hsColWide)
        {
            List<string> list = new List<string> { "MOID", "ROID", "IROID", "RELOID" };
            foreach (string str in lstOrd)
            {
                int width = hsColWide.Contains(str) ? Convert.ToInt32(hsColWide[str]) : 100;
                lvw.Columns.Add(str, str, width);
            }
        }

        public static void SetComBoxItem(string lb, ComboBox ctrl, string defValue, bool isprint)
        {
            ArrayList tsRes = GetTsRes(lb) as ArrayList;
            if (tsRes != null)
            {
                tsRes.Sort();
                foreach (string str in tsRes)
                {
                    if (!(isprint && ((!(lb != "托晒方式") && !(str != "回收")) || ((!(lb != "托晒打印状态") && !(str != "未发打印")) || !(str != "直接回收")))))
                    {
                        ctrl.Items.Add(str);
                    }
                }
                ctrl.Items.Insert(0, "");
                if (!(string.IsNullOrEmpty(defValue) || !tsRes.Contains(defValue)))
                {
                    ctrl.Text = defValue;
                }
            }
        }

        public static void SetLvwClsValues(Hashtable hsCols, ListView lvw, ArrayList lstOrd, DataTable tb, string clsName)
        {
            lvw.Items.Clear();
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            Hashtable hashtable = new Hashtable();
            IDictionaryEnumerator enumerator = hsCols.GetEnumerator();
            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                list2.Add(enumerator.Key);
                list.Add(enumerator.Value);
                hashtable[enumerator.Value] = enumerator.Key;
            }
            List<string> list3 = new List<string> { "MOID", "ROID", "IROID", "RELOID" };
            foreach (DataRow row in tb.Rows)
            {
                int num = 0;
                ListViewItem item = new ListViewItem();
                string text = "";
                Guid masterOid = new Guid((byte[]) row["MOID"]);
                Guid revOid = new Guid((byte[]) row["ROID"]);
                Guid iterOid = new Guid((byte[]) row["IROID"]);
                foreach (string str2 in lstOrd)
                {
                    if (tb.Columns.Contains(str2))
                    {
                        string attrName = (hashtable[str2] == null) ? "" : hashtable[str2].ToString();
                        DEMetaAttribute attribute = ModelContext.MetaModel.GetAttribute(clsName, attrName);
                        text = "";
                        if (row[str2] == DBNull.Value)
                        {
                            text = "";
                        }
                        else if ((tb.Columns[str2].DataType == typeof(byte[])) && (str2.IndexOf("人") != -1))
                        {
                            Guid userId = new Guid((byte[]) row[str2]);
                            if (userId.Equals(Guid.Empty))
                            {
                                text = "";
                            }
                            else
                            {
                                DEUser userByOid = PLUser.Agent.GetUserByOid(userId);
                                if (userByOid != null)
                                {
                                    text = userByOid.Name;
                                }
                            }
                        }
                        else if (tb.Columns[str2].DataType == typeof(DateTime))
                        {
                            text = Convert.ToDateTime(row[str2]).ToString("yyyy.MM.dd HH:mm");
                        }
                        else if (attribute == null)
                        {
                            text = row[str2].ToString();
                        }
                        else if (attribute.DataType2 == PLMDataType.Bool)
                        {
                            text = (row[str2] == DBNull.Value) ? "" : (((row[str2].ToString().ToUpper() == "True".ToUpper()) || (row[str2].ToString().ToUpper() == "Y")) ? "是" : "否");
                        }
                        else
                        {
                            text = row[str2].ToString();
                        }
                        if (num == 0)
                        {
                            item.Text = text;
                        }
                        else
                        {
                            item.SubItems.Add(text);
                        }
                        num++;
                    }
                }
                if (!masterOid.Equals(Guid.Empty))
                {
                    DEBusinessItem item2 = PLItem.Agent.GetBizItem(masterOid, revOid, iterOid, ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid, BizItemMode.BizItem) as DEBusinessItem;
                    if (item2 != null)
                    {
                        item.Tag = item2;
                        lvw.Items.Add(item);
                    }
                }
            }
        }

        private static void SetLvwRelValues(ListView lv, ArrayList lstOrder, DERelationBizItem dItem, ArrayList lstRleAttrs, string units)
        {
            ListViewItem item = new ListViewItem {
                Tag = dItem
            };
            int num = 0;
            string str = "";
            string str2 = "";
            if (!string.IsNullOrEmpty(units))
            {
                units = units.Replace("（", "(").Replace("）", ")");
                str = units.Substring(0, units.LastIndexOf("("));
                str2 = units.Substring(units.LastIndexOf("(") + 1).Replace(")", "");
            }
            foreach (string str3 in lstOrder)
            {
                string str4 = "";
                foreach (DEMetaAttribute attribute in lstRleAttrs)
                {
                    if (attribute.Label == str3)
                    {
                        object obj2 = dItem.Relation.AttrValueExists(attribute.Name) ? dItem.Relation.GetAttrValue(attribute.Name) : null;
                        str4 = (obj2 == null) ? "" : obj2.ToString();
                        if (!string.IsNullOrEmpty(str4))
                        {
                            if ((attribute.DataType2 == PLMDataType.Guid) && (attribute.Label.IndexOf("人") != -1))
                            {
                                Guid g = (Guid) obj2;
                                if (!Guid.Empty.Equals(g))
                                {
                                    DEUser userByOid = PLUser.Agent.GetUserByOid(g);
                                    str4 = (userByOid != null) ? userByOid.Name : "";
                                }
                                else
                                {
                                    str4 = "";
                                }
                            }
                            if (attribute.DataType2 == PLMDataType.DateTime)
                            {
                                str4 = Convert.ToDateTime(str4).ToString("yyyy.MM.dd hh:mm");
                            }
                        }
                        if ((attribute.Name == ConstAm.TDS_ATTR_MTZS) && string.IsNullOrEmpty(str4))
                        {
                            str4 = "1";
                            dItem.Relation.SetAttrValue(ConstAm.TDS_ATTR_MTZS, 1);
                        }
                        if (!string.IsNullOrEmpty(units))
                        {
                            if (attribute.Name == "FS")
                            {
                                str4 = str2;
                            }
                            if (attribute.Name == "JSDW")
                            {
                                str4 = str;
                            }
                        }
                        break;
                    }
                }
                if (num == 0)
                {
                    item.Text = str4;
                }
                else if (lv.Columns.ContainsKey(str3))
                {
                    item.SubItems.Add(str4);
                }
                num++;
            }
            lv.Items.Add(item);
        }

        public static void SetLvwSentRelValues(ListView lv, ArrayList lstOrder, DERelationBizItem relitem, ArrayList lstRleAttrs, string units, bool isRedo)
        {
            ListViewItem item = new ListViewItem {
                Tag = relitem
            };
            int num = 0;
            string str = "";
            string unit = units.Substring(0, units.LastIndexOf("("));
            string attrs = (relitem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW) == null) ? "" : relitem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW).ToString();
            foreach (string str4 in lstOrder)
            {
                foreach (DEMetaAttribute attribute in lstRleAttrs)
                {
                    if (attribute.Label == str4)
                    {
                        object attrValue = relitem.Relation.GetAttrValue(attribute.Name);
                        str = (attrValue == null) ? "" : attrValue.ToString();
                        if ((attribute.DataType2 == PLMDataType.Guid) && (attribute.Label.IndexOf("人") != -1))
                        {
                            Guid g = (Guid) attrValue;
                            if (!Guid.Empty.Equals(g))
                            {
                                DEUser userByOid = PLUser.Agent.GetUserByOid(g);
                                str = (userByOid != null) ? userByOid.Name : "";
                            }
                            else
                            {
                                str = "";
                            }
                        }
                        else if (attribute.DataType2 == PLMDataType.DateTime)
                        {
                            str = Convert.ToDateTime(str).ToString("yyyy.MM.dd hh:mm");
                        }
                        if (!string.IsNullOrEmpty(units))
                        {
                            ArrayList list;
                            if (attribute.Name == "FS")
                            {
                                str = GetAttrByUnit(attrs, unit, false, out list, true);
                            }
                            if (attribute.Name == ConstAm.SENTBOM_ATTR_FFDW)
                            {
                                str = unit;
                            }
                            if (attribute.Name == ConstAm.SENTBOM_ATTR_FFFS)
                            {
                                str = GetAttrByUnit(str, unit, false, out list, true);
                            }
                            if (attribute.Name == ConstAm.SENTBOM_ATTR_SIGNTIME)
                            {
                                str = GetAttrByUnit(str, unit, false, out list, true);
                            }
                            if (attribute.Name == ConstAm.SENTBOM_ATTR_SINGNER)
                            {
                                str = GetAttrByUnit(str, unit, false, out list, true);
                            }
                            if (attribute.Name == ConstAm.SENTBOM_ATTR_JSR)
                            {
                                str = GetAttrByUnit(str, unit, false, out list, true);
                            }
                            if (attribute.Name == ConstAm.SENTBOM_ATTR_FFSM)
                            {
                                str = GetAttrByUnit(str, unit, false, out list, true);
                            }
                            if (attribute.Name == ConstAm.SENTBOM_ATTR_SENTSTATUS)
                            {
                                str = GetAttrByUnit(str, unit, false, out list, true);
                            }
                        }
                    }
                }
                if (num == 0)
                {
                    item.Text = str;
                }
                else
                {
                    item.SubItems.Add(str);
                }
                num++;
            }
            lv.Items.Add(item);
        }

        public static void SetLvwValues(Hashtable hsCols, ListView lvw, ArrayList lstOrd, DEBusinessItem item)
        {
            Hashtable hashtable = ConvertItem(hsCols, item);
            ListViewItem item2 = new ListViewItem {
                Tag = item
            };
            int num = 0;
            foreach (string str in lstOrd)
            {
                if (lvw.Columns.ContainsKey(str))
                {
                    if (num == 0)
                    {
                        item2.Name = item2.Text = (hashtable[str] == null) ? "" : hashtable[str].ToString();
                    }
                    else
                    {
                        item2.SubItems.Add((hashtable[str] == null) ? "" : hashtable[str].ToString());
                    }
                    num++;
                }
            }
            lvw.Items.Add(item2);
        }

        public static void SetSigner(ComboBox cb)
        {
            cb.Items.Clear();
            cb.Text = "";
            foreach (string str in lstSigners)
            {
                cb.Items.Add(str);
            }
        }

        public static bool ToPrintOrSent(DEBusinessItem item, bool isPrint, out StringBuilder strErr)
        {
            string action = isPrint ? "ToPrint" : "ToSent";
            string str2 = isPrint ? "发送打印" : "直接回收";
            Agent.PrintOrSentTsd(ClientData.LogonUser.Oid, item, action, out strErr);
            if (strErr.Length > 0)
            {
                return false;
            }
            item = PLItem.Agent.GetBizItem(item.MasterOid, 0, 0, ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid, BizItemMode.BizItem) as DEBusinessItem;
            return true;
        }

        internal static void UpdateLvwBySentBom(ListView lv, ArrayList lstOrder, DERelationBizItem relitem, string unit)
        {
            bool isRedo = relitem.Relation.RelationName != ConstAm.SENTBOM_RELCLS;
            DEMetaRelation relation = ModelContext.MetaModel.GetRelation(relitem.Relation.RelationName);
            ArrayList relationAttributes = ModelContext.MetaModel.GetRelationAttributes(relation.Oid, 1);
            object attrValue = relitem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW);
            if (attrValue != null)
            {
                ArrayList list2 = new ArrayList(attrValue.ToString().Split(new char[] { ';' },StringSplitOptions.RemoveEmptyEntries));
                for (int i = 0; i < list2.Count; i++)
                {
                    if (string.IsNullOrEmpty(unit) || (list2[i].ToString().IndexOf(unit + "(") != -1))
                    {
                        SetLvwSentRelValues(lv, lstOrder, relitem, relationAttributes, list2[i].ToString(), isRedo);
                    }
                }
            }
        }

        internal static void UpdateLvwRelValues(ListView lv, ArrayList lstOrder, DERelationBizItem dItem, string unit)
        {
            DEMetaRelation relation = ModelContext.MetaModel.GetRelation(dItem.Relation.RelationName);
            ArrayList relationAttributes = ModelContext.MetaModel.GetRelationAttributes(relation.Name);
            object attrValue = dItem.Relation.GetAttrValue("JSDW");
            if (attrValue != null)
            {
                foreach (string str in attrValue.ToString().Replace("；", ";").Split(new char[] { ';' }))
                {
                    if (string.IsNullOrEmpty(unit) || (str.IndexOf(unit + "(") == 0))
                    {
                        SetLvwRelValues(lv, lstOrder, dItem, relationAttributes, str);
                    }
                }
            }
            else
            {
                SetLvwRelValues(lv, lstOrder, dItem, relationAttributes, "");
            }
        }

        public static void UpdateLvwValues(Hashtable hsCols, ListView lvw, ArrayList lstOrd, DEBusinessItem item)
        {
            Hashtable hashtable = ConvertItem(hsCols, item);
            List<string> list = new List<string> { "MOID", "ROID", "IROID" };
            ListViewItem item2 = null;
            bool flag = false;
            foreach (ListViewItem item3 in lvw.Items)
            {
                DEBusinessItem tag = item3.Tag as DEBusinessItem;
                if (tag.MasterOid == item.MasterOid)
                {
                    item2 = item3;
                    flag = true;
                    break;
                }
            }
            if (item2 == null)
            {
                item2 = new ListViewItem();
            }
            item2.Tag = item;
            int num = 0;
            foreach (string str in lstOrd)
            {
                if (lvw.Columns.ContainsKey(str))
                {
                    if (num == 0)
                    {
                        item2.Text = item2.Name = (hashtable[str] == null) ? "" : hashtable[str].ToString();
                    }
                    else if (!flag)
                    {
                        item2.SubItems.Add((hashtable[str] == null) ? "" : hashtable[str].ToString());
                    }
                    else
                    {
                        item2.SubItems[lvw.Columns[str].Index].Text = (hashtable[str] == null) ? "" : hashtable[str].ToString();
                    }
                    num++;
                }
            }
            if (!flag)
            {
                lvw.Items.Insert(0, item2);
            }
        }

        public static void UpdatePrintLvwRelValues(ListView lv, ArrayList lstOrder, DERelationBizItem dItem)
        {
            DEMetaRelation relation = ModelContext.MetaModel.GetRelation(dItem.Relation.RelationName);
            ArrayList relationAttributes = ModelContext.MetaModel.GetRelationAttributes(relation.Oid, 1);
            ListViewItem item = null;
            bool flag = false;
            foreach (ListViewItem item2 in lv.Items)
            {
                DERelationBizItem tag = item2.Tag as DERelationBizItem;
                if (tag.Relation.Oid == dItem.Relation.Oid)
                {
                    flag = true;
                    item = item2;
                    break;
                }
            }
            if (!flag)
            {
                item = new ListViewItem();
            }
            item.Tag = dItem;
            int num = 0;
            foreach (string str in lstOrder)
            {
                string str2 = "";
                foreach (DEMetaAttribute attribute in relationAttributes)
                {
                    if (attribute.Label == str)
                    {
                        str2 = dItem.Relation.AttrValueExists(attribute.Name) ? dItem.Relation.GetAttrValue(attribute.Name).ToString() : "";
                        if (!string.IsNullOrEmpty(str2))
                        {
                            if (attribute.DataType2 == PLMDataType.DateTime)
                            {
                                str2 = Convert.ToDateTime(str2).ToString("yyyy.MM.dd hh:mm");
                            }
                            if ((attribute.DataType2 == PLMDataType.Guid) && (attribute.Label.IndexOf("人") != -1))
                            {
                                Guid g = new Guid(str2);
                                if (!Guid.Empty.Equals(g))
                                {
                                    DEUser userByOid = PLUser.Agent.GetUserByOid(g);
                                    str2 = (userByOid == null) ? "" : userByOid.Name;
                                }
                                else
                                {
                                    str2 = "";
                                }
                            }
                        }
                        break;
                    }
                }
                if (num == 0)
                {
                    item.Text = str2;
                }
                else if (lv.Columns.ContainsKey(str))
                {
                    if (!flag)
                    {
                        item.SubItems.Add(str2);
                    }
                    else
                    {
                        item.SubItems[lv.Columns[str].Index].Text = str2;
                    }
                }
                num++;
            }
            if (!flag)
            {
                lv.Items.Add(item);
            }
        }

        public static void UpdateSentBom(DERelationBizItem relItem, string unit, int fs, string signner, string sm)
        {
            int num;
            int num2;
            int num3;
            ArrayList list;
            string str = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW).ToString();
            GetSentFs(relItem, unit, out num, out num3);
            string attrs = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFFS) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFFS).ToString();
            GetAttrByUnit(attrs, unit, true, out list);
            list.Add(string.Concat(new object[] { unit, "(", fs, ")" }));
            attrs = ResetAttrs(list);
            relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_FFFS, attrs);
            UpdateSentSign(relItem, unit, signner);
            string str3 = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFSM) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFSM).ToString();
            GetAttrByUnit(str3, unit, true, out list);
            if (!string.IsNullOrEmpty(sm))
            {
                list.Add(unit + "(" + sm + ")");
            }
            str3 = ResetAttrs(list);
            relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_FFSM, str3);
            GetSentBomAllFs(relItem, out num, out num2, out num3);
            relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_REALFS, num3);
        }

        public static void UpdateSentSign(DERelationBizItem relItem, string unit, string signer)
        {
            ArrayList list;
            string str = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SINGNER) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SINGNER).ToString();
            string attrValue = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SIGNTIME) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_SIGNTIME).ToString();
            if (!(!string.IsNullOrEmpty(str) || string.IsNullOrEmpty(signer)))
            {
                str = unit + "(" + signer + ")";
                attrValue = unit + "(" + DateTime.Now.ToString("yyyy.MM.dd HH:mm") + ")";
            }
            else
            {
                string[] c = str.Split(new char[] { ';' });
                string[] strArray2 = attrValue.Split(new char[] { ';' });
                ArrayList list2 = new ArrayList(c);
                ArrayList list3 = new ArrayList(strArray2);
                string str3 = "";
                string str4 = "";
                foreach (string str5 in list2)
                {
                    if (str5.IndexOf(unit + "(") > -1)
                    {
                        str3 = str5;
                        break;
                    }
                }
                foreach (string str5 in list3)
                {
                    if (str5.IndexOf(unit + "(") > -1)
                    {
                        str4 = str5;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(str4))
                {
                    list3.Remove(str4);
                    list2.Remove(str3);
                    if (!string.IsNullOrEmpty(signer))
                    {
                        str4 = unit + "(" + DateTime.Now.ToString("yyyy.MM.dd HH:mm") + ")";
                        list3.Add(str4);
                        str3 = unit + "(" + signer + ")";
                        list2.Add(str3);
                    }
                }
                else if (!string.IsNullOrEmpty(signer))
                {
                    list3.Add(unit + "(" + DateTime.Now.ToString("yyyy.MM.dd HH:mm") + ")");
                    list2.Add(unit + "(" + signer + ")");
                }
                list3.Sort();
                list2.Sort();
                attrValue = "";
                str = "";
                foreach (string str6 in list3)
                {
                    attrValue = attrValue + str6 + ";";
                }
                foreach (string str6 in list2)
                {
                    str = str + str6 + ";";
                }
                if (str.LastIndexOf(";") > 0)
                {
                    str = str.Remove(str.LastIndexOf(";"), 1);
                }
                if (attrValue.LastIndexOf(";") > 0)
                {
                    attrValue = attrValue.Remove(attrValue.LastIndexOf(";"), 1);
                }
            }
            string attrs = (relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_JSR) == null) ? "" : relItem.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_JSR).ToString();
            GetAttrByUnit(attrs, unit, true, out list);
            if (!string.IsNullOrEmpty(signer))
            {
                list.Add(unit + "(" + ClientData.LogonUser.Name + ")");
            }
            attrs = ResetAttrs(list);
            relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_JSR, attrs);
            relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_SINGNER, str);
            relItem.Relation.SetAttrValue(ConstAm.SENTBOM_ATTR_SIGNTIME, attrValue);
        }

        public static void UpdateTsdDw(DERelationBizItem relItem, string unit, int fs)
        {
            if (!string.IsNullOrEmpty(unit))
            {
                string str = (relItem.Relation.GetAttrValue("JSDW") == null) ? "" : relItem.Relation.GetAttrValue("JSDW").ToString();
                if (string.IsNullOrEmpty(str))
                {
                    str = string.Concat(new object[] { unit, "(", fs, ")" });
                }
                else
                {
                    ArrayList list = new ArrayList(str.Split(new char[] { ';' }));
                    string str2 = "";
                    foreach (string str3 in list)
                    {
                        if (str3.IndexOf(unit + "(") == 0)
                        {
                            str2 = str3;
                            break;
                        }
                    }
                    list.Remove(str2);
                    list.Add(string.Concat(new object[] { unit, "(", fs, ")" }));
                    list.Sort();
                    str = "";
                    foreach (string str3 in list)
                    {
                        str = str + ";" + str3;
                    }
                    str = str.Substring(1);
                }
                relItem.Relation.SetAttrValue("JSDW", str);
            }
        }

        public static IArchivManage Agent
        {
            get
            {
                if (_agent == null)
                {
                    _agent = RemoteProxy.GetObject(typeof(IArchivManage), "TiPLM/DQArchivManage/SVR/BFEntrance.rem") as IArchivManage;
                }
                return _agent;
            }
        }

        public static string SetCanSignUserName
        {
            set
            {
                lstSigners = new ArrayList();
                string str = value;
                PLRole role = new PLRole();
                ArrayList allRoles = role.GetAllRoles();
                foreach (DERole role2 in allRoles)
                {
                    if (role2.Name == str)
                    {
                        ArrayList usersByRole = role.GetUsersByRole(role2.Oid);
                        foreach (DEUser user in usersByRole)
                        {
                            lstSigners.Add(user.Name);
                        }
                    }
                }
                lstSigners.Sort();
            }
        }

        internal class CompareTsd : IComparer
        {
            public int Compare(object x, object y)
            {
                IBizItem item = x as IBizItem;
                IBizItem item2 = y as IBizItem;
                return string.Compare(item.Id, item2.Id);
            }
        }
    }
}

