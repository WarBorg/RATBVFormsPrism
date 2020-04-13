using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Prism.Commands;
using Prism.Navigation;
using RATBVData.Models.Enums;
using RATBVData.Models.Models;
using RATBVFormsPrism.Interfaces;
using Xamarin.Forms;

namespace RATBVFormsPrism.ViewModels
{
    public class BusLinesViewModel : BusViewModelBase
    {
        #region Dependencies

        private readonly IBusDataService _busDataService;
        private readonly IBusRepository _busRepository;
        private readonly IUserDialogs _userDilaogsService;
        private readonly IConnectivityService _connectivityService;
        private readonly INavigationService _navigationService;

        #endregion

        #region Fields

        private bool _isGettingData = false;

        #endregion

        #region Properties

        private List<BusLineViewModel> _busLines;
        public List<BusLineViewModel> BusLines
        {
            get => _busLines;
            set => SetProperty(ref _busLines, value);
        }

        private List<BusLineViewModel> _midiBusLines;
        public List<BusLineViewModel> MidiBusLines
        {
            get => _midiBusLines;
            set => SetProperty(ref _midiBusLines, value);
        }

        private List<BusLineViewModel> _trolleybusLines;
        public List<BusLineViewModel> TrolleybusLines
        {
            get => _trolleybusLines;
            set => SetProperty(ref _trolleybusLines, value);
        }

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
            get
            {
                _refreshCommand ??= new DelegateCommand(DoRefreshCommand);
                return _refreshCommand;
            }
        }

        #endregion

        #region Constructors

        public BusLinesViewModel(IBusDataService busDataService,
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
        
        #endregion

        #region Command Methods

        private async void DoRefreshCommand()
        {
            if (_connectivityService.IsInternetAvailable)
            {
                await GetBusLinesAsync(isForcedRefresh: true);
            }
            else
            {
                _userDilaogsService.Toast("No Internet connection detected");
            }

            IsBusy = false;
        }

        #endregion

        #region Navigation Methods

        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.Back)
            {
                return;
            }

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
            // Used because IsBusy is in sync the with Refresh Command and it will refresh all three tabs
            // making the call to the server three times
            if (_isGettingData)
            {
                return;
            }

            _isGettingData = true;

            var busLines = await _busRepository.GetBusLinesAsync(isForcedRefresh);

            LastUpdated = busLines.FirstOrDefault()
                                  .LastUpdateDate;

            GetBusLinesByType(busLines);

            _isGettingData = false;
        }

        private void GetBusLinesByType(List<BusLineModel> busLines)
        {
            BusLines = busLines.Where(bl => bl.Type == BusTypes.Bus.ToString())
                               .Select(busLine => new BusLineViewModel(busLine, _navigationService))
                               .ToList();
            MidiBusLines = busLines.Where(bl => bl.Type == BusTypes.Midibus.ToString())
                                   .Select(busLine => new BusLineViewModel(busLine, _navigationService))
                                   .ToList();
            TrolleybusLines = busLines.Where(bl => bl.Type == BusTypes.Trolleybus.ToString())
                                      .Select(busLine => new BusLineViewModel(busLine, _navigationService))
                                      .ToList();
        }

        #endregion
    }
}