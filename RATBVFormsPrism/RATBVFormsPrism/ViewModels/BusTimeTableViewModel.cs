using Acr.UserDialogs;
using Prism.Commands;
using Prism.Navigation;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RATBVFormsPrism.ViewModels
{
    public class BusTimeTableViewModel : BusViewModelBase
    {
        #region Members

        private readonly IBusDataService _busDataService;
        private readonly IBusWebService _busWebService;

        private BusStationModel _busStation;

        private List<BusTimeTableModel> _busTimeTableWeekdays;
        private List<BusTimeTableModel> _busTimeTableSaturday;
        private List<BusTimeTableModel> _busTimeTableSunday;
        private List<BusTimeTableModel> _busTimeTableHolidayWeekdays;

        private string _lastUpdated = "never";

        private bool _isBusy;

        private DelegateCommand _refreshCommand;

        #endregion Members

        #region Properties

        //TODO add bus line number
        public string BusLineAndStation
        {
            get { return BusStation == null ? String.Empty : BusStation.Name; }
        }

        public BusStationModel BusStation
        {
            get { return _busStation; }
            set { SetProperty(ref _busStation, value); }
        }

        public List<BusTimeTableModel> BusTimeTableWeekdays
        {
            get { return _busTimeTableWeekdays; }
            set { SetProperty(ref _busTimeTableWeekdays, value); }
        }

        public List<BusTimeTableModel> BusTimeTableSaturday
        {
            get { return _busTimeTableSaturday; }
            set { SetProperty(ref _busTimeTableSaturday, value); }
        }

        public List<BusTimeTableModel> BusTimeTableSunday
        {
            get { return _busTimeTableSunday; }
            set { SetProperty(ref _busTimeTableSunday, value); }
        }

        public List<BusTimeTableModel> BusTimeTableHolidayWeekdays
        {
            get { return _busTimeTableHolidayWeekdays; }
            set { SetProperty(ref _busTimeTableHolidayWeekdays, value); }
        }

        public override string Title
        {
            get
            {
                if (Device.RuntimePlatform == Device.WinPhone)
                    return String.Format("{0} - Updated on {1}", BusLineAndStation, LastUpdated);

                return BusLineAndStation;
            }
        }

        public string LastUpdated
        {
            get { return _lastUpdated; }
            set
            {
                SetProperty(ref _lastUpdated, value);
                RaisePropertyChanged(nameof(Title));
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        #region Commands

        public DelegateCommand RefreshCommand
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

        public BusTimeTableViewModel(IBusDataService busDataService, IBusWebService busWebService)
        {
            _busDataService = busDataService;
            _busWebService = busWebService;
        }
        
        #endregion Constructors

        #region Methods

        #region Commands

        private async void DoRefreshCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            _refreshCommand.RaiseCanExecuteChanged();

            await GetWebBusTimeTableAsync(BusStation.SchedualLink);

            IsBusy = false;
            _refreshCommand.RaiseCanExecuteChanged();
        }

        #endregion Commands

        #region Navigation

        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            BusStation = parameters[AppNavigation.BusStation] as BusStationModel;

            await GetBusTimeTableAsync();
        }

        #endregion Navigation

        private async Task GetBusTimeTableAsync()
        {
            if (BusStation == null)
                return;

            var busTimeTableCount = await _busDataService.CountBusTimeTableAsync((int)BusStation.Id);

            if (busTimeTableCount == 0)
                await GetWebBusTimeTableAsync(BusStation.SchedualLink);
            else
                await GetLocalBusTimeTableAsync();
        }

        private async Task GetWebBusTimeTableAsync(string schedualLink)
        {
            using (UserDialogs.Instance.Loading($"Fetching Data... "))
            {
                List<BusTimeTableModel> busTimetable = await _busWebService.GetBusTimeTableAsync(schedualLink);

                GetTimeTableByTimeOfWeek(busTimetable);

                LastUpdated = String.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);

                await AddBusStationsToDatabaseAsync(busTimetable);
            }
        }

        private async Task GetLocalBusTimeTableAsync()
        {
            List<BusTimeTableModel> busTimetable = await _busDataService.GetBusTimeTableByBusStationId((int)BusStation.Id);

            LastUpdated = busTimetable.FirstOrDefault().LastUpdateDate;

            GetTimeTableByTimeOfWeek(busTimetable);
        }

        private void GetTimeTableByTimeOfWeek(List<BusTimeTableModel> busTimetable)
        {
            BusTimeTableWeekdays = busTimetable.Where(btt => btt.TimeOfWeek == TimeOfTheWeek.WeekDays).ToList();
            BusTimeTableSaturday = busTimetable.Where(btt => btt.TimeOfWeek == TimeOfTheWeek.Saturday).ToList();
            BusTimeTableSunday = busTimetable.Where(btt => btt.TimeOfWeek == TimeOfTheWeek.Sunday).ToList();
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

        #endregion Methods
    }
}
