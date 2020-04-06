using System;
using System.IO;
using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.iOS.Services;
using SQLite;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_iOS))]

namespace RATBVFormsPrism.iOS.Services
{
    public class SQLite_iOS : ISQLite
    {
        #region ISQLite Methods

        private string GetPath(string databaseName)
        {
            var sqliteFilename = databaseName;

            // Documents folder
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder
                                                                        .Personal);

            // Library folder
            string libraryPath = Path.Combine(documentsPath, "..", "Library"); 

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