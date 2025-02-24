//
// (c) duketwo 2022
//

extern alias SC;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVESharpCore.Cache;
using EVESharpCore.Controllers.ActionQueue.Actions.Base;
using EVESharpCore.Controllers.Base;
using EVESharpCore.Framework;
using EVESharpCore.Lookup;
using EVESharpCore.Questor.States;
using SC::SharedComponents.EVE.ClientSettings;
using SC::SharedComponents.EVE.DatabaseSchemas;
using SC::SharedComponents.Extensions;
//using SC::SharedComponents.SQLite;
using SC::SharedComponents.Utility;
//using ServiceStack.OrmLite;
using SharpDX.Direct2D1;


namespace EVESharpCore.Controllers.Abyssal
{
    public partial class AbyssalController : AbyssalBaseController
    {

        public void PVPState()
        {

            // Check if we can open another filament, if we can we go into a new abyss, else we need to fight

            // 1. If invulnerable start overheat before activating modules to not break the invuln. If not invuln overheat anyway. Overheat hardener, shield booster, prop mod.
            // 2. Lauch EC-600 drones if avail.
            // 3. Activate modules. Activate all modules at once. At this point we lose the invulnerable state. Move towards a position which gets the highest angular velocity to the enemies.
            // 4. If we are actively being aggressed, activate the assault damage control.
            // 5. If we are in a pod, bring the pod to a safe spot.
            // 6. If we survived the gank, let them know in local

        }

    }
}
