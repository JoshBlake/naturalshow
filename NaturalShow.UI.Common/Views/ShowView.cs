using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using NaturalShow.UI.Common.Models;
using Blake.NUI.WPF.Common;
using Blake.NUI.WPF.Gestures;
using System.Windows.Interactivity;
using NaturalShow.UI.Common.Controls;

namespace NaturalShow.UI.Common.Views
{
    public class ShowView : UserControl
    {
        #region IsLocked DP
        public bool IsLocked
        {
            get { return (bool)GetValue(IsLockedProperty); }
            set { SetValue(IsLockedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLocked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register("IsLocked", typeof(bool), typeof(ShowView), new UIPropertyMetadata(true));

        
        #endregion

        #region Properties

        public List<ZoomCanvasItem> ManipulationGroup = new List<ZoomCanvasItem>();

        public Rect Bounds
        {
            get
            {
                return GetBounds();
            }
        }

        public ShowModel Model
        {
            get
            {
                return this.DataContext as ShowModel;
            }
        }

        #endregion

        #region Events

        public event EventHandler Tap;

        private void OnTap()
        {
            if (Tap == null)
                return;

            Tap(this, EventArgs.Empty);
        }

        #endregion

        #region Constructors

        public ShowView()
        {
            InitTapGesture();
        }

        public ShowView(ShowModel model)
        {
            InitBinding(model);
            InitTapGesture();
        }

        #endregion

        #region Methods

        void InitTapGesture()
        {
            TapGestureTrigger tap = new TapGestureTrigger();
            tap.HandlesTouches = false;
            tap.Tap += new EventHandler(tap_Tap);
        }

        void tap_Tap(object sender, EventArgs e)
        {
            OnTap();
        }

        private Rect GetBounds()
        {
            return DisplayMatrix.MatrixToBounds(this.Model.DisplayMatrix, this.RenderSize);
        }
        
        public void ProcessManipulationDelta(ManipulationDelta deltaManipulation, Point manipulationOrigin)
        {
            if (this.Model == null)
            {
                return;
            }

            Point center = this.Model.DisplayMatrix.Center;

            //Get offset of the manipulation
            Vector offsetToCenter = (Vector)(center - manipulationOrigin);

            //Scale the offset
            Vector scaledOffsetToCenter = (Vector)(offsetToCenter * deltaManipulation.Scale.X);
            //Update the translation so manipulation remains centered
            Vector deltaTranslation = deltaManipulation.Translation + scaledOffsetToCenter - offsetToCenter;

            this.Model.DisplayMatrix.Center += deltaTranslation;

            if (this.Model.CanRotate)
            {
                double newOrientation = (this.Model.DisplayMatrix.Orientation + deltaManipulation.Rotation) % 360.0;
                if (newOrientation < 0.0)
                    newOrientation += 360.0;

                this.Model.DisplayMatrix.Orientation = newOrientation;
            }
            
            Vector oldScale = this.Model.DisplayMatrix.Scale;

            this.Model.DisplayMatrix.Scale = new Vector(oldScale.X * deltaManipulation.Scale.X,
                                                        oldScale.Y * deltaManipulation.Scale.Y);


        }
        

        public void InitBinding(ShowModel model)
        {
            this.DataContext = model;
            
            Binding lockedBinding = new Binding("IsLocked");
            lockedBinding.Mode = BindingMode.TwoWay;
            this.SetBinding(ShowView.IsLockedProperty, lockedBinding);

            Binding centerBinding = new Binding("Center");
            centerBinding.Mode = BindingMode.TwoWay;
            centerBinding.Source = model.DisplayMatrix;
            this.SetBinding(ZoomCanvasItem.CenterProperty, centerBinding);

            Binding orientationBinding = new Binding("Orientation");
            orientationBinding.Mode = BindingMode.TwoWay;
            orientationBinding.Source = model.DisplayMatrix;
            this.SetBinding(ZoomCanvasItem.OrientationProperty, orientationBinding);

            Binding scaleBinding = new Binding("Scale");
            scaleBinding.Mode = BindingMode.TwoWay;
            scaleBinding.Source = model.DisplayMatrix;
            this.SetBinding(ZoomCanvasItem.ScaleProperty, scaleBinding);

            Binding zindexBinding = new Binding("ZIndex");
            zindexBinding.Mode = BindingMode.TwoWay;
            zindexBinding.Source = model.DisplayMatrix;
            this.SetBinding(Canvas.ZIndexProperty, zindexBinding);
            
            Binding isDirtyBinding = new Binding("IsDirty");
            isDirtyBinding.Mode = BindingMode.TwoWay;
            isDirtyBinding.Source = model.DisplayMatrix;
            this.SetBinding(ZoomCanvasItem.IsDirtyProperty, isDirtyBinding);

            Binding canRotateBinding = new Binding("CanRotate");
            canRotateBinding.Mode = BindingMode.TwoWay;
            this.SetBinding(ZoomCanvasItem.CanRotateProperty, canRotateBinding);
             
        }
        
        #endregion

    }
}
