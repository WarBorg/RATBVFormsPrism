using System;

namespace RATBVFormsPrism.Services
{
    public interface ISQLiteConnectionFactory
    {
        public ISQLiteAsyncConnection GetAsyncSqlConnection(ISQLiteService sqliteService);

        [Obsolete("Please use the GetAsyncSqlConnection method")]
        public ISQLiteConnection GetSqlConnection(ISQLiteService sqliteService);
    }
}
