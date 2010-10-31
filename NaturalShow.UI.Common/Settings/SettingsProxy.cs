using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;

namespace NaturalShow.UI.Common.Settings
{
    public static class SettingsProxy
    {
        public static string GetShowResourcesPath()
        {
            return GetSettingsProvider().GetShowResourcesPath();
        }

        public static string GetAbsoluteShowFile()
        {
            return Path.Combine(GetShowResourcesPath(), GetShowFile());
        }

        public static string GetShowFile()
        {
            return GetSettingsProvider().GetShowFile();
        }

        public static string GetAbsoluteShowTempFile()
        {
            return Path.Combine(GetShowResourcesPath(), GetShowTempFile());
        }

        public static string GetShowTempFile()
        {
            return GetSettingsProvider().GetShowTempFile();
        }

        private static ISettingsProvider GetSettingsProvider()
        {
            ISettingsProvider provider = Application.Current as ISettingsProvider;
            if (provider == null)
            {
                throw new Exception("Application does not implement ISettingsProvider");
            }
            return provider;
        }
    }
}
