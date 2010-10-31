using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using NaturalShow.UI.Common.Models;
using NaturalShow.UI.Common.Views;
using System.Windows.Controls;
using NaturalShow.UI.Common.Controls;

namespace NaturalShow.UI.Common.Factory
{
    public class TextEntryCursor
    {
        public ShowModel CursorModel { get; set; }
        public ShowView CursorView { get; set; }

        private ZoomCanvas _zoomCanvas;
        private ShowContext _showContext;

        public bool SelectAll { get; set; }

        public TextEntryCursor(ZoomCanvas itemsControl, ShowContext context)
        {
            DeselectCursor();
            _zoomCanvas = itemsControl;
            _showContext = context;
        }

        public void DoTextEntry(string input)
        {
            if (CursorModel == null)
            {
                CreateTextCursor();
            }
            if (SelectAll)
            {
                CursorModel.Text = "";
                SelectAll = false;
            }

            CursorModel.Text += input;
        }

        public void DoDelete()
        {
            ShowZoneModel zoneModel = CursorModel as ShowZoneModel;
            //if (zoneModel != null && zoneModel.IsMap)
            //{
            //    ShowMapView mapView = CursorView as ShowMapView;
            //    if (mapView != null)
            //    {
            //        mapView.map.Dispose();
            //        mapView.map = null;
            //    }
            //}

            if (_zoomCanvas.Items.Contains(CursorView))
            {
                _zoomCanvas.Items.Remove(CursorView);
            }
            if (_showContext.Models.Contains(CursorModel))
            {
                _showContext.Models.Remove(CursorModel);
            }
            DeselectCursor();
        }

        public void DoBackspace()
        {
            if (CursorModel == null)
            {
                return;
            }
            if (SelectAll)
            {
                CursorModel.Text = "";
                SelectAll = false;
            }

            string text = CursorModel.Text;
            if (text.Length != 0)
            {
                CursorModel.Text = text.Substring(0, text.Length - 1);
            }
        }

        public void DeselectCursor()
        {
            if (CursorModel != null)
            {
                CursorModel.Color = CursorModel.DefaultColor;
            }
            CursorModel = null;
            CursorView = null;
        }

        public void SelectCursor(ShowView view)
        {
            DeselectCursor();

            if (view == null)
                return;

            CursorView = view;
            CursorModel = view.Model;
            
            CursorModel.Color = Colors.Blue;

        }

        private void CreateTextCursor()
        {
            DeselectCursor();

            ShowTextModel textModel = new ShowTextModel();
            Point p = new Point(_zoomCanvas.ActualWidth / 2,
                                _zoomCanvas.ActualHeight * 0.75);

            textModel.DisplayMatrix.Center = _zoomCanvas.ScreenToWorld(p);
            textModel.DisplayMatrix.Scale = _zoomCanvas.ViewScaleInvert * 2;
            textModel.IsLocked = false;

            _showContext.Models.Add(textModel);

            ShowTextView textView = new ShowTextView(textModel);
            _zoomCanvas.Items.Add(textView);

            SelectCursor(textView);
        }
    }
}
