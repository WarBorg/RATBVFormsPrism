﻿using Microsoft.Practices.Unity;
using Prism.Unity;
using RATBVFormsPrism.Interfaces;
using RATBVFormsPrism.Services;
using RATBVFormsPrism.Views;

//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace RATBVFormsPrism
{
    public partial class App : PrismApplication
    {
        protected override async void OnInitialized()
        {
            InitializeComponent();

            await NavigationService.NavigateAsync($"{nameof(RATBVNavigation)}/{nameof(BusLines)}");
        }

        protected override void RegisterTypes()
        {
            Container.RegisterTypeForNavigation<RATBVNavigation>();
            Container.RegisterTypeForNavigation<BusLines>();
            Container.RegisterTypeForNavigation<BusStations>();
            //Container.RegisterTypeForNavigation<BusStations, BusStationsViewModel>();
            Container.RegisterTypeForNavigation<BusTimeTable>();
            //Container.RegisterTypeForNavigation<BusTimeTable, BusTimeTableViewModel>();
        }

        protected override void ConfigureContainer()
        {
            Container.RegisterType<IBusDataService, BusDataService>();
            Container.RegisterType<IBusWebService, BusWebService>();

            base.ConfigureContainer();
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
