namespace DQArchivManageClt
{
    using DQArchivManageCommon;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;
    using Thyt.TiPLM.DEL.Admin.DataModel;
    using Thyt.TiPLM.DEL.Product;
    using Thyt.TiPLM.PLL.Admin.DataModel;
    using Thyt.TiPLM.PLL.Admin.NewResponsibility;
    using Thyt.TiPLM.PLL.Product2;
    using Thyt.TiPLM.UIL.Common;
    using Thyt.TiPLM.UIL.Product.Common;
    using Thyt.TiPLM.UIL.Product.Common.UserControls;

    public class UcTsdItem : UserControl
    {
        private bool _bEdit;
        private bool _bNew;
        private string _bpmName;
        private bool _isChg;
        private bool _issaved;
        private DEBusinessItem _theItem;
        private Button btnAddBomItem;
        private Button btnAddBPM;
        private Button btnApp;
        private Button btnClose;
        private Button btnDelBomItem;
        private Button btnSaveBomItem;
        private Button btnToPrint;
        private Button btnToSent;
        private CheckBox chkFilter;
        private CheckBox chkUseHelp;
        private ContextMenuStrip cMenuDelRel;
        private IContainer components;
        private ObjectNavigateContext context;
        private ToolStripMenuItem D_DEL_TSD_BOM;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private Hashtable hsCols;
        private Hashtable hsColWide;
        private PropertyPageContent input;
        private Label label2;
        private Label label20;
        private Label label22;
        private Label label23;
        private ArrayList lstDocCode;
        private ArrayList lstOrder;
        private ArrayList lstUnits;
        private SortableListView lvwRleItems;
        private SortableListView lvwTsdBomEdit;
        private Panel panel1;
        private Panel panel2;
        private Panel pnlEditTsdBom;
        private ResWkInfo resWkTsd;
        private SplitContainer splitContainer1;
        private TabControl tbCtrlTsdBom;
        private TabPage tPTsBomDocId;
        private TabPage tPTsInfo;
        private ComboBox txtDocCode;
        private NumericUpDown txtNumFs;
        private NumericUpDown txtNumMtZs;
        private ComboBox txtUnit;
        private UclAttrs ucAttr;
        private UCNewItem ucNewItem;

        public UcTsdItem(string bpmName)
        {
            this._isChg = false;
            this._theItem = null;
            this.context = new ObjectNavigateContext();
            this.hsCols = null;
            this.input = null;
            this.lstDocCode = null;
            this.lstOrder = null;
            this.lstUnits = null;
            this.ucAttr = null;
            this.ucNewItem = null;
            this._bpmName = "";
            this.components = null;
            this.InitializeComponent();
            this._bNew = true;
            this.lstDocCode = new ArrayList();
            this.resWkTsd = new ResWkInfo();
            this.resWkTsd.Dock = DockStyle.Fill;
            this.panel2.Controls.Add(this.resWkTsd);
            this.SetBtnAndPnlStatue();
            this.InitUc();
            this.InitIvwRelItem();
            this.InitLvwBom();
            if (!string.IsNullOrEmpty(bpmName))
            {
                this._bpmName = bpmName;
            }
        }

        public UcTsdItem(DEBusinessItem item)
        {
            this._isChg = false;
            this._theItem = null;
            this.context = new ObjectNavigateContext();
            this.hsCols = null;
            this.input = null;
            this.lstDocCode = null;
            this.lstOrder = null;
            this.lstUnits = null;
            this.ucAttr = null;
            this.ucNewItem = null;
            this._bpmName = "";
            this.components = null;
            this.InitializeComponent();
            this._bNew = item == null;
            if (item != null)
            {
                PlArchivManage.GetRelListOfDEBizItem(item, ConstAm.TDSBOM_RELCLASS);
                this._theItem = item;
            }
            this.lstDocCode = new ArrayList();
            this.resWkTsd = new ResWkInfo();
            this.resWkTsd.Dock = DockStyle.Fill;
            this.panel2.Controls.Add(this.resWkTsd);
            this.SetBtnAndPnlStatue();
            this.InitUc();
            this.InitIvwRelItem();
            this.InitLvwBom();
        }

        private void AddTsdBom()
        {
            DERelationBizItem item = PlArchivManage.GetRelItemById(this.txtDocCode.Text, this._theItem, ConstAm.TDSBOM_RELCLASS);
            if (item == null)
            {
                ArrayList docClsById = PlArchivManage.Agent.GetDocClsById(this.txtDocCode.Text.ToUpper());
                if (docClsById.Count == 0)
                {
                    MessageBox.Show("输入的图纸代号在PLM中不存在！", "图纸代号不存在", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                if (docClsById.Count > 1)
                {
                    MessageBox.Show("输入的图号，在PLM中存在多个类型，无法通过本界面添加，请在”托晒单详细信息“中添加该文档。", "图纸代号类型", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                DEBusinessItem docItem = PLItem.Agent.GetBizItemByMaster(this.txtDocCode.Text.ToUpper(), docClsById[0].ToString(), 0, ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid, BizItemMode.BizItem) as DEBusinessItem;
                item = PlArchivManage.AddNewRelItem(docItem, ConstAm.TDSBOM_RELCLASS, this._theItem);
                this._theItem.Iteration.LinkRelationSet.GetRelationBizItemList(ConstAm.TDSBOM_RELCLASS).AddRelationItem(item);
            }
            item.Relation.SetAttrValue("MTZS", Convert.ToInt32(this.txtNumMtZs.Value));
            PlArchivManage.UpdateTsdDw(item, this.txtUnit.Text, Convert.ToInt32(this.txtNumFs.Value));
            this._isChg = true;
        }

        private void AfterReleased()
        {
            this._theItem = PLItem.Agent.GetBizItem(this._theItem.MasterOid, 0, 0, ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid, BizItemMode.BizItem) as DEBusinessItem;
            if (BizItemHandlerEvent.Instance.D_AfterIterationUpdated != null)
            {
                BizItemHandlerEvent.Instance.D_AfterIterationUpdated(BizOperationHelper.ConvertPLMBizItemDelegateParam(this._theItem));
            }
            if ((this._theItem.State == ItemState.CheckOut) && (this._theItem.Holder != ClientData.LogonUser.Oid))
            {
                this._bEdit = true;
            }
            else
            {
                this._bEdit = false;
            }
            this.ucAttr.CurItem = this._theItem;
            this.ucAttr.isEditable = this._bEdit;
            this.ucAttr.Display(true);
            this.SetBtnAndPnlStatue();
            this.RefreshBoms();
        }

        private void btnAddBomItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtDocCode.Text))
            {
                if (string.IsNullOrEmpty(this.txtUnit.Text) || (this.lvwRleItems.Items.Count == 0))
                {
                    return;
                }
                DialogResult result = MessageBox.Show("将要为当前托晒单的所有文档添加，发送单位(" + this.txtUnit.Text + ")\r\n 是: 已经存在该单位的的文档发放份数会被替换；\r\n 否: 不修改已经存在的文档的份数", "添加单位", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                {
                    return;
                }
                bool canUpdate = result == DialogResult.Yes;
                this.UpdateTsdBomAll(canUpdate);
            }
            else
            {
                if (this.DocAndUnitIsExists(this.txtDocCode.Text, this.txtUnit.Text))
                {
                    MessageBox.Show("要添加的数据已经存在");
                    return;
                }
                this.AddTsdBom();
            }
            this.RefreshTdsItemForm();
        }

        private void btnAddBPM_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.resWkTsd.ResValue))
            {
                if (this._bNew)
                {
                    Hashtable mapPropertiesToValue = new Hashtable();
                    mapPropertiesToValue.Add("WKFLINFO", this.resWkTsd.ResValue);
                    this.ucNewItem.SetProperties(mapPropertiesToValue);
                }
                else
                {
                    this.ucAttr.Save();
                    this.ucAttr.CurItem.Iteration.SetAttrValue("WKFLINFO", this.resWkTsd.Text);
                    this.ucAttr.Display(true);
                }
            }
        }

