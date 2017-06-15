namespace DP.Tinast.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.Foundation;
    using Windows.Storage;
    using Windows.Storage.Streams;
    using MetroLog;
    using Newtonsoft.Json;

    /// <summary>
    /// Represent display configuration.
    /// </summary>
    public class DisplayConfiguration
    {
        /// <summary>
        /// The file name
        /// </summary>
        const string FileName = "displayConfiguration.json";

        /// <summary>
        /// The logger
        /// </summary>
        private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<DisplayConfiguration>();
        
        /// <summary>
        /// Gets or sets the type of the boost pid.
        /// </summary>
        /// <value>
        /// The type of the boost pid.
        /// </value>
        public PidType BoostPidType { get; set; }

        /// <summary>
        /// Gets or sets the type of the afr pid.
        /// </summary>
        /// <value>
        /// The type of the afr pid.
        /// </value>
        public PidType AfrPidType { get; set; }

        /// <summary>
        /// Gets or sets the type of the load pid.
        /// </summary>
        /// <value>
        /// The type of the load pid.
        /// </value>
        public PidType LoadPidType { get; set; }

        /// <summary>
        /// Gets or sets the type of the oil temp pid.
        /// </summary>
        /// <value>
        /// The type of the oil temp pid.
        /// </value>
        public PidType OilTempPidType { get; set; }

        /// <summary>
        /// Gets or sets the type of the coolant temp pid.
        /// </summary>
        /// <value>
        /// The type of the coolant temp pid.
        /// </value>
        public PidType CoolantTempPidType { get; set; }

        /// <summary>
        /// Gets or sets the type of the intake temp pid.
        /// </summary>
        /// <value>
        /// The type of the intake temp pid.
        /// </value>
        public PidType IntakeTempPidType { get; set; }

        /// <summary>
        /// Gets or sets the boost offset for the MAP reading.
        /// </summary>
        /// <value>
        /// The boost offset.
        /// </value>
        public double BoostOffset { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum boost.
        /// </summary>
        /// <value>
        /// The maximum boost.
        /// </value>
        public int MaxBoost { get; set; } = 15;

        /// <summary>
        /// Gets or sets the maximum idle load.
        /// </summary>
        /// <value>
        /// The maximum idle load.
        /// </value>
        public double MaxIdleLoad { get; set; } = 25;

        /// <summary>
        /// Gets or sets the oil temp minimum.
        /// </summary>
        /// <value>
        /// The oil temp minimum.
        /// </value>
        public double OilTempMin { get; set; } = 160;

        /// <summary>
        /// Gets or sets the oil temp maximum.
        /// </summary>
        /// <value>
        /// The oil temp maximum.
        /// </value>
        public double OilTempMax { get; set; } = 240;

        /// <summary>
        /// Gets or sets the coolant temp minimum.
        /// </summary>
        /// <value>
        /// The coolant temp minimum.
        /// </value>
        public double CoolantTempMin { get; set; } = 160;

        /// <summary>
        /// Gets or sets the coolant temp maximum.
        /// </summary>
        /// <value>
        /// The coolant temp maximum.
        /// </value>
        public double CoolantTempMax { get; set; } = 240;

        /// <summary>
        /// Gets or sets the intake temp minimum.
        /// </summary>
        /// <value>
        /// The intake temp minimum.
        /// </value>
        public double IntakeTempMin { get; set; } = 0;

        /// <summary>
        /// Gets or sets the intake temp maximum.
        /// </summary>
        /// <value>
        /// The intake temp maximum.
        /// </value>
        public double IntakeTempMax { get; set; } = 170;

        /// <summary>
        /// Gets or sets a value indicating whether aggressive ELM327 timing can be used.
        /// </summary>
        /// <value>
        ///   <c>true</c> if aggressive ELM327 timing can be used; otherwise, <c>false</c>.
        /// </value>
        public bool AggressiveTiming { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum requests allowed at once.
        /// </summary>
        /// <value>
        /// The maximum pid requests allowed at once.
        /// </value>
        public int MaxPidsAtOnce { get; set; } = 6;

        /// <summary>
        /// Saves this configuration file.
        /// </summary>
        /// <returns></returns>
        public async Task Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            StorageFolder appFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await appFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);

            this.log.Info("Saving config to '{0}'", file.Path);
            this.log.Trace("{0}", json);
            await FileIO.WriteTextAsync(file, json);
            this.log.Info("Config saved.");
        }

        /// <summary>
        /// Loads this configuration file.
        /// </summary>
        /// <returns></returns>
        public static async Task<DisplayConfiguration> Load()
        {
            ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<DisplayConfiguration>();
            try
            {
                StorageFolder appFolder = ApplicationData.Current.LocalFolder;
                return await ReadConfig(log, appFolder);
            }
            catch (FileNotFoundException)
            {
                log.Warn("Local config file not found.");
                try
                {
                    StorageFolder installFolder = Package.Current.InstalledLocation;
                    return await ReadConfig(log, installFolder);
                }
                catch (FileNotFoundException)
                {
#if DEBUG
                    log.Warn("Default config file not found.");
                    DisplayConfiguration ret = new DisplayConfiguration();

                    // Make sure we can save the default configuration that we just created.
                    await ret.Save();
                    return ret;
#else
                    throw;
#endif // DEBUG
                }
            }
        }

        /// <summary>
        /// Reads the configuration.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        private static async Task<DisplayConfiguration> ReadConfig(ILogger log, StorageFolder folder)
        {
            log.Info(@"Reading config from '{0}\{1}'", folder.Path, FileName);
            StorageFile file = await folder.GetFileAsync(FileName);
            if (file == null)
            {
                throw new FileNotFoundException("Not found.", file.Name);
            }

            string json = await FileIO.ReadTextAsync(file);
            DisplayConfiguration ret = JsonConvert.DeserializeObject<DisplayConfiguration>(json);
            log.Info("Config read.");
            return ret;
        }
    }
}
