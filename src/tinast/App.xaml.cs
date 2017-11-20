namespace DP.Tinast
{
    using Microsoft.HockeyApp;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : TinastApp
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            HockeyClient.Current.Configure("97e8a58ba9a74a2bb9a8b8d46a464b7b");
        }
    }
}
