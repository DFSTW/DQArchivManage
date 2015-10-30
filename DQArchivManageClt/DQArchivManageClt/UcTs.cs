namespace DQArchivManageClt
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;
    using Thyt.TiPLM.DEL.Product;
    using Thyt.TiPLM.PLL.Admin.NewResponsibility;
    using Thyt.TiPLM.PLL.Product2;
    using Thyt.TiPLM.UIL.Common;
    using Thyt.TiPLM.UIL.Product.Common;

    public class UcTs : UserControl
    {
        private Button btnTsClear;
        private Button btnTsQiuckSch;
        private Button btnTsSchDocId;
        private Button btnUpdateBpm;
        private ContextMenuStrip cMenuTSD;
        private IContainer components = null;
        private ToolStripMenuItem D_ADD_TSD;
        private PLMBizItemDelegate d_AfterAbandon;
        private PLMBizItemDelegate d_AfterCheckIn;
        private PLMBizItemDelegate d_AfterCheckOut;
        private PLMDelegate2 d_AfterDeleted;
        private PLMBizItemDelegate d_AfterIterationUpdated;
        private PLMBizItemDelegate d_AfterMasterUpdated;
        private PLMBizItemDelegate d_AfterReleased;
        private PLMBizItemDelegate d_AfterRevisionCreated;
        private PLMBizItemDelegate d_AfterUndoAbandon;
        private PLMBizItemDelegate d_AfterUndoCheckOut;
        private PLMBizItemDelegate d_AfterUndoNewRevision;
        private ToolStripMenuItem D_DEL_TSD;
        private ToolStripMenuItem D_OPEN_TSD;
        private ToolStripMenuItem D_TOPRINT_TSD;
        private ToolStripMenuItem D_TOSENT_TSD;
        private Hashtable hsCols = null;
        private Hashtable hsColWide;
        private Label label1;
        private Label lbTsId;
        private ArrayList lstCando;
        private ArrayList lstOrder = null;
        private PLMSimpleDelegate Lvw_AfterTabClose = null;
        private PLMSimpleDelegate Lvw_AfterTsdCreate = null;
        private SortableListView lvwTSD;
        private Panel panel1;
        private Panel pnlTsSch;
        private ResWkInfo resWkTsd;
        private TabControl tbCtrlTS;
        private TabPage tPTsdLst;
        private TextBox txtTsID;

        public UcTs()
        {
            this.InitializeComponent();
            this.InitTsdlvw();
        }

        private void AfterDeleted(object sender, PLMOperationArgs e)
        {
            if ((this.lvwTSD.Items.Count != 0) && (((e != null) && (e.BizItems != null)) && (e.BizItems.Length != 0)))
            {
                ArrayList list = new ArrayList(e.BizItems);
                for (int i = 0; i < this.lvwTSD.Items.Count; i++)
                {
                    ListViewItem item = this.lvwTSD.Items[i];
                    DEBusinessItem dest = null;
                    if (item.Tag is DEBusinessItem)
                    {
                        dest = (DEBusinessItem) item.Tag;
                    }
                    else if (item.Tag is DERelationBizItem)
                    {
                        dest = ((DERelationBizItem) item.Tag).BizItem;
                    }
                    if (dest != null)
                    {
                        if (PSStart.EqualMaster((IBizItem[]) list.ToArray(typeof(IBizItem)), dest) != null)
                        {
                            this.lvwTSD.Items.RemoveAt(i);
                            i--;
                        }
                        this.CloseTsdTab(dest.Id);
                    }
                }
            }
        }

        private void AfterItemUpdated(IBizItem[] bizItems)
        {
            if ((bizItems != null) && (bizItems.Length != 0))
            {
                ArrayList list = new ArrayList(PSConvert.ToBizItems(bizItems, ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid));
                foreach (DEBusinessItem item in list)
                {
                    if (item.ClassName != "DQDOSSIERPRINT")
                    {
                        break;
                    }
                    string tsStatue = PlArchivManage.GetTsStatue(item);
                    string str2 = tsStatue;
                    if ((str2 == null) || ((str2 != "打印完成") && (str2 != "打印取消")))
                    {
                        if (tsStatue == "开始打印")
                        {
                            bool flag = false;
                            foreach (ListViewItem item2 in this.lvwTSD.Items)
                            {
                                DEBusinessItem tag = item2.Tag as DEBusinessItem;
                                if (tag.MasterOid == item.MasterOid)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                continue;
                            }
                        }
                        this.UpdateTsdItem(item);
                    }
                }
            }
        }

        private void btnTsClear_Click(object sender, EventArgs e)
        {
            this.lvwTSD.Items.Clear();
        }

        private void btnTsQiuckSch_Click(object sender, EventArgs e)
        {
            FrmTsdQuickSch sch = new FrmTsdQuickSch();
            if (sch.ShowDialog() == DialogResult.OK)
            {
                DataSet ds = sch.ds;
                this.lvwTSD.Items.Clear();
                if (((ds != null) && (ds.Tables.Count > 0)) && (ds.Tables[0].Rows.Count > 0))
                {
                    PlArchivManage.SetLvwClsValues(this.hsCols, this.lvwTSD, this.lstOrder, ds.Tables[0], "DQDOSSIERPRINT");
                }
                this.lvwTSD.Refresh();
            }
        }

        private void btnTsSchDocId_Click(object sender, EventArgs e)
        {
            this.lvwTSD.Items.Clear();
            DataSet tSD = PlArchivManage.Agent.GetTSD(this.txtTsID.Text, this.resWkTsd.ResValue);
            if (((tSD != null) && (tSD.Tables.Count > 0)) && (tSD.Tables[0].Rows.Count > 0))
            {
                PlArchivManage.SetLvwClsValues(this.hsCols, this.lvwTSD, this.lstOrder, tSD.Tables[0], "DQDOSSIERPRINT");
            }
            this.lvwTSD.Refresh();
        }

        private void btnUpdateBpm_Click(object sender, EventArgs e)
        {
            PlArchivManage.SetBpmInfo(this.resWkTsd, true);
        }

        private void ckUpdateBPM_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void CloseTsdTab(object obj)
        {
            TabPage page = obj as TabPage;
            if (page != null)
            {
                this.tbCtrlTS.TabPages.Remove(page);
            }
            else
            {
                string str = obj.ToString();
                for (int i = 0; i < this.tbCtrlTS.TabPages.Count; i++)
                {
                    page = this.tbCtrlTS.TabPages[i];
                    if (page.Text == str)
                    {
                        this.tbCtrlTS.TabPages.Remove(page);
                        break;
                    }
                }
            }
        }

        private void D_ADD_TSD_Click(object sender, EventArgs e)
        {
            this.OpenTsdEdit(null);
        }

        private void D_DEL_TSD_Click(object sender, EventArgs e)
        {
            DEBusinessItem tag;
            ArrayList list = new ArrayList();
            ArrayList masterOids = new ArrayList();
            foreach (ListViewItem item2 in this.lvwTSD.SelectedItems)
            {
                tag = item2.Tag as DEBusinessItem;
                if ((((tag.State != ItemState.Release) && ((tag.State != ItemState.CheckOut) || (tag.Holder == ClientData.LogonUser.Oid))) && (tag.Phase == Guid.Empty)) && (tag.RevNum <= 1))
                {
                    list.Add(tag);
                    masterOids.Add(tag.MasterOid);
                }
            }
            if (list.Count == 0)
            {
                MessageBox.Show("没有能够删除的托晒单！", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            else if (MessageBox.Show("是否要删除选中托晒单 ？删除后不能恢复！", "警告！", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
            {
                Hashtable hashtable;
                PLItem.Agent.DeleteItems(masterOids, ClientData.LogonUser.Oid, out hashtable);
                for (int i = 0; i < this.lvwTSD.Items.Count; i++)
                {
                    tag = this.lvwTSD.Items[i].Tag as DEBusinessItem;
                    if (masterOids.Contains(tag.MasterOid))
                    {
                        this.lvwTSD.Items.RemoveAt(i);
                        i--;
                        this.CloseTsdTab(tag.Id);
                    }
                }
                this.lvwTSD.Refresh();
            }
        }

        private void D_OPEN_TSD_Click(object sender, EventArgs e)
        {
            if (this.lvwTSD.SelectedItems.Count == 1)
            {
                ListViewItem item = this.lvwTSD.SelectedItems[0];
                DEBusinessItem tag = item.Tag as DEBusinessItem;
                this.OpenTsdEdit(tag);
            }
        }

        private void D_OutPut_TSD_Click(object sender, EventArgs e)
        {
            TsdOutPut put = new TsdOutPut();
            foreach (ListViewItem item in this.lvwTSD.SelectedItems)
            {
                DEBusinessItem tag = item.Tag as DEBusinessItem;
                string wk = (tag.Iteration.GetAttrValue("WKFLINFO") == null) ? "" : tag.Iteration.GetAttrValue("WKFLINFO").ToString();
                put.StartOutPut(tag.IterOid, tag, wk);
            }
        }

        private void D_TOPRINT_TSD_Click(object sender, EventArgs e)
        {
            ArrayList lstItems = new ArrayList();
            foreach (ListViewItem item in this.lvwTSD.SelectedItems)
            {
                DEBusinessItem tag = item.Tag as DEBusinessItem;
                if ((tag.State == ItemState.CheckOut) && (tag.Holder == ClientData.LogonUser.Oid))
                {
                    PLItem.Agent.CheckIn(tag.MasterOid, tag.ClassName, ClientData.LogonUser.Oid, "");
                    lstItems.Add(tag);
                }
                if (tag.State == ItemState.CheckIn)
                {
                    lstItems.Add(tag);
                }
            }
            if (lstItems.Count == 0)
            {
                MessageBox.Show("没有能够打印的托晒单！", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            else
            {
                StringBuilder builder;
                ArrayList list2 = PlArchivManage.Agent.CheckTsdRight(lstItems, "ToPrint", out builder, "托晒");
                if (list2.Count == 0)
                {
                    FrmArchivManage.frmMian.DisplayTextInRichtBox("没有数据可以发送打印：\r\n" + builder.ToString(), 0, true);
                }
                else
                {
                    if (builder.Length > 0)
                    {
                        DialogResult result = MessageBox.Show("发现数据错误，见详细信息中的内容，是否继续？", "发送打印错误", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("下列数据无法 发送打印：\r\n" + builder.ToString(), 0, result == DialogResult.No);
                        if (result != DialogResult.Yes)
                        {
                            return;
                        }
                    }
                    StringBuilder builder2 = new StringBuilder();
                    for (int i = 0; i < list2.Count; i++)
                    {
                        StringBuilder builder3;
                        DEBusinessItem item3 = list2[i] as DEBusinessItem;
                        PlArchivManage.Agent.PrintOrSentTsd(ClientData.LogonUser.Oid, item3, "ToPrint", out builder3);
                        if (builder3.Length > 0)
                        {
                            builder.Append(builder3.ToString());
                        }
                        else
                        {
                            item3 = PLItem.Agent.GetBizItem(item3.MasterOid, 0, 0, ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid, BizItemMode.BizItem) as DEBusinessItem;
                            string attrValue = (item3.Iteration.GetAttrValue("SM") == null) ? "" : item3.Iteration.GetAttrValue("SM").ToString();
                            attrValue = attrValue + " " + ClientData.LogonUser.Name + ":发送打印";
                            item3.Iteration.SetAttrValue("SM", attrValue);
                            item3.Iteration = PLItem.UpdateItemIterationDirectly(item3, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption, false);
                            if (BizItemHandlerEvent.Instance.D_AfterIterationUpdated != null)
                            {
                                BizItemHandlerEvent.Instance.D_AfterIterationUpdated(BizOperationHelper.ConvertPLMBizItemDelegateParam(item3));
                            }
                            builder2.Append("\t" + item3.Id);
                        }
                    }
                    if (builder.Length > 0)
                    {
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("下列托晒单发送打印没有成功\r\n" + builder.ToString(), 0, true);
                    }
                    if (builder2.Length > 0)
                    {
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("下列托晒单发送打印成功\r\n" + builder2.ToString(), 1, true);
                    }
                }
            }
        }

        private void D_TOSENT_TSD_Click(object sender, EventArgs e)
        {
            ArrayList lstItems = new ArrayList();
            foreach (ListViewItem item in this.lvwTSD.SelectedItems)
            {
                DEBusinessItem tag = item.Tag as DEBusinessItem;
                if (((tag.State == ItemState.CheckOut) && (tag.Holder == ClientData.LogonUser.Oid)) || (tag.State == ItemState.CheckIn))
                {
                    lstItems.Add(tag);
                }
            }
            if (lstItems.Count == 0)
            {
                MessageBox.Show("没有能够回收的托晒单！", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            else
            {
                StringBuilder builder;
                ArrayList list2 = PlArchivManage.Agent.CheckTsdRight(lstItems, "ToSent", out builder, "托晒");
                if (list2.Count == 0)
                {
                    FrmArchivManage.frmMian.DisplayTextInRichtBox("没有数据可以发送回收：\r\n" + builder.ToString(), 0, true);
                }
                else
                {
                    if (builder.Length > 0)
                    {
                        DialogResult result = MessageBox.Show("发现数据错误，见详细信息中的内容，是否继续？", "发送托晒单直接回收错误", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("下列数据无法 直接回收：\r\n" + builder.ToString(), 0, result == DialogResult.No);
                        if (result != DialogResult.Yes)
                        {
                            return;
                        }
                    }
                    StringBuilder builder2 = new StringBuilder();
                    StringBuilder builder3 = new StringBuilder();
                    for (int i = 0; i < list2.Count; i++)
                    {
                        StringBuilder builder4;
                        DEBusinessItem item3 = list2[i] as DEBusinessItem;
                        PlArchivManage.Agent.PrintOrSentTsd(ClientData.LogonUser.Oid, item3, "ToSent", out builder4);
                        if (builder4.Length > 0)
                        {
                            builder.Append(builder4.ToString());
                        }
                        else
                        {
                            item3 = PLItem.Agent.GetBizItem(item3.MasterOid, 0, 0, ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid, BizItemMode.BizItem) as DEBusinessItem;
                            try
                            {
                                PlArchivManage.CommitWorkItem(item3);
                            }
                            catch (Exception exception)
                            {
                                builder3.Append("\t" + item3.Id + ":" + exception.Message + "\r\n\t    " + exception.ToString());
                            }
                            if (BizItemHandlerEvent.Instance.D_AfterReleased != null)
                            {
                                BizItemHandlerEvent.Instance.D_AfterReleased(BizOperationHelper.ConvertPLMBizItemDelegateParam(item3));
                            }
                            builder2.Append("\t" + item3.Id);
                        }
                    }
                    if (builder.Length > 0)
                    {
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("下列托晒单直接回收没有成功\r\n" + builder.ToString(), 0, true);
                    }
                    if (builder2.Length > 0)
                    {
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("下列托晒单直接回收成功\r\n" + builder2.ToString(), 1, true);
                    }
                    if (builder3.Length > 0)
                    {
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("下列托晒单已发送回收，但提交流程失败\r\n" + builder3.ToString(), 2, true);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            BizItemHandlerEvent.Instance.D_AfterReleased = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterReleased, this.d_AfterReleased);
            BizItemHandlerEvent.Instance.D_AfterRevisionCreated = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterRevisionCreated, this.d_AfterRevisionCreated);
            BizItemHandlerEvent.Instance.D_AfterUndoNewRevision = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterUndoNewRevision, this.d_AfterUndoNewRevision);
            BizItemHandlerEvent.Instance.D_AfterCheckIn = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterCheckIn, this.d_AfterCheckIn);
            BizItemHandlerEvent.Instance.D_AfterCheckOut = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterCheckOut, this.d_AfterCheckOut);
            BizItemHandlerEvent.Instance.D_AfterUndoCheckOut = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterUndoCheckOut, this.d_AfterUndoCheckOut);
            BizItemHandlerEvent.Instance.D_AfterIterationUpdated = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterIterationUpdated, this.d_AfterIterationUpdated);
            BizItemHandlerEvent.Instance.D_AfterMasterUpdated = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterMasterUpdated, this.d_AfterMasterUpdated);
            BizItemHandlerEvent.Instance.D_AfterDeleted = (PLMDelegate2) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterDeleted, this.d_AfterDeleted);
            BizItemHandlerEvent.Instance.D_AfterUndoCheckOut = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterUndoCheckOut, this.d_AfterUndoCheckOut);
            BizItemHandlerEvent.Instance.D_AfterUndoAbandon = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterUndoAbandon, this.d_AfterUndoAbandon);
            BizItemHandlerEvent.Instance.D_AfterAbandon = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterAbandon, this.d_AfterAbandon);
            DelegatesOfAm.Instance.D_AfterTsdTabClose = (PLMSimpleDelegate) Delegate.Remove(DelegatesOfAm.Instance.D_AfterTsdTabClose, this.Lvw_AfterTabClose);
            DelegatesOfAm.Instance.D_AfterTsdCreate = (PLMSimpleDelegate) Delegate.Remove(DelegatesOfAm.Instance.D_AfterTsdCreate, this.Lvw_AfterTsdCreate);
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void GetCando()
        {
            if (PLGrantPerm.CanDoClassOperation(ClientData.LogonUser.Oid, "DQDOSSIERPRINT", Guid.Empty, "ClaRel_CREATE") == 1)
            {
                this.lstCando.Add(this.D_ADD_TSD);
            }
            if (PLGrantPerm.CanDoClassOperation(ClientData.LogonUser.Oid, "DQDOSSIERPRINT", Guid.Empty, "ClaRel_BROWSE") == 1)
            {
                this.lstCando.Add(this.D_OPEN_TSD);
            }
            if (PLGrantPerm.CanDoClassOperation(ClientData.LogonUser.Oid, "DQDOSSIERPRINT", Guid.Empty, "ClaRel_DELETE") == 1)
            {
                this.lstCando.Add(this.D_DEL_TSD);
            }
            if (PLGrantPerm.CanDoClassOperation(ClientData.LogonUser.Oid, "DQDOSSIERPRINT", Guid.Empty, "ClaRel_CHECKIN") == 1)
            {
                this.lstCando.Add(this.D_TOPRINT_TSD);
            }
            if (PLGrantPerm.CanDoClassOperation(ClientData.LogonUser.Oid, "DQDOSSIERPRINT", Guid.Empty, "ClaRel_RELEASE") == 1)
            {
                this.lstCando.Add(this.D_TOSENT_TSD);
            }
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.tbCtrlTS = new TabControl();
            this.tPTsdLst = new TabPage();
            this.lvwTSD = new SortableListView();
            this.pnlTsSch = new Panel();
            this.label1 = new Label();
            this.btnTsSchDocId = new Button();
            this.btnTsClear = new Button();
            this.btnTsQiuckSch = new Button();
            this.lbTsId = new Label();
            this.txtTsID = new TextBox();
            this.cMenuTSD = new ContextMenuStrip(this.components);
            this.D_OPEN_TSD = new ToolStripMenuItem();
            this.D_DEL_TSD = new ToolStripMenuItem();
            this.D_ADD_TSD = new ToolStripMenuItem();
            this.D_TOPRINT_TSD = new ToolStripMenuItem();
            this.D_TOSENT_TSD = new ToolStripMenuItem();
            this.panel1 = new Panel();
            this.btnUpdateBpm = new Button();
            this.tbCtrlTS.SuspendLayout();
            this.tPTsdLst.SuspendLayout();
            this.pnlTsSch.SuspendLayout();
            this.cMenuTSD.SuspendLayout();
            base.SuspendLayout();
            this.tbCtrlTS.Controls.Add(this.tPTsdLst);
            this.tbCtrlTS.Dock = DockStyle.Fill;
            this.tbCtrlTS.Location = new Point(0, 0);
            this.tbCtrlTS.Name = "tbCtrlTS";
            this.tbCtrlTS.SelectedIndex = 0;
            this.tbCtrlTS.Size = new Size(0x2ff, 0x1b1);
            this.tbCtrlTS.TabIndex = 0;
            this.tPTsdLst.Controls.Add(this.lvwTSD);
            this.tPTsdLst.Controls.Add(this.pnlTsSch);
            this.tPTsdLst.Location = new Point(4, 0x16);
            this.tPTsdLst.Name = "tPTsdLst";
            this.tPTsdLst.Padding = new Padding(3);
            this.tPTsdLst.Size = new Size(0x2f7, 0x197);
            this.tPTsdLst.TabIndex = 0;
            this.tPTsdLst.Text = "托晒单列表";
            this.tPTsdLst.UseVisualStyleBackColor = true;
            this.lvwTSD.AllowDrop = true;
            this.lvwTSD.Dock = DockStyle.Fill;
            this.lvwTSD.FullRowSelect = true;
            this.lvwTSD.HideSelection = false;
            this.lvwTSD.Location = new Point(3, 0x2e);
            this.lvwTSD.Name = "lvwTSD";
            this.lvwTSD.Size = new Size(0x2f1, 0x166);
            this.lvwTSD.SortingOrder = SortOrder.None;
            this.lvwTSD.TabIndex = 1;
            this.lvwTSD.UseCompatibleStateImageBehavior = false;
            this.lvwTSD.View = View.Details;
            this.lvwTSD.DragDrop += new DragEventHandler(this.lvwTSD_DragDrop);
            this.lvwTSD.DragEnter += new DragEventHandler(this.lvwTSD_DragEnter);
            this.lvwTSD.DoubleClick += new EventHandler(this.lvwTSD_DoubleClick);
            this.lvwTSD.MouseUp += new MouseEventHandler(this.lvwTSD_MouseUp);
            this.pnlTsSch.BackColor = Color.WhiteSmoke;
            this.pnlTsSch.Controls.Add(this.btnUpdateBpm);
            this.pnlTsSch.Controls.Add(this.panel1);
            this.pnlTsSch.Controls.Add(this.label1);
            this.pnlTsSch.Controls.Add(this.btnTsSchDocId);
            this.pnlTsSch.Controls.Add(this.btnTsClear);
            this.pnlTsSch.Controls.Add(this.btnTsQiuckSch);
            this.pnlTsSch.Controls.Add(this.lbTsId);
            this.pnlTsSch.Controls.Add(this.txtTsID);
            this.pnlTsSch.Dock = DockStyle.Top;
            this.pnlTsSch.Location = new Point(3, 3);
            this.pnlTsSch.Name = "pnlTsSch";
            this.pnlTsSch.Size = new Size(0x2f1, 0x2b);
            this.pnlTsSch.TabIndex = 0;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0xcd, 15);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x35, 12);
            this.label1.TabIndex = 0x20;
            this.label1.Text = "流程名称";
            this.btnTsSchDocId.Anchor = AnchorStyles.Right;
            this.btnTsSchDocId.Font = new Font("宋体", 9f, FontStyle.Bold, GraphicsUnit.Point, 0x86);
            this.btnTsSchDocId.Location = new Point(0x20c, 9);
            this.btnTsSchDocId.Name = "btnTsSchDocId";
            this.btnTsSchDocId.Size = new Size(0x23, 0x17);
            this.btnTsSchDocId.TabIndex = 0x1f;
            this.btnTsSchDocId.Text = "...";
            this.btnTsSchDocId.UseVisualStyleBackColor = true;
            this.btnTsSchDocId.Click += new EventHandler(this.btnTsSchDocId_Click);
            this.btnTsClear.Anchor = AnchorStyles.Right;
            this.btnTsClear.Location = new Point(660, 9);
            this.btnTsClear.Name = "btnTsClear";
            this.btnTsClear.Size = new Size(0x4b, 0x17);
            this.btnTsClear.TabIndex = 30;
            this.btnTsClear.Text = "清空";
            this.btnTsClear.UseVisualStyleBackColor = true;
            this.btnTsClear.Click += new EventHandler(this.btnTsClear_Click);
            this.btnTsQiuckSch.Anchor = AnchorStyles.Right;
            this.btnTsQiuckSch.Location = new Point(0x240, 8);
            this.btnTsQiuckSch.Name = "btnTsQiuckSch";
            this.btnTsQiuckSch.Size = new Size(0x4e, 0x17);
            this.btnTsQiuckSch.TabIndex = 0x1c;
            this.btnTsQiuckSch.Text = "快速查询";
            this.btnTsQiuckSch.UseVisualStyleBackColor = true;
            this.btnTsQiuckSch.Click += new EventHandler(this.btnTsQiuckSch_Click);
            this.lbTsId.AutoSize = true;
            this.lbTsId.Location = new Point(3, 13);
            this.lbTsId.Name = "lbTsId";
            this.lbTsId.Size = new Size(0x1d, 12);
            this.lbTsId.TabIndex = 0x18;
            this.lbTsId.Text = "图号";
            this.txtTsID.Location = new Point(0x26, 10);
            this.txtTsID.Name = "txtTsID";
            this.txtTsID.Size = new Size(0xa1, 0x15);
            this.txtTsID.TabIndex = 0x17;
            this.cMenuTSD.Items.AddRange(new ToolStripItem[] { this.D_OPEN_TSD, this.D_DEL_TSD, this.D_ADD_TSD, this.D_TOPRINT_TSD, this.D_TOSENT_TSD });
            this.cMenuTSD.Name = "cMenuTSD";
            this.cMenuTSD.Size = new Size(0x89, 0x72);
            this.D_OPEN_TSD.Name = "D_OPEN_TSD";
            this.D_OPEN_TSD.Size = new Size(0x88, 0x16);
            this.D_OPEN_TSD.Text = "打开托晒单";
            this.D_OPEN_TSD.Click += new EventHandler(this.D_OPEN_TSD_Click);
            this.D_DEL_TSD.Name = "D_DEL_TSD";
            this.D_DEL_TSD.Size = new Size(0x88, 0x16);
            this.D_DEL_TSD.Text = "删除托晒单";
            this.D_DEL_TSD.Click += new EventHandler(this.D_DEL_TSD_Click);
            this.D_ADD_TSD.Name = "D_ADD_TSD";
            this.D_ADD_TSD.Size = new Size(0x88, 0x16);
            this.D_ADD_TSD.Text = "新增托晒单";
            this.D_ADD_TSD.Click += new EventHandler(this.D_ADD_TSD_Click);
            this.D_TOPRINT_TSD.Name = "D_TOPRINT_TSD";
            this.D_TOPRINT_TSD.Size = new Size(0x88, 0x16);
            this.D_TOPRINT_TSD.Text = "发 送 打 印";
            this.D_TOPRINT_TSD.ToolTipText = "定版托晒单";
            this.D_TOPRINT_TSD.Click += new EventHandler(this.D_TOPRINT_TSD_Click);
            this.D_TOSENT_TSD.Name = "D_TOSENT_TSD";
            this.D_TOSENT_TSD.Size = new Size(0x88, 0x16);
            this.D_TOSENT_TSD.Text = "直 接 回 收";
            this.D_TOSENT_TSD.Click += new EventHandler(this.D_TOSENT_TSD_Click);
            this.panel1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.panel1.Location = new Point(0x108, 8);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0xb0, 0x20);
            this.panel1.TabIndex = 0x21;
            this.btnUpdateBpm.Anchor = AnchorStyles.Right;
            this.btnUpdateBpm.Location = new Point(0x1c7, 10);
            this.btnUpdateBpm.Name = "btnUpdateBpm";
            this.btnUpdateBpm.Size = new Size(0x3f, 0x17);
            this.btnUpdateBpm.TabIndex = 0x22;
            this.btnUpdateBpm.Text = "更新流程";
            this.btnUpdateBpm.UseVisualStyleBackColor = true;
            this.btnUpdateBpm.Click += new EventHandler(this.btnUpdateBpm_Click);
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            base.Controls.Add(this.tbCtrlTS);
            base.Name = "UcTs";
            base.Size = new Size(0x2ff, 0x1b1);
            base.Load += new EventHandler(this.UcTs_Load);
            this.tbCtrlTS.ResumeLayout(false);
            this.tPTsdLst.ResumeLayout(false);
            this.pnlTsSch.ResumeLayout(false);
            this.pnlTsSch.PerformLayout();
            this.cMenuTSD.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void InitTsdlvw()
        {
            this.hsCols = PlArchivManage.Agent.GetViewOfCol("托晒单", out this.lstOrder, out this.hsColWide);
            PlArchivManage.SetCol(this.hsCols, this.lvwTSD, "TSD", this.lstOrder, this.hsColWide);
            this.lstCando = new ArrayList();
            this.GetCando();
            this.d_AfterIterationUpdated = new PLMBizItemDelegate(this.AfterItemUpdated);
            this.d_AfterMasterUpdated = new PLMBizItemDelegate(this.AfterItemUpdated);
            this.d_AfterDeleted = new PLMDelegate2(this.AfterDeleted);
            this.d_AfterRevisionCreated = new PLMBizItemDelegate(this.AfterItemUpdated);
            this.d_AfterReleased = new PLMBizItemDelegate(this.AfterItemUpdated);
            this.d_AfterUndoNewRevision = new PLMBizItemDelegate(this.AfterItemUpdated);
            this.d_AfterCheckIn = new PLMBizItemDelegate(this.AfterItemUpdated);
            this.d_AfterCheckOut = new PLMBizItemDelegate(this.AfterItemUpdated);
            this.d_AfterUndoCheckOut = new PLMBizItemDelegate(this.AfterItemUpdated);
            this.d_AfterAbandon = new PLMBizItemDelegate(this.AfterItemUpdated);
            this.d_AfterUndoAbandon = new PLMBizItemDelegate(this.AfterItemUpdated);
            this.Lvw_AfterTabClose = new PLMSimpleDelegate(this.CloseTsdTab);
            this.Lvw_AfterTsdCreate = new PLMSimpleDelegate(this.ItemCreated);
            BizItemHandlerEvent.Instance.D_AfterReleased = (PLMBizItemDelegate) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterReleased, this.d_AfterReleased);
            BizItemHandlerEvent.Instance.D_AfterRevisionCreated = (PLMBizItemDelegate) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterRevisionCreated, this.d_AfterRevisionCreated);
            BizItemHandlerEvent.Instance.D_AfterUndoNewRevision = (PLMBizItemDelegate) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterUndoNewRevision, this.d_AfterUndoNewRevision);
            BizItemHandlerEvent.Instance.D_AfterCheckIn = (PLMBizItemDelegate) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterCheckIn, this.d_AfterCheckIn);
            BizItemHandlerEvent.Instance.D_AfterCheckOut = (PLMBizItemDelegate) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterCheckOut, this.d_AfterCheckOut);
            BizItemHandlerEvent.Instance.D_AfterUndoCheckOut = (PLMBizItemDelegate) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterUndoCheckOut, this.d_AfterUndoCheckOut);
            BizItemHandlerEvent.Instance.D_AfterIterationUpdated = (PLMBizItemDelegate) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterIterationUpdated, this.d_AfterIterationUpdated);
            BizItemHandlerEvent.Instance.D_AfterMasterUpdated = (PLMBizItemDelegate) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterMasterUpdated, this.d_AfterMasterUpdated);
            BizItemHandlerEvent.Instance.D_AfterDeleted = (PLMDelegate2) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterDeleted, this.d_AfterDeleted);
            BizItemHandlerEvent.Instance.D_AfterUndoAbandon = (PLMBizItemDelegate) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterUndoAbandon, this.d_AfterUndoAbandon);
            BizItemHandlerEvent.Instance.D_AfterAbandon = (PLMBizItemDelegate) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterAbandon, this.d_AfterAbandon);
            DelegatesOfAm.Instance.D_AfterTsdTabClose = (PLMSimpleDelegate) Delegate.Combine(DelegatesOfAm.Instance.D_AfterTsdTabClose, this.Lvw_AfterTabClose);
            DelegatesOfAm.Instance.D_AfterTsdCreate = (PLMSimpleDelegate) Delegate.Combine(DelegatesOfAm.Instance.D_AfterTsdCreate, this.Lvw_AfterTsdCreate);
        }

        private void ItemCreated(object obj)
        {
            DEBusinessItem item = obj as DEBusinessItem;
            if (item != null)
            {
                this.UpdateTsdItem(obj);
                this.CloseTsdTab("新建托晒单");
                this.OpenTsdEdit(item);
            }
        }

        private void lvwTSD_DoubleClick(object sender, EventArgs e)
        {
            if ((this.lvwTSD.SelectedItems.Count == 1) && (PLGrantPerm.CanDoClassOperation(ClientData.LogonUser.Oid, "DQDOSSIERPRINT", Guid.Empty, "ClaRel_BROWSE") != 0))
            {
                ListViewItem item = this.lvwTSD.SelectedItems[0];
                IBizItem tag = item.Tag as IBizItem;
                this.OpenItem(sender, tag);
            }
        }

        private void lvwTSD_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data != null)
            {
                DEBusinessItem item;
                ArrayList list = new ArrayList();
                ArrayList list2 = new ArrayList();
                foreach (ListViewItem item2 in this.lvwTSD.Items)
                {
                    IBizItem tag = item2.Tag as IBizItem;
                    list2.Add(tag.MasterOid);
                }
                if (e.Data.GetDataPresent(typeof(CLCopyData)))
                {
                    CLCopyData data = (CLCopyData) e.Data.GetData(typeof(CLCopyData));
                    foreach (object obj2 in data)
                    {
                        item = PlArchivManage.GetItem(obj2);
                        if ((item.Master.ClassName == "DQDOSSIERPRINT") && ((item != null) && !list2.Contains(item.MasterOid)))
                        {
                            list2.Add(item.MasterOid);
                            list.Add(item);
                        }
                    }
                }
                else
                {
                    item = PlArchivManage.GetItem(e.Data.GetData(typeof(IBizItem)));
                    if (item != null)
                    {
                        if (item.Master.ClassName != "DQDOSSIERPRINT")
                        {
                            return;
                        }
                        if (!list2.Contains(item.MasterOid))
                        {
                            list2.Add(item.MasterOid);
                            list.Add(item);
                        }
                    }
                }
                if (list.Count > 0)
                {
                    foreach (DEBusinessItem item4 in list)
                    {
                        PlArchivManage.SetLvwValues(this.hsCols, this.lvwTSD, this.lstOrder, item4);
                    }
                    this.lvwTSD.Refresh();
                }
            }
        }

        private void lvwTSD_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CLCopyData)))
            {
                CLCopyData data = (CLCopyData) e.Data.GetData(typeof(CLCopyData));
                if (((data != null) && (data.Count != 0)) && (((data[0] is DEBusinessItem) || (data[0] is DESmartBizItem)) || (data[0] is DERelationBizItem)))
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else if ((e.Data.GetDataPresent(typeof(DEBusinessItem)) || e.Data.GetDataPresent(typeof(DESmartBizItem))) || e.Data.GetDataPresent(typeof(DERelationBizItem)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void lvwTSD_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right) && (e.Clicks == 1))
            {
                ListViewItem itemAt = this.lvwTSD.GetItemAt(e.X, e.Y);
                this.cMenuTSD.Items.Clear();
                if (this.lstCando.Contains(this.D_ADD_TSD))
                {
                    this.cMenuTSD.Items.Add(this.D_ADD_TSD);
                }
                if (itemAt != null)
                {
                    if ((this.lvwTSD.SelectedItems.Count == 1) && this.lstCando.Contains(this.D_OPEN_TSD))
                    {
                        this.cMenuTSD.Items.Add(this.D_OPEN_TSD);
                    }
                    if (this.lstCando.Contains(this.D_DEL_TSD))
                    {
                        this.cMenuTSD.Items.Add(this.D_DEL_TSD);
                    }
                    if (this.lstCando.Contains(this.D_TOPRINT_TSD))
                    {
                        this.cMenuTSD.Items.Add(this.D_TOPRINT_TSD);
                        this.cMenuTSD.Items.Add(this.D_TOSENT_TSD);
                    }
                }
                if (this.cMenuTSD.Items.Count > 0)
                {
                    this.cMenuTSD.Show(this.lvwTSD, e.X, e.Y);
                }
            }
        }

        private void OpenItem(object sender, IBizItem it)
        {
            if (BizItemHandlerEvent.Instance.D_OpenItem != null)
            {
                PSEventArgs args = new PSEventArgs {
                    PSOption = ClientData.UserGlobalOption
                };
                PLMOperationArgs e = new PLMOperationArgs(FrmLogon.PLMProduct.ToString(), PLMLocation.ItemList.ToString(), it);
                BizItemHandlerEvent.Instance.D_OpenItem(sender, e);
            }
        }

        private void OpenTsdEdit(DEBusinessItem item)
        {
            TabPage page = null;
            string text = (item == null) ? "新建托晒单" : item.Id;
            if ((item == null) && (this.tbCtrlTS.TabPages[text] != null))
            {
                MessageBox.Show("目前有新建对象，还没有保存，必须关闭或保存现有新建对象，才能继续新建托晒单", "已有托晒单", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                if (this.tbCtrlTS.TabPages[text] != null)
                {
                    page = this.tbCtrlTS.TabPages[text];
                }
                if (page == null)
                {
                    UcTsdItem item2;
                    page = new TabPage(text) {
                        Name = text
                    };
                    if (item != null)
                    {
                        item2 = new UcTsdItem(item);
                    }
                    else
                    {
                        item2 = new UcTsdItem(this.resWkTsd.ResValue);
                    }
                    item2.Dock = DockStyle.Fill;
                    page.Controls.Add(item2);
                    this.tbCtrlTS.TabPages.Add(page);
                }
                this.tbCtrlTS.SelectTab(page);
            }
        }

        private void UcTs_Load(object sender, EventArgs e)
        {
            this.resWkTsd = new ResWkInfo();
            this.resWkTsd.Dock = DockStyle.Fill;
            this.panel1.Controls.Add(this.resWkTsd);
        }

        private void UpdateTsdItem(object obj)
        {
            DEBusinessItem item = obj as DEBusinessItem;
            if (item != null)
            {
                PlArchivManage.UpdateLvwValues(this.hsCols, this.lvwTSD, this.lstOrder, item);
            }
        }
    }
}

