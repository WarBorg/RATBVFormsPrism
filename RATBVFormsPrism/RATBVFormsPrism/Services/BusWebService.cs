using HtmlAgilityPack;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RATBVFormsPrism.Services
{
    public class BusWebService : IBusWebService
    {
        #region Methods

        #region Bus Lines

        public async Task<List<BusLineModel>> GetBusLinesAsync()
        {
            var id = 1;

            var busLines = new List<BusLineModel>();

            string url = "http://www.ratbv.ro/trasee-si-orare/";

            try
            {
                var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

                var response = (HttpWebResponse)await httpWebRequest.GetResponseAsync();

                Stream responseStream = response.GetResponseStream();

                var doc = new HtmlDocument();

                doc.Load(response.GetResponseStream());

                var div = doc.DocumentNode
                    .Descendants("div")
                    .Where(x => x.Attributes.Contains("class") &&
                                x.Attributes["class"].Value.Contains("box continut-pagina"))
                    .SingleOrDefault();

                var table = div.Element("table");

                // Skip one because of the bus type titles on first row
                var busLinesRows = table.Element("tbody").Elements("tr").Skip(1);

                foreach (var row in busLinesRows)
                {
                    var items = row.Elements("td").ToArray();

                    var line = string.Empty;
                    var route = string.Empty;
                    var type = string.Empty;

                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i].InnerText.Trim().Replace("&nbsp;", string.Empty).Length == 0)
                            continue;

                        var str = items[i].InnerText
                            .Trim()
                            .Replace("&nbsp;", " ")
                            .Replace("&acirc;", "â");

                        string linkNormalWay = items[i].Descendants("a").FirstOrDefault().Attributes["href"].Value;
                        string linkReverseWay = linkNormalWay.Replace("dus", "intors");

                        CleanUpBusLinesText(ref line, ref route, ref type, i, str);

                        busLines.Add(new BusLineModel
                        {
                            Id = id++,
                            Name = line,
                            Route = route,
                            Type = type,
                            LinkNormalWay = linkNormalWay,
                            LinkReverseWay = linkReverseWay
                        });
                    }
                }

                return busLines;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void CleanUpBusLinesText(ref string line, ref string route, ref string type, int i, string str)
        {
            // Add a space between the eventual letter after the number (ex 5B)
            str = Regex.Replace(
                str,
                "(?<=[0-9])(?=[A-Za-z])|(?<=[A-Za-z])(?=[0-9])",
                " ");

            var line_route = str
                .Split(' ')
                .ToList();

            // In case the number contains a letter connate it back to the number (ex 5B)
            if (line_route[2].Length == 1)
            {
                line_route[1] += line_route[2];
                line_route.RemoveAt(2);
            }

            line = $"{line_route[0]} {line_route[1]}";
            
            // When creating the route skip the first two items as they are the line number
            route = line_route
                .Skip(2)
                .Aggregate((k, l) => $"{k} {l}");

            if (i == 0)
                type = BusTypes.Bus;
            else if (i == 1)
                type = BusTypes.Midibus;
            else if (i == 2)
                type = BusTypes.Trolleybus;
        }

        #endregion Bus Lines

        #region Bus Stations

        public async Task<List<BusStationModel>> GetBusStationsAsync(string lineNumberLink)
        {   
            var busStations = new List<BusStationModel>();

            var lineNumberStationsLink = await GetBusMainDisplayAsync(lineNumberLink, true);

            var url = string.Format("{0}{1}", "http://www.ratbv.ro/afisaje/", lineNumberStationsLink);

            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            var response = (HttpWebResponse)await httpWebRequest.GetResponseAsync();

            Stream responseStream = response.GetResponseStream();

            HtmlDocument doc = new HtmlDocument();

            doc.Load(response.GetResponseStream());

            var div = doc.DocumentNode
                    .Descendants("div")
                    .Where(x => x.Attributes.Contains("id") &&
                                x.Attributes["id"].Value.Contains("div_center_"))
                    .ToList();

            bool isFirstScheduleLink = true;
            string firstScheduleLink = String.Empty;

            foreach (HtmlNode station in div)
            {
                string stationName = station.InnerText.Trim();
                string scheduleLink = station.Descendants("a").FirstOrDefault().Attributes["href"].Value;
                string lineLink = lineNumberStationsLink.Substring(0, lineNumberStationsLink.IndexOf('/'));
                string fullSchedualLink = String.Format("{0}/{1}", lineLink, scheduleLink);

                // Save the first schedule link 
                if (isFirstScheduleLink)
                {
                    firstScheduleLink = scheduleLink;

                    isFirstScheduleLink = false;
                }

                if (fullSchedualLink.Contains("/../"))
                {
                    string reverseScheduleLink = firstScheduleLink;
                    string reverseLineLink = fullSchedualLink.Substring(fullSchedualLink.LastIndexOf("/") + 1).Replace(".html", String.Empty);

                    if (reverseScheduleLink.Contains("_cl1_"))
                        reverseScheduleLink = reverseScheduleLink.Replace("_cl1_", "_cl2_");
                    else if (reverseScheduleLink.Contains("_cl2_"))
                        reverseScheduleLink = reverseScheduleLink.Replace("_cl2_", "_cl1_");

                    fullSchedualLink = String.Format("{0}/{1}", reverseLineLink, reverseScheduleLink);
                }

                busStations.Add(new BusStationModel
                {
                    Name = stationName,
                    SchedualLink = fullSchedualLink
                });
            }

            return busStations;
        }

        private async Task<string> GetBusMainDisplayAsync(string lineNumberLink, bool IsStationRequest)
        {
            try
            {
                string url = String.Format("{0}{1}", "http://www.ratbv.ro", lineNumberLink);

                var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

                var response = (HttpWebResponse)await httpWebRequest.GetResponseAsync();

                Stream responseStream = response.GetResponseStream();

                HtmlDocument doc = new HtmlDocument();

                doc.Load(response.GetResponseStream());

                if (IsStationRequest)
                {
                    HtmlNode frameStations = doc.DocumentNode
                            .Descendants("frame")
                            .Where(x => x.Attributes.Contains("name") &&
                                        x.Attributes.Contains("noresize") &&
                                        x.Attributes["name"].Value.Equals("frTabs") &&
                                        x.Attributes["noresize"].Value.Equals("noresize"))
                            .SingleOrDefault();

                    return frameStations.Attributes["src"].Value;
                }
                else
                {
                    HtmlNode frameSchedual = doc.DocumentNode
                            .Descendants("frame")
                            .Where(x => x.Attributes.Contains("name") &&
                                        x.Attributes["name"].Value.Equals("MainFrame"))
                            .SingleOrDefault();

                    return frameSchedual.Attributes["src"].Value;
                }
            }
            catch (Exception)
            {
                throw;
            }
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
