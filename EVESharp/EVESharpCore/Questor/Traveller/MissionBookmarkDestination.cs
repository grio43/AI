﻿// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace EVESharpCore.Questor.Traveller
{
    /**
    public class MissionBookmarkDestination2 : TravelerDestination
    {
        private DateTime _nextAction;

        public MissionBookmarkDestination2(DirectAgentMissionBookmark bookmark)
        {
            if (!ESCache.Instance.DirectEve.Session.IsReady) return;

            if (bookmark == null)
            {
                Log.WriteLine("Invalid mission bookmark!");

                AgentId = -1;
                Title = null;
                SolarSystemId = ESCache.Instance.DirectEve.Session.SolarSystemId ?? -1;
                return;
            }

            Log.WriteLine("Destination set to mission bookmark [" + bookmark.Title + "]");
            AgentId = bookmark.AgentId ?? -1;
            Title = bookmark.Title;
            SolarSystemId = bookmark.SolarSystemId ?? -1;
        }

        public MissionBookmarkDestination2(int agentId, string title)
            : this(GetMissionBookmark(agentId, title))
        {
        }

        public long AgentId { get; set; }

        public string Title { get; set; }

        private static DirectAgentMissionBookmark GetMissionBookmark(long agentId, string title)
        {
            var mission = ESCache.Instance.DirectEve.AgentMissions.FirstOrDefault(m => m.AgentId == agentId);
            if (mission == null)
                return null;

            return mission.Bookmarks.FirstOrDefault(b => b.Title.ToLower() == title.ToLower());
        }

        public override bool PerformFinalDestinationTask()
        {
            // Mission bookmarks have a 1.000.000 distance warp-to limit (changed it to 150.000.000 as there are some bugged missions around)
            return BookmarkDestination2.PerformFinalDestinationTask2(GetMissionBookmark(AgentId, Title), 150000000, ref _nextAction);
        }
    }
    **/
}