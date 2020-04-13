using System.Net.Http;
using RATBVFormsPrism.Services;

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