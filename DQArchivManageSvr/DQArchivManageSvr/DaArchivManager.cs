namespace DQArchivManageSvr
{
    using Oracle.DataAccess.Client;
    using Oracle.DataAccess.Types;
    using System;
    using System.Collections;
    using System.Data;
    using System.Runtime.InteropServices;
    using System.Text;
    using Thyt.TiPLM.DAL.Common;
    using Thyt.TiPLM.DAL.Product.ORA;
    using Thyt.TiPLM.DEL.Product;

    public class DaArchivManager
    {
        private OracleCommand _cmd;
        private readonly DBParameter dbparam;

        public DaArchivManager(DBParameter dbparam)
        {
            this.dbparam = dbparam;
        }

        internal ArrayList CheckTsdRight(ArrayList lstItems, string action, out StringBuilder strInfo, string clslb)
        {
            this.cmd.Parameters.Clear();
            ArrayList list = new ArrayList();
            Hashtable hashtable = new Hashtable();
            strInfo = new StringBuilder();
            this.cmd.CommandType = CommandType.Text;
            this.cmd.CommandText = "insert into plm_tmp_archivmanage(plm_ssoid,plm_moid,plm_roid,plm_iroid,plm_id,plm_cls) values(:ssoid,:mOid,:rOid,:Iroid,:tId,:clslb)";
            Guid ssoid = Guid.NewGuid();
            this.cmd.Parameters.Add(":ssoid", OracleDbType.Raw).Value = ssoid.ToByteArray();
            OracleParameter parameter = this.cmd.Parameters.Add(":mOid", OracleDbType.Raw);
            OracleParameter parameter2 = this.cmd.Parameters.Add(":rOid", OracleDbType.Raw);
            OracleParameter parameter3 = this.cmd.Parameters.Add(":Iroid", OracleDbType.Raw);
            OracleParameter parameter4 = this.cmd.Parameters.Add(":tId", OracleDbType.Varchar2);
            this.cmd.Parameters.Add(":clslb", OracleDbType.Varchar2).Value = clslb;
            foreach (DEBusinessItem item in lstItems)
            {
                parameter.Value = item.MasterOid.ToByteArray();
                parameter2.Value = item.RevOid.ToByteArray();
                parameter3.Value = item.IterOid.ToByteArray();
                parameter4.Value = item.Id;
                hashtable[item.Id] = item;
                this.cmd.ExecuteNonQuery();
            }
            this.cmd.Parameters.Clear();
            strInfo = new StringBuilder();
            this.cmd.CommandType = CommandType.StoredProcedure;
            this.cmd.CommandText = "PLM_DQ_DOSSIER.CheckTsdIsRight";
            this.cmd.Parameters.Add(":ssoid", OracleDbType.Raw).Value = ssoid.ToByteArray();
            this.cmd.Parameters.Add(":taction", OracleDbType.Varchar2).Value = action;
            this.cmd.ExecuteNonQuery();
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.Text;
            this.cmd.CommandText = "select PLM_ID,PLM_INFO from plm_tmp_archivmanage where plm_ssoid =:ssoid";
            this.cmd.Parameters.Add(":ssoid", OracleDbType.Raw).Value = ssoid.ToByteArray();
            OracleDataReader reader = this.cmd.ExecuteReader();
            while (reader.Read())
            {
                string str = reader.GetOracleString(0).Value;
                if (!reader.IsDBNull(1))
                {
                    strInfo.Append("托晒单:" + str + ":");
                    strInfo.Append(reader.GetOracleString(1).Value);
                }
                else
                {
                    list.Add(hashtable[str]);
                }
            }
            this.ClearTmpData(ssoid);
            return list;
        }

        private void ClearTmpData(Guid ssoid)
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            this.cmd.CommandText = "PLM_DQ_DOSSIER.ClearTmpData";
            this.cmd.Parameters.Add(":ssoid", OracleDbType.Raw).Value = ssoid.ToByteArray();
            this.cmd.ExecuteNonQuery();
        }

        internal DataSet GetBpmNameByUserOid(Guid useroid)
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            this.cmd.CommandText = "PLM_DQ_DOSSIER.GetBpmNameByUserOid";
            this.cmd.Parameters.Add(":userOid ", OracleDbType.Raw).Value = useroid.ToByteArray();
            OracleParameter parameter = this.cmd.Parameters.Add(":rGetBpmName", OracleDbType.RefCursor, ParameterDirection.Output);
            this.cmd.Prepare();
            this.cmd.ExecuteNonQuery();
            OracleRefCursor cursor = parameter.Value as OracleRefCursor;
            DataSet set = new DataSet();
            OracleDataReader dataReader = cursor.GetDataReader();
            DataTable table = new DataTable("BPM");
            table.Load(dataReader);
            set.Tables.Add(table);
            return set;
        }

        internal ArrayList GetDocClsById(string doccode)
        {
            ArrayList list = new ArrayList();
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.Text;
            this.cmd.CommandText = " select distinct m.plm_m_class  from plm_psm_itemmaster_revision m, plm_cusv_doc t, plm_sys_metaclass  c  where m.plm_m_lastrevision = m.plm_r_revision  and m.plm_r_oid = t.plm_revisionoid and m.plm_m_id = :tid and m.plm_m_state <> 'A'   and m.plm_m_class = c.plm_name  and (C.PLM_LABEL LIKE '%图%' or C.PLM_LABEL LIKE '%文档%' or C.PLM_LABEL LIKE '%标准%') and c.PLM_LABEL not like '%打包单%' ";
            this.cmd.Parameters.Add(":tid", OracleDbType.Varchar2).Value = doccode;
            OracleDataReader reader = this.cmd.ExecuteReader();
            while (reader.Read())
            {
                string str = reader.GetOracleString(0).Value;
                list.Add(str);
            }
            return list;
        }

        internal DataSet GetDrawingForTsOutput(Guid iroid)
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            this.cmd.Parameters.Add(":Iroid", OracleDbType.Raw).Value = iroid.ToByteArray();
            OracleParameter parameter = this.cmd.Parameters.Add(":rTsd", OracleDbType.RefCursor, ParameterDirection.Output);
            this.cmd.CommandText = "PLM_DQ_DOSSIER.GetDrawingForTsOutput";
            this.cmd.Prepare();
            this.cmd.ExecuteNonQuery();
            OracleRefCursor cursor = parameter.Value as OracleRefCursor;
            DataSet set = new DataSet();
            DataTable table = new DataTable("TSD");
            table.Load(cursor.GetDataReader());
            set.Tables.Add(table);
            set.AcceptChanges();
            return set;
        }

        internal DataSet GetSecondDocStandard()
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.Text;
            this.cmd.CommandText = "select t.plm_id DWID ,t.plm_name DWNAME from plm_cus_TIRELXBMZY t where t.plm_name is not null";
            DataSet set = new DataSet();
            OracleDataReader reader = this.cmd.ExecuteReader();
            DataTable table = new DataTable("DW");
            DataTable table2 = new DataTable("NUM");
            table.Load(reader);
            this.cmd.CommandText = " select * from plm_cus_CONTENT2SENTNUM ";
            reader = this.cmd.ExecuteReader();
            table2.Load(reader);
            set.Tables.Add(table);
            set.Tables.Add(table2);
            set.AcceptChanges();
            return set;
        }

        internal DataSet GetSentLst(string docCode, string wkinfo, string tsdId, string tUnit, string tstype, string sentstate, DateTime dFrom, DateTime dTo)
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            DataSet set = new DataSet();
            this.cmd.CommandText = "PLM_DQ_DOSSIER.QuickSchSent";
            this.cmd.Parameters.Add(":DocCode", OracleDbType.Varchar2).Value = docCode;
            this.cmd.Parameters.Add(":BpmName", OracleDbType.Varchar2).Value = wkinfo;
            this.cmd.Parameters.Add(":TsdId", OracleDbType.Varchar2).Value = tsdId;
            this.cmd.Parameters.Add(":tUnit", OracleDbType.Varchar2).Value = tUnit;
            this.cmd.Parameters.Add(":TsType", OracleDbType.Varchar2).Value = tstype;
            this.cmd.Parameters.Add(":sentstate", OracleDbType.Varchar2).Value = sentstate;
            this.cmd.Parameters.Add(":FromTime", OracleDbType.Date).Value = new DateTime(dFrom.Year, dFrom.Month, dFrom.Day, 0, 0, 0);
            this.cmd.Parameters.Add(":ToTime", OracleDbType.Date).Value = new DateTime(dTo.Year, dTo.Month, dTo.Day, 0x17, 0x3b, 0x3b);
            OracleParameter parameter = this.cmd.Parameters.Add(":rs", OracleDbType.RefCursor, ParameterDirection.Output);
            this.cmd.ExecuteNonQuery();
            OracleDataReader dataReader = (parameter.Value as OracleRefCursor).GetDataReader();
            DataTable table = new DataTable("SENT");
            table.Load(dataReader);
            set.Tables.Add(table);
            return set;
        }

        internal DataSet GetSentResultForOutPut(Guid iroid)
        {
            OracleDataReader dataReader;
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            this.cmd.CommandText = "PLM_DQ_DOSSIER.GetSentResultForOutPut";
            this.cmd.Parameters.Add(":Iroid", OracleDbType.Raw).Value = iroid.ToByteArray();
            OracleParameter parameter = this.cmd.Parameters.Add(":rSent", OracleDbType.RefCursor, ParameterDirection.Output);
            OracleParameter parameter2 = this.cmd.Parameters.Add(":rReCover", OracleDbType.RefCursor, ParameterDirection.Output);
            this.cmd.ExecuteNonQuery();
            OracleRefCursor cursor = parameter.Value as OracleRefCursor;
            OracleRefCursor cursor2 = parameter2.Value as OracleRefCursor;
            DataTable table = new DataTable("FF");
            DataTable table2 = new DataTable("HS");
            DataSet set = new DataSet();
            if (!cursor.IsNull)
            {
                dataReader = cursor.GetDataReader();
                table.Load(dataReader);
                set.Tables.Add(table);
            }
            if (!cursor2.IsNull)
            {
                dataReader = cursor2.GetDataReader();
                table2.Load(dataReader);
                set.Tables.Add(table2);
            }
            set.AcceptChanges();
            return set;
        }

        internal DataSet GetTsd(string docId, string wkName)
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            this.cmd.Parameters.Add(":DocId", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(docId) ? "" : docId;
            this.cmd.Parameters.Add(":wkName", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(wkName) ? "" : wkName;
            OracleParameter parameter = this.cmd.Parameters.Add(":rTsd", OracleDbType.RefCursor, ParameterDirection.Output);
            this.cmd.CommandText = "PLM_DQ_DOSSIER.GetTSDQuick";
            this.cmd.Prepare();
            this.cmd.ExecuteNonQuery();
            OracleRefCursor cursor = parameter.Value as OracleRefCursor;
            DataTable table = new DataTable("TSD");
            if (!cursor.IsNull)
            {
                table.Load(cursor.GetDataReader());
            }
            DataSet set = new DataSet();
            set.Tables.Add(table);
            set.AcceptChanges();
            return set;
        }

        internal DataSet GetTsd(string docCode, string bpmInfo, string tsStatue, string tsType, string orgPrintUser, string ftlx, string unit, DateTime dFromTime, DateTime dToTime, bool isPrint)
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            DataSet set = new DataSet();
            this.cmd.CommandText = "PLM_DQ_DOSSIER.QuickSchTsd";
            this.cmd.Parameters.Add(":DocCode", OracleDbType.Varchar2).Value = docCode;
            this.cmd.Parameters.Add(":BpmInfp", OracleDbType.Varchar2).Value = bpmInfo;
            this.cmd.Parameters.Add(":TsStatue", OracleDbType.Varchar2).Value = tsStatue;
            this.cmd.Parameters.Add(":TsType", OracleDbType.Varchar2).Value = tsType;
            this.cmd.Parameters.Add(":OrgPrintUser", OracleDbType.Varchar2).Value = orgPrintUser;
            this.cmd.Parameters.Add(":FtLx", OracleDbType.Varchar2).Value = ftlx;
            this.cmd.Parameters.Add(":unit", OracleDbType.Varchar2).Value = unit;
            this.cmd.Parameters.Add(":FromTime", OracleDbType.Date).Value = new DateTime(dFromTime.Year, dFromTime.Month, dFromTime.Day, 0, 0, 0);
            this.cmd.Parameters.Add(":ToTime", OracleDbType.Date).Value = new DateTime(dToTime.Year, dToTime.Month, dToTime.Day, 0x17, 0x3b, 0x3b);
            this.cmd.Parameters.Add(":IsPrint", OracleDbType.Int32).Value = isPrint ? 1 : 0;
            OracleParameter parameter = this.cmd.Parameters.Add(":rs", OracleDbType.RefCursor, ParameterDirection.Output);
            this.cmd.ExecuteNonQuery();
            OracleDataReader dataReader = (parameter.Value as OracleRefCursor).GetDataReader();
            DataTable table = new DataTable("TSD");
            table.Load(dataReader);
            set.Tables.Add(table);
            return set;
        }

        internal DataSet GetTSDForPrint(Guid useroid, string docId, string wkName)
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            this.cmd.Parameters.Add(":DocId", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(docId) ? "" : docId;
            this.cmd.Parameters.Add(":wkName", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(wkName) ? "" : wkName;
            this.cmd.Parameters.Add(":useroid", OracleDbType.Raw).Value = useroid.ToByteArray();
            OracleParameter parameter = this.cmd.Parameters.Add(":rTsd", OracleDbType.RefCursor, ParameterDirection.Output);
            this.cmd.CommandText = "PLM_DQ_DOSSIER.GetPrintQuick";
            this.cmd.Prepare();
            this.cmd.ExecuteNonQuery();
            OracleRefCursor cursor = parameter.Value as OracleRefCursor;
            DataTable table = new DataTable("TSD");
            if (!cursor.IsNull)
            {
                table.Load(cursor.GetDataReader());
            }
            DataSet set = new DataSet();
            set.Tables.Add(table);
            set.AcceptChanges();
            return set;
        }

        internal DataSet GetTsdFsdwByDoc(Guid useroid, string doccode, string clsname, string docname, string yct, string ftlx)
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            this.cmd.CommandText = "PLM_DQ_DOSSIER.GetTsdFsdwByDoc";
            this.cmd.Parameters.Add(":useroid", OracleDbType.Raw).Value = useroid;
            this.cmd.Parameters.Add(":doccode", OracleDbType.Raw).Value = doccode;
            this.cmd.Parameters.Add(":clsname", OracleDbType.Varchar2).Value = clsname;
            this.cmd.Parameters.Add(":docname", OracleDbType.Varchar2).Value = docname;
            this.cmd.Parameters.Add(":yct", OracleDbType.Varchar2).Value = yct;
            this.cmd.Parameters.Add(":ftlx", OracleDbType.Varchar2).Value = ftlx;
            OracleParameter parameter = this.cmd.Parameters.Add(":rs", OracleDbType.RefCursor, ParameterDirection.Output);
            OracleParameter parameter2 = this.cmd.Parameters.Add(":rsNum", OracleDbType.Decimal, ParameterDirection.Output);
            this.cmd.ExecuteNonQuery();
            OracleDecimal num2 = (OracleDecimal) parameter2.Value;
            if (num2.ToInt32() == 0)
            {
                return null;
            }
            OracleDataReader dataReader = (parameter.Value as OracleRefCursor).GetDataReader();
            DataSet set = new DataSet();
            DataTable table = new DataTable();
            table.Load(dataReader);
            set.Tables.Add(table);
            return set;
        }

        internal DataSet GetTsOrSentBom(Guid tsdOid, string tp)
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            this.cmd.Parameters.Add(":tsdoid", OracleDbType.Raw).Value = tsdOid.ToByteArray();
            this.cmd.Parameters.Add(":tp", OracleDbType.Varchar2).Value = tp;
            OracleParameter parameter = this.cmd.Parameters.Add(":rs", OracleDbType.RefCursor, ParameterDirection.Output);
            this.cmd.CommandText = "PLM_DQ_DOSSIER.GetTsdOrSentBom";
            this.cmd.Prepare();
            this.cmd.ExecuteNonQuery();
            OracleRefCursor cursor = parameter.Value as OracleRefCursor;
            DataTable table = new DataTable(tp);
            if (!cursor.IsNull)
            {
                table.Load(cursor.GetDataReader());
            }
            DataSet set = new DataSet();
            set.Tables.Add(table);
            set.AcceptChanges();
            return set;
        }

        internal DataSet GetTsRes(Hashtable hsGetResType)
        {
            DataSet set = new DataSet();
            if (hsGetResType.Count != 0)
            {
                this.cmd.Parameters.Clear();
                this.cmd.CommandType = CommandType.StoredProcedure;
                this.cmd.CommandText = "PLM_DQ_DOSSIER.GetTsAndSentRes";
                OracleParameter parameter = this.cmd.Parameters.Add(":tbname", OracleDbType.Varchar2);
                OracleParameter parameter2 = this.cmd.Parameters.Add(":Res", OracleDbType.RefCursor, ParameterDirection.Output);
                IDictionaryEnumerator enumerator = hsGetResType.GetEnumerator();
                enumerator.Reset();
                while (enumerator.MoveNext())
                {
                    string str = enumerator.Value.ToString();
                    string tableName = enumerator.Key.ToString();
                    parameter.Value = str;
                    this.cmd.ExecuteNonQuery();
                    OracleRefCursor cursor = parameter2.Value as OracleRefCursor;
                    if (!cursor.IsNull)
                    {
                        DataTable table = new DataTable(tableName);
                        OracleDataReader dataReader = cursor.GetDataReader();
                        table.Load(dataReader);
                        set.Tables.Add(table);
                        if (!dataReader.IsClosed)
                        {
                            dataReader.Close();
                        }
                    }
                }
                set.AcceptChanges();
            }
            return set;
        }

        public Hashtable GetViewOfCol(string tp, out ArrayList lstOrd, out Hashtable hswide)
        {
            lstOrd = new ArrayList();
            hswide = new Hashtable();
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            this.cmd.Parameters.Add(":v_type", OracleDbType.Varchar2).Value = tp;
            OracleParameter parameter = this.cmd.Parameters.Add(":rs", OracleDbType.RefCursor, ParameterDirection.Output);
            this.cmd.CommandText = "PLM_DQ_DOSSIER.GetViewCol";
            this.cmd.Prepare();
            this.cmd.ExecuteNonQuery();
            OracleDataReader dataReader = (parameter.Value as OracleRefCursor).GetDataReader();
            Hashtable hashtable = new Hashtable();
            while (dataReader.Read())
            {
                string str = dataReader.GetOracleString(0).Value;
                string str2 = dataReader.GetOracleString(1).Value;
                if (!dataReader.IsDBNull(2))
                {
                    int num = dataReader.GetOracleDecimal(2).ToInt32();
                    hswide[str2] = num;
                }
                lstOrd.Add(str2);
                hashtable[str] = str2;
            }
            return hashtable;
        }

        internal void PrintOrSentTsd(Guid useroid, DEBusinessItem item, string action, out StringBuilder strErr)
        {
            this.cmd.Parameters.Clear();
            strErr = new StringBuilder();
            this.cmd.CommandType = CommandType.StoredProcedure;
            Guid ssoid = Guid.NewGuid();
            this.cmd.CommandText = "PLM_DQ_DOSSIER.TsdToPrintOrSent";
            this.cmd.Parameters.Add(":ssoid", OracleDbType.Raw).Value = ssoid.ToByteArray();
            this.cmd.Parameters.Add(":tAction", OracleDbType.Varchar2).Value = action;
            this.cmd.Parameters.Add(":useroid", OracleDbType.Raw).Value = useroid.ToByteArray();
            this.cmd.Parameters.Add(":mOid", OracleDbType.Raw).Value = item.MasterOid.ToByteArray();
            this.cmd.Parameters.Add(":tId", OracleDbType.Varchar2).Value = item.Id;
            this.cmd.Parameters.Add(":clentip", OracleDbType.Varchar2).Value = DAItem.GetClientIp();
            this.cmd.ExecuteNonQuery();
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.Text;
            this.cmd.CommandText = "select PLM_ID,PLM_INFO from plm_tmp_archivmanage where plm_ssoid =:ssoid";
            this.cmd.Parameters.Add(":ssoid", OracleDbType.Raw).Value = ssoid.ToByteArray();
            OracleDataReader reader = this.cmd.ExecuteReader();
            while (reader.Read())
            {
                string str = reader.GetOracleString(0).Value;
                if (!reader.IsDBNull(1))
                {
                    strErr.Append("托晒单:" + str + ":");
                    strErr.Append(reader.GetOracleString(1).Value);
                }
            }
            this.ClearTmpData(ssoid);
        }

        private OracleCommand cmd
        {
            get
            {
                if (this._cmd == null)
                {
                    this._cmd = new OracleCommand();
                    this._cmd.Connection = this.dbparam.Connection as OracleConnection;
                }
                return this._cmd;
            }
        }

        internal void SignSentItem(DEBusinessItem item, string unit, string signer, string sm)
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            Guid ssoid = Guid.NewGuid();
            this.cmd.CommandText = "PLM_DQ_DOSSIER.SignSentItem";
            this.cmd.Parameters.Add(":ioid", OracleDbType.Raw).Value = item.IterOid.ToByteArray();
            this.cmd.Parameters.Add(":unit", OracleDbType.Varchar2).Value = unit;
            this.cmd.Parameters.Add(":signer", OracleDbType.Varchar2).Value = signer;
            this.cmd.Parameters.Add(":sm", OracleDbType.Varchar2).Value = sm;
            this.cmd.ExecuteNonQuery();
            
        }

        internal void SignSentBefore(string unit, string signer)
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            Guid ssoid = Guid.NewGuid();
            this.cmd.CommandText = "PLM_DQ_DOSSIER.SignSentBefore";
            this.cmd.Parameters.Add(":unit", OracleDbType.Varchar2).Value = unit;
            this.cmd.Parameters.Add(":sign", OracleDbType.Varchar2).Value = signer;
            this.cmd.ExecuteNonQuery();
        }

        internal DataSet GetSentLstSuiJi(string docCode, string wkinfo, string tsdId, string tUnit, string tstype, string sentstate, DateTime dFrom, DateTime dTo)
        {
            this.cmd.Parameters.Clear();
            this.cmd.CommandType = CommandType.StoredProcedure;
            DataSet set = new DataSet();
            this.cmd.CommandText = "PLM_DQ_DOSSIER.QuickSchSentSuiji";
            this.cmd.Parameters.Add(":DocCode", OracleDbType.Varchar2).Value = docCode;
            this.cmd.Parameters.Add(":BpmName", OracleDbType.Varchar2).Value = wkinfo;
            this.cmd.Parameters.Add(":TsdId", OracleDbType.Varchar2).Value = tsdId;
            this.cmd.Parameters.Add(":tUnit", OracleDbType.Varchar2).Value = tUnit;
            this.cmd.Parameters.Add(":TsType", OracleDbType.Varchar2).Value = tstype;
            this.cmd.Parameters.Add(":sentstate", OracleDbType.Varchar2).Value = sentstate;
            this.cmd.Parameters.Add(":FromTime", OracleDbType.Date).Value = new DateTime(dFrom.Year, dFrom.Month, dFrom.Day, 0, 0, 0);
            this.cmd.Parameters.Add(":ToTime", OracleDbType.Date).Value = new DateTime(dTo.Year, dTo.Month, dTo.Day, 0x17, 0x3b, 0x3b);
            OracleParameter parameter = this.cmd.Parameters.Add(":rs", OracleDbType.RefCursor, ParameterDirection.Output);
            this.cmd.ExecuteNonQuery();
            OracleDataReader dataReader = (parameter.Value as OracleRefCursor).GetDataReader();
            DataTable table = new DataTable("SENT");
            table.Load(dataReader);
            set.Tables.Add(table);
            return set;
        }
    }
}

