using System;
using System.IO;
using RATBVFormsPrism.Services;
using SQLite;

namespace RATBVFormsPrism.iOS.Services
{
    public class SQLite_iOS : ISQLiteService
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