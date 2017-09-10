using SQLite;

namespace RATBVFormsPrism.Interfaces
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection(string databaseName);
        SQLiteAsyncConnection GetAsyncConnection(string databaseName);
    }
}
