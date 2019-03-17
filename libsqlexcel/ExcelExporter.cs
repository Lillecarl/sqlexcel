using System.Data;
using OfficeOpenXml;

namespace libsqlexcel
{
    public class ExcelExporter
    {
        public void Export(DataTable dataTable, string filename)
        {
            if (System.IO.File.Exists(filename))
                System.IO.File.Delete(filename);

            using (var pack = new ExcelPackage(new System.IO.FileInfo(filename)))
            {
                var sheet = pack.Workbook.Worksheets.Add("data");
                sheet.Cells["A1"].LoadFromDataTable(dataTable, true);
                sheet.Cells[sheet.Dimension.Address].AutoFilter = true;
                pack.Save();
            }
        }
    }
}