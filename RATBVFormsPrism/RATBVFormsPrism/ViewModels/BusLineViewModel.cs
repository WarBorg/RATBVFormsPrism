using System.Windows.Input;
using Prism.Commands;
using Prism.Navigation;
using RATBVData.Models.Models;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Views;

namespace RATBVFormsPrism.ViewModels
{
    public class BusLineViewModel : BusViewModelBase
    {
        #region Dependencies

        private readonly INavigationService _navigationService;

        #endregion

        #region Fields

        private readonly BusLineModel _busLine;

        #endregion

        #region Properties

        public string Name { get; set; }
        public string Route { get; set; }

        #endregion

        #region Command Properties

        public ICommand ShowSelectedBusLineStationsCommand
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    if (IsInternetAvailable)
                    {
                        var parameters = new NavigationParameters
                        {
                            { AppNavigation.BusLine, _busLine }
                        };

                        //await _navigationService.Navigate($"{nameof(RATBVNavigation)}/{nameof(BusLines)}/{nameof(BusStations)}", parameters);
                        await _navigationService.NavigateAsync(nameof(BusStations), parameters);
                    }
                });
            }
        }

        #endregion

        #region Constructors

        public BusLineViewModel(BusLineModel busLine,
                                INavigationService navigationService)
        {
            _busLine = busLine;
            _navigationService = navigationService;

            Name = _busLine.Name;
            Route = _busLine.Route;
        }
        
        #endregion
    }
}