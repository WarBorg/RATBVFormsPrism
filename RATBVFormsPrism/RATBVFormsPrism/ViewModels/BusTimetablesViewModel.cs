﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Commands;
using Prism.Navigation;
using RATBVData.Models.Enums;
using RATBVData.Models.Models;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Exceptions;
using RATBVFormsPrism.Services;
using Xamarin.Forms;

namespace RATBVFormsPrism.ViewModels
{
    public class BusTimetablesViewModel : BusViewModelBase
    {
        #region Fields

        private BusStationModel? _busStation;

        #endregion

        #region Properties

        public RangeObservableCollection<BusTimeTableModel> BusTimetableWeekdays
        {
            get;

        } = new RangeObservableCollection<BusTimeTableModel>();

        public RangeObservableCollection<BusTimeTableModel> BusTimetableSaturday
        {
            get;

        } = new RangeObservableCollection<BusTimeTableModel>();

        public RangeObservableCollection<BusTimeTableModel> BusTimetableSunday
        {
            get;

        } = new RangeObservableCollection<BusTimeTableModel>();

        public RangeObservableCollection<BusTimeTableModel> BusTimetableHolidayWeekdays
        {
            get;

        } = new RangeObservableCollection<BusTimeTableModel>();

        // TODO Add bus line number
        public string BusLineAndStation
        {
            get => _busStation == null ? string.Empty : _busStation.Name;
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

        private DelegateCommand? _refreshCommand;
        public ICommand RefreshCommand
        {
            get => _refreshCommand ??= new DelegateCommand(RefreshTimetables, () => !IsBusy);
        }

        #endregion

        #region Constructors and Dependencies

        private readonly IBusRepository _busRepository;
        private readonly IUserDialogs _userDilaogsService;
        private readonly IConnectivityService _connectivityService;

        public BusTimetablesViewModel(IBusRepository busRepository,
                                      IUserDialogs userDialogsService,
                                      IConnectivityService connectivityService)
        {
            _busRepository = busRepository ?? throw new ArgumentException(nameof(busRepository));
            _userDilaogsService = userDialogsService ?? throw new ArgumentException(nameof(userDialogsService));
            _connectivityService = connectivityService ?? throw new ArgumentException(nameof(connectivityService));
        }

        #endregion

        #region Command Methods

        private async void RefreshTimetables()
        {
            if (_connectivityService.IsInternetAvailable)
            {
                using (_userDilaogsService.Loading("Fetching Data... "))
                {
                    await GetBusTimeTableAsync(isForcedRefresh: true);
                }
            }
            else
            {
                _userDilaogsService.Toast("No Internet connection detected");
            }

            IsBusy = false;
        }

        #endregion

        #region Navigation Override Methods

        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            //TODO use JSON serialization when sending data between pages
            if (parameters[AppNavigation.BusStation] is BusStationModel busStation)
            {
                _busStation = busStation;
            }

            using (_userDilaogsService.Loading("Fetching Data... "))
            {
                await GetBusTimeTableAsync(isForcedRefresh: false);
            }
        }

        #endregion

        #region Methods

        private async Task GetBusTimeTableAsync(bool isForcedRefresh)
        {
            if (_busStation == null)
            {
                _userDilaogsService.Toast("No Timetables found for selected bus station");

                return;
            }

            if (_busStation.Id == null)
            {
                _userDilaogsService.Toast($"Timetable database error for {_busStation.Name} bus station");

                return;
            }

            try
            {
                var busTimetables = await _busRepository.GetBusTimetablesAsync(_busStation.ScheduleLink,
                                                                               _busStation.Id.Value,
                                                                               isForcedRefresh);

                LastUpdated = busTimetables.FirstOrDefault()
                                           .LastUpdateDate;

                GetTimeTableByTimeOfWeek(busTimetables);
            }
            catch (NoInternetException)
            {
                _userDilaogsService.Toast("Internet connection is necessary to get bus timetables",
                                          dismissTimer: TimeSpan.FromSeconds(5));
            }
            catch (Exception)
            {
                _userDilaogsService.Toast("Something went wrong when getting bus timetables");
            }
        }

        private void GetTimeTableByTimeOfWeek(List<BusTimeTableModel> busTimetable)
        {
            BusTimetableWeekdays.ReplaceRange(busTimetable.Where(btt => btt.TimeOfWeek == TimeOfTheWeek.WeekDays
                                                                                                       .ToString()));

            BusTimetableSaturday.ReplaceRange(busTimetable.Where(btt => btt.TimeOfWeek == TimeOfTheWeek.Saturday
                                                                                                       .ToString()));

            BusTimetableSunday.ReplaceRange(busTimetable.Where(btt => btt.TimeOfWeek == TimeOfTheWeek.Sunday
                                                                                                     .ToString()));
        }

        #endregion
    }
}