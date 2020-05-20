using System;

namespace RATBVFormsPrism.Services
{
    public interface ISQLiteService
    {
        [Obsolete]
        ISQLiteConnection GetConnection(string databaseName);
        ISQLiteAsyncConnection GetAsyncConnection(string databaseName);
    }
}
