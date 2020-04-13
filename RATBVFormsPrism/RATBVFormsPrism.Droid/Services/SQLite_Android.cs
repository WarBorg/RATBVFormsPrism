using System;
using System.IO;
using RATBVFormsPrism.Services;
using SQLite;

namespace RATBVFormsPrism.Droid.Services
{
    public class SQLite_Android : ISQLiteService
	{
        #region ISQLite Methods

        private string GetPath(string databaseName)
        {
            var sqliteFilename = databaseName;

            // Documents folder
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder
                                                                        .Personal); 

            return Path.Combine(documentsPath, sqliteFilename);
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