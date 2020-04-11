using System;

namespace RATBVFormsPrism.Interfaces
{
    public interface IHttpServiceOptions
    {
        string BaseUrl { get; }
        TimeSpan Timeout { get; }
    }
}