using System.Collections.Generic;
using System.Threading.Tasks;
using RATBVData.Models.Models;

namespace RATBVFormsPrism.Services
{
    public interface IBusDataService
    {
        Task CreateAllTablesAsync();
        Task DropAllTablesAsync();
        Task DeleteAllTablesAsync();

        Task<int> CountBusLinesAsync();
        Task<List<BusLineModel>> GetBusLinesAsync();
        Task<int> InsertOrReplaceBusLinesAsync(IEnumerable<BusLineModel> busLines);

        Task<int> CountBusStationsByBusLineIdAndDirectionAsync(int busLineId, string direction);
        Task<List<BusStationModel>> GetBusStationsByBusLineIdAndDirectionAsync(int busId, string direction);
        Task<int> InsertOrReplaceBusStationsAsync(IEnumerable<BusStationModel> busStations);

        Task<int> CountBusTimeTableByBusStationIdAsync(int busStationId);
        Task<List<BusTimeTableModel>> GetBusTimeTableByBusStationId(int busStationId);
        Task<int> InsertOrReplaceBusTimeTablesAsync(IEnumerable<BusTimeTableModel> busTimeTables);
    }
}
