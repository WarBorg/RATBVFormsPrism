using SQLite.Net;
using SQLite.Net.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATBVFormsPrism.Interfaces
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection(string databaseName);
        SQLiteAsyncConnection GetAsyncConnection(string databaseName);
    }
}
