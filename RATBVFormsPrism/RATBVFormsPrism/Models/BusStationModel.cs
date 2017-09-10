using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

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
