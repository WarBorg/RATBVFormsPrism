using SQLite;

namespace RATBVFormsPrism.Services
{
    public interface ISQLiteService
    {
        SQLiteConnection GetConnection(string databaseName);
        SQLiteAsyncConnection GetAsyncConnection(string databaseName);
    }
}
