using System;

namespace SharedComponents.EVE.ClientSettings
{
    [Serializable]
    public class ShipFitting
    {
        #region Constructors

        public ShipFitting()
        {
        }

        public ShipFitting(string fittingName, string b64Fitting)
        {
            FittingName = fittingName;
            B64Fitting = b64Fitting;
        }

        #endregion Constructors

        #region Properties

        public string B64Fitting { get; set; }
        public string FittingName { get; set; }

        #endregion Properties
    }
}