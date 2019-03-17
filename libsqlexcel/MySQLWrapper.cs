using System;
using System.Data;
using System.Collections.Generic;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace libsqlexcel
{
    public class MySQLWrapper : IDBWrapper
    {
        private MySql.Data.MySqlClient.MySqlConnection dbconn = null;

        public bool Connect(string host, string user, string pass, uint port)
        {
            var connectionstring = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder()
            {
                Server = host,
                UserID = user,
                Password = pass,
                Port = port,
                ConvertZeroDateTime = true,
            };

            dbconn = new MySqlConnection(connectionstring.GetConnectionString(true));

            try
            {
                dbconn.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Could not connect to database, error: {0}", ex.ToString()));
                return false;
            }
        }

        public IEnumerable<string> GetDatabases()
        {
            using (var reader = new MySql.Data.MySqlClient.MySqlCommand("SHOW DATABASES", dbconn).ExecuteReader())
                while (reader.Read())
                    yield return reader.GetString(0);
        }

        public IEnumerable<string> GetTables(string databasename)
        {
            dbconn.ChangeDatabase(databasename);

            using (var reader = new MySql.Data.MySqlClient.MySqlCommand("SHOW TABLES", dbconn).ExecuteReader())
                while (reader.Read())
                    yield return reader.GetString(0);
        }

        public DataTable GetTableRows(string databasename, string tablename)
        {
            dbconn.ChangeDatabase(databasename);
            var table = new DataTable();

            using (var command = new MySqlCommand(string.Format("SELECT * FROM {0}", tablename), dbconn))
            {
                using (var adapter = new MySqlDataAdapter(command))
                    adapter.Fill(table);
            }
            return table;
        }
    }
}