using System.Net.Http;
using RATBVFormsPrism.Interfaces;

namespace RATBVFormsPrism.Services
{
    public class DefaultHttpMessageHandler : DelegatingHandler, ICustomHttpMessageHandler
    {
        #region Constructors

        public DefaultHttpMessageHandler()
        {
            InnerHandler = new HttpClientHandler();
        }

        #endregion
    }
}