using Acr.UserDialogs;
using RATBVFormsPrism.Interfaces;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace RATBVFormsPrism.Services
{
    public class ConnectivityService : IConnectivityService
    {
        #region Dependencies

        private readonly IConnectivity _connectivity;
        
        #endregion

        #region IConnectivityService Properties

        public bool IsInternetAvailable
        {
            get => _connectivity.NetworkAccess == NetworkAccess.Internet;
        }

        #endregion

        #region Constrctors

        public ConnectivityService(IConnectivity connectivity)
        {
            _connectivity = connectivity;
        }

        #endregion
    }
}
