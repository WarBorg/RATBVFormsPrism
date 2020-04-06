using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using RATBVData.Models.Enums;
using RATBVData.Models.Models;
using RATBVFormsPrism.Constants;
using RATBVFormsPrism.Interfaces;
using Xamarin.Forms;

namespace RATBVFormsPrism.ViewModels
{
    public class BusLinesViewModel : BusViewModelBase
    {
        #region Members

        private readonly INavigationService _navigationService;
        private readonly IBusDataService _busDataService;
        private readonly IBusWebService _busWebService;

        private List<BusLineViewModel> _busLines;
        private List<BusLineViewModel> _midiBusLines;
        private List<BusLineViewModel> _trolleybusLines;

        private string _lastUpdated = "never";

        private bool _isBusy;

        private DelegateCommand _refreshCommand;

        #endregion Members

        #region Properties

        public List<BusLineModel> AllBusLines { get; private set; }
        
        public List<BusLineViewModel> BusLines
        {
            get { return _busLines; }
            set { SetProperty(ref _busLines, value); }
        }

        public List<BusLineViewModel> MidiBusLines
        {
            get { return _midiBusLines; }
            set { SetProperty(ref _midiBusLines, value); }
        }

        public List<BusLineViewModel> TrolleybusLines
        {
            get { return _trolleybusLines; }
            set { SetProperty(ref _trolleybusLines, value); }
        }

        public override string Title
        {
            get
            {
                if (Device.RuntimePlatform == Device.UWP)
                    return String.Format("Bus Lines - Updated on {0}", LastUpdated);

                return "Bus Lines";
            }
        }

        public string LastUpdated
        {
            get => _lastUpdated;
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

        public BusLinesViewModel(IBusDataService busDataService, IBusWebService busWebService, INavigationService navigationService)
        {
            _busDataService = busDataService;
            _busWebService = busWebService;
            _navigationService = navigationService;
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

            await GetWebBusLinesAsync();

            IsBusy = false;
            _refreshCommand.RaiseCanExecuteChanged();

            await AddBusLinesToDatabaseAsync();
        }

        #endregion Commands

        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            AllBusLines = new List<BusLineModel>();

            //using (UserDialogs.Instance.Loading($"Fetching Data... "))
            //{
                // Create tables, if they already exist nothing will happen
                await _busDataService.CreateAllTablesAsync();

                var busLinesNumber = await _busDataService.CountBusLines;

                if (busLinesNumber == 0)
                {
                    await GetWebBusLinesAsync();

                    await AddBusLinesToDatabaseAsync();
                }
                else
                {
                    await GetLocalBusLinesAsync();
                }
            //}
        }

        private async Task GetWebBusLinesAsync()
        {
            if (!IsInternetAvailable())
                return;

            AllBusLines = await _busWebService.GetBusLinesAsync();

            GetBusLinesByType();

            LastUpdated = string.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);
        }

        private async Task GetLocalBusLinesAsync()
        {
            AllBusLines = await _busDataService.GetBusLinesByNameAsync();

            GetBusLinesByType();

            LastUpdated = AllBusLines.FirstOrDefault().LastUpdateDate;
        }

        private void GetBusLinesByType()
        {
            BusLines = AllBusLines.Where(bl => bl.Type == BusTypes.Bus.ToString())
                                  .Select(busLine => new BusLineViewModel(busLine, _navigationService))
                                  .ToList();
            MidiBusLines = AllBusLines.Where(bl => bl.Type == BusTypes.Midibus.ToString())
                                      .Select(busLine => new BusLineViewModel(busLine, _navigationService))
                                      .ToList();
            TrolleybusLines = AllBusLines.Where(bl => bl.Type == BusTypes.Trolleybus.ToString())
                                         .Select(busLine => new BusLineViewModel(busLine, _navigationService))
                                         .ToList();
        }

        private async Task AddBusLinesToDatabaseAsync()
        {
            foreach (var busLine in AllBusLines)
                busLine.LastUpdateDate = LastUpdated;

            await _busDataService.InsertOrReplaceBusLinesAsync(AllBusLines);
        }

        #endregion Methods

    }
}