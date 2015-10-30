namespace DQArchivManageClt
{
    using DQArchivManageCommon;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;
    using Thyt.TiPLM.DEL.Product;
    using Thyt.TiPLM.PLL.Product2;
    using Thyt.TiPLM.UIL.Common;
    using Thyt.TiPLM.UIL.Product.Common;

    public class UcPrintItem : UserControl
    {
        private bool _bEdit;
        private bool _canEdit = false;
        private bool _isChg = false;
        private DEBusinessItem _theItem = null;
        private Button btnClose;
        private Button btnPrintCancel;
        private Button btnPrintEnd;
        private IContainer components = null;
        private Hashtable hsCols = null;
        private Hashtable hsColWide;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private ArrayList lstOrder = null;
        private SortableListView lvwPrintBom;
        private Panel panel1;
        private TextBox txtDesc;
        private TextBox txtDocCode;
        private TextBox txtDocName;
        private TextBox txtDocRev;
        private TextBox txtNumFs;
        private TextBox txtNumZs;
        private TextBox txtStatus;

        public UcPrintItem(DEBusinessItem item)
        {
            this.InitializeComponent();
            this._theItem = item;
            this.InitUc();
            this.InitIvwRelItem();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            bool flag = false;
            if (this._isChg)
            {
                int num;
                int num2;
                int num3;
                PlArchivManage.CheckPrintItem(this._theItem, out num, out num2, out num3);
                if (num2 > 0)
                {
                    if (MessageBox.Show("是否保存当前的打印状态设置后退出 ？", "未保存", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                    {
                        this.Save();
                    }
                }
                else if (num <= 0)
                {
                    if (num3 > 0)
                    {
                        switch (MessageBox.Show("是:将取消打印！\r\n否:仅保存修改! ", "取消打印", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                        {
                            case DialogResult.Yes:
                                PlArchivManage.CancelPrint(this._theItem, "");
                                this.Save();
                                break;

                            case DialogResult.No:
                                this.Save();
                                break;
                        }
                    }
                }
                else
                {
                    switch (MessageBox.Show("是: 将结束打印，不再允许更改！\r\n否:仅保存修改! ", "完成打印", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                    {
                        case DialogResult.Yes:
                        {
                            StringBuilder builder;
                            PlArchivManage.EndPrint(this._theItem, "", false);
                            this.Save();
                            ArrayList lstItems = new ArrayList();
                            lstItems.Add(this._theItem);
                            PlArchivManage.Agent.CheckTsdRight(lstItems, "EndPrint", out builder, "托晒");
                            if (builder.Length <= 0)
                            {
                                PlArchivManage.Agent.PrintOrSentTsd(ClientData.LogonUser.Oid, this._theItem, "EndPrint", out builder);
                                if (builder.Length > 0)
                                {
                                    FrmArchivManage.frmMian.DisplayTextInRichtBox("完成打印失败，检测到下列错误：\r\n\t" + builder.ToString(), 0, true);
                                    return;
                                }
                                flag = true;
                                break;
                            }
                            FrmArchivManage.frmMian.DisplayTextInRichtBox("完成打印失败，检测到下列错误：\r\n\t" + builder.ToString(), 0, true);
                            return;
                        }
                        case DialogResult.No:
                            this.Save();
                            break;
                    }
                }
            }
            this._theItem = PLItem.Agent.GetBizItem(this._theItem.MasterOid, 0, 0, ClientData.UserGlobalOption.CurView, ClientData.LogonUser.Oid, BizItemMode.BizItem) as DEBusinessItem;
            try
            {
                if (flag)
                {
                    PlArchivManage.CommitWorkItem(this._theItem);
                }
            }
            catch (Exception exception)
            {
                FrmArchivManage.frmMian.DisplayTextInRichtBox("打印完成，但提交流程失败:" + this._theItem.Id + exception.ToString(), 2, true);
            }
            if (BizItemHandlerEvent.Instance.D_AfterIterationUpdated != null)
            {
                BizItemHandlerEvent.Instance.D_AfterIterationUpdated(BizOperationHelper.ConvertPLMBizItemDelegateParam(this._theItem));
            }
            DelegatesOfAm.Instance.D_AfterPrintTabClose(this._theItem.Id);
        }

        private void btnPrintCancel_Click(object sender, EventArgs e)
        {
            if (this.lvwPrintBom.SelectedItems.Count != 0)
            {
                ListViewItem current;
                ArrayList list = new ArrayList();
                ArrayList list2 = new ArrayList();
                IEnumerator enumerator = this.lvwPrintBom.SelectedItems.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    current = (ListViewItem) enumerator.Current;
                    DERelationBizItem tag = current.Tag as DERelationBizItem;
                    if (!(this.GetRelItemStatus(tag) == "已取消"))
                    {
                        list.Add(tag);
                        list2.Add(current);
                    }
                }
                if (list.Count != 0)
                {
                    if (list.Count == 1)
                    {
                        current = list2[0] as ListViewItem;
                        this.lvwPrintBom.SelectedItems.Clear();
                        current.Selected = true;
                    }
                    bool isSameReason = false;
                    for (int i = 0; i < list.Count; i++)
                    {
                        bool flag2;
                        DERelationBizItem relItem = list[i] as DERelationBizItem;
                        if (i < (list.Count - 1))
                        {
                            flag2 = false;
                        }
                        else
                        {
                            flag2 = true;
                        }
                        if (!isSameReason)
                        {
                            FrmInputRemark remark = new FrmInputRemark(relItem.Id + "取消打印", false, flag2);
                            DialogResult result = remark.ShowDialog();
                            isSameReason = remark.IsSameReason;
                            string strMarkup = remark.StrMarkup;
                            if (isSameReason)
                            {
                                if (result != DialogResult.OK)
                                {
                                    int count = list.Count - i;
                                    list.RemoveRange(i, count);
                                }
                                else
                                {
                                    for (int j = i; j < list.Count; j++)
                                    {
                                        DERelationBizItem item4 = list[j] as DERelationBizItem;
                                        PlArchivManage.CancelPrint(item4, strMarkup);
                                        PlArchivManage.UpdatePrintLvwRelValues(this.lvwPrintBom, this.lstOrder, item4);
                                        this._isChg = true;
                                    }
                                }
                                break;
                            }
                            if (result != DialogResult.OK)
                            {
                                list.RemoveAt(i);
                                i--;
                            }
                            else
                            {
                                PlArchivManage.CancelPrint(relItem, strMarkup);
                                PlArchivManage.UpdatePrintLvwRelValues(this.lvwPrintBom, this.lstOrder, relItem);
                                this._isChg = true;
                            }
                        }
                    }
                    this.RefreshFormByDocId();
                }
            }
        }

        private void btnPrintEnd_Click(object sender, EventArgs e)
        {
            if (this.lvwPrintBom.SelectedItems.Count != 0)
            {
                ListViewItem current;
                ArrayList list = new ArrayList();
                ArrayList list2 = new ArrayList();
                IEnumerator enumerator = this.lvwPrintBom.SelectedItems.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    current = (ListViewItem) enumerator.Current;
                    DERelationBizItem tag = current.Tag as DERelationBizItem;
                    if (!(this.GetRelItemStatus(tag) == "已打印"))
                    {
                        list.Add(tag);
                        list2.Add(current);
                    }
                }
                if (list.Count != 0)
                {
                    if (list.Count == 1)
                    {
                        current = list2[0] as ListViewItem;
                        current.Selected = true;
                    }
                    bool isSameReason = false;
                    for (int i = 0; i < list.Count; i++)
                    {
                        bool flag2;
                        DERelationBizItem relItem = list[i] as DERelationBizItem;
                        if (i < (list.Count - 1))
                        {
                            flag2 = false;
                        }
                        else
                        {
                            flag2 = true;
                        }
                        if (!isSameReason)
                        {
                            FrmInputRemark remark = new FrmInputRemark(relItem.Id + "完成打印", true, flag2);
                            DialogResult result = remark.ShowDialog();
                            isSameReason = remark.IsSameReason;
                            string strMarkup = remark.StrMarkup;
                            if (isSameReason)
                            {
                                if (result != DialogResult.OK)
                                {
                                    int count = list.Count - i;
                                    list.RemoveRange(i, count);
                                }
                                else
                                {
                                    for (int j = i; j < list.Count; j++)
                                    {
                                        DERelationBizItem item4 = list[j] as DERelationBizItem;
                                        PlArchivManage.EndPrint(item4, strMarkup);
                                        PlArchivManage.UpdatePrintLvwRelValues(this.lvwPrintBom, this.lstOrder, item4);
                                        this._isChg = true;
                                    }
                                }
                                break;
                            }
                            if (result != DialogResult.OK)
                            {
                                list.RemoveAt(i);
                                i--;
                            }
                            else
                            {
                                PlArchivManage.EndPrint(relItem, strMarkup);
                                PlArchivManage.UpdatePrintLvwRelValues(this.lvwPrintBom, this.lstOrder, relItem);
                                this._isChg = true;
                            }
                        }
                    }
                    this.RefreshFormByDocId();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private string GetRelItemStatus(DERelationBizItem relItem)
        {
            object attrValue = relItem.Relation.GetAttrValue(ConstAm.TDSBOM_ATTR_TSSTATUE);
            return ((attrValue == null) ? "未打印" : attrValue.ToString());
        }

        private void InitializeComponent()
        {
            this.panel1 = new Panel();
            this.txtDesc = new TextBox();
            this.label7 = new Label();
            this.btnClose = new Button();
            this.btnPrintEnd = new Button();
            this.btnPrintCancel = new Button();
            this.txtStatus = new TextBox();
            this.label6 = new Label();
            this.txtNumZs = new TextBox();
            this.label5 = new Label();
            this.txtNumFs = new TextBox();
            this.label4 = new Label();
            this.txtDocRev = new TextBox();
            this.txtDocName = new TextBox();
            this.label3 = new Label();
            this.label2 = new Label();
            this.txtDocCode = new TextBox();
            this.label1 = new Label();
            this.lvwPrintBom = new SortableListView();
            this.panel1.SuspendLayout();
            base.SuspendLayout();
            this.panel1.Controls.Add(this.txtDesc);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Controls.Add(this.btnPrintEnd);
            this.panel1.Controls.Add(this.btnPrintCancel);
            this.panel1.Controls.Add(this.txtStatus);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.txtNumZs);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.txtNumFs);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.txtDocRev);
            this.panel1.Controls.Add(this.txtDocName);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.txtDocCode);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = DockStyle.Bottom;
            this.panel1.Location = new Point(0, 0x131);
            this.panel1.Name = "panel1";
            this.panel1.Size = new Size(0x2b9, 0x7a);
            this.panel1.TabIndex = 0;
            this.txtDesc.Location = new Point(0x3e, 80);
            this.txtDesc.MaxLength = 500;
            this.txtDesc.Name = "txtDesc";
            this.txtDesc.ReadOnly = true;
            this.txtDesc.Size = new Size(0x1ca, 0x15);
            this.txtDesc.TabIndex = 0x10;
            this.label7.AutoSize = true;
            this.label7.Location = new Point(3, 0x55);
            this.label7.Name = "label7";
            this.label7.Size = new Size(0x35, 12);
            this.label7.TabIndex = 15;
            this.label7.Text = "发放说明";
            this.btnClose.Anchor = AnchorStyles.Right;
            this.btnClose.Location = new Point(0x255, 0x4c);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(0x4b, 0x17);
            this.btnClose.TabIndex = 14;
            this.btnClose.Text = "关  闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            this.btnPrintEnd.Anchor = AnchorStyles.Right;
            this.btnPrintEnd.Location = new Point(0x255, 0x2e);
            this.btnPrintEnd.Name = "btnPrintEnd";
            this.btnPrintEnd.Size = new Size(0x4b, 0x17);
            this.btnPrintEnd.TabIndex = 13;
            this.btnPrintEnd.Text = "打印完成";
            this.btnPrintEnd.UseVisualStyleBackColor = true;
            this.btnPrintEnd.Click += new EventHandler(this.btnPrintEnd_Click);
            this.btnPrintCancel.Anchor = AnchorStyles.Right;
            this.btnPrintCancel.Location = new Point(0x255, 0x10);
            this.btnPrintCancel.Name = "btnPrintCancel";
            this.btnPrintCancel.Size = new Size(0x4b, 0x17);
            this.btnPrintCancel.TabIndex = 12;
            this.btnPrintCancel.Text = "取消打印";
            this.btnPrintCancel.UseVisualStyleBackColor = true;
            this.btnPrintCancel.Click += new EventHandler(this.btnPrintCancel_Click);
            this.txtStatus.Location = new Point(0x3e, 0x30);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.Size = new Size(0x8f, 0x15);
            this.txtStatus.TabIndex = 11;
            this.label6.AutoSize = true;
            this.label6.Location = new Point(0x1b, 0x33);
            this.label6.Name = "label6";
            this.label6.Size = new Size(0x1d, 12);
            this.label6.TabIndex = 10;
            this.label6.Text = "状态";
            this.txtNumZs.Location = new Point(420, 0x2d);
            this.txtNumZs.Name = "txtNumZs";
            this.txtNumZs.ReadOnly = true;
            this.txtNumZs.Size = new Size(100, 0x15);
            this.txtNumZs.TabIndex = 9;
            this.label5.AutoSize = true;
            this.label5.Location = new Point(0x181, 0x33);
            this.label5.Name = "label5";
            this.label5.Size = new Size(0x1d, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "张数";
            this.txtNumFs.Location = new Point(0xf6, 0x30);
            this.txtNumFs.Name = "txtNumFs";
            this.txtNumFs.ReadOnly = true;
            this.txtNumFs.Size = new Size(0x85, 0x15);
            this.txtNumFs.TabIndex = 7;
            this.label4.AutoSize = true;
            this.label4.Location = new Point(0xd3, 0x33);
            this.label4.Name = "label4";
            this.label4.Size = new Size(0x1d, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "份数";
            this.txtDocRev.Location = new Point(420, 14);
            this.txtDocRev.Name = "txtDocRev";
            this.txtDocRev.ReadOnly = true;
            this.txtDocRev.Size = new Size(100, 0x15);
            this.txtDocRev.TabIndex = 5;
            this.txtDocName.Location = new Point(0xf6, 0x10);
            this.txtDocName.Name = "txtDocName";
            this.txtDocName.ReadOnly = true;
            this.txtDocName.Size = new Size(0x85, 0x15);
            this.txtDocName.TabIndex = 4;
            this.label3.AutoSize = true;
            this.label3.Location = new Point(0x181, 0x11);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x1d, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "版本";
            this.label2.AutoSize = true;
            this.label2.Location = new Point(0xd3, 0x13);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x1d, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "名称";
            this.txtDocCode.Location = new Point(0x3e, 14);
            this.txtDocCode.Name = "txtDocCode";
            this.txtDocCode.ReadOnly = true;
            this.txtDocCode.Size = new Size(0x8f, 0x15);
            this.txtDocCode.TabIndex = 1;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0x1b, 0x15);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x1d, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "图号";
            this.lvwPrintBom.Dock = DockStyle.Fill;
            this.lvwPrintBom.FullRowSelect = true;
            this.lvwPrintBom.HideSelection = false;
            this.lvwPrintBom.Location = new Point(0, 0);
            this.lvwPrintBom.Name = "lvwPrintBom";
            this.lvwPrintBom.Size = new Size(0x2b9, 0x131);
            this.lvwPrintBom.SortingOrder = SortOrder.None;
            this.lvwPrintBom.TabIndex = 1;
            this.lvwPrintBom.UseCompatibleStateImageBehavior = false;
            this.lvwPrintBom.View = View.Details;
            this.lvwPrintBom.SelectedIndexChanged += new EventHandler(this.lvwPrintBom_SelectedIndexChanged);
            this.lvwPrintBom.KeyUp += new KeyEventHandler(this.lvwPrintBom_KeyUp);
            base.AutoScaleDimensions = new SizeF(6f, 12f);
            base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            base.Controls.Add(this.lvwPrintBom);
            base.Controls.Add(this.panel1);
            base.Name = "UcPrintItem";
            base.Size = new Size(0x2b9, 0x1ab);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            base.ResumeLayout(false);
        }

        private void InitIvwRelItem()
        {
            this.hsCols = PlArchivManage.Agent.GetViewOfCol("打印明细", out this.lstOrder, out this.hsColWide);
            PlArchivManage.SetCol(this.hsCols, this.lvwPrintBom, "PrintBom", this.lstOrder, this.hsColWide);
            this.RefreshBoms();
        }

        private void InitUc()
        {
            if (this._theItem.Iteration.GetAttrValue("TSSTATUS").ToString() == "开始打印")
            {
                this.btnPrintCancel.Enabled = this.btnPrintEnd.Enabled = true;
                this._canEdit = true;
            }
            else
            {
                this.btnPrintCancel.Enabled = this.btnPrintEnd.Enabled = false;
            }
        }

        private void lvwPrintBom_KeyUp(object sender, KeyEventArgs e)
        {
            if ((this.lvwPrintBom.SelectedItems.Count != 0) && (this.lvwPrintBom.SelectedItems.Count <= 1))
            {
                int index = this.lvwPrintBom.SelectedItems[0].Index;
                if (e.KeyCode == Keys.Down)
                {
                    if (index < (this.lvwPrintBom.Items.Count - 1))
                    {
                        this.lvwPrintBom.Items[index].Selected = false;
                        this.lvwPrintBom.Items[index + 1].Selected = true;
                    }
                }
                else if ((e.KeyCode == Keys.Up) && (index != 0))
                {
                    this.lvwPrintBom.Items[index].Selected = false;
                    this.lvwPrintBom.Items[index - 1].Selected = true;
                }
            }
        }

        private void lvwPrintBom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.lvwPrintBom.SelectedItems.Count == 1)
            {
                ListViewItem item = this.lvwPrintBom.SelectedItems[0];
                DERelationBizItem tag = item.Tag as DERelationBizItem;
                this.RefreshForm(tag);
            }
        }

        private void RefreshBoms()
        {
            DERelationBizItemList relListOfDEBizItem = PlArchivManage.GetRelListOfDEBizItem(this._theItem, ConstAm.TDSBOM_RELCLASS);
            this.lvwPrintBom.Items.Clear();
            foreach (DERelationBizItem item in relListOfDEBizItem.RelationBizItems)
            {
                if (item.Relation.State != RelationState.Deleted)
                {
                    PlArchivManage.UpdatePrintLvwRelValues(this.lvwPrintBom, this.lstOrder, item);
                }
            }
        }

        private void RefreshForm(DERelationBizItem relItem)
        {
            this.txtDocCode.Text = relItem.Id;
            this.txtDocName.Text = relItem.BizItem.Name;
            this.txtDocRev.Text = (relItem.Relation.GetAttrValue("DOCREV") == null) ? "" : relItem.Relation.GetAttrValue("DOCREV").ToString();
            object attrValue = relItem.Relation.GetAttrValue("MTZS");
            this.txtNumZs.Text = (attrValue == null) ? "" : attrValue.ToString();
            attrValue = relItem.Relation.GetAttrValue("FS");
            this.txtNumFs.Text = (attrValue == null) ? "" : attrValue.ToString();
            attrValue = relItem.Relation.GetAttrValue(ConstAm.TDSBOM_ATTR_TSSTATUE);
            this.txtStatus.Text = (attrValue == null) ? "未打印" : attrValue.ToString();
            attrValue = relItem.Relation.GetAttrValue("FFSM");
            this.txtDesc.Text = (attrValue == null) ? "" : attrValue.ToString();
        }

        private void RefreshFormByDocId()
        {
            if (!string.IsNullOrEmpty(this.txtDocCode.Text))
            {
                foreach (ListViewItem item in this.lvwPrintBom.Items)
                {
                    DERelationBizItem tag = item.Tag as DERelationBizItem;
                    if (tag.Id == this.txtDocCode.Text)
                    {
                        this.RefreshForm(tag);
                    }
                }
            }
        }

        private void Save()
        {
            this._theItem.Iteration = PLItem.UpdateItemIterationDirectly(this._theItem, ClientData.LogonUser.Oid, true, ClientData.UserGlobalOption, false);
            if (BizItemHandlerEvent.Instance.D_AfterIterationUpdated != null)
            {
                BizItemHandlerEvent.Instance.D_AfterIterationUpdated(BizOperationHelper.ConvertPLMBizItemDelegateParam(this._theItem));
            }
        }
    }
}

