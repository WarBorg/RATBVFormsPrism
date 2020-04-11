using Acr.UserDialogs;
using Prism;
using Prism.Ioc;
using Prism.Unity;
using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.Services;
using RATBVFormsPrism.Views;
using Refit;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace RATBVFormsPrism
{
    public partial class App : PrismApplication
    {
        private bool _isWebApiServerLocal = false;

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
            if (_isWebApiServerLocal)
            {
                containerRegistry.Register<IHttpServiceOptions, LocalHttpServiceOptions>();
                containerRegistry.Register<ICustomHttpMessageHandler, InsecureHttpMessageHandler>();

            }
            else
            {
                containerRegistry.Register<IHttpServiceOptions, DefaultHttpServiceOptions>();
                containerRegistry.Register<ICustomHttpMessageHandler, DefaultHttpMessageHandler>();
            }

            containerRegistry.Register<IHttpService, HttpService>();
            containerRegistry.Register<IConnectivity, ConnectivityImplementation>();

            containerRegistry.Register<IConnectivityService, ConnectivityService>();
            containerRegistry.Register<IBusDataService, BusDataService>();
            containerRegistry.Register<IBusWebService, BusWebService>();

            containerRegistry.RegisterInstance<IUserDialogs>(UserDialogs.Instance);
            containerRegistry.RegisterInstance<IBusApi>(
                RestService.For<IBusApi>(
                    ((PrismApplication)Current).Container
                                               .Resolve<IHttpService>()
                                               .HttpClient
                ));

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