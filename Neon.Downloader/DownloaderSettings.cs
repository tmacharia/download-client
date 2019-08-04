using System;
using System.Collections.Generic;
using System.IO;
using Common;

namespace Neon.Downloader
{
    public class DownloaderSettings
    {
        private static IDictionary<string, object> _settings;

        public DownloaderSettings()
        {
            _settings = new Dictionary<string, object>();
            _settings = LoadSettings();
        }

        public int MaxThreads
        {
            get { return Get<int>(nameof(MaxThreads)); }
            set { Save(nameof(MaxThreads), value); }
        }
        public int MaxParallelDownloads
        {
            get { return Get<int>(nameof(MaxParallelDownloads)); }
            set { Save(nameof(MaxParallelDownloads), value); }
        }
        public string DefaultDownloadFolder
        {
            get { return Get<string>(nameof(DefaultDownloadFolder)); }
            set { Save(nameof(DefaultDownloadFolder), value); }
        }

        public IDictionary<string,object> Settings
        {
            get
            {
                if (_settings == null || _settings.Count < 1)
                    _settings = LoadSettings();
                return _settings;
            }
        }

        private TResult Get<TResult>(string key)
        {
            object obj = Settings[key];
            obj = Convert.ChangeType(obj, typeof(TResult));
            return (TResult)obj;
        }
        public void Save(string key, object value)
        {
            if (Settings.ContainsKey(key))
            {
                _settings[key] = value;
            }
            else
            {
                _settings.Add(key, value);
            }
            SaveSettings();
            LoadSettings();
        }

        private IDictionary<string, object> LoadSettings()
        {
            var pairs = ReadSettings();
            if (pairs == null || pairs.Count < 1)
            {
                _settings.Add(nameof(MaxThreads), Globals.MaxThreads);
                _settings.Add(nameof(MaxParallelDownloads), Globals.MaxParallelDownloads);
                _settings.Add(nameof(DefaultDownloadFolder), Globals.DownloadFolder);

                SaveSettings();
            }
            else
                return pairs;
            return ReadSettings();
        }
        private IDictionary<string,object> ReadSettings()
        {
            if (!Directory.Exists(Globals.AppFolder))
                Directory.CreateDirectory(Globals.AppFolder);

            using (var fs = File.Open(Globals.SettingsFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    return sr.ReadToEnd().DeserializeTo<Dictionary<string, object>>();
                }
            }
        }
        private void SaveSettings()
        {
            File.WriteAllText(Globals.SettingsFile, _settings.ToJson());
        }
    }
}