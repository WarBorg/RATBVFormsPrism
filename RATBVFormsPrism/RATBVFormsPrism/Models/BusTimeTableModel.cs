using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATBVFormsPrism.Models
{
    public class BusTimeTableModel
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; } // without nullable on Id, InsertOrReplace will not autoincrement the Id

        [ForeignKey(typeof(BusStationModel))]
        public int BusStationId { get; set; }
        public string Hour { get; set; }
        public string Minutes { get; set; }
        public string TimeOfWeek { get; set; }
        public string LastUpdateDate { get; set; }
    }
}
