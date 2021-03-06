﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Azure.Functions.Cli.Interfaces;

namespace Azure.Functions.Cli.Common
{
    internal class PersistentSettings : ISettings
    {
        private static readonly string PersistentSettingsPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".azurefunctions", "config");

        private readonly DiskBacked<Dictionary<string, object>> store;

        public PersistentSettings() : this(true)
        { }

        public PersistentSettings(bool global)
        {
            if (global)
            {
                FileSystemHelpers.EnsureDirectory(Path.GetDirectoryName(PersistentSettingsPath));
                store = DiskBacked.Create<Dictionary<string, object>>(PersistentSettingsPath);
            }
            else
            {
                store = DiskBacked.Create<Dictionary<string, object>>(Path.Combine(Environment.CurrentDirectory, ".config"));
            }
        }

        private T GetConfig<T>(T @default = default(T), [CallerMemberName] string key = null)
        {
            if (store.Value.ContainsKey(key))
            {
                return (T)store.Value[key];
            }
            else
            {
                return @default;
            }
        }

        private void SetConfig(object value, [CallerMemberName] string key = null)
        {
            store.Value[key] = value;
            store.Commit();
        }

        public Dictionary<string, object> GetSettings()
        {
            return typeof(ISettings)
                .GetProperties()
                .ToDictionary(p => p.Name, p => p.GetValue(this));
        }

        public void SetSetting(string name, string value)
        {
            store.Value[name] = JsonConvert.DeserializeObject<JToken>(value);
            store.Commit();
        }

        public bool DisplayLaunchingRunServerWarning { get { return GetConfig(true); } set { SetConfig(value); } }

        public bool RunFirstTimeCliExperience { get { return GetConfig(true); } set { SetConfig(value); } }

        public string CurrentSubscription { get { return GetConfig(string.Empty); } set { SetConfig(value); } }

        public string CurrentTenant { get { return GetConfig(string.Empty); } set { SetConfig(value); } }

        public string MachineId { get { return GetConfig(string.Empty);  } set { SetConfig(value); } }

        public string IsDockerContainer { get { return GetConfig(string.Empty); } set { SetConfig(value); } }
    }
}
