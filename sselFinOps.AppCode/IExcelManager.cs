using System;

namespace sselFinOps.AppCode
{
    public interface IExcelManager : IDisposable
    {
        void OpenWorkbook(string path);

        void SetActiveWorksheet(string name);

        void SetActiveWorksheet(int index);

        void SetCellTextValue(int row, int col, object value);

        void SetCellTextValue(string cref, object value);

        void SetCellNumberValue(int row, int col, object value);

        void SetCellNumberValue(string cref, object value);

        void SetCellFormula(int row, int col, string value);

        void SetColumnCollapsed(int col, bool value);

        void SetColumnCollapsed(string cref, bool value);

        void SetColumnWidth(int col, double value);

        void SetColumnWidth(string cref, double value);

        void SaveAs(string path);
    }
}