        private void btnApp_Click(object sender, EventArgs e)
        {
            if (!this._bNew && (this.btnApp.Text == "编  辑"))
            {
                this._theItem = PLItem.Agent.CheckOut(this._theItem.MasterOid, "DQDOSSIERPRINT", ClientData.LogonUser.Oid);
                PlArchivManage.GetRelListOfDEBizItem(this._theItem, ConstAm.TDSBOM_RELCLASS);
                if ((this._theItem.State != ItemState.CheckOut) || (this._theItem.Holder != ClientData.LogonUser.Oid))
                {
                    MessageBox.Show("无法检出，不能编辑", "无法编辑", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    if (BizItemHandlerEvent.Instance.D_AfterCheckOut != null)
                    {
                        BizItemHandlerEvent.Instance.D_AfterCheckOut(BizOperationHelper.ConvertPLMBizItemDelegateParam(this._theItem));
                    }
                    this.ucAttr.CurItem = this._theItem;
                    this.ucAttr.isEditable = true;
                    this.ucAttr.Display(true);
                    this.SetBtnAndPnlStatue();
                    this.RefreshBoms();
                }
            }
            else if (this.IsChange)
            {
                this.Save();
                if (!this._bNew)
                {
                    this.RefreshBoms();
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (this._bEdit)
            {
                if (this.IsChange && (MessageBox.Show("数据没有保存,是否保存后退出 ？", "未保存", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes))
                {
                    this.Save();
                }
                if (this._theItem != null)
                {
                    if (this._issaved)
                    {
                        this._theItem = PLItem.Agent.CheckIn(this._theItem.MasterOid, this._theItem.ClassName, ClientData.LogonUser.Oid, "托晒单编辑");
                        if (BizItemHandlerEvent.Instance.D_AfterCheckIn != null)
                        {
                            BizItemHandlerEvent.Instance.D_AfterCheckIn(BizOperationHelper.ConvertPLMBizItemDelegateParam(this._theItem));
                        }
                    }
                    else
                    {
                        this._theItem = PLItem.Agent.GetBizItem(this._theItem.MasterOid, 0, 0, ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid, BizItemMode.BizItem) as DEBusinessItem;
                        if (BizItemHandlerEvent.Instance.D_AfterIterationUpdated != null)
                        {
                            BizItemHandlerEvent.Instance.D_AfterIterationUpdated(BizOperationHelper.ConvertPLMBizItemDelegateParam(this._theItem));
                        }
                    }
                }
            }
            if (this._theItem == null)
            {
                DelegatesOfAm.Instance.D_AfterTsdTabClose(base.Parent);
            }
            else
            {
                DelegatesOfAm.Instance.D_AfterTsdTabClose(base.Parent);
            }
        }

        private void btnDelBomItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtDocCode.Text) || !string.IsNullOrEmpty(this.txtUnit.Text))
            {
                DEMetaAttribute relationAttribute = ModelContext.MetaModel.GetRelationAttribute(ConstAm.TDSBOM_RELCLASS, "JSDW");
                for (int i = 0; i < this.lvwTsdBomEdit.Items.Count; i++)
                {
                    ListViewItem item = this.lvwTsdBomEdit.Items[i];
                    DERelationBizItem tag = item.Tag as DERelationBizItem;
                    string id = tag.Id;
                    string text = item.SubItems[this.lvwTsdBomEdit.Columns[relationAttribute.Label].Index].Text;
                    if ((string.IsNullOrEmpty(this.txtDocCode.Text) || (id == this.txtDocCode.Text.ToUpper())) && (string.IsNullOrEmpty(this.txtUnit.Text) || (text == this.txtUnit.Text)))
                    {
                        PlArchivManage.DelteTsdDw(tag, this.txtUnit.Text);
                        this._isChg = true;
                    }
                }
                this.RefreshTdsItemForm();
            }
        }

        private void btnSaveBomItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtDocCode.Text) || !string.IsNullOrEmpty(this.txtUnit.Text))
            {
                DERelationBizItem tag;
                if (string.IsNullOrEmpty(this.txtDocCode.Text))
                {
                    foreach (ListViewItem item2 in this.lvwTsdBomEdit.Items)
                    {
                        tag = item2.Tag as DERelationBizItem;
                        if (PlArchivManage.GetTsdBomFs(tag, this.txtUnit.Text) > 0)
                        {
                            PlArchivManage.UpdateTsdDw(tag, this.txtUnit.Text, Convert.ToInt32(this.txtNumFs.Value));
                            this._isChg = true;
                        }
                    }
                }
                else if (string.IsNullOrEmpty(this.txtUnit.Text))
                {
                    ArrayList list = new ArrayList();
                    foreach (ListViewItem item2 in this.lvwTsdBomEdit.Items)
                    {
                        tag = item2.Tag as DERelationBizItem;
                        if (!list.Contains(tag.Id))
                        {
                            tag.Relation.SetAttrValue("MTZS", Convert.ToInt32(this.txtNumMtZs.Value));
                            this._isChg = true;
                            list.Add(tag.Id);
                        }
                    }
                }
                else
                {
                    tag = PlArchivManage.GetRelItemById(this.txtDocCode.Text.ToUpper(), this._theItem, ConstAm.TDSBOM_RELCLASS);
                    if (tag == null)
                    {
                        MessageBox.Show(this.txtDocCode.Text + "在当前托晒单的明细中不存在，无法修改！", "无法修改托晒明细", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return;
                    }
                    tag.Relation.SetAttrValue("MTZS", Convert.ToInt32(this.txtNumMtZs.Value));
                    PlArchivManage.UpdateTsdDw(tag, this.txtUnit.Text, Convert.ToInt32(this.txtNumFs.Value));
                    this._isChg = true;
                }
                this.RefreshTdsItemForm();
            }
        }

        private void btnToPrint_Click(object sender, EventArgs e)
        {
            StringBuilder builder;
            if (this.IsChange)
            {
                this.Save();
            }
            if (!PlArchivManage.CheckItemCanPrintOrSent(true, this._theItem, out builder))
            {
                FrmArchivManage.frmMian.DisplayTextInRichtBox("\r\n托晒单" + this._theItem.Id + "无法发送打印：\r\n\t" + builder.ToString(), 0, true);
            }
            else
            {
                try
                {
                    if ((this._theItem.State == ItemState.CheckOut) && (this._theItem.Holder == ClientData.LogonUser.Oid))
                    {
                        this._theItem = PLItem.Agent.CheckIn(this._theItem.MasterOid, this._theItem.ClassName, ClientData.LogonUser.Oid, "开始打印");
                    }
                    PlArchivManage.ToPrintOrSent(this._theItem, true, out builder);
                    if (builder.Length > 0)
                    {
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("\r\n托晒单" + this._theItem.Id + "发送打印失败：\r\n\t" + builder.ToString(), 0, true);
                    }
                    else
                    {
                        string attrValue = (this._theItem.Iteration.GetAttrValue("SM") == null) ? "" : this._theItem.Iteration.GetAttrValue("SM").ToString();
                        attrValue = attrValue + " " + ClientData.LogonUser.Name + ":发送打印";
                        this._theItem.Iteration.SetAttrValue("SM", attrValue);
                        this._theItem.Iteration = PLItem.UpdateItemIterationDirectly(this._theItem, ClientData.LogonUser.Oid, false);
                        if (BizItemHandlerEvent.Instance.D_AfterIterationUpdated != null)
                        {
                            BizItemHandlerEvent.Instance.D_AfterIterationUpdated(BizOperationHelper.ConvertPLMBizItemDelegateParam(this._theItem));
                        }
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("\r\n托晒单" + this._theItem.Id + "发送打印成功：\r\n\t", 1, true);
                    }
                }
                finally
                {
                    this.AfterReleased();
                }
            }
        }

        private void btnToSent_Click(object sender, EventArgs e)
        {
            StringBuilder builder;
            if (this.IsChange)
            {
                this.Save();
            }
            if (!PlArchivManage.CheckItemCanPrintOrSent(false, this._theItem, out builder))
            {
                FrmArchivManage.frmMian.DisplayTextInRichtBox("托晒单" + this._theItem.Id + "无法直接回收：\r\n\t" + builder.ToString(), 0, true);
            }
            else
            {
                try
                {
                    if ((this._theItem.State == ItemState.CheckOut) && (this._theItem.Holder == ClientData.LogonUser.Oid))
                    {
                        this._theItem = PLItem.Agent.CheckIn(this._theItem.MasterOid, this._theItem.ClassName, ClientData.LogonUser.Oid, "");
                    }
                    PlArchivManage.ToPrintOrSent(this._theItem, false, out builder);
                    if (builder.Length > 0)
                    {
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("托晒单" + this._theItem.Id + "直接回收失败：\r\n\t" + builder.ToString(), 0, true);
                    }
                    else
                    {
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("托晒单" + this._theItem.Id + "直接回收成功\r\n", 1, true);
                    }
                }
                finally
                {
                    this.AfterReleased();
                }
            }
        }

        private void chkFilter_CheckedChanged(object sender, EventArgs e)
        {
            this.RefreshEditBoms();
        }

        private void D_DEL_TSD_BOM_Click(object sender, EventArgs e)
        {
            DERelationBizItemList relationBizItemList = this._theItem.Iteration.LinkRelationSet.GetRelationBizItemList(ConstAm.TDSBOM_RELCLASS);
            if (relationBizItemList == null)
            {
                relationBizItemList = new DERelationBizItemList(ConstAm.TDSBOM_RELCLASS);
                this._theItem.Iteration.LinkRelationSet.AddRelationList(ConstAm.TDSBOM_RELCLASS, relationBizItemList);
            }
            ArrayList list2 = new ArrayList(this.lvwRleItems.SelectedItems);
            for (int i = 0; i < list2.Count; i++)
            {
                ListViewItem item = list2[i] as ListViewItem;
                DERelationBizItem tag = item.Tag as DERelationBizItem;
                relationBizItemList.DeleteLinkRelation(tag.MasterOid);
                this._isChg = true;
            }
            this.RefreshBoms();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DocAndUnitIsExists(string id, string unit)
        {
            foreach (DERelationBizItem item in this.GetCurRelItems)
            {
                if (!string.IsNullOrEmpty(id) && (item.Id == id.ToUpper()))
                {
                    if (!string.IsNullOrEmpty(unit))
                    {
                        string str = (item.Relation.GetAttrValue("JSDW") == null) ? "" : item.Relation.GetAttrValue("JSDW").ToString();
                        if (str.IndexOf(unit + "(") != -1)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            return false;
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.tbCtrlTsdBom = new TabControl();
            this.tPTsInfo = new TabPage();
            this.splitContainer1 = new SplitContainer();
            this.groupBox3 = new GroupBox();
            this.panel1 = new Panel();
            this.btnAddBPM = new Button();
            this.btnClose = new Button();
            this.btnToSent = new Button();
            this.btnApp = new Button();
            this.btnToPrint = new Button();
            this.lvwRleItems = new SortableListView();
            this.tPTsBomDocId = new TabPage();
            this.lvwTsdBomEdit = new SortableListView();
            this.pnlEditTsdBom = new Panel();
            this.chkUseHelp = new CheckBox();
            this.chkFilter = new CheckBox();
            this.groupBox2 = new GroupBox();
            this.label22 = new Label();
            this.txtUnit = new ComboBox();
            this.txtNumFs = new NumericUpDown();
            this.label23 = new Label();
            this.groupBox1 = new GroupBox();
            this.txtNumMtZs = new NumericUpDown();
            this.label2 = new Label();
            this.label20 = new Label();
            this.txtDocCode = new ComboBox();
            this.btnDelBomItem = new Button();
            this.btnSaveBomItem = new Button();
            this.btnAddBomItem = new Button();
            this.cMenuDelRel = new ContextMenuStrip(this.components);
            this.D_DEL_TSD_BOM = new ToolStripMenuItem();
            this.panel2 = new Panel();
            this.tbCtrlTsdBom.SuspendLayout();
            this.tPTsInfo.SuspendLayout();
            this.splitContainer1.BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tPTsBomDocId.SuspendLayout();
            this.pnlEditTsdBom.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.txtNumFs.BeginInit();
            this.groupBox1.SuspendLayout();
            this.txtNumMtZs.BeginInit();
            this.cMenuDelRel.SuspendLayout();
            base.SuspendLayout();
            this.tbCtrlTsdBom.Controls.Add(this.tPTsInfo);
            this.tbCtrlTsdBom.Controls.Add(this.tPTsBomDocId);
            this.tbCtrlTsdBom.Dock = DockStyle.Fill;
            this.tbCtrlTsdBom.Location = new Point(0, 0);
            this.tbCtrlTsdBom.Name = "tbCtrlTsdBom";
            this.tbCtrlTsdBom.SelectedIndex = 0;
            this.tbCtrlTsdBom.Size = new Size(0x30d, 0x1a9);
            this.tbCtrlTsdBom.TabIndex = 1;
            this.tPTsInfo.Controls.Add(this.splitContainer1);
            this.tPTsInfo.Location = new Point(4, 0x16);
            this.tPTsInfo.Name = "tPTsInfo";
            this.tPTsInfo.Padding = new Padding(3);
            this.tPTsInfo.Size = new Size(0x305, 0x18f);
            this.tPTsInfo.TabIndex = 0;
            this.tPTsInfo.Text = "托晒单详细信息";
            this.tPTsInfo.UseVisualStyleBackColor = true;
            this.splitContainer1.Dock = DockStyle.Fill;
            this.splitContainer1.Location = new Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = Orientation.Horizontal;
            this.splitContainer1.Panel1.Controls.Add(this.groupBox3);
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            this.splitContainer1.Panel2.Controls.Add(this.lvwRleItems);
            this.splitContainer1.Size = new Size(0x2ff, 0x189);
            this.splitContainer1.SplitterDistance = 0x11e;
            this.splitContainer1.TabIndex = 0;
            this.groupBox3.BackColor = Color.WhiteSmoke;
            this.groupBox3.Dock = DockStyle.Fill;
            this.groupBox3.Location = new Point(0, 0x2e);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new Size(0x2ff, 240);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.panel1.BackColor = Color.WhiteSmoke;
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.btnAddBPM);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Controls.Add(this.btnToSent);
            this.panel1.Controls.Add(this.btnApp);
            this.panel1.Controls.Add(this.btnToPrint);
            this.panel1.Dock = DockStyle.Top;
            this.panel1.Location = new Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0x2ff, 0x2e);
            this.panel1.TabIndex = 0;
            this.btnAddBPM.Anchor = AnchorStyles.Right;
            this.btnAddBPM.Location = new Point(290, 11);
            this.btnAddBPM.Name = "btnAddBPM";
            this.btnAddBPM.Size = new Size(0x4b, 0x17);
            this.btnAddBPM.TabIndex = 0x20;
            this.btnAddBPM.Text = "添加流程";
            this.btnAddBPM.UseVisualStyleBackColor = true;
            this.btnAddBPM.Click += new EventHandler(this.btnAddBPM_Click);
            this.btnClose.Anchor = AnchorStyles.Right;
            this.btnClose.Location = new Point(0x2ab, 10);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(0x4b, 0x17);
            this.btnClose.TabIndex = 0x1d;
            this.btnClose.Text = "关  闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            this.btnToSent.Anchor = AnchorStyles.Right;
            this.btnToSent.Location = new Point(440, 10);
            this.btnToSent.Name = "btnToSent";
            this.btnToSent.Size = new Size(0x4b, 0x17);
            this.btnToSent.TabIndex = 30;
            this.btnToSent.Text = "回  收";
            this.btnToSent.UseVisualStyleBackColor = true;
            this.btnToSent.Click += new EventHandler(this.btnToSent_Click);
            this.btnApp.Anchor = AnchorStyles.Right;
            this.btnApp.Location = new Point(0x25a, 10);
            this.btnApp.Name = "btnApp";
            this.btnApp.Size = new Size(0x4b, 0x17);
            this.btnApp.TabIndex = 0x1c;
            this.btnApp.Text = "应  用";
            this.btnApp.UseVisualStyleBackColor = true;
            this.btnApp.Click += new EventHandler(this.btnApp_Click);
            this.btnToPrint.Anchor = AnchorStyles.Right;
            this.btnToPrint.Location = new Point(0x209, 10);
            this.btnToPrint.Name = "btnToPrint";
            this.btnToPrint.Size = new Size(0x4b, 0x17);
            this.btnToPrint.TabIndex = 0x1b;
            this.btnToPrint.Text = "发送打印";
            this.btnToPrint.UseVisualStyleBackColor = true;
            this.btnToPrint.Click += new EventHandler(this.btnToPrint_Click);
            this.lvwRleItems.AllowDrop = true;
            this.lvwRleItems.Dock = DockStyle.Fill;
            this.lvwRleItems.FullRowSelect = true;
            this.lvwRleItems.HideSelection = false;
            this.lvwRleItems.Location = new Point(0, 0);
            this.lvwRleItems.Name = "lvwRleItems";
            this.lvwRleItems.Size = new Size(0x2ff, 0x67);
            this.lvwRleItems.SortingOrder = SortOrder.None;
            this.lvwRleItems.TabIndex = 0;
            this.lvwRleItems.UseCompatibleStateImageBehavior = false;
            this.lvwRleItems.View = View.Details;
            this.lvwRleItems.DragDrop += new DragEventHandler(this.lvwRleItems_DragDrop);
            this.lvwRleItems.DragEnter += new DragEventHandler(this.lvwRleItems_DragEnter);
            this.lvwRleItems.DoubleClick += new EventHandler(this.lvwRleItems_DoubleClick);
            this.lvwRleItems.MouseUp += new MouseEventHandler(this.lvwRleItems_MouseUp);
            this.tPTsBomDocId.BackColor = Color.WhiteSmoke;
            this.tPTsBomDocId.Controls.Add(this.lvwTsdBomEdit);
            this.tPTsBomDocId.Controls.Add(this.pnlEditTsdBom);
            this.tPTsBomDocId.Location = new Point(4, 0x16);
            this.tPTsBomDocId.Name = "tPTsBomDocId";
            this.tPTsBomDocId.Padding = new Padding(3);
            this.tPTsBomDocId.Size = new Size(0x305, 0x18f);
            this.tPTsBomDocId.TabIndex = 1;
            this.tPTsBomDocId.Text = "图号托晒明细";
            this.lvwTsdBomEdit.Dock = DockStyle.Fill;
            this.lvwTsdBomEdit.FullRowSelect = true;
            this.lvwTsdBomEdit.HideSelection = false;
            this.lvwTsdBomEdit.Location = new Point(3, 3);
            this.lvwTsdBomEdit.Name = "lvwTsdBomEdit";
            this.lvwTsdBomEdit.Size = new Size(0x2ff, 0x132);
            this.lvwTsdBomEdit.SortingOrder = SortOrder.None;
            this.lvwTsdBomEdit.TabIndex = 4;
            this.lvwTsdBomEdit.UseCompatibleStateImageBehavior = false;
            this.lvwTsdBomEdit.View = View.Details;
            this.lvwTsdBomEdit.SelectedIndexChanged += new EventHandler(this.lvwTsdBomEdit_SelectedIndexChanged);
            this.pnlEditTsdBom.Controls.Add(this.chkUseHelp);
            this.pnlEditTsdBom.Controls.Add(this.chkFilter);
            this.pnlEditTsdBom.Controls.Add(this.groupBox2);
            this.pnlEditTsdBom.Controls.Add(this.groupBox1);
            this.pnlEditTsdBom.Controls.Add(this.btnDelBomItem);
            this.pnlEditTsdBom.Controls.Add(this.btnSaveBomItem);
            this.pnlEditTsdBom.Controls.Add(this.btnAddBomItem);
            this.pnlEditTsdBom.Dock = DockStyle.Bottom;
            this.pnlEditTsdBom.Location = new Point(3, 0x135);
            this.pnlEditTsdBom.Name = "pnlEditTsdBom";
            this.pnlEditTsdBom.Size = new Size(0x2ff, 0x57);
            this.pnlEditTsdBom.TabIndex = 3;
            this.chkUseHelp.AutoSize = true;
            this.chkUseHelp.Location = new Point(0x1ad, 0x35);
            this.chkUseHelp.Name = "chkUseHelp";
            this.chkUseHelp.Size = new Size(0x54, 0x10);
            this.chkUseHelp.TabIndex = 0x17;
            this.chkUseHelp.Text = "二次图数量";
            this.chkUseHelp.UseVisualStyleBackColor = true;
            this.chkFilter.AutoSize = true;
            this.chkFilter.Location = new Point(0x1ad, 0x1a);
            this.chkFilter.Name = "chkFilter";
            this.chkFilter.Size = new Size(0x30, 0x10);
            this.chkFilter.TabIndex = 0x16;
            this.chkFilter.Text = "过滤";
            this.chkFilter.UseVisualStyleBackColor = true;
            this.chkFilter.CheckedChanged += new EventHandler(this.chkFilter_CheckedChanged);
            this.groupBox2.Controls.Add(this.label22);
            this.groupBox2.Controls.Add(this.txtUnit);
            this.groupBox2.Controls.Add(this.txtNumFs);
            this.groupBox2.Controls.Add(this.label23);
            this.groupBox2.Location = new Point(0xe1, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new Size(0xbf, 0x51);
            this.groupBox2.TabIndex = 0x15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "按单位编辑";
            this.label22.AutoSize = true;
            this.label22.Location = new Point(6, 0x15);
            this.label22.Name = "label22";
            this.label22.Size = new Size(0x1d, 12);
            this.label22.TabIndex = 4;
            this.label22.Text = "单位";
            this.txtUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            this.txtUnit.FormattingEnabled = true;
            this.txtUnit.Location = new Point(0x29, 0x15);
            this.txtUnit.Name = "txtUnit";
            this.txtUnit.Size = new Size(0x85, 20);
            this.txtUnit.TabIndex = 14;
            this.txtUnit.SelectedIndexChanged += new EventHandler(this.txtUnit_SelectedIndexChanged);
            this.txtNumFs.Location = new Point(0x29, 50);
            int[] bitsTmp = new int[4];
            bitsTmp[0] = 0x3e8;
            this.txtNumFs.Maximum = new decimal(bitsTmp);
            this.txtNumFs.Name = "txtNumFs";
            this.txtNumFs.Size = new Size(0x85, 0x15);
            this.txtNumFs.TabIndex = 8;
            int[] bitsTmp1 = new int[4];
            bitsTmp1[0] = 1;
            this.txtNumFs.Value = new decimal(bitsTmp1);
            this.label23.AutoSize = true;
            this.label23.Location = new Point(7, 0x38);
            this.label23.Name = "label23";
            this.label23.Size = new Size(0x1d, 12);
            this.label23.TabIndex = 6;
            this.label23.Text = "份数";
            this.groupBox1.Controls.Add(this.txtNumMtZs);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label20);
            this.groupBox1.Controls.Add(this.txtDocCode);
            this.groupBox1.Dock = DockStyle.Left;
            this.groupBox1.Location = new Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(0xdb, 0x57);
            this.groupBox1.TabIndex = 0x13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "托晒明细属性";
            this.txtNumMtZs.Location = new Point(0x3d, 0x34);
            int[] bitsTmp2 = new int[4];
            bitsTmp2[0] = 0x3e8;
            this.txtNumMtZs.Maximum = new decimal(bitsTmp2);
            this.txtNumMtZs.Name = "txtNumMtZs";
            this.txtNumMtZs.Size = new Size(0x8e, 0x15);
            this.txtNumMtZs.TabIndex = 0x15;
            int[] bitsTmp3 = new int[4];
            bitsTmp3[0] = 1;
            this.txtNumMtZs.Value = new decimal(bitsTmp3);
            this.label2.AutoSize = true;
            this.label2.Location = new Point(9, 0x39);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x35, 12);
            this.label2.TabIndex = 20;
            this.label2.Text = "每份张数";
            this.label20.AutoSize = true;
            this.label20.Location = new Point(6, 0x18);
            this.label20.Name = "label20";
            this.label20.Size = new Size(0x1d, 12);
            this.label20.TabIndex = 0;
            this.label20.Text = "图号";
            this.txtDocCode.FormattingEnabled = true;
            this.txtDocCode.Location = new Point(0x3d, 0x18);
            this.txtDocCode.Name = "txtDocCode";
            this.txtDocCode.Size = new Size(0x8e, 20);
            this.txtDocCode.TabIndex = 13;
            this.txtDocCode.SelectedIndexChanged += new EventHandler(this.txtDocCode_SelectedIndexChanged);
            this.btnDelBomItem.Anchor = AnchorStyles.Right;
            this.btnDelBomItem.Location = new Point(0x2a7, 0x40);
            this.btnDelBomItem.Name = "btnDelBomItem";
            this.btnDelBomItem.Size = new Size(0x4b, 0x17);
            this.btnDelBomItem.TabIndex = 0x12;
            this.btnDelBomItem.Text = "删除";
            this.btnDelBomItem.UseVisualStyleBackColor = true;
            this.btnDelBomItem.Click += new EventHandler(this.btnDelBomItem_Click);
            this.btnSaveBomItem.Anchor = AnchorStyles.Right;
            this.btnSaveBomItem.Location = new Point(0x2a7, 0x23);
            this.btnSaveBomItem.Name = "btnSaveBomItem";
            this.btnSaveBomItem.Size = new Size(0x4b, 0x17);
            this.btnSaveBomItem.TabIndex = 0x11;
            this.btnSaveBomItem.Text = "修改";
            this.btnSaveBomItem.UseVisualStyleBackColor = true;
            this.btnSaveBomItem.Click += new EventHandler(this.btnSaveBomItem_Click);
            this.btnAddBomItem.Anchor = AnchorStyles.Right;
            this.btnAddBomItem.Location = new Point(0x2a7, 6);
            this.btnAddBomItem.Name = "btnAddBomItem";
            this.btnAddBomItem.Size = new Size(0x4b, 0x17);
            this.btnAddBomItem.TabIndex = 0x10;
            this.btnAddBomItem.Text = "添加";
            this.btnAddBomItem.UseVisualStyleBackColor = true;
            this.btnAddBomItem.Click += new EventHandler(this.btnAddBomItem_Click);
            this.cMenuDelRel.Items.AddRange(new ToolStripItem[] { this.D_DEL_TSD_BOM });
            this.cMenuDelRel.Name = "cMenuDelRel";
            this.cMenuDelRel.Size = new Size(0x7d, 0x1a);
            this.D_DEL_TSD_BOM.Name = "D_DEL_TSD_BOM";
            this.D_DEL_TSD_BOM.Size = new Size(0x7c, 0x16);
            this.D_DEL_TSD_BOM.Text = "移除文档";
            this.D_DEL_TSD_BOM.Click += new EventHandler(this.D_DEL_TSD_BOM_Click);
            this.panel2.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this.panel2.Location = new Point(13, 8);
            this.panel2.Name = "panel2";
            this.panel2.Size = new Size(0x10f, 0x20);
            this.panel2.TabIndex = 0x22;
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            base.Controls.Add(this.tbCtrlTsdBom);
            base.Name = "UcTsdItem";
            base.Size = new Size(0x30d, 0x1a9);
            base.Load += new EventHandler(this.UcTsdItem_Load);
            this.tbCtrlTsdBom.ResumeLayout(false);
            this.tPTsInfo.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tPTsBomDocId.ResumeLayout(false);
            this.pnlEditTsdBom.ResumeLayout(false);
            this.pnlEditTsdBom.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.txtNumFs.EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.txtNumMtZs.EndInit();
            this.cMenuDelRel.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void InitIvwRelItem()
        {
            if (!this._bNew)
            {
                this.hsCols = PlArchivManage.Agent.GetViewOfCol("托晒单明细", out this.lstOrder, out this.hsColWide);
                PlArchivManage.SetCol(this.hsCols, this.lvwRleItems, "TSDBom", this.lstOrder, this.hsColWide);
                PlArchivManage.SetCol(this.hsCols, this.lvwTsdBomEdit, "TSDBom", this.lstOrder, this.hsColWide);
                this.RefreshBoms();
            }
        }

        private void InitLvwBom()
        {
            if (!this._bNew)
            {
                this.lstUnits = PlArchivManage.GetTsRes("路线部门") as ArrayList;
                for (int i = 0; i < this.lstUnits.Count; i++)
                {
                    string item = this.lstUnits[i].ToString();
                    this.txtUnit.Items.Add(item);
                }
                this.txtUnit.Items.Insert(0, "");
                if (!this.IsYct)
                {
                    this.chkUseHelp.Checked = true;
                }
            }
        }

        private void InitUc()
        {
            if (this._bNew)
            {
                this.splitContainer1.SplitterDistance = this.splitContainer1.Height;
                PropertyPageContent input = new PropertyPageContent(ModelContext.MetaModel.GetClassEx("DQDOSSIERPRINT"), null, ClientData.UserGlobalOption, null, this, null, PropertyPageMode.SINGLE);
                this.ucNewItem = new UCNewItem(false, true, false);
                this.ucNewItem.CreatedItem = null;
                this.ucNewItem.IsInSelfDefinePage = true;
                this.ucNewItem.SetInput(input);
                this.ucNewItem.Dock = DockStyle.Fill;
                this.groupBox3.Controls.Add(this.ucNewItem);
                this.tbCtrlTsdBom.TabPages.Remove(this.tPTsBomDocId);
            }
            else
            {
                this.ucAttr = new UclAttrs();
                this.ucAttr.Dock = DockStyle.Fill;
                this.groupBox3.Controls.Add(this.ucAttr);
                this.context.Option = ClientData.UserGlobalOption;
                this.ucAttr.SetContext(this.context);
                this.ucAttr.CurMeta = ModelContext.MetaModel.GetClassEx(this._theItem.ClassName);
                this.ucAttr.CurItem = this._theItem;
                this.ucAttr.isEditable = this._bEdit;
                this.ucAttr.Display(true);
            }
        }

        private void lvwRleItems_DoubleClick(object sender, EventArgs e)
        {
            if (this.lvwRleItems.SelectedItems.Count > 0)
            {
                ListViewItem item = this.lvwRleItems.SelectedItems[0];
                DEBusinessItem tag = null;
                if (item.Tag is DEBusinessItem)
                {
                    tag = (DEBusinessItem) item.Tag;
                }
                else
                {
                    if (!(item.Tag is DERelationBizItem))
                    {
                        return;
                    }
                    tag = ((DERelationBizItem) item.Tag).BizItem;
                }
                List<IBizItem> items = new List<IBizItem> {
                    tag
                };
                PLMOperationArgs args = new PLMOperationArgs(FrmLogon.PLMProduct.ToString(), PLMLocation.ItemList.ToString(), items, ClientData.UserGlobalOption);
                BizOperationHelper.DefaultOpen(this, args);
            }
        }

        private void lvwRleItems_DragDrop(object sender, DragEventArgs e)
        {
            if ((e.Data != null) && this._bEdit)
            {
                DEBusinessItem item;
                ArrayList list = new ArrayList();
                ArrayList list2 = new ArrayList();
                foreach (DERelationBizItem item2 in this.GetCurRelItems)
                {
                    if (!list2.Contains(item2.MasterOid))
                    {
                        list2.Add(item2.MasterOid);
                    }
                }
                if (e.Data.GetDataPresent(typeof(CLCopyData)))
                {
                    CLCopyData data = (CLCopyData) e.Data.GetData(typeof(CLCopyData));
                    foreach (object obj2 in data)
                    {
                        item = PlArchivManage.GetItem(obj2);
                        if (ModelContext.MetaModel.IsChild("DOC", item.Master.ClassName) && ((item != null) && !list2.Contains(item.MasterOid)))
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
                        if (!ModelContext.MetaModel.IsChild("DOC", item.Master.ClassName))
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
                    foreach (DEBusinessItem item3 in list)
                    {
                        DERelationBizItem dItem = PlArchivManage.AddNewRelItem(item3, ConstAm.TDSBOM_RELCLASS, this._theItem);
                        PlArchivManage.AddLvwRelValues(this.lvwRleItems, this.lstOrder, dItem, "");
                        this._isChg = true;
                    }
                    this.RefreshBoms();
                }
            }
        }

        private void lvwRleItems_DragEnter(object sender, DragEventArgs e)
        {
            if (this._bEdit)
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
        }

        private void lvwRleItems_MouseUp(object sender, MouseEventArgs e)
        {
            if (((this._bEdit && (e.Button == MouseButtons.Right)) && (e.Clicks == 1)) && (this.lvwRleItems.GetItemAt(e.X, e.Y) != null))
            {
                this.cMenuDelRel.Show(this.lvwRleItems, e.X, e.Y);
            }
        }

        private void lvwTsdBomEdit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.lvwTsdBomEdit.SelectedItems.Count == 1)
            {
                ListViewItem item = this.lvwTsdBomEdit.SelectedItems[0];
                DERelationBizItem tag = item.Tag as DERelationBizItem;
                this.txtDocCode.Text = tag.Id;
                object attrValue = tag.Relation.GetAttrValue("MTZS");
                this.txtNumMtZs.Value = (attrValue == null) ? 1 : Convert.ToInt32(attrValue);
                DEMetaAttribute relationAttribute = ModelContext.MetaModel.GetRelationAttribute(ConstAm.TDSBOM_RELCLASS, "FS");
                string text = item.SubItems[this.lvwTsdBomEdit.Columns[relationAttribute.Label].Index].Text;
                if (!string.IsNullOrEmpty(text))
                {
                    this.txtNumFs.Value = Convert.ToInt32(text);
                }
                else
                {
                    this.txtNumFs.Value = 1M;
                }
                relationAttribute = ModelContext.MetaModel.GetRelationAttribute(ConstAm.TDSBOM_RELCLASS, "JSDW");
                string str2 = item.SubItems[this.lvwTsdBomEdit.Columns[relationAttribute.Label].Index].Text;
                if (!string.IsNullOrEmpty(str2))
                {
                    this.txtUnit.Text = str2;
                }
            }
        }

        private void RefreshBoms()
        {
            if (!this._bNew)
            {
                DERelationBizItemList relListOfDEBizItem = PlArchivManage.GetRelListOfDEBizItem(this._theItem, ConstAm.TDSBOM_RELCLASS);
                this.lvwRleItems.Items.Clear();
                foreach (DERelationBizItem item in relListOfDEBizItem.RelationBizItems)
                {
                    if (item.Relation.State != RelationState.Deleted)
                    {
                        PlArchivManage.AddLvwRelValues(this.lvwRleItems, this.lstOrder, item, "");
                    }
                }
                this.RefreshEditBoms();
            }
        }

        private void RefreshEditBoms()
        {
            DERelationBizItemList relListOfDEBizItem = PlArchivManage.GetRelListOfDEBizItem(this._theItem, ConstAm.TDSBOM_RELCLASS);
            this.lvwTsdBomEdit.Items.Clear();
            foreach (DERelationBizItem item in relListOfDEBizItem.RelationBizItems)
            {
                if ((item.Relation.State != RelationState.Deleted) && ((string.IsNullOrEmpty(this.txtDocCode.Text) || !this.chkFilter.Checked) || (item.Id == this.txtDocCode.Text.ToUpper())))
                {
                    PlArchivManage.UpdateLvwRelValues(this.lvwTsdBomEdit, this.lstOrder, item, this.chkFilter.Checked ? this.txtUnit.Text : "");
                }
            }
            this.ResetDocCode();
        }

        private void RefreshTdsItemForm()
        {
            if (this._isChg)
            {
                foreach (DERelationBizItem item in this.GetCurRelItems)
                {
                    PlArchivManage.ResetZSofTdsBom(item);
                }
                this.RefreshBoms();
                this.RefreshEditBoms();
            }
        }

        private void ResetDocCode()
        {
            this.lstDocCode.Clear();
            foreach (DERelationBizItem item in this.GetCurRelItems)
            {
                if (!this.lstDocCode.Contains(item.Id))
                {
                    this.lstDocCode.Add(item.Id);
                }
            }
            this.lstDocCode.Sort();
            this.txtDocCode.Items.Clear();
            for (int i = 0; i < this.lstDocCode.Count; i++)
            {
                string str = this.lstDocCode[i].ToString();
                this.txtDocCode.Items.Add(str);
            }
            this.txtDocCode.Items.Insert(0, "");
        }

        private void Save()
        {
            if (this._bNew)
            {
                this._theItem = this.CurItem;
                if (this._theItem != null)
                {
                    this._theItem = PLItem.Agent.CreateItem(this._theItem, null, ClientData.LogonUser.Oid);
                    DelegatesOfAm.Instance.D_AfterTsdCreate(this._theItem);
                }
            }
            else if (this.IsChange)
            {
                if (this.ucAttr.IsChanged)
                {
                    this.ucAttr.Save();
                }
                PlArchivManage.ResetZsAndFsOfTsd(this._theItem);
                this._theItem.Iteration = PLItem.UpdateItemIteration(this._theItem.Iteration, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption);
                PlArchivManage.GetRelListOfDEBizItem(this._theItem, ConstAm.TDSBOM_RELCLASS);
                this.ucAttr.Display(true);
                if (BizItemHandlerEvent.Instance.D_AfterIterationUpdated != null)
                {
                    BizItemHandlerEvent.Instance.D_AfterIterationUpdated(BizOperationHelper.ConvertPLMBizItemDelegateParam(this._theItem));
                }
                this._isChg = false;
                this._issaved = true;
            }
        }

        private void SetBtnAndPnlStatue()
        {
            if (this._theItem == null)
            {
                this._bEdit = true;
            }
            else if ((this._theItem.State == ItemState.CheckOut) && (this._theItem.Holder == ClientData.LogonUser.Oid))
            {
                this._bEdit = true;
            }
            else if (this._theItem.State == ItemState.CheckIn)
            {
                //StringBuilder c;
                //object attrValue = this._theItem.Iteration.GetAttrValue("TSSTATUS");
                if (/*(attrValue != null && attrValue.ToString() != "开始打印")&&!PlArchivManage.CheckItemCanPrintOrSent(true, this._theItem, out c) &&*/ PLGrantPerm.CanDoClassOperation(ClientData.LogonUser.Oid, "DQDOSSIERPRINT", Guid.Empty, "ClaRel_EDIT") == 1)
                {
                    this.btnApp.Text = "编  辑";
                    this._bEdit = false;
                }
            }
            else
            {
                this._bEdit = false;
            }
            this.pnlEditTsdBom.Enabled = this._bEdit;
            if (!(this._bEdit || !(this.btnApp.Text == "编  辑")))
            {
                this.btnApp.Enabled = true;
            }
            else
            {
                this.btnApp.Enabled = this._bNew || this._bEdit;
                this.btnApp.Text = "应  用";
            }
            this.btnAddBPM.Enabled = this.resWkTsd.Enabled = this._bEdit;
            this.btnToSent.Enabled = this.btnToPrint.Enabled = this._bEdit && !this._bNew;
        }

        private void txtDocCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.chkFilter.Checked)
            {
                this.RefreshEditBoms();
            }
        }

        private void txtUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.chkFilter.Checked)
            {
                this.RefreshEditBoms();
            }
            if (/*this.chkUseHelp.Checked &&*/ !string.IsNullOrEmpty(this.txtUnit.Text))
            {
                object attrValue = this._theItem.Iteration.GetAttrValue("YCT");
                string ftly = "";
                //if ((attrValue == null) || (attrValue.ToString() != "一次图"))
                {
                    attrValue = this._theItem.Iteration.GetAttrValue("FTLX");
                    if (attrValue != null)
                    {
                        ftly = attrValue.ToString();
                    }
                    int sedNum = PlArchivManage.GetSedNum(this.txtUnit.Text, ftly);
                    this.txtNumFs.Value = sedNum;
                }
            }
        }

        private void UcTsdItem_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this._bpmName))
            {
                this.resWkTsd.ResValue = this._bpmName;
            }
        }

        private void UpdateTsdBomAll(bool canUpdate)
        {
            foreach (DERelationBizItem item in this.GetCurRelItems)
            {
                int tsdBomFs = PlArchivManage.GetTsdBomFs(item, this.txtUnit.Text);
                if (canUpdate || (tsdBomFs == 0))
                {
                    PlArchivManage.UpdateTsdDw(item, this.txtUnit.Text, Convert.ToInt32(this.txtNumFs.Value));
                    this._isChg = true;
                }
            }
        }

        public DEBusinessItem CurItem
        {
            get
            {
                if (this.groupBox3.Controls.Count > 0)
                {
                    UCNewItem item = this.groupBox3.Controls[0] as UCNewItem;
                    if (item != null)
                    {
                        if (item.IsChanged)
                        {
                            item.Save();
                        }
                        return item.CreatedItem;
                    }
                    UclAttrs attrs = this.groupBox3.Controls[0] as UclAttrs;
                    if (attrs != null)
                    {
                        if (attrs.isEditable && attrs.IsChanged)
                        {
                            attrs.Save();
                        }
                        return attrs.CurItem;
                    }
                }
                return this._theItem;
            }
            set
            {
                this._theItem = value;
            }
        }

        private ArrayList GetCurRelItems
        {
            get
            {
                return PlArchivManage.GetRelListOfDEBizItem(this._theItem, ConstAm.TDSBOM_RELCLASS).RelationBizItems;
            }
        }

        private bool IsChange
        {
            get
            {
                if (!this._bEdit)
                {
                    return false;
                }
                return ((this._bNew && this.ucNewItem.IsChanged) || (this.ucAttr.IsChanged || this._isChg));
            }
        }

        private bool IsYct
        {
            get
            {
                if (this._theItem == null)
                {
                    return false;
                }
                if (this._bNew)
                {
                    return false;
                }
                if (this.ucAttr.IsChanged)
                {
                    this.ucAttr.Save();
                }
                object attrValue = this._theItem.Iteration.GetAttrValue("YCT");
                return ((attrValue == null) || (attrValue.ToString() == "一次图"));
            }
        }
    }
}

