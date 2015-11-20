namespace DQArchivManageClt
{
    using DQArchivManageCommon;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;
    using Thyt.TiPLM.DEL.Admin.DataModel;
    using Thyt.TiPLM.DEL.Product;
    using Thyt.TiPLM.PLL.Admin.DataModel;
    using Thyt.TiPLM.PLL.Admin.NewResponsibility;
    using Thyt.TiPLM.PLL.Product2;
    using Thyt.TiPLM.UIL.Common;

    public class UcSent : UserControl
    {
        private Button btnSch;
        private Button btnSignOK;
        private Button btnTsClear;
        private CheckBox chkEndSent;
        private ContextMenuStrip cMenuSent;
        private IContainer components = null;
        private PLMBizItemDelegate d_AfterAbandon;
        private PLMBizItemDelegate d_AfterCheckIn;
        private PLMBizItemDelegate d_AfterCheckOut;
        private PLMDelegate2 d_AfterDeleted;
        private PLMBizItemDelegate d_AfterIterationUpdated;
        private PLMBizItemDelegate d_AfterMasterUpdated;
        private PLMBizItemDelegate d_AfterReleased;
        private PLMBizItemDelegate d_AfterRevisionCreated;
        private PLMSimpleDelegate d_AfterTabClose;
        private PLMBizItemDelegate d_AfterUndoAbandon;
        private PLMBizItemDelegate d_AfterUndoCheckOut;
        private PLMBizItemDelegate d_AfterUndoNewRevision;
        private DateTimePicker dTFromTime;
        private DateTimePicker dTToTime;
        private GroupBox groupBox1;
        private GroupBox gSentSet;
        private Hashtable hsCols;
        private Hashtable hswide;
        private Label label1;
        private Label label10;
        private Label label11;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private ArrayList lstOrder;
        private ArrayList lstQuickSignItem = null;
        private SortableListView lvwSentLst;
        private ToolStripMenuItem mCancelSent;
        private ToolStripMenuItem mEndSent;
        private ToolStripMenuItem mItemOutPutBySentLst;
        private ToolStripMenuItem mItemOutPutByUnit;
        private ToolStripMenuItem mOpen;
        private ToolStripMenuItem mOutPut;
        private ToolStripMenuItem mSentByUnit;
        private Panel panel1;
        private TabControl tbCtrlSent;
        private TabPage tpSentLst;
        private TextBox txtBpm;
        private TextBox txtDocCode;
        private ComboBox txtIsSent;
        private ComboBox txtSignner;
        private TextBox txtSignSm;
        private ComboBox txtSignUnit;
        private TextBox txtTsdId;
        private ComboBox txtTsType;
        private ComboBox txtUnit;

        public UcSent()
        {
            this.InitializeComponent();
            this.Init();
        }

        private void AfterDeleted(object sender, PLMOperationArgs e)
        {
            if ((this.lvwSentLst.Items.Count != 0) && (((e != null) && (e.BizItems != null)) && (e.BizItems.Length != 0)))
            {
                ArrayList list = new ArrayList(e.BizItems);
                for (int i = 0; i < this.lvwSentLst.Items.Count; i++)
                {
                    ListViewItem item = this.lvwSentLst.Items[i];
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
                            this.lvwSentLst.Items.RemoveAt(i);
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
                    if (!(item.ClassName != "DQDOSSIRSENT"))
                    {
                        PlArchivManage.UpdateLvwValues(this.hsCols, this.lvwSentLst, this.lstOrder, item);
                    }
                }
            }
        }

        private void btnSch_Click(object sender, EventArgs e)
        {
            if (this.dTFromTime.Value > this.dTToTime.Value)
            {
                MessageBox.Show("截止时间的起始日期存在问题");
            }
            else
            {
                this.lvwSentLst.Items.Clear();
                DataSet set;
                if (!this.IsSUIJI)
                {
                    set = PlArchivManage.Agent.GetSentLst(this.txtDocCode.Text, this.txtBpm.Text, this.txtTsdId.Text, this.txtUnit.Text, this.txtTsType.Text, this.txtIsSent.Text, this.dTFromTime.Value, this.dTToTime.Value);
                }
                else
                {
                    set = PlArchivManage.Agent.GetSentLstSuiJi(this.txtDocCode.Text, this.txtBpm.Text, this.txtTsdId.Text, this.txtUnit.Text, this.txtTsType.Text, this.txtIsSent.Text, this.dTFromTime.Value, this.dTToTime.Value);
                
                }
                if (((set != null) && (set.Tables.Count > 0)) && (set.Tables[0].Rows.Count > 0))
                {
                    PlArchivManage.SetLvwClsValues(this.hsCols, this.lvwSentLst, this.lstOrder, set.Tables[0], "DQDOSSIRSENT");
                }
                this.lvwSentLst.Refresh();
            }
        }

        private void btnSignOK_Click(object sender, EventArgs e)
        {
            if (this.lvwSentLst.Items.Count != 0)
            {
                string text = this.txtSignUnit.Text;
                string str2 = this.txtSignner.Text;
                string sm = this.txtSignSm.Text;
                if (string.IsNullOrEmpty(text))
                {
                    MessageBox.Show("没有设置签收单位");
                }
                else if (string.IsNullOrEmpty(str2))
                {
                    MessageBox.Show("签收人没有选择");
                }
                else
                {
                    ArrayList list = new ArrayList();
                    for (int i = 0; i < this.lvwSentLst.Items.Count; i++)
                    {
                        StringBuilder builder;
                        DEBusinessItem tag = this.lvwSentLst.Items[i].Tag as DEBusinessItem;
                        list.Add(tag);
                        PlArchivManage.QuickSign(tag, text, str2, sm, out builder);
                        if (builder.Length == 0)
                        {
                            FrmArchivManage.frmMian.DisplayTextInRichtBox(text + "对" + tag.Id + "进行签收，签收人：" + str2, 1, true);
                            if ((tag.State == ItemState.CheckOut) && (tag.Holder == ClientData.LogonUser.Oid))
                            {
                                tag.Iteration = PLItem.UpdateItemIteration(tag.Iteration, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption);
                            }
                            else
                            {
                                tag.Iteration = PLItem.UpdateItemIterationDirectly(tag, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption, false);
                            }
                            this.ReNewOpen(tag);
                        }
                        else
                        {
                            FrmArchivManage.frmMian.DisplayTextInRichtBox(text + "对" + tag.Id + "进行签收失败：" + builder.ToString(), 0, true);
                            tag.Iteration = PLItem.Agent.GetItemIteration(tag.IterOid, tag.ClassName, false, ClientData.LogonUser.Oid);
                        }
                    }
                    PlArchivManage.Agent.SignSentList(list, text, str2, sm);
                    foreach (DEBusinessItem item2 in list)
                    {
                        StringBuilder strErr = PlArchivManage.CheckSentRight(item2, true);
                        if (strErr.Length == 0)
                        {
                            PlArchivManage.EndSent(item2, sm, out strErr);
                            if (strErr.Length == 0)
                            {
                                FrmArchivManage.frmMian.DisplayTextInRichtBox("对" + item2.Id + "完成处理", 1, true);
                            }
                            this.ReNewOpen(item2);
                        }
                    }
                    if (BizItemHandlerEvent.Instance.D_AfterIterationUpdated != null)
                    {
                        BizItemHandlerEvent.Instance.D_AfterIterationUpdated((IBizItem[]) list.ToArray(typeof(IBizItem)));
                    }
                    int selectedIndex = this.txtSignUnit.SelectedIndex;
                    this.txtSignUnit.SelectedIndex = 0;
                    this.txtSignUnit.SelectedIndex = selectedIndex;
                }
            }
        }

        private void btnTsClear_Click(object sender, EventArgs e)
        {
            this.lvwSentLst.Items.Clear();
        }

        private void chkEndSent_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkEndSent.Checked)
            {
                this.chkEndSent.Checked = false;
                this.txtSignSm.Text = "";
                this.txtSignner.SelectedIndex = 0;
                this.txtSignUnit.SelectedIndex = 0;
                this.gSentSet.Enabled = false;
            }
        }

        private void CloseTsdTab(object obj)
        {
            string str = obj.ToString();
            TabPage page = this.tbCtrlSent.TabPages[str];
            if (page != null)
            {
                this.tbCtrlSent.TabPages.Remove(page);
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
            BizItemHandlerEvent.Instance.D_AfterUndoAbandon = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterUndoAbandon, this.d_AfterUndoAbandon);
            BizItemHandlerEvent.Instance.D_AfterAbandon = (PLMBizItemDelegate) Delegate.Remove(BizItemHandlerEvent.Instance.D_AfterAbandon, this.d_AfterAbandon);
            DelegatesOfAm.Instance.D_AfterTsdTabClose = (PLMSimpleDelegate) Delegate.Remove(DelegatesOfAm.Instance.D_AfterTsdTabClose, this.d_AfterTabClose);
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Init()
        {
            PlArchivManage.SetComBoxItem("托晒方式", this.txtTsType, "", false);
            PlArchivManage.SetComBoxItem("路线部门", this.txtUnit, "", false);
            PlArchivManage.SetComBoxItem("收发状态", this.txtIsSent, "未收发", false);
            DateTime time = DateTime.Now.AddDays(-7.0);
            this.dTFromTime.Value = new DateTime(time.Year, time.Month, time.Day);
            DateTime time2 = this.dTToTime.Value.AddDays(15.0).AddSeconds(-1.0);
            this.dTToTime.Value = time2;
            this.hsCols = PlArchivManage.Agent.GetViewOfCol("收发", out this.lstOrder, out this.hswide);
            PlArchivManage.SetCol(this.hsCols, this.lvwSentLst, "Sent", this.lstOrder, this.hswide);
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
            this.d_AfterTabClose = new PLMSimpleDelegate(this.CloseTsdTab);
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
            DelegatesOfAm.Instance.D_AfterSentTabClose = (PLMSimpleDelegate) Delegate.Combine(DelegatesOfAm.Instance.D_AfterSentTabClose, this.d_AfterTabClose);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.tbCtrlSent = new TabControl();
            this.tpSentLst = new TabPage();
            this.lvwSentLst = new SortableListView();
            this.panel1 = new Panel();
            this.gSentSet = new GroupBox();
            this.txtSignSm = new TextBox();
            this.label11 = new Label();
            this.chkEndSent = new CheckBox();
            this.btnSignOK = new Button();
            this.txtSignner = new ComboBox();
            this.label10 = new Label();
            this.txtSignUnit = new ComboBox();
            this.label9 = new Label();
            this.txtBpm = new TextBox();
            this.label7 = new Label();
            this.txtTsdId = new TextBox();
            this.label6 = new Label();
            this.txtUnit = new ComboBox();
            this.label2 = new Label();
            this.btnSch = new Button();
            this.txtTsType = new ComboBox();
            this.txtDocCode = new TextBox();
            this.txtIsSent = new ComboBox();
            this.label3 = new Label();
            this.groupBox1 = new GroupBox();
            this.dTFromTime = new DateTimePicker();
            this.label8 = new Label();
            this.label5 = new Label();
            this.dTToTime = new DateTimePicker();
            this.label4 = new Label();
            this.label1 = new Label();
            this.btnTsClear = new Button();
            this.cMenuSent = new ContextMenuStrip(this.components);
            this.mOpen = new ToolStripMenuItem();
            this.mEndSent = new ToolStripMenuItem();
            this.mOutPut = new ToolStripMenuItem();
            this.mItemOutPutByUnit = new ToolStripMenuItem();
            this.mItemOutPutBySentLst = new ToolStripMenuItem();
            this.mCancelSent = new ToolStripMenuItem();
            this.mSentByUnit = new ToolStripMenuItem();
            this.tbCtrlSent.SuspendLayout();
            this.tpSentLst.SuspendLayout();
            this.panel1.SuspendLayout();
            this.gSentSet.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.cMenuSent.SuspendLayout();
            base.SuspendLayout();
            this.tbCtrlSent.Controls.Add(this.tpSentLst);
            this.tbCtrlSent.Dock = DockStyle.Fill;
            this.tbCtrlSent.Location = new Point(0, 0);
            this.tbCtrlSent.Name = "tbCtrlSent";
            this.tbCtrlSent.SelectedIndex = 0;
            this.tbCtrlSent.Size = new Size(0x367, 0x17a);
            this.tbCtrlSent.TabIndex = 1;
            this.tpSentLst.Controls.Add(this.lvwSentLst);
            this.tpSentLst.Controls.Add(this.panel1);
            this.tpSentLst.Location = new Point(4, 0x16);
            this.tpSentLst.Name = "tpSentLst";
            this.tpSentLst.Padding = new Padding(3);
            this.tpSentLst.Size = new Size(0x35f, 0x160);
            this.tpSentLst.TabIndex = 0;
            this.tpSentLst.Text = "收发列表";
            this.tpSentLst.UseVisualStyleBackColor = true;
            this.lvwSentLst.AllowDrop = true;
            this.lvwSentLst.Dock = DockStyle.Fill;
            this.lvwSentLst.FullRowSelect = true;
            this.lvwSentLst.HideSelection = false;
            this.lvwSentLst.Location = new Point(3, 0x76);
            this.lvwSentLst.Name = "lvwSentLst";
            this.lvwSentLst.Size = new Size(0x359, 0xe7);
            this.lvwSentLst.SortingOrder = SortOrder.None;
            this.lvwSentLst.TabIndex = 1;
            this.lvwSentLst.UseCompatibleStateImageBehavior = false;
            this.lvwSentLst.View = View.Details;
            this.lvwSentLst.DragDrop += new DragEventHandler(this.lvwSentLst_DragDrop);
            this.lvwSentLst.DragEnter += new DragEventHandler(this.lvwSentLst_DragEnter);
            this.lvwSentLst.DoubleClick += new EventHandler(this.lvwSentLst_DoubleClick);
            this.lvwSentLst.MouseUp += new MouseEventHandler(this.lvwSentLst_MouseUp);
            this.panel1.BackColor = Color.WhiteSmoke;
            this.panel1.Controls.Add(this.gSentSet);
            this.panel1.Controls.Add(this.txtBpm);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.txtTsdId);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.txtUnit);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btnSch);
            this.panel1.Controls.Add(this.txtTsType);
            this.panel1.Controls.Add(this.txtDocCode);
            this.panel1.Controls.Add(this.txtIsSent);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnTsClear);
            this.panel1.Dock = DockStyle.Top;
            this.panel1.Location = new Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0x359, 0x73);
            this.panel1.TabIndex = 0;
            this.gSentSet.Controls.Add(this.txtSignSm);
            this.gSentSet.Controls.Add(this.label11);
            this.gSentSet.Controls.Add(this.chkEndSent);
            this.gSentSet.Controls.Add(this.btnSignOK);
            this.gSentSet.Controls.Add(this.txtSignner);
            this.gSentSet.Controls.Add(this.label10);
            this.gSentSet.Controls.Add(this.txtSignUnit);
            this.gSentSet.Controls.Add(this.label9);
            this.gSentSet.Enabled = false;
            this.gSentSet.Location = new Point(-3, 0x44);
            this.gSentSet.Name = "gSentSet";
            this.gSentSet.Size = new Size(0x34b, 0x2c);
            this.gSentSet.TabIndex = 0x38;
            this.gSentSet.TabStop = false;
            this.txtSignSm.Location = new Point(0x1df, 0x13);
            this.txtSignSm.Name = "txtSignSm";
            this.txtSignSm.Size = new Size(0xbc, 0x15);
            this.txtSignSm.TabIndex = 0x1f;
            this.label11.AutoSize = true;
            this.label11.Location = new Point(420, 0x15);
            this.label11.Name = "label11";
            this.label11.Size = new Size(0x35, 12);
            this.label11.TabIndex = 30;
            this.label11.Text = "发放说明";
            this.chkEndSent.AutoSize = true;
            this.chkEndSent.Location = new Point(0x2a1, 0x15);
            this.chkEndSent.Name = "chkEndSent";
            this.chkEndSent.Size = new Size(0x48, 0x10);
            this.chkEndSent.TabIndex = 5;
            this.chkEndSent.Text = "结束签收";
            this.chkEndSent.UseVisualStyleBackColor = true;
            this.chkEndSent.CheckedChanged += new EventHandler(this.chkEndSent_CheckedChanged);
            this.btnSignOK.Location = new Point(0x2f4, 0x10);
            this.btnSignOK.Name = "btnSignOK";
            this.btnSignOK.Size = new Size(0x4b, 0x17);
            this.btnSignOK.TabIndex = 4;
            this.btnSignOK.Text = "签收";
            this.btnSignOK.UseVisualStyleBackColor = true;
            this.btnSignOK.Click += new EventHandler(this.btnSignOK_Click);
            this.txtSignner.FormattingEnabled = true;
            this.txtSignner.Location = new Point(0x120, 0x12);
            this.txtSignner.Name = "txtSignner";
            this.txtSignner.Size = new Size(0x79, 20);
            this.txtSignner.TabIndex = 3;
            this.label10.AutoSize = true;
            this.label10.Location = new Point(0xe3, 0x15);
            this.label10.Name = "label10";
            this.label10.Size = new Size(0x29, 12);
            this.label10.TabIndex = 2;
            this.label10.Text = "签收人";
            this.txtSignUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            this.txtSignUnit.FormattingEnabled = true;
            this.txtSignUnit.Location = new Point(80, 0x12);
            this.txtSignUnit.Name = "txtSignUnit";
            this.txtSignUnit.Size = new Size(0x79, 20);
            this.txtSignUnit.TabIndex = 1;
            this.txtSignUnit.SelectedIndexChanged += new EventHandler(this.txtSignUnit_SelectedIndexChanged);
            this.label9.AutoSize = true;
            this.label9.Location = new Point(20, 0x15);
            this.label9.Name = "label9";
            this.label9.Size = new Size(0x35, 12);
            this.label9.TabIndex = 0;
            this.label9.Text = "签收单位";
            this.txtBpm.Location = new Point(0x4d, 0x29);
            this.txtBpm.Name = "txtBpm";
            this.txtBpm.Size = new Size(0x84, 0x15);
            this.txtBpm.TabIndex = 0x37;
            this.label7.AutoSize = true;
            this.label7.Location = new Point(15, 0x2e);
            this.label7.Name = "label7";
            this.label7.Size = new Size(0x35, 12);
            this.label7.TabIndex = 0x36;
            this.label7.Text = "流程信息";
            this.txtTsdId.Location = new Point(0x11d, 14);
            this.txtTsdId.Name = "txtTsdId";
            this.txtTsdId.Size = new Size(0x77, 0x15);
            this.txtTsdId.TabIndex = 0x35;
            this.label6.AutoSize = true;
            this.label6.Location = new Point(0xdb, 0x11);
            this.label6.Name = "label6";
            this.label6.Size = new Size(0x35, 12);
            this.label6.TabIndex = 0x34;
            this.label6.Text = "托晒单号";
            this.txtUnit.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.txtUnit.AutoCompleteSource = AutoCompleteSource.ListItems;
            this.txtUnit.FormattingEnabled = true;
            this.txtUnit.Location = new Point(0x11d, 0x2c);
            this.txtUnit.Name = "txtUnit";
            this.txtUnit.Size = new Size(120, 20);
            this.txtUnit.TabIndex = 0x33;
            this.label2.AutoSize = true;
            this.label2.Location = new Point(0xde, 0x2d);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x35, 12);
            this.label2.TabIndex = 50;
            this.label2.Text = "发放单位";
            this.btnSch.Location = new Point(0x2f1, 12);
            this.btnSch.Name = "btnSch";
            this.btnSch.Size = new Size(0x4b, 0x18);
            this.btnSch.TabIndex = 0x31;
            this.btnSch.Text = "查询";
            this.btnSch.UseVisualStyleBackColor = true;
            this.btnSch.Click += new EventHandler(this.btnSch_Click);
            this.txtTsType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.txtTsType.FormattingEnabled = true;
            this.txtTsType.Location = new Point(0x1dc, 14);
            this.txtTsType.Name = "txtTsType";
            this.txtTsType.Size = new Size(0x5c, 20);
            this.txtTsType.TabIndex = 0x30;
            this.txtDocCode.Location = new Point(0x4d, 12);
            this.txtDocCode.Name = "txtDocCode";
            this.txtDocCode.Size = new Size(0x84, 0x15);
            this.txtDocCode.TabIndex = 0x2e;
            this.txtIsSent.DropDownStyle = ComboBoxStyle.DropDownList;
            this.txtIsSent.FormattingEnabled = true;
            this.txtIsSent.Location = new Point(0x1dc, 0x2a);
            this.txtIsSent.Name = "txtIsSent";
            this.txtIsSent.Size = new Size(0x5c, 20);
            this.txtIsSent.TabIndex = 0x2d;
            this.label3.AutoSize = true;
            this.label3.Location = new Point(0x1a1, 0x2d);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x35, 12);
            this.label3.TabIndex = 0x2c;
            this.label3.Text = "收发状态";
            this.groupBox1.Controls.Add(this.dTFromTime);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.dTToTime);
            this.groupBox1.Location = new Point(580, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(0xa2, 0x45);
            this.groupBox1.TabIndex = 0x2a;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "截止日期";
            this.dTFromTime.Location = new Point(0x23, 0x10);
            this.dTFromTime.Name = "dTFromTime";
            this.dTFromTime.Size = new Size(0x76, 0x15);
            this.dTFromTime.TabIndex = 0x17;
            this.label8.AutoSize = true;
            this.label8.Location = new Point(9, 0x2a);
            this.label8.Name = "label8";
            this.label8.Size = new Size(0x11, 12);
            this.label8.TabIndex = 0x16;
            this.label8.Text = "至";
            this.label5.AutoSize = true;
            this.label5.Location = new Point(9, 0x13);
            this.label5.Name = "label5";
            this.label5.Size = new Size(0x11, 12);
            this.label5.TabIndex = 0x15;
            this.label5.Text = "从";
            this.dTToTime.Location = new Point(0x23, 0x2a);
            this.dTToTime.Name = "dTToTime";
            this.dTToTime.Size = new Size(0x76, 0x15);
            this.dTToTime.TabIndex = 20;
            this.label4.AutoSize = true;
            this.label4.Location = new Point(0x1a1, 0x13);
            this.label4.Name = "label4";
            this.label4.Size = new Size(0x35, 12);
            this.label4.TabIndex = 0x29;
            this.label4.Text = "收发类型";
            this.label1.AutoSize = true;
            this.label1.Location = new Point(13, 15);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x35, 12);
            this.label1.TabIndex = 0x26;
            this.label1.Text = "图    号";
            this.btnTsClear.Location = new Point(0x2f1, 0x2c);
            this.btnTsClear.Name = "btnTsClear";
            this.btnTsClear.Size = new Size(0x4b, 0x18);
            this.btnTsClear.TabIndex = 0x25;
            this.btnTsClear.Text = "清空";
            this.btnTsClear.UseVisualStyleBackColor = true;
            this.btnTsClear.Click += new EventHandler(this.btnTsClear_Click);
            this.cMenuSent.Items.AddRange(new ToolStripItem[] { this.mOpen, this.mEndSent, this.mOutPut, this.mCancelSent, this.mSentByUnit });
            this.cMenuSent.Name = "cMenuSent";
            this.cMenuSent.Size = new Size(0x89, 0x72);
            this.mOpen.Name = "mOpen";
            this.mOpen.Size = new Size(0x88, 0x16);
            this.mOpen.Text = "打开收发单";
            this.mOpen.Click += new EventHandler(this.mOpen_Click);
            this.mEndSent.Name = "mEndSent";
            this.mEndSent.Size = new Size(0x88, 0x16);
            this.mEndSent.Text = "完 成 收 发";
            this.mEndSent.Click += new EventHandler(this.mEndSent_Click);
            this.mOutPut.DropDownItems.AddRange(new ToolStripItem[] { this.mItemOutPutByUnit, this.mItemOutPutBySentLst });
            this.mOutPut.Name = "mOutPut";
            this.mOutPut.Size = new Size(0x88, 0x16);
            this.mOutPut.Text = "输出收发单";
            this.mItemOutPutByUnit.Name = "mItemOutPutByUnit";
            this.mItemOutPutByUnit.Size = new Size(0x94, 0x16);
            this.mItemOutPutByUnit.Text = " 按接受单位";
            this.mItemOutPutByUnit.Click += new EventHandler(this.mItemOutPutByUnit_Click);
            this.mItemOutPutBySentLst.Name = "mItemOutPutBySentLst";
            this.mItemOutPutBySentLst.Size = new Size(0x94, 0x16);
            this.mItemOutPutBySentLst.Text = "按收发单代号";
            this.mItemOutPutBySentLst.Click += new EventHandler(this.mItemOutPutBySentLst_Click);
            this.mCancelSent.Name = "mCancelSent";
            this.mCancelSent.Size = new Size(0x88, 0x16);
            this.mCancelSent.Text = "取 消 收 发";
            this.mCancelSent.Click += new EventHandler(this.mCancelSent_Click);
            this.mSentByUnit.Name = "mSentByUnit";
            this.mSentByUnit.Size = new Size(0x88, 0x16);
            this.mSentByUnit.Text = "批 量 签 收";
            this.mSentByUnit.Click += new EventHandler(this.mSentByUnit_Click);
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            base.Controls.Add(this.tbCtrlSent);
            base.Name = "UcSent";
            base.Size = new Size(0x367, 0x17a);
            base.Load += new EventHandler(this.UcSent_Load);
            this.tbCtrlSent.ResumeLayout(false);
            this.tpSentLst.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.gSentSet.ResumeLayout(false);
            this.gSentSet.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.cMenuSent.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void lvwSentLst_DoubleClick(object sender, EventArgs e)
        {
            if (this.lvwSentLst.SelectedItems.Count != 0)
            {
                ListViewItem item = this.lvwSentLst.SelectedItems[0];
                if (PLGrantPerm.CanDoClassOperation(ClientData.LogonUser.Oid, "DQDOSSIRSENT", Guid.Empty, "ClaRel_BROWSE") == 1)
                {
                    DEBusinessItem tag = item.Tag as DEBusinessItem;
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
        }

        private void lvwSentLst_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data != null)
            {
                DEBusinessItem item;
                ArrayList list = new ArrayList();
                ArrayList list2 = new ArrayList();
                foreach (ListViewItem item2 in this.lvwSentLst.Items)
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
                        if ((item.Master.ClassName == "DQDOSSIRSENT") && ((item != null) && !list2.Contains(item.MasterOid)))
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
                        if (item.Master.ClassName != "DQDOSSIRSENT")
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
                        PlArchivManage.SetLvwValues(this.hsCols, this.lvwSentLst, this.lstOrder, item4);
                    }
                    this.lvwSentLst.Refresh();
                }
            }
        }

        private void lvwSentLst_DragEnter(object sender, DragEventArgs e)
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

        private void lvwSentLst_MouseUp(object sender, MouseEventArgs e)
        {
            if (((e.Button == MouseButtons.Right) && (e.Clicks == 1)) && (this.lvwSentLst.GetItemAt(e.X, e.Y) != null))
            {
                this.cMenuSent.Items.Clear();
                if (this.lvwSentLst.SelectedItems.Count == 1)
                {
                    this.cMenuSent.Items.Add(this.mOpen);
                }
                this.cMenuSent.Items.Add(this.mEndSent);
                this.cMenuSent.Items.Add(this.mSentByUnit);
                this.cMenuSent.Items.Add(this.mCancelSent);
                this.cMenuSent.Items.Add(this.mOutPut);
                if (this.cMenuSent.Items.Count > 0)
                {
                    this.cMenuSent.Show(this.lvwSentLst, e.Location);
                }
            }
        }

        private void mCancelSent_Click(object sender, EventArgs e)
        {
            if (this.lvwSentLst.SelectedItems.Count != 0)
            {
                ArrayList list = new ArrayList();
                foreach (ListViewItem item in this.lvwSentLst.SelectedItems)
                {
                    DEBusinessItem tag = item.Tag as DEBusinessItem;
                    if ((tag.State == ItemState.CheckOut) && (tag.Holder == ClientData.LogonUser.Oid))
                    {
                        list.Add(tag);
                    }
                    else if (tag.State == ItemState.CheckIn)
                    {
                        list.Add(tag);
                    }
                }
                if (list.Count == 0)
                {
                    MessageBox.Show("收发帐不符合要求，被别人检出，或已经定版！");
                }
                else
                {
                    int num;
                    DEBusinessItem item3;
                    StringBuilder builder = new StringBuilder();
                    for (num = 0; num < list.Count; num++)
                    {
                        item3 = list[num] as DEBusinessItem;
                        StringBuilder builder2 = PlArchivManage.CheckSentRight(item3, false);
                        if (builder2.Length > 0)
                        {
                            builder.Append(item3.Id + "不能完成取消收发\r\t" + builder2);
                            list.Remove(item3);
                            num--;
                        }
                    }
                    if (builder.Length > 0)
                    {
                        FrmArchivManage.frmMian.DisplayTextInRichtBox(builder.ToString(), 0, true);
                    }
                    StringBuilder builder3 = new StringBuilder();
                    for (num = 0; num < list.Count; num++)
                    {
                        int num2;
                        DEBusinessItem item4;
                        item3 = list[num] as DEBusinessItem;
                        bool isJustOne = (list.Count - num) > 1;
                        FrmInputRemark remark = new FrmInputRemark("收发帐:" + item3.Id + "取消收发", false, isJustOne);
                        DialogResult result = remark.ShowDialog();
                        bool isSameReason = remark.IsSameReason;
                        if (result != DialogResult.OK)
                        {
                            if (isSameReason)
                            {
                                num2 = num;
                                while (num2 < list.Count)
                                {
                                    item4 = list[num2] as DEBusinessItem;
                                    list.RemoveAt(num2);
                                    num2--;
                                    num2++;
                                }
                            }
                            else
                            {
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
                                    PlArchivManage.CancelSent(item4, remark.StrMarkup);
                                    this.ReNewOpen(item3);
                                }
                                break;
                            }
                            PlArchivManage.CancelSent(item3, remark.StrMarkup);
                            this.ReNewOpen(item3);
                        }
                    }
                    if (list.Count > 0)
                    {
                        for (num = 0; num < list.Count; num++)
                        {
                            item3 = list[num] as DEBusinessItem;
                            builder3.Append(item3.Id);
                            builder3.Append(";");
                        }
                    }
                    if (builder3.Length > 0)
                    {
                        BizItemHandlerEvent.Instance.D_AfterIterationUpdated((IBizItem[]) list.ToArray(typeof(IBizItem)));
                        builder3 = builder3.Remove(builder3.Length - 1, 1);
                        builder3.Insert(0, "\r\n 取消收发完成");
                        FrmArchivManage.frmMian.DisplayTextInRichtBox(builder3.ToString(), 1, true);
                    }
                }
            }
        }

        private void mEndSent_Click(object sender, EventArgs e)
        {
            if (this.lvwSentLst.SelectedItems.Count != 0)
            {
                ArrayList list = new ArrayList();
                foreach (ListViewItem item in this.lvwSentLst.SelectedItems)
                {
                    DEBusinessItem tag = item.Tag as DEBusinessItem;
                    if ((tag.State == ItemState.CheckOut) && (tag.Holder == ClientData.LogonUser.Oid))
                    {
                        list.Add(tag);
                    }
                    else if (tag.State == ItemState.CheckIn)
                    {
                        object attrValue = tag.Iteration.GetAttrValue(ConstAm.SENT_ATTR_SENTSTATUS);
                        string str = (attrValue == null) ? "" : attrValue.ToString();
                        if (str != "已收发")
                        {
                            list.Add(tag);
                        }
                    }
                }
                if (list.Count == 0)
                {
                    MessageBox.Show("收发帐不符合要求，被别人检出，或已经定版！");
                }
                else
                {
                    int num;
                    DEBusinessItem item3;
                    StringBuilder strErr = new StringBuilder();
                    for (num = 0; num < list.Count; num++)
                    {
                        item3 = list[num] as DEBusinessItem;
                        StringBuilder builder2 = PlArchivManage.CheckSentRight(item3, true);
                        if (builder2.Length > 0)
                        {
                            strErr.Append(item3.Id + "不能完成收发\r\t" + builder2);
                            list.Remove(item3);
                            num--;
                        }
                    }
                    if (strErr.Length > 0)
                    {
                        FrmArchivManage.frmMian.DisplayTextInRichtBox(strErr.ToString(), 0, true);
                    }
                    else
                    {
                        PlArchivManage.Agent.SignSentList(list, string.Empty, string.Empty, string.Empty);
                        StringBuilder builder3 = new StringBuilder();
                        for (num = 0; num < list.Count; num++)
                        {
                            int num2;
                            DEBusinessItem item4;
                            item3 = list[num] as DEBusinessItem;
                            bool isJustOne = (list.Count - num) > 1;
                            FrmInputRemark remark = new FrmInputRemark("收发帐:" + item3.Id + "结束收发", true, isJustOne);
                            DialogResult result = remark.ShowDialog();
                            bool isSameReason = remark.IsSameReason;
                            if (result != DialogResult.OK)
                            {
                                if (isSameReason)
                                {
                                    num2 = num;
                                    while (num2 < list.Count)
                                    {
                                        item4 = list[num2] as DEBusinessItem;
                                        list.RemoveAt(num2);
                                        num2--;
                                        num2++;
                                    }
                                }
                                else
                                {
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
                                        PlArchivManage.EndSent(item4, remark.StrMarkup, out strErr);
                                        this.ReNewOpen(item3);
                                    }
                                    break;
                                }
                                PlArchivManage.EndSent(item3, remark.StrMarkup, out strErr);
                                this.ReNewOpen(item3);
                            }
                        }
                        if (list.Count > 0)
                        {
                            for (num = 0; num < list.Count; num++)
                            {
                                item3 = list[num] as DEBusinessItem;
                                builder3.Append(item3.Id);
                                builder3.Append(";");
                            }
                        }
                        if (builder3.Length > 0)
                        {
                            builder3 = builder3.Remove(builder3.Length - 1, 1);
                            builder3.Insert(0, "\r\n 收发完成");
                            FrmArchivManage.frmMian.DisplayTextInRichtBox(builder3.ToString(), 1, true);
                        }
                    }
                    
                }
            }
        }

        private void mItemOutPutBySentLst_Click(object sender, EventArgs e)
        {
            if (this.lvwSentLst.SelectedItems.Count != 0)
            {
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "图纸分发登记.xls");
                string str2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "图纸回收登记.xls");
                if (!File.Exists(path))
                {
                    MessageBox.Show("《图纸分发登记.xls》 没有部署到插件的客户端中，不能输出分发登记表");
                }
                else if (!File.Exists(str2))
                {
                    MessageBox.Show("《图纸回收登记.xls》 没有部署到插件的客户端中，不能输出回收登记表");
                }
                else
                {
                    foreach (ListViewItem item in this.lvwSentLst.SelectedItems)
                    {
                        DEBusinessItem tag = item.Tag as DEBusinessItem;
                        DataSet sentResultForOutPut = PlArchivManage.Agent.GetSentResultForOutPut(tag, ClientData.LogonUser.Name);
                        if ((sentResultForOutPut == null) || (sentResultForOutPut.Tables.Count == 0))
                        {
                            FrmArchivManage.frmMian.DisplayTextInRichtBox(tag.Id + "没有数据，无法导出收发登记", 2, true);
                        }
                        FolderBrowserDialog dialog = new FolderBrowserDialog {
                            Description = "导出" + tag.Id + "收发登记"
                        };
                        if (dialog.ShowDialog() != DialogResult.OK)
                        {
                            return;
                        }
                        string selectedPath = dialog.SelectedPath;
                        DataTable tb = sentResultForOutPut.Tables.Contains("FF") ? sentResultForOutPut.Tables["FF"] : null;
                        if ((tb != null) && (tb.Rows.Count > 0))
                        {
                            this.OutPutSentBom(tb, path, tag, selectedPath);
                            FrmArchivManage.frmMian.DisplayTextInRichtBox(tag.Id + "导出发放登记成功", 1, false);
                        }
                        DataTable table2 = sentResultForOutPut.Tables.Contains("HS") ? sentResultForOutPut.Tables["HS"] : null;
                        if ((table2 != null) && (table2.Rows.Count > 0))
                        {
                            this.OutPutSentBom(table2, str2, tag, selectedPath);
                            FrmArchivManage.frmMian.DisplayTextInRichtBox(tag.Id + "导出回收登记成功", 1, false);
                        }
                    }
                    FrmArchivManage.frmMian.ShowRs();
                }
            }
        }

        private void mItemOutPutByUnit_Click(object sender, EventArgs e)
        {
            if (this.lvwSentLst.SelectedItems.Count != 0)
            {
                ArrayList lstItems = new ArrayList();
                foreach (ListViewItem item in this.lvwSentLst.SelectedItems)
                {
                    DEBusinessItem tag = item.Tag as DEBusinessItem;
                    lstItems.Add(tag);
                }
                var tmp = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "图纸分发登记1.xls");
                string str2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "图纸回收登记1.xls");
                if (!File.Exists(path))
                {
                    MessageBox.Show("《图纸分发登记1.xls》 没有部署到插件的客户端中，不能输出分发登记表");
                }
                else if (!File.Exists(str2))
                {
                    MessageBox.Show("《图纸回收登记1.xls》 没有部署到插件的客户端中，不能输出回收登记表");
                }
                else
                {
                    FrmUnit unit = new FrmUnit();
                    if (unit.ShowDialog() != DialogResult.OK)
                    {
                        MessageBox.Show("输出取消");
                    }
                    else
                    {
                        Hashtable hashtable;
                        ArrayList lstUnit = unit.LstUnit;
                        DataSet set = PlArchivManage.Agent.GetSentResultForOutPut(lstUnit, lstItems, out hashtable, ClientData.LogonUser.Name);
                        if ((set == null) || (set.Tables.Count == 0))
                        {
                            FrmArchivManage.frmMian.DisplayTextInRichtBox("没有数据，无法导出收发登记", 2, true);
                        }
                        FolderBrowserDialog dialog = new FolderBrowserDialog {
                            Description = "按单位导出收发登记"
                        };
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string selectedPath = dialog.SelectedPath;
                            foreach (string str4 in lstUnit)
                            {
                                int num = 0;
                                if (hashtable.Contains(str4))
                                {
                                    num = (int) hashtable[str4];
                                }
                                if (num != 0)
                                {
                                    DataTable tb = set.Tables.Contains("FF" + num.ToString()) ? set.Tables["FF" + num.ToString()] : null;
                                    if ((tb != null) && (tb.Rows.Count > 0))
                                    {
                                        this.OutPutSentBomByUnit(tb, path, str4, selectedPath);
                                        FrmArchivManage.frmMian.DisplayTextInRichtBox(str4 + "导出发放登记成功", 1, false);
                                    }
                                    else
                                    {
                                        FrmArchivManage.frmMian.DisplayTextInRichtBox(str4 + "根据当前选择的内容没有发放登记", 2, false);
                                    }
                                    DataTable table2 = set.Tables.Contains("HS" + num.ToString()) ? set.Tables["HS" + num.ToString()] : null;
                                    if ((table2 != null) && (table2.Rows.Count > 0))
                                    {
                                        this.OutPutSentBomByUnit(table2, str2, str4, selectedPath);
                                        FrmArchivManage.frmMian.DisplayTextInRichtBox(str4 + "导出回收登记成功", 1, false);
                                    }
                                    else
                                    {
                                        FrmArchivManage.frmMian.DisplayTextInRichtBox(str4 + "根据当前选择的内容没有回收登记", 2, false);
                                    }
                                }
                            }
                            FrmArchivManage.frmMian.ShowRs();
                        }
                    }
                }
            }
        }

        private void mOpen_Click(object sender, EventArgs e)
        {
            if (this.lvwSentLst.SelectedItems.Count == 1)
            {
                ListViewItem item = this.lvwSentLst.SelectedItems[0];
                DEBusinessItem tag = item.Tag as DEBusinessItem;
                this.OpenSentEdit(tag);
            }
        }

        private void mSentByUnit_Click(object sender, EventArgs e)
        {
            if (this.lvwSentLst.SelectedItems.Count != 0)
            {
                ArrayList lstPrintItems = new ArrayList();
                foreach (ListViewItem item in this.lvwSentLst.SelectedItems)
                {
                    DEBusinessItem tag = item.Tag as DEBusinessItem;
                    if ((tag.State == ItemState.CheckOut) && (tag.Holder == ClientData.LogonUser.Oid))
                    {
                        lstPrintItems.Add(tag);
                    }
                    else if (tag.State == ItemState.CheckIn)
                    {
                        object attrValue = tag.Iteration.GetAttrValue(ConstAm.SENT_ATTR_SENTSTATUS);
                        string str = (attrValue == null) ? "" : attrValue.ToString();
                        if (str != "已收发")
                        {
                            lstPrintItems.Add(tag);
                        }
                    }
                }
                if (lstPrintItems.Count == 0)
                {
                    MessageBox.Show("收发帐不符合要求，被别人检出，或已经完成处理！");
                }
                else
                {
                    this.gSentSet.Enabled = true;
                    this.chkEndSent.Checked = false;
                    this.txtSignUnit.Items.Clear();
                    this.txtSignner.Items.Clear();
                    PlArchivManage.SetSigner(this.txtSignner);
                    ArrayList allUnitByQuickSign = PlArchivManage.GetAllUnitByQuickSign(lstPrintItems);
                    if (lstPrintItems.Count == 0)
                    {
                        MessageBox.Show("没有需要进行签收的数据");
                        this.chkEndSent.Checked = true;
                    }
                    else if (allUnitByQuickSign.Count == 0)
                    {
                        MessageBox.Show("没有需要进行签收的单位");
                        this.chkEndSent.Checked = true;
                    }
                    else
                    {
                        this.lstQuickSignItem = new ArrayList(lstPrintItems);
                        foreach (object obj3 in allUnitByQuickSign)
                        {
                            this.txtSignUnit.Items.Add(obj3);
                        }
                        this.txtSignUnit.Items.Insert(0, "");
                        this.txtSignUnit.SelectedIndex = 0;
                    }
                }
            }
        }

        private void OpenSentEdit(DEBusinessItem item)
        {
            TabPage page = null;
            string id = item.Id;
            if (this.tbCtrlSent.TabPages[id] != null)
            {
                page = this.tbCtrlSent.TabPages[id];
            }
            if (page == null)
            {
                page = new TabPage(id) {
                    Name = id
                };
                UcSentItem item2 = new UcSentItem(item) {
                    Dock = DockStyle.Fill
                };
                page.Controls.Add(item2);
                this.tbCtrlSent.TabPages.Add(page);
            }
            this.tbCtrlSent.SelectTab(page);
        }

        private void OutPutSentBom(DataTable tb, string tmpPath, DEBusinessItem item, string folder)
        {
            string fileName = Path.GetFileName(tmpPath);
            fileName = "(" + item.Id + ")" + fileName;
            fileName = Path.Combine(folder, fileName);
            try
            {
                File.Copy(tmpPath, fileName, true);
            }
            catch (Exception exception)
            {
                MessageBox.Show("无法拷贝导出模板文件到指定位置：" + exception.Message);
                return;
            }
            DataSet ds = new DataSet();
            DataTable table = tb.Copy();
            ds.Tables.Add(table);
            ArrayList list = OutPutExcel.FindLable(fileName);
            Hashtable hsLable = new Hashtable();
            if (list.Count > 0)
            {
                ArrayList attributes = ModelContext.MetaModel.GetAttributes("DQDOSSIRSENT", 1);
                foreach (DEMetaAttribute attribute in attributes)
                {
                    string label;
                    object obj2 = null;
                    if (list.Contains(attribute.Label))
                    {
                        label = attribute.Label;
                    }
                    else
                    {
                        if (!list.Contains(attribute.Name))
                        {
                            continue;
                        }
                        label = attribute.Name;
                    }
                    object attrValue = item.Iteration.GetAttrValue(attribute.Name);
                    if (attrValue != null)
                    {
                        switch (attribute.DataType)
                        {
                            case 0:
                            case 1:
                                try
                                {
                                    obj2 = Convert.ToUInt32(attrValue);
                                }
                                catch
                                {
                                }
                                break;

                            case 3:
                            case 4:
                            case 5:
                                obj2 = item.Iteration.GetAttrValue(attribute.Name).ToString();
                                break;

                            case 6:
                                try
                                {
                                    obj2 = Convert.ToDecimal(attrValue);
                                }
                                catch
                                {
                                }
                                break;

                            case 7:
                                obj2 = Convert.ToDateTime(attrValue).ToShortDateString();
                                break;
                        }
                        if (obj2 != null)
                        {
                            hsLable.Add(label, obj2);
                        }
                    }
                }
            }
            OutPutExcel.GetReportResult(ds, hsLable, fileName);
        }

        private void OutPutSentBomByUnit(DataTable tb, string tmpPath, string unit, string fPh)
        {
            string fileName = Path.GetFileName(tmpPath);
            fileName = "(" + unit + ")" + fileName;
            fileName = Path.Combine(fPh, fileName);
            try
            {
                File.Copy(tmpPath, fileName, true);
            }
            catch (Exception exception)
            {
                MessageBox.Show("无法拷贝导出模板文件到指定位置：" + exception.Message);
                return;
            }
            DataSet ds = new DataSet();
            DataTable table = tb.Copy();
            ds.Tables.Add(table);
            int num = 0;
            int num2 = 0;
            int tmpnum = 0;
            foreach (DataRow row in table.Rows)
            {
                tmpnum = (row["份数"] == DBNull.Value) ? 0 : Convert.ToInt32(row["份数"]);
                num += tmpnum;
                num2 += (row["每份张数"] == DBNull.Value) ? 0 :tmpnum * Convert.ToInt32(row["每份张数"]);
            }
            ArrayList list = OutPutExcel.FindLable(fileName);
            Hashtable hsLable = new Hashtable();
            foreach (string str2 in list)
            {
                string str3 = str2;
                if (str3 != null)
                {
                    if (!(str3 == "单位"))
                    {
                        if ((str3 == "份数") || (str3 == "总份数"))
                        {
                            goto Label_018F;
                        }
                        if (str3 == "每份张数")
                        {
                            goto Label_01A2;
                        }
                    }
                    else
                    {
                        hsLable[str2] = unit;
                    }
                }
                continue;
            Label_018F:
                hsLable[str2] = num;
                continue;
            Label_01A2:
                hsLable[str2] = num2;
            }
            OutPutExcel.GetReportResult(ds, hsLable, fileName);
        }

        private void ReNewOpen(DEBusinessItem item)
        {
            TabPage page = this.tbCtrlSent.TabPages[item.Id];
            if (page != null)
            {
                this.CloseTsdTab(item.Id);
                this.OpenSentEdit(item);
            }
        }

        private void txtSignUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = this.txtSignUnit.Text;
            this.lvwSentLst.Items.Clear();
            for (int i = 0; i < this.lstQuickSignItem.Count; i++)
            {
                DEBusinessItem item = this.lstQuickSignItem[i] as DEBusinessItem;
                if (string.IsNullOrEmpty(text) || PlArchivManage.IsUnSent(item, text))
                {
                    PlArchivManage.SetLvwValues(this.hsCols, this.lvwSentLst, this.lstOrder, item);
                }
            }
            this.lvwSentLst.Refresh();
        }

        private void UcSent_Load(object sender, EventArgs e)
        {
        }

        public bool IsSUIJI { get; set; }
    }
}

