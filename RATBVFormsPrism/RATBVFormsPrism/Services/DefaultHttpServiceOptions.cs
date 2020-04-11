using System;
using RATBVFormsPrism.Interfaces;

namespace RATBVFormsPrism.Services
{
    public class DefaultHttpServiceOptions : IHttpServiceOptions
    {
        #region Constants

        private const string RemoteApiBaseAddress = "https://ratbvwebapi.azurewebsites.net/api";

        #endregion

        #region Properties

        public string BaseUrl
        {
            get => RemoteApiBaseAddress;
        }

        public TimeSpan Timeout
        {
            get => TimeSpan.FromSeconds(30);
        }

        #endregion
    }
}