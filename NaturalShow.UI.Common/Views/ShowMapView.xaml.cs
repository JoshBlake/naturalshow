using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NaturalShow.UI.Common.Models;
using NaturalShow.UI.Common.Controls;
using InfoStrat.VE;

namespace NaturalShow.UI.Common.Views
{
    /// <summary>
    /// Interaction logic for ShowZoneView.xaml
    /// </summary>
    public partial class ShowMapView : ShowView
    {
        public ShowMapView(ShowZoneModel model)
            : base(model)
        {
            InitializeComponent();

            this.MinHeight = 150;
            this.MinWidth = 150;

            this.LayoutUpdated += new EventHandler(ShowMapView_LayoutUpdated);
        }

        void ShowMapView_LayoutUpdated(object sender, EventArgs e)
        {
            this.Height = this.ActualWidth;
        }


        private void map_MapLoaded(object sender, EventArgs e)
        {
        }

        private void VEPushPin_Click(object sender, VEPushPinClickedEventArgs e)
        {
            VEPushPin pin = sender as VEPushPin;
            if (pin == null)
                return;
            map.FlyTo(pin.LatLong, -90, 0, 1000, null);
        }
    }
}
