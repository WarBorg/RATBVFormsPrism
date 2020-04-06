using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RATBVFormsPrism.Models;
using Refit;

namespace RATBVFormsPrism.Interfaces
{
    public interface IBusApi
    {
        [Get("/buslines")]
        Task<List<BusLineModel>> GetBusLines();

        [Get("/buslines/{lineNumber}")]
        Task<BusLineModel> GetBusLine(string lineNumber);
    }
}
