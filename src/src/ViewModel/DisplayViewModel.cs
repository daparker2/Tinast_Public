
namespace DP.Tinast.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using Windows.ApplicationModel.Core;
    using Windows.UI.Core;
    using Interfaces;
    using MetroLog;

    /// <summary>
    /// Represent a view model for the display.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    class DisplayViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<DisplayViewModel>();

        /// <summary>
        /// The ELM 227 driver.
        /// </summary>
        private IElm327Driver driver;

        /// <summary>
        /// The property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        public DisplayViewModel(IElm327Driver driver)
        {
            this.driver = driver;
        }

        /// <summary>
        /// Called when a set of properties change on the view model.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>A task object.</returns>
        protected virtual async Task OnPropertyChanged(string[] properties)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (string propertyName in properties)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            });
        }
    }
}
