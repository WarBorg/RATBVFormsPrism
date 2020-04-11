using System.Collections.Generic;
using System.Threading.Tasks;
using RATBVData.Models.Models;

namespace RATBVFormsPrism.Interfaces
{
    public interface IBusRepository
    {
        Task<List<BusLineModel>> GetBusLinesAsync(bool isForcedRefresh);
        Task<List<BusStationModel>> GetBusStationsAsync(string lineNumberLink);
        Task<List<BusTimeTableModel>> GetBusTimeTableAsync(string schedualLink);
    }
}
