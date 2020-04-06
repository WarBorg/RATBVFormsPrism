using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RATBVData.Models.Models;
using RATBVFormsPrism.Interfaces;
using SQLite;
using Xamarin.Forms;

namespace RATBVFormsPrism.Services
{
    public class BusDataService : IBusDataService
    {
        #region Memebers

        private readonly SQLiteAsyncConnection _asyncConnection;

        #endregion Memebers

        #region Constructor

        public BusDataService()
        {
            string sqliteFilename = "ratbvPrism.sql";

            _asyncConnection = DependencyService.Get<ISQLite>().GetAsyncConnection(sqliteFilename);
        }

        #endregion Constructor

        #region Methods

        #region Universal

        public async Task CreateAllTablesAsync()
        {
            await _asyncConnection.CreateTableAsync<BusLineModel>();
            await _asyncConnection.CreateTableAsync<BusStationModel>();
            await _asyncConnection.CreateTableAsync<BusTimeTableModel>();
        }

        public async Task DropAllTablesAsync()
        {
            await _asyncConnection.DropTableAsync<BusLineModel>();
            await _asyncConnection.DropTableAsync<BusStationModel>();
            await _asyncConnection.DropTableAsync<BusTimeTableModel>();
        }

        public async Task DeleteAllTablesAsync()
        {
            await DeleteAllAsync<BusLineModel>();
            await DeleteAllAsync<BusStationModel>();
            await DeleteAllAsync<BusTimeTableModel>();
        }

        #endregion Universal

        #region Bus Lines

        public Task<int> CountBusLines
        {
            get 
            {
                return _asyncConnection.Table<BusLineModel>().CountAsync();
            }
        }

        public async Task<List<BusLineModel>> GetBusLinesByNameAsync(string nameFilter = null)
        {
            if (nameFilter == null)
                nameFilter = String.Empty;

            return await (from busLineTable in _asyncConnection.Table<BusLineModel>()
                          where busLineTable.Name.Contains(nameFilter)
                          orderby busLineTable.Id
                          select busLineTable).ToListAsync();
        }

        public async Task<BusLineModel> GetBusLineByIdAsync(int Id)
        {
            return await _asyncConnection.GetAsync<BusLineModel>(Id);
        }

        public async Task<int> InsertBusLineAsync(BusLineModel busLine)
        {
            return await _asyncConnection.InsertAsync(busLine);
        }

        public async Task<int> UpdateBusLineAsync(BusLineModel busLine)
        {
            return await _asyncConnection.UpdateAsync(busLine);
        }

        public async Task<int> DeleteBusLineAsync(BusLineModel busLine)
        {
            return await _asyncConnection.DeleteAsync(busLine);
        }

        public async Task<int> InsertOrReplaceBusLinesAsync(IEnumerable<BusLineModel> busLines)
        {
            return await InsertOrReplaceAllAsync(busLines);
        }

        #endregion Bus Lines

        #region Bus Stations

        public async Task<int> CountBusStationsAsync(int busLineId, string direction)
        {
            return await (from busStationTable in _asyncConnection.Table<BusStationModel>()
                          where busStationTable.BusLineId == busLineId
                          && busStationTable.Direction == direction
                          select busStationTable).CountAsync();
        }

        public async Task<List<BusStationModel>> GetBusStationsByNameAsync(int busId, string direction, string nameFilter = null)
        {
            if (nameFilter == null)
                nameFilter = String.Empty;

            return await (from busStationTable in _asyncConnection.Table<BusStationModel>()
                          where busStationTable.BusLineId == busId
                          && busStationTable.Direction == direction
                          && busStationTable.Name.Contains(nameFilter)
                          orderby busStationTable.Id
                          select busStationTable).ToListAsync();
        }

        public async Task<BusStationModel> GetBusStationByIdAsync(int Id)
        {
            return await _asyncConnection.GetAsync<BusStationModel>(Id);
        }

