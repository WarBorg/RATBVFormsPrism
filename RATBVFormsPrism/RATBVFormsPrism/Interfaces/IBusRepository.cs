﻿using System.Collections.Generic;
using System.Threading.Tasks;
using RATBVData.Models.Models;

namespace RATBVFormsPrism.Interfaces
{
    public interface IBusRepository
    {
        Task<List<BusLineModel>> GetBusLinesAsync(bool isForcedRefresh);

        Task<List<BusStationModel>> GetBusStationsAsync(string directionLink,
                                                        string direction,
                                                        int busLineId,
                                                        bool isForcedRefresh);

        Task<List<BusTimeTableModel>> GetBusTimeTableAsync(string schedualLink,
                                                           int busStationId,
                                                           bool isForcedRefresh);

        Task DownloadAllStationsSchedualsAsync(string normalDirectionLink,
                                               string reverseDirectionLink,
                                               int busLineId);
    }
}
