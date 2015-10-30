namespace DQArchivManageClt
{
    using NPOI.SS.UserModel;
    using NPOI.SS.Util;
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using Thyt.TiPLM.DEL.Product;
    using Thyt.TiPLM.PLL.Admin.NewResponsibility;

    internal class TsdOutPut
    {
        private string _id;
        private DEBusinessItem _item;
        private string _name;
        private string _SM;
        private string _TSTYPE;
        private string _wk;
        private string _YCT;
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
                IWorkbook workbook;
                FileStream stream;
                int num5;
                string path = str + @"\plmtuoshaidan.xls";
                int num3 = drawingForTsOutPut.Tables[0].Rows.Count;
                int num4 = drawingForTsOutPut.Tables[0].Columns.Count;
                using (stream = File.OpenRead(path))
                {
                    workbook = WorkbookFactory.Create(stream);
                    stream.Close();
                }
                ISheet sheetAt = workbook.GetSheetAt(0);
                for (num5 = 0; num5 < sheetAt.NumMergedRegions; num5++)
                {
                    CellRangeAddress mergedRegion = sheetAt.GetMergedRegion(num5);
                    switch (sheetAt.GetRow(mergedRegion.FirstRow).GetCell(mergedRegion.FirstColumn).ToString())
                    {
                        case "$liuchengmingcheng":
                            sheetAt.GetRow(mergedRegion.FirstRow).GetCell(mergedRegion.FirstColumn).SetCellValue(this._wk);
                            break;

                        case "$name":
                            sheetAt.GetRow(mergedRegion.FirstRow).GetCell(mergedRegion.FirstColumn).SetCellValue(this._name);
                            break;

                        case "$tsfs":
                            sheetAt.GetRow(mergedRegion.FirstRow).GetCell(mergedRegion.FirstColumn).SetCellValue(this._TSTYPE);
                            break;

                        case "$ttfs":
                            sheetAt.GetRow(mergedRegion.FirstRow).GetCell(mergedRegion.FirstColumn).SetCellValue(this._YCT);
                            break;

                        case "$sm":
                            sheetAt.GetRow(mergedRegion.FirstRow).GetCell(mergedRegion.FirstColumn).SetCellValue(this._SM);
                            break;
                    }
                }
                for (num5 = 0; num5 < 5; num5++)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        switch (sheetAt.GetRow(num5).GetCell(i).ToString())
                        {
                            case "$tsfs":
                                sheetAt.GetRow(num5).GetCell(i).SetCellValue(this._TSTYPE);
                                break;

                            case "$ttfs":
                                sheetAt.GetRow(num5).GetCell(i).SetCellValue(this._YCT);
                                break;

                            case "$SM":
                                sheetAt.GetRow(num5).GetCell(i).SetCellValue(this._SM);
                                break;
                        }
                    }
                }
                int num7 = 5;
                int num8 = 1;
                int num9 = 0;
                short num10 = 460;
                ICellStyle style = workbook.CreateCellStyle();
                style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Medium;
                style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Medium;
                style.BorderRight = NPOI.SS.UserModel.BorderStyle.Medium;
                style.BorderTop = NPOI.SS.UserModel.BorderStyle.Medium;
                style.VerticalAlignment = VerticalAlignment.Center;
                style.WrapText = true;
                foreach (DataRow row in drawingForTsOutPut.Tables[0].Rows)
                {
                    IRow row2 = sheetAt.CreateRow(num7++);
                    row2.Height = (short) (num10 * ((Encoding.Default.GetByteCount(row[5].ToString()) / 40) + 1));
                    ICell cell = row2.CreateCell(0);
                    cell.SetCellValue((double) num8++);
                    cell.CellStyle = style;
                    cell = row2.CreateCell(1);
                    cell.SetCellValue(row[0].ToString());
                    cell.CellStyle = style;
                    cell = row2.CreateCell(2);
                    cell.SetCellValue(row[2].ToString());
                    cell.CellStyle = style;
                    cell = row2.CreateCell(3);
                    cell.SetCellValue(row[3].ToString());
                    cell.CellStyle = style;
                    cell.CellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    cell = row2.CreateCell(4);
                    cell.SetCellValue(row[4].ToString());
                    cell.CellStyle = style;
                    cell.CellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    cell = row2.CreateCell(5);
                    cell.SetCellValue(row[5].ToString());
                    cell.CellStyle = style;
                    string str4 = row.Table.Columns[3].ToString();
                    num9 += Convert.ToInt32(row[3].ToString());
                }
                sheetAt.CreateRow(num7 + 2).CreateCell(1).SetCellValue("合计" + num9 + " 张");
                sheetAt.GetRow(num7 + 2).CreateCell(3).SetCellValue("计划签字:");
                sheetAt.GetRow(num7 + 2).CreateCell(4).SetCellValue(PLUser.Agent.GetUserByOid(this._item.Creator).Name);
                sheetAt.GetRow(num7 + 2).CreateCell(5).SetCellValue(DateTime.Now.ToShortDateString());
                using (stream = new FileStream("C:/a.xls", FileMode.Create))
                {
                    workbook.Write(stream);
                    stream.Close();
                }
                Process.Start("C:/a.xls");
                e.Result = "报表已生成...请保存...";
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
            this._YCT = (item.Iteration.GetAttrValue("YCT") == null) ? "" : item.Iteration.GetAttrValue("YCT").ToString();
            this._SM = (item.Iteration.GetAttrValue("SM") == null) ? "" : item.Iteration.GetAttrValue("SM").ToString();
            this._TSTYPE = (item.Iteration.GetAttrValue("TSTYPE") == null) ? "" : item.Iteration.GetAttrValue("TSTYPE").ToString();
        }
    }
}

