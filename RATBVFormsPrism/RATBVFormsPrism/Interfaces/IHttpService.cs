using System.Net.Http;

namespace RATBVFormsPrism.Interfaces
{
    public interface IHttpService
    {
        HttpClient HttpClient { get; }
    }
}
