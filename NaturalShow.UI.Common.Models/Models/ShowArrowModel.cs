using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SurfaceShow.Common.Models
{
    public class ShowArrowModel : ShowObjectModel
    {
        #region Properties

        private Point _startOffset;
        public Point StartOffset
        {
            get
            {
                return _startOffset;
            }
            set
            {
                _startOffset = value;
                NotifyPropertyChanged("StartOffset");
            }
        }

        private Point _endOffset;
        public Point EndOffset
        {
            get
            {
                return _endOffset;
            }
            set
            {
                _endOffset = value;
                NotifyPropertyChanged("EndOffset");
            }
        }

        private int _startObjectID;
        public int StartObjectID
        {
            get
            {
                return _startObjectID;
            }
            set
            {
                _startObjectID = value;
                NotifyPropertyChanged("StartObjectID");
            }
        }

        private int _endObjectID;
        public int EndObjectID
        {
            get
            {
                return _endObjectID;
            }
            set
            {
                _endObjectID = value;
                NotifyPropertyChanged("EndObjectID");
            }
        }
                
        #endregion

        #region Constructors

        public ShowArrowModel()
            : base()
        {
            this.Type = ShowObjectType.Arrow;

            this.CanRotate = true;

        }

        #endregion
    }
}
