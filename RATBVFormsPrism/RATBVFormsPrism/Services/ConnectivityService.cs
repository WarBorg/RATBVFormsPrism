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
        private readonly IUserDialogs _userDialogs;

        #endregion

        #region IConnectivityService Properties

        public bool IsInternetAvailable
        {
            get
            {
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    _userDialogs.Toast("No Internet connection detected");

                    return false;
                }

                return true;
            }
        }

        #endregion

        #region Constrctors

        public ConnectivityService(IConnectivity connectivity,
                                   IUserDialogs userDialogs)
        {
            _connectivity = connectivity;
            _userDialogs = userDialogs;
        }

        #endregion
    }
}
