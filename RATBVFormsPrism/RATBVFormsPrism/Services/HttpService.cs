using System;
using System.Net.Http;
using RATBVFormsPrism.Interfaces;

namespace RATBVFormsPrism.Services
{
    public class HttpService : IHttpService
    {
        #region Properties

        public HttpClient HttpClient
        {
            get;
        }

        #endregion

        #region Constructors

        public HttpService(ICustomHttpMessageHandler customHttpMessageHandler,
                           IHttpServiceOptions httpServiceOptions)
        {
            HttpClient = new HttpClient(customHttpMessageHandler as HttpMessageHandler)
            {
                BaseAddress = new Uri(httpServiceOptions.BaseUrl),
                Timeout = httpServiceOptions.Timeout
            };
        }

        #endregion
    }
}