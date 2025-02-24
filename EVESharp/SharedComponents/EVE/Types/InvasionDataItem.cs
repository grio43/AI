using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.EVE.Types
{
    [Serializable]
    public class InvasionDataItem
    {
        public InvasionDataItem(int solarSystemId, int templateId, DateTime lastUpdate, InvasionType invasionType, double influence, int state)
        {
            SolarSystemId = solarSystemId;
            TemplateId = templateId;
            LastUpdate = lastUpdate;
            InvasionType = invasionType;
            Influence = influence;
            State = state;
        }

        public int SolarSystemId { get; private set; }
        public int TemplateId { get; private set; }
        public DateTime LastUpdate { get; private set; }
        public InvasionType InvasionType { get; private set; }
        public double Influence { get; private set; }
        public int State { get; private set; }

        public override string ToString()
        {
            return $"{nameof(SolarSystemId)}: {SolarSystemId}, {nameof(TemplateId)}: {TemplateId}, {nameof(LastUpdate)}: {LastUpdate}, {nameof(InvasionType)}: {InvasionType}, {nameof(Influence)}: {Influence}, {nameof(State)}: {State}";
        }
    }
}
