using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVESharpCore.Cache;
using EVESharpCore.Framework;
using EVESharpCore.Logging;

namespace EVESharpCore.Controllers.ActionQueue.Actions
{
    public class WndEventLogTriggerAction : Base.ActionQueueAction
    {
        public WndEventLogTriggerAction()
        {
            this.Action = () =>
            {
                var jw = ESCache.Instance.DirectEve.Windows.OfType<DirectJournalWindow>().FirstOrDefault();
                if (jw == null)
                {

                    ESCache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenJournal);
                    return;
                }

                if (jw.SelectedMainTab != MainTab.AgentMissions)
                {
                    Log.WriteLine("Switching journal tab to Missions.");
                    jw.SwitchMaintab(MainTab.AgentMissions);
                    return;
                }
            };
        }
    }
}
