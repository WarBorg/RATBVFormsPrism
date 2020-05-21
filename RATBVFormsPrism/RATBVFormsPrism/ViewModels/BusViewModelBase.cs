using Prism.Mvvm;
using Prism.Navigation;

namespace RATBVFormsPrism.ViewModels
{
    public class BusViewModelBase : BindableBase, INavigationAware
    {
        #region Properties

        public virtual string Title { get; set; } = string.Empty;

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
