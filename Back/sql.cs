using System.Data.Common;
using System.Xml;
using MySql.Data.MySqlClient;

namespace RightVisionBot.Back
{
    internal class sql
    {
        private MySqlConnection db;
        public sql(string connection) => db = new MySqlConnection(connection);

        public List<string> Read(string query, string columnName)
        {
            try
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.Connection = db;
                    cmd.CommandText = query;
                    using (var reader = cmd.ExecuteReader())
                    {
                        List<List<string>> data = new();
                        List<string> rows = new();
                        while (reader.Read())
                            rows.Add(reader[columnName].ToString());

                        reader.Close();
                        db.Close();
                        return rows;
                    }
                }
            }
            catch (MySqlException ex)
            {
                db.Close();
                throw ex;
            }
        }

        public List<Dictionary<string, object>> ExtRead(string query, string[] columnNames)
        {
            Console.WriteLine("Подключение к базе...");
            try
            {
                db.Open();
                using var cmd = db.CreateCommand();
                cmd.Connection = db;
                cmd.CommandText = query;
                using var reader = cmd.ExecuteReader();
                Console.WriteLine("Подключено. Извлечение данных...");
                List<Dictionary<string, object>> data = new();

                while (reader.Read())
                {
                    Dictionary<string, object> row = new();
                    foreach (var columnName in columnNames)
                    {
                        row[columnName] = reader[columnName];
                    }

                    data.Add(row);
                }

                db.Close();
                reader.Close();
                Console.WriteLine("Данные получены");
                return data;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Произошла ошибка при подключении к базе данных! (ExtRead)");
                db.Close();
                throw ex;
            }
        }

        public bool TableExists(string tableName)
        {
            try
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.Connection = db;
                    cmd.CommandText = $"SHOW TABLES LIKE '{tableName}'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        bool tableExists = reader.HasRows;
                        reader.Close();
                        db.Close();
                        return tableExists;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Произошла ошибка при подключении к базе данных! (TableExists)");
                db.Close();
                throw ex;
            }
        }
    }
}
