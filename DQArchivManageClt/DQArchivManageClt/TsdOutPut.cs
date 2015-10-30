namespace DQArchivManageClt
{
    using Microsoft.Office.Interop.Excel;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Xml;
    using Thyt.TiPLM.DEL.Admin.DataModel;
    using Thyt.TiPLM.DEL.Product;
    using Thyt.TiPLM.PLL.Admin.DataModel;
    using Thyt.TiPLM.PLL.Admin.NewResponsibility;

    public class TsdOutPut
    {
        private string _id;
        private DEBusinessItem _item;
        private string _name;
        private string _wk;
        private readonly BackgroundWorker bkwMain = new BackgroundWorker();
        private FrmBar frmBar = null;
        private string[] sttrs = new string[] { "签章类型" };

        public TsdOutPut()
        {
            this.Init();
        }

        private void bkwMain_DoWork(object sender, DoWorkEventArgs e)
        {
            Guid argument = (Guid) e.Argument;
            DataSet drawingForTsOutPut = PlArchivManage.Agent.GetDrawingForTsOutPut(argument);
            int count = drawingForTsOutPut.Tables[0].Rows.Count;
            int length = Assembly.GetExecutingAssembly().Location.LastIndexOf(@"\");
            string str = Assembly.GetExecutingAssembly().Location.Substring(0, length);
            if (count == 0)
            {
                MessageBox.Show("未查询到任何数据！", "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                e.Result = "未查询到任何数据！...";
            }
            else
            {
                string str2 = str + @"\plmtuoshaidan.xls";
                ApplicationClass class2 = new ApplicationClass();
                Workbooks workbooks = class2.Workbooks;
                object obj2 = Missing.Value;
                IntPtr hwnd = new IntPtr(class2.Hwnd);
                int iD = 0;
                GetWindowThreadProcessId(hwnd, out iD);
                Process processById = Process.GetProcessById(iD);
                try
                {
                    Workbook workbook = workbooks.Open(str2, Type.Missing, false, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    Worksheet worksheet = (Worksheet) workbook.Worksheets.get_Item(1);
                    int num4 = drawingForTsOutPut.Tables[0].Rows.Count;
                    int num5 = drawingForTsOutPut.Tables[0].Columns.Count;
                    Range range = null;
                    Range range2 = null;
                    Range range3 = null;
                    Range range4 = null;
                    Range range5 = null;
                    Range range6 = null;
                    Range range7 = null;
                    Range range8 = null;
                    Range range9 = null;
                    Range range10 = null;
                    range = worksheet.get_Range("D2", obj2);
                    if (range.Text.ToString().Replace("$", "") == "liuchengmingcheng")
                    {
                        range.Value2=this._wk;
                    }
                    range10 = worksheet.get_Range("D3", obj2);
                    if (range10.Text.ToString().Replace("$", "") == "name")
                    {
                        range10.Value2 = this._name;
                    }
                    int num6 = 5;
                    int num7 = 0;
                    int num8 = 0;
                    for (int i = 0; i < num4; i++)
                    {
                        int num10 = drawingForTsOutPut.Tables[0].Rows.Count;
                        int num11 = num4 - i;
                        num6++;
                        try
                        {
                            range2 = worksheet.get_Range("A" + num6, obj2);
                            range2.Value2=i + 1;
                            range2.Cells.Borders[XlBordersIndex.xlEdgeTop].LineStyle = 1;
                            range2.Cells.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = 1;
                            if ((num4 - i) == 1)
                            {
                                range2.Cells.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = 1;
                            }
                            range3 = worksheet.get_Range("B" + num6, obj2);
                            range3.Value2=drawingForTsOutPut.Tables[0].Rows[i][0].ToString();
                            range3.Cells.Borders[XlBordersIndex.xlEdgeTop].LineStyle = 1;
                            range3.Cells.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = 1;
                            if ((num4 - i) == 1)
                            {
                                range3.Cells.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = 1;
                            }
                            range4 = worksheet.get_Range("C" + num6, obj2);
                            range4.Value2=drawingForTsOutPut.Tables[0].Rows[i][2].ToString();
                            range4.Cells.Borders[XlBordersIndex.xlEdgeTop].LineStyle = 1;
                            range4.Cells.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = 1;
                            if ((num4 - i) == 1)
                            {
                                range4.Cells.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = 1;
                            }
                            range5 = worksheet.get_Range("D" + num6, obj2);
                            range5.Value2=drawingForTsOutPut.Tables[0].Rows[i][3].ToString();
                            range5.Cells.Borders[XlBordersIndex.xlEdgeTop].LineStyle = 1;
                            range5.Cells.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = 1;
                            if ((num4 - i) == 1)
                            {
                                range5.Cells.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = 1;
                            }
                            num7 += Convert.ToInt32(drawingForTsOutPut.Tables[0].Rows[i][3].ToString());
                            num8 = num7;
                            range6 = worksheet.get_Range("E" + num6, obj2);
                            range6.Value2=drawingForTsOutPut.Tables[0].Rows[i][4].ToString();
                            range6.Cells.Borders[XlBordersIndex.xlEdgeTop].LineStyle = 1;
                            range6.Cells.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = 1;
                            if ((num4 - i) == 1)
                            {
                                range6.Cells.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = 1;
                            }
                            range7 = worksheet.get_Range("F" + num6, obj2);
                            range7.Value2=drawingForTsOutPut.Tables[0].Rows[i][5].ToString();
                            range7.Cells.Borders[XlBordersIndex.xlEdgeTop].LineStyle = 1;
                            range7.Cells.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = 1;
                            range7.Cells.Borders[XlBordersIndex.xlEdgeRight].LineStyle = 1;
                            if ((num4 - i) == 1)
                            {
                                range7.Cells.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = 1;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    worksheet.get_Range("B" + (num6 + 2), obj2).Value2 = "合计" + num8 + " 张";
                    range8 = worksheet.get_Range("C" + (num6 + 2), obj2);
                    range8.Font.Size = "12";
                    range8.Value2 = "计划签字";
                    range9 = worksheet.get_Range("D" + (num6 + 2), obj2);
                    range9.Font.Size = "12";
                    range9.Font.Bold = true;
                    range9.ShrinkToFit= true;
                    range9.Value2 = PLUser.Agent.GetUserByOid(this._item.Creator).Name;
                    string path = str + @"\托晒单输出设置.xlm";
                    if (File.Exists(path))
                    {
                        ArrayList attributes = ModelContext.MetaModel.GetAttributes(this._item.ClassName);
                        XmlDocument document = new XmlDocument();
                        document.Load(path);
                        foreach (XmlElement element in document.DocumentElement.ChildNodes)
                        {
                            string str4 = element.GetAttribute("AttrLabel");
                            string str5 = element.GetAttribute("Address");
                            foreach (DEMetaAttribute attribute in attributes)
                            {
                                if (str4 == attribute.Label)
                                {
                                    Range range11 = worksheet.get_Range(str5 + (num6 + 3), obj2);
                                    range11.Font.Size = 12;
                                    range11.Value2 = str4;
                                    (range11.Cells[range11.Row, range11.Column + 1] as Range).Value2=(this._item.Iteration.GetAttrValue(attribute.Name) == null) ? "" : this._item.Iteration.GetAttrValue(attribute.Name).ToString();
                                }
                            }
                        }
                    }
                    class2.DisplayAlerts = false;
                    workbook.SaveAs(str + @"\plmtuoshaidan.1.xls", obj2, obj2, obj2, obj2, obj2, XlSaveAsAccessMode.xlExclusive, obj2, obj2, obj2, obj2, obj2);
                }
                catch (Exception exception)
                {
                    throw exception;
                }
                finally
                {
                    processById.Kill();
                    object obj3 = Missing.Value;
                    ApplicationClass application = new ApplicationClass();
                    application.Application.Workbooks.Open(str + @"\plmtuoshaidan.1.xls", obj3, obj3, obj3, obj3, obj3, obj3, obj3, obj3, obj3, obj3, obj3, obj3, obj3, obj3);
                    application.Visible = true;
                    e.Result = "报表已生成...请保存...";
                }
            }
        }

        private void bkwMain_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    MessageBox.Show("托晒单" + this._id + "输出完成");
                }
                else
                {
                    this.frmBar.ToClose(false);
                    FrmArchivManage.frmMian.DisplayTextInRichtBox("输出托晒单" + this._id + "错误\r\n\t" + e.Error.Message, 0, true);
                }
                this.frmBar.ToClose(true);
            }
            finally
            {
                this.frmBar.ToClose();
            }
        }

        [DllImport("User32.dll", CharSet=CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        private void Init()
        {
            this.bkwMain.WorkerReportsProgress = true;
            this.bkwMain.DoWork += new DoWorkEventHandler(this.bkwMain_DoWork);
            this.bkwMain.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bkwMain_RunWorkerCompleted);
        }

        public void StartOutPut(Guid iroid, DEBusinessItem item, string wk)
        {
            this._name = item.Name;
            this._id = item.Id;
            this._item = item;
            this._wk = wk;
            this.frmBar = new FrmBar(this.bkwMain);
            this.bkwMain.RunWorkerAsync(iroid);
            this.frmBar.ToAStart();
        }
    }
}

