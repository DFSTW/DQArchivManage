namespace DQArchivManageSvr
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.InteropServices;
    using System.Text;
    using Thyt.TiPLM.Common;
    using Thyt.TiPLM.DAL.Common;
    using Thyt.TiPLM.DEL.Product;

    public class BrArchivManager
    {
        internal ArrayList CheckTsdRight(ArrayList lstItems, string action, out StringBuilder strInfo, string clslb)
        {
            ArrayList list2;
            DBParameter dbParameter = DBUtil.GetDbParameter(true);
            try
            {
                dbParameter.Open();
                ArrayList list = new DaArchivManager(dbParameter).CheckTsdRight(lstItems, action, out strInfo, clslb);
                dbParameter.Commit();
                list2 = list;
            }
            catch (Exception exception)
            {
                throw new PLMException("验证是否可处理出错", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return list2;
        }

        private ArrayList GetAllDocUnit(DataTable tb, ArrayList lstUnit)
        {
            ArrayList list = new ArrayList();
            foreach (string str in lstUnit)
            {
                bool flag = true;
                foreach (DataRow row in tb.Rows)
                {
                    if (string.IsNullOrEmpty(this.GetValueByUnit(row, "单位", str)))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    list.Add(str);
                }
            }
            return list;
        }

        internal DataSet GetBpmNameByUserOid(Guid useroid)
        {
            DataSet bpmNameByUserOid;
            DBParameter dbParameter = DBUtil.GetDbParameter(false);
            try
            {
                dbParameter.Open();
                bpmNameByUserOid = new DaArchivManager(dbParameter).GetBpmNameByUserOid(useroid);
            }
            catch (Exception exception)
            {
                throw new PLMException("根据登录用户获取流程信息失败", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return bpmNameByUserOid;
        }

        internal ArrayList GetDocClsById(string doccode)
        {
            ArrayList docClsById;
            DBParameter dbParameter = DBUtil.GetDbParameter(false);
            try
            {
                dbParameter.Open();
                docClsById = new DaArchivManager(dbParameter).GetDocClsById(doccode);
            }
            catch (Exception exception)
            {
                throw new PLMException("根据图号获取图档类型失败", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return docClsById;
        }

        private ArrayList GetDocLst(DataTable tb)
        {
            ArrayList list = new ArrayList();
            foreach (DataRow row in tb.Rows)
            {
                string str = row["图号"].ToString();
                list.Add(str);
            }
            list.Sort();
            return list;
        }

        internal DataSet GetDrawingForTsOutput(Guid iroid)
        {
            DataSet drawingForTsOutput;
            DBParameter dbParameter = DBUtil.GetDbParameter(false);
            try
            {
                dbParameter.Open();
                drawingForTsOutput = new DaArchivManager(dbParameter).GetDrawingForTsOutput(iroid);
            }
            catch (Exception exception)
            {
                throw new PLMException("托晒单輸出失敗", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return drawingForTsOutput;
        }

        private void GetNewRowByUnit(DEBusinessItem item, DataTable tbOrg, bool isall, DataTable tbObj, string unit, string username)
        {
            string str = item.Iteration.GetAttrValue("TSTYPE").ToString();
            string str2 = (item.Iteration.GetAttrValue("DOCCODE") == null) ? "" : item.Iteration.GetAttrValue("DOCCODE").ToString();
            string str3 = (item.Iteration.GetAttrValue("DOCNAME") == null) ? "" : item.Iteration.GetAttrValue("DOCNAME").ToString();
            string str4 = (item.Iteration.GetAttrValue("VERIT") == null) ? "" : item.Iteration.GetAttrValue("VERIT").ToString();
            string str5 = (item.Iteration.GetAttrValue("WKFLINFO") == null) ? "" : item.Iteration.GetAttrValue("WKFLINFO").ToString();
            if (string.IsNullOrEmpty(str2))
            {
                str2 = str5;
            }
            if (isall)
            {
                DataRow rNew = tbObj.NewRow();
                rNew["总图号"] = str2;
                this.ResetNewRowValue(rNew, tbOrg, unit, username);
                rNew["图号"] = "";
                rNew["收发类型"] = str;
                if (tbObj.Columns.Contains("名称"))
                {
                    rNew["名称"] = str3;
                }
                if (tbObj.Columns.Contains("版本"))
                {
                    rNew["版本"] = str4;
                }
                if (tbObj.Columns.Contains("流程信息"))
                {
                    rNew["流程信息"] = str5;
                }
                tbObj.Rows.Add(rNew);
            }
            else
            {
                this.ResetNewRowValue(tbObj, tbOrg, unit, str2, username);
            }
        }

        private DataTable GetNewTable(string FfOrHs, DataSet ds)
        {
            DataTable table = null;
            if (ds.Tables.Contains(FfOrHs))
            {
                DataTable table2 = ds.Tables[FfOrHs];
                table = new DataTable(table2.TableName);
                table.Columns.Add("总图号", typeof(string));
                foreach (DataColumn column in table2.Columns)
                {
                    if (column.ColumnName != "单位")
                    {
                        table.Columns.Add(column.ColumnName, column.DataType);
                    }
                }
            }
            return table;
        }

        private DataTable GetNewTableForOutPut(DEBusinessItem item, DataTable tb, string username)
        {
            string str = item.Iteration.GetAttrValue("TSTYPE").ToString();
            string str2 = (item.Iteration.GetAttrValue("DOCCODE") == null) ? "" : item.Iteration.GetAttrValue("DOCCODE").ToString();
            string str3 = (item.Iteration.GetAttrValue("DOCNAME") == null) ? "" : item.Iteration.GetAttrValue("DOCNAME").ToString();
            string str4 = (item.Iteration.GetAttrValue("VERIT") == null) ? "" : item.Iteration.GetAttrValue("VERIT").ToString();
            string str5 = (item.Iteration.GetAttrValue("WKFLINFO") == null) ? "" : item.Iteration.GetAttrValue("WKFLINFO").ToString();
            if (string.IsNullOrEmpty(str2))
            {
                str2 = str5;
            }
            if (tb.Rows.Count != 0)
            {
                DataRow row;
                DataTable table = tb.Clone();
                table.TableName = tb.TableName;
                ArrayList unit = this.GetUnit(tb);
                ArrayList docLst = this.GetDocLst(tb);
                ArrayList allDocUnit = this.GetAllDocUnit(tb, unit);
                for (int i = 0; i < allDocUnit.Count; i++)
                {
                    string str6 = allDocUnit[i].ToString();
                    unit.Remove(str6);
                    row = table.NewRow();
                    this.ResetNewRowValue(row, tb, str6, username);
                    if (i == 0)
                    {
                        row["图号"] = str2;
                    }
                    else
                    {
                        row["图号"] = "";
                    }
                    row["收发类型"] = str;
                    if (table.Columns.Contains("名称"))
                    {
                        row["名称"] = str3;
                    }
                    if (table.Columns.Contains("版本"))
                    {
                        row["版本"] = str4;
                    }
                    if (table.Columns.Contains("流程信息"))
                    {
                        row["流程信息"] = str5;
                    }
                    table.Rows.Add(row);
                }
                foreach (string str7 in docLst)
                {
                    foreach (string str6 in unit)
                    {
                        DataRow[] rowArray = tb.Select("图号='" + str7 + "'");
                        if (rowArray.Length == 0)
                        {
                            break;
                        }
                        DataRow row2 = rowArray[0];
                        row = table.NewRow();
                        this.ResetNewRowValue(ref row, row2, str6, username);
                        if (row != null)
                        {
                            row["收发类型"] = str;
                            table.Rows.Add(row);
                        }
                    }
                }
                return table;
            }
            return null;
        }

        internal DataSet GetSecondDocStandard()
        {
            DataSet secondDocStandard;
            DBParameter dbParameter = DBUtil.GetDbParameter(false);
            try
            {
                dbParameter.Open();
                secondDocStandard = new DaArchivManager(dbParameter).GetSecondDocStandard();
            }
            catch (Exception exception)
            {
                throw new PLMException("获取二次图默认数量失败", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return secondDocStandard;
        }

        internal DataSet GetSentLst(string docCode, string wkinfo, string tsdId, string tunit, string tstype, string sentstate, DateTime dFrom, DateTime dTo)
        {
            DataSet set;
            DBParameter dbParameter = DBUtil.GetDbParameter(false);
            try
            {
                dbParameter.Open();
                set = new DaArchivManager(dbParameter).GetSentLst(docCode, wkinfo, tsdId, tunit, tstype, sentstate, dFrom, dTo);
            }
            catch (Exception exception)
            {
                throw new PLMException("获取收发清单出错", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return set;
        }

        internal DataSet GetSentResultForOutPut(DEBusinessItem item, string username)
        {
            DataSet set2;
            DBParameter dbParameter = DBUtil.GetDbParameter(false);
            try
            {
                dbParameter.Open();
                DataSet sentResultForOutPut = new DaArchivManager(dbParameter).GetSentResultForOutPut(item.IterOid);
                if (sentResultForOutPut.Tables.Contains("FF"))
                {
                    DataTable table = sentResultForOutPut.Tables["FF"];
                    sentResultForOutPut.Tables.Remove(table);
                    if (table.Rows.Count > 0)
                    {
                        DataTable table2 = this.GetNewTableForOutPut(item, table, username);
                        sentResultForOutPut.Tables.Add(table2);
                    }
                }
                if (sentResultForOutPut.Tables.Contains("HS"))
                {
                    DataTable table3 = sentResultForOutPut.Tables["HS"];
                    sentResultForOutPut.Tables.Remove(table3);
                    if (table3.Rows.Count > 0)
                    {
                        DataTable table4 = this.GetNewTableForOutPut(item, table3, username);
                        sentResultForOutPut.Tables.Add(table4);
                    }
                }
                sentResultForOutPut.AcceptChanges();
                set2 = sentResultForOutPut;
            }
            catch (Exception exception)
            {
                throw new PLMException("获取用于收发登记的收发帐明细失败", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return set2;
        }

        public DataSet GetSentResultForOutPut(ArrayList lstUnit, ArrayList lstItems, out Hashtable hsTbIdx, string username)
        {
            DataSet set4;
            DBParameter dbParameter = DBUtil.GetDbParameter(false);
            try
            {
                DEBusinessItem current;
                dbParameter.Open();
                DaArchivManager manager = new DaArchivManager(dbParameter);
                DataSet set = new DataSet();
                hsTbIdx = new Hashtable();
                Hashtable hashtable = new Hashtable();
                Hashtable hashtable2 = new Hashtable();
                ArrayList lstTH = new ArrayList();
                DataTable newTable = null;
                DataTable table2 = null;
                IEnumerator enumerator = lstItems.GetEnumerator();
                {
                    while (enumerator.MoveNext())
                    {
                        current = (DEBusinessItem) enumerator.Current;
                        DataSet sentResultForOutPut = manager.GetSentResultForOutPut(current.IterOid);
                        if (newTable == null)
                        {
                            newTable = this.GetNewTable("FF", sentResultForOutPut);
                        }
                        if (table2 == null)
                        {
                            table2 = this.GetNewTable("HS", sentResultForOutPut);
                        }
                        string tH = this.GetTH(current);
                        hashtable[tH] = current;
                        hashtable2[tH] = sentResultForOutPut;
                        lstTH.Add(tH);
                    }
                }
                lstTH.Sort();
                int num = 0;
                Hashtable MapUnitLstTH = new Hashtable();
                Hashtable hashtable4 = new Hashtable();
                foreach (string unit in lstUnit)
                {
                    DataSet datasetList;
                    ArrayList lstUsedTH2;
                    DataTable table4;
                    num++;
                    hsTbIdx[unit] = num;
                    foreach (string tuhao in lstTH)
                    {
                        datasetList = hashtable2[tuhao] as DataSet;
                        /*
                        if (datasetList.Tables.Contains("FF") && this.IsAllDocContainsUnit(datasetList.Tables["FF"], unit))
                        {
                            ArrayList lstUsedTH;
                            if (MapUnitLstTH.Contains(unit))
                            {
                                lstUsedTH = MapUnitLstTH[unit] as ArrayList;
                            }
                            else
                            {
                                lstUsedTH = new ArrayList();
                            }
                            lstUsedTH.Add(tuhao);
                            MapUnitLstTH[unit] = lstUsedTH;
                        }
                        if (datasetList.Tables.Contains("HS") && this.IsAllDocContainsUnit(datasetList.Tables["HS"], unit))
                        {
                            ArrayList list3;
                            if (hashtable4.Contains(unit))
                            {
                                list3 = hashtable4[unit] as ArrayList;
                            }
                            else
                            {
                                list3 = new ArrayList();
                            }
                            list3.Add(tuhao);
                            hashtable4[unit] = list3;
                        }
                         * */
                    }
                    ArrayList lstTH2 = new ArrayList(lstTH);
                    if (newTable != null)
                    {
                        DataTable tbObj = newTable.Clone();
                        tbObj.TableName = "FF" + num.ToString();
                        if (MapUnitLstTH.Contains(unit))
                        {
                            lstUsedTH2 = MapUnitLstTH[unit] as ArrayList;
                            foreach (string usedTH in lstUsedTH2)
                            {
                                lstTH2.Remove(usedTH);
                                datasetList = hashtable2[usedTH] as DataSet;
                                table4 = datasetList.Tables["FF"];
                                current = hashtable[usedTH] as DEBusinessItem;
                                this.GetNewRowByUnit(current, table4, true, tbObj, unit, username);
                            }
                        }
                        foreach (string str3 in lstTH2)
                        {
                            datasetList = hashtable2[str3] as DataSet;
                            table4 = datasetList.Tables["FF"];
                            current = hashtable[str3] as DEBusinessItem;
                            this.GetNewRowByUnit(current, table4, false, tbObj, unit, username);
                        }
                        
                        if (tbObj.Rows.Count > 0)
                        {
                            DataView dv = tbObj.DefaultView;
                            dv.Sort = "图号";
                            DataTable sortedtbObj = dv.ToTable();
                            set.Tables.Add(sortedtbObj);
                        }
                    }
                    if (table2 != null)
                    {
                        DataTable table5 = table2.Clone();
                        table5.TableName = "HS" + num.ToString();
                        lstTH2 = new ArrayList(lstTH);
                        if (hashtable4.Contains(unit))
                        {
                            lstUsedTH2 = hashtable4[unit] as ArrayList;
                            foreach (string str3 in lstUsedTH2)
                            {
                                lstTH2.Remove(str3);
                                datasetList = hashtable2[str3] as DataSet;
                                table4 = datasetList.Tables["HS"];
                                current = hashtable[str3] as DEBusinessItem;
                                this.GetNewRowByUnit(current, table4, true, table5, unit, username);
                            }
                        }
                        foreach (string str3 in lstTH2)
                        {
                            datasetList = hashtable2[str3] as DataSet;
                            table4 = datasetList.Tables["HS"];
                            current = hashtable[str3] as DEBusinessItem;
                            this.GetNewRowByUnit(current, table4, false, table5, unit, username);
                        }
                        if (table5.Rows.Count > 0)
                        {
                            DataView dv = table5.DefaultView;
                            dv.Sort = "图号";
                            DataTable sortedtbObj = dv.ToTable();
                            set.Tables.Add(sortedtbObj);
                        }
                    }
                }
                set.AcceptChanges();
                set4 = set;
            }
            catch (Exception exception)
            {
                throw new PLMException("获取用于收发登记的收发帐明细失败", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return set4;
        }

        private string GetTH(DEBusinessItem item)
        {
            string str = (item.Iteration.GetAttrValue("DOCCODE") == null) ? "" : item.Iteration.GetAttrValue("DOCCODE").ToString();
            string str2 = (item.Iteration.GetAttrValue("WKFLINFO") == null) ? "" : item.Iteration.GetAttrValue("WKFLINFO").ToString();
            if (string.IsNullOrEmpty(str))
            {
                str = str2;
            }
            return str;
        }

        internal DataSet GetTSD(string docId, string wkName)
        {
            DataSet tsd;
            DBParameter dbParameter = DBUtil.GetDbParameter(false);
            try
            {
                dbParameter.Open();
                tsd = new DaArchivManager(dbParameter).GetTsd(docId, wkName);
            }
            catch (Exception exception)
            {
                throw new PLMException("根据图号与流程获取托晒单出错", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return tsd;
        }

        internal DataSet GetTSD(string docCode, string bpmInfo, string TsStatue, string TsType, string OrgPrintUser, string ftlx, string unit, DateTime dFromTime, DateTime dToTime, bool isPrint)
        {
            DataSet set;
            DBParameter dbParameter = DBUtil.GetDbParameter(false);
            try
            {
                dbParameter.Open();
                set = new DaArchivManager(dbParameter).GetTsd(docCode, bpmInfo, TsStatue, TsType, OrgPrintUser, ftlx, unit, dFromTime, dToTime, isPrint);
            }
            catch (Exception exception)
            {
                throw new PLMException("托晒单快速查询出错", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return set;
        }

        internal DataSet GetTSDForPrint(string docId, Guid useroid, string wkName)
        {
            DataSet set2;
            DBParameter dbParameter = DBUtil.GetDbParameter(true);
            try
            {
                dbParameter.Open();
                DataSet set = new DaArchivManager(dbParameter).GetTSDForPrint(useroid, docId, wkName);
                dbParameter.Commit();
                set2 = set;
            }
            catch (Exception exception)
            {
                dbParameter.Rollback();
                throw new PLMException("获取用于打印的托晒单失败", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return set2;
        }

        internal DataSet GetTsdFsdwByDoc(Guid useroid, string doccode, string clsname, string docname, string yct, string ftlx)
        {
            DataSet set2;
            DBParameter dbParameter = DBUtil.GetDbParameter(true);
            try
            {
                dbParameter.Open();
                DataSet set = new DaArchivManager(dbParameter).GetTsdFsdwByDoc(useroid, doccode, clsname, docname, yct, ftlx);
                dbParameter.Commit();
                set2 = set;
            }
            catch (Exception exception)
            {
                dbParameter.Rollback();
                throw new PLMException("获取托晒单发送单位失败", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return set2;
        }

        internal DataSet GetTsRes(Hashtable hsGetResType)
        {
            DataSet tsRes;
            DBParameter dbParameter = DBUtil.GetDbParameter(false);
            try
            {
                dbParameter.Open();
                tsRes = new DaArchivManager(dbParameter).GetTsRes(hsGetResType);
            }
            catch (Exception exception)
            {
                throw new PLMException("获取资源数据出错", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return tsRes;
        }

        private ArrayList GetUnit(DataTable tb)
        {
            ArrayList list = new ArrayList();
            foreach (DataRow row in tb.Rows)
            {
                string[] strArray = row["单位"].ToString().Split(new char[] { ';' });
                foreach (string str2 in strArray)
                {
                    string str3;
                    int length = str2.LastIndexOf("(");
                    if (length != -1)
                    {
                        str3 = str2.Substring(0, length);
                    }
                    else
                    {
                        str3 = str2;
                    }
                    if (!list.Contains(str3))
                    {
                        list.Add(str3);
                    }
                }
            }
            list.Sort();
            return list;
        }

        private string GetValueByUnit(DataRow row, string colnme, string unit)
        {
            string str = (row[colnme] == DBNull.Value) ? "" : row[colnme].ToString();
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            if (str.IndexOf(unit + "(") == -1)
            {
                return null;
            }
            string[] strArray = str.Split(new char[] { ';' });
            foreach (string str2 in strArray)
            {
                if (str2.IndexOf(unit + "(") != -1)
                {
                    string str3 = str2.Replace(unit + "(", "");
                    return str3.Substring(0, str3.Length - 1);
                }
            }
            return "";
        }

        internal Hashtable GetViewOfCol(string v_type, out ArrayList lstOrder, out Hashtable hswide)
        {
            Hashtable hashtable;
            DBParameter dbParameter = DBUtil.GetDbParameter(false);
            try
            {
                dbParameter.Open();
                hashtable = new DaArchivManager(dbParameter).GetViewOfCol(v_type, out lstOrder, out hswide);
            }
            catch (Exception exception)
            {
                throw new PLMException("获取显示列出错", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return hashtable;
        }

        private bool IsAllDocContainsUnit(DataTable tb, string unit)
        {
            foreach (DataRow row in tb.Rows)
            {
                if (string.IsNullOrEmpty(this.GetValueByUnit(row, "单位", unit)))
                {
                    return false;
                }
            }
            return true;
        }

        internal void PrintOrSentTsd(Guid useroid, DEBusinessItem item, string action, out StringBuilder strErr)
        {
            DBParameter dbParameter = DBUtil.GetDbParameter(true);
            try
            {
                dbParameter.Open();
                new DaArchivManager(dbParameter).PrintOrSentTsd(useroid, item, action, out strErr);
                if (strErr.Length > 0)
                {
                    dbParameter.Rollback();
                }
                else
                {
                    dbParameter.Commit();
                }
            }
            catch (Exception exception)
            {
                dbParameter.Rollback();
                throw new PLMException("托晒单发送打印失败", exception);
            }
            finally
            {
                dbParameter.Close();
            }
        }

        private void ResetNewRowValue(ref DataRow rNew, DataRow row, string unit, string userName)
        {
            string str = this.GetValueByUnit(row, "单位", unit);
            if (string.IsNullOrEmpty(str))
            {
                rNew = null;
            }
            else
            {
                List<string> list = new List<string> { "经办", "时间", "签收人", "说明", "回收份数" };
                rNew.ItemArray = row.ItemArray;
                int num = Convert.ToInt32(str);
                int num2 = (row["每份张数"] == DBNull.Value) ? 0 : Convert.ToInt32(row["每份张数"]);
                rNew["单位"] = unit;
                rNew["份数"] = num;
                rNew["每份张数"] = num2;
                foreach (string str2 in list)
                {
                    if (rNew.Table.Columns.Contains(str2))
                    {
                        string str3 = this.GetValueByUnit(rNew, str2, unit);
                        if (str2 == "经办")
                        {
                            rNew[str2] = string.IsNullOrEmpty(str3) ? userName : str3;
                        }
                        else
                        {
                            rNew[str2] = string.IsNullOrEmpty(str3) ? "" : str3;
                        }
                    }
                }
            }
        }

        private void ResetNewRowValue(DataRow rNew, DataTable tbFF, string unit, string username)
        {
            List<string> list = new List<string> { "经办", "时间", "签收人", "说明", "回收份数" };
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            foreach (DataRow row in tbFF.Rows)
            {
                if (num3 == 0)
                {
                    foreach (DataColumn column in tbFF.Columns)
                    {
                        if (rNew.Table.Columns.Contains(column.ColumnName))
                        {
                            rNew[column.ColumnName] = row[column];
                        }
                    }
                }
                string str = this.GetValueByUnit(row, "单位", unit);
                num += Convert.ToInt32(str);
                num2 += (row["每份张数"] == DBNull.Value) ? 0 : Convert.ToInt32(row["每份张数"]);
            }
            rNew["份数"] = num;
            rNew["每份张数"] = num2;
            if (rNew.Table.Columns.Contains("单位"))
            {
                rNew["单位"] = unit;
            }
            foreach (string str2 in list)
            {
                if (tbFF.Columns.Contains(str2))
                {
                    string str3 = this.GetValueByUnit(rNew, str2, unit);
                    if (str2 == "经办")
                    {
                        rNew[str2] = string.IsNullOrEmpty(str3) ? username : str3;
                    }
                    else if (str2 == "时间")
                    {

                    }
                    else
                    {
                        rNew[str2] = string.IsNullOrEmpty(str3) ? "" : str3;
                    }
                }
            }
        }

        private void ResetNewRowValue(DataTable tbNew, DataTable tbFF, string unit, string docId, string userName)
        {
            List<string> list = new List<string> { "经办", "时间", "签收人", "说明", "回收份数" };
            foreach (DataRow row in tbFF.Rows)
            {
                int num2 = 0;
                int num3 = 0;
                if (!string.IsNullOrEmpty(this.GetValueByUnit(row, "单位", unit)))
                {
                    DataRow row2 = tbNew.NewRow();
                    row2["总图号"] = docId;
                    foreach (DataColumn column in tbFF.Columns)
                    {
                        if (row2.Table.Columns.Contains(column.ColumnName))
                        {
                            row2[column.ColumnName] = row[column];
                        }
                    }
                    num2 = Convert.ToInt32(this.GetValueByUnit(row, "单位", unit));
                    num3 = (row["每份张数"] == DBNull.Value) ? 0 : Convert.ToInt32(row["每份张数"]);
                    row2["份数"] = num2;
                    row2["每份张数"] = num3;
                    if (row2.Table.Columns.Contains("单位"))
                    {
                        row2["单位"] = unit;
                    }
                    foreach (string str3 in list)
                    {
                        if (tbFF.Columns.Contains(str3) && tbNew.Columns.Contains(str3))
                        {
                            string str = this.GetValueByUnit(row2, str3, unit);
                            if (str3 == "经办")
                            {
                                row2[str3] = string.IsNullOrEmpty(str) ? userName : str;
                            }
                            else if (str3 == "时间")
                            {
                                
                            }
                            else
                            {
                                row2[str3] = string.IsNullOrEmpty(str) ? "" : str;
                            }
                        }
                    }
                    tbNew.Rows.Add(row2);
                }
            }
        }

        internal void SignSentList(ArrayList lisItems, string unit, string signer, string sm)
        {
            Hashtable hashtable;
            DBParameter dbParameter = DBUtil.GetDbParameter(false);
            try
            {
                dbParameter.Open();
                var manager = new DaArchivManager(dbParameter);
                manager.SignSentBefore(unit, signer);
                foreach (DEBusinessItem item in lisItems)
                {
                    manager.SignSentItem(item, unit, signer, sm);
                }
            }
            catch (Exception exception)
            {
                throw new PLMException("SignSentList错误", exception);
            }
            finally
            {
                dbParameter.Close();
            }
            return;
        }
    }
}

