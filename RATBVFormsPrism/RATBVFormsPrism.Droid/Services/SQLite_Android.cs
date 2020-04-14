using System;
using System.IO;
using RATBVFormsPrism.Services;

namespace RATBVFormsPrism.Droid.Services
{
    public class SQLite_Android : ISQLiteService
	{
        #region ISQLiteService Methods

        public ISQLiteConnection GetConnection(string databaseName)
        {
            var path = GetPath(databaseName);

            var conn = new CustomSQLiteConnection(path);

            // Return the database connection 
            return conn;
        }

        public ISQLiteAsyncConnection GetAsyncConnection(string databaseName)
        {
            var path = GetPath(databaseName);

            var conn = new CustomSQLiteAsyncConnection(path);

            // Return the database connection 
            return conn;
        }

        #endregion

        #region Methods

        private string GetPath(string databaseName)
        {
            var sqliteFilename = databaseName;

            // Documents folder
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder
                                                                        .Personal);

            return Path.Combine(documentsPath, sqliteFilename);
        }

        #endregion
    }
}