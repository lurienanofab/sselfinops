using GemBox.ExcelLite;
using System;

namespace sselFinOps.AppCode.Excel
{
    public class GemBoxExcelLiteManager : IExcelManager
    {
        private ExcelFile _workbook;

        public GemBoxExcelLiteManager()
        {
            ExcelLite.SetLicense("EL6N-Z669-AZZG-3LS7");
        }

        public void OpenWorkbook(string path)
        {
            _workbook = new ExcelFile();
            _workbook.LoadXls(path);
        }

        public void SaveAs(string path)
        {
            _workbook.SaveXls(path);
        }

        public void SetActiveWorksheet(string name)
        {
            _workbook.Worksheets.ActiveWorksheet = _workbook.Worksheets[name];
        }

        public void SetActiveWorksheet(int index)
        {
            _workbook.Worksheets.ActiveWorksheet = _workbook.Worksheets[index];
        }

        public void SetCellFormula(int row, int col, string value)
        {
            _workbook.Worksheets.ActiveWorksheet.Cells[row, col].Formula = value;
        }

        public void SetCellTextValue(int row, int col, object value)
        {
            var cell = _workbook.Worksheets.ActiveWorksheet.Cells[row, col];
            cell.Value = value;
        }

        public void SetCellTextValue(string cref, object value)
        {
            var cell = _workbook.Worksheets.ActiveWorksheet.Cells[cref];
            cell.Value = value;
        }

        public void SetCellNumberValue(int row, int col, object value)
        {
            var cell = _workbook.Worksheets.ActiveWorksheet.Cells[row, col];
            cell.Value = value;
        }

        public void SetCellNumberValue(string cref, object value)
        {
            var cell = _workbook.Worksheets.ActiveWorksheet.Cells[cref];
            cell.Value = value;
        }

        public void SetColumnCollapsed(int col, bool value)
        {
            _workbook.Worksheets.ActiveWorksheet.Columns[col].Collapsed = value;
        }

        public void SetColumnCollapsed(string cref, bool value)
        {
            _workbook.Worksheets.ActiveWorksheet.Columns[cref].Collapsed = value;
        }

        public void SetColumnWidth(int col, double value)
        {
            _workbook.Worksheets.ActiveWorksheet.Columns[col].Width = Convert.ToInt32(value);
        }

        public void SetColumnWidth(string cref, double value)
        {
            _workbook.Worksheets.ActiveWorksheet.Columns[cref].Width = Convert.ToInt32(value);
        }

        public void Dispose()
        {
            _workbook = null;
            GC.Collect();
        }
    }
}
