using System.Reflection;
using System;
using BepInEx.Configuration;
using System.Text.RegularExpressions;

namespace WellRoundedBalance.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ConfigFieldAttribute : Attribute
    {
        public string name;
        public string desc;
        public object defaultValue;

        public ConfigFieldAttribute(string name, object defaultValue) => Init(name, string.Empty, defaultValue);
        public ConfigFieldAttribute(string name, string desc, object defaultValue) => Init(name, desc, defaultValue);
        public void Init(string name, string desc, object defaultValue)
        {
            this.name = name;
            this.desc = desc;
            this.defaultValue = defaultValue;
        }
    }

    public class ConfigManager
    {
        internal static bool ConfigChanged = false;
        internal static bool VersionChanged = false;
        public static void HandleConfigAttributes(Type type, string section, ConfigFile config)
        {
            var backupPath = Regex.Replace(config.ConfigFilePath, "\\W", "") + " : " + section;

            foreach (var field in type.GetFields(BindingFlags.Static))
            {
                var configattr = field.GetCustomAttribute<ConfigFieldAttribute>();
                if (configattr == null)
                    continue;

                var val = config.Bind(new ConfigDefinition(section, configattr.name), configattr.defaultValue, new ConfigDescription(configattr.desc));
                var backupVal = Main.WRBBackupConfig.Bind(new ConfigDefinition(backupPath, configattr.name), val.DefaultValue, new ConfigDescription(configattr.desc));
                // Main.WRBLogger.LogDebug(section + " : " + configattr.name + " " + val.DefaultValue + " / " + val.BoxedValue + " ... " + backupVal.DefaultValue + " / " + backupVal.BoxedValue + " >> " + VersionChanged);

                if (!ConfigEqual(backupVal.DefaultValue, backupVal.BoxedValue))
                {
                    // Main.WRBLogger.LogDebug("Config Updated: " + section + " : " + configattr.name + " from " + val.BoxedValue + " to " + val.DefaultValue);
                    if (VersionChanged)
                    {
                        // Main.WRBLogger.LogDebug("Autosyncing...");
                        val.BoxedValue = val.DefaultValue;
                        backupVal.BoxedValue = backupVal.DefaultValue;
                    }
                }

                if (!ConfigEqual(val.DefaultValue, val.BoxedValue))
                    ConfigChanged = true;

                val.SettingChanged += (sender, args) => field.SetValue(null, (args as SettingChangedEventArgs).ChangedSetting.BoxedValue);
                field.SetValue(null, val.BoxedValue);
            }
        }

        private static bool ConfigEqual(object a, object b)
        {
            if (a.Equals(b)) 
                return true;

            return float.TryParse(a.ToString(), out var fa) &&
                float.TryParse(b.ToString(), out var fb) &&
                Mathf.Abs(fa - fb) < 0.0001;
        }
    }
}