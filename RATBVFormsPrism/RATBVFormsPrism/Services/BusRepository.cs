﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RATBVData.Models.Models;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Exceptions;
using Refit;

namespace RATBVFormsPrism.Services
{
    public class BusRepository : IBusRepository
    {
        #region Constructors and Dependencies

        private readonly IBusApi _busApi;
        private readonly IBusDataService _busDataService;
        private readonly IConnectivityService _connectivityService;

        public BusRepository(IBusApi busApi,
                             IBusDataService busDataService,
                             IConnectivityService connectivityService)
        {
            _busApi = busApi ?? throw new ArgumentException(nameof(busApi));
            _busDataService = busDataService ?? throw new ArgumentException(nameof(busDataService));
            _connectivityService = connectivityService ?? throw new ArgumentException(nameof(connectivityService));
        }

        #endregion

        #region IBusWebService Methods

        public async Task<List<BusLineModel>> GetBusLinesAsync(bool isForcedRefresh)
        {
            try
            {
                var busLinesCount = await _busDataService.CountBusLinesAsync();

                if (isForcedRefresh || busLinesCount == 0)
                {
                    if (!_connectivityService.IsInternetAvailable)
                    {
                        throw new NoInternetException();
                    }

                    var busLines = await _busApi.GetBusLines();

                    var lastUpdated = string.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);

                    await InsertBusLinesInDatabaseAsync(busLines, lastUpdated);
                }

                return await _busDataService.GetBusLinesAsync();
            }
            catch (ValidationApiException validationException)
            {
                throw validationException;
            }
            catch (ApiException apiException)
            {
                throw apiException;
            }
            catch (Exception exeption)
            {
                throw exeption;
            }
        }

        public async Task<List<BusStationModel>> GetBusStationsAsync(string directionLink,
                                                                     string direction,
                                                                     int busLineId,
                                                                     bool isForcedRefresh)
        {
            try
            {
                var busStationsCount = await _busDataService.CountBusStationsByBusLineIdAndDirectionAsync(busLineId,
                                                                                                          direction);
                if (isForcedRefresh || busStationsCount == 0)
                {
                    if (!_connectivityService.IsInternetAvailable)
                    {
                        throw new NoInternetException();
                    }

                    var busStations = await _busApi.GetBusStations(directionLink);

                    var lastUpdated = string.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);

                    await InsertBusStationsInDatabaseAsync(busStations,
                                                           busLineId,
                                                           lastUpdated,
                                                           direction);
                }
                
                return await _busDataService.GetBusStationsByBusLineIdAndDirectionAsync(busLineId,
                                                                                        direction);
            }
            catch (ValidationApiException validationException)
            {
                throw validationException;
            }
            catch (ApiException apiException)
            {
                throw apiException;
            }
            catch (Exception exeption)
            {
                throw exeption;
            }
        }

        public async Task<List<BusTimeTableModel>> GetBusTimetablesAsync(string scheduleLink,
                                                                         int busStationId,
                                                                         bool isForcedRefresh)
        {
            try
            {
                var busTimeTableCount = await _busDataService.CountBusTimeTableByBusStationIdAsync(busStationId);

                if (isForcedRefresh || busTimeTableCount == 0)
                {
                    if (!_connectivityService.IsInternetAvailable)
                    {
                        throw new NoInternetException();
                    }

                    var busTimetables = await _busApi.GetBusTimeTables(scheduleLink);

                    var lastUpdated = string.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);

                    await InsertBusTimetablesInDatabaseAsync(busTimetables,
                                                             busStationId,
                                                             lastUpdated);
                }

                return await _busDataService.GetBusTimeTableByBusStationId(busStationId);
            }
            catch (ValidationApiException validationException)
            {
                throw validationException;
            }
            catch (ApiException apiException)
            {
                throw apiException;
            }
            catch (Exception exeption)
            {
                throw exeption;
            }
        }

        public async Task DownloadAllStationsTimetablesAsync(string normalDirectionLink,
                                                             string reverseDirectionLink,
                                                             int busLineId)
        {
            try
            {
                var lastUpdated = string.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);

                // Get bus stations for normal direction
                var busStationsNormalDirection = await _busApi.GetBusStations(normalDirectionLink);

                busStationsNormalDirection.ForEach(b => b.Direction = RouteDirections.Normal);

                // Get bus stations for reverse direction
                var busStationsReverseDirection = await _busApi.GetBusStations(reverseDirectionLink);

                busStationsReverseDirection.ForEach(b => b.Direction = RouteDirections.Reverse);

                // Concatenate bus stations for both directions
                var busStations = busStationsNormalDirection.Concat(busStationsReverseDirection);

                await InsertBusStationsInDatabaseAsync(busStations,
                                                       busLineId,
                                                       lastUpdated);

                foreach (var busStation in busStations)
                {
                    var busTimetables = await _busApi.GetBusTimeTables(busStation.ScheduleLink);

                    await InsertBusTimetablesInDatabaseAsync(busTimetables,
                                                             busStation.Id ?? 0,
                                                             lastUpdated);
                }
            }
            catch (ValidationApiException validationException)
            {
                throw validationException;
            }
            catch (ApiException apiException)
            {
                throw apiException;
            }
            catch (Exception exeption)
            {
                throw exeption;
            }
        }

        #endregion

        #region Methods

        private async Task InsertBusLinesInDatabaseAsync(IEnumerable<BusLineModel> busLines,
                                                         string lastUpdated)
        {
            foreach (var busLine in busLines)
            {
                busLine.LastUpdateDate = lastUpdated;
            }

            await _busDataService.InsertOrReplaceBusLinesAsync(busLines);
        }

        private async Task InsertBusStationsInDatabaseAsync(IEnumerable<BusStationModel> busStations,
                                                            int busLineId,
                                                            string lastUpdated,
                                                            string? direction = null)
        {
            foreach (var busStation in busStations)
            {
                // Add foreign key and direction before inserting in database
                busStation.BusLineId = busLineId;
                busStation.LastUpdateDate = lastUpdated;

                if (direction != null)
                {
                    busStation.Direction = direction;
                }
            }

            await _busDataService.InsertOrReplaceBusStationsAsync(busStations);
        }

        private async Task InsertBusTimetablesInDatabaseAsync(IEnumerable<BusTimeTableModel> busTimeTables,
                                                              int busStationId,
                                                              string lastUpdated)
        {
            foreach (var busTimetableHour in busTimeTables)
            {
                // Add foreign key before inserting in database
                busTimetableHour.BusStationId = busStationId;
                busTimetableHour.LastUpdateDate = lastUpdated;
            }

            await _busDataService.InsertOrReplaceBusTimeTablesAsync(busTimeTables);
        }

        #endregion
    }
}