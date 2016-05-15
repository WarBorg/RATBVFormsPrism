using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATBVFormsPrism.Models
{
    public class BusStationModel
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; } // without nullable on Id, InsertOrReplace will not autoincrement the Id
        
        [ForeignKey(typeof(BusLineModel))]
        public int BusLineId { get; set; }
        public string Name { get; set; }
        public string Direction { get; set; }
        public string SchedualLink { get; set; }
        public string LastUpdateDate { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<BusTimeTableModel> BusTimeTables { get; set; }
    }
}
