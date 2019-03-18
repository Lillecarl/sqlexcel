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
		ConnectionTimeout = 500000,
		DefaultCommandTimeout = 500000,
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
	    var databases = new List<string>();

            using (var reader = new MySql.Data.MySqlClient.MySqlCommand("SELECT table_schema FROM INFORMATION_SCHEMA.tables GROUP BY table_schema", dbconn).ExecuteReader())
                while (reader.Read())
                    databases.Add(reader.GetString(0));

	    return databases;
        }

        public IEnumerable<string> GetTables(string databasename)
        {
	    var tables = new List<string>();

            using (var reader = new MySqlCommand(string.Format("SELECT table_name FROM INFORMATION_SCHEMA.tables WHERE table_schema = \"{0}\"", databasename), dbconn).ExecuteReader())
                while (reader.Read())
                    tables.Add(reader.GetString(0));

	    return tables;
        }

        public DataTable GetTableRows(string databasename, string tablename)
        {
            dbconn.ChangeDatabase(databasename);
            var table = new DataTable();

            using (var command = new MySqlCommand(string.Format("SELECT * FROM {0}", tablename), dbconn))
                using (var adapter = new MySqlDataAdapter(command))
                    adapter.Fill(table);

            return table;
        }
    }
}
