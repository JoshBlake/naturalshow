using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using NaturalShow.UI.Common.Settings;
using NaturalShow.UI.Properties;

namespace NaturalShow.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISettingsProvider
    {
        #region ISettingsProvider Implementation
        
        public string GetShowResourcesPath()
        {
            return Settings.Default.Path;
        }

        public string GetShowFile()
        {
            return Settings.Default.ShowFile;
        }

        public string GetShowTempFile()
        {
            return Settings.Default.ShowFileTemp;
        }

        #endregion

    }
}