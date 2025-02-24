using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.EVE.ClientSettings.Pinata.Main
{
    [Serializable]
    public class PinataMainSetting
    {
        public PinataMainSetting()
        {
            
        }
        public Region Region { get; set; }
    }
}
