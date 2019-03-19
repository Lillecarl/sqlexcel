using System;
using System.IO;
using libsqlexcel;
using OfficeOpenXml;
using CommandLine;
using System.Collections.Generic;
using System.Data;

namespace sqlexcel
{
    public class Options
    {
        [Option(Required = true, HelpText = "Database server type")]
        public string dbtype { get; set; }
        [Option(Required = false, HelpText = "IP or hostname of database server")]
        public string host { get; set; }
        [Option(Required = false, HelpText = "Database username")]
        public string user { get; set; }
        [Option(Required = false, HelpText = "Database password")]
        public string pass { get; set; }
        [Option(Required = false, HelpText = "Database port")]
        public uint port { get; set; }
        [Option(Required = false, HelpText = "Database name")]
        public string dbname { get; set; }
        [Option(Required = false, HelpText = "Table name")]
        public string tablename { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
  .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
  .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
        }

        static void RunOptionsAndReturnExitCode(Options opts)
        {
            IDBWrapper wrap = null;

            switch (opts.dbtype.ToLower())
            {
                case "psql":
                    wrap = new PostgresWrapper();
                    break;
                case "mysql":
                case "mariadb":
                    wrap = new MySQLWrapper();
                    break;
            }

            if (wrap.Connect(opts.host, opts.user, opts.pass, opts.port))
            {
                foreach (var dbname in wrap.GetDatabases())
                {
                    if (dbname.ToLower().Contains("_schema"))
                        continue;

                    Console.WriteLine("Exporting {0}", dbname);

                    if (!Directory.Exists(dbname))
                        System.IO.Directory.CreateDirectory(dbname);

                    foreach (var tablename in wrap.GetTables(dbname))
                    {
                        if ((string.IsNullOrWhiteSpace(opts.dbname) || string.IsNullOrWhiteSpace(opts.tablename)) ||
                        (opts.dbname == dbname && string.IsNullOrWhiteSpace(opts.tablename) ||
                        opts.dbname == dbname && opts.tablename == tablename))
                        {
                            Console.WriteLine("Exporting {0}.{1}", dbname, tablename);

                            string filename = Path.Combine(Directory.GetCurrentDirectory(), dbname, tablename + ".xlsx");
                            var tables = SplitTable(wrap.GetTableRows(dbname, tablename), 1000000);
                            new ExcelExporter().Export(tables, filename);
                        }
                    }
                }
            }
            else
                Console.WriteLine("Could not connect to database");
        }

        private static IEnumerable<DataTable> SplitTable(DataTable originalTable, int batchSize)
        {
            List<DataTable> tables = new List<DataTable>();

            if (originalTable.Rows.Count < batchSize)
            {
                tables.Add(originalTable);
                return tables;
            }

            int i = 0;
            int j = 1;
            DataTable newDt = originalTable.Clone();
            newDt.TableName = "Table_" + j;
            newDt.Clear();

            foreach (DataRow row in originalTable.Rows)
            {
                DataRow newRow = newDt.NewRow();
                newRow.ItemArray = row.ItemArray;
                newDt.Rows.Add(newRow);
                i++;
                if (i == batchSize)
                {
                    tables.Add(newDt);
                    j++;
                    newDt = originalTable.Clone();
                    newDt.TableName = "Table_" + j;
                    newDt.Clear();
                    i = 0;
                }
            }

            if (newDt.Rows.Count > 0)
            {
                tables.Add(newDt);
                j++;
                newDt = originalTable.Clone();
                newDt.TableName = "Table_" + j;
                newDt.Clear();

            }
            return tables;
        }
    }
}
