namespace DQArchivManageClt
{
    using DQArchivManageCommon;
    using System;
    using System.Collections;
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

    public class UcSentItem : UserControl
    {
        private bool _bEdit = false;
        private bool _isChg = false;
        private bool _issaved = false;
        private DEBusinessItem _theItem = null;
        private Button btnApp;
        private Button btnAppHSBom;
        private Button btnCancelFF;
        private Button btnCancelHS;
        private Button btnCloseSent;
        private Button btnEndFF;
        private Button btnEndHS;
        private Button btnEndSent;
        private Button btnSaveFFBom;
        private CheckBox chkFilterFF;
        private CheckBox chkFilterHS;
        private IContainer components = null;
        private GroupBox grpSentattr;
        private Hashtable hsCols1 = null;
        private Hashtable hsCols2 = null;
        private Hashtable hsColWide1;
        private Hashtable hsColWide2;
        private Label label1;
        private Label label10;
        private Label label11;
        private Label label12;
        private Label label13;
        private Label label14;
        private Label label15;
        private Label label16;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private ArrayList lstOrder1 = null;
        private ArrayList lstOrder2 = null;
        private SortableListView lvwRecycleBom;
        private SortableListView lvwSentBom;
        private Panel panel1;
        private Panel panel3;
        private Panel pnlFF;
        private Panel pnlHS;
        private TabControl tCtrlSentBom;
        private TabPage tpRecyclebom;
        private TabPage tpSentBom;
        private TabPage tPSentInfo;
        private string tstype;
        private ComboBox txtDocCode;
        private ComboBox txtDocCodeR;
        private TextBox txtDocName;
        private TextBox txtDocNameR;
        private TextBox txtDocRev;
        private TextBox txtDocRevR;
        private TextBox txtFFSM;
        private TextBox txtHsSm;
        private NumericUpDown txtNumMtZs;
        private NumericUpDown txtNumRealFS;
        private NumericUpDown txtNumRealFsR;
        private NumericUpDown txtNumZsR;
        private ComboBox txtSigner;
        private ComboBox txtSingerR;
        private ComboBox txtUnit;
        private ComboBox txtUnitR;
        private UclAttrs ucAttr = null;

        public UcSentItem(DEBusinessItem item)
        {
            this.InitializeComponent();
            this._theItem = item;
            this.SetBtnAndPnlStatue();
            this.InitUc();
            this.InitLvwBom();
        }

        private void btnApp_Click(object sender, EventArgs e)
        {
            if (this.btnApp.Text == "编  辑")
            {
                this._theItem = PLItem.Agent.CheckOut(this._theItem.MasterOid, "DQDOSSIERPRINT", ClientData.LogonUser.Oid);
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
                    this.ucAttr.isEditable = false;
                    this.ucAttr.Display(true);
                    this.SetBtnAndPnlStatue();
                    this.RefreshBoms();
                    this.RefreshRrvBoms();
                }
            }
            else if (this.IsChange)
            {
                this.Save();
                this.RefreshBoms();
                this.RefreshRrvBoms();
            }
        }

