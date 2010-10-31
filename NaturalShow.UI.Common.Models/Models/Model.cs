using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace NaturalShow.UI.Common.Models
{
    public class Model : Animatable, INotifyPropertyChanged
    {
        protected override Freezable CreateInstanceCore()
        {
            return new Model();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(String info)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        #endregion
    }
}
