using RATBVFormsPrism.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATBVFormsPrism.Interfaces
{
    public interface IBusWebService
    {
        Task<List<BusLineModel>> GetBusLinesAsync();
        Task<List<BusStationModel>> GetBusStationsAsync(string lineNumberLink);
        Task<List<BusTimeTableModel>> GetBusTimeTableAsync(string schedualLink);
    }
}
