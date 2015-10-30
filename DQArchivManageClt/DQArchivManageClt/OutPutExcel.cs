namespace DQArchivManageClt
{
    using Microsoft.Office.Interop.Excel;
    using System;
    using System.Collections;
    using System.Data;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;
    using Thyt.TiPLM.Common;

    public class OutPutExcel
    {
        private Range bottom;
        private _Worksheet curSheet;
        private DataSet ds;
        private ApplicationClass ExcelApp;
        //private ApplicationClass ExcelApp;
        private Hashtable hsLable;
        private Hashtable hsObjInfo = new Hashtable();
        private Hashtable hsPartClass = null;
        private Hashtable hsPicture = new Hashtable();
        private Hashtable hsPS = new Hashtable();
        private Hashtable hsRootPicRange = new Hashtable();
        private int i_page;
        private int i_page_num;
        private bool IsAddPics;
        private bool IsFindPictureSite = false;
        private bool IsSplitPage = true;
        private readonly ArrayList lstAttr = new ArrayList();
        private readonly object missing = Missing.Value;
        private readonly string pathModel;
        private Process pnow;
        private Range rEnd;
        private Range rNextPage;
        private int rowNumOfPage;
        public static OutPutExcel rpt = null;
        private Range rStart;
        private StringBuilder strErr;
        private Workbook workbook;

        public OutPutExcel(string path)
        {
            this.pathModel = path;
            try
            {
                this.Init();
            }
            catch
            {
                this.KillExcel();
            }
        }

        private void ClearSign()
        {
            Range range = this.curSheet.get_Range("A1", this.missing);
            ArrayList list = new ArrayList();
            range.Select();
            for (range = this.curSheet.Cells.Find("<*>", range, (XlFindLookIn)(-4123), (XlLookAt)2, (XlSearchOrder)1, XlSearchDirection.xlNext, this.missing, this.missing, this.missing); range != null; range = this.curSheet.Cells.FindNext(range))
            {
                string item = range.get_Address(this.missing, this.missing, XlReferenceStyle.xlA1, this.missing, this.missing);
                if (list.Contains(item))
                {
                    break;
                }
                list.Add(item);
                if (range.Value2.ToString().Trim().IndexOf("<**") == -1)
                {
                    range.Value2="";
                }
            }
        }

        private void CopyModelPage(int i_PageNum)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 1;
            int num4 = 0;
            Range range = this.curSheet.get_Range("A1", this.missing);
            string str = "";
            range.Select();
            range = this.curSheet.Cells.Find("<page*>", range, (XlFindLookIn) (-4123), (XlLookAt) 2, (XlSearchOrder) 1, XlSearchDirection.xlNext, this.missing, this.missing, this.missing);
            if (range != null)
            {
                num = num4 = range.Row;
                num2 = range.Column;
                str = string.Concat(new object[] { "第 ", num3, " 页   共 ", this.i_page, " 页" });
                range.Value2 = str;
            }
            this.ClearSign();
            Range range2 = this.curSheet.get_Range("A1", this.bottom);
            if (i_PageNum > 1)
            {
                this.ExcelApp.CutCopyMode = XlCutCopyMode.xlCopy; ;
                range2.Copy(this.missing);
                ((Range) this.ExcelApp.Rows["1:" + this.rowNumOfPage,this.missing]).Copy(this.missing);
            }
            for (int i = 1; i < i_PageNum; i++)
            {
                Range range3 = this.curSheet.get_Range("A" + ((this.bottom.Row * i) + 1), this.missing);
                this.curSheet.HPageBreaks.Add(range3);
                string str2 = Convert.ToString((int) ((i * this.rowNumOfPage) + 1));
                this.curSheet.get_Range("A" + str2, this.missing).Select();
                this.curSheet.Paste(this.missing, this.missing);
                if (range != null)
                {
                    num3 = i + 1;
                    str = string.Concat(new object[] { "第 ", num3, " 页   共 ", this.i_page, " 页" });
                    num = num4 + (this.bottom.Row * i);
                    ((Range) this.curSheet.Cells[num, num2]).Value2 = str ;
                }
            }
            this.curSheet.PageSetup.PrintArea = "";
        }

        private void FillReportObjectInfo()
        {
            try
            {
                Range range = null;
                IDictionaryEnumerator enumerator = this.hsLable.GetEnumerator();
                for (int i = 1; i <= this.workbook.Sheets.Count; i++)
                {
                    _Worksheet worksheet = (_Worksheet) this.workbook.Sheets[i];
                    if (worksheet != null)
                    {
                        this.curSheet = worksheet;
                    }
                    else
                    {
                        continue;
                    }
                    this.curSheet.Activate();
                    enumerator.Reset();
                    while (enumerator.MoveNext())
                    {
                        string lableName = enumerator.Key.ToString();
                        range = this.FindLableRange(lableName);
                        if (range != null)
                        {
                            string str2 = enumerator.Value.ToString();
                            range.Value2 = "'" + enumerator.Value;
                        }
                    }
                    this.ClearSign();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                PLMEventLog.WriteExceptionLog("报表图片签字", exception);
            }
        }

        private void FillReportResult()
        {
            this.FillResultByArray();
        }

        private void FillResultByArray()
        {
            int num = 0;
            object[,] objArray = null;
            int count = this.ds.Tables[0].Columns.Count;
            Range range = null;
            ArrayList list = new ArrayList();
            for (int i = 0; i < this.i_page; i++)
            {
                DataRow row;
                int num4 = (i * this.i_page_num) + 1;
                list.Clear();
                int num5 = 1;
                while (num5 <= this.i_page_num)
                {
                    num = (i * this.i_page_num) + num5;
                    if (num > this.ds.Tables[0].Rows.Count)
                    {
                        num--;
                        break;
                    }
                    row = this.ds.Tables[0].Rows[num - 1];
                    list.Add(row);
                    num5++;
                }
                int num6 = num;
                objArray = new object[(num6 - num4) + 1, count];
                ArrayList list2 = new ArrayList();
                for (int j = 0; j < list.Count; j++)
                {
                    row = list[j] as DataRow;
                    for (num5 = 0; num5 < count; num5++)
                    {
                        DataColumn column = row.Table.Columns[num5];
                        if (row[num5] != DBNull.Value)
                        {
                            if (column.DataType == Type.GetType("System.DateTime"))
                            {
                                objArray[j, num5] = Convert.ToDateTime(row[num5]).ToShortDateString();
                            }
                            else if ((((column.DataType == Type.GetType("System.Int16")) || (column.DataType == Type.GetType("System.Int32"))) || ((column.DataType == Type.GetType("System.Int64")) || (column.DataType == Type.GetType("System.Decimal")))) || (column.DataType == Type.GetType("System.Double")))
                            {
                                objArray[j, num5] = row[num5];
                            }
                            else
                            {
                                objArray[j, num5] = "'" + row[num5];
                            }
                        }
                    }
                }
                range = (Range) this.curSheet.Cells[(i * this.rowNumOfPage) + this.rStart.Row, this.rStart.Column];
                range.Resize[list.Count, count].Value2 =objArray;
            }
        }

        public static ArrayList FindLable(string path)
        {
            rpt = new OutPutExcel(path);
            return rpt.lstAttr;
        }

        private Range FindLableRange(string lableName)
        {
            string str;
            if (lableName.IndexOf("*>") == -1)
            {
                str = "<" + lableName + "*>";
            }
            else
            {
                str = lableName;
            }
            return this.curSheet.Cells.Find(str, this.missing, this.missing, this.missing, this.missing, XlSearchDirection.xlNext, this.missing, this.missing, this.missing);
        }

        private Range GetMergeRangeBottom(Range obj)
        {
            string str = obj.get_Address(true, true, XlReferenceStyle.xlA1, this.missing, this.missing);
            if (str.IndexOf(":") != -1)
            {
                str = str.Substring(str.IndexOf(":") + 1);
            }
            return this.curSheet.Cells.get_Range(str, this.missing);
        }

        public static void GetReportResult(DataSet ds, Hashtable hsLable, string pathModel)
        {
            rpt.ShowResult(ds, hsLable);
            rpt.ExcelApp.ActiveWorkbook.Save();
            rpt.ExcelApp.ActiveWorkbook.Close(Missing.Value, Missing.Value, Missing.Value);
            rpt.KillExcel();
        }

        public void Init()
        {
            Exception exception;
            DateTime now = DateTime.Now;
            TimeSpan zero = TimeSpan.Zero;
            int num = 0;
            try
            {
                this.ExcelApp = new ApplicationClass();
                this.ExcelApp.Visible = false;
                ArrayList list = new ArrayList();
                foreach (Process process in Process.GetProcessesByName("EXCEL"))
                {
                    list.Add(process);
                }
                foreach (Process process2 in list)
                {
                    TimeSpan span2 = (TimeSpan) (now - process2.StartTime);
                    if (num == 0)
                    {
                        zero = span2;
                        this.pnow = process2;
                    }
                    else if (span2 < zero)
                    {
                        zero = span2;
                        this.pnow = process2;
                    }
                    num++;
                }
            }
            catch (Exception exception2)
            {
                exception = exception2;
                MessageBox.Show(exception.Message);
                throw exception;
            }
            try
            {
                this.workbook = this.ExcelApp.Workbooks.Open(this.pathModel, this.missing, this.missing, this.missing, this.missing, this.missing, this.missing, this.missing, this.missing, this.missing, this.missing, this.missing, this.missing, this.missing, this.missing);
                int i = 1;
                for (i = 1; i <= this.workbook.Sheets.Count; i++)
                {
                    _Worksheet worksheet = (_Worksheet) this.workbook.Sheets[i];
                    if (worksheet != null)
                    {
                        this.curSheet = worksheet;
                    }
                    else
                    {
                        continue;
                    }
                    this.curSheet.Activate();
                    this.ExcelApp.ActiveWindow.View = XlWindowView.xlNormalView;
                    Range range = this.curSheet.get_Range("A1", this.missing);
                    range = this.curSheet.get_Range("A1", this.missing);
                    range.Select();
                    range = this.curSheet.Cells.Find("<*>", range, (XlFindLookIn) (-4123), (XlLookAt) 2, (XlSearchOrder) 1, XlSearchDirection.xlNext, this.missing, this.missing, this.missing);
                    string str = (range == null) ? "" : range.get_Address(this.missing, this.missing, XlReferenceStyle.xlA1, this.missing, this.missing);
                    string item = "";
                    while (range != null)
                    {
                        item = range.Value2.ToString().Trim();
                        if (((item.IndexOf(".") == -1) && item.StartsWith("<")) && item.EndsWith("*>"))
                        {
                            item = item.Remove(0, "<".Length);
                            int startIndex = item.LastIndexOf("*>");
                            item = item.Remove(startIndex, "*>".Length).Trim().ToUpper();
                            if (!this.lstAttr.Contains(item))
                            {
                                this.lstAttr.Add(item);
                            }
                        }
                        range = this.curSheet.Cells.FindNext(range);
                        if ((range != null) && (range.get_Address(this.missing, this.missing, XlReferenceStyle.xlA1, this.missing, this.missing) == str))
                        {
                            break;
                        }
                    }
                    if (i == 1)
                    {
                        if (!this.IsFindPictureSite)
                        {
                            this.IsAddPics = false;
                        }
                        this.strErr = new StringBuilder();
                        this.rStart = this.FindLableRange("dbof");
                        this.rEnd = this.FindLableRange("deof");
                        this.rNextPage = this.FindLableRange("aeof");
                        if (this.rNextPage == null)
                        {
                            this.IsSplitPage = false;
                        }
                        if (this.rEnd == null)
                        {
                            throw new Exception("模版上缺少分页标识苻<deof*>");
                        }
                        if (this.rStart == null)
                        {
                            throw new Exception("模版上缺少数据填充起始标识符<dbof*>");
                        }
                        if ((this.rNextPage != null) && ((bool) this.rNextPage.MergeCells))
                        {
                            this.bottom = this.GetMergeRangeBottom(this.rNextPage);
                            this.rowNumOfPage = this.bottom.Row;
                        }
                        else if (this.rNextPage != null)
                        {
                            this.rowNumOfPage = this.rNextPage.Row;
                            this.bottom = this.rNextPage;
                        }
                    }
                }
                this.curSheet = (_Worksheet) this.workbook.Sheets[i];
                this.curSheet.Activate();
            }
            catch (Exception exception3)
            {
                exception = exception3;
                MessageBox.Show(exception.Message);
                throw exception;
            }
        }

        private void InitModel()
        {
            Range range = this.curSheet.get_Range("A1", this.missing);
            range = this.curSheet.get_Range("A1", this.missing);
            range.Select();
            range = this.curSheet.Cells.Find("<now*>", range, (XlFindLookIn)(-4123), (XlLookAt)2, (XlSearchOrder)1, XlSearchDirection.xlNext, this.missing, this.missing, this.missing);
            if (range != null)
            {
                range.Value2 = DateTime.Today.ToString("yyyy/MM/dd");
            }
            if (this.hsLable.Count > 0)
            {
                this.FillReportObjectInfo();
            }
            this.curSheet = (_Worksheet) this.workbook.Sheets[1];
            this.curSheet.Activate();
            this.i_page_num = (this.rEnd.Row - this.rStart.Row) + 1;
            int count = this.ds.Tables[0].Rows.Count;
            this.i_page = ((count % this.i_page_num) == 0) ? (count / this.i_page_num) : ((count / this.i_page_num) + 1);
            if (!this.IsSplitPage)
            {
                this.InsertPage(this.i_page);
            }
            else
            {
                this.CopyModelPage(this.i_page);
            }
        }

        private void InsertPage(int i_PageNum)
        {
            Range range = this.curSheet.get_Range("A1", this.missing);
            range.Select();
            range = this.curSheet.Cells.Find("<page*>", range, (XlFindLookIn)(-4123), (XlLookAt)2, (XlSearchOrder)1, XlSearchDirection.xlNext, this.missing, this.missing, this.missing);
            if (range != null)
            {
                range.Value2 = "共 一 页";
            }
            this.ClearSign();
            this.curSheet.get_Range(this.rStart, this.rEnd).Rows.Select();
            string str = this.rStart.Row + ":" + this.rEnd.Row;
            string str2 = "";
            int num = 0;
            for (int i = 1; i < i_PageNum; i++)
            {
                this.ExcelApp.CutCopyMode = XlCutCopyMode.xlCopy;
                if (i == (i_PageNum - 1))
                {
                    int num3 = this.ds.Tables[0].Rows.Count % ((this.rEnd.Row - this.rStart.Row) + 1);
                    if (num3 != 0)
                    {
                        num3 += this.rStart.Row - 1;
                        str = this.rStart.Row + ":" + num3;
                    }
                }
                ((Range) this.curSheet.Rows[str, this.missing]).Select();
                ((Range) this.ExcelApp.Selection).Copy(this.missing);
                num = (this.rEnd.Row + ((this.rEnd.Row - this.rStart.Row) * (i - 1))) + 1;
                str2 = num + ":" + num;
                ((Range) this.curSheet.Rows[str2, this.missing]).Select();
                ((Range) this.ExcelApp.Selection).Insert((XlInsertShiftDirection) (-4121), this.missing);
            }
            this.curSheet.PageSetup.PrintArea = "";
            this.i_page = 1;
            this.i_page_num = i_PageNum * this.i_page_num;
        }

        private void KillExcel()
        {
            if (this.pnow != null)
            {
                this.pnow.Kill();
            }
        }

        public void ShowResult(DataSet dsReult, Hashtable hsLables)
        {
            this.ds = dsReult;
            this.hsLable = hsLables;
            try
            {
                this.InitModel();
                this.FillReportResult();
                this.ExcelApp.Visible = true;
            }
            catch (Exception exception)
            {
                this.KillExcel();
                MessageBox.Show(exception.Message);
                throw exception;
            }
        }
    }
}

