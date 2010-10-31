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
using NaturalShow.UI.Common.Views;
using NaturalShow.UI.Common.Models;
using System.Windows.Threading;
using System.Diagnostics;
using Blake.NUI.WPF.Common;
using Blake.NUI.WPF.Utility;

namespace NaturalShow.UI.Common.Controls
{
    #region Enum and EventArgs

    public enum ZoomCenterMode
    {
        ManipulationCentered,
        ScreenCentered
    }

    public class ObjectMovedEventArgs : EventArgs
    {
        public ShowView AffectedObject { get; private set; }
        public ManipulationDelta DeltaManipulation { get; private set; }

        public ObjectMovedEventArgs(ShowView affectedObject, ManipulationDelta deltaManipulation)
        {
            this.AffectedObject = affectedObject;
            this.DeltaManipulation = deltaManipulation;
        }
    }

    public class ObjectSelectedEventArgs : EventArgs
    {
        public ShowView AffectedObject { get; private set; }

        public ObjectSelectedEventArgs(ShowView affectedObject)
        {
            this.AffectedObject = affectedObject;
        }
    }

    #endregion

    public class ZoomCanvas : ItemsControl
    {
        #region Enum

        enum DoubleTapSequence
        {
            None,
            FirstTouchDown,
            FirstTouchUp,
            SecondTouchDown
        }

        #endregion

        #region Class Members

        DispatcherTimer doubleTapTimer = new DispatcherTimer();
        ZoomCanvasItem firstTapItem = null;
        DoubleTapSequence doubleTapSequence = DoubleTapSequence.None;

        DispatcherTimer updateTimer = new DispatcherTimer();
        bool isZoomUpdateNecessary = true;
        bool isPositionUpdateNecessary = true;

        Dictionary<TouchDevice, ShowView> draggingItems = new Dictionary<TouchDevice, ShowView>();

        FrameworkElement zoomElement;
        string zoomElementName = "PART_ZoomElement";

        Panel layoutRoot;
        string layoutRootName = "PART_LayoutRoot";

        public DisplayMatrix CurrentView
        {
            get;
            private set;
        }

        List<ZoomCanvasItem> dirtyItems = new List<ZoomCanvasItem>();

        #endregion

        #region Properties

        public bool IsRotateEnabled { get; set; }

        public ZoomCenterMode ZoomCenter { get; set; }

        public Matrix ViewMatrix
        {
            get
            {
                return CurrentView.TransformMatrix;
            }
        }

        public Vector ViewScale
        {
            get
            {
                return GetScaleFromMatrix(CurrentView.TransformMatrix);
            }
        }

        public Matrix ViewMatrixInvert
        {
            get
            {
                Matrix ret = CurrentView.TransformMatrix;
                if (!ret.HasInverse)
                {
                    Debug.Write("Matrix has no inverse");
                    return Matrix.Identity;
                }
                ret.Invert();
                return ret;
            }
        }
        public Vector ViewScaleInvert
        {
            get
            {
                return GetScaleFromMatrix(ViewMatrixInvert);
            }
        }

        #endregion

        #region Constructors

