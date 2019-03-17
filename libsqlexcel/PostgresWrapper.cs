using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace libsqlexcel
{
    public class PostgresWrapper : IDBWrapper
    {
        private Npgsql.NpgsqlConnection dbconn = null;
        public bool Connect(string host, string user, string pass, uint port)
        {
            var stringbuilder = new NpgsqlConnectionStringBuilder()
            {
                Host = host,
                Username = user,
                Password = pass,
                Port = (int)port,
            };

            dbconn = new NpgsqlConnection(stringbuilder.ToString());

            try {
                dbconn.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not connect to database, error: {0}", ex.ToString());
                return false;
            }
        }

        public IEnumerable<string> GetDatabases()
        {
            using (var reader = new NpgsqlCommand("\\l", dbconn).ExecuteReader())
                while (reader.Read())
                    yield return reader.GetString(0);
        }

        public IEnumerable<string> GetTables(string databasename)
        {
            dbconn.ChangeDatabase(databasename);

            using (var reader = new NpgsqlCommand("\\dt", dbconn).ExecuteReader())
                while (reader.Read())
                    yield return reader.GetString(1);
        }

        public DataTable GetTableRows(string databasename, string tablename)
        {
            dbconn.ChangeDatabase(databasename);
            var table = new DataTable();

            using (var command = new NpgsqlCommand(string.Format("SELECT * FROM {0}", tablename), dbconn))
            {
                using (var adapter = new NpgsqlDataAdapter(command))
                    adapter.Fill(table);
            }
            return table;
        }
 
    }
}