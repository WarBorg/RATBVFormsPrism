using SQLite;

namespace RATBVFormsPrism.Services
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection(string databaseName);
        SQLiteAsyncConnection GetAsyncConnection(string databaseName);
    }
}
