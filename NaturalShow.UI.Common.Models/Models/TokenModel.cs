using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace NaturalShow.UI.Common.Models
{
    public class TokenModel : ShowModel
    {
        #region Properties

        #region LinkedIDs

        public const string LinkedIDsPropertyName = "LinkedIDs";

        private ObservableCollection<Guid> _linkedIDs = new ObservableCollection<Guid>();

        public ObservableCollection<Guid> LinkedIDs
        {
            get
            {
                return _linkedIDs;
            }
            set
            {
                if (_linkedIDs == value)
                {
                    return;
                }

                var oldValue = _linkedIDs;
                _linkedIDs = value;

                RaisePropertyChanged(LinkedIDsPropertyName);
            }
        }
        #endregion

        #region TagValue

        public const string TagValuePropertyName = "TagValue";

        private long _tagValue = 1;

        public long TagValue
        {
            get
            {
                return _tagValue;
            }

            set
            {
                if (_tagValue == value)
                {
                    return;
                }

                var oldValue = _tagValue;
                _tagValue = value;

                RaisePropertyChanged(TagValuePropertyName);
            }
        }

        #endregion

        #region IsDown

        [XmlIgnore]
        public bool IsDown { get; set; }

        #endregion

        #endregion

        #region Constructors

        public TokenModel()
            : base()
        {
            IsDown = false;
        }

        #endregion

        #region Public Methods

        public void AddLinkedID(Guid id)
        {
            if (LinkedIDs.Contains(id))
                return;
            
            LinkedIDs.Add(id);
        }

        public void RemoveLinkedID(Guid id)
        {
            if (!LinkedIDs.Contains(id))
                return;

            LinkedIDs.Remove(id);
        }

        #endregion
    }
}
