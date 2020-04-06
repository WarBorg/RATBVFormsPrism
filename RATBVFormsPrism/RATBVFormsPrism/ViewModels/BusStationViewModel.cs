using System.Windows.Input;
using Prism.Commands;
using Prism.Navigation;
using RATBVData.Models.Models;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Views;

namespace RATBVFormsPrism.ViewModels
{
    public class BusStationViewModel : BusViewModelBase
    {
        #region Dependencies

        private readonly INavigationService _navigationService;

        #endregion

        #region Fields

        private readonly BusStationModel _busStation;

        #endregion

        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }
        public string SchedualLink { get; set; }

        #endregion

        #region Command Properties

        public ICommand ShowSelectedBusTimeTableCommand
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    if (IsInternetAvailable)
                    {
                        var parameters = new NavigationParameters
                        {
                            { AppNavigation.BusStation, _busStation }
                        };

                        await _navigationService.NavigateAsync(nameof(BusTimeTable), parameters);
                    }
                });
            }
        }

        #endregion

        #region Constructors

        public BusStationViewModel(BusStationModel busStation,
                                   INavigationService navigationService)
        {
            _busStation = busStation;
            _navigationService = navigationService;

            Id = (int)_busStation.Id;
            Name = _busStation.Name;
            SchedualLink = _busStation.SchedualLink;
        }
        
        #endregion
    }
}
