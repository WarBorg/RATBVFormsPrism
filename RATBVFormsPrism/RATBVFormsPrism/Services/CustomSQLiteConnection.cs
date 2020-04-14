using System;
using SQLite;

namespace RATBVFormsPrism.Services
{
    [Obsolete("Please use the CustomAsyncSQLiteConnection method")]
    public class CustomSQLiteConnection : SQLiteConnection, ISQLiteConnection
    {
        public CustomSQLiteConnection(string databasePath,
                                      bool storeDateTimeAsTicks = true)
            : base(databasePath,
                   storeDateTimeAsTicks)
        {
        }

        public CustomSQLiteConnection(string databasePath,
                                      SQLiteOpenFlags openFlags,
                                      bool storeDateTimeAsTicks = true)
            : base(databasePath,
                   openFlags,
                   storeDateTimeAsTicks)
        {
        }
    }
}
