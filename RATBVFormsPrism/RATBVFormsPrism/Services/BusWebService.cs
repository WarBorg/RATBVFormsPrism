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

        #region Constructor

        public BusWebService()
        {
            const string IOSAddress = "https://localhost:5001/api";
            const string AndroidAddress = "https://10.0.2.2:5001/api";
                                                    
            string baseAddress = DeviceInfo.Platform == DevicePlatform.Android ? AndroidAddress : IOSAddress;

            var httpClient = new HttpClient(GetInsecureHandler())
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromSeconds(30)
            };

            _busApi = RestService.For<IBusApi>(httpClient);
        }

        #endregion

        #region Methods

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