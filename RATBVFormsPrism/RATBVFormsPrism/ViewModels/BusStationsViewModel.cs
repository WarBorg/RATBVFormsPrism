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
        private readonly IBusRepository _busRepository;
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
                                    IBusRepository busRepository,
                                    IUserDialogs userDialogsService,
                                    IConnectivityService connectivityService,
                                    INavigationService navigationService)
        {
            _busDataService = busDataService;
            _busRepository = busRepository;
            _userDilaogsService = userDialogsService;
            _connectivityService = connectivityService;
            _navigationService = navigationService;
        }

        #endregion Constructors

        #region Command Methods

        private async void DoReverseCommand()
        {
            using (_userDilaogsService.Loading("Fetching Data... "))
            {
                await GetBusStationsAsync(isRefresh: false,
                                          shouldReverseWay: true);
            }
        }

        private async void DoRefreshCommand()
        {
            if (_connectivityService.IsInternetAvailable)
            {
                await GetBusStationsAsync(isRefresh: true,
                                          shouldReverseWay: false);
            }

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
            if (BusLine == null)
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

                    _linkDirection = BusLine.LinkNormalWay;
                }
                else if (shouldReverseWay && Direction == RouteDirections.Normal)
                {
                    Direction = RouteDirections.Reverse;

                    _linkDirection = BusLine.LinkReverseWay;
                }
                else if (shouldReverseWay && Direction == RouteDirections.Reverse)
                {
                    Direction = RouteDirections.Normal;

                    _linkDirection = BusLine.LinkNormalWay;
                }
            }

            var busStations = await _busRepository.GetBusStationsAsync(_linkDirection,
                                                                       Direction,
                                                                       BusLine.Id,
                                                                       isRefresh);

            LastUpdated = busStations.FirstOrDefault()
                                     .LastUpdateDate;

            BusStations = busStations.Select(busStation => new BusStationViewModel(busStation, _navigationService))
                                     .ToList();
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
                var busTimetables = await _busRepository.GetBusTimeTableAsync(busStation.SchedualLink);

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