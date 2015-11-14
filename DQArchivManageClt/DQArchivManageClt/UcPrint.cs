namespace DQArchivManageClt
{
    using DQArchivManageCommon;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;
    using Thyt.TiPLM.DEL.Product;
    using Thyt.TiPLM.PLL.Admin.NewResponsibility;
    using Thyt.TiPLM.PLL.Environment;
    using Thyt.TiPLM.PLL.FileService;
    using Thyt.TiPLM.PLL.Product2;
    using Thyt.TiPLM.UIL.Common;

    public class UcPrint : UserControl
    {
        private Button btnTsClear;
        private Button btnTsQiuckSch;
        private Button btnTsSchDocId;
        private Button btnUpdateBpm;
        private bool canOutput = false;
        private ContextMenuStrip cMenuPrint;
        private IContainer components = null;
        private PLMDelegate2 d_AfterDeleted;
        private PLMBizItemDelegate d_AfterIterationUpdated;
        private PLMBizItemDelegate d_AfterReleased;
        private PLMSimpleDelegate d_AfterTabClose = null;
        private PLMSimpleDelegate d_TsdEndPrint = null;
        private Hashtable hsCols = null;
        private Hashtable hsColWide;
        private Label label1;
        private Label lbTsId;
        private ArrayList lstOrder = null;
        private ArrayList lstSchPrint;
        private SortableListView lvwPrintLst;
        private ToolStripMenuItem mItemBack;
        private ToolStripMenuItem mItemCancelPrint;
        private ToolStripMenuItem mItemDownRrt;
        private ToolStripMenuItem mItemOpen;
        private ToolStripMenuItem mItemOutPut;
        private ToolStripMenuItem mItemRePrint;
        private ToolStripMenuItem mItemToSent;
        private Panel panel1;
        private Panel panel2;
        private ResWkInfo resWkTsd;
        private TabControl tbCtrlPrint;
        private TabPage tpPrintLst;
        private TextBox txtTsID;

        public UcPrint()
        {
            this.InitializeComponent();
            this.InitPrintlvw();
        }

        private void AfterPrintCloseTab(object obj)
        {
            string id;
            DEBusinessItem item = obj as DEBusinessItem;
            if (item == null)
            {
                id = obj.ToString();
            }
            else
            {
                id = item.Id;
            }
            TabPage page = this.tbCtrlPrint.TabPages[id];
            if (page != null)
            {
                this.tbCtrlPrint.TabPages.Remove(page);
            }
        }

        private void AfterTsdDel(object sender, PLMOperationArgs e)
        {
            if ((this.lvwPrintLst.Items.Count != 0) && (((e != null) && (e.BizItems != null)) && (e.BizItems.Length != 0)))
            {
                ArrayList list = new ArrayList(e.BizItems);
                for (int i = 0; i < this.lvwPrintLst.Items.Count; i++)
                {
                    ListViewItem item = this.lvwPrintLst.Items[i];
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
                        IBizItem item3 = PSStart.EqualMaster((IBizItem[]) list.ToArray(typeof(IBizItem)), dest);
                        if (item3 != null)
                        {
                            this.lvwPrintLst.Items.RemoveAt(i);
                            i--;
                            for (int j = 0; j < this.lstSchPrint.Count; j--)
                            {
                                DEBusinessItem item4 = this.lstSchPrint[j] as DEBusinessItem;
                                if (item4.MasterOid == item3.MasterOid)
                                {
                                    this.lstSchPrint.RemoveAt(j);
                                }
                            }
                        }
                        this.AfterPrintCloseTab(dest.Id);
                    }
                }
            }
        }

        private void AfterTsdUpdate(IBizItem[] bizItems)
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
                    switch (PlArchivManage.GetTsStatue(item))
                    {
                        case "打印完成":
                        case "打印取消":
                        case "开始打印":
                            this.UpdateTsdItem(item);
                            break;
                    }
                }
            }
        }

        private void btnTsClear_Click(object sender, EventArgs e)
        {
            this.lstSchPrint.Clear();
            this.lvwPrintLst.Items.Clear();
        }

        private void btnTsQiuckSch_Click(object sender, EventArgs e)
        {
            FrmTsdQuickSch sch = new FrmTsdQuickSch(true);
            if (sch.ShowDialog() == DialogResult.OK)
            {
                DataSet ds = sch.ds;
                this.lvwPrintLst.Items.Clear();
                if (((ds != null) && (ds.Tables.Count > 0)) && (ds.Tables[0].Rows.Count > 0))
                {
                    PlArchivManage.SetLvwClsValues(this.hsCols, this.lvwPrintLst, this.lstOrder, ds.Tables[0], "DQDOSSIERPRINT");
                }
                this.lvwPrintLst.Refresh();
            }
        }

        private void btnTsSchDocId_Click(object sender, EventArgs e)
        {
            this.lvwPrintLst.Items.Clear();
            DataSet set = PlArchivManage.Agent.GetTSDForPrint(this.txtTsID.Text, this.resWkTsd.ResValue, ClientData.LogonUser.Oid);
            if (((set != null) && (set.Tables.Count > 0)) && (set.Tables[0].Rows.Count > 0))
            {
                PlArchivManage.SetLvwClsValues(this.hsCols, this.lvwPrintLst, this.lstOrder, set.Tables[0], "DQDOSSIERPRINT");
            }
            this.lvwPrintLst.Refresh();
        }

        private void btnUpdateBpm_Click(object sender, EventArgs e)
        {
            PlArchivManage.SetBpmInfo(this.resWkTsd, true);
        }

        protected override void Dispose(bool disposing)
        {
            DelegatesOfAm.Instance.D_AfterPrintTabClose = (PLMSimpleDelegate) Delegate.Remove(DelegatesOfAm.Instance.D_AfterPrintTabClose, this.d_AfterTabClose);
            BizItemHandlerEvent.Instance.D_AfterIterationUpdated = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterIterationUpdated, this.d_AfterIterationUpdated);
            BizItemHandlerEvent.Instance.D_AfterReleased = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterReleased, this.d_AfterReleased);
            BizItemHandlerEvent.Instance.D_AfterDeleted = (PLMDelegate2) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterDeleted, this.d_AfterDeleted);
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tbCtrlPrint = new System.Windows.Forms.TabControl();
            this.tpPrintLst = new System.Windows.Forms.TabPage();
            this.lvwPrintLst = new Thyt.TiPLM.UIL.Common.SortableListView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnUpdateBpm = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnTsSchDocId = new System.Windows.Forms.Button();
            this.btnTsClear = new System.Windows.Forms.Button();
            this.btnTsQiuckSch = new System.Windows.Forms.Button();
            this.lbTsId = new System.Windows.Forms.Label();
            this.txtTsID = new System.Windows.Forms.TextBox();
            this.cMenuPrint = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mItemOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.mItemCancelPrint = new System.Windows.Forms.ToolStripMenuItem();
            this.mItemToSent = new System.Windows.Forms.ToolStripMenuItem();
            this.mItemRePrint = new System.Windows.Forms.ToolStripMenuItem();
            this.mItemOutPut = new System.Windows.Forms.ToolStripMenuItem();
            this.mItemBack = new System.Windows.Forms.ToolStripMenuItem();
            this.mItemDownRrt = new System.Windows.Forms.ToolStripMenuItem();
            this.tbCtrlPrint.SuspendLayout();
            this.tpPrintLst.SuspendLayout();
            this.panel1.SuspendLayout();
            this.cMenuPrint.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbCtrlPrint
            // 
            this.tbCtrlPrint.Controls.Add(this.tpPrintLst);
            this.tbCtrlPrint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbCtrlPrint.Location = new System.Drawing.Point(0, 0);
            this.tbCtrlPrint.Name = "tbCtrlPrint";
            this.tbCtrlPrint.SelectedIndex = 0;
            this.tbCtrlPrint.Size = new System.Drawing.Size(770, 422);
            this.tbCtrlPrint.TabIndex = 0;
            // 
            // tpPrintLst
            // 
            this.tpPrintLst.Controls.Add(this.lvwPrintLst);
            this.tpPrintLst.Controls.Add(this.panel1);
            this.tpPrintLst.Location = new System.Drawing.Point(4, 22);
            this.tpPrintLst.Name = "tpPrintLst";
            this.tpPrintLst.Padding = new System.Windows.Forms.Padding(3);
            this.tpPrintLst.Size = new System.Drawing.Size(762, 396);
            this.tpPrintLst.TabIndex = 0;
            this.tpPrintLst.Text = "打印列表";
            this.tpPrintLst.UseVisualStyleBackColor = true;
            // 
            // lvwPrintLst
            // 
            this.lvwPrintLst.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvwPrintLst.FullRowSelect = true;
            this.lvwPrintLst.HideSelection = false;
            this.lvwPrintLst.Location = new System.Drawing.Point(3, 51);
            this.lvwPrintLst.Name = "lvwPrintLst";
            this.lvwPrintLst.Size = new System.Drawing.Size(756, 342);
            this.lvwPrintLst.SortingOrder = System.Windows.Forms.SortOrder.None;
            this.lvwPrintLst.TabIndex = 1;
            this.lvwPrintLst.UseCompatibleStateImageBehavior = false;
            this.lvwPrintLst.View = System.Windows.Forms.View.Details;
            this.lvwPrintLst.DoubleClick += new System.EventHandler(this.lvwPrintLst_DoubleClick);
            this.lvwPrintLst.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lvwPrintLst_MouseUp);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel1.Controls.Add(this.btnUpdateBpm);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnTsSchDocId);
            this.panel1.Controls.Add(this.btnTsClear);
            this.panel1.Controls.Add(this.btnTsQiuckSch);
            this.panel1.Controls.Add(this.lbTsId);
            this.panel1.Controls.Add(this.txtTsID);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(756, 48);
            this.panel1.TabIndex = 0;
            // 
            // btnUpdateBpm
            // 
            this.btnUpdateBpm.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnUpdateBpm.Location = new System.Drawing.Point(466, 10);
            this.btnUpdateBpm.Name = "btnUpdateBpm";
            this.btnUpdateBpm.Size = new System.Drawing.Size(63, 23);
            this.btnUpdateBpm.TabIndex = 42;
            this.btnUpdateBpm.Text = "更新流程";
            this.btnUpdateBpm.UseVisualStyleBackColor = true;
            this.btnUpdateBpm.Click += new System.EventHandler(this.btnUpdateBpm_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Location = new System.Drawing.Point(275, 8);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(176, 32);
            this.panel2.TabIndex = 41;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(217, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 40;
            this.label1.Text = "流程名称";
            // 
            // btnTsSchDocId
            // 
            this.btnTsSchDocId.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnTsSchDocId.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnTsSchDocId.Location = new System.Drawing.Point(544, 11);
            this.btnTsSchDocId.Name = "btnTsSchDocId";
            this.btnTsSchDocId.Size = new System.Drawing.Size(35, 23);
            this.btnTsSchDocId.TabIndex = 38;
            this.btnTsSchDocId.Text = "...";
            this.btnTsSchDocId.UseVisualStyleBackColor = true;
            this.btnTsSchDocId.Click += new System.EventHandler(this.btnTsSchDocId_Click);
            // 
            // btnTsClear
            // 
            this.btnTsClear.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnTsClear.Location = new System.Drawing.Point(669, 10);
            this.btnTsClear.Name = "btnTsClear";
            this.btnTsClear.Size = new System.Drawing.Size(75, 23);
            this.btnTsClear.TabIndex = 37;
            this.btnTsClear.Text = "清空";
            this.btnTsClear.UseVisualStyleBackColor = true;
            this.btnTsClear.Click += new System.EventHandler(this.btnTsClear_Click);
            // 
            // btnTsQiuckSch
            // 
            this.btnTsQiuckSch.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnTsQiuckSch.Location = new System.Drawing.Point(585, 12);
            this.btnTsQiuckSch.Name = "btnTsQiuckSch";
            this.btnTsQiuckSch.Size = new System.Drawing.Size(78, 23);
            this.btnTsQiuckSch.TabIndex = 35;
            this.btnTsQiuckSch.Text = "快速查询";
            this.btnTsQiuckSch.UseVisualStyleBackColor = true;
            this.btnTsQiuckSch.Click += new System.EventHandler(this.btnTsQiuckSch_Click);
            // 
            // lbTsId
            // 
            this.lbTsId.AutoSize = true;
            this.lbTsId.Location = new System.Drawing.Point(15, 16);
            this.lbTsId.Name = "lbTsId";
            this.lbTsId.Size = new System.Drawing.Size(29, 12);
            this.lbTsId.TabIndex = 33;
            this.lbTsId.Text = "图号";
            // 
            // txtTsID
            // 
            this.txtTsID.Location = new System.Drawing.Point(50, 13);
            this.txtTsID.Name = "txtTsID";
            this.txtTsID.Size = new System.Drawing.Size(161, 21);
            this.txtTsID.TabIndex = 32;
            // 
            // cMenuPrint
            // 
            this.cMenuPrint.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mItemOpen,
            this.mItemCancelPrint,
            this.mItemToSent,
            this.mItemRePrint,
            this.mItemOutPut,
            this.mItemBack,
            this.mItemDownRrt});
            this.cMenuPrint.Name = "cMenuPrint";
            this.cMenuPrint.Size = new System.Drawing.Size(153, 180);
            // 
            // mItemOpen
            // 
            this.mItemOpen.Name = "mItemOpen";
            this.mItemOpen.Size = new System.Drawing.Size(148, 22);
            this.mItemOpen.Text = "打开托晒单";
            this.mItemOpen.Click += new System.EventHandler(this.mItemOpen_Click);
            // 
            // mItemCancelPrint
            // 
            this.mItemCancelPrint.Enabled = false;
            this.mItemCancelPrint.Name = "mItemCancelPrint";
            this.mItemCancelPrint.Size = new System.Drawing.Size(152, 22);
            this.mItemCancelPrint.Text = "取 消 打 印";
            this.mItemCancelPrint.Visible = false;
            this.mItemCancelPrint.Click += new System.EventHandler(this.mItemCancelPrint_Click);
            // 
            // mItemToSent
            // 
            this.mItemToSent.Name = "mItemToSent";
            this.mItemToSent.Size = new System.Drawing.Size(148, 22);
            this.mItemToSent.Text = "打 印 完 成";
            this.mItemToSent.Click += new System.EventHandler(this.mItemToSent_Click);
            // 
            // mItemRePrint
            // 
            this.mItemRePrint.Enabled = false;
            this.mItemRePrint.Name = "mItemRePrint";
            this.mItemRePrint.Size = new System.Drawing.Size(152, 22);
            this.mItemRePrint.Text = "重 新 打 印";
            this.mItemRePrint.Visible = false;
            this.mItemRePrint.Click += new System.EventHandler(this.mItemRePrint_Click);
            // 
            // mItemOutPut
            // 
            this.mItemOutPut.Name = "mItemOutPut";
            this.mItemOutPut.Size = new System.Drawing.Size(148, 22);
            this.mItemOutPut.Text = "输出托晒单";
            this.mItemOutPut.Click += new System.EventHandler(this.mItemOutPut_Click);
            // 
            // mItemBack
            // 
            this.mItemBack.Enabled = false;
            this.mItemBack.Name = "mItemBack";
            this.mItemBack.Size = new System.Drawing.Size(152, 22);
            this.mItemBack.Text = "打 印 回 退";
            this.mItemBack.Visible = false;
            this.mItemBack.Click += new System.EventHandler(this.mItemBack_Click);
            // 
            // mItemDownRrt
            // 
            this.mItemDownRrt.Name = "mItemDownRrt";
            this.mItemDownRrt.Size = new System.Drawing.Size(148, 22);
            this.mItemDownRrt.Text = "下载打印文件";
            this.mItemDownRrt.Click += new System.EventHandler(this.MItemDownRrt_Click);
            // 
            // UcPrint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbCtrlPrint);
            this.Name = "UcPrint";
            this.Size = new System.Drawing.Size(770, 422);
            this.Load += new System.EventHandler(this.UcPrint_Load);
            this.tbCtrlPrint.ResumeLayout(false);
            this.tpPrintLst.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.cMenuPrint.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void InitPrintlvw()
        {
            this.hsCols = PlArchivManage.Agent.GetViewOfCol("打印", out this.lstOrder, out this.hsColWide);
            PlArchivManage.SetCol(this.hsCols, this.lvwPrintLst, "Print", this.lstOrder, this.hsColWide);
            this.lstSchPrint = new ArrayList();
            this.d_AfterTabClose = new PLMSimpleDelegate(this.AfterPrintCloseTab);
            this.d_AfterIterationUpdated = new PLMBizItemDelegate(this.AfterTsdUpdate);
            this.d_AfterReleased = new PLMBizItemDelegate(this.AfterTsdUpdate);
            this.d_AfterDeleted = new PLMDelegate2(this.AfterTsdDel);
            DelegatesOfAm.Instance.D_AfterPrintTabClose = (PLMSimpleDelegate) Delegate.Combine(DelegatesOfAm.Instance.D_AfterPrintTabClose, this.d_AfterTabClose);
            BizItemHandlerEvent.Instance.D_AfterIterationUpdated = (PLMBizItemDelegate) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterIterationUpdated, this.d_AfterIterationUpdated);
            BizItemHandlerEvent.Instance.D_AfterReleased = (PLMBizItemDelegate) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterReleased, this.d_AfterReleased);
            BizItemHandlerEvent.Instance.D_AfterDeleted = (PLMDelegate2) Delegate.Combine(BizItemHandlerEvent.Instance.D_AfterDeleted, this.d_AfterDeleted);
            if (PLGrantPerm.CanDoClassOperation(ClientData.LogonUser.Oid, "DQDOSSIERPRINT", Guid.Empty, "ClaRel_DOWNLOAD") == 1)
            {
                this.canOutput = true;
            }
        }

        private void lvwPrintLst_DoubleClick(object sender, EventArgs e)
        {
            if ((PLGrantPerm.CanDoClassOperation(ClientData.LogonUser.Oid, "DQDOSSIERPRINT", Guid.Empty, "ClaRel_BROWSE") == 1) && (this.lvwPrintLst.SelectedItems.Count == 1))
            {
                ListViewItem item = this.lvwPrintLst.SelectedItems[0];
                IBizItem tag = item.Tag as IBizItem;
                if (BizItemHandlerEvent.Instance.D_OpenItem != null)
                {
                    PSEventArgs args = new PSEventArgs {
                        PSOption = ClientData.UserGlobalOption
                    };
                    PLMOperationArgs args2 = new PLMOperationArgs(FrmLogon.PLMProduct.ToString(), PLMLocation.ItemList.ToString(), tag);
                    BizItemHandlerEvent.Instance.D_OpenItem(sender, args2);
                }
            }
        }

        private void lvwPrintLst_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right) && (e.Clicks == 1))
            {
                ListViewItem itemAt = this.lvwPrintLst.GetItemAt(e.X, e.Y);
                this.cMenuPrint.Items.Clear();
                if (this.lvwPrintLst.SelectedItems.Count == 1)
                {
                    this.cMenuPrint.Items.Add(this.mItemOpen);
                }
                if ((this.lvwPrintLst.SelectedItems.Count > 0) && this.canOutput)
                {
                    this.cMenuPrint.Items.Add(this.mItemOutPut);
                    this.cMenuPrint.Items.Add(this.mItemDownRrt);
                }
                bool flag = false;
                bool flag2 = false;
                bool flag3 = true;
                foreach (ListViewItem item2 in this.lvwPrintLst.SelectedItems)
                {
                    DEBusinessItem tag = item2.Tag as DEBusinessItem;
                    if (tag.State != ItemState.Abandoned)
                    {
                        if (tag.State != ItemState.Release)
                        {
                            flag3 = false;
                        }
                        object attrValue = tag.Iteration.GetAttrValue("TSSTATUS");
                        string str = (attrValue == null) ? "" : attrValue.ToString();
                        if (str != null)
                        {
                            if (!(str == "开始打印"))
                            {
                                if (str == "打印取消")
                                {
                                    goto Label_0187;
                                }
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                    }
                    continue;
                Label_0187:
                    flag2 = true;
                }
                if (flag)
                {
                    this.cMenuPrint.Items.Add(this.mItemToSent);
                    this.cMenuPrint.Items.Add(this.mItemCancelPrint);
                }
                if (flag2)
                {
                    this.cMenuPrint.Items.Add(this.mItemRePrint);
                }
                if (!(flag3 || (!flag && !flag2)))
                {
                    this.cMenuPrint.Items.Add(this.mItemBack);
                }
                if (this.cMenuPrint.Items.Count > 0)
                {
                    this.cMenuPrint.Show(this.lvwPrintLst, e.Location);
                }
            }
        }

        private void mItemBack_Click(object sender, EventArgs e)
        {
            if (this.lvwPrintLst.SelectedItems.Count != 0)
            {
                ArrayList list = new ArrayList();
                foreach (ListViewItem item in this.lvwPrintLst.SelectedItems)
                {
                    DEBusinessItem tag = item.Tag as DEBusinessItem;
                    if (PlArchivManage.GetTsStatue(tag) != "打印完成")
                    {
                        list.Add(tag);
                    }
                }
                if (list.Count == 0)
                {
                    MessageBox.Show("没有可以打印回退的托晒单！");
                }
                else
                {
                    int num;
                    DEBusinessItem item3;
                    string strMarkup;
                    StringBuilder builder = new StringBuilder();
                    StringBuilder builder2 = new StringBuilder();
                    bool isSameReason = false;
                    Hashtable hashtable = new Hashtable();
                    for (num = 0; num < list.Count; num++)
                    {
                        int num2;
                        DEBusinessItem item4;
                        item3 = list[num] as DEBusinessItem;
                        bool isJustOne = (list.Count - num) == 1;
                        FrmInputRemark remark = new FrmInputRemark("托晒单:" + item3.Id + "打印回退", false, isJustOne);
                        DialogResult result = remark.ShowDialog();
                        isSameReason = remark.IsSameReason;
                        strMarkup = remark.StrMarkup;
                        if (result != DialogResult.OK)
                        {
                            if (isSameReason)
                            {
                                num2 = num;
                                while (num2 < list.Count)
                                {
                                    item4 = list[num2] as DEBusinessItem;
                                    builder2.Append(item4.Id);
                                    builder2.Append(";");
                                    list.RemoveAt(num2);
                                    num2--;
                                    num2++;
                                }
                            }
                            else
                            {
                                builder2.Append(item3.Id);
                                builder2.Append(";");
                                list.RemoveAt(num);
                                num--;
                            }
                        }
                        else
                        {
                            if (isSameReason)
                            {
                                for (num2 = num; num2 < list.Count; num2++)
                                {
                                    item4 = list[num2] as DEBusinessItem;
                                    hashtable[item4] = strMarkup;
                                    num++;
                                }
                                break;
                            }
                            hashtable[item3] = strMarkup;
                        }
                    }
                    if (list.Count > 0)
                    {
                        try
                        {
                            for (num = 0; num < list.Count; num++)
                            {
                                item3 = list[num] as DEBusinessItem;
                                strMarkup = (hashtable[item3] == null) ? "" : hashtable[item3].ToString();
                                PlArchivManage.BackPrint(item3, strMarkup);
                                item3.Iteration = PLItem.UpdateItemIterationDirectly(item3, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption, false);
                                builder.Append(item3.Id);
                                builder.Append(";");
                                this.ReNewOpen(item3);
                            }
                            if (builder.Length > 0)
                            {
                                builder = builder.Remove(builder.Length - 1, 1);
                                builder.Insert(0, "\r\n 打印回退");
                                FrmArchivManage.frmMian.DisplayTextInRichtBox(builder.ToString(), 1, true);
                            }
                        }
                        finally
                        {
                            BizItemHandlerEvent.Instance.D_AfterIterationUpdated((IBizItem[]) list.ToArray(typeof(IBizItem)));
                        }
                    }
                    else
                    {
                        MessageBox.Show("此次操作被取消");
                    }
                }
            }
        }

        private void mItemCancelPrint_Click(object sender, EventArgs e)
        {
            if (this.lvwPrintLst.SelectedItems.Count != 0)
            {
                ArrayList list = new ArrayList();
                foreach (ListViewItem item in this.lvwPrintLst.SelectedItems)
                {
                    DEBusinessItem tag = item.Tag as DEBusinessItem;
                    string tsStatue = PlArchivManage.GetTsStatue(tag);
                    if ((tsStatue != "打印完成") && (tsStatue != "打印取消"))
                    {
                        list.Add(tag);
                    }
                }
                if (list.Count == 0)
                {
                    MessageBox.Show("没有可以取消打印的托晒单！");
                }
                else
                {
                    int num;
                    DEBusinessItem item3;
                    string strMarkup;
                    StringBuilder builder = new StringBuilder();
                    StringBuilder builder2 = new StringBuilder();
                    bool isSameReason = false;
                    Hashtable hashtable = new Hashtable();
                    for (num = 0; num < list.Count; num++)
                    {
                        int num2;
                        DEBusinessItem item4;
                        item3 = list[num] as DEBusinessItem;
                        bool isJustOne = (list.Count - num) == 1;
                        FrmInputRemark remark = new FrmInputRemark("托晒单:" + item3.Id + "取消打印", false, isJustOne);
                        DialogResult result = remark.ShowDialog();
                        isSameReason = remark.IsSameReason;
                        strMarkup = remark.StrMarkup;
                        if (result != DialogResult.OK)
                        {
                            if (isSameReason)
                            {
                                num2 = num;
                                while (num2 < list.Count)
                                {
                                    item4 = list[num2] as DEBusinessItem;
                                    builder2.Append(item4.Id);
                                    builder2.Append(";");
                                    list.RemoveAt(num2);
                                    num2--;
                                    num2++;
                                }
                            }
                            else
                            {
                                builder2.Append(item3.Id);
                                builder2.Append(";");
                                list.RemoveAt(num);
                                num--;
                            }
                        }
                        else
                        {
                            if (isSameReason)
                            {
                                for (num2 = num; num2 < list.Count; num2++)
                                {
                                    item4 = list[num2] as DEBusinessItem;
                                    hashtable[item4] = strMarkup;
                                    num++;
                                }
                                break;
                            }
                            hashtable[item3] = strMarkup;
                        }
                    }
                    if (list.Count > 0)
                    {
                        try
                        {
                            for (num = 0; num < list.Count; num++)
                            {
                                item3 = list[num] as DEBusinessItem;
                                strMarkup = (hashtable[item3] == null) ? "" : hashtable[item3].ToString();
                                PlArchivManage.CancelPrint(item3, strMarkup);
                                item3.Iteration = PLItem.UpdateItemIterationDirectly(item3, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption, false);
                                builder.Append(item3.Id);
                                builder.Append(";");
                                this.ReNewOpen(item3);
                            }
                            if (builder.Length > 0)
                            {
                                builder = builder.Remove(builder.Length - 1, 1);
                                builder.Insert(0, "\r\n 打印取消");
                                FrmArchivManage.frmMian.DisplayTextInRichtBox(builder.ToString(), 1, true);
                            }
                        }
                        finally
                        {
                            BizItemHandlerEvent.Instance.D_AfterIterationUpdated((IBizItem[]) list.ToArray(typeof(IBizItem)));
                        }
                    }
                    else
                    {
                        MessageBox.Show("此次操作被取消");
                    }
                }
            }
        }

        private void MItemDownRrt_Click(object sender, EventArgs e)
        {
            if (this.lvwPrintLst.SelectedItems.Count != 0)
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog {
                    Description = "下载打印文件"
                };
                if (DialogResult.OK == dialog.ShowDialog())
                {
                    string selectedPath = dialog.SelectedPath;
                    bool flag = false;
                    foreach (ListViewItem item in this.lvwPrintLst.SelectedItems)
                    {
                        DEBusinessItem tag = item.Tag as DEBusinessItem;
                        if (tag != null)
                        {
                            if (tag.FileCount == 0)
                            {
                                tag.Iteration.FileList.AddRange(PLItem.Agent.GetSecureFiles(tag.Iteration.Oid, ClientData.LogonUser.Oid));
                            }
                            if (tag.FileCount == 0)
                            {
                                FrmArchivManage.frmMian.DisplayTextInRichtBox("托晒单【" + tag.Id + "】没有打印文件", 2, true);
                            }
                            else
                            {
                                foreach (DESecureFile file in tag.FileList)
                                {
                                    string str2 = file.FileName.ToLower();
                                    if ((str2.EndsWith(".rar") || str2.EndsWith(".zip")) || str2.EndsWith(".prt"))
                                    {
                                        try
                                        {
                                            string str3 = FSClientUtil.DownloadFile("ClaRel_DOWNLOAD", file.FileOid, selectedPath);
                                            FrmArchivManage.frmMian.DisplayTextInRichtBox("下载托晒单【" + tag.Id + "】文件" + file.FileName + "打印文件成功", 1, true);
                                        }
                                        catch (Exception exception)
                                        {
                                            FrmArchivManage.frmMian.DisplayTextInRichtBox("下载托晒单【" + tag.Id + "】文件" + file.FileName + "打印文件失败" + exception.Message, 2, true);
                                            continue;
                                        }
                                        flag = true;
                                    }
                                }
                            }
                        }
                    }
                    if (!flag)
                    {
                        MessageBox.Show("没有需要下载的打印文件");
                    }
                    else
                    {
                        MessageBox.Show("托晒打印文件加载完毕");
                    }
                }
            }
        }

        private void mItemOpen_Click(object sender, EventArgs e)
        {
            if (this.lvwPrintLst.SelectedItems.Count == 1)
            {
                ListViewItem item = this.lvwPrintLst.SelectedItems[0];
                DEBusinessItem tag = item.Tag as DEBusinessItem;
                this.OpenTsdEdit(tag);
            }
        }

        private void mItemOutPut_Click(object sender, EventArgs e)
        {
            TsdOutPut put = new TsdOutPut();
            foreach (ListViewItem item in this.lvwPrintLst.SelectedItems)
            {
                DEBusinessItem tag = item.Tag as DEBusinessItem;
                string wk = (tag.Iteration.GetAttrValue("WKFLINFO") == null) ? "" : tag.Iteration.GetAttrValue("WKFLINFO").ToString();
                put.StartOutPut(tag.IterOid, tag, wk);
            }
        }

        private void mItemRePrint_Click(object sender, EventArgs e)
        {
            if (this.lvwPrintLst.SelectedItems.Count != 0)
            {
                ArrayList list = new ArrayList();
                foreach (ListViewItem item in this.lvwPrintLst.SelectedItems)
                {
                    DEBusinessItem tag = item.Tag as DEBusinessItem;
                    if (PlArchivManage.GetTsStatue(tag) == "打印取消")
                    {
                        list.Add(tag);
                    }
                }
                if (list.Count == 0)
                {
                    MessageBox.Show("没有可以重新打印的托晒单！");
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        DEBusinessItem item3 = list[i] as DEBusinessItem;
                        PlArchivManage.RePrint(item3);
                        item3.Iteration = PLItem.UpdateItemIterationDirectly(item3, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption, false);
                    }
                    if (list.Count > 0)
                    {
                        BizItemHandlerEvent.Instance.D_AfterIterationUpdated((IBizItem[]) list.ToArray(typeof(IBizItem)));
                        foreach (DEBusinessItem item3 in list)
                        {
                            this.ReNewOpen(item3);
                        }
                    }
                }
            }
        }

        private void mItemToSent_Click(object sender, EventArgs e)
        {
            if (this.lvwPrintLst.SelectedItems.Count != 0)
            {
                ArrayList list = new ArrayList();
                StringBuilder builder = new StringBuilder();
                foreach (ListViewItem item in this.lvwPrintLst.SelectedItems)
                {
                    DEBusinessItem tag = item.Tag as DEBusinessItem;
                    if (PlArchivManage.GetTsStatue(tag) == "开始打印")
                    {
                        list.Add(tag);
                    }
                }
                if (list.Count == 0)
                {
                    MessageBox.Show("托晒单状态不符合要求，必须处于“开始打印”阶段的托晒单才能完成打印，发往收发部门！");
                }
                else
                {
                    DialogResult result = MessageBox.Show("是： 对于打印状态为[已取消]的托晒明细，也将设置为[已完成]\r\n否：已取消打印的托晒明细不做处理。", "完成打印方式选择", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    int num = (result == DialogResult.Yes) ? 1 : ((result == DialogResult.No) ? 2 : 0);
                    if (num == 0)
                    {
                        MessageBox.Show("取消完成打印操作");
                    }
                    else
                    {
                        int num2;
                        DEBusinessItem current;
                        StringBuilder builder2 = new StringBuilder();
                        StringBuilder builder3 = new StringBuilder();
                        Hashtable hashtable = new Hashtable();
                        bool isPowerPrintBomAll = num == 1;
                        ArrayList list2 = new ArrayList();
                        for (num2 = 0; num2 < list.Count; num2++)
                        {
                            int num3;
                            int num4;
                            int num5;
                            current = list[num2] as DEBusinessItem;
                            PlArchivManage.CheckPrintItem(current, out num3, out num4, out num5);
                            if (!(isPowerPrintBomAll || (((num3 != 0) || (num4 != 0)) || (num5 == 0))))
                            {
                                list2.Add(current);
                                list.Remove(current);
                                num2--;
                            }
                            else
                            {
                                int num6;
                                DEBusinessItem item4;
                                bool isJustOne = (list.Count - num2) == 1;
                                FrmInputRemark remark = new FrmInputRemark("托晒单:" + current.Id + "完成打印", true, isJustOne);
                                result = remark.ShowDialog();
                                bool isSameReason = remark.IsSameReason;
                                string strMarkup = remark.StrMarkup;
                                if (result != DialogResult.OK)
                                {
                                    if (isSameReason)
                                    {
                                        num6 = num2;
                                        while (num6 < list.Count)
                                        {
                                            item4 = list[num6] as DEBusinessItem;
                                            builder3.Append(item4.Id);
                                            builder3.Append(";");
                                            list.RemoveAt(num6);
                                            num6--;
                                            num6++;
                                        }
                                    }
                                    else
                                    {
                                        builder3.Append(current.Id);
                                        builder3.Append(";");
                                        list.RemoveAt(num2);
                                        num2--;
                                    }
                                }
                                else
                                {
                                    if (isSameReason)
                                    {
                                        for (num6 = num2; num6 < list.Count; num6++)
                                        {
                                            item4 = list[num6] as DEBusinessItem;
                                            hashtable[item4] = strMarkup;
                                            num2++;
                                        }
                                        break;
                                    }
                                    hashtable[current] = strMarkup;
                                }
                            }
                        }
                        if (list2.Count > 0)
                        {
                            string str2 = "";
                            IEnumerator enumerator = list2.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                current = (DEBusinessItem) enumerator.Current;
                                str2 = str2 + current.Id + " ";
                            }
                            FrmArchivManage.frmMian.DisplayTextInRichtBox("下列托晒单中所有明细均以被取消打印，不能完成打印：\r\n" + str2, 2, true);
                        }
                        if (list.Count > 0)
                        {
                            StringBuilder builder4 = new StringBuilder();
                            ArrayList list3 = new ArrayList();
                            for (num2 = 0; num2 < list.Count; num2++)
                            {
                                StringBuilder builder5;
                                current = list[num2] as DEBusinessItem;
                                string sm = (hashtable[current] == null) ? "" : hashtable[current].ToString();
                                PlArchivManage.EndPrint(current, sm, isPowerPrintBomAll);
                                current.Iteration = PLItem.UpdateItemIterationDirectly(current, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption, false);
                                PlArchivManage.Agent.PrintOrSentTsd(ClientData.LogonUser.Oid, current, "EndPrint", out builder5);
                                if (builder5.Length > 0)
                                {
                                    builder4.Append(builder5.ToString());
                                }
                                else
                                {
                                    list3.Add(current);
                                }
                            }
                            if (list3.Count > 0)
                            {
                                ArrayList list4 = new ArrayList();
                                ArrayList list5 = new ArrayList();
                                ArrayList list6 = new ArrayList();
                                for (num2 = 0; num2 < list3.Count; num2++)
                                {
                                    IBizItem item5 = list3[num2] as IBizItem;
                                    list4.Add(item5.MasterOid);
                                    list5.Add(0);
                                    list6.Add(0);
                                }
                                list3 = PLItem.Agent.GetBizItems((Guid[]) list4.ToArray(typeof(Guid)), (int[]) list5.ToArray(typeof(int)), (int[]) list6.ToArray(typeof(int)), ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid, BizItemMode.BizItem);
                                BizItemHandlerEvent.Instance.D_AfterIterationUpdated((IBizItem[]) list3.ToArray(typeof(IBizItem)));
                                foreach (DEBusinessItem item6 in list3)
                                {
                                    builder2.Append(item6.Id);
                                    builder2.Append(";");
                                    this.ReNewOpen(item6);
                                    object systemParameter = new PLSystemParam().GetSystemParameter(ConstAm.ISUSEAUTOCOMMIT);
                                    if (systemParameter != null)
                                    {
                                        bool flag4 = systemParameter.ToString() == "Y";
                                        try
                                        {
                                            if (flag4)
                                            {
                                                PlArchivManage.CommitWorkItem(item6);
                                            }
                                        }
                                        catch (Exception exception)
                                        {
                                            builder.Append("\t" + item6.Id + ":" + exception.Message + "\r\n\t    " + exception.ToString());
                                        }
                                    }
                                }
                            }
                        }
                        if (builder2.Length > 0)
                        {
                            builder2 = builder2.Remove(builder2.Length - 1, 1);
                            builder2.Insert(0, "\r\n 完成打印");
                            FrmArchivManage.frmMian.DisplayTextInRichtBox(builder2.ToString(), 1, true);
                        }
                        if (builder3.Length > 0)
                        {
                            builder3 = builder3.Remove(builder2.Length - 1, 1);
                            builder3.Insert(0, "\r\n 被取消打印：");
                            FrmArchivManage.frmMian.DisplayTextInRichtBox(builder3.ToString(), 2, true);
                        }
                        if (builder.Length > 0)
                        {
                            FrmArchivManage.frmMian.DisplayTextInRichtBox("下列托晒单已完成打印，但提交流程失败\r\n" + builder.ToString(), 2, true);
                        }
                    }
                }
            }
        }

        private void OpenTsdEdit(DEBusinessItem item)
        {
            TabPage page = null;
            string id = item.Id;
            if (this.tbCtrlPrint.TabPages[id] != null)
            {
                page = this.tbCtrlPrint.TabPages[id];
            }
            if (page == null)
            {
                page = new TabPage(id) {
                    Name = id
                };
                UcPrintItem item2 = new UcPrintItem(item) {
                    Dock = DockStyle.Fill
                };
                page.Controls.Add(item2);
                this.tbCtrlPrint.TabPages.Add(page);
            }
            this.tbCtrlPrint.SelectTab(page);
        }

        private void ReNewOpen(DEBusinessItem item)
        {
            TabPage page = this.tbCtrlPrint.TabPages[item.Id];
            if (page != null)
            {
                this.AfterPrintCloseTab(item);
                this.OpenTsdEdit(item);
            }
        }

        private void UcPrint_Load(object sender, EventArgs e)
        {
            this.resWkTsd = new ResWkInfo();
            this.resWkTsd.Dock = DockStyle.Fill;
            this.panel2.Controls.Add(this.resWkTsd);
        }

        private void UpdateTsdItem(object obj)
        {
            DEBusinessItem item = obj as DEBusinessItem;
            if (((item != null) && (item.State == ItemState.Release)) && (item.ClassName == "DQDOSSIERPRINT"))
            {
                PlArchivManage.UpdateLvwValues(this.hsCols, this.lvwPrintLst, this.lstOrder, item);
            }
        }
    }
}

