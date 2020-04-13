using System.Collections.Generic;
using System.Threading.Tasks;
using RATBVData.Models.Models;

namespace RATBVFormsPrism.Services
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

        Task DownloadAllStationsTimetablesAsync(string normalDirectionLink,
                                                string reverseDirectionLink,
                                                int busLineId);
    }
}
