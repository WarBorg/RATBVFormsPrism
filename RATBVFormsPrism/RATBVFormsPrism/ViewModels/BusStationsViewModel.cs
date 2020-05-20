using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Commands;
using Prism.Navigation;
using RATBVData.Models.Models;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Services;
using RATBVFormsPrism.Views;
using Xamarin.Forms;

namespace RATBVFormsPrism.ViewModels
{
    public class BusStationsViewModel : BusViewModelBase
    {
        #region Fields

        private string _directionLink = string.Empty;
        private BusLineModel _busLine;

        #endregion

        #region Properties

        public RangeObservableCollection<BusStationsItemViewModel> BusStations
        {
            get;

        } = new RangeObservableCollection<BusStationsItemViewModel>();

        private string _direction = string.Empty;
        public string Direction
        {
            get => _direction;
            set
            {
                SetProperty(ref _direction, value);

                BusLineName = _busLine == null
                                        ? string.Empty
                                        : $"{_busLine.Name} - {value}";
            }
        }

        private string _busLineName = "Linia";
        public string BusLineName
        {
            get => _busLineName;
            set => SetProperty(ref _busLineName, value);
        }

        public override string Title
        {
            get => Device.RuntimePlatform == Device.UWP
                                           ? $"Bus Stations - Updated on {LastUpdated}"
                                           : "Bus Stations";
        }

        private string _lastUpdated = "never";
        public string LastUpdated
        {
            get => _lastUpdated;
            set
            {
                SetProperty(ref _lastUpdated, value);
                RaisePropertyChanged(nameof(Title));
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion

        #region Command Properties

        private DelegateCommand _reverseTripCommand;
        public ICommand ReverseTripCommand
        {
            get => _reverseTripCommand ??= new DelegateCommand(ShowReverseTripStations);
        }

        private DelegateCommand _downloadStationsTimetablesCommand;
        public ICommand DownloadStationsTimetablesCommand
        {
            get => _downloadStationsTimetablesCommand ??= new DelegateCommand(DownloadAllStationTimetables);
        }

        private DelegateCommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get => _refreshCommand ??= new DelegateCommand(RefreshBusStations, () => !IsBusy);
        }

        #endregion

        #region Constructors and Dependencies

        private readonly IBusRepository _busRepository;
        private readonly IUserDialogs _userDilaogsService;
        private readonly IConnectivityService _connectivityService;
        private readonly INavigationService _navigationService;

        public BusStationsViewModel(IBusRepository busRepository,
                                    IUserDialogs userDialogsService,
                                    IConnectivityService connectivityService,
                                    INavigationService navigationService)
        {
            _busRepository = busRepository;
            _userDilaogsService = userDialogsService;
            _connectivityService = connectivityService;
            _navigationService = navigationService;
        }

        #endregion Constructors

        #region Command Methods

        private async void ShowReverseTripStations()
        {
            using (_userDilaogsService.Loading("Fetching Data... "))
            {
                await GetBusStationsAsync(isRefresh: false,
                                          shouldReverseWay: true);
            }
        }

        private async void RefreshBusStations()
        {
            if (_connectivityService.IsInternetAvailable)
            {
                using (_userDilaogsService.Loading("Fetching Data... "))
                {
                    await GetBusStationsAsync(isRefresh: true,
                                              shouldReverseWay: false);
                }
            }
            else
            {
                _userDilaogsService.Toast("No Internet connection detected");
            }

            IsBusy = false;
        }

        private async void DownloadAllStationTimetables()
        {
            if (!_connectivityService.IsInternetAvailable)
            {
                _userDilaogsService.Toast("Internet connection is necessary to download all bus stations time tables");

                return;
            }

            using (_userDilaogsService.Loading("Downlaoding Time Tables... "))
            {
                await _busRepository.DownloadAllStationsTimetablesAsync(_busLine.LinkNormalWay,
                                                                        _busLine.LinkReverseWay,
                                                                        _busLine.Id);
            }

            _userDilaogsService.Toast("Download complete for all bus stations time tables");
        }

        #endregion

        #region Navigation Override Methods

        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.Back)
            {
                return;
            }

            // On back request the parameter comes null
            //TODO use JSON serialization when sending data between pages
            if (parameters[AppNavigation.BusLine] is BusLineModel busline)
            {
                _busLine = busline;
            }

            using (_userDilaogsService.Loading("Fetching Data... "))
            {
                await GetBusStationsAsync(isRefresh: false,
                                          shouldReverseWay: false);
            }
        }

        #endregion

        #region Methods

        private async Task GetBusStationsAsync(bool isRefresh, bool shouldReverseWay)
        {
            if (_busLine == null)
            {
                return;
            }

            // If there is a forced user refresh we want to keep the same Direction
            if (!isRefresh)
            {
                // Initial view of the stations list should be normal way
                if (!shouldReverseWay)
                {
                    Direction = RouteDirections.Normal;

                    _directionLink = _busLine.LinkNormalWay;
                }
                else if (shouldReverseWay && Direction == RouteDirections.Normal)
                {
                    Direction = RouteDirections.Reverse;

                    _directionLink = _busLine.LinkReverseWay;
                }
                else if (shouldReverseWay && Direction == RouteDirections.Reverse)
                {
                    Direction = RouteDirections.Normal;

                    _directionLink = _busLine.LinkNormalWay;
                }
            }

            var busStations = await _busRepository.GetBusStationsAsync(_directionLink,
                                                                       Direction,
                                                                       _busLine.Id,
                                                                       isRefresh);

            LastUpdated = busStations.FirstOrDefault()
                                     .LastUpdateDate;

            BusStations.ReplaceRange(busStations.Select(busStation => new BusStationsItemViewModel(busStation,
                                                                                                   _navigationService)));
        }

        #endregion

        #region BusStationsItemViewModel Class

        public class BusStationsItemViewModel : BusViewModelBase
        {
            #region Fields

            private readonly BusStationModel _busStation;

            #endregion

            #region Properties

            public int Id { get; }
            public string Name { get; }
            public string ScheduleLink { get; }

            #endregion

            #region Command Properties

            private DelegateCommand _busStationSelectedCommand;
            public ICommand BusStationSelectedCommand
            {
                get => _busStationSelectedCommand ??= new DelegateCommand(ShowTimetablesForSelectedBusStation);
            }

            #endregion

            #region Constructors and Dependencies

            private readonly INavigationService _navigationService;

            public BusStationsItemViewModel(BusStationModel busStation,
                                            INavigationService navigationService)
            {
                _busStation = busStation;
                _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

                Id = _busStation?.Id.Value ?? 0;
                Name = _busStation?.Name;
                ScheduleLink = _busStation?.ScheduleLink;
            }

            #endregion

            #region Command Methods

            private async void ShowTimetablesForSelectedBusStation()
            {
                var parameters = new NavigationParameters
            {
                { AppNavigation.BusStation, _busStation }
            };

                await _navigationService.NavigateAsync(nameof(BusTimetables), parameters);
            }

            #endregion
        }

        #endregion
    }
}