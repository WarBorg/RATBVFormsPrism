using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Commands;
using Prism.Navigation;
using RATBVData.Models.Models;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Interfaces;
using Xamarin.Forms;

namespace RATBVFormsPrism.ViewModels
{
    public class BusStationsViewModel : BusViewModelBase
    {
        #region Dependencies

        private readonly IBusDataService _busDataService;
        private readonly IBusRepository _busWebService;
        private readonly IUserDialogs _userDilaogsService;
        private readonly IConnectivityService _connectivityService;
        private readonly INavigationService _navigationService;

        #endregion

        #region Fields

        private string _linkDirection = string.Empty;

        #endregion

        #region Properties

        private BusLineModel _busLine;
        public BusLineModel BusLine
        {
            get => _busLine;
            set => SetProperty(ref _busLine, value);
        }

        private string _direction = string.Empty;
        public string Direction
        {
            get => _direction;
            set
            {
                SetProperty(ref _direction, value);

                BusLineName = BusLine == null
                                       ? string.Empty
                                       : $"{BusLine.Name} - {value}";
            }
        }

        private string _busLineName = "Linia";
        public string BusLineName
        {
            get => _busLineName;
            set => SetProperty(ref _busLineName, value);
        }

        private List<BusStationViewModel> _busStations;
        public List<BusStationViewModel> BusStations
        {
            get => _busStations;
            set => SetProperty(ref _busStations, value);
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

        private DelegateCommand _reverseCommand;
        public ICommand ReverseCommand
        {
            get
            {
                _reverseCommand ??= new DelegateCommand(DoReverseCommand);
                return _reverseCommand;
            }
        }

        private DelegateCommand _downloadCommand;
        public ICommand DownloadCommand
        {
            get
            {
                _downloadCommand ??= new DelegateCommand(DoDownloadCommand);
                return _downloadCommand;
            }
        }

        private DelegateCommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                _refreshCommand ??= new DelegateCommand(DoRefreshCommand);
                return _refreshCommand;
            }
        }

        #endregion

        #region Constructors

        public BusStationsViewModel(IBusDataService busDataService,
                                    IBusRepository busWebService,
                                    IUserDialogs userDialogsService,
                                    IConnectivityService connectivityService,
                                    INavigationService navigationService)
        {
            _busDataService = busDataService;
            _busWebService = busWebService;
            _userDilaogsService = userDialogsService;
            _connectivityService = connectivityService;
            _navigationService = navigationService;
        }

        #endregion Constructors

        #region Command Methods

        private async void DoReverseCommand() => await GetBusStationsAsync(false, true);

        private async void DoRefreshCommand()
        {
            await GetBusStationsAsync(true);

            IsBusy = false;
        }

        private async void DoDownloadCommand()
        {
            if (!_connectivityService.IsInternetAvailable)
            {
                return;
            }

            await DownloadAllStationsSchedualsAsync();

            _userDilaogsService.Toast("Download complete for all bus stations");
        }

        #endregion

        #region Navigation Methods

        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            // On back request the parameter comes null
            //TODO use JSON serialization when sending data between pages
            if (parameters[AppNavigation.BusLine] is BusLineModel busline)
            {
                BusLine = busline;
            }

            await GetBusStationsAsync();
        }

        #endregion

        #region Methods

        private async Task GetBusStationsAsync(bool isRefresh = false, bool isReverse = false)
        {
            if (BusLine == null)
            {
                return;
            }

            if (!isRefresh)
            {
                // Initial view of the stations list should be normal way
                if (!isReverse)
                {
                    Direction = RouteDirections.Normal;

                    _linkDirection = BusLine.LinkNormalWay;
                }
                else if (isReverse && Direction == RouteDirections.Normal)
                {
                    Direction = RouteDirections.Reverse;

                    _linkDirection = BusLine.LinkReverseWay;
                }
                else if (isReverse && Direction == RouteDirections.Reverse)
                {
                    Direction = RouteDirections.Normal;

                    _linkDirection = BusLine.LinkNormalWay;
                }
            }

            var busStationsCount = await _busDataService.CountBusStationsAsync(BusLine.Id, Direction);

            if (isRefresh || (busStationsCount == 0))
            {
                if (isRefresh)
                {
                    await GetWebBusStationsAsync(_linkDirection, Direction);
                }
                else
                {
                    await GetBusStationsWithLoadingScreenAsync(_linkDirection, Direction);
                }

            }
            else
            {
                await GetLocalBusStationsAsync();
            }
        }

        private async Task GetBusStationsWithLoadingScreenAsync(string linkDirection, string direction)
        {
            using (_userDilaogsService.Loading($"Fetching Data... "))
            {
                await GetWebBusStationsAsync(linkDirection, direction);
            }
        }

        private async Task GetWebBusStationsAsync(string linkDirection, string direction)
        {
            if (!_connectivityService.IsInternetAvailable)
            {
                return;
            }

            var busStations = await _busWebService.GetBusStationsAsync(linkDirection);

            if (busStations == null)
            {
                return;
            }

            LastUpdated = string.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);

            await AddBusStationsToDatabaseAsync(busStations, direction);

            BusStations = busStations.Select(busStation => new BusStationViewModel(busStation, _navigationService))
                                     .ToList();
        }

        private async Task GetLocalBusStationsAsync()
        {
            var busStations = await _busDataService.GetBusStationsByNameAsync(BusLine.Id, Direction);

            if (busStations == null)
            {
                return;
            }

            BusStations = busStations.Select(busStation => new BusStationViewModel(busStation, _navigationService))
                                     .ToList();

            LastUpdated = busStations.FirstOrDefault()
                                     .LastUpdateDate;
        }

        private async Task AddBusStationsToDatabaseAsync(List<BusStationModel> busStations, string direction)
        {
            foreach (var busStation in busStations)
            {
                // Add foreign key and direction before inserting in database
                busStation.BusLineId = BusLine.Id;
                busStation.Direction = direction;
                busStation.LastUpdateDate = LastUpdated;
            }

            await _busDataService.InsertOrReplaceBusStationsAsync(busStations);
        }

        private async Task DownloadAllStationsSchedualsAsync()
        {
            if (!_connectivityService.IsInternetAvailable)
            {
                return;
            }

            var lastUpdatedTimeTable = string.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);

            foreach (var busStation in BusStations)
            {
                var busTimetables = await _busWebService.GetBusTimeTableAsync(busStation.SchedualLink);

                foreach (var busTimetableHour in busTimetables)
                {
                    // Add foreign key before inserting in database
                    busTimetableHour.BusStationId = busStation.Id;
                    busTimetableHour.LastUpdateDate = lastUpdatedTimeTable;
                }

                await _busDataService.InsertOrReplaceBusTimeTablesAsync(busTimetables);
            }
        }

        #endregion
    }
}