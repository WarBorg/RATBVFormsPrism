using SQLite;

namespace RATBVFormsPrism.Services
{
    public class CustomSQLiteAsyncConnection : SQLiteAsyncConnection, ISQLiteAsyncConnection
    {
        public CustomSQLiteAsyncConnection(string databasePath,
                                           bool storeDateTimeAsTicks = true)
            : base(databasePath,
                   storeDateTimeAsTicks)
        {
        }

        public CustomSQLiteAsyncConnection(string databasePath,
                                           SQLiteOpenFlags openFlags,
                                           bool storeDateTimeAsTicks = true)
            : base(databasePath,
                   openFlags,
                   storeDateTimeAsTicks)
        {
        }
    }
}
