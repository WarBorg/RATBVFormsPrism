using System;
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
        public string ScheduleLink { get; set; }

        #endregion

        #region Command Properties

        private DelegateCommand _showSelectedBusTimeTableCommand;
        public ICommand ShowSelectedBusTimeTableCommand
        {
            get
            {
                _showSelectedBusTimeTableCommand ??= new DelegateCommand(DoShowSelectedBusTimeTableCommand);
                return _showSelectedBusTimeTableCommand;
            }
        }

        #endregion

        #region Constructors

        public BusStationViewModel(BusStationModel busStation,
                                   INavigationService navigationService)
        {
            _busStation = busStation;
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            Id = _busStation?.Id.Value ?? 0;
            Name = _busStation?.Name;
            ScheduleLink = _busStation?.SchedualLink;
        }

        #endregion

        #region Command Methods

        private async void DoShowSelectedBusTimeTableCommand()
        {
            var parameters = new NavigationParameters
            {
                { AppNavigation.BusStation, _busStation }
            };

            await _navigationService.NavigateAsync(nameof(BusTimeTable), parameters);
        }

        #endregion
    }
}
