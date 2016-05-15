using Foundation;
using UIKit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using RATBVFormsPrism.iOS.Services;
using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.Services;
using SQLite.Net;
using SQLite.Net.Async;

[assembly: Dependency (typeof (SQLite_iOS))]

namespace RATBVFormsPrism.iOS.Services
{
    public class SQLite_iOS : ISQLite
    {
        public SQLite_iOS ()
        {
        }

        #region ISQLite implementation

        private string GetPath(string databaseName)
        {
            var sqliteFilename = databaseName;
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
            string libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder

            return Path.Combine(libraryPath, sqliteFilename);
        }

        public SQLiteConnection GetConnection(string databaseName)
        {
            var path = GetPath(databaseName);

            var plat = new SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS();
            var conn = new SQLiteConnection(plat, path);

            // Return the database connection 
            return conn;
        }

        public SQLiteAsyncConnection GetAsyncConnection(string databaseName)
        {
            var path = GetPath(databaseName);

            var platform = new SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS();
            var connectionParameters = new SQLiteConnectionString(path, false);

            var connectionWithLock = new SQLiteConnectionWithLock(platform, connectionParameters);

            var conn = new SQLiteAsyncConnection(() => connectionWithLock);

            // Return the database connection 
            return conn;
        }

        #endregion
    }
}