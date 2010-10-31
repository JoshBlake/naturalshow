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
using SurfaceShow.Common.Models;
using Petzold.Media2D;

namespace SurfaceShow.Common.Views
{
    /// <summary>
    /// Interaction logic for ShowArrowView.xaml
    /// </summary>
    public partial class ShowArrowView : ShowView
    {
        public ShowArrowView(ShowArrowModel model)
            : base(model)
        {
            InitializeComponent();

            InitArrow(model);

        }

        private void InitArrow(ShowArrowModel model)
        {
            ArrowLine arrow = new ArrowLine();
            
            arrow.X1 = model.StartOffset.X;
            arrow.Y1 = model.StartOffset.Y;

            arrow.X2 = model.EndOffset.X;
            arrow.Y2 = model.EndOffset.Y;

            arrow.IsArrowClosed = true;
            arrow.ArrowEnds = ArrowEnds.End;

            SolidColorBrush brush = new SolidColorBrush(Colors.White);

            arrow.Stroke = brush;
            arrow.StrokeThickness = 15;

            LayoutRoot.Children.Add(arrow);
        }
    }
}
