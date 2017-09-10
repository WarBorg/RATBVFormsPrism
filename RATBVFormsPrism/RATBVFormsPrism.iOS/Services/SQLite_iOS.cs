using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.iOS.Services;
using SQLite;
using System;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_iOS))]

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
            
            var conn = new SQLiteConnection(path);

            // Return the database connection 
            return conn;
        }

        public SQLiteAsyncConnection GetAsyncConnection(string databaseName)
        {
            var path = GetPath(databaseName);

            var conn = new SQLiteAsyncConnection(path);

            // Return the database connection 
            return conn;
        }

        #endregion
    }
}