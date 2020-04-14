namespace RATBVFormsPrism.Services
{
    public interface ISQLiteService
    {
        ISQLiteConnection GetConnection(string databaseName);
        ISQLiteAsyncConnection GetAsyncConnection(string databaseName);
    }
}
