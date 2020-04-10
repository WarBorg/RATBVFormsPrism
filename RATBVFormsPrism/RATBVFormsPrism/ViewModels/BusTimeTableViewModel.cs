using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Commands;
using Prism.Navigation;
using RATBVData.Models.Enums;
using RATBVData.Models.Models;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Interfaces;
using Xamarin.Forms;

namespace RATBVFormsPrism.ViewModels
{
    public class BusTimeTableViewModel : BusViewModelBase
    {
        #region Dependencies

        private readonly IBusDataService _busDataService;
        private readonly IBusWebService _busWebService;
        private readonly IUserDialogs _userDilaogsService;
        private readonly IConnectivityService _connectivityService;

        #endregion

        #region Properties

        //TODO add bus line number
        public string BusLineAndStation
        {
            get => BusStation == null ? string.Empty : BusStation.Name;
        }

        private BusStationModel _busStation;
        public BusStationModel BusStation
        {
            get => _busStation;
            set => SetProperty(ref _busStation, value);
        }

        private List<BusTimeTableModel> _busTimeTableWeekdays;
        public List<BusTimeTableModel> BusTimeTableWeekdays
        {
            get => _busTimeTableWeekdays;
            set => SetProperty(ref _busTimeTableWeekdays, value);
        }

        private List<BusTimeTableModel> _busTimeTableSaturday;
        public List<BusTimeTableModel> BusTimeTableSaturday
        {
            get => _busTimeTableSaturday;
            set => SetProperty(ref _busTimeTableSaturday, value);
        }

        private List<BusTimeTableModel> _busTimeTableSunday;
        public List<BusTimeTableModel> BusTimeTableSunday
        {
            get => _busTimeTableSunday;
            set => SetProperty(ref _busTimeTableSunday, value);
        }

        private List<BusTimeTableModel> _busTimeTableHolidayWeekdays;
        public List<BusTimeTableModel> BusTimeTableHolidayWeekdays
        {
            get => _busTimeTableHolidayWeekdays;
            set => SetProperty(ref _busTimeTableHolidayWeekdays, value);
        }

        public override string Title
        {
            get => Device.RuntimePlatform == Device.UWP
                                           ? $"{BusLineAndStation} - Updated on {LastUpdated}"
                                           : BusLineAndStation;
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

        public BusTimeTableViewModel(IBusDataService busDataService,
                                     IBusWebService busWebService,
                                     IUserDialogs userDialogsService,
                                     IConnectivityService connectivityService)
        {
            _busDataService = busDataService;
            _busWebService = busWebService;
            _userDilaogsService = userDialogsService;
            _connectivityService = connectivityService;
        }
        
        #endregion

        #region Command Methods

        private async void DoRefreshCommand()
        {
            await GetWebBusTimeTableAsync(BusStation.SchedualLink);

            IsBusy = false;
        }

        #endregion

        #region Navigation Methods

        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            //TODO use JSON serialization when sending data between pages
            if (parameters[AppNavigation.BusStation] is BusStationModel busStation)
            {
                BusStation = busStation;
            }

            await GetBusTimeTableAsync();
        }

        #endregion

        #region Methods

        private async Task GetBusTimeTableAsync()
        {
            if (BusStation == null)
            {
                return;
            }

            var busTimeTableCount = await _busDataService.CountBusTimeTableAsync((int)BusStation.Id);

            if (busTimeTableCount == 0)
            {
                await GetBusTimeTableWithLoadingScreenAsync(BusStation.SchedualLink);
            }
            else
            {
                await GetLocalBusTimeTableAsync();
            }
        }

        private async Task GetBusTimeTableWithLoadingScreenAsync(string schedualLink)
        {
            using (_userDilaogsService.Loading($"Fetching Data... "))
            {
                await GetWebBusTimeTableAsync(schedualLink);
            }
        }

        private async Task GetWebBusTimeTableAsync(string schedualLink)
        {
            if (!_connectivityService.IsInternetAvailable)
            {
                return;
            }

            var busTimetables = await _busWebService.GetBusTimeTableAsync(schedualLink);

            GetTimeTableByTimeOfWeek(busTimetables);

            LastUpdated = string.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);

            await AddBusStationsToDatabaseAsync(busTimetables);
        }

        private async Task GetLocalBusTimeTableAsync()
        {
            var busTimetables = await _busDataService.GetBusTimeTableByBusStationId((int)BusStation.Id);

            LastUpdated = busTimetables.FirstOrDefault()
                                       .LastUpdateDate;

            GetTimeTableByTimeOfWeek(busTimetables);
        }

        private void GetTimeTableByTimeOfWeek(List<BusTimeTableModel> busTimetable)
        {
            BusTimeTableWeekdays = busTimetable.Where(btt => btt.TimeOfWeek == TimeOfTheWeek.WeekDays
                                                                                            .ToString())
                                               .ToList();
            BusTimeTableSaturday = busTimetable.Where(btt => btt.TimeOfWeek == TimeOfTheWeek.Saturday
                                                                                            .ToString())
                                               .ToList();
            BusTimeTableSunday = busTimetable.Where(btt => btt.TimeOfWeek == TimeOfTheWeek.Sunday
                                                                                          .ToString())
                                             .ToList();
        }

        private async Task AddBusStationsToDatabaseAsync(List<BusTimeTableModel> busTimeTables)
        {
            foreach (var busTimetableHour in busTimeTables)
            {
                // Add foreign key before inserting in database
                busTimetableHour.BusStationId = (int)BusStation.Id;
                busTimetableHour.LastUpdateDate = LastUpdated;
            }

            await _busDataService.InsertOrReplaceBusTimeTablesAsync(busTimeTables);
        }

        #endregion
    }
}