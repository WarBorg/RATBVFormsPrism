using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using RATBVData.Models.Models;
using RATBVFormsPrism.Constants;
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
            var handler = new HttpClientHandler();

            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost"))
                {
                    return true;
                }

                return errors == System.Net.Security.SslPolicyErrors.None;
            };

            return handler;
        }

        #region IBusWebService Methods

        public async Task<List<BusLineModel>> GetBusLinesAsync()
        {
            return await _busApi.GetBusLines();
        }

        public async Task<List<BusStationModel>> GetBusStationsAsync(string lineNumberLink)
        {
            return await _busApi.GetBusStations(lineNumberLink);
        }

        #endregion Bus Stations

        #region Bus Time Table

        public async Task<List<BusTimeTableModel>> GetBusTimeTableAsync(string schedualLink)
        {
            var busTimeTable = new List<BusTimeTableModel>();

            string url = String.Format("{0}{1}", "http://www.ratbv.ro/afisaje/", schedualLink);

            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            var response = (HttpWebResponse)await httpWebRequest.GetResponseAsync();

            Stream responseStream = response.GetResponseStream();

            HtmlDocument doc = new HtmlDocument();

            doc.Load(response.GetResponseStream());

            var tableWeekdays = doc.GetElementbyId("tabele").ChildNodes[1];
            var tableWeekend = doc.GetElementbyId("tabele").ChildNodes[3];

            // Get the time of week time table
            GetTimeTablePerTimeofWeek(busTimeTable, tableWeekdays, TimeOfTheWeek.WeekDays);
            // Get the weekend time table
            GetTimeTablePerTimeofWeek(busTimeTable, tableWeekend, TimeOfTheWeek.Saturday);
            GetTimeTablePerTimeofWeek(busTimeTable, tableWeekend, TimeOfTheWeek.Sunday);

            return busTimeTable;
        }

        private void GetTimeTablePerTimeofWeek(List<BusTimeTableModel> busTimeTable, HtmlNode tableWeekdays, string timeOfWeek)
        {
            var hour = string.Empty;
            var minutes = string.Empty;

            // Skip first three items because of time of week div, hour div and minutes div
            foreach (HtmlNode node in tableWeekdays.Descendants("div").ToList().Skip(3))
            {
                if (node.Id == "web_class_hours")
                {
                    hour = node.InnerText.Replace('\n', ' ').Trim();
                }
                else if (node.Id == "web_class_minutes")
                {
                    foreach (var minuteNode in node.Descendants("div").ToList())
                        minutes += " " + minuteNode.InnerText.Trim();
                }

                if (!string.IsNullOrEmpty(hour) && !string.IsNullOrEmpty(minutes))
                {
                    busTimeTable.Add(new BusTimeTableModel
                    {
                        Hour = hour,
                        Minutes = minutes.Substring(1),
                        TimeOfWeek = timeOfWeek
                    });

                    hour = minutes = string.Empty;
                }
            }
        }

        #endregion Bus Time Table
        
        #endregion Methods
    }
}
