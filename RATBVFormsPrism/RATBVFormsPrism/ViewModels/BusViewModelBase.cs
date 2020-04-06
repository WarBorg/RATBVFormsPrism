using Acr.UserDialogs;
using Plugin.Connectivity;
using Prism.Mvvm;
using Prism.Navigation;

namespace RATBVFormsPrism.ViewModels
{
    public class BusViewModelBase : BindableBase, INavigationAware
    {
        public virtual string Title { get; set; }

        public virtual void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public virtual void OnNavigatedTo(NavigationParameters parameters)
        {
        }

        public virtual void OnNavigatingTo(NavigationParameters parameters)
        {
        }

        protected bool IsInternetAvailable()
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
}
