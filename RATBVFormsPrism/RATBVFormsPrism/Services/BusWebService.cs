using System.Collections.Generic;
using System.Threading.Tasks;
using RATBVData.Models.Models;
using RATBVFormsPrism.Interfaces;
using Refit;

namespace RATBVFormsPrism.Services
{
    public class BusWebService : IBusWebService
    {
        #region Dependencies

        private readonly IBusApi _busApi;

        #endregion

        #region Constructors

        public BusWebService(IBusApi busApi)
        {
            _busApi = busApi;
        }

        #endregion

        #region IBusWebService Methods

        public async Task<List<BusLineModel>> GetBusLinesAsync()
        {
            try
            {
                return await _busApi.GetBusLines();
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

        public async Task<List<BusStationModel>> GetBusStationsAsync(string lineNumberLink)
        {
            try
            {
                return await _busApi.GetBusStations(lineNumberLink);
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
    }
}