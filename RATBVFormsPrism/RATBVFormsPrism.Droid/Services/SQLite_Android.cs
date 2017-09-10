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

using SQLite;
using Xamarin.Forms;
using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.Services;

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