namespace DQArchivManageClt
{
    using Infragistics.Win;
    using Infragistics.Win.UltraWinEditors;
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Windows.Forms;
    using Thyt.TiPLM.UIL.Common;

    public class ResWkInfo : UltraTextEditor
    {
        private IContainer components;
        private SelectResHandler2 dlhandler;
        private DataSet ds;
        private UcResWk ucBpm;

        public ResWkInfo()
        {
            this.components = null;
            this.InitializeComponent();
            this.Init();
        }

        public ResWkInfo(IContainer container)
        {
            this.components = null;
            container.Add(this);
            this.InitializeComponent();
        }

        public ResWkInfo(string bpmName)
        {
            this.components = null;
            this.InitializeComponent();
            this.Init();
            this.Text = bpmName;
        }

        protected override void Dispose(bool disposing)
        {
            this.ucBpm.ResTextChanged -= this.dlhandler;
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Init()
        {
            this.ds = PlArchivManage.Agent.GetBpmNameByUserOid(ClientData.LogonUser.Oid);
            this.ucBpm = new UcResWk(this.ds);
            DropDownEditorButton button = base.ButtonsRight["SelectRes"] as DropDownEditorButton;
            button.Control = this.ucBpm;
            this.dlhandler = new SelectResHandler2(this.ucUser_ResSelected);
            this.ucBpm.ResTextChanged += this.dlhandler;
            base.BeforeEditorButtonDropDown += new BeforeEditorButtonDropDownEventHandler(this.ResCombo_BeforeDropDown);
            base.Resize += new EventHandler(this.ResCombo_Resize);
            base.TextChanged += new EventHandler(this.ResCombo_TextChanged);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            DropDownEditorButton button = new DropDownEditorButton("SelectRes");
            base.SuspendLayout();
            button.Key = "SelectRes";
            button.RightAlignDropDown = DefaultableBoolean.False;
            base.ButtonsRight.Add(button);
            base.NullText = "(无)";
            base.Size = new Size(100, 0x15);
            base.ResumeLayout(false);
        }

        private void ResCombo_BeforeDropDown(object sender, BeforeEditorButtonDropDownEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            this.ucBpm.Filter(this.ResValue);
            Cursor.Current = Cursors.Default;
        }

        private void ResCombo_Resize(object sender, EventArgs e)
        {
            if (this.ucBpm.Width < base.Width)
            {
                this.ucBpm.Width = base.Width;
            }
        }

        private void ResCombo_TextChanged(object sender, EventArgs e)
        {
            this.ucBpm.Filter(this.ResValue);
        }

        public void SetDataSource(DataSet ds)
        {
            this.ucBpm.SetDataSource(ds);
        }

        private void ucUser_ResSelected(string str_sel)
        {
            if (!(str_sel == this.ResValue))
            {
                this.Text = str_sel;
            }
        }

        public string ResValue
        {
            get
            {
                string str = this.Text.TrimEnd(new char[0]);
                if (str == "(无)")
                {
                    return "";
                }
                return str;
            }
            set
            {
                this.Text = value;
            }
        }
    }
}

