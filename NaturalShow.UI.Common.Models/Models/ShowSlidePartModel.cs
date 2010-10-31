using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NaturalShow.UI.Common.Models;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace NaturalShow.UI.Common.Models
{
    public class ShowSlidePartModel : ShowObjectModel
    {
        #region Fields

        List<ShowSlidePartModel> _children = new List<ShowSlidePartModel>();

        #endregion

        #region Properties

        public List<ShowSlidePartModel> Children
        {
            get
            {
                return _children;
            }
        }

        private Guid _parentID;
        public Guid ParentID
        {
            get
            {
                return _parentID;
            }
            set
            {
                _parentID = value;
                RaisePropertyChanged("ParentID");
            }
        }
        

        private ShowSlidePartModel _parent;
        [XmlIgnore]
        public ShowSlidePartModel Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
                if (_parent != null)
                {
                    ParentID = _parent.ModelID;
                }
                RaisePropertyChanged("Parent");
            }
        }

        private Rect _bounds;
        public Rect Bounds
        {
            get
            {
                return _bounds;
            }
            set
            {
                _bounds = value;
                RaisePropertyChanged("Bounds");
            }
        }

        private int _indentLevel;
        public int IndentLevel
        {
            get
            {
                return _indentLevel;
            }
            set
            {
                _indentLevel = value;
                RaisePropertyChanged("IndentLevel");
            }
        }
        
        private string _uriPath;
        public string UriPath
        {
            get
            {
                return _uriPath;
            }
            set
            {
                if (value == null ||
                    value.Length == 0)
                    return;

                _uriPath = value;
                RaisePropertyChanged("UriPath");
                RaisePropertyChanged("Uri");
            }
        }

        public Uri Uri
        {
            get
            {
                if (UriPath == null)
                    return null;
                return new Uri(UriPath);
            }
        }

        private Size _size;
        public Size Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value; 
                RaisePropertyChanged("Size");
            }
        }
        
        #endregion

        #region Constructors
        
        public ShowSlidePartModel()
            : base()
        {
            this.Type = ShowObjectType.Slide;

            this.CanRotate = false;
            Size = new Size(150, 150);
        }

        public ShowSlidePartModel(string filename)
            : base()
        {
            this.Type = ShowObjectType.Slide;

            this.CanRotate = false;
            this.UriPath = filename;

            Size = new Size(150, 150);
        }

        #endregion

        #region Public Methods

        public void ConvertToText()
        {
            this.Type = ShowObjectType.Text;
            Text = "";
            FontSize = 24;
            Color = Colors.White;
        }

        #endregion

    }
}
