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
            : base(typeof(MainPage))
        {
            this.InitializeComponent();
        }
    }
}
