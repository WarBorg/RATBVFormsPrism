using RATBVFormsPrism.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATBVFormsPrism.Interfaces
{
    public interface IBusDataService
    {
        Task CreateAllTablesAsync();
        Task DropAllTablesAsync();
        Task DeleteAllTablesAsync();

        Task<int> CountBusLines { get; }
        Task<List<BusLineModel>> GetBusLinesByNameAsync(string nameFilter = null);
        Task<BusLineModel> GetBusLineByIdAsync(int Id);
        Task<int> InsertBusLineAsync(BusLineModel busLine);
        Task<int> UpdateBusLineAsync(BusLineModel busLine);
        Task<int> DeleteBusLineAsync(BusLineModel busLine);
        Task<int> InsertOrReplaceBusLinesAsync(IEnumerable<BusLineModel> busLines);

        Task<int> CountBusStationsAsync(int busLineId, string direction);
        Task<List<BusStationModel>> GetBusStationsByNameAsync(int busId, string direction, string nameFilter = null);
        Task<BusStationModel> GetBusStationByIdAsync(int Id);
        Task<int> InsertBusStationAsync(BusStationModel busStation);
        Task<int> UpdateBusStationAsync(BusStationModel busStation);
        Task<int> DeleteBusStationAsync(BusStationModel busStation);
        Task<int> InsertOrReplaceBusStationsAsync(IEnumerable<BusStationModel> busStations);

        Task<int> CountBusTimeTableAsync(int busStationId);
        Task<List<BusTimeTableModel>> GetBusTimeTableByBusStationId(int busStationId);
        Task<BusTimeTableModel> GetBusTimeTableById(int Id);
        Task<int> InsertBusTimeTableAsync(BusTimeTableModel busTimeTable);
        Task<int> UpdateBusTimeTableAsync(BusTimeTableModel busTimeTable);
        Task<int> DeleteBusTimeTableAsync(BusTimeTableModel busTimeTable);
        Task<int> InsertOrReplaceBusTimeTablesAsync(IEnumerable<BusTimeTableModel> busTimeTables);
    }
}
