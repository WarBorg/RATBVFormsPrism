using Prism.Commands;
using Prism.Navigation;
using RATBVFormsPrism.Models;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Views;

namespace RATBVFormsPrism.ViewModels
{
    public class BusStationViewModel : BusViewModelBase
    {
        #region Members

        private readonly INavigationService _navigationService;

        private readonly BusStationModel _busStation;

        #endregion Members

        #region Properties

        public int Id { get; set; }
        public string Name { get; set; }
        public string SchedualLink { get; set; }

        #region Commands

        public DelegateCommand ShowSelectedBusTimeTableCommand
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    if (IsInternetAvailable())
                    {
                        var parameters = new NavigationParameters();
                        parameters.Add(AppNavigation.BusStation, _busStation);

                        //await _navigationService.NavigateAsync<BusTimeTableViewModel>(parameters);
                        await _navigationService.NavigateAsync(nameof(BusTimeTable), parameters);
                    }
                });
            }
        }

        #endregion Commands

        #endregion Properties

        #region Constructors

        public BusStationViewModel(BusStationModel busStation, INavigationService navigationService)
        {
            _busStation = busStation;
            _navigationService = navigationService;

            Id = (int)_busStation.Id;
            Name = _busStation.Name;
            SchedualLink = _busStation.SchedualLink;
        }
        
        #endregion Constructors
    }
}
