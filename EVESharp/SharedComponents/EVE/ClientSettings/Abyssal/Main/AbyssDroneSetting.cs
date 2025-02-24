using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.EVE.ClientSettings.Abyssal.Main
{
    [Serializable]
    public class AbyssDrone
    {
        [Browsable(false)]
        public DroneSize Type { get; set; }

        public int TypeId { get; set; }

        public int Amount { get; set; }
    }

    [Serializable]
    public enum DroneSize
    {
        Small,
        Medium,
        Large,
    }


}
