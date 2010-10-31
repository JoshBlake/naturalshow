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

namespace NaturalShow.UI.Common.Views
{
    /// <summary>
    /// Interaction logic for ShowTextView.xaml
    /// </summary>
    public partial class ShowTextView : ShowView
    {
        public ShowTextView(ShowTextModel model)
            : base(model)
        {
            InitializeComponent();

        }

    }
}
