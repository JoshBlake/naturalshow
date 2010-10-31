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
using System.ComponentModel;
using NaturalShow.UI.Common.Models;
using NaturalShow.UI.Common.Views;
using System.IO;
using System.Windows.Media.Animation;
using Blake.NUI.WPF.Utility;
using NaturalShow.UI.Common.Settings;

namespace NaturalShow.UI.Common.Controls
{

    public partial class ControlBar : UserControl, INotifyPropertyChanged
    {
        #region Class Members

        public bool IsLocked { get; private set; }
        public bool IsImageLocked { get; private set; }

        string path;
        string filter;

        private bool canMapBeTriggered = false;

        private double _barYPosition = 100;
        private double BarYPosition
        {
            get
            {
                return _barYPosition;
            }
            set
            {
                _barYPosition = value;
                Canvas.SetTop(pnlBar, _barYPosition);
                //                transform.Y = _barYPosition;
                NotifyPropertyChanged("BarYPosition");
            }
        }

        double closePosition;
        double transitionPosition;
        double openPosition;
        double maxPosition;

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        #endregion

        #region Events

        public event EventHandler<LockChangedEventArgs> LockChanged;
        private void OnLockChanged(bool isLocked)
        {
            if (LockChanged != null)
            {
                LockChanged(this, new LockChangedEventArgs(isLocked));
            }
        }

        public event EventHandler SaveCommand;
        private void OnSaveCommand()
        {
            if (SaveCommand != null)
            {
                SaveCommand(this, EventArgs.Empty);
            }
        }

        public event EventHandler<AddObjectEventArgs> AddZoneCommand;
        private void OnAddZoneCommand(Point center, Size size)
        {
            if (AddZoneCommand != null)
            {
                AddZoneCommand(this, new AddObjectEventArgs(center, size));
            }
        }

        private void OnAddMapCommand(Point center, Size size)
        {
            if (AddZoneCommand != null)
            {
                AddZoneCommand(this, new AddObjectEventArgs(center, size, true));
            }
        }

        public event EventHandler<AddObjectEventArgs> AddImageCommand;
        private void OnAddImageCommand(Point center, Size size, string uriPath)
        {
            if (AddImageCommand != null)
            {
                AddImageCommand(this, new AddObjectEventArgs(center, size, uriPath));
            }
        }

        #endregion

        #region Constructors

        public ControlBar()
        {
            InitializeComponent();

            path = SettingsProxy.GetShowResourcesPath();
            filter = "*.png";
            
            SetLock(true);
            this.Loaded += new RoutedEventHandler(ControlBar_Loaded);
            this.LayoutUpdated += new EventHandler(ControlBar_LayoutUpdated);
            Canvas.SetRight(pnlBar, Application.Current.MainWindow.ActualWidth);

            FileSystemWatcher watcher = new FileSystemWatcher(path, filter);
            watcher.Created += new FileSystemEventHandler(watcher_Created);
            watcher.EnableRaisingEvents = true;
        }

        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            bool access = Dispatcher.CheckAccess();
            Dispatcher.Invoke((Action)delegate { GenerateImageControl(e.FullPath); });
            //GenerateImageControl(e.FullPath);
        }

        void ControlBar_LayoutUpdated(object sender, EventArgs e)
        {
            UpdatePositions();
        }

        void ControlBar_Loaded(object sender, RoutedEventArgs e)
        {

            SetupControls();
            UpdatePositions();
            BarYPosition = closePosition;
        }

        private void UpdatePositions()
        {
            closePosition = LayoutRoot.ActualHeight - 120.0;
            transitionPosition = closePosition - 100;
            openPosition = 0;
            maxPosition = Math.Min(0, LayoutRoot.ActualHeight - pnlBar.ActualHeight);

        }

        #endregion

        #region Control Setup

        private void SetupControls()
        {
            GenerateZoneControl();
            GenerateImageControl("");

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fi = di.GetFiles(filter);
            foreach (FileInfo file in fi)
            {
                string uriPath = path + file.Name;
                GenerateImageControl(uriPath);
            }

            GenerateMapControl();
        }

        private void GenerateImageControl(string uriPath)
        {
            ShowImageModel imageModel = new ShowImageModel();

            imageModel.UriPath = uriPath;
            imageModel.Text = uriPath;
            Viewbox vb = new Viewbox();
            vb.Width = 100;
            vb.Height = 100;
            vb.Margin = new Thickness(20.0);

            ShowImageView imageView = new ShowImageView(imageModel);
            imageView.Background = new SolidColorBrush(Colors.Transparent);

            vb.Child = imageView;

            imageView.IsManipulationEnabled = true;
            imageView.ManipulationDelta += imageView_ManipulationDelta;

            pnlControls.Children.Add(vb);
        }

