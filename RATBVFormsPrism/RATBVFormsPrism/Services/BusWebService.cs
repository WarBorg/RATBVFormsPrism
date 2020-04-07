using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using RATBVData.Models.Models;
using RATBVFormsPrism.Interfaces;
using Refit;
using Xamarin.Essentials;

namespace RATBVFormsPrism.Services
{
    public class BusWebService : IBusWebService
    {
        #region Dependencies

        private readonly IBusApi _busApi;

        #endregion

        #region Constructors

        public BusWebService()
        {
            const string IOSLocalBaseAddress = "https://localhost:5001/api";
            const string AndroidLocalBaseAddress = "https://10.0.2.2:5001/api";
            const string RemoteApiBaseAddress = "https://ratbvwebapi.azurewebsites.net/api";


            string baseLocalAddress = DeviceInfo.Platform == DevicePlatform.Android
                                                           ? AndroidLocalBaseAddress
                                                           : IOSLocalBaseAddress;

            // To be used as a param for RestService.For<IBusApi> when debugging locally
            var localHttpClient = new HttpClient(GetInsecureHandler())
            {
                BaseAddress = new Uri(baseLocalAddress),
                Timeout = TimeSpan.FromSeconds(30)
            };

            _busApi = RestService.For<IBusApi>(RemoteApiBaseAddress);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method to get passed the SSL problem when debugging locally
        /// </summary>
        /// <returns>HTTP Handler that bypasses SSL verification</returns>
        private HttpClientHandler GetInsecureHandler()
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (cert.Issuer.Equals("CN=localhost"))
                    {
                        return true;
                    }

                    return errors == System.Net.Security.SslPolicyErrors.None;
                }
            };
        }

        #endregion

        #region IBusWebService Methods

        public async Task<List<BusLineModel>> GetBusLinesAsync()
        {
            return await _busApi.GetBusLines();
        }

        public async Task<List<BusStationModel>> GetBusStationsAsync(string lineNumberLink)
        {
            return await _busApi.GetBusStations(lineNumberLink);
        }

        public async Task<List<BusTimeTableModel>> GetBusTimeTableAsync(string schedualLink)
        {
            return await _busApi.GetBusTimeTables(schedualLink);
        }

        #endregion
    }
}