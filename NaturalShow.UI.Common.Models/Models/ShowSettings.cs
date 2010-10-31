using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NaturalShow.UI.Common.Models
{
    public class ShowSettings : ShowObjectModel
    {
        #region Properties

        #region ShowPath

        /// <summary>
        /// The <see cref="ShowPath" /> property's name.
        /// </summary>
        public const string ShowPathPropertyName = "ShowPath";

        private string _showPath = null;

        /// <summary>
        /// Gets the ShowPath property.
        /// </summary>
        public string ShowPath
        {
            get
            {
                return _showPath;
            }

            set
            {
                if (_showPath == value)
                {
                    return;
                }

                var oldValue = _showPath;
                _showPath = value;

                // Update bindings, no broadcast
                RaisePropertyChanged(ShowPathPropertyName);
            }
        }

        #endregion

        #region ImagePath

        /// <summary>
        /// The <see cref="ImagePath" /> property's name.
        /// </summary>
        public const string ImagePathPropertyName = "ImagePath";

        private string _imagePath = null;

        /// <summary>
        /// Gets the ImagePath property.
        /// </summary>
        public string ImagePath
        {
            get
            {
                return _imagePath;
            }

            set
            {
                if (_imagePath == value)
                {
                    return;
                }

                var oldValue = _imagePath;
                _imagePath = value;
                
                // Update bindings, no broadcast
                RaisePropertyChanged(ImagePathPropertyName);
                RaisePropertyChanged(ImageUriPropertyName);
            }
        }

        #endregion

        #region ImageUri

        /// <summary>
        /// The <see cref="ImageUri" /> property's name.
        /// </summary>
        public const string ImageUriPropertyName = "ImageUri";

        private Uri _imageUri = null;

        /// <summary>
        /// Gets the ImageUri property.
        /// </summary>
        public Uri ImageUri
        {
            get
            {
                return new Uri(ImagePath);
            }
        }

        #endregion

        #endregion

        #region Constructors

        public ShowSettings()
        {

        }

        #endregion
    }
}
