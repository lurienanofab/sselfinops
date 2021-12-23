using SpreadsheetLight;
using System;

namespace sselFinOps.AppCode.Excel
{
    public class SpreadsheetLightManager : IExcelManager
    {
        private SLDocument _workbook;

        public void OpenWorkbook(string path)
        {
            _workbook = new SLDocument(path);
        }

        public void SaveAs(string path)
        {
            _workbook.SaveAs(path);
        }

        public void SetActiveWorksheet(string name)
        {
            _workbook.SelectWorksheet(name);
        }

        public void SetActiveWorksheet(int index)
        {
            var names = _workbook.GetWorksheetNames();
            SetActiveWorksheet(names[index]);
        }

        public void SetCellFormula(int row, int col, string value)
        {
            _workbook.SetCellValue(row + 1, col + 1, value);
        }

        public void SetCellTextValue(int row, int col, object value)
        {
            _workbook.SetCellValue(row + 1, col + 1, Convert.ToString(value));
        }

        public void SetCellTextValue(string cref, object value)
        {
            _workbook.SetCellValue(cref, Convert.ToString(value));
        }

        public void SetCellNumberValue(int row, int col, object value)
        {
            _workbook.SetCellValueNumeric(row + 1, col + 1, Convert.ToString(value));
        }

        public void SetCellNumberValue(string cref, object value)
        {
            _workbook.SetCellValueNumeric(cref, Convert.ToString(value));
        }

        public void SetColumnCollapsed(int col, bool value)
        {
            if (value)
                _workbook.CollapseColumns(col + 1);
            else
                _workbook.ExpandColumns(col + 1);
        }

        public void SetColumnCollapsed(string cref, bool value)
        {
            if (value)
                _workbook.CollapseColumns(cref);
            else
                _workbook.ExpandColumns(cref);
        }

        public void SetColumnWidth(int col, double value)
        {
            _workbook.SetColumnWidth(col + 1, value);
        }

        public void SetColumnWidth(string cref, double value)
        {
            _workbook.SetColumnWidth(cref, value);
        }

        public void Dispose()
        {
            _workbook.Dispose();
        }
    }
}
