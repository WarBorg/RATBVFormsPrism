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
using RATBVFormsPrism.Interfaces;
using Xamarin.Forms;

namespace RATBVFormsPrism.ViewModels
{
    public class BusLinesViewModel : BusViewModelBase
    {
        #region Dependencies

        private readonly INavigationService _navigationService;
        private readonly IBusDataService _busDataService;
        private readonly IBusWebService _busWebService;

        #endregion

        #region Properties

        public List<BusLineModel> AllBusLines { get; private set; }
        
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
                _refreshCommand ??= new DelegateCommand(DoRefreshCommand, () => { return !IsBusy; });
                return _refreshCommand;
            }
        }

        #endregion

        #region Constructors

        public BusLinesViewModel(IBusDataService busDataService,
                                 IBusWebService busWebService,
                                 INavigationService navigationService)
        {
            _busDataService = busDataService;
            _busWebService = busWebService;
            _navigationService = navigationService;
        }
        
        #endregion

        #region Command Methods

        private async void DoRefreshCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            _refreshCommand.RaiseCanExecuteChanged();

            await GetWebBusLinesAsync();

            IsBusy = false;
            _refreshCommand.RaiseCanExecuteChanged();

            await AddBusLinesToDatabaseAsync();
        }

        #endregion

        #region Methods

        public async override void OnNavigatedTo(NavigationParameters parameters)
        {
            AllBusLines = new List<BusLineModel>();

            // ERROR Object not set to an instance of an object :|
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
            if (!IsInternetAvailable)
            {
                return;
            }

            AllBusLines = await _busWebService.GetBusLinesAsync();

            GetBusLinesByType();

            LastUpdated = string.Format("{0:d} {1:HH:mm}", DateTime.Now.Date, DateTime.Now);
        }

        private async Task GetLocalBusLinesAsync()
        {
            AllBusLines = await _busDataService.GetBusLinesByNameAsync();

            GetBusLinesByType();

            LastUpdated = AllBusLines.FirstOrDefault()
                                     .LastUpdateDate;
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
            {
                busLine.LastUpdateDate = LastUpdated;
            }

            await _busDataService.InsertOrReplaceBusLinesAsync(AllBusLines);
        }

        #endregion
    }
}