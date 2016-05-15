using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using RATBVFormsPrism.Droid.Services;

using SQLite.Net;
using Xamarin.Forms;
using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.Services;
using SQLite.Net.Async;

[assembly: Dependency (typeof (SQLite_Android))]

namespace RATBVFormsPrism.Droid.Services
{
	public class SQLite_Android : ISQLite
	{
		public SQLite_Android ()
		{
		}

        #region ISQLite implementation

        private string GetPath(string databaseName)
        {
            var sqliteFilename = databaseName;
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder

            return Path.Combine(documentsPath, sqliteFilename);
        }

        public SQLiteConnection GetConnection(string databaseName)
        {
            var path = GetPath(databaseName);

            var plat = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
            var conn = new SQLiteConnection(plat, path);

            // Return the database connection 
            return conn;
        }

        public SQLiteAsyncConnection GetAsyncConnection(string databaseName)
        {
            var path = GetPath(databaseName);

            var platform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
            var connectionParameters = new SQLiteConnectionString(path, false);

            var connectionWithLock = new SQLiteConnectionWithLock(platform, connectionParameters);

            var conn = new SQLiteAsyncConnection(() => connectionWithLock);

            // Return the database connection 
            return conn;
        }

        #endregion
    }
}