using System;
using System.IO;
using libsqlexcel;
using OfficeOpenXml;
using CommandLine;
using System.Collections.Generic;

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
                            var table = wrap.GetTableRows(dbname, tablename);
                            new ExcelExporter().Export(table, filename);
                        }
                    }
                }
            }
            else
                Console.WriteLine("Could not connect to database");
        }
    }
}
