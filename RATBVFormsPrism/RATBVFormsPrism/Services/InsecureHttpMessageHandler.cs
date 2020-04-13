using System.Net.Http;
using RATBVFormsPrism.Services;

namespace RATBVFormsPrism.Services
{
    public class InsecureHttpMessageHandler : DelegatingHandler, ICustomHttpMessageHandler
    {
        #region Constructors

        /// <summary>
        /// Implementation to bypass the SSL problem when debugging with local web api server
        /// </summary>
        public InsecureHttpMessageHandler()
        {
            InnerHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (cert.Issuer.Equals("CN=localhost"))
                    {
                        return true;
                    }

                    return errors == System.Net.Security.SslPolicyErrors.None;
                }
            };
        }

        #endregion
    }
}