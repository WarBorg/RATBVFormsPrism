﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RATBVData.Models.Models;
using RATBVFormsPrism.Interfaces;
using Refit;

namespace RATBVFormsPrism.Services
{
    public class BusRepository : IBusRepository
    {
        #region Dependencies

        private readonly IBusApi _busApi;
        private readonly IBusDataService _busDataService;

        #endregion

        #region Constructors

        public BusRepository(IBusApi busApi,
                             IBusDataService busDataService)
        {
            _busApi = busApi;
            _busDataService = busDataService;
        }

        #endregion

        #region IBusWebService Methods

        public async Task<List<BusLineModel>> GetBusLinesAsync(bool isForcedRefresh)
        {
            try
            {
                var busLinesNumber = await _busDataService.CountBusLines;

                if (isForcedRefresh || busLinesNumber == 0)
                {
                    var busLines = await _busApi.GetBusLines();

                    var lastUpdated = string.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);

                    await InsertBusLinesInDatabaseAsync(busLines, lastUpdated);
                }

                return await _busDataService.GetBusLinesByNameAsync();
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

        public async Task<List<BusStationModel>> GetBusStationsAsync(string lineNumberLink,
                                                                     string direction,
                                                                     int busLineId,
                                                                     bool isForcedRefresh)
        {
            try
            {
                var busStationsCount = await _busDataService.CountBusStationsAsync(busLineId, direction);

                if (isForcedRefresh || (busStationsCount == 0))
                {
                        var busStations = await _busApi.GetBusStations(lineNumberLink);

                    var lastUpdated = string.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);

                    await InsertBusStationsInDatabaseAsync(busStations, direction, busLineId, lastUpdated);
                }
                
                return await _busDataService.GetBusStationsByNameAsync(busLineId, direction);
            }
            catch (ValidationApiException validationException)
            {
                throw validationException;
            }
            catch (ApiException exception)
            {
                throw exception;
            }
        }

        public async Task<List<BusTimeTableModel>> GetBusTimeTableAsync(string schedualLink)
        {
            try
            {
                return await _busApi.GetBusTimeTables(schedualLink);
            }
            catch (ValidationApiException validationException)
            {
                throw validationException;
            }
            catch (ApiException exception)
            {
                throw exception;
            }
        }

        #endregion

        #region Methods

        private async Task InsertBusLinesInDatabaseAsync(List<BusLineModel> busLines,
                                                         string lastUpdate)
        {
            busLines.ForEach(b => b.LastUpdateDate = lastUpdate);

            await _busDataService.InsertOrReplaceBusLinesAsync(busLines);
        }

        private async Task InsertBusStationsInDatabaseAsync(List<BusStationModel> busStations,
                                                            string direction,
                                                            int busLineId,
                                                            string lastUpdate)
        {
            foreach (var busStation in busStations)
            {
                // Add foreign key and direction before inserting in database
                busStation.BusLineId = busLineId;
                busStation.Direction = direction;
                busStation.LastUpdateDate = lastUpdate;
            }

            await _busDataService.InsertOrReplaceBusStationsAsync(busStations);
        }

        #endregion
    }
}