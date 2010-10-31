using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NaturalShow.UI.Common.Settings
{
    public interface ISettingsProvider
    {
        string GetShowResourcesPath();
        string GetShowFile();
        string GetShowTempFile();
    }
}
