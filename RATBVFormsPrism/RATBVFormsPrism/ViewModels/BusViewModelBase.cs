//using RATBVForms.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;

using Acr.UserDialogs;
using Plugin.Connectivity;
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

        protected bool IsInternetAvailable()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                //UserDialogs.Instance.ErrorToast("No Internet connection detected");
                UserDialogs.Instance.ShowError("No Internet connection detected");

                return false;
            }

            return true;
        }
    }
}