        private void btnAppHSBom_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtUnitR.Text))
            {
                if (string.IsNullOrEmpty(this.txtDocCodeR.Text))
                {
                    foreach (DERelationBizItem item in this.GetCurRelItemsR)
                    {
                        if (this.DocAndUnitIsExists(item.Id, this.txtUnitR.Text, this.GetCurRelItemsR))
                        {
                            PlArchivManage.UpdateSentBom(item, this.txtUnitR.Text, Convert.ToInt32(this.txtNumRealFsR.Value), this.txtSingerR.Text, this.txtHsSm.Text);
                            this._isChg = true;
                        }
                    }
                }
                else
                {
                    if (!this.DocAndUnitIsExists(this.txtDocCodeR.Text, this.txtUnitR.Text, this.GetCurRelItemsR))
                    {
                        MessageBox.Show("图号:" + this.txtDocCodeR.Text + "单位 :" + this.txtUnitR.Text + "数据不存在");
                        return;
                    }
                    PlArchivManage.UpdateSentBom(PlArchivManage.GetRelItemById(this.txtDocCodeR.Text.ToUpper(), this._theItem, ConstAm.SENTRBOM_RELCLS), this.txtUnitR.Text, Convert.ToInt32(this.txtNumRealFsR.Value), this.txtSingerR.Text, this.txtHsSm.Text);
                    this._isChg = true;
                }
                this.RefreshSentItemForm(false);
            }
        }

        private void btnCancelFF_Click(object sender, EventArgs e)
        {
            this.EndOrCancel(false, true);
        }

        private void btnCancelHS_Click(object sender, EventArgs e)
        {
            this.EndOrCancel(false, false);
        }

        private void btnCloseSent_Click(object sender, EventArgs e)
        {
            if (this.IsChange && (MessageBox.Show("是否保存当前的收发设置后退出 ？", "未保存", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes))
            {
                this.Save();
                this._theItem = PLItem.Agent.CheckIn(this._theItem.MasterOid, this._theItem.ClassName, ClientData.LogonUser.Oid, "收发帐编辑");
                if (BizItemHandlerEvent.Instance.D_AfterCheckIn != null)
                {
                    BizItemHandlerEvent.Instance.D_AfterCheckIn(BizOperationHelper.ConvertPLMBizItemDelegateParam(this._theItem));
                }
            }
            if (this._bEdit && !this._issaved)
            {
                this._theItem = PLItem.Agent.GetBizItem(this._theItem.MasterOid, 0, 0, ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid, BizItemMode.BizItem) as DEBusinessItem;
                if (BizItemHandlerEvent.Instance.D_AfterIterationUpdated != null)
                {
                    BizItemHandlerEvent.Instance.D_AfterIterationUpdated(BizOperationHelper.ConvertPLMBizItemDelegateParam(this._theItem));
                }
            }
            DelegatesOfAm.Instance.D_AfterSentTabClose(this._theItem.Id);
        }

        private void btnEndFF_Click(object sender, EventArgs e)
        {
            this.EndOrCancel(true, true);
        }

        private void btnEndHS_Click(object sender, EventArgs e)
        {
            this.EndOrCancel(true, false);
        }

        private void btnEndSent_Click(object sender, EventArgs e)
        {
            FrmInputRemark remark = new FrmInputRemark("完成" + this._theItem.Id + "有关图纸的收发", true, true);
            if (remark.ShowDialog() == DialogResult.OK)
            {
                StringBuilder builder;
                if (this.IsChange)
                {
                    this.Save();
                }
                PlArchivManage.EndSent(this._theItem, remark.StrMarkup, out builder);
                if (builder.Length == 0)
                {
                    FrmArchivManage.frmMian.DisplayTextInRichtBox("完成" + this._theItem.Id + "有关图纸的收发", 1, false);
                }
                else
                {
                    FrmArchivManage.frmMian.DisplayTextInRichtBox(string.Concat(new object[] { "未完成", this._theItem.Id, "有关图纸的收发：", builder }), 0, true);
                }
            }
            this.ReNewGetItem();
        }

        private void btnSaveFFBom_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtUnit.Text))
            {
                if (string.IsNullOrEmpty(this.txtDocCode.Text))
                {
                    foreach (DERelationBizItem item in this.GetCurRelItems)
                    {
                        if (this.DocAndUnitIsExists(item.Id, this.txtUnit.Text, this.GetCurRelItems))
                        {
                            PlArchivManage.UpdateSentBom(item, this.txtUnit.Text, Convert.ToInt32(this.txtNumRealFS.Value), this.txtSigner.Text, this.txtFFSM.Text);
                            this._isChg = true;
                        }
                    }
                }
                else
                {
                    if (!this.DocAndUnitIsExists(this.txtDocCode.Text, this.txtUnit.Text, this.GetCurRelItems))
                    {
                        MessageBox.Show("图号:" + this.txtDocCode.Text + "单位 :" + this.txtUnit.Text + "数据不存在");
                        return;
                    }
                    PlArchivManage.UpdateSentBom(PlArchivManage.GetRelItemById(this.txtDocCode.Text.ToUpper(), this._theItem, ConstAm.SENTBOM_RELCLS), this.txtUnit.Text, Convert.ToInt32(this.txtNumRealFS.Value), this.txtSigner.Text, this.txtFFSM.Text);
                    this._isChg = true;
                }
                this.RefreshSentItemForm(true);
            }
        }

        private void chkFilterFF_CheckedChanged(object sender, EventArgs e)
        {
            this.RefreshBoms();
        }

        private void chkFilterHS_CheckedChanged(object sender, EventArgs e)
        {
            this.RefreshRrvBoms();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DocAndUnitIsExists(string id, string unit, ArrayList itemLst)
        {
            foreach (DERelationBizItem item in itemLst)
            {
                if (!string.IsNullOrEmpty(id) && (item.Id == id.ToUpper()))
                {
                    if (string.IsNullOrEmpty(unit))
                    {
                        return true;
                    }
                    string str = (item.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW) == null) ? "" : item.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_FFDW).ToString();
                    return (str.IndexOf(unit + "(") != -1);
                }
            }
            return false;
        }

        private void EndOrCancel(bool isEnd, bool isFf)
        {
            string str = isEnd ? "完成" : "取消";
            string str2 = isFf ? "发放" : "回收";
            string str3 = isFf ? this.txtDocCode.Text : this.txtDocCodeR.Text;
            string str4 = isFf ? this.txtUnit.Text : this.txtUnitR.Text;
            string str5 = isFf ? this.txtFFSM.Text : this.txtHsSm.Text;
            string str6 = isFf ? this.txtSigner.Text : this.txtSingerR.Text;
            ArrayList itemLst = isFf ? this.GetCurRelItems : this.GetCurRelItemsR;
            if (!string.IsNullOrEmpty(str3) || !string.IsNullOrEmpty(str4))
            {
                if (string.IsNullOrEmpty(str3))
                {
                    if (MessageBox.Show(str + this.txtUnit.Text + "有关图纸的" + str2, str + str2, MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        return;
                    }
                }
                else if (string.IsNullOrEmpty(str4))
                {
                    if (MessageBox.Show(str + "图号为" + str3 + "所有相关单位的" + str2, str + str2, MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        return;
                    }
                }
                else if (!this.DocAndUnitIsExists(str3, str4, itemLst))
                {
                    MessageBox.Show("要修改的数据不存在");
                    return;
                }
                if (!(isEnd || !string.IsNullOrEmpty(str5)))
                {
                    MessageBox.Show("取消原因没有填写");
                }
                else if (isEnd && string.IsNullOrEmpty(str6))
                {
                    MessageBox.Show("签收人没有填写");
                }
                else
                {
                    StringBuilder builder;
                    StringBuilder builder2;
                    PlArchivManage.EndOrCancelSent(str3, str4, str5, str6, itemLst, isEnd, out builder, out builder2);
                    if (builder.Length > 0)
                    {
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("下列数据没有完成" + str + str2 + "处理：" + builder.ToString(), 0, false);
                    }
                    if (builder2.Length > 0)
                    {
                        FrmArchivManage.frmMian.DisplayTextInRichtBox("下列数据完成" + str + str2 + "处理：" + builder2.ToString(), 1, false);
                        this._isChg = true;
                        this.RefreshSentItemForm(isFf);
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.tCtrlSentBom = new TabControl();
            this.tPSentInfo = new TabPage();
            this.grpSentattr = new GroupBox();
            this.panel1 = new Panel();
            this.btnCloseSent = new Button();
            this.btnApp = new Button();
            this.btnEndSent = new Button();
            this.tpSentBom = new TabPage();
            this.lvwSentBom = new SortableListView();
            this.pnlFF = new Panel();
            this.btnCancelFF = new Button();
            this.btnEndFF = new Button();
            this.chkFilterFF = new CheckBox();
            this.txtFFSM = new TextBox();
            this.txtNumMtZs = new NumericUpDown();
            this.txtNumRealFS = new NumericUpDown();
            this.txtUnit = new ComboBox();
            this.txtDocRev = new TextBox();
            this.txtDocName = new TextBox();
            this.txtDocCode = new ComboBox();
            this.label5 = new Label();
            this.btnSaveFFBom = new Button();
            this.label4 = new Label();
            this.label7 = new Label();
            this.label8 = new Label();
            this.label6 = new Label();
            this.label3 = new Label();
            this.label2 = new Label();
            this.label1 = new Label();
            this.tpRecyclebom = new TabPage();
            this.lvwRecycleBom = new SortableListView();
            this.panel3 = new Panel();
            this.pnlHS = new Panel();
            this.btnCancelHS = new Button();
            this.btnEndHS = new Button();
            this.chkFilterHS = new CheckBox();
            this.txtHsSm = new TextBox();
            this.txtNumZsR = new NumericUpDown();
            this.txtNumRealFsR = new NumericUpDown();
            this.txtUnitR = new ComboBox();
            this.txtDocRevR = new TextBox();
            this.txtDocNameR = new TextBox();
            this.txtDocCodeR = new ComboBox();
            this.label9 = new Label();
            this.btnAppHSBom = new Button();
            this.label10 = new Label();
            this.label11 = new Label();
            this.label12 = new Label();
            this.label13 = new Label();
            this.label14 = new Label();
            this.label15 = new Label();
            this.label16 = new Label();
            this.txtSigner = new ComboBox();
            this.txtSingerR = new ComboBox();
            this.tCtrlSentBom.SuspendLayout();
            this.tPSentInfo.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tpSentBom.SuspendLayout();
            this.pnlFF.SuspendLayout();
            this.txtNumMtZs.BeginInit();
            this.txtNumRealFS.BeginInit();
            this.tpRecyclebom.SuspendLayout();
            this.panel3.SuspendLayout();
            this.pnlHS.SuspendLayout();
            this.txtNumZsR.BeginInit();
            this.txtNumRealFsR.BeginInit();
            base.SuspendLayout();
            this.tCtrlSentBom.Controls.Add(this.tPSentInfo);
            this.tCtrlSentBom.Controls.Add(this.tpSentBom);
            this.tCtrlSentBom.Controls.Add(this.tpRecyclebom);
            this.tCtrlSentBom.Dock = DockStyle.Fill;
            this.tCtrlSentBom.Location = new Point(0, 0);
            this.tCtrlSentBom.Name = "tCtrlSentBom";
            this.tCtrlSentBom.SelectedIndex = 0;
            this.tCtrlSentBom.Size = new Size(0x2f2, 0x1d5);
            this.tCtrlSentBom.TabIndex = 0;
            this.tPSentInfo.Controls.Add(this.grpSentattr);
            this.tPSentInfo.Controls.Add(this.panel1);
            this.tPSentInfo.Location = new Point(4, 0x16);
            this.tPSentInfo.Name = "tPSentInfo";
            this.tPSentInfo.Padding = new Padding(3);
            this.tPSentInfo.Size = new Size(0x2ea, 0x1bb);
            this.tPSentInfo.TabIndex = 0;
            this.tPSentInfo.Text = "收发帐信息";
            this.tPSentInfo.UseVisualStyleBackColor = true;
            this.grpSentattr.BackColor = Color.WhiteSmoke;
            this.grpSentattr.Dock = DockStyle.Fill;
            this.grpSentattr.Location = new Point(3, 3);
            this.grpSentattr.Name = "grpSentattr";
            this.grpSentattr.Size = new Size(740, 0x182);
            this.grpSentattr.TabIndex = 1;
            this.grpSentattr.TabStop = false;
            this.panel1.BackColor = Color.WhiteSmoke;
            this.panel1.Controls.Add(this.btnCloseSent);
            this.panel1.Controls.Add(this.btnApp);
            this.panel1.Controls.Add(this.btnEndSent);
            this.panel1.Dock = DockStyle.Bottom;
            this.panel1.Location = new Point(3, 0x185);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(740, 0x33);
            this.panel1.TabIndex = 0;
            this.btnCloseSent.Anchor = AnchorStyles.Right;
            this.btnCloseSent.Location = new Point(0x28c, 15);
            this.btnCloseSent.Name = "btnCloseSent";
            this.btnCloseSent.Size = new Size(0x4b, 0x17);
            this.btnCloseSent.TabIndex = 2;
            this.btnCloseSent.Text = "关闭";
            this.btnCloseSent.UseVisualStyleBackColor = true;
            this.btnCloseSent.Click += new EventHandler(this.btnCloseSent_Click);
            this.btnApp.Anchor = AnchorStyles.Right;
            this.btnApp.Location = new Point(0x23b, 15);
            this.btnApp.Name = "btnApp";
            this.btnApp.Size = new Size(0x4b, 0x17);
            this.btnApp.TabIndex = 1;
            this.btnApp.Text = "应  用";
            this.btnApp.UseVisualStyleBackColor = true;
            this.btnApp.Click += new EventHandler(this.btnApp_Click);
            this.btnEndSent.Anchor = AnchorStyles.Right;
            this.btnEndSent.Location = new Point(490, 15);
            this.btnEndSent.Name = "btnEndSent";
            this.btnEndSent.Size = new Size(0x4b, 0x17);
            this.btnEndSent.TabIndex = 0;
            this.btnEndSent.Text = "完成处理";
            this.btnEndSent.UseVisualStyleBackColor = true;
            this.btnEndSent.Click += new EventHandler(this.btnEndSent_Click);
            this.tpSentBom.Controls.Add(this.lvwSentBom);
            this.tpSentBom.Controls.Add(this.pnlFF);
            this.tpSentBom.Location = new Point(4, 0x16);
            this.tpSentBom.Name = "tpSentBom";
            this.tpSentBom.Padding = new Padding(3);
            this.tpSentBom.Size = new Size(0x2ea, 0x1bb);
            this.tpSentBom.TabIndex = 1;
            this.tpSentBom.Text = "发放明细";
            this.tpSentBom.UseVisualStyleBackColor = true;
            this.lvwSentBom.Dock = DockStyle.Fill;
            this.lvwSentBom.FullRowSelect = true;
            this.lvwSentBom.GridLines = true;
            this.lvwSentBom.HideSelection = false;
            this.lvwSentBom.Location = new Point(3, 3);
            this.lvwSentBom.Name = "lvwSentBom";
            this.lvwSentBom.Size = new Size(740, 0x150);
            this.lvwSentBom.SortingOrder = SortOrder.None;
            this.lvwSentBom.TabIndex = 1;
            this.lvwSentBom.UseCompatibleStateImageBehavior = false;
            this.lvwSentBom.View = View.Details;
            this.lvwSentBom.SelectedIndexChanged += new EventHandler(this.lvwSentBom_SelectedIndexChanged);
            this.lvwSentBom.KeyUp += new KeyEventHandler(this.lvwSentBom_KeyUp);
            this.pnlFF.BackColor = Color.WhiteSmoke;
            this.pnlFF.Controls.Add(this.txtSigner);
            this.pnlFF.Controls.Add(this.btnCancelFF);
            this.pnlFF.Controls.Add(this.btnEndFF);
            this.pnlFF.Controls.Add(this.chkFilterFF);
            this.pnlFF.Controls.Add(this.txtFFSM);
            this.pnlFF.Controls.Add(this.txtNumMtZs);
            this.pnlFF.Controls.Add(this.txtNumRealFS);
            this.pnlFF.Controls.Add(this.txtUnit);
            this.pnlFF.Controls.Add(this.txtDocRev);
            this.pnlFF.Controls.Add(this.txtDocName);
            this.pnlFF.Controls.Add(this.txtDocCode);
            this.pnlFF.Controls.Add(this.label5);
            this.pnlFF.Controls.Add(this.btnSaveFFBom);
            this.pnlFF.Controls.Add(this.label4);
            this.pnlFF.Controls.Add(this.label7);
            this.pnlFF.Controls.Add(this.label8);
            this.pnlFF.Controls.Add(this.label6);
            this.pnlFF.Controls.Add(this.label3);
            this.pnlFF.Controls.Add(this.label2);
            this.pnlFF.Controls.Add(this.label1);
            this.pnlFF.Dock = DockStyle.Bottom;
            this.pnlFF.Location = new Point(3, 0x153);
            this.pnlFF.Name = "pnlFF";
            this.pnlFF.Size = new Size(740, 0x65);
            this.pnlFF.TabIndex = 0;
            this.btnCancelFF.Location = new Point(0x283, 0x25);
            this.btnCancelFF.Name = "btnCancelFF";
            this.btnCancelFF.Size = new Size(0x4b, 0x18);
            this.btnCancelFF.TabIndex = 0x22;
            this.btnCancelFF.Text = "取消处理";
            this.btnCancelFF.UseVisualStyleBackColor = true;
            this.btnCancelFF.Click += new EventHandler(this.btnCancelFF_Click);
            this.btnEndFF.Location = new Point(0x283, 8);
            this.btnEndFF.Name = "btnEndFF";
            this.btnEndFF.Size = new Size(0x4b, 0x17);
            this.btnEndFF.TabIndex = 0x21;
            this.btnEndFF.Text = "处理完成";
            this.btnEndFF.UseVisualStyleBackColor = true;
            this.btnEndFF.Click += new EventHandler(this.btnEndFF_Click);
            this.chkFilterFF.AutoSize = true;
            this.chkFilterFF.Location = new Point(0x240, 15);
            this.chkFilterFF.Name = "chkFilterFF";
            this.chkFilterFF.Size = new Size(0x30, 0x10);
            this.chkFilterFF.TabIndex = 0x20;
            this.chkFilterFF.Text = "过滤";
            this.chkFilterFF.UseVisualStyleBackColor = true;
            this.chkFilterFF.CheckedChanged += new EventHandler(this.chkFilterFF_CheckedChanged);
            this.txtFFSM.Location = new Point(70, 0x42);
            this.txtFFSM.Name = "txtFFSM";
            this.txtFFSM.Size = new Size(0x131, 0x15);
            this.txtFFSM.TabIndex = 0x1d;
            this.txtNumMtZs.Location = new Point(0x1bf, 40);
            int[] bits = new int[4];
            bits[0] = 0x7d0;
            this.txtNumMtZs.Maximum = new decimal(bits);
            this.txtNumMtZs.Name = "txtNumMtZs";
            this.txtNumMtZs.ReadOnly = true;
            this.txtNumMtZs.Size = new Size(0x72, 0x15);
            this.txtNumMtZs.TabIndex = 0x1c;
            this.txtNumRealFS.Location = new Point(0x1c1, 11);
            int[] bits1 = new int[4];
            bits1[0] = 0x3e8;
            this.txtNumRealFS.Maximum = new decimal(bits1);
            this.txtNumRealFS.Name = "txtNumRealFS";
            this.txtNumRealFS.Size = new Size(0x70, 0x15);
            this.txtNumRealFS.TabIndex = 0x1b;
            this.txtUnit.DropDownStyle = ComboBoxStyle.DropDownList;
            this.txtUnit.FormattingEnabled = true;
            this.txtUnit.Location = new Point(0x33, 0x25);
            this.txtUnit.Name = "txtUnit";
            this.txtUnit.Size = new Size(0x8f, 20);
            this.txtUnit.TabIndex = 0x1a;
            this.txtUnit.SelectedIndexChanged += new EventHandler(this.txtUnit_SelectedIndexChanged);
            this.txtDocRev.Location = new Point(0x111, 40);
            this.txtDocRev.Name = "txtDocRev";
            this.txtDocRev.ReadOnly = true;
            this.txtDocRev.Size = new Size(0x66, 0x15);
            this.txtDocRev.TabIndex = 0x19;
            this.txtDocName.Location = new Point(0x111, 12);
            this.txtDocName.Name = "txtDocName";
            this.txtDocName.ReadOnly = true;
            this.txtDocName.Size = new Size(100, 0x15);
            this.txtDocName.TabIndex = 0x18;
            this.txtDocCode.DropDownStyle = ComboBoxStyle.DropDownList;
            this.txtDocCode.FormattingEnabled = true;
            this.txtDocCode.Location = new Point(0x33, 10);
            this.txtDocCode.Name = "txtDocCode";
            this.txtDocCode.Size = new Size(0x8f, 20);
            this.txtDocCode.TabIndex = 0x17;
            this.txtDocCode.SelectedIndexChanged += new EventHandler(this.txtDocCode_SelectedIndexChanged);
            this.label5.AutoSize = true;
            this.label5.Location = new Point(0xd6, 0x2b);
            this.label5.Name = "label5";
            this.label5.Size = new Size(0x35, 12);
            this.label5.TabIndex = 0x16;
            this.label5.Text = "图纸版本";
            this.btnSaveFFBom.Anchor = AnchorStyles.Left;
            this.btnSaveFFBom.Location = new Point(0x283, 0x41);
            this.btnSaveFFBom.Name = "btnSaveFFBom";
            this.btnSaveFFBom.Size = new Size(0x4b, 0x17);
            this.btnSaveFFBom.TabIndex = 20;
            this.btnSaveFFBom.Text = "修改";
            this.btnSaveFFBom.UseVisualStyleBackColor = true;
            this.btnSaveFFBom.Click += new EventHandler(this.btnSaveFFBom_Click);
            this.label4.AutoSize = true;
            this.label4.Location = new Point(11, 0x44);
            this.label4.Name = "label4";
            this.label4.Size = new Size(0x35, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "发放说明";
            this.label7.AutoSize = true;
            this.label7.Location = new Point(0x17f, 0x2a);
            this.label7.Name = "label7";
            this.label7.Size = new Size(0x35, 12);
            this.label7.TabIndex = 7;
            this.label7.Text = "每份张数";
            this.label8.AutoSize = true;
            this.label8.Location = new Point(0x18b, 70);
            this.label8.Name = "label8";
            this.label8.Size = new Size(0x29, 12);
            this.label8.TabIndex = 6;
            this.label8.Text = "签收人";
            this.label6.AutoSize = true;
            this.label6.Location = new Point(0x10, 40);
            this.label6.Name = "label6";
            this.label6.Size = new Size(0x1d, 12);
            this.label6.TabIndex = 4;
            this.label6.Text = "单位";
            this.label3.AutoSize = true;
            this.label3.Location = new Point(0x17f, 0x11);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x35, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "发放份数";
            this.label2.AutoSize = true;
            this.label2.Location = new Point(0xd6, 15);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x35, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "图纸名称";
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0x10, 13);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x1d, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "图号";
            this.tpRecyclebom.Controls.Add(this.lvwRecycleBom);
            this.tpRecyclebom.Controls.Add(this.panel3);
            this.tpRecyclebom.Location = new Point(4, 0x16);
            this.tpRecyclebom.Name = "tpRecyclebom";
            this.tpRecyclebom.Size = new Size(0x2ea, 0x1bb);
            this.tpRecyclebom.TabIndex = 2;
            this.tpRecyclebom.Text = "回收明细";
            this.tpRecyclebom.UseVisualStyleBackColor = true;
            this.lvwRecycleBom.Dock = DockStyle.Fill;
            this.lvwRecycleBom.FullRowSelect = true;
            this.lvwRecycleBom.HideSelection = false;
            this.lvwRecycleBom.Location = new Point(0, 0);
            this.lvwRecycleBom.Name = "lvwRecycleBom";
            this.lvwRecycleBom.Size = new Size(0x2ea, 0x157);
            this.lvwRecycleBom.SortingOrder = SortOrder.None;
            this.lvwRecycleBom.TabIndex = 2;
            this.lvwRecycleBom.UseCompatibleStateImageBehavior = false;
            this.lvwRecycleBom.View = View.Details;
            this.lvwRecycleBom.SelectedIndexChanged += new EventHandler(this.lvwRecycleBom_SelectedIndexChanged);
            this.lvwRecycleBom.KeyUp += new KeyEventHandler(this.lvwRecycleBom_KeyUp);
            this.panel3.BackColor = Color.WhiteSmoke;
            this.panel3.Controls.Add(this.pnlHS);
            this.panel3.Dock = DockStyle.Bottom;
            this.panel3.Location = new Point(0, 0x157);
            this.panel3.Name = "panel3";
            this.panel3.Size = new Size(0x2ea, 100);
            this.panel3.TabIndex = 1;
            this.pnlHS.BackColor = Color.WhiteSmoke;
            this.pnlHS.Controls.Add(this.txtSingerR);
            this.pnlHS.Controls.Add(this.btnCancelHS);
            this.pnlHS.Controls.Add(this.btnEndHS);
            this.pnlHS.Controls.Add(this.chkFilterHS);
            this.pnlHS.Controls.Add(this.txtHsSm);
            this.pnlHS.Controls.Add(this.txtNumZsR);
            this.pnlHS.Controls.Add(this.txtNumRealFsR);
            this.pnlHS.Controls.Add(this.txtUnitR);
            this.pnlHS.Controls.Add(this.txtDocRevR);
            this.pnlHS.Controls.Add(this.txtDocNameR);
            this.pnlHS.Controls.Add(this.txtDocCodeR);
            this.pnlHS.Controls.Add(this.label9);
            this.pnlHS.Controls.Add(this.btnAppHSBom);
            this.pnlHS.Controls.Add(this.label10);
            this.pnlHS.Controls.Add(this.label11);
            this.pnlHS.Controls.Add(this.label12);
            this.pnlHS.Controls.Add(this.label13);
            this.pnlHS.Controls.Add(this.label14);
            this.pnlHS.Controls.Add(this.label15);
            this.pnlHS.Controls.Add(this.label16);
            this.pnlHS.Dock = DockStyle.Fill;
            this.pnlHS.Location = new Point(0, 0);
            this.pnlHS.Name = "pnlHS";
            this.pnlHS.Size = new Size(0x2ea, 100);
            this.pnlHS.TabIndex = 1;
            this.btnCancelHS.Location = new Point(0x287, 0x26);
            this.btnCancelHS.Name = "btnCancelHS";
            this.btnCancelHS.Size = new Size(0x4b, 0x18);
            this.btnCancelHS.TabIndex = 0x24;
            this.btnCancelHS.Text = "取消处理";
            this.btnCancelHS.UseVisualStyleBackColor = true;
            this.btnCancelHS.Click += new EventHandler(this.btnCancelHS_Click);
            this.btnEndHS.Location = new Point(0x287, 9);
            this.btnEndHS.Name = "btnEndHS";
            this.btnEndHS.Size = new Size(0x4b, 0x17);
            this.btnEndHS.TabIndex = 0x23;
            this.btnEndHS.Text = "处理完成";
            this.btnEndHS.UseVisualStyleBackColor = true;
            this.btnEndHS.Click += new EventHandler(this.btnEndHS_Click);
            this.chkFilterHS.AutoSize = true;
            this.chkFilterHS.Location = new Point(0x237, 15);
            this.chkFilterHS.Name = "chkFilterHS";
            this.chkFilterHS.Size = new Size(0x30, 0x10);
            this.chkFilterHS.TabIndex = 0x1f;
            this.chkFilterHS.Text = "过滤";
            this.chkFilterHS.UseVisualStyleBackColor = true;
            this.chkFilterHS.CheckedChanged += new EventHandler(this.chkFilterHS_CheckedChanged);
            this.txtHsSm.Location = new Point(70, 0x42);
            this.txtHsSm.Name = "txtHsSm";
            this.txtHsSm.Size = new Size(0x11d, 0x15);
            this.txtHsSm.TabIndex = 0x1d;
            this.txtNumZsR.Location = new Point(0x1a2, 0x24);
            int[] bits2 = new int[4];
            bits2[0] = 0x7d0;
            this.txtNumZsR.Maximum = new decimal(bits2);
            this.txtNumZsR.Name = "txtNumZsR";
            this.txtNumZsR.ReadOnly = true;
            this.txtNumZsR.Size = new Size(0x79, 0x15);
            this.txtNumZsR.TabIndex = 0x1c;
            this.txtNumRealFsR.Location = new Point(0xff, 0x26);
            int[] bits3 = new int[4];
            bits3[0] = 0x3e8;
            this.txtNumRealFsR.Maximum = new decimal(bits3);
            this.txtNumRealFsR.Name = "txtNumRealFsR";
            this.txtNumRealFsR.Size = new Size(100, 0x15);
            this.txtNumRealFsR.TabIndex = 0x1b;
            this.txtUnitR.DropDownStyle = ComboBoxStyle.DropDownList;
            this.txtUnitR.FormattingEnabled = true;
            this.txtUnitR.Location = new Point(0x2e, 0x26);
            this.txtUnitR.Name = "txtUnitR";
            this.txtUnitR.Size = new Size(0x8e, 20);
            this.txtUnitR.TabIndex = 0x1a;
            this.txtUnitR.SelectedIndexChanged += new EventHandler(this.txtUnitR_SelectedIndexChanged);
            this.txtDocRevR.Location = new Point(0x1a2, 10);
            this.txtDocRevR.Name = "txtDocRevR";
            this.txtDocRevR.ReadOnly = true;
            this.txtDocRevR.Size = new Size(0x79, 0x15);
            this.txtDocRevR.TabIndex = 0x19;
            this.txtDocNameR.Location = new Point(0xff, 10);
            this.txtDocNameR.Name = "txtDocNameR";
            this.txtDocNameR.ReadOnly = true;
            this.txtDocNameR.Size = new Size(100, 0x15);
            this.txtDocNameR.TabIndex = 0x18;
            this.txtDocCodeR.DropDownStyle = ComboBoxStyle.DropDownList;
            this.txtDocCodeR.FormattingEnabled = true;
            this.txtDocCodeR.Location = new Point(0x2e, 11);
            this.txtDocCodeR.Name = "txtDocCodeR";
            this.txtDocCodeR.Size = new Size(0x8f, 20);
            this.txtDocCodeR.TabIndex = 0x17;
            this.txtDocCodeR.SelectedIndexChanged += new EventHandler(this.txtDocCodeR_SelectedIndexChanged);
            this.label9.AutoSize = true;
            this.label9.Location = new Point(0x167, 13);
            this.label9.Name = "label9";
            this.label9.Size = new Size(0x35, 12);
            this.label9.TabIndex = 0x16;
            this.label9.Text = "图纸版本";
            this.btnAppHSBom.Anchor = AnchorStyles.Left;
            this.btnAppHSBom.Location = new Point(0x287, 0x44);
            this.btnAppHSBom.Name = "btnAppHSBom";
            this.btnAppHSBom.Size = new Size(0x4b, 0x17);
            this.btnAppHSBom.TabIndex = 20;
            this.btnAppHSBom.Text = "修改";
            this.btnAppHSBom.UseVisualStyleBackColor = true;
            this.btnAppHSBom.Click += new EventHandler(this.btnAppHSBom_Click);
            this.label10.AutoSize = true;
            this.label10.Location = new Point(11, 0x44);
            this.label10.Name = "label10";
            this.label10.Size = new Size(0x35, 12);
            this.label10.TabIndex = 8;
            this.label10.Text = "回收说明";
            this.label11.AutoSize = true;
            this.label11.Location = new Point(360, 0x26);
            this.label11.Name = "label11";
            this.label11.Size = new Size(0x35, 12);
            this.label11.TabIndex = 7;
            this.label11.Text = "每份张数";
            this.label12.AutoSize = true;
            this.label12.Location = new Point(0x175, 0x42);
            this.label12.Name = "label12";
            this.label12.Size = new Size(0x29, 12);
            this.label12.TabIndex = 6;
            this.label12.Text = "签收人";
            this.label13.AutoSize = true;
            this.label13.Location = new Point(11, 0x29);
            this.label13.Name = "label13";
            this.label13.Size = new Size(0x1d, 12);
            this.label13.TabIndex = 4;
            this.label13.Text = "单位";
            this.label14.AutoSize = true;
            this.label14.Location = new Point(200, 0x29);
            this.label14.Name = "label14";
            this.label14.Size = new Size(0x35, 12);
            this.label14.TabIndex = 3;
            this.label14.Text = "回收份数";
            this.label15.AutoSize = true;
            this.label15.Location = new Point(0xc5, 0x11);
            this.label15.Name = "label15";
            this.label15.Size = new Size(0x35, 12);
            this.label15.TabIndex = 1;
            this.label15.Text = "图纸名称";
            this.label16.AutoSize = true;
            this.label16.Location = new Point(11, 14);
            this.label16.Name = "label16";
            this.label16.Size = new Size(0x1d, 12);
            this.label16.TabIndex = 0;
            this.label16.Text = "图号";
            this.txtSigner.FormattingEnabled = true;
            this.txtSigner.Location = new Point(0x1c1, 0x44);
            this.txtSigner.Name = "txtSigner";
            this.txtSigner.Size = new Size(0x70, 20);
            this.txtSigner.TabIndex = 0x23;
            this.txtSingerR.FormattingEnabled = true;
            this.txtSingerR.Location = new Point(0x1a2, 0x42);
            this.txtSingerR.Name = "txtSingerR";
            this.txtSingerR.Size = new Size(0x79, 20);
            this.txtSingerR.TabIndex = 0x25;
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            base.Controls.Add(this.tCtrlSentBom);
            base.Name = "UcSentItem";
            base.Size = new Size(0x2f2, 0x1d5);
            this.tCtrlSentBom.ResumeLayout(false);
            this.tPSentInfo.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tpSentBom.ResumeLayout(false);
            this.pnlFF.ResumeLayout(false);
            this.pnlFF.PerformLayout();
            this.txtNumMtZs.EndInit();
            this.txtNumRealFS.EndInit();
            this.tpRecyclebom.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.pnlHS.ResumeLayout(false);
            this.pnlHS.PerformLayout();
            this.txtNumZsR.EndInit();
            this.txtNumRealFsR.EndInit();
            base.ResumeLayout(false);
        }

        private void InitLvwBom()
        {
            int num;
            string str;
            ArrayList tsRes = PlArchivManage.GetTsRes("路线部门") as ArrayList;
            if (this.tCtrlSentBom.TabPages.Contains(this.tpSentBom))
            {
                for (num = 0; num < tsRes.Count; num++)
                {
                    str = tsRes[num].ToString();
                    this.txtUnit.Items.Add(str);
                }
                this.txtUnit.Items.Insert(0, "");
            }
            if (this.tCtrlSentBom.TabPages.Contains(this.tpRecyclebom))
            {
                for (num = 0; num < tsRes.Count; num++)
                {
                    str = tsRes[num].ToString();
                    this.txtUnitR.Items.Add(str);
                }
                this.txtUnitR.Items.Insert(0, "");
            }
        }

        private void InitUc()
        {
            this.hsCols1 = PlArchivManage.Agent.GetViewOfCol("发放明细", out this.lstOrder1, out this.hsColWide1);
            this.hsCols2 = PlArchivManage.Agent.GetViewOfCol("回收明细", out this.lstOrder2, out this.hsColWide2);
            PlArchivManage.SetCol(this.hsCols1, this.lvwSentBom, "SentBom", this.lstOrder1, this.hsColWide1);
            PlArchivManage.SetCol(this.hsCols2, this.lvwRecycleBom, "SentBom", this.lstOrder2, this.hsColWide2);
            PlArchivManage.SetSigner(this.txtSigner);
            PlArchivManage.SetSigner(this.txtSingerR);
            this.tstype = this._theItem.Iteration.GetAttrValue("TSTYPE").ToString();
            if (this.tstype == "回收")
            {
                this.tCtrlSentBom.TabPages.Remove(this.tpSentBom);
            }
            else
            {
                this.SetDocIds(this.GetCurRelItems, this.txtDocCode);
                this.RefreshBoms();
            }
            if ((this.tstype == "新发") || (this.tstype == "补发"))
            {
                this.tCtrlSentBom.TabPages.Remove(this.tpRecyclebom);
            }
            else
            {
                this.SetDocIds(this.GetCurRelItemsR, this.txtDocCodeR);
                this.RefreshRrvBoms();
            }
            ObjectNavigateContext context = new ObjectNavigateContext();
            this.ucAttr = new UclAttrs();
            this.ucAttr.Dock = DockStyle.Fill;
            this.grpSentattr.Controls.Add(this.ucAttr);
            context.Option = ClientData.UserGlobalOption;
            this.ucAttr.CurMeta = ModelContext.MetaModel.GetClassEx(this._theItem.ClassName);
            this.ucAttr.SetContext(context);
            this.ucAttr.CurItem = this._theItem;
            this.ucAttr.isEditable = false;
            this.ucAttr.Display(true);
        }

        private void lvwRecycleBom_KeyUp(object sender, KeyEventArgs e)
        {
            if ((this.lvwRecycleBom.SelectedItems.Count != 0) && (this.lvwRecycleBom.SelectedItems.Count <= 1))
            {
                int index = this.lvwRecycleBom.SelectedItems[0].Index;
                if (e.KeyCode == Keys.Down)
                {
                    if (index < (this.lvwRecycleBom.Items.Count - 1))
                    {
                        this.lvwRecycleBom.Items[index].Selected = false;
                        this.lvwRecycleBom.Items[index + 1].Selected = true;
                    }
                }
                else if ((e.KeyCode == Keys.Up) && (index != 0))
                {
                    this.lvwRecycleBom.Items[index].Selected = false;
                    this.lvwRecycleBom.Items[index - 1].Selected = true;
                }
            }
        }

        private void lvwRecycleBom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.lvwRecycleBom.SelectedItems.Count == 1)
            {
                ListViewItem item = this.lvwRecycleBom.SelectedItems[0];
                DERelationBizItem tag = item.Tag as DERelationBizItem;
                this.txtDocCodeR.Text = tag.Id;
                this.txtDocNameR.Text = tag.BizItem.Name;
                this.txtDocRevR.Text = (tag.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_REV) == null) ? "" : tag.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_REV).ToString();
                object attrValue = tag.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_MTZS);
                this.txtNumMtZs.Value = (attrValue == null) ? 0 : Convert.ToInt32(attrValue);
                DEMetaAttribute relationAttribute = ModelContext.MetaModel.GetRelationAttribute(ConstAm.SENTBOM_RELCLS, ConstAm.SENTBOM_ATTR_FFFS);
                string text = item.SubItems[this.lvwSentBom.Columns[relationAttribute.Label].Index].Text;
                if (!string.IsNullOrEmpty(text))
                {
                    this.txtNumRealFS.Value = Convert.ToInt32(text);
                }
                relationAttribute = ModelContext.MetaModel.GetRelationAttribute(ConstAm.SENTBOM_RELCLS, ConstAm.SENTBOM_ATTR_FFDW);
                string str2 = item.SubItems[this.lvwSentBom.Columns[relationAttribute.Label].Index].Text;
                this.txtUnitR.Text = str2;
                relationAttribute = ModelContext.MetaModel.GetRelationAttribute(ConstAm.SENTBOM_RELCLS, ConstAm.SENTBOM_ATTR_FFSM);
                string str3 = item.SubItems[this.lvwSentBom.Columns[relationAttribute.Label].Index].Text;
                this.txtHsSm.Text = str3;
                relationAttribute = ModelContext.MetaModel.GetRelationAttribute(ConstAm.SENTBOM_RELCLS, ConstAm.SENTBOM_ATTR_SINGNER);
                string str4 = item.SubItems[this.lvwSentBom.Columns[relationAttribute.Label].Index].Text;
                this.txtSingerR.Text = str4;
            }
        }

        private void lvwSentBom_KeyUp(object sender, KeyEventArgs e)
        {
            if ((this.lvwSentBom.SelectedItems.Count != 0) && (this.lvwSentBom.SelectedItems.Count <= 1))
            {
                int index = this.lvwSentBom.SelectedItems[0].Index;
                if (e.KeyCode == Keys.Down)
                {
                    if (index < (this.lvwSentBom.Items.Count - 1))
                    {
                        this.lvwSentBom.Items[index].Selected = false;
                        this.lvwSentBom.Items[index + 1].Selected = true;
                    }
                }
                else if ((e.KeyCode == Keys.Up) && (index != 0))
                {
                    this.lvwSentBom.Items[index].Selected = false;
                    this.lvwSentBom.Items[index - 1].Selected = true;
                }
            }
        }

        private void lvwSentBom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.lvwSentBom.SelectedItems.Count == 1)
            {
                ListViewItem item = this.lvwSentBom.SelectedItems[0];
                DERelationBizItem tag = item.Tag as DERelationBizItem;
                this.txtDocCode.Text = tag.Id;
                this.txtDocName.Text = tag.BizItem.Name;
                this.txtDocRev.Text = (tag.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_REV) == null) ? "" : tag.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_REV).ToString();
                object attrValue = tag.Relation.GetAttrValue(ConstAm.SENTBOM_ATTR_MTZS);
                this.txtNumMtZs.Value = (attrValue == null) ? 0 : Convert.ToInt32(attrValue);
                DEMetaAttribute relationAttribute = ModelContext.MetaModel.GetRelationAttribute(ConstAm.SENTBOM_RELCLS, ConstAm.SENTBOM_ATTR_FFFS);
                string text = item.SubItems[this.lvwSentBom.Columns[relationAttribute.Label].Index].Text;
                if (!string.IsNullOrEmpty(text))
                {
                    this.txtNumRealFS.Value = Convert.ToInt32(text);
                }
                relationAttribute = ModelContext.MetaModel.GetRelationAttribute(ConstAm.SENTBOM_RELCLS, ConstAm.SENTBOM_ATTR_FFDW);
                string str2 = item.SubItems[this.lvwSentBom.Columns[relationAttribute.Label].Index].Text;
                this.txtUnit.Text = str2;
                relationAttribute = ModelContext.MetaModel.GetRelationAttribute(ConstAm.SENTBOM_RELCLS, ConstAm.SENTBOM_ATTR_FFSM);
                string str3 = item.SubItems[this.lvwSentBom.Columns[relationAttribute.Label].Index].Text;
                this.txtFFSM.Text = str3;
                relationAttribute = ModelContext.MetaModel.GetRelationAttribute(ConstAm.SENTBOM_RELCLS, ConstAm.SENTBOM_ATTR_SINGNER);
                string str4 = item.SubItems[this.lvwSentBom.Columns[relationAttribute.Label].Index].Text;
                this.txtSigner.Text = str4;
            }
        }

        private void RefreshBoms()
        {
            ArrayList list = new ArrayList();
            DERelationBizItemList relListOfDEBizItem = PlArchivManage.GetRelListOfDEBizItem(this._theItem, ConstAm.SENTBOM_RELCLS);
            this.lvwSentBom.Items.Clear();
            foreach (DERelationBizItem item in relListOfDEBizItem.RelationBizItems)
            {
                if (item.Relation.State != RelationState.Deleted)
                {
                    list.Add(item.Id);
                    if ((string.IsNullOrEmpty(this.txtDocCode.Text) || !this.chkFilterFF.Checked) || (item.Id == this.txtDocCode.Text))
                    {
                        PlArchivManage.UpdateLvwBySentBom(this.lvwSentBom, this.lstOrder1, item, this.chkFilterFF.Checked ? this.txtUnit.Text : "");
                    }
                }
            }
        }

        private void RefreshRrvBoms()
        {
            DERelationBizItemList relListOfDEBizItem = PlArchivManage.GetRelListOfDEBizItem(this._theItem, ConstAm.SENTRBOM_RELCLS);
            this.lvwRecycleBom.Items.Clear();
            foreach (DERelationBizItem item in relListOfDEBizItem.RelationBizItems)
            {
                if ((item.Relation.State != RelationState.Deleted) && ((string.IsNullOrEmpty(this.txtDocCodeR.Text) || !this.chkFilterHS.Checked) || (item.Id == this.txtDocCodeR.Text)))
                {
                    PlArchivManage.UpdateLvwBySentBom(this.lvwRecycleBom, this.lstOrder2, item, this.chkFilterHS.Checked ? this.txtUnitR.Text : "");
                }
            }
        }

        private void RefreshSentItemForm(bool isFF)
        {
            if (this._isChg)
            {
                if (isFF)
                {
                    this.RefreshBoms();
                }
                else
                {
                    this.RefreshRrvBoms();
                }
            }
        }

        private void ReNewGetItem()
        {
            this._theItem = PLItem.Agent.GetBizItem(this._theItem.MasterOid, 0, 0, ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid, BizItemMode.BizItem) as DEBusinessItem;
            if (BizItemHandlerEvent.Instance.D_AfterIterationUpdated != null)
            {
                BizItemHandlerEvent.Instance.D_AfterIterationUpdated(BizOperationHelper.ConvertPLMBizItemDelegateParam(this._theItem));
            }
            this.ucAttr.CurItem = this._theItem;
            this.ucAttr.isEditable = false;
            this.ucAttr.Display(true);
            this.SetBtnAndPnlStatue();
            this.RefreshBoms();
        }

        private void Save()
        {
            if (this.IsChange)
            {
                this.ucAttr.Save();
                PlArchivManage.ResetRealfsOfSent(this._theItem);
                this._theItem.Iteration = PLItem.UpdateItemIteration(this._theItem.Iteration, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption);
                this.ucAttr.Display(true);
                if (BizItemHandlerEvent.Instance.D_AfterIterationUpdated != null)
                {
                    BizItemHandlerEvent.Instance.D_AfterIterationUpdated(BizOperationHelper.ConvertPLMBizItemDelegateParam(this._theItem));
                }
                this._issaved = true;
                this._isChg = false;
            }
        }

        private void SetBtnAndPnlStatue()
        {
            this.btnApp.Text = "应  用";
            if ((this._theItem.State == ItemState.CheckOut) && (this._theItem.Holder == ClientData.LogonUser.Oid))
            {
                this._bEdit = true;
                this.btnApp.Enabled = true;
            }
            else if (this._theItem.State == ItemState.CheckIn)
            {
                if (PLGrantPerm.CanDoClassOperation(ClientData.LogonUser.Oid, "DQDOSSIRSENT", Guid.Empty, "ClaRel_EDIT") == 1)
                {
                    this.btnApp.Text = "编  辑";
                    this.btnApp.Enabled = true;
                    this._bEdit = false;
                }
            }
            else
            {
                this._bEdit = false;
                this.btnApp.Enabled = false;
            }
            this.btnEndSent.Enabled = this.pnlFF.Enabled = this.pnlHS.Enabled = this._bEdit;
        }

        private void SetDocIds(ArrayList lst, ComboBox cb)
        {
            int num;
            ArrayList list = new ArrayList();
            for (num = 0; num < lst.Count; num++)
            {
                DERelationBizItem item = lst[num] as DERelationBizItem;
                string str = item.Relation.GetAttrValue("DOCCODE").ToString();
                list.Add(str);
            }
            list.Sort();
            for (num = 0; num < list.Count; num++)
            {
                cb.Items.Add(list[num].ToString());
            }
            cb.Items.Insert(0, "");
        }

        private void txtDocCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.chkFilterFF.Checked)
            {
                this.RefreshBoms();
            }
        }

        private void txtDocCodeR_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.chkFilterHS.Checked)
            {
                this.RefreshRrvBoms();
            }
        }

        private void txtUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.chkFilterFF.Checked)
            {
                this.RefreshBoms();
            }
        }

        private void txtUnitR_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.chkFilterHS.Checked)
            {
                this.RefreshRrvBoms();
            }
        }

        private ArrayList GetCurRelItems
        {
            get
            {
                return PlArchivManage.GetRelListOfDEBizItem(this._theItem, ConstAm.SENTBOM_RELCLS).RelationBizItems;
            }
        }

        private ArrayList GetCurRelItemsR
        {
            get
            {
                return PlArchivManage.GetRelListOfDEBizItem(this._theItem, ConstAm.SENTRBOM_RELCLS).RelationBizItems;
            }
        }

        private bool IsChange
        {
            get
            {
                return (this.ucAttr.IsChanged || this._isChg);
            }
        }
    }
}

