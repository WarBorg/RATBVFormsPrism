using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Commands;
using Prism.Navigation;
using RATBVData.Models.Models;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Services;
using Xamarin.Forms;

namespace RATBVFormsPrism.ViewModels
{
    public class BusStationsViewModel : BusViewModelBase
    {
        #region Dependencies

        private readonly IBusRepository _busRepository;
        private readonly IUserDialogs _userDilaogsService;
        private readonly IConnectivityService _connectivityService;
        private readonly INavigationService _navigationService;

        #endregion

        #region Fields

        private string _directionLink = string.Empty;
        private BusLineModel _busLine;

        #endregion

        #region Properties

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
                _refreshCommand ??= new DelegateCommand(DoRefreshCommand, () => !IsBusy);
                return _refreshCommand;
            }
        }

        #endregion

        #region Constructors

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

        private async void DoDownloadCommand()
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

        #region Navigation Methods

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

            BusStations = busStations.Select(busStation => new BusStationViewModel(busStation, _navigationService))
                                     .ToList();
        }

        #endregion
    }
}