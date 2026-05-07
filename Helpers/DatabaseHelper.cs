using MySqlConnector;

namespace LocAutoPlusApp.Helpers
{
    //internal class DatabaseHelper
    //{
    //}

    public static class DatabaseHelper
    {
        private const string ConnectionString =
            "Server=127.0.0.1;Port=3306;Database=locautoplus2;Uid=root;Pwd=;CharSet=utf8mb4;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}
