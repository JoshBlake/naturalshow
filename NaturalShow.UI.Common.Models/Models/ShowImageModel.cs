using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace NaturalShow.UI.Common.Models
{
    public class ShowImageModel : ShowObjectModel
    {
        #region Properties

        private string _uriPath;
        public string UriPath
        {
            get
            {
                return _uriPath;
            }
            set
            {
                if (value.Length == 0)
                    return;

                _uriPath = value;
                //_uriPath = _uriPath.Replace("E:", "D:");
                RaisePropertyChanged("UriPath");
                RaisePropertyChanged("Uri");
            }
        }

        public Uri Uri
        {
            get
            {
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
                _size = value; RaisePropertyChanged("Size");
            }
        }
        
        #endregion

        #region Constructors

        public ShowImageModel()
            : base()
        {
            this.Type = ShowObjectType.Image;

            this.CanRotate = true;

            Random rand = new Random();
            int randImage = rand.Next(1, 8);
            UriPath = "pack://application:,,,/NaturalShow.UI.Common;component/Resources/Images/Thumbnail-" + randImage.ToString() + ".png";

            Size = new Size(150, 150);
        }

        #endregion
    }
}
