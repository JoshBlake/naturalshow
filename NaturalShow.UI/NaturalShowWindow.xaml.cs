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
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Ink;
using System.Diagnostics;
using System.Windows.Media.Animation;
using NaturalShow.UI.Common.Controls;
using NaturalShow.UI.Common.Models;
using NaturalShow.UI.Common.Views;
using NaturalShow.UI.Common.Factory;
using System.Threading;
using Blake.NUI.WPF.Touch;
using Blake.NUI.WPF.Common;
using Blake.NUI.WPF.Utility;
using NaturalShow.UI.Common.Settings;

namespace NaturalShow.UI
{

    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class NaturalShowWindow : Window
    {
        #region Fields

        TextEntryCursor textEntry;

        ShowContext showContext = ShowContext.DefaultContext;

        string path;
        string showfile;
        string showfiletemp;

        bool isLocked = true;
        
        Random rand = new Random();
        
        DispatcherTimer saveTimer;

        bool isSaveNeeded;

        Dictionary<TouchDevice, Ellipse> auras = new Dictionary<TouchDevice, Ellipse>();

        #endregion

        #region Constructors
        public NaturalShowWindow()
        {
            InitializeComponent();

            NativeTouchDevice.RegisterEvents(this);
            TouchVisualizationAdorner.AddTouchVisualizations(this);
            
            path = SettingsProxy.GetShowResourcesPath();
            showfile = SettingsProxy.GetAbsoluteShowFile();
            showfiletemp = SettingsProxy.GetAbsoluteShowTempFile();
            SetupEvents();

            Init();

            saveTimer = new DispatcherTimer();
            saveTimer.Interval = TimeSpan.FromMilliseconds(1000);
            saveTimer.Tick += new EventHandler(saveTimer_Tick);
            saveTimer.Start();

            this.Closed += new EventHandler(NaturalShowWindow_Closed);
        }

        void NaturalShowWindow_Closed(object sender, EventArgs e)
        {
            saveTimer.Stop();
            zoomCanvas.Closing();
        }

        #endregion
        
        #region Methods

        private void SetupEvents()
        {
            this.TextInput += NaturalShowWindow_TextInput;
            this.KeyDown += NaturalShowWindow_KeyDown;

            zoomCanvas.ViewChanged += zoomCanvas_ViewChanged;
            zoomCanvas.ObjectMoved += zoomCanvas_ObjectMoved;
            zoomCanvas.ObjectSelected += zoomCanvas_ObjectSelected;
            zoomCanvas.ObjectDoubleTap += zoomCanvas_ObjectDoubleTap;

            this.PreviewTouchUp += NaturalShowWindow_PreviewTouchUp;
            
            controlBar.AddZoneCommand += controlBar_AddZoneCommand;
            controlBar.AddImageCommand += controlBar_AddImageCommand;
            controlBar.LockChanged += controlBar_LockChanged;
            controlBar.SaveCommand += controlBar_SaveCommand;
        }

        void controlBar_AddImageCommand(object sender, AddObjectEventArgs e)
        {
            ShowImageModel imgModel = new ShowImageModel();
            imgModel.UriPath = e.UriPath;

            Point centerStart = zoomCanvas.PointFromScreen(e.Center);

            Point centerTarget = new Point(zoomCanvas.ActualWidth / 2 + rand.Next(-100, 100),
                                           zoomCanvas.ActualHeight / 2 + rand.Next(-100, 100));

            double orientationStart = -180;
            double orientationTarget = 0;

            Vector scaleStart = new Vector(0.1, 0.1);
            Vector scaleTarget = zoomCanvas.ViewScaleInvert;

            GenerateModel(imgModel, centerStart, centerTarget, orientationStart, orientationTarget, scaleStart, scaleTarget);

        }

        void controlBar_SaveCommand(object sender, EventArgs e)
        {
            textEntry.DeselectCursor();
            showContext.Save(showfile);
        }

        void controlBar_LockChanged(object sender, LockChangedEventArgs e)
        {
            isLocked = e.IsLocked;

            showContext.SetLocked(isLocked);
            textEntry.DeselectCursor();
        }

        void controlBar_AddZoneCommand(object sender, AddObjectEventArgs e)
        {
            Point centerStart = zoomCanvas.PointFromScreen(e.Center);

            Point centerTarget = new Point(zoomCanvas.ActualWidth / 2 + rand.Next(-100, 100),
                                           zoomCanvas.ActualHeight / 2 + rand.Next(-100, 100));

            double orientationStart = -180;
            double orientationTarget = 0;

            Vector scaleStart = new Vector(0.2, 0.2);
            Vector scaleTarget = zoomCanvas.ViewScaleInvert * 3;

            if (e.IsMap)
            {
                GenerateMapModel(centerStart, centerTarget, orientationStart, orientationTarget, scaleStart, scaleTarget);
            }
            else
            {
                ShowZoneModel zoneModel = new ShowZoneModel();
                zoneModel.Text = "Zone";

                ShowView view = GenerateModel(zoneModel, centerStart, centerTarget, orientationStart, orientationTarget, scaleStart, scaleTarget);

                textEntry.SelectCursor(view);
                textEntry.SelectAll = true;
            }
        }

        void zoomCanvas_ObjectDoubleTap(object sender, ObjectSelectedEventArgs e)
        {
            if (this.isLocked)
            {

                ShowView view = e.AffectedObject;
                Rect bounds = view.Bounds;
                DisplayMatrix data = view.Model.DisplayMatrix;

                double margin = 0.95;
                double scaleFactorX = margin * zoomCanvas.ActualWidth / (view.ActualWidth * data.Scale.X);
                double scaleFactorY = margin * zoomCanvas.ActualHeight / (view.ActualHeight * data.Scale.Y);
                double scaleFactor = Math.Min(scaleFactorX, scaleFactorY);
                Vector scale = new Vector(scaleFactor, scaleFactor);

                DisplayMatrix newTransform = new DisplayMatrix();
                newTransform.Center = zoomCanvas.CurrentView.Center;
                newTransform.Orientation = zoomCanvas.CurrentView.Orientation;
                newTransform.Scale = scale;

                Point p1 = newTransform.TransformMatrix.Transform(data.Center);

                Vector offset = new Vector(zoomCanvas.ActualWidth / 2, zoomCanvas.ActualHeight / 2);
                Vector p2 = offset - new Vector(p1.X, p1.Y);

                Point center = zoomCanvas.CurrentView.Center + p2;

                ZoomViewTo(center, 0, scale);
            }
            else
            {
                showContext.SendToFront(e.AffectedObject.Model);
            }
        }

        private void ZoomViewTo(Point center, double orientation, Vector scale)
        {
            CircleEase ease = new CircleEase();
            ease.EasingMode = EasingMode.EaseInOut;
            AnimateUtility.AnimateElementPoint(zoomCanvas.CurrentView,  DisplayMatrix.CenterProperty, center, 0, 1.5, ease);
            AnimateUtility.AnimateElementDouble(zoomCanvas.CurrentView, DisplayMatrix.OrientationProperty, orientation, 0, 1.5, ease);
            AnimateUtility.AnimateElementVector(zoomCanvas.CurrentView, DisplayMatrix.ScaleProperty, scale, 0, 1.5, ease);
        }

        void zoomCanvas_ObjectSelected(object sender, ObjectSelectedEventArgs e)
        {
            if (!e.AffectedObject.IsLocked)
            {
                textEntry.SelectCursor(e.AffectedObject);
            }
        }

        void NaturalShowWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                if (!isLocked)
                {
                    textEntry.DoBackspace();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                if (!isLocked)
                {
                    textEntry.DoDelete();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                textEntry.DeselectCursor();

                e.Handled = true;
            }
        }

        void NaturalShowWindow_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            isSaveNeeded = true;
        }

