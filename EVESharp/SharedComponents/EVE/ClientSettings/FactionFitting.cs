using System;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SharedComponents.EVE.ClientSettings
{
    /**
    [Serializable]
    public class FactionFitting
    {
        #region Properties

        public int Dronetype { get; set; }
        public string Faction { get; set; }
        public string FittingName { get; set; }

        #endregion Properties

        public FactionFitting(XElement xmlMissionFitting)
        {
            Dronetype = (int)xmlMissionFitting.Attribute("dronetype");
            FittingName = (string)xmlMissionFitting.Attribute("fitting");
            Faction = (string)xmlMissionFitting.Attribute("faction");
            //ShipName = (string)xmlMissionFitting.Attribute("ship");
        }
    }
    **/

    [Serializable]
    [XmlRoot(ElementName = "factionfitting")]
    public class FactionFitting
    {
        #region Properties

        [XmlAttribute(AttributeName = "dronetype")]
        public int Dronetype { get; set; }

        [XmlAttribute(AttributeName = "faction")]
        public string FactionName { get; set; }

        [XmlAttribute(AttributeName = "fitting")]
        public string FittingName { get; set; }

        [XmlAttribute(AttributeName = "ship")]
        public string ShipName { get; set; }

        #endregion Properties

        public FactionFitting(XElement xmlMissionFitting)
        {
            //Dronetype = (int)xmlMissionFitting.Attribute("dronetype") ;
            FittingName = (string)xmlMissionFitting.Attribute("fitting");
            FactionName = (string)xmlMissionFitting.Attribute("faction");
            ShipName = (string)xmlMissionFitting.Attribute("ship") ?? string.Empty;
        }
    }
}