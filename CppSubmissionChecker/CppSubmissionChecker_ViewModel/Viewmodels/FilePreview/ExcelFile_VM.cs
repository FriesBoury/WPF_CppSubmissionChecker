using System.Data;


using OfficeOpenXml;

namespace CppSubmissionChecker_ViewModel.Viewmodels.FilePreview
{
    public class ExcelFile_VM : FilePreviewBaseVM
    {
        public DataTable DataTable { get; set; }
        public ExcelFile_VM(string filePath) : base(filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage package = new ExcelPackage();
            
            package.Load(File.OpenRead(filePath));
            var ws = package.Workbook.Worksheets.First();


            DataTable = new DataTable();
            bool hasHeader = true;
            foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
            {
                DataTable.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
            }
            var startRow = hasHeader ? 2 : 1;
            for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
            {
                var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                var row = DataTable.NewRow();
                foreach (var cell in wsRow)
                {
                    int columnidx = cell.Start.Column - 1;
                    while (DataTable.Columns.Count <= columnidx)
                    {
                        DataTable.Columns.Add();
                    }
                    row[columnidx] = cell.Text;
                }
                DataTable.Rows.Add(row);
            }
            package.Dispose();
        }
    }
}
