using Prism;
using Prism.Ioc;
using RATBVFormsPrism.Droid.Services;
using RATBVFormsPrism.Services;

namespace RATBVFormsPrism.Droid
{
    public class AndroidPlatformInitializer : IPlatformInitializer
    {
        #region IPlatformInitializer Methods

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<ISQLiteService, SQLite_Android>();
        }

        #endregion
    }
}
