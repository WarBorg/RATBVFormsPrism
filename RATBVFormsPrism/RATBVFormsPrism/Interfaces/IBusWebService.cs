using System.Collections.Generic;
using System.Threading.Tasks;
using RATBVData.Models.Models;

namespace RATBVFormsPrism.Interfaces
{
    public interface IBusWebService
    {
        Task<List<BusLineModel>> GetBusLinesAsync();
        Task<List<BusStationModel>> GetBusStationsAsync(string lineNumberLink);
        Task<List<BusTimeTableModel>> GetBusTimeTableAsync(string schedualLink);
    }
}
