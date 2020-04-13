using Prism;
using Prism.Ioc;
using RATBVFormsPrism.iOS.Services;
using RATBVFormsPrism.Services;

namespace RATBVFormsPrism.iOS
{
    public class IOSPlatformInitializer : IPlatformInitializer
    {
        #region IPlatformInitializer Methods

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<ISQLiteService, SQLite_iOS>();
        }

        #endregion
    }
}
