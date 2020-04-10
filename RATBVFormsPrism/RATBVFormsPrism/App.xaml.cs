using Acr.UserDialogs;
using Prism.Ioc;
using Prism.Unity;
using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.Services;
using RATBVFormsPrism.Views;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace RATBVFormsPrism
{
    public partial class App : PrismApplication
    {
        protected override async void OnInitialized()
        {
            InitializeComponent();

            await NavigationService.NavigateAsync($"{nameof(RATBVNavigation)}/{nameof(BusLines)}");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<RATBVNavigation>();
            containerRegistry.RegisterForNavigation<BusLines>();
            containerRegistry.RegisterForNavigation<BusStations>();
            containerRegistry.RegisterForNavigation<BusTimeTable>();
        }

        protected override void RegisterRequiredTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<IUserDialogs>(UserDialogs.Instance);

            containerRegistry.Register<IConnectivity, ConnectivityImplementation>();

            containerRegistry.Register<IConnectivityService, ConnectivityService>();
            containerRegistry.Register<IBusDataService, BusDataService>();
            containerRegistry.Register<IBusWebService, BusWebService>();

            base.RegisterRequiredTypes(containerRegistry);
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}