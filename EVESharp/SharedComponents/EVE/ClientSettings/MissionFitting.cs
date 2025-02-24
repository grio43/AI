using System;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SharedComponents.EVE.ClientSettings
{
    [Serializable]
    [XmlRoot(ElementName = "missionfitting")]
    public class MissionFitting
    {
        #region Properties

        //[XmlAttribute(AttributeName = "dronetype")]
        //public int Dronetype { get; set; }

        [XmlAttribute(AttributeName = "fitting")]
        public string FittingName { get; set; }

        [XmlAttribute(AttributeName = "mission")]
        public string MissionName { get; set; }

        [XmlAttribute(AttributeName = "ship")]
        public string ShipName { get; set; }

        #endregion Properties

        public MissionFitting(XElement xmlMissionFitting)
        {
            //Dronetype = (int)xmlMissionFitting.Attribute("dronetype") ?? Drones.DefaultDroneTypeID;
            FittingName = (string)xmlMissionFitting.Attribute("fitting");
            MissionName = (string)xmlMissionFitting.Attribute("mission");
            ShipName = (string)xmlMissionFitting.Attribute("ship") ?? string.Empty;
        }
    }

    /**
    [Serializable]
    public class MissionFitting
    {
        #region Properties

        public int Dronetype { get; set; }
        public string FittingName { get; set; }
        public string Mission { get; set; }
        public string ShipName { get; set; }

        #endregion Properties

        public MissionFitting(XElement xmlMissionFitting)
        {
            Dronetype = (int)xmlMissionFitting.Attribute("dronetype");
            FittingName = (string)xmlMissionFitting.Attribute("fitting");
            Mission = (string)xmlMissionFitting.Attribute("mission");
            ShipName = (string)xmlMissionFitting.Attribute("ship");
        }
    }
    **/
}