        public async Task<int> InsertBusStationAsync(BusStationModel busStation)
        {
            return await _asyncConnection.InsertAsync(busStation);
        }

        public async Task<int> UpdateBusStationAsync(BusStationModel busStation)
        {
            return await _asyncConnection.UpdateAsync(busStation);
        }

        public async Task<int> DeleteBusStationAsync(BusStationModel busStation)
        {
            return await _asyncConnection.DeleteAsync(busStation);
        }

        public async Task<int> InsertOrReplaceBusStationsAsync(IEnumerable<BusStationModel> busStations)
        {
            var busLineId = busStations.FirstOrDefault()?.BusLineId ?? 0;
            var busDirection = busStations.FirstOrDefault()?.Direction ?? string.Empty;

            var storedBusStations = await GetBusStationsByNameAsync(busLineId, busDirection);

            if (storedBusStations.Count > 0)
            {
                foreach (var station in busStations)
                {
                    var existingStation = storedBusStations.FirstOrDefault(b => b.Name == station.Name);

                    if (existingStation != null)
                        station.Id = existingStation.Id;
                }
            }

            return await InsertOrReplaceAllAsync(busStations);
        }

        #endregion Bus Stations

        #region Bus Time Table

        public async Task<int> CountBusTimeTableAsync(int busStationId)
        {
            return await (from busStationTable in _asyncConnection.Table<BusTimeTableModel>()
                          where busStationTable.BusStationId == busStationId
                          select busStationTable).CountAsync();
        }

        public async Task<List<BusTimeTableModel>> GetBusTimeTableByBusStationId(int busStationId)
        {
            return await (from busTimeTable in _asyncConnection.Table<BusTimeTableModel>()
                          where busTimeTable.BusStationId == busStationId
                          orderby busTimeTable.Id
                          select busTimeTable).ToListAsync();
        }

        public async Task<BusTimeTableModel> GetBusTimeTableById(int Id)
        {
            return await _asyncConnection.GetAsync<BusTimeTableModel>(Id);
        }

        public async Task<int> InsertBusTimeTableAsync(BusTimeTableModel busTimeTable)
        {
            return await _asyncConnection.InsertAsync(busTimeTable);
        }

        public async Task<int> UpdateBusTimeTableAsync(BusTimeTableModel busTimeTable)
        {
            return await _asyncConnection.UpdateAsync(busTimeTable);
        }

        public async Task<int> DeleteBusTimeTableAsync(BusTimeTableModel busTimeTable)
        {
            return await _asyncConnection.DeleteAsync(busTimeTable);
        }

        public async Task<int> InsertOrReplaceBusTimeTablesAsync(IEnumerable<BusTimeTableModel> busTimeTables)
        {
            var busStationId = busTimeTables.FirstOrDefault()?.BusStationId ?? 0;

            var storedBusTimeTables = await GetBusTimeTableByBusStationId(busStationId);

            if (storedBusTimeTables.Count > 0)
            {
                foreach (var timeTable in busTimeTables)
                {
                    var existingTimeTable = storedBusTimeTables.FirstOrDefault(b => b.BusStationId == timeTable.BusStationId && b.Hour == timeTable.Hour);

                    if (existingTimeTable != null)
                        timeTable.Id = existingTimeTable.Id;
                }
            }

            return await InsertOrReplaceAllAsync(busTimeTables);
        }

        #endregion Bus Time Table

        #region Extensions

        public async Task<int> InsertOrReplaceAllAsync(IEnumerable items, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            var returnItems = 0;

            foreach (var item in items)
            {
                await _asyncConnection.InsertOrReplaceAsync(item);

                returnItems++;
            }

            return returnItems;
        }

        private Task<int> DeleteAllAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Factory.StartNew(() =>
            {
                var conn = _asyncConnection.GetConnection();
                using (conn.Lock())
                {
                    return conn.DeleteAll<T>();
                }
            }, cancellationToken, TaskCreationOptions.None, null ?? TaskScheduler.Default);
        }

        #endregion Extensions

        #endregion Methods
    }
}
