using Acr.UserDialogs;
using Plugin.Connectivity;
using Prism.Mvvm;
using Prism.Navigation;

namespace RATBVFormsPrism.ViewModels
{
    public class BusViewModelBase : BindableBase, INavigationAware
    {
        #region Properties

        public virtual string Title { get; set; }

        protected bool IsInternetAvailable
        {
            get
            {
                // TODO use Xamarin Essentials
                if (!CrossConnectivity.Current.IsConnected)
                {
                    UserDialogs.Instance.Toast("No Internet connection detected");

                    return false;
                }

                return true;
            }
        }

        #endregion

        #region INavigationAware Virtual Methods

        public virtual void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public virtual void OnNavigatedTo(NavigationParameters parameters)
        {
        }

        public virtual void OnNavigatingTo(NavigationParameters parameters)
        {
        }

        #endregion
    }
}
