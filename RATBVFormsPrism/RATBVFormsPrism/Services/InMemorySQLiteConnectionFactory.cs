using System;

namespace RATBVFormsPrism.Services
{
    public class InMemorySQLiteConnectionFactory : ISQLiteConnectionFactory
    {        
        public ISQLiteAsyncConnection GetAsyncSqlConnection(ISQLiteService sqliteService)
        {
            return sqliteService.GetAsyncConnection(":memory:");
        }

        [Obsolete("Please use the GetAsyncSqlConnection method")]
        public ISQLiteConnection GetSqlConnection(ISQLiteService sqliteService)
        {
            throw new NotImplementedException();
        }
    }
}
