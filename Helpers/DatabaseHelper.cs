using MySqlConnector;

namespace LocAutoPlusApp.Helpers
{
    //internal class DatabaseHelper
    //{
    //}

    public static class DatabaseHelper
    {
        private const string ConnectionString =
            "Server=localhost;Port=3306;Database=sc2kuph3194_locautoplus;Uid=sc2kuph3194_locautoplus;Pwd=juD3xxSDUlpjNdnR;CharSet=utf8mb4;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}
