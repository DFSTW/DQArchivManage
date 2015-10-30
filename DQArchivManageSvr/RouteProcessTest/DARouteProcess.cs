using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DQ.Common.RouteProcess;
using Oracle.DataAccess.Client;
using Thyt.TiPLM.DAL.Common;

namespace RouteProcessTest
{
    class DARouteProcess :DABase
    {
        public DARouteProcess(DBParameter dbParam)
            : base(dbParam)
        {
        }

        public DEFullRelationBizItem1 GetPSRootItem(string prjId, long child_pos_id, out Guid masterOid, out int rev)
        {
            OracleCommand command = new OracleCommand
            {
                Connection = (OracleConnection)base.dbParam.Connection,
                CommandText = "select t.*,rownum plm_index from (select * from  plm_cus_c_taskroute c where c.plm_project_id=:prjid) t  where t.plm_child_posid=:childposid "
            };
            command.Parameters.Add(":prjid", OracleDbType.NVarchar2).Value = prjId;
            command.Parameters.Add(":child_pos_id", OracleDbType.Int64).Value = child_pos_id;
            OracleDataReader reader = null;
            List<DEFullRelationBizItem1> list = new List<DEFullRelationBizItem1>();
            masterOid = Guid.Empty;
            rev = 0;
            try
            {
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DEFullRelationBizItem1 item = new DEFullRelationBizItem1
                    {
                        oid = new Guid((byte[])reader.GetValue(0)),
                        child_posid = child_pos_id,
                        parnte_posid = Convert.ToInt64(reader.GetValue(2))
                    };
                    masterOid = item.partMasterOid = new Guid((byte[])reader.GetValue(3));
                    rev = item.partRev = reader.GetInt32(4);
                    item.parentMasterOid = new Guid((byte[])reader.GetValue(8));
                    item.paranteRevm = reader.GetInt32(9);
                    item.parentId = reader.GetString(10);
                    item.parentName = reader.GetString(11);
                    item.relaOid = new Guid((byte[])reader.GetValue(12));
                    item.Number = reader.GetDecimal(13);
                    item.order = reader.GetInt32(14);
                    item.Level = reader.GetInt32(15);
                    item.prjMasterOid = new Guid((byte[])reader.GetValue(0x10));
                    item.prjId = reader.GetString(0x11);
                    item.id_path = reader.GetString(0x19);
                    item.moid_path = reader.GetString(0x1a);
                    item.index = reader.GetInt32(50);
                    item.Creator = reader.GetString(0x12);
                    item.CreateTime = reader.GetDateTime(0x13);
                    return item;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return null;
        }

 

        public List<DEFullRelationBizItem1> GetPSItems(string prjId, Guid rootMasterOid, long child_pos_id, int level, out List<Guid> masterOids, out List<int> revs)
        {
            if (level == 0)
                level = 20;
            else
                level = level + 1;
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = (OracleConnection)this.dbParam.Connection;

            cmd.CommandText = "select t.*,rownum plm_index from " +
            "(select * from  plm_cus_c_taskroute c where c.plm_project_id=:prjid  ) t " +
            "start with t.plm_child_posid=:childposid " +
            "connect by prior t.plm_child_posid=t.plm_parent_posid and level<=:level1 ";
            if (level == 20)
                cmd.CommandText = cmd.CommandText + "order by plm_index";
            else
                cmd.CommandText = cmd.CommandText + " order by plm_level,plm_order ";


            cmd.Parameters.Add(":prjid", OracleDbType.Varchar2).Value = prjId;
            cmd.Parameters.Add(":child_pos_id", OracleDbType.Int64).Value = child_pos_id;
            cmd.Parameters.Add(":level1", OracleDbType.Int32).Value = level;

            OracleDataReader dr = null;
            List<DEFullRelationBizItem1> lst = new List<DEFullRelationBizItem1>();
            List<Guid> l_masterOids = new List<Guid>();
            List<int> l_revs = new List<int>();
            try
            {
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    DEFullRelationBizItem1 rpItem = new DEFullRelationBizItem1();
                    rpItem.oid = new Guid((Byte[])dr.GetValue(0));
                    rpItem.child_posid = Convert.ToInt64(dr.GetValue(1));
                    rpItem.parnte_posid = Convert.ToInt64(dr.GetValue(2));
                    rpItem.partMasterOid = new Guid((Byte[])dr.GetValue(3));

                    rpItem.partRev = dr.GetInt32(4);
                    if (!l_masterOids.Contains(rpItem.partMasterOid))
                    {
                        l_masterOids.Add(rpItem.partMasterOid);
                        l_revs.Add(rpItem.partRev);
                    }

                    rpItem.parentMasterOid = new Guid((Byte[])dr.GetValue(8));
                    rpItem.paranteRevm = dr.GetInt32(9);
                    rpItem.parentId = dr.GetString(10);
                    rpItem.parentName = dr.GetString(11);

                    rpItem.relaOid = new Guid((Byte[])dr.GetValue(12));
                    if (!dr.IsDBNull(13))
                        rpItem.Number = dr.GetDecimal(13);
                    if (!dr.IsDBNull(14))
                        rpItem.order = dr.GetInt32(14);
                    if (!dr.IsDBNull(15))
                        rpItem.Level = dr.GetInt32(15);
                    rpItem.prjMasterOid = new Guid((Byte[])dr.GetValue(16));
                    rpItem.prjId = dr.GetString(17);

                    rpItem.moid_path = dr.GetString(25);
                    rpItem.id_path = dr.GetString(26);
                    rpItem.index = (int)dr.GetDecimal(50);
                    rpItem.Creator = dr.GetString(18);
                    rpItem.CreateTime = dr.GetDateTime(19);

                    if (rpItem != null)
                        lst.Add(rpItem);
                }
                masterOids = l_masterOids;
                revs = l_revs;
            }
            finally
            {
                if (dr != null)
                    dr.Close();
            }

            return lst;
        }

    }
}