        static ZoomCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomCanvas), new FrameworkPropertyMetadata(typeof(ZoomCanvas)));
        }

        public ZoomCanvas()
        {
            this.IsRotateEnabled = false;
            this.ZoomCenter = ZoomCenterMode.ManipulationCentered;
            this.LayoutUpdated += new EventHandler(ZoomCanvas_LayoutUpdated);
            this.RenderTransformOrigin = new Point(0.5, 0.5);

            CurrentView = new DisplayMatrix();
            CurrentView.ViewChanged += CurrentView_ViewChanged;
            CurrentView.Center = new Point(0, 0);

            doubleTapTimer.Interval = TimeSpan.FromMilliseconds(1000);
            doubleTapTimer.Tick += new EventHandler(doubleTapTimer_Tick);

            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromMilliseconds(15);
            updateTimer.Tick += new EventHandler(updateTimer_Tick);
            updateTimer.Start();
        }

        #endregion

        #region Overridden Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.zoomElement = this.GetTemplateChild(zoomElementName) as FrameworkElement;

            this.layoutRoot = this.GetTemplateChild(layoutRootName) as Panel;

            if (this.layoutRoot != null)
            {
                layoutRoot.IsManipulationEnabled = true;
                layoutRoot.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(LayoutRootCanvas_ManipulationDelta);
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ZoomCanvasItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ZoomCanvasItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            this.InvalidateArrange();
            ZoomCanvasItem container = element as ZoomCanvasItem;

            container.RenderTransformOrigin = new Point(0.5, 0.5);

            container.IsManipulationEnabled = true;
            container.TouchDown += child_TouchDown;
            container.TouchUp += container_TouchUp;

            container.ManipulationDelta += child_ManipulationDelta;
            container.ManipulationStarting += child_ManipulationStarting;
            container.ManipulationInertiaStarting += child_ManipulationInertiaStarting;
            container.ManipulationCompleted += container_ManipulationCompleted;
            container.ViewChanged += DisplayData_ViewChanged;

            ShowView view = item as ShowView;
            if (view != null)
            {
                view.Model.DisplayMatrix.ViewChanged += container.DisplayMatrix_ViewChanged;
            }

            isPositionUpdateNecessary = true;
            dirtyItems.Add(container);
        }

        #endregion

        #region Update Timer Methods

        void DisplayData_ViewChanged(object sender, EventArgs e)
        {
            this.isPositionUpdateNecessary = true;

            SetItemDirty(sender);
        }

        void updateTimer_Tick(object sender, EventArgs e)
        {
            if (isZoomUpdateNecessary)
            {
                UpdateZoomMatrix();
                isZoomUpdateNecessary = false;
            }
            if (isPositionUpdateNecessary)
            {
                PositionAllChildren();
                isPositionUpdateNecessary = false;
            }
        }

        void CurrentView_ViewChanged(object sender, EventArgs e)
        {
            isZoomUpdateNecessary = true;
        }

        void ZoomCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            isPositionUpdateNecessary = true;
            PositionAllChildren();
        }

        public void Closing()
        {
            updateTimer.Stop();
            doubleTapTimer.Stop();
        }

        #endregion

        #region Events

        public event EventHandler ViewChanged;
        public event EventHandler<ObjectMovedEventArgs> ObjectMoved;
        public event EventHandler<ObjectSelectedEventArgs> ObjectSelected;
        public event EventHandler<ObjectSelectedEventArgs> ObjectDoubleTap;

        private void OnViewChanged()
        {
            if (ViewChanged != null)
            {
                ViewChanged(this, EventArgs.Empty);
            }
        }

        private void OnObjectMoved(ShowView affectedObject, ManipulationDelta deltaManipulation)
        {
            if (ObjectMoved != null)
            {
                ObjectMoved(this, new ObjectMovedEventArgs(affectedObject, deltaManipulation));
            }
        }

        private void OnObjectSelected(ShowView affectedObject)
        {
            if (ObjectSelected != null)
            {
                ObjectSelected(this, new ObjectSelectedEventArgs(affectedObject));
            }
        }
        private void OnObjectDoubleTap(ShowView affectedObject)
        {
            if (ObjectDoubleTap != null)
            {
                ObjectDoubleTap(this, new ObjectSelectedEventArgs(affectedObject));
            }
        }

        #endregion

        #region Matrix Helpers

        public Point WorldToScreen(Point p)
        {
            return ViewMatrix.Transform(p);
        }

        public Vector WorldToScreen(Vector v)
        {
            return ViewMatrix.Transform(v);
        }

        public Point ScreenToWorld(Point p)
        {
            return ViewMatrixInvert.Transform(p);
        }

        public Vector ScreenToWorld(Vector v)
        {
            return ViewMatrixInvert.Transform(v);
        }

        public static Vector GetScaleFromMatrix(Matrix matrix)
        {
            Vector ret = new Vector();

            ret.X = new Vector(matrix.M11, matrix.M12).Length;
            ret.Y = new Vector(matrix.M21, matrix.M22).Length;

            return ret;
        }

        #endregion

        #region Layout Manipulation Events

        void LayoutRootCanvas_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (zoomElement == null || layoutRoot == null)
            {
                return;
            }

            if (this.doubleTapSequence == DoubleTapSequence.SecondTouchDown)
            {
                return;
            }

            if (this.CurrentView.HasAnimatedProperties)
            {
                AnimateUtility.StopAnimation(this.CurrentView, DisplayMatrix.CenterProperty);
                AnimateUtility.StopAnimation(this.CurrentView, DisplayMatrix.OrientationProperty);
                AnimateUtility.StopAnimation(this.CurrentView, DisplayMatrix.ScaleProperty);
            }

            //Get the deltas into World frame-of-reference
            Vector deltaTranslation = e.DeltaManipulation.Translation;
            double deltaRotation = e.DeltaManipulation.Rotation;
            Vector deltaScale = e.DeltaManipulation.Scale;
            Point manipulationOrigin = e.ManipulationOrigin;

            UpdateZoomView(deltaTranslation, deltaRotation, deltaScale, manipulationOrigin);

            //We took care of the event
            e.Handled = true;

            foreach (object item in this.Items)
            {
                ZoomCanvasItem child = GetZoomCanvasItem(item);

                if (child != null && !GetIsLocked(child) && child.AreAnyTouchesCaptured)
                {
                    ShowView view = child.View;

                    Vector scale = deltaScale;
                    if (scale.X != 0)
                        scale.X = 1 / scale.X;
                    if (scale.Y != 0)
                        scale.Y = 1 / scale.Y;

                    ManipulationDelta deltaManipulation = new ManipulationDelta(-1 * deltaTranslation,
                                                                                -1 * deltaRotation,
                                                                                scale,
                                                                                new Vector());
                    UpdatePosition(view, deltaManipulation, manipulationOrigin, 1);

                    PositionChild(child);
                    if (dirtyItems.Contains(child))
                    {
                        dirtyItems.Remove(child);
                    }

                }
            }

            OnViewChanged();

        }

        private void UpdateZoomView(Vector deltaTranslation, double deltaRotation, Vector deltaScale, Point manipulationOrigin)
        {
            if (!IsRotateEnabled)
            {
                deltaRotation = 0;
            }

            Point center = this.CurrentView.Center;

            if (double.IsNaN(center.X))
            {
                center.X = 0;
                this.CurrentView.Center = center;
            }
            if (double.IsNaN(center.Y))
            {
                center.Y = 0;
                this.CurrentView.Center = center;
            }

            //Get offset of the manipulation
            Vector offsetToCenter = (Vector)(center - manipulationOrigin);

            //Scale the offset
            Vector scaledOffsetToCenter = (Vector)(offsetToCenter * deltaScale.X);
            //Update the translation so manipulation remains centered
            deltaTranslation += scaledOffsetToCenter - offsetToCenter;


            CurrentView.Center += deltaTranslation;
            CurrentView.Orientation += deltaRotation;

            Vector oldScale = CurrentView.Scale;
            CurrentView.Scale = new Vector(oldScale.X * deltaScale.X,
                                           oldScale.Y * deltaScale.Y);
            UpdateZoomMatrix();

        }

        private void UpdateZoomMatrix()
        {
            if (zoomElement == null)
                return;
            isZoomUpdateNecessary = false;

            //UpdateScatterMatrix(zoomElement, zoomElement.RenderSize, center, CurrentView.Orientation, new Vector(1, 1));
            //UpdateScatterMatrix(zoomElement, zoomElement.RenderSize, new Point(0,0), 0, CurrentView.Scale);
            UpdateScatterMatrix(zoomElement, zoomElement.RenderSize, CurrentView.Center, CurrentView.Orientation, CurrentView.Scale);

        }

        private void OldUpdateZoomView(ManipulationDeltaEventArgs e)
        {
            //Get the deltas into World frame-of-reference
            Vector deltaTranslation = e.DeltaManipulation.Translation;
            double deltaRotation = e.DeltaManipulation.Rotation;
            Vector deltaScale = e.DeltaManipulation.Scale;
            Point manipulationOrigin = e.ManipulationOrigin;

            //Initialize a blank Matrix
            Matrix elementMatrix = new Matrix();
            //Try to get the existing MatrixTransform
            //The first time, this will return null and we will use the blank Matrix above
            MatrixTransform transformMatrix = zoomElement.RenderTransform as MatrixTransform;

            //If it exists,
            if (transformMatrix != null)
            {
                //Then retrieve the matrix from it
                elementMatrix = transformMatrix.Matrix;
            }

            //Apply the delta translation
            elementMatrix.Translate(deltaTranslation.X, deltaTranslation.Y);

            Point transformCenter;

            if (ZoomCenter == ZoomCenterMode.ScreenCentered)
            {
                transformCenter = new Point(layoutRoot.ActualWidth / 2,
                                            layoutRoot.ActualHeight / 2);
            }
            else
            {
                //Transform ManipulationOrigin by the current matrix
                transformCenter = elementMatrix.Transform(manipulationOrigin);
            }

            if (IsRotateEnabled)
            {
                //Apply the delta rotation
                elementMatrix.RotateAt(deltaRotation, transformCenter.X, transformCenter.Y);
            }

            //Apply the delta scale
            elementMatrix.ScaleAt(deltaScale.X, deltaScale.Y, transformCenter.X, transformCenter.Y);

            //Update with the transformed matrix
            zoomElement.RenderTransform = new MatrixTransform(elementMatrix);
            /*
            _viewMatrix = elementMatrix;
            _viewMatrixInvert = elementMatrix;
            _viewMatrixInvert.Invert();
             */
        }

        #endregion

        #region Children Touch Events

        void child_TouchDown(object sender, TouchEventArgs e)
        {
            ZoomCanvasItem selectedChild = GetZoomCanvasItem(sender);

            if (selectedChild == null)
                return;

            ShowView view = selectedChild.View;

            if (view == null)
                return;

            if (firstTapItem == null)
            {
                firstTapItem = selectedChild;
                doubleTapTimer.Start();
                doubleTapSequence = DoubleTapSequence.FirstTouchDown;
            }
            else if (firstTapItem == selectedChild &&
                     doubleTapSequence == DoubleTapSequence.FirstTouchUp)
            {
                OnObjectDoubleTap(selectedChild.View);

                doubleTapSequence = DoubleTapSequence.SecondTouchDown;
            }

            OnObjectSelected(view);

            //TODO: double tap gesture, boundstomatrix, zoom in currentview

        }

        void container_TouchUp(object sender, TouchEventArgs e)
        {
            ZoomCanvasItem selectedChild = GetZoomCanvasItem(sender);

            if (selectedChild == null)
                return;

            ShowView view = selectedChild.View;

            if (view == null)
                return;

            if (firstTapItem == selectedChild &&
                doubleTapSequence == DoubleTapSequence.FirstTouchDown)
            {
                doubleTapSequence = DoubleTapSequence.FirstTouchUp;
            }
            else
            {
                doubleTapSequence = DoubleTapSequence.None;
            }

        }

        void doubleTapTimer_Tick(object sender, EventArgs e)
        {
            firstTapItem = null;
            doubleTapTimer.Stop();

            doubleTapSequence = DoubleTapSequence.None;
        }

        public ZoomCanvasItem GetZoomCanvasItem(object o)
        {
            ZoomCanvasItem child = o as ZoomCanvasItem;
            if (child == null)
                child = ItemContainerGenerator.ContainerFromItem(o) as ZoomCanvasItem;
            return child;
        }

        private bool GetIsLocked(ZoomCanvasItem item)
        {
            if (item == null)
                return false;

            ShowView view = item.View;

            if (view == null)
                return false;

            return view.IsLocked;
        }

        #endregion

        #region Children Manipulation Events

        void container_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            ZoomCanvasItem child = GetZoomCanvasItem(sender);

            if (child == null)
            {
                return;
            }

            ShowView zoneView = child.View;

            if (zoneView == null)
            {
                return;
            }

            zoneView.ManipulationGroup.Clear();
        }

        void child_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            //Smooth deceleration at .001 DIP/millisecond
            //e.TranslationBehavior.DesiredDeceleration = .001;

            //Smooth deceleration at .001 degrees/millisecond
            //e.RotationBehavior.DesiredDeceleration = .001;

            e.Handled = true;
        }

        void child_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            //Manipulations should be relative to the parent element
            e.ManipulationContainer = layoutRoot;

            SelectMoveGroup(sender);

            e.Handled = true;
        }

        void SelectMoveGroup(object sender)
        {
            ZoomCanvasItem child = GetZoomCanvasItem(sender);

            if (child == null)
            {
                return;
            }

            ShowZoneView zoneView = child.View as ShowZoneView;

            if (zoneView == null)
            {
                return;
            }

            zoneView.ManipulationGroup.Clear();

            Rect bounds = zoneView.Bounds;

            //Special behavior: three fingers will move just the zone
            if (child.TouchesCaptured.Count() >= 3)
                return;

            foreach (ShowView view in this.Items)
            {
                if (view.Equals(zoneView))
                    continue;

                Rect itemBounds = view.Bounds;
                if (bounds.Contains(itemBounds))
                {
                    zoneView.ManipulationGroup.Add(GetZoomCanvasItem(view));
                }
            }

        }

        void child_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ZoomCanvasItem child = GetZoomCanvasItem(sender);

            if (child == null)
            {
                return;
            }

            ShowView view = child.View;

            if (view == null || GetIsLocked(child))
            {
                //Since the element is locked, forward the touches to the layoutroot for moving whole background
                child.TouchesCaptured.ToList().ForEach(t => layoutRoot.CaptureTouch(t));

                e.Complete();
                return;
            }

            UpdatePosition(view, e.DeltaManipulation, e.ManipulationOrigin, child.TouchesCaptured.Count());

            PositionChild(child);
            if (dirtyItems.Contains(child))
            {
                dirtyItems.Remove(child);
            }

            foreach (ZoomCanvasItem item in view.ManipulationGroup)
            {
                PositionChild(item);
                if (dirtyItems.Contains(item))
                {
                    dirtyItems.Remove(item);
                }
            }

            //We took care of the event
            e.Handled = true;

        }

        private void UpdatePosition(ShowView view, ManipulationDelta delta, Point manipulationOrigin, int numTouches)
        {

            //Get the deltas into World frame-of-reference
            Vector deltaTranslation = ScreenToWorld(delta.Translation);
            double deltaRotation = delta.Rotation;
            Vector deltaScale = delta.Scale;
            manipulationOrigin = ScreenToWorld(manipulationOrigin);

            //Only allow rotation with 3 or more touches. (Only Images have rotation enabled by default.)
            if (numTouches < 3)
            {
                deltaRotation = 0;
            }

            ManipulationDelta deltaManipulation = new ManipulationDelta(deltaTranslation, deltaRotation, deltaScale, delta.Expansion);

            view.ProcessManipulationDelta(deltaManipulation, manipulationOrigin);
            
            //Special case, use 3 touches to move a zone without moving the containing group
            if (numTouches < 3)
            {
                foreach (ZoomCanvasItem item in view.ManipulationGroup)
                {
                    item.View.ProcessManipulationDelta(deltaManipulation, manipulationOrigin);
                }
            }

            OnObjectMoved(view, deltaManipulation);
        }

        #endregion

        #region Scatter Methods

        private void SetItemDirty(object sender)
        {
            ZoomCanvasItem item = sender as ZoomCanvasItem;
            if (item == null)
                GetZoomCanvasItem(sender);

            if (item != null &&
                !this.dirtyItems.Contains(item))
            {
                this.dirtyItems.Add(item);
            }
        }

        private void PositionAllChildren()
        {
            isPositionUpdateNecessary = false;
            if (this.dirtyItems.Count != 0)
            {
                foreach (ZoomCanvasItem item in this.dirtyItems)
                {
                    PositionChild(item);
                }
                this.dirtyItems.Clear();
            }
        }

        private void PositionChild(ZoomCanvasItem item)
        {
            if (item == null)
                return;

            if (!item.IsDirty)
                return;

            item.IsDirty = false;

            Canvas.SetZIndex(item, item.ZIndex);

            double orientation = item.Orientation;
            Vector scale = item.Scale;
            Point center = item.Center;

            double width = item.ActualWidth;
            double height = item.ActualHeight;
            if (double.IsNaN(center.X))
            {
                center.X = item.Center.X;
                if (double.IsNaN(center.X))
                {
                    center.X = this.ActualWidth / 2;
                }
            }
            if (double.IsNaN(center.Y))
            {
                center.Y = item.Center.Y;
                if (double.IsNaN(center.Y))
                {
                    center.Y = this.ActualHeight * 0.75;
                }
            }

            UpdateScatterMatrix(item, item.RenderSize, center, orientation, scale);
        }

        private static void UpdateScatterMatrix(FrameworkElement element, Size size, Point center, double orientation, Vector scale)
        {

            Vector offset = CalculateRenderOffset(size, element.RenderTransformOrigin, center, orientation, scale);

            TransformGroup txg = element.RenderTransform as TransformGroup;
            if ((txg != null) && (txg.Children.Count == 3))
            {
                RotateTransform rtx = txg.Children[0] as RotateTransform;
                if (rtx != null)
                {
                    rtx.Angle = orientation;
                }
                else
                {
                    txg.Children[0] = new RotateTransform(orientation);
                }
                ScaleTransform stx = txg.Children[1] as ScaleTransform;
                if (stx != null)
                {
                    stx.ScaleX = scale.X;
                    stx.ScaleY = scale.Y;
                }
                else
                {
                    txg.Children[1] = new ScaleTransform(scale.X, scale.Y);
                }

                TranslateTransform ttx = txg.Children[2] as TranslateTransform;
                if (ttx != null)
                {
                    ttx.X = offset.X;
                    ttx.Y = offset.Y;
                }
                else
                {
                    txg.Children[2] = new TranslateTransform(offset.X, offset.Y);
                }

            }
            else
            {
                txg = new TransformGroup();
                txg.Children.Add(new RotateTransform(orientation));
                txg.Children.Add(new ScaleTransform(scale.X, scale.Y));
                txg.Children.Add(new TranslateTransform(offset.X, offset.Y));
                element.RenderTransform = txg;

            }
        }

        internal static Vector CalculateRenderOffset(Size size, Point renderTransformOrigin, Point center, double angle, Vector scale)
        {
            double width = size.Width;
            double height = size.Height;
            Point renderedCenter = GetRenderMatrix(GetRenderOrigin(width, height, renderTransformOrigin), new Vector(0.0, 0.0), angle, scale).Transform(new Point(width * 0.5, height * 0.5));
            return (Vector)(center - renderedCenter);
        }

        internal static Point GetRenderOrigin(double width, double height, Point renderTransformOrigin)
        {
            return new Point(width * renderTransformOrigin.X, height * renderTransformOrigin.Y);
        }

        private static Matrix GetRenderMatrix(Point renderOrigin, Vector offset, double rotation, Vector scale)
        {
            Matrix mx = Matrix.Identity;
            mx.RotateAt(rotation, renderOrigin.X, renderOrigin.Y);
            mx.ScaleAt(scale.X, scale.Y, renderOrigin.X, renderOrigin.Y);
            mx.Translate(offset.X, offset.Y);
            return mx;
        }

        #endregion
    }
}
