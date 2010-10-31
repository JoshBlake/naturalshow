using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Input;
using Blake.NUI.WPF.Common;
using System.Xml.Serialization;

namespace NaturalShow.UI.Common.Models
{
    public enum ShowModelDisplayMatrixMode
    {
        Primary,
        Secondary
    };

    public class ShowModel : Model
    {
        #region Properties
        
        private string _text;
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                DisplayMatrix.UpdateMatrix();
                RaisePropertyChanged("Text");
            }
        }

        private double _fontSize;
        public double FontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                _fontSize = value; RaisePropertyChanged("FontSize");
            }
        }

        public Color DefaultColor { get; set; }

        private Color _color;
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                _brush = new SolidColorBrush(_color);

                RaisePropertyChanged("Color");
                RaisePropertyChanged("Brush");
            }
        }

        private Brush _brush;
        public Brush Brush
        {
            get
            {
                if (_brush == null)
                    _brush = new SolidColorBrush(_color);

                return _brush;
            }
        }

        private Guid _modelID = Guid.NewGuid();
        public Guid ModelID
        {
            get
            {
                return _modelID;
            }
            set
            {
                _modelID = value;
                RaisePropertyChanged("ModelID");
            }
        }

        private bool _canRotate;
        public bool CanRotate
        {
            get
            {
                return _canRotate;
            }
            set
            {
                _canRotate = value;
                RaisePropertyChanged("CanRotate");
            }
        }
        
        //private Point _corner;
        //public Point Corner
        //{
        //    get
        //    {
        //        return _corner;
        //    }
        //    set
        //    {
        //        _corner = value;
        //        RaisePropertyChanged("Corner");
        //    }
        //}

        private bool _isGhost;
        public bool IsGhost
        {
            get
            {
                return _isGhost;
            }
            set
            {
                _isGhost = value;
                RaisePropertyChanged("IsGhost");
            }
        }

        private bool _isLocked;
        public bool IsLocked
        {
            get
            {
                return _isLocked;
            }
            set
            {
                _isLocked = value;
                RaisePropertyChanged("IsLocked");
            }
        }

        private long? _associatedTagValue;
        public long? AssociatedTagValue
        {
            get
            {
                return _associatedTagValue;
            }
            set
            {
                _associatedTagValue = value;
                RaisePropertyChanged("AssociatedTagValue");
            }
        }

        private ShowModelDisplayMatrixMode _displayMatrixMode;

        [XmlIgnore]
        public ShowModelDisplayMatrixMode DisplayMatrixMode
        {
            get
            {
                return _displayMatrixMode;
            }
            set
            {
                if (_displayMatrixMode == value)
                    return;

                _displayMatrixMode = value;
                RaisePropertyChanged("DisplayMatrixMode");
                RaisePropertyChanged("DisplayMatrix");
            }
        }

        [XmlIgnore]
        public DisplayMatrix DisplayMatrix
        {
            get
            {
                if (DisplayMatrixMode == ShowModelDisplayMatrixMode.Primary)
                    return PrimaryDisplayMatrix;
                else
                    return SecondaryDisplayMatrix;
            }
        }

        private DisplayMatrix _primaryDisplayMatrix;
        public DisplayMatrix PrimaryDisplayMatrix
        {
            get
            {
                return _primaryDisplayMatrix;
            }
            set
            {
                _primaryDisplayMatrix = value;
                RaisePropertyChanged("PrimaryDisplayMatrix");
            }
        }

        private DisplayMatrix _secondaryDisplayMatrix;
        public DisplayMatrix SecondaryDisplayMatrix
        {
            get
            {
                return _secondaryDisplayMatrix;
            }
            set
            {
                _secondaryDisplayMatrix = value;
                RaisePropertyChanged("SecondaryDisplayMatrix");
            }
        }

        #endregion
                        
        #region Constructors

        public ShowModel()
        {
            Color = Colors.White;
            DefaultColor = Colors.White;
            FontSize = 24;
            CanRotate = false;

            IsGhost = false;
            IsLocked = true;

            AssociatedTagValue = null;

            DisplayMatrixMode = ShowModelDisplayMatrixMode.Primary;
            PrimaryDisplayMatrix = new DisplayMatrix();
            SecondaryDisplayMatrix = new DisplayMatrix();
        }

        #endregion

    }
}