        void saveTimer_Tick(object sender, EventArgs e)
        {
            if (isSaveNeeded)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(SaveWorker));
            }
        }

        private void SaveWorker(object stateInfo)
        {
            isSaveNeeded = false;
            showContext.Save(showfiletemp);
        }

        private void Init()
        {
            textEntry = new TextEntryCursor(zoomCanvas, showContext);

            showContext.Load(showfile);

            isLocked = true;

            showContext.SetLocked(true);

            List<ShowView> views = showContext.CreateViews();

            foreach (ShowView view in views)
            {
                zoomCanvas.Items.Add(view);
            }

        }

        void zoomCanvas_ViewChanged(object sender, EventArgs e)
        {
            textEntry.DeselectCursor();
        }

        void zoomCanvas_ObjectMoved(object sender, ObjectMovedEventArgs e)
        {

        }

        void NaturalShowWindow_TextInput(object sender, TextCompositionEventArgs e)
        {
            if (!this.isLocked)
            {
                textEntry.DoTextEntry(e.Text);
            }
        }

        #endregion
        
        #region Drag Drop

        #endregion

        #region UI event handlers

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Init();
        }


        #endregion

        private void inkCanvas_TouchDown(object sender, TouchEventArgs e)
        {

        }

        private void inkCanvas_TouchUp(object sender, TouchEventArgs e)
        {

        }

        private ShowView GenerateModel(ShowModel model, Point centerStart, Point centerTarget, double orientationStart, double orientationTarget, Vector scaleStart, Vector scaleTarget)
        {
            centerStart = zoomCanvas.ScreenToWorld(centerStart);
            centerTarget = zoomCanvas.ScreenToWorld(centerTarget);

            model.IsLocked = this.isLocked;
            model.DisplayMatrix.Center = centerStart;
            model.DisplayMatrix.Orientation = orientationStart;
            model.DisplayMatrix.Scale = scaleStart;

            AnimateUtility.AnimateElementPoint( model.DisplayMatrix, DisplayMatrix.CenterProperty, centerTarget, 0, 1);
            AnimateUtility.AnimateElementDouble(model.DisplayMatrix, DisplayMatrix.OrientationProperty, orientationTarget, 0, 1);
            AnimateUtility.AnimateElementVector(model.DisplayMatrix, DisplayMatrix.ScaleProperty, scaleTarget, 0, 1);

            showContext.Models.Add(model);
            showContext.SendToFront(model);

            ShowView view = showContext.CreateView(model);
            zoomCanvas.Items.Add(view);

            return view;
        }
        
        private void GenerateMapModel(Point centerStart, Point centerTarget, double orientationStart, double orientationTarget, Vector scaleStart, Vector scaleTarget)
        {
            ShowZoneModel mapModel = new ShowZoneModel();
            mapModel.Text = "Bing Maps 3D";
            ShowMapView mapView = new ShowMapView(mapModel);
        
            ShowZoneModel model = mapView.Model as ShowZoneModel;

            if (model == null)
                return;

            centerStart = zoomCanvas.ScreenToWorld(centerStart);
            centerTarget = zoomCanvas.ScreenToWorld(centerTarget);

            model.IsMap = true;
            model.IsLocked = this.isLocked;
            model.DisplayMatrix.Center = centerStart;
            model.DisplayMatrix.Orientation = orientationStart;
            model.DisplayMatrix.Scale = scaleStart;

            AnimateUtility.AnimateElementPoint( model.DisplayMatrix, DisplayMatrix.CenterProperty, centerTarget, 0, 1);
            AnimateUtility.AnimateElementDouble(model.DisplayMatrix, DisplayMatrix.OrientationProperty, orientationTarget, 0, 1);
            AnimateUtility.AnimateElementVector(model.DisplayMatrix, DisplayMatrix.ScaleProperty, scaleTarget, 0, 1);

            if (!showContext.Models.Contains(model))
            {
                showContext.Models.Add(model);
            }
            showContext.SendToFront(model);

            if (!zoomCanvas.Items.Contains(mapView))
            {
                zoomCanvas.Items.Add(mapView);
            }
        }
         
    }
}