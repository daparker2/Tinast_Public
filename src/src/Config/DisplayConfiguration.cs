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
    class DisplayConfiguration
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
        public PidType BoostPidType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the afr pid.
        /// </summary>
        /// <value>
        /// The type of the afr pid.
        /// </value>
        public PidType AfrPidType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the oil temp pid.
        /// </summary>
        /// <value>
        /// The type of the oil temp pid.
        /// </value>
        public PidType OilTempPidType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the coolant temp pid.
        /// </summary>
        /// <value>
        /// The type of the coolant temp pid.
        /// </value>
        public PidType CoolantTempPidType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the intake temp pid.
        /// </summary>
        /// <value>
        /// The type of the intake temp pid.
        /// </value>
        public PidType IntakeTempPidType
        {
            get;
            set;
        }

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
            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            using (IOutputStream outputStream = fileStream.GetOutputStreamAt(0))
            using (DataWriter dataWriter = new DataWriter(outputStream))
            {
                dataWriter.WriteString(json);
                this.log.Info("Config saved.");
            }
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
                    log.Warn("Default config file not found.");
                    DisplayConfiguration ret = new DisplayConfiguration();

                    // Make sure we can save the default configuration that we just created.
                    await ret.Save();
                    return ret;
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
