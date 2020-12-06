using System;
using System.Threading.Tasks;
using SpotifyLib.Services;

namespace SpotifyLib
{
    public class Configuration
    {
        //Time sync
        public readonly TimeProvider.Method TimeSynchronizationMethod = TimeProvider.Method.Ntp;
        public readonly int TimeManualCorrection;

        // Cache
        public readonly bool CacheEnabled;
       // public readonly StorageFolder CacheDir;
        public readonly bool DoCacheCleanUp;

        // Stored credentials
        public readonly bool StoreCredentials;
        public readonly Func<string> StoreCredentialsFunction;

        // Fetching
        public readonly bool RetryOnChunkError;

        /// <summary>
        /// General config file
        /// </summary>
        /// <param name="cacheEnabled">TODO</param>
        /// <param name="doCacheCleanUp">TODO</param>
        /// <param name="storeCredentials">Whether or not app should locally store credentials</param>
        /// <param name="retryOnChunkError">TODO</param>
        /// <param name="timeManualCorrection">TODO</param>
        /// <param name="storeCredentialsFunction">A function to save the stored credential. Should return path file.</param>
        public Configuration(bool cacheEnabled,
            bool doCacheCleanUp,
            bool storeCredentials,
            bool retryOnChunkError,
            int timeManualCorrection,
            Func<string> storeCredentialsFunction = null)
        {
            CacheEnabled = cacheEnabled;
          //  CacheDir = ApplicationData.Current.LocalCacheFolder;
            DoCacheCleanUp = doCacheCleanUp;
            this.StoreCredentials = storeCredentials;
            this.StoreCredentialsFunction = storeCredentialsFunction;
            TimeManualCorrection = timeManualCorrection;
        }
    }
}
