using System.Data;
using System.Collections.Generic;
using OfficeOpenXml;

namespace libsqlexcel
{
    public class ExcelExporter
    {
        public void Export(IEnumerable<DataTable> dataTables, string filename)
        {
            if (System.IO.File.Exists(filename))
                System.IO.File.Delete(filename);

            using (var pack = new ExcelPackage(new System.IO.FileInfo(filename)))
            {
                uint i = 1;
                foreach (var table in dataTables)
                {
                    var sheet = pack.Workbook.Worksheets.Add(i.ToString());
                    sheet.Cells["A1"].LoadFromDataTable(table, true);
                    sheet.Cells[sheet.Dimension.Address].AutoFilter = true;
                    ++i;
                }
                pack.Save();
            }
        }
    }
}