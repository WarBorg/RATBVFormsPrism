using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.Services;
using RATBVFormsPrism.WinPhone.Services;
using SQLite.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_WinPhone))]

namespace RATBVFormsPrism.WinPhone.Services
{
    public class SQLite_WinPhone : ISQLite
    {
        public SQLite_WinPhone() 
        { 
        }

        #region ISQLite implementation

        public SQLite.Net.SQLiteConnection GetConnection(string databaseName)
        {
            var sqliteFilename = databaseName;
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, sqliteFilename);

            // Create the connection
            var plat = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();
            var conn = new SQLite.Net.SQLiteConnection(plat, path);
            
            // Return the database connection 
            return conn;
        }

        public SQLite.Net.Async.SQLiteAsyncConnection GetAsyncConnection(string databaseName)
        {
            var sqliteFilename = databaseName;
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, sqliteFilename);

            // Create the connection
            var platform = new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT();
            var connectionParameters = new SQLiteConnectionString(path, false); 
            var sqliteConnectionPool = new SQLiteConnectionPool(platform); 

            var conn = new SQLite.Net.Async.SQLiteAsyncConnection(() => sqliteConnectionPool.GetConnection(connectionParameters));

            // Return the database connection 
            return conn;
        }

        #endregion
    }
}
