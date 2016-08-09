using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism.Commands;
using Prism.Navigation;
using Xamarin.Forms;

using RATBVFormsPrism.Constants;
//using RATBVFormsPrism.Core.Interfaces;
//using RATBVFormsPrism.Core.ViewModels;
using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.Models;
using Acr.UserDialogs;

namespace RATBVFormsPrism.ViewModels
{
    public class BusStationsViewModel : BusViewModelBase
    {
        #region Members

        private readonly INavigationService _navigationService;
        private readonly IBusDataService _busDataService;
        private readonly IBusWebService _busWebService;

        private string _direction = string.Empty;
        private string _linkDirection = string.Empty;

        private BusLineModel _busLine;

        private List<BusStationViewModel> _busStations;

        private string _busLineName = "Linia";
        private string _lastUpdated = "never";

        private bool _isBusy;

        private DelegateCommand _reverseCommand;
        private DelegateCommand _refreshCommand;
        private DelegateCommand _downloadCommand;

        #endregion Members

        #region Properties

        public BusLineModel BusLine
        {
            get { return _busLine; }
            set 
            { 
                SetProperty(ref _busLine, value);
                //BusLineName = value == null ? string.Empty : $"{value.Name} - {Direction}");
            }
        }

        public string Direction
        {
            get { return _direction; }
            set 
            { 
                SetProperty(ref _direction, value);
                BusLineName = BusLine == null ? string.Empty : $"{BusLine.Name} - {value}";
            }
        }

        public string BusLineName
        {
            get { return _busLineName; }
            set { SetProperty(ref _busLineName, value); }
        }

        public List<BusStationViewModel> BusStations
        {
            get { return _busStations; }
            set { SetProperty(ref _busStations, value); }
        }

        public override string Title
        {
            get
            {
                if (Device.OS == TargetPlatform.WinPhone)
                    return $"Bus Stations - Updated on {LastUpdated}";

                return "Bus Stations";
            }
        }

        public string LastUpdated
        {
            get { return _lastUpdated; }
            set
            {
                SetProperty(ref _lastUpdated, value);
                OnPropertyChanged(() => Title);
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        #region Commands

        public ICommand ReverseCommand
        {
            get
            {
                _reverseCommand = _reverseCommand ?? new DelegateCommand(DoReverseCommand);
                return _reverseCommand;
            }
        }

        public ICommand DownloadCommand
        {
            get
            {
                _downloadCommand = _downloadCommand ?? new DelegateCommand(DoDownloadCommand);
                return _downloadCommand;
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                _refreshCommand = _refreshCommand ?? new DelegateCommand(DoRefreshCommand, () => { return !IsBusy; });
                return _refreshCommand;
            }
        }

        #endregion Commands

        #endregion Properties

        #region Constructors

        public BusStationsViewModel(IBusDataService busDataService, IBusWebService busWebService, INavigationService navigationService)
        {
            _busDataService = busDataService;
            _busWebService = busWebService;
            _navigationService = navigationService;
        }
        
        #endregion Constructors

        #region Methods

        #region Commands

        private async void DoReverseCommand()
        {
            await GetBusStationsAsync(false, true);
        }

        private async void DoRefreshCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            _refreshCommand.RaiseCanExecuteChanged();

            await GetBusStationsAsync(true);

            IsBusy = false;
            _refreshCommand.RaiseCanExecuteChanged();
        }

        private async void DoDownloadCommand()
        {
            if (!IsInternetAvailable())
                return;

            await DownloadAllStationsSchedualsAsync();

            //UserDialogs.Instance.SuccessToast("Download complete for all bus stations");
            UserDialogs.Instance.ShowSuccess("Download complete for all bus stations");
        }
        
        #endregion Commands

        #region Navigation

        public override void OnNavigatedFrom(NavigationParameters parameters)
        {

        }

        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            //TODO use JSON serialization when sending data between pages
            BusLine = parameters[AppNavigation.BusLine] as BusLineModel;
            
            await GetBusStationsAsync();
        }

        #endregion Navigation

        private async Task GetBusStationsAsync(bool isRefresh = false, bool isReverse = false)
        {
            if (BusLine == null)
                return;

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
                await GetWebBusStationsAsync(_linkDirection, Direction);
            else
                await GetLocalBusStationsAsync();
        }

        private async Task GetWebBusStationsAsync(string linkDirection, string direction)
        {
            if (!IsInternetAvailable())
                return;

            List<BusStationModel> busStations = await _busWebService.GetBusStationsAsync(linkDirection);

            if (busStations == null)
                return;

            LastUpdated = String.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);

            await AddBusStationsToDatabaseAsync(busStations, direction);
            
            BusStations = busStations.Select(busStation => new BusStationViewModel(busStation, _navigationService)).ToList();
        }

        private async Task GetLocalBusStationsAsync()
        {
            List<BusStationModel> busStations = await _busDataService.GetBusStationsByNameAsync(BusLine.Id, Direction);

            if (busStations == null)
                return;

            BusStations = busStations.Select(busStation => new BusStationViewModel(busStation, _navigationService)).ToList();

            LastUpdated = busStations.FirstOrDefault().LastUpdateDate;
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
            if (!IsInternetAvailable())
                return;

            var lastUpdatedTimeTable = String.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);

            foreach (var busStation in BusStations)
            {
                List<BusTimeTableModel> busTimetables = await _busWebService.GetBusTimeTableAsync(busStation.SchedualLink);

                foreach (var busTimetableHour in busTimetables)
                {
                    // Add foreign key before inserting in database
                    busTimetableHour.BusStationId = busStation.Id;
                    busTimetableHour.LastUpdateDate = lastUpdatedTimeTable;
                }

                await _busDataService.InsertOrReplaceBusTimeTablesAsync(busTimetables);
            }
        }

        #endregion Methods
    }
}
