using System;
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
using RATBVFormsPrism.Services;
using RATBVFormsPrism.Views;
using Xamarin.Forms;

namespace RATBVFormsPrism.ViewModels
{
    public class BusLinesViewModel : BusViewModelBase
    {
        #region Properties

        public RangeObservableCollection<BusLinesItemViewModel> BusLines
        {
            get;

        } = new RangeObservableCollection<BusLinesItemViewModel>();

        public RangeObservableCollection<BusLinesItemViewModel> MidiBusLines
        {
            get;

        } = new RangeObservableCollection<BusLinesItemViewModel>();

        public RangeObservableCollection<BusLinesItemViewModel> TrolleybusLines
        {
            get;

        } = new RangeObservableCollection<BusLinesItemViewModel>();

        public override string Title
        {
            get => Device.RuntimePlatform == Device.UWP
                                           ? $"Bus Lines - Updated on {LastUpdated}"
                                           : "Bus Lines";
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
            get => _refreshCommand ??= new DelegateCommand(RefreshBusLines, () => !IsBusy);
        }

        #endregion

        #region Constructor and Dependencies

        private readonly IBusDataService _busDataService;
        private readonly IBusRepository _busRepository;
        private readonly IUserDialogs _userDilaogsService;
        private readonly IConnectivityService _connectivityService;
        private readonly INavigationService _navigationService;

        public BusLinesViewModel(IBusDataService busDataService,
                                 IBusRepository busRepository,
                                 IUserDialogs userDialogsService,
                                 IConnectivityService connectivityService,
                                 INavigationService navigationService)
        {
            _busDataService = busDataService ?? throw new ArgumentException(nameof(busDataService));
            _busRepository = busRepository ?? throw new ArgumentException(nameof(busRepository));
            _userDilaogsService = userDialogsService ?? throw new ArgumentException(nameof(userDialogsService));
            _connectivityService = connectivityService ?? throw new ArgumentException(nameof(connectivityService));
            _navigationService = navigationService ?? throw new ArgumentException(nameof(navigationService));
        }

        #endregion

        #region Command Methods

        private async void RefreshBusLines()
        {
            if (_connectivityService.IsInternetAvailable)
            {
                using (_userDilaogsService.Loading("Fetching Data... "))
                {
                    await GetBusLinesAsync(isForcedRefresh: true);
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
            if (parameters.GetNavigationMode() == NavigationMode.Back)
            {
                return;
            }

            // TODO improve this and move the create tables in the Repository
            // ERROR Object not set to an instance of an object :|
            //using (_userDilaogsService.Loading("Fetching Data... "))
            //{
            // Create tables, if they already exist nothing will happen
            await _busDataService.CreateAllTablesAsync();

            await GetBusLinesAsync(isForcedRefresh: false);
            //}
        }

        #endregion

        #region Methods

        private async Task GetBusLinesAsync(bool isForcedRefresh)
        {
            var busLines = await _busRepository.GetBusLinesAsync(isForcedRefresh);

            LastUpdated = busLines.FirstOrDefault()
                                  .LastUpdateDate;

            GetBusLinesByType(busLines);
        }

        private void GetBusLinesByType(List<BusLineModel> busLines)
        {
            BusLines.ReplaceRange(busLines.Where(bl => bl.Type == BusTypes.Bus.ToString())
                                          .Select(busLine => new BusLinesItemViewModel(busLine,
                                                                                       _navigationService)));

            MidiBusLines.ReplaceRange(busLines.Where(bl => bl.Type == BusTypes.Midibus.ToString())
                                              .Select(busLine => new BusLinesItemViewModel(busLine,
                                                                                           _navigationService)));

            TrolleybusLines.ReplaceRange(busLines.Where(bl => bl.Type == BusTypes.Trolleybus.ToString())
                                                 .Select(busLine => new BusLinesItemViewModel(busLine,
                                                                                              _navigationService)));
        }

        #endregion

        #region BusLinesItemViewModel Class

        public class BusLinesItemViewModel : BusViewModelBase
        {
            #region Fields

            private readonly BusLineModel _busLine;

            #endregion

            #region Properties

            public string Name { get; }
            public string Route { get; }

            #endregion

            #region Command Properties

            private DelegateCommand _busLineSelectedCommand;
            public ICommand BusLineSelectedCommand
            {
                get => _busLineSelectedCommand ??= new DelegateCommand(ShowStationsForSelectedBusLine);
            }

            #endregion

            #region Constructor and Dependencies

            private readonly INavigationService _navigationService;

            public BusLinesItemViewModel(BusLineModel busLine,
                                         INavigationService navigationService)
            {
                _busLine = busLine;
                _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

                Name = _busLine?.Name;
                Route = _busLine?.Route;
            }

            #endregion

            #region Command Methods

            private async void ShowStationsForSelectedBusLine()
            {
                var parameters = new NavigationParameters
            {
                { AppNavigation.BusLine, _busLine }
            };

                //await _navigationService.Navigate($"{nameof(RATBVNavigation)}/{nameof(BusLines)}/{nameof(BusStations)}", parameters);
                await _navigationService.NavigateAsync(nameof(BusStations), parameters);
            }

            #endregion
        }

        #endregion
    }
}