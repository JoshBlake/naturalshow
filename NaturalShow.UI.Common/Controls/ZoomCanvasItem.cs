using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using NaturalShow.UI.Common.Views;

namespace NaturalShow.UI.Common.Controls
{
    public class ZoomCanvasItem : ContentControl
    {
        
        #region Center DP

        public static Point GetCenter(DependencyObject obj)
        {
            return (Point)obj.GetValue(CenterProperty);
        }

        public static void SetCenter(DependencyObject obj, Point value)
        {
            obj.SetValue(CenterProperty, value);
        }

        // Using a DependencyProperty as the backing store for Center.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.RegisterAttached("Center", typeof(Point), typeof(ZoomCanvasItem), 
                                                new FrameworkPropertyMetadata(new Point(double.NaN, double.NaN)));

        public static Point GetItemCenter(object item)
        {
            if (item == null)
                return new Point(double.NaN, double.NaN);

            DependencyObject depObj = item as DependencyObject;

            if (depObj == null)
                return new Point(double.NaN, double.NaN);

            return ZoomCanvasItem.GetCenter(depObj);
        }

        public Point Center
        {
            get
            {
                if (this.DataContext == null)
                    return ZoomCanvasItem.GetCenter(this);
                return GetItemCenter(this.DataContext);
            }
            set
            {
                if (this.DataContext == null)
                {
                    ZoomCanvasItem.SetCenter(this, value);
                } 
                else
                {
                    DependencyObject depObj = this.DataContext as DependencyObject;
                    if (depObj != null)
                        ZoomCanvasItem.SetCenter(depObj, value);
                }
            }
        }
        #endregion

        #region Orientation DP

        public static double GetOrientation(DependencyObject obj)
        {
            return (double)obj.GetValue(OrientationProperty);
        }

        public static void SetOrientation(DependencyObject obj, double value)
        {
            obj.SetValue(OrientationProperty, value);
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.RegisterAttached("Orientation", typeof(double), typeof(ZoomCanvasItem), 
                                                new FrameworkPropertyMetadata(0.0));

        public static double GetItemOrientation(object item)
        {
            if (item == null)
                return 0.0;

            DependencyObject depObj = item as DependencyObject;

            if (depObj == null)
                return 0.0;

            return ZoomCanvasItem.GetOrientation(depObj);
        }

        public double Orientation
        {
            get
            {
                if (this.DataContext == null)
                    return ZoomCanvasItem.GetOrientation(this);
                return GetItemOrientation(this.DataContext);
            }
            set
            {
                if (this.DataContext == null)
                {
                    ZoomCanvasItem.SetOrientation(this, value);
                }
                else
                {
                    DependencyObject depObj = this.DataContext as DependencyObject;
                    if (depObj != null)
                        ZoomCanvasItem.SetOrientation(depObj, value);
                }
            }
        }
        #endregion

        #region Scale DP

        public static Vector GetScale(DependencyObject obj)
        {
            return (Vector)obj.GetValue(ScaleProperty);
        }

        public static void SetScale(DependencyObject obj, Vector value)
        {
            obj.SetValue(ScaleProperty, value);
        }

        // Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.RegisterAttached("Scale", typeof(Vector), typeof(ZoomCanvasItem),
                                                new FrameworkPropertyMetadata(new Vector(0.0, 0.0)));

        public static Vector GetItemScale(object item)
        {
            if (item == null)
                return new Vector(1.0, 1.0);

            DependencyObject depObj = item as DependencyObject;

            if (depObj == null)
                return new Vector(1.0, 1.0);

            return ZoomCanvasItem.GetScale(depObj);
        }

        public Vector Scale
        {
            get
            {
                if (this.DataContext == null)
                    return ZoomCanvasItem.GetScale(this);
                return GetItemScale(this.DataContext);
            }
            set
            {
                if (this.DataContext == null)
                {
                    ZoomCanvasItem.SetScale(this, value);
                }
                else
                {
                    DependencyObject depObj = this.DataContext as DependencyObject;
                    if (depObj != null)
                        ZoomCanvasItem.SetScale(depObj, value);
                }
            }
        }

        #endregion

        #region ZIndex DP

        public static int GetZIndex(object item)
        {
            if (item == null)
                return 0;

            UIElement element = item as UIElement;

            if (element == null)
                return 0;

            return Canvas.GetZIndex(element);
        }

        public int ZIndex
        {
            get
            {
                if (this.DataContext == null)
                    return Canvas.GetZIndex(this);
                return GetZIndex(this.DataContext);
            }
            set
            {
                if (this.DataContext == null)
                {
                    Canvas.SetZIndex(this, value);
                }
                else
                {
                    UIElement element = this.DataContext as UIElement;
                    if (element != null)
                        Canvas.SetZIndex(element, value);
                }
            }
        }

        #endregion

        #region CanRotate DP

        public static bool GetCanRotate(DependencyObject obj)
        {
            return (bool)obj.GetValue(CanRotateProperty);
        }

        public static void SetCanRotate(DependencyObject obj, bool value)
        {
            obj.SetValue(CanRotateProperty, value);
        }

        // Using a DependencyProperty as the backing store for CanvasCorner.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanRotateProperty =
            DependencyProperty.RegisterAttached("CanRotate", typeof(bool), typeof(ZoomCanvasItem), new FrameworkPropertyMetadata(true));

        #endregion

        #region IsDirty

        public static bool GetIsDirty(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDirtyProperty);
        }

        public static void SetIsDirty(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDirtyProperty, value);
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDirtyProperty =
            DependencyProperty.RegisterAttached("IsDirty", typeof(bool), typeof(ZoomCanvasItem),
                                                new FrameworkPropertyMetadata(true));

        public static bool GetItemIsDirty(object item)
        {
            if (item == null)
                return false;

            DependencyObject depObj = item as DependencyObject;

            if (depObj == null)
                return false;

            return ZoomCanvasItem.GetIsDirty(depObj);
        }

        public bool IsDirty
        {
            get
            {
                if (this.DataContext == null)
                    return ZoomCanvasItem.GetIsDirty(this);
                return GetItemIsDirty(this.DataContext);
            }
            set
            {
                if (this.DataContext == null)
                {
                    ZoomCanvasItem.SetIsDirty(this, value);
                }
                else
                {
                    DependencyObject depObj = this.DataContext as DependencyObject;
                    if (depObj != null)
                        ZoomCanvasItem.SetIsDirty(depObj, value);
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler ViewChanged;

        private void OnViewChanged()
        {
            if (ViewChanged != null)
            {
                ViewChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        public ShowView View
        {
            get
            {
                return DataContext as ShowView;
            }
        }

        public static bool GetItemCanRotate(object item)
        {
            bool canRotate = true;

            if (item == null)
                return canRotate;

            DependencyObject depObj = item as DependencyObject;

            if (depObj == null)
                return canRotate;
            
            canRotate = ZoomCanvasItem.GetCanRotate(depObj);
            
            return canRotate;
        }

        public ZoomCanvasItem()
        {
        }

        public void DisplayMatrix_ViewChanged(object sender, EventArgs e)
        {
            OnViewChanged();
        }
    }
}
