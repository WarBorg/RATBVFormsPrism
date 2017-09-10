using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace RATBVFormsPrism.Models
{
    public class BusLineModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Route { get; set; }
        public string Type { get; set; }
        public string Color { get; set; }
        public string LinkNormalWay { get; set; }
        public string LinkReverseWay { get; set; }
        public string LastUpdateDate { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<BusStationModel> BusStations { get; set; }
    }
}
