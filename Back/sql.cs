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
                using var cmd = db.CreateCommand();
                cmd.Connection = db;
                cmd.CommandText = query;
                using var reader = cmd.ExecuteReader();
                List<string> rows = new();
                while (reader.Read())
                    rows.Add(reader[columnName].ToString());

                reader.Close();
                db.Close();
                return rows;
            }
            catch
            {
                db.Close();
                throw;
            }
        }

        public List<Dictionary<string, string>> ExtRead(string query, string[] columnNames)
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
                List<Dictionary<string, string>> data = new();

                while (reader.Read())
                {
                    Dictionary<string, string> row = new();
                    foreach (var columnName in columnNames)
                        row[columnName] = reader[columnName].ToString();

                    data.Add(row);
                }

                db.Close();
                reader.Close();
                Console.WriteLine("Данные получены");
                return data;
            }
            catch
            {
                Console.WriteLine("Произошла ошибка при подключении к базе данных! (ExtRead)");
                db.Close();
                throw;
            }
        }
    }
}
