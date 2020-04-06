using System.Collections.Generic;
using System.Threading.Tasks;
using RATBVData.Models.Models;
using Refit;

namespace RATBVFormsPrism.Interfaces
{
    public interface IBusApi
    {
        [Get("/buslines")]
        Task<List<BusLineModel>> GetBusLines();

        [Get("/buslines/{lineNumber}")]
        Task<BusLineModel> GetBusLine(string lineNumber);

        [Get("/busstations/{lineNumberLink}")]
        Task<List<BusStationModel>> GetBusStations(string lineNumberLink);
    }
}
