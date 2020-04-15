using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RATBVData.Models.Models;

namespace RATBVFormsPrism.Services
{
    public class BusDataService : IBusDataService
    {
        #region Fields

        private readonly ISQLiteAsyncConnection _asyncConnection;

        #endregion

        #region Constructors

        public BusDataService(ISQLiteAsyncConnection asyncConnection)
        {
            _asyncConnection = asyncConnection;
        }

        #endregion

        #region Universal Methods

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

        #endregion

        #region Bus Lines Methods

        public async Task<int> CountBusLinesAsync()
        {
            return await _asyncConnection.Table<BusLineModel>()
                                        ?.CountAsync();
        }

        public async Task<List<BusLineModel>> GetBusLineAsync()
        {
            return await (from busLineTable in _asyncConnection.Table<BusLineModel>()
                          orderby busLineTable.Id
                          select busLineTable).ToListAsync();
        }

        public async Task<int> InsertOrReplaceBusLinesAsync(IEnumerable<BusLineModel> busLines)
        {
            return await InsertOrReplaceAllAsync(busLines);
        }

        #endregion

        #region Bus Stations Methods

        public async Task<int> CountBusStationsByBusLineIdAndDirectionAsync(int busLineId,
                                                                            string direction)
        {
            return await (from busStationTable in _asyncConnection.Table<BusStationModel>()
                          where busStationTable.BusLineId == busLineId
                          && busStationTable.Direction == direction
                          select busStationTable).CountAsync();
        }

        public async Task<List<BusStationModel>> GetBusStationsByBusLineIdAndDirectionAsync(int busId,
                                                                                            string direction)
        {
            return await (from busStationTable in _asyncConnection.Table<BusStationModel>()
                          where busStationTable.BusLineId == busId
                             && busStationTable.Direction == direction
                          orderby busStationTable.Id
                          select busStationTable).ToListAsync();
        }

        public async Task<int> InsertOrReplaceBusStationsAsync(IEnumerable<BusStationModel> busStations)
        {
            var busLineId = busStations.FirstOrDefault()?.BusLineId ?? 0;
            var busDirection = busStations.FirstOrDefault()?.Direction ?? string.Empty;

            var storedBusStations = await GetBusStationsByBusLineIdAndDirectionAsync(busLineId,
                                                                                     busDirection);

            if (storedBusStations.Count > 0)
            {
                foreach (var station in busStations)
                {
                    var existingStation = storedBusStations.FirstOrDefault(b => b.Name == station.Name);

                    if (existingStation != null)
                    {
                        station.Id = existingStation.Id;
                    }
                }
            }

            return await InsertOrReplaceAllAsync(busStations);
        }

        #endregion

        #region Bus Time Table Methods

        public async Task<int> CountBusTimeTableByBusStationIdAsync(int busStationId)
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

        public async Task<int> InsertOrReplaceBusTimeTablesAsync(IEnumerable<BusTimeTableModel> busTimeTables)
        {
            var busStationId = busTimeTables.FirstOrDefault()?.BusStationId ?? 0;

            var storedBusTimeTables = await GetBusTimeTableByBusStationId(busStationId);

            if (storedBusTimeTables.Count > 0)
            {
                foreach (var timeTable in busTimeTables)
                {
                    var existingTimeTable = storedBusTimeTables.FirstOrDefault(b => b.BusStationId == timeTable.BusStationId
                                                                                 && b.Hour == timeTable.Hour
                                                                                 && b.TimeOfWeek == timeTable.TimeOfWeek);

                    if (existingTimeTable != null)
                    {
                        timeTable.Id = existingTimeTable.Id;
                    }
                }
            }

            return await InsertOrReplaceAllAsync(busTimeTables);
        }

        #endregion

        #region Insert / Delete Methods

        private async Task<int> InsertOrReplaceAllAsync(IEnumerable items,
                                                       CancellationToken cancellationToken = default)
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

        private Task<int> DeleteAllAsync<T>(CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() =>
            {
                var conn = _asyncConnection.GetConnection();

                using (conn?.Lock())
                {
                    return conn?.DeleteAll<T>() ?? 0;
                }
            }, cancellationToken, TaskCreationOptions.None, null ?? TaskScheduler.Default);
        }

        #endregion

        #region NOT USED METHODS

        #region Bus Lines Methods

        private async Task<List<BusLineModel>> GetBusLinesByNameAsync(string nameFilter = null)
        {
            if (nameFilter == null)
            {
                nameFilter = string.Empty;
            }

            return await (from busLineTable in _asyncConnection.Table<BusLineModel>()
                          where busLineTable.Name
                                            .Contains(nameFilter)
                          orderby busLineTable.Id
                          select busLineTable).ToListAsync();
        }

        private async Task<BusLineModel> GetBusLineByIdAsync(int Id)
        {
            return await _asyncConnection.GetAsync<BusLineModel>(Id);
        }

        private async Task<int> InsertBusLineAsync(BusLineModel busLine)
        {
            return await _asyncConnection.InsertAsync(busLine);
        }

        private async Task<int> UpdateBusLineAsync(BusLineModel busLine)
        {
            return await _asyncConnection.UpdateAsync(busLine);
        }

        private async Task<int> DeleteBusLineAsync(BusLineModel busLine)
        {
            return await _asyncConnection.DeleteAsync(busLine);
        }

        #endregion

        #region Bus Stations Methods

        private async Task<List<BusStationModel>> GetBusStationsByNameAsync(int busId,
                                                                           string direction,
                                                                           string nameFilter = null)
        {
            if (nameFilter == null)
            {
                nameFilter = string.Empty;
            }

            return await (from busStationTable in _asyncConnection.Table<BusStationModel>()
                          where busStationTable.BusLineId == busId
                             && busStationTable.Direction == direction
                             && busStationTable.Name
                                               .Contains(nameFilter)
                          orderby busStationTable.Id
                          select busStationTable).ToListAsync();
        }

        private async Task<BusStationModel> GetBusStationByIdAsync(int Id)
        {
            return await _asyncConnection.GetAsync<BusStationModel>(Id);
        }

        private async Task<int> InsertBusStationAsync(BusStationModel busStation)
        {
            return await _asyncConnection.InsertAsync(busStation);
        }

        private async Task<int> UpdateBusStationAsync(BusStationModel busStation)
        {
            return await _asyncConnection.UpdateAsync(busStation);
        }

        private async Task<int> DeleteBusStationAsync(BusStationModel busStation)
        {
            return await _asyncConnection.DeleteAsync(busStation);
        }

        #endregion

        #region Bus Time Table Methods

        private async Task<BusTimeTableModel> GetBusTimeTableById(int Id)
        {
            return await _asyncConnection.GetAsync<BusTimeTableModel>(Id);
        }

        private async Task<int> InsertBusTimeTableAsync(BusTimeTableModel busTimeTable)
        {
            return await _asyncConnection.InsertAsync(busTimeTable);
        }

        private async Task<int> UpdateBusTimeTableAsync(BusTimeTableModel busTimeTable)
        {
            return await _asyncConnection.UpdateAsync(busTimeTable);
        }

        private async Task<int> DeleteBusTimeTableAsync(BusTimeTableModel busTimeTable)
        {
            return await _asyncConnection.DeleteAsync(busTimeTable);
        }

        #endregion

        #endregion
    }
}