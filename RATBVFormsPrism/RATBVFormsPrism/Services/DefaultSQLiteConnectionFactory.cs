using System;

namespace RATBVFormsPrism.Services
{
    public class DefaultSQLiteConnectionFactory : ISQLiteConnectionFactory
    {
        public ISQLiteAsyncConnection GetAsyncSqlConnection(ISQLiteService sqliteService)
        {
            string sqliteFilename = "ratbvPrism.sql";

            return sqliteService.GetAsyncConnection(sqliteFilename);
        }

        [Obsolete("Please use the GetAsyncSqlConnection method")]
        public ISQLiteConnection GetSqlConnection(ISQLiteService sqliteService)
        {
            throw new NotImplementedException();
        }
    }
}