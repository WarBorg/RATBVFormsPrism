using System;

namespace RATBVFormsPrism.Services
{
    public interface IHttpServiceOptions
    {
        string BaseUrl { get; }
        TimeSpan Timeout { get; }
    }
}