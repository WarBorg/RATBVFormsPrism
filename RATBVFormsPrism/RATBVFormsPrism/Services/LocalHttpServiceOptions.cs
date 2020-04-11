using System;
using RATBVFormsPrism.Interfaces;
using Xamarin.Essentials;

namespace RATBVFormsPrism.Services
{
    public class LocalHttpServiceOptions : IHttpServiceOptions
    {
        #region Constants

        private const string IOSLocalBaseAddress = "https://localhost:5001/api";
        private const string AndroidLocalBaseAddress = "https://10.0.2.2:5001/api";

        #endregion

        #region Properties

        public string BaseUrl
        {
            get => DeviceInfo.Platform == DevicePlatform.Android
                                        ? AndroidLocalBaseAddress
                                        : IOSLocalBaseAddress;
        }

        public TimeSpan Timeout
        {
            get => TimeSpan.FromSeconds(5);
        }

        #endregion
    }
}