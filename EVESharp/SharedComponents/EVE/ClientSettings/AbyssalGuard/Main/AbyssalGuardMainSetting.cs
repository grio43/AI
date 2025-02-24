using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.EVE.ClientSettings.AbyssalGuard.Main
{

    public enum AbyssalGuardMode
    {
        Orca,
        ConcordSpawner,
    }

    [Serializable]
    public class AbyssalGuardMainSetting
    {
        public AbyssalGuardMainSetting()
        {

        }

        [Description("The name of the bookmark where the abyss runner will open the abyss.")]
        public string AbyssalBookmarkName { get; set; }

        [Description("The name of the bookmark where we will move after we got ganked.")]
        public string AbyssalHomeBookmarkName { get; set; }

        [Description("The character name of the abyssal runner.")]
        public string AbyssCharacterName { get; set; }

        [Description("The character name of the abyssal runner.")]
        public AbyssalGuardMode GuardMode { get; set; }

    }
}
