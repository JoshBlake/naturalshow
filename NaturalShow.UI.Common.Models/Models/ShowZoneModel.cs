using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace NaturalShow.UI.Common.Models
{
    public class ShowZoneModel : ShowModel
    {
        #region Properties

        public bool IsMap { get; set; }

        #endregion

        public ShowZoneModel()
        {
            IsMap = false;
            DefaultColor = Colors.Black;
            Color = Colors.Black;
            FontSize = 32;
        }

    }
}
