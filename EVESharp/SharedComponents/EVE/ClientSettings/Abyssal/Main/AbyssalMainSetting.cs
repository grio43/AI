using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.EVE.ClientSettings.Abyssal.Main
{
    [Serializable]
    public class AbyssalMainSetting
    {
        public AbyssalMainSetting()
        {

        }
        public AbyssFilamentType FilamentType { get; set; }

        public AbyssShipType ShipType { get; set; }
        public int HomeSystemId { get; set; }

        [Description("M/s")]
        public int MaxSpeedWithPropMod { get; set; }

        [Description("Meters")]
        public int MaxDroneRange { get; set; }

        public int GigajoulePerSecExcess { get; set; }

        [Description("How many Kikimoaras can we tank?")]
        public int KikimoraTankThreshold { get; set; } = 3;

        [Description("How many Marhsals can we tank?")]
        public int MarshalTankThreshold { get; set; } = 1;

        [Description("How many Rougue Drone Battlecruisers can we tank?")]
        public int BCTankthreshold { get; set; } = 2;

        public int AmmoTypeId { get; set; }

        public MTUType MTUType { get; set; }

        [Description("Meters")]
        public int WeaponMaxRange { get; set; }

        public bool DisableOverheat { get; set; }

        public bool SplitDrones { get; set; } = true;

        public bool AlwaysMoveIntoWeaponRange { get; set; } = false;

        public string HomeStationBookmarkName { get; set; }

        public string RepairStationBookmarkName { get; set; }

        public string AbyssalBookmarkName { get; set; }

        [Description("Guard character name, can be left empty.")]
        public string AbyssalGuardCharacterName { get; set; }

        public string SurveyDumpStationBookmarkName { get; set; }

        [Description("The threshold for the bot to consider selling the database surveys. (Million)")]
        public long SurveyDumpThreshold { get; set; } = 2000;





        [Description("This value will be added on top  of the SurveyDumpThreshold for each day of the month. (Million)")]
        public long SurveyDumpDailyAdditionValue { get; set; } = 100;


        public BindingList<AbyssDrone> DroneBayItems { get; set; } = new BindingList<AbyssDrone>();

        public BindingList<AbyssBooster> Boosters { get; set; } = new BindingList<AbyssBooster>();

        [Description("Breidls wake up ping")]
        public bool SoundNotifications { get; set; }

    }

    public enum AbyssShipType
    {
        Ishtar, // 12005
        Gila, // 17715
        Vexor, // 626
        Worm, // 17930
        Algos, // 32872
        Tristan, // 593
		MaulusN, // 37456
        Cerberus, // 11993
    }

    public enum AbyssFilamentType
    {
        DarkT0, // 56132
        DarkT1, // 47762
        DarkT2, // 47892
        DarkT3, // 47893
        DarkT4, // 47894
        DarkT5, // 47895
        DarkT6, // 56140
        GammaT0, // 56136 
        GammaT1, // 47764
        GammaT2, // 47900
        GammaT3, // 47901
        GammaT4, // 47902
        GammaT5, // 47903
        GammaT6, // 56143 
        FirestormT0, // 56134
        FirestormT1, // 47763
        FirestormT2, // 47896
        FirestormT3, // 47897
        FirestormT4, // 47898
        FirestormT5, // 47899
        FirestormT6, // 56142
        ExoticT0, // 56133
        ExoticT1, // 47761
        ExoticT2, // 47888
        ExoticT3, // 47889
        ExoticT4, // 47890
        ExoticT5, // 47891
        ExoticT6, // 56141
        ElectricalT0, // 56131
        ElectricalT1, // 47765
        ElectricalT2, // 47904
        ElectricalT3, // 47905
        ElectricalT4, // 47906
        ElectricalT5, // 47907
        ElectricalT6, // 56139
    }

    public enum MTUType
    {
        Standard, // 33702
        Packrat, // 33700
        Magpie // 33475

    }

}
