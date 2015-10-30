namespace DQArchivManageClt
{
    using NPOI.SS.UserModel;
    using System;
    using System.Collections;
    using System.Data;
    using System.IO;

    internal class OutPutExcel
    {
        public static string pathModel = string.Empty;

        internal static ArrayList FindLable(string fileName)
        {
            pathModel = fileName;
            ArrayList list = new ArrayList();
            using (FileStream stream = File.OpenRead(fileName))
            {
                ISheet sheetAt = WorkbookFactory.Create(stream).GetSheetAt(0);
                for (int i = sheetAt.FirstRowNum; i <= sheetAt.LastRowNum; i++)
                {
                    for (int j = sheetAt.GetRow(i).FirstCellNum; j <= sheetAt.GetRow(i).LastCellNum; j++)
                    {
                        ICell cell = sheetAt.GetRow(i).GetCell(j);
                        if (cell != null)
                        {
                            string str = cell.ToString();
                            if (str.StartsWith("<") && str.EndsWith("*>"))
                            {
                                list.Add(str.Substring(1, str.IndexOf('*') - 1).Trim());
                            }
                        }
                    }
                }
            }
            return list;
        }

        private static ICell FindLable(ISheet sheet, string label)
        {
            for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                for (int j = sheet.GetRow(i).FirstCellNum; j <= sheet.GetRow(i).LastCellNum; j++)
                {
                    ICell cell = sheet.GetRow(i).GetCell(j);
                    if ((cell != null) && (cell.ToString() == ("<" + label + "*>")))
                    {
                        return sheet.GetRow(i).GetCell(j);
                    }
                }
            }
            return null;
        }

        internal static void GetReportResult(DataSet ds, Hashtable hsLable, string fileName)
        {
            IWorkbook workbook;
            using (FileStream stream = File.OpenRead(pathModel))
            {
                workbook = WorkbookFactory.Create(stream);
                stream.Close();
            }
            ISheet sheetAt = workbook.GetSheetAt(0);
            foreach (DictionaryEntry entry in hsLable)
            {
                ICell cell = FindLable(sheetAt, (string) entry.Key);
                if (cell != null)
                {
                    cell.SetCellValue(entry.Value.ToString());
                }
            }
            ICell cell2 = FindLable(sheetAt, "dbof");
            FindLable(sheetAt, "deof").SetCellValue("");
            int count = ds.Tables[0].Columns.Count;
            int rowIndex = cell2.RowIndex;
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                IRow row2 = sheetAt.CreateRow(rowIndex++);
                for (int i = 0; i < count; i++)
                {
                    DataColumn column = row.Table.Columns[i];
                    if (row[i] != DBNull.Value)
                    {
                        if (column.DataType == Type.GetType("System.DateTime"))
                        {
                            row2.CreateCell(i).SetCellValue(Convert.ToDateTime(row[i]).ToShortDateString());
                        }
                        else if (((column.DataType == Type.GetType("System.Int16")) || (column.DataType == Type.GetType("System.Int32"))) || ((column.DataType == Type.GetType("System.Int64")) || (column.DataType == Type.GetType("System.Decimal"))))
                        {
                            row2.CreateCell(i).SetCellValue((double) Convert.ToInt32(row[i]));
                        }
                        else
                        {
                            row2.CreateCell(i).SetCellValue(row[i].ToString());
                        }
                    }
                }
            }
            using (FileStream stream2 = new FileStream(pathModel, FileMode.Create))
            {
                workbook.Write(stream2);
                stream2.Close();
            }
        }
    }
}

