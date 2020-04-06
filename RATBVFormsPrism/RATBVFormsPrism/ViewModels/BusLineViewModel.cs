﻿using Prism.Commands;
using Prism.Navigation;
using RATBVData.Models.Models;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Views;

namespace RATBVFormsPrism.ViewModels
{
    public class BusLineViewModel : BusViewModelBase
    {
        #region Members

        private readonly INavigationService _navigationService;

        private readonly BusLineModel _busLine;

        #endregion Members

        #region Properties

        public string Name { get; set; }
        public string Route { get; set; }

        #region Commands

        public DelegateCommand ShowSelectedBusLineStationsCommand
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    if (IsInternetAvailable())
                    {
                        var parameters = new NavigationParameters();
                        parameters.Add(AppNavigation.BusLine, _busLine);

                        //await _navigationService.Navigate($"{nameof(RATBVNavigation)}/{nameof(BusLines)}/{nameof(BusStations)}", parameters);
                        //await _navigationService.NavigateAsync<BusStationsViewModel>(parameters);
                        await _navigationService.NavigateAsync(nameof(BusStations), parameters);
                    }
                });
            }
        }

        #endregion Commands

        #endregion Properties

        #region Constructors

        public BusLineViewModel(BusLineModel busLine, INavigationService navigationService)
        {
            _busLine = busLine;
            _navigationService = navigationService;

            Name = _busLine.Name;
            Route = _busLine.Route;
        }
        
        #endregion Constructors
    }
}
