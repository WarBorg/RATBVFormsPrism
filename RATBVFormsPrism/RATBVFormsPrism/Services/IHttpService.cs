using System.Net.Http;

namespace RATBVFormsPrism.Services
{
    public interface IHttpService
    {
        HttpClient HttpClient { get; }
    }
}
