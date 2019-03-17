using System.Collections.Generic;
using System.Data;

namespace libsqlexcel
{
    public interface IDBWrapper
    {
         bool Connect(string host, string user, string pass, uint port);
         IEnumerable<string> GetDatabases();
         IEnumerable<string> GetTables(string database);
         DataTable GetTableRows(string databasename, string tablename);
    }
}