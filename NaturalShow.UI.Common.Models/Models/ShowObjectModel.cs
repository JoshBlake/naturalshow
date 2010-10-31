using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace NaturalShow.UI.Common.Models
{

    public enum ShowObjectType
    {
        Text,
        Image,
        Slide,
        Arrow,
        Object
    }

    public class ShowObjectModel : ShowModel
    {
        #region Properties

        private ShowObjectType _type;
        public ShowObjectType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                RaisePropertyChanged("Type");
            }
        }
        
        private string _placeholderUriPath;
        public string PlaceholderUriPath
        {
            get
            {
                return _placeholderUriPath;
            }
            set
            {
                _placeholderUriPath = value;
                RaisePropertyChanged("PlaceholderUri");
                RaisePropertyChanged("PlaceholderUriPath");
            }
        }

        public Uri PlaceholderUri
        {
            get
            {
                return new Uri(PlaceholderUriPath);
            }
        }

        #endregion

        
        #region Constructors

        public ShowObjectModel()
            : base()
        {
            Type = ShowObjectType.Object;

            Random rand = new Random();
            int randImage = rand.Next(1, 8);
            PlaceholderUriPath = "pack://application:,,,/Resources/Images/Thumbnail-" + randImage.ToString() + ".png";

        }

        #endregion

    }
}
