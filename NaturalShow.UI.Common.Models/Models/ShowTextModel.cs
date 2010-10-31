using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace NaturalShow.UI.Common.Models
{
    public class ShowTextModel : ShowObjectModel
    {
        #region Properties
        


        #endregion

        #region Constructors

        public ShowTextModel()
            : base()
        {
            this.Type = ShowObjectType.Text;
            Text = "";
            FontSize = 24;
            Color = Colors.White;

        }

        #endregion
    }
}