        void imageView_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ShowImageView imageView = sender as ShowImageView;

            if (imageView == null)
                return;

            if (Math.Abs(e.CumulativeManipulation.Translation.Y) < 15)
            {
                e.Handled = true;
                if (e.CumulativeManipulation.Translation.X < -10)
                {
                    Size size = imageView.RenderSize;
                    Point center = imageView.PointToScreen(new Point(imageView.ActualWidth / 2, imageView.ActualHeight / 2));

                    ShowImageModel model = imageView.Model as ShowImageModel;

                    if (model != null)
                        OnAddImageCommand(center, size, model.Text);

                    e.Complete();
                }
            }
        }

        private void GenerateZoneControl()
        {
            ShowZoneModel zoneModel = new ShowZoneModel();
            zoneModel.Text = "Zone";

            Viewbox vb = new Viewbox();
            vb.Width = 100;
            vb.Height = 100;
            vb.Margin = new Thickness(20.0);

            ShowZoneView zoneView = new ShowZoneView(zoneModel);
            zoneView.Background = new SolidColorBrush(Colors.Transparent);

            vb.Child = zoneView;

            zoneView.IsManipulationEnabled = true;
            zoneView.ManipulationDelta += zoneView_ManipulationDelta;

            pnlControls.Children.Add(vb);
        }

        void zoneView_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ShowZoneView zoneView = sender as ShowZoneView;

            if (zoneView == null)
                return;

            if (Math.Abs(e.CumulativeManipulation.Translation.Y) < 15)
            {
                e.Handled = true;
                if (e.CumulativeManipulation.Translation.X < -10)
                {
                    Size size = zoneView.RenderSize;
                    Point center = zoneView.PointToScreen(new Point(zoneView.ActualWidth / 2, zoneView.ActualHeight / 2));

                    OnAddZoneCommand(center, size);

                    e.Complete();
                }
            }
        }

        private void GenerateMapControl()
        {
            ShowZoneModel zoneModel = new ShowZoneModel();
            zoneModel.Text = "Map";

            Viewbox vb = new Viewbox();
            vb.Width = 100;
            vb.Height = 100;
            vb.Margin = new Thickness(20.0);

            ShowZoneView zoneView = new ShowZoneView(zoneModel);
            zoneView.Background = new SolidColorBrush(Colors.Transparent);

            vb.Child = zoneView;

            zoneView.IsManipulationEnabled = true;
            zoneView.ManipulationStarting += zoneView_ManipulationStarting;
            zoneView.ManipulationDelta += mapView_ManipulationDelta;

            pnlControls.Children.Add(vb);
        }

        void zoneView_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            canMapBeTriggered = true;
        }

        void mapView_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ShowZoneView zoneView = sender as ShowZoneView;

            if (zoneView == null || !canMapBeTriggered)
                return;

            if (Math.Abs(e.CumulativeManipulation.Translation.Y) < 15)
            {
                e.Handled = true;
                if (e.CumulativeManipulation.Translation.X < -10)
                {
                    Size size = zoneView.RenderSize;
                    Point center = zoneView.PointToScreen(new Point(zoneView.ActualWidth / 2, zoneView.ActualHeight / 2));

                    canMapBeTriggered = false;
                    e.Complete();
                    OnAddMapCommand(center, size);

                }
            }
            else
            {
                canMapBeTriggered = false;
            }
        }

        #endregion

        #region Bar Manipulation Methods

        private void SetLock(bool isLocked)
        {
            if (isLocked == this.IsLocked)
                return;

            this.IsLocked = isLocked;
            OnLockChanged(this.IsLocked);
            SetImageLock(this.IsLocked);
        }

        private void SetImageLock(bool isImageLocked)
        {
            if (isImageLocked == this.IsImageLocked)
                return;

            this.IsImageLocked = isImageLocked;
            if (isImageLocked)
            {
                imgLockIcon.Source = new BitmapImage(new Uri("pack://application:,,,/NaturalShow.UI.Common;component/Resources/Lock.png", UriKind.Absolute));
                AnimateUtility.AnimateElementDouble(pnlBar, FrameworkElement.OpacityProperty, 0.05, 0, 0.2);
                //                pnlControls.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                imgLockIcon.Source = new BitmapImage(new Uri("pack://application:,,,/NaturalShow.UI.Common;component/Resources/Unlock.png", UriKind.Absolute));
                AnimateUtility.AnimateElementDouble(pnlBar, FrameworkElement.OpacityProperty, 1.0, 0, 0.2);
                //                pnlControls.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Grid_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = LayoutRoot;
        }

        private void Grid_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            UpdateBarPosition(e);
        }

        private void UpdateBarPosition(ManipulationDeltaEventArgs e)
        {

            double newPosition = BarYPosition + e.DeltaManipulation.Translation.Y;

            if (newPosition > transitionPosition)
            {
                SetImageLock(true);
            }
            else
            {
                SetImageLock(false);
            }

            if (newPosition < maxPosition)
            {
                newPosition = maxPosition;
                e.Complete();
            }
            if (newPosition > closePosition)
            {
                newPosition = closePosition;
                e.Complete();
            }

            BarYPosition = newPosition;
        }

        private void Grid_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            double minInitialVelocity = 0.5;
            double flickVelocity = 3;
            double initialVelocity = e.InitialVelocities.LinearVelocity.Y;

            if (Math.Abs(initialVelocity) <= 0.01)
            {
                if (BarYPosition > transitionPosition)
                {
                    e.TranslationBehavior.DesiredDisplacement = closePosition - BarYPosition;
                    e.TranslationBehavior.InitialVelocity = new Vector(0, minInitialVelocity);
                }
            }
            else if (BarYPosition > transitionPosition &&
                     initialVelocity < 0.0)
            {
                e.TranslationBehavior.DesiredDisplacement = BarYPosition - openPosition;
                e.TranslationBehavior.InitialVelocity = new Vector(0, -flickVelocity);
            }
            else if (initialVelocity < 0.02 && BarYPosition > 0)
            {
                e.TranslationBehavior.DesiredDisplacement = BarYPosition;
                e.TranslationBehavior.InitialVelocity = new Vector(0, -flickVelocity);
            }
            else if (initialVelocity > 0.02 && BarYPosition > 0)
            {
                e.TranslationBehavior.DesiredDisplacement = closePosition - BarYPosition;
                e.TranslationBehavior.InitialVelocity = new Vector(0, flickVelocity);
            }
            else
            {
                e.TranslationBehavior.DesiredDeceleration = 0.004;
            }
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (BarYPosition > transitionPosition)
            {
                BarYPosition = closePosition;
                SetLock(true);
            }
            else
            {
                SetLock(false);
            }
        }

        private void tglSave_TouchDown(object sender, TouchEventArgs e)
        {
            DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();

            EasingDoubleKeyFrame dk1 = new EasingDoubleKeyFrame(1.2, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(250)));
            QuadraticEase qe = new QuadraticEase();
            qe.EasingMode = EasingMode.EaseOut;
            dk1.EasingFunction = qe;

            EasingDoubleKeyFrame dk2 = new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500)));
            QuadraticEase qe2 = new QuadraticEase();
            qe2.EasingMode = EasingMode.EaseIn;
            dk2.EasingFunction = qe2;

            da.KeyFrames.Add(dk1);
            da.KeyFrames.Add(dk2);

            saveScale.BeginAnimation(ScaleTransform.ScaleXProperty, da);
            saveScale.BeginAnimation(ScaleTransform.ScaleYProperty, da);

            OnSaveCommand();

        }

        #endregion
    }

    public class LockChangedEventArgs : EventArgs
    {
        public bool IsLocked { get; private set; }

        public LockChangedEventArgs(bool isLocked)
        {
            this.IsLocked = isLocked;
        }
    }

    public class AddObjectEventArgs : EventArgs
    {
        public Point Center { get; private set; }
        public Size Size { get; private set; }
        public string UriPath { get; private set; }
        public bool IsMap { get; private set; }

        public AddObjectEventArgs(Point center, Size size)
        {
            this.Center = center;
            this.Size = size;
        }

        public AddObjectEventArgs(Point center, Size size, bool isMap)
        {
            this.Center = center;
            this.Size = size;
            this.IsMap = isMap;
        }

        public AddObjectEventArgs(Point center, Size size, string uriPath)
        {
            this.Center = center;
            this.Size = size;
            this.UriPath = uriPath;
        }
    }
}
