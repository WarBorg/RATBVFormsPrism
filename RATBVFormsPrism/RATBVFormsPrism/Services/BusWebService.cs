using HtmlAgilityPack;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.Models;
using RATBVFormsPrism.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

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

                //await Task.Delay(5000);
                Thread.

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
                        string linkReverseWay = string.Empty;
                        
                        // Check if is in new format or not
                        if (linkNormalWay.Contains("dus")) // Old format
                        {
                            linkReverseWay = linkNormalWay.Replace("dus", "intors");
                        }
                        else // New Format
                        {
                            linkNormalWay += "timetable/";
                            linkReverseWay = linkNormalWay.Replace("tour", "retour");
                        }

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

        #endregion Bus Lines

        #region Bus Stations

        public async Task<List<BusStationModel>> GetBusStationsAsync(string lineNumberLink)
        {
            var lineNumberStationsLink = string.Empty;

            var busStations = new List<BusStationModel>();

            if (lineNumberLink.Contains("dus") || lineNumberLink.Contains("intors")) // Old format
            {
                lineNumberStationsLink = await GetBusMainDisplayAsync(lineNumberLink, true);

                return await GetBusStationsAsyncOld(lineNumberStationsLink, busStations);
            }

            // New format
            return await GetBusStationsAsyncNew(lineNumberLink, busStations);
        }

        private async Task<List<BusStationModel>> GetBusStationsAsyncOld(string lineNumberStationsLink, List<BusStationModel> busStations)
        {   
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

        private async Task<List<BusStationModel>> GetBusStationsAsyncNew(string lineNumberStationsLink, List<BusStationModel> busStations)
        {
            string url = String.Format("{0}{1}", "http://www.ratbv.ro", lineNumberStationsLink);

            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            var response = (HttpWebResponse)await httpWebRequest.GetResponseAsync();

            Stream responseStream = response.GetResponseStream();

            HtmlDocument doc = new HtmlDocument();

            doc.Load(response.GetResponseStream());

            var divStations = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.Attributes.Contains("class") &&
                            x.Attributes["class"].Value.Contains("box butoane-statii"))
                .SingleOrDefault();

            var stations = divStations
                .Descendants("a")
                .ToList();

            foreach (HtmlNode station in stations)
            {
                string stationName = station.InnerText.Trim();
                string fullSchedualLink = station.Attributes["href"].Value;

                busStations.Add(new BusStationModel
                {
                    Name = stationName,
                    SchedualLink = fullSchedualLink
                });
            }

            return busStations;
        }

        #endregion Bus Stations

        #region Bus Time Table

        public async Task<List<BusTimeTableModel>> GetBusTimeTableAsync(string schedualLink)
        {
            var busTimeTable = new List<BusTimeTableModel>();

            if (schedualLink.Contains("dus") || schedualLink.Contains("intors"))
                return await GetBusTimeTableAsyncOld(schedualLink, busTimeTable);

            return await GetBusTimeTableAsyncNew(schedualLink, busTimeTable);
        }

        private async Task<List<BusTimeTableModel>> GetBusTimeTableAsyncOld(string schedualLink, List<BusTimeTableModel> busTimeTable)
        {
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

                if (string.IsNullOrEmpty(hour) && string.IsNullOrEmpty(minutes))
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

        private async Task<List<BusTimeTableModel>> GetBusTimeTableAsyncNew(string schedualLink, List<BusTimeTableModel> busTimeTable)
        {
            string url = String.Format("{0}{1}", "http://www.ratbv.ro", schedualLink);

            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            var response = (HttpWebResponse)await httpWebRequest.GetResponseAsync();

            Stream responseStream = response.GetResponseStream();

            HtmlDocument doc = new HtmlDocument();

            doc.Load(response.GetResponseStream());

            var tableSchedual = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.Attributes.Contains("class") &&
                            x.Attributes["class"].Value.Contains("box tabel-statii"))
                .SingleOrDefault()
                .Descendants("table")
                .SingleOrDefault();

            foreach (HtmlNode hourLine in tableSchedual.Descendants("tr").Skip(1))
            {
                var hourLineDetails = hourLine.Descendants("td").ToList();

                // Skip the lines that don't have a schedule in them
                if (hourLineDetails.Count < 5)
                    continue;

                string hour = hourLineDetails[0].InnerText.Replace('\n', ' ').Replace(" ", String.Empty).Replace("&nbsp;", " ").Trim();
                string minutesWeekdays = hourLineDetails[1].InnerText.Replace('\n', ' ').Replace(" ", String.Empty).Replace("&nbsp;", " ").Trim();
                string minutesSaturday = hourLineDetails[2].InnerText.Replace('\n', ' ').Replace(" ", String.Empty).Replace("&nbsp;", " ").Trim();
                string minutesSunday = hourLineDetails[3].InnerText.Replace('\n', ' ').Replace(" ", String.Empty).Replace("&nbsp;", " ").Trim();
                string minutesSummerHoliday = hourLineDetails[4].InnerText.Replace('\n', ' ').Replace(" ", String.Empty).Replace("&nbsp;", " ").Trim();

                if (hour != String.Empty && minutesWeekdays != String.Empty)
                {
                    busTimeTable.Add(new BusTimeTableModel
                    {
                        Hour = hour,
                        Minutes = minutesWeekdays,
                        TimeOfWeek = TimeOfTheWeek.WeekDays
                    });
                }

                if (hour != String.Empty && minutesSaturday != String.Empty)
                {
                    busTimeTable.Add(new BusTimeTableModel
                    {
                        Hour = hour,
                        Minutes = minutesSaturday,
                        TimeOfWeek = TimeOfTheWeek.Saturday
                    });
                }

                if (hour != String.Empty && minutesSunday != String.Empty)
                {
                    busTimeTable.Add(new BusTimeTableModel
                    {
                        Hour = hour,
                        Minutes = minutesSunday,
                        TimeOfWeek = TimeOfTheWeek.Sunday
                    });
                }

                if (hour != String.Empty && minutesSummerHoliday != String.Empty)
                {
                    busTimeTable.Add(new BusTimeTableModel
                    {
                        Hour = hour,
                        Minutes = minutesSummerHoliday,
                        TimeOfWeek = TimeOfTheWeek.WeekDaysHoliday
                    });
                }

                hour = minutesWeekdays = minutesSaturday = minutesSunday = minutesSummerHoliday = String.Empty;
            }

            return busTimeTable;
        }

        #endregion Bus Time Table
        
        #endregion Methods
    }
}
