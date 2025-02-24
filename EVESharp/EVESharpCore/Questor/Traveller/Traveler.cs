// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

extern alias SC;

using EVESharpCore.Cache;
using EVESharpCore.Controllers.ActionQueue.Actions.Base;
using EVESharpCore.Framework;
using EVESharpCore.Logging;
using EVESharpCore.Questor.BackgroundTasks;
using EVESharpCore.Questor.Combat;
using EVESharpCore.Lookup;
using EVESharpCore.Questor.States;
using SC::SharedComponents.EVE;
using SC::SharedComponents.IPC;
using SC::SharedComponents.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using EVESharpCore.Questor.Behaviors;
using EVESharpCore.Controllers;
using EVESharpCore.Framework.Lookup;

namespace EVESharpCore.Questor.Traveller
{
    public static class Traveler
    {
        #region Constructors

        static Traveler()
        {
            Time.Instance.NextTravelerAction = DateTime.MinValue;
        }

        #endregion Constructors

        #region Properties

        public static TravelerDestination Destination
        {
            get => _destination;
            set
            {
                _destination = value;
                MyTravelToBookmark = null;
                if (value == null)
                {
                    if (DirectEve.Interval(5000)) Log.WriteLine("Destination is now null");
                    ControllerManager.Instance.GetController<ActionQueueController>().RemoveAllActions();
                    State.CurrentTravelerState = TravelerState.AtDestination;
                    ESCache.Instance.TaskSetEveAccountAttribute(nameof(EveAccount.TravelerDestinationSystemId), 0);
                }
                else
                {
                    //if (Destination != null && Destination.SolarSystem != null) Log.WriteLine("Destination is now [" + Destination.SolarSystem.Name + "]");
                    State.CurrentTravelerState = TravelerState.Idle;
                    ESCache.Instance.TaskSetEveAccountAttribute(nameof(EveAccount.TravelerDestinationSystemId), Destination.SolarSystemId);
                }
            }
        }

        internal static void ResetTraveler()
        {
            Destination = null;
            State.CurrentTravelerState = TravelerState.Idle;
        }

        #endregion Properties

        #region Fields

        public static DirectLocation _location;
        public static bool IgnoreSuspectTimer;
        private static TravelerDestination _destination { get; set; }
        //private static long? _destinationId;
        private static List<long> _destinationRoute;
        private static int _locationErrors;
        private static string _locationName;
        private static DateTime _nextGetLocation;
        private static IEnumerable<DirectBookmark> myHomeBookmarks;

        #endregion Fields

        #region Methods

        private static ActionQueueAction _covertCloakActionQueueAction;

        public static DirectBookmark MyTravelToBookmark { get; set; } = null;

        private static bool PlayNotificationSounds => true;

        internal static void HandleNotifications()
        {
            try
            {
                if (!DirectEve.HasFrameChanged())
                    return;

                //
                //if (IsTravelerAtDestination)
                {
                    if (DirectEve.Interval(10000))
                    {
                        Log.WriteLine("Notification!: At Destination");
                        if (PlayNotificationSounds) Util.PlayNoticeSound();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Exception [" + ex + "]");
            }
        }

        public static bool ChangeTravelerState(TravelerState state)
        {
            try
            {
                if (State.CurrentTravelerState != state)
                {
                    Log.WriteLine("New TravelerState [" + state + "]");
                    State.CurrentTravelerState = state;
                    if (State.CurrentTravelerState == TravelerState.AtDestination)
                    {
                        ResetStatesToDefaults(null);
                        return true;
                    }

                    if (State.CurrentTravelerState == TravelerState.Traveling && Time.Instance.NextTravelerAction > DateTime.UtcNow)
                    {
                        ProcessState();
                        return true;
                    }

                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine("Exception [" + ex + "]");
                return false;
            }
        }

        public static bool DefendOnTravel()
        {
            try
            {
                bool canWarp = true;
                if (ESCache.Instance.InWarp) return false;
                //
                // defending yourself is more important that the traveling part... so it comes first.
                //
                if (ESCache.Instance.InSpace)
                    if (ESCache.Instance.DirectEve.Modules.Any(i => i.IsCloak && i.IsActive))
                    {
                        if (DebugConfig.DebugGotobase) Log.WriteLine("Travel: _combat.ProcessState()");

                        if (Combat.Combat.TargetedBy.Any(t => t.IsWarpScramblingMe))
                        {
                            Drones.DronesShouldBePulled = false;
                            if (DebugConfig.DebugGotobase) Log.WriteLine("Travel: we are scrambled");
                            canWarp = false;
                        }
                    }

                if (NavigateOnGrid.SpeedTank && !Salvage.LootWhileSpeedTanking)
                {
                    if (DebugConfig.DebugTargetWrecks) Log.WriteLine("Salvage.OpenWrecks = false;");
                    Salvage.OpenWrecks = false;
                }

                return canWarp;
            }
            catch (Exception ex)
            {
                Log.WriteLine("Exception [" + ex + "]");
                return true;
            }
        }

        public static void LoadSettings(XElement CharacterSettingsXml, XElement CommonSettingsXml)
        {
            try
            {
                Log.WriteLine("LoadSettings: Traveler");
                IgnoreSuspectTimer =
                    (bool?)CharacterSettingsXml.Element("ignoreSuspectTimer") ??
                    (bool?)CommonSettingsXml.Element("ignoreSuspectTimer") ?? false;
                Log.WriteLine("Traveler: ignoreSuspectTimer [" + IgnoreSuspectTimer + "]");
            }
            catch (Exception exception)
            {
                Log.WriteLine("Error Loading Weapon and targeting Settings [" + exception + "]");
            }
        }

        private static void ActivateCovertOpsCloak()
        {
            if (ESCache.Instance.DirectEve.Modules.Any(i => i.IsCovertOpsCloak))
            {
                if (ESCache.Instance.DirectEve.Modules.Any(i => i.IsCovertOpsCloak && !i.IsActive))
                {
                    if (ESCache.Instance.DirectEve.Modules.FirstOrDefault(i => i.IsCovertOpsCloak).ActivateCovertOpsCloak)
                        Log.WriteLine("Cloak Activated");

                    return;
                }

                Traveler.BoolRunEveryFrame = false;
                return;

            }
        }

        public static void ProcessState()
        {
            // Only pulse state changes every 1000ms
            if (DateTime.UtcNow < Time.Instance.NextTravelerAction)
            {
                if (DebugConfig.DebugTraveler) Log.WriteLine("TravelerState.Traveling: if (DateTime.UtcNow > Time.Instance.NextTravelerAction)");
                return;
            }

            if (ESCache.Instance.InSpace && ESCache.Instance.Stations.Any(s => s.Distance < (double)Distances.TwentyFiveHundredMeters) && (ESCache.Instance.Entities.Any(e => e.IsPlayer && e.IsAttacking) || ESCache.Instance.Entities.Count(e => e.IsTargetedBy && e.IsPlayer) > 1))
            {
                var station = ESCache.Instance.Stations.FirstOrDefault(s => s.Distance < (double)Distances.TwentyFiveHundredMeters);
                if (ESCache.Instance.InWarp)
                {
                    Log.WriteLine($"We are outside of a station and being aggressed by another player or targeted by more than 2. Trying to stop the ship and dock.");
                    if (DirectEve.Interval(500, 1000))
                    {
                        ESCache.Instance.DirectEve.ExecuteCommand(EVESharpCore.Framework.DirectCmd.CmdStopShip);
                    }
                    return;
                }
                else
                {
                    Log.WriteLine("Docking attempt.");
                    station.Dock();
                    return;
                }
            }

            if (DebugConfig.DebugTraveler) Log.WriteLine("TravelerState.Traveling: CurrentTravelerState [" + State.CurrentTravelerState + "]");
            switch (State.CurrentTravelerState)
            {
                case TravelerState.Idle:
                    ChangeTravelerState(TravelerState.Traveling);
                    break;

                case TravelerState.Traveling:
                    if (!ESCache.Instance.InSpace && !ESCache.Instance.InStation)
                    {
                        if (DebugConfig.DebugTraveler) Log.WriteLine("TravelerState.Traveling: if (!ESCache.Instance.InSpace && !ESCache.Instance.InStation) return");
                        Time.Instance.NextTravelerAction = DateTime.UtcNow;
                        return;
                    }

                    ActivateCovertOpsCloak();

                    //if we are in warp, do nothing, as nothing can actually be done until we are out of warp anyway.
                    if (ESCache.Instance.InWarp)
                    {
                        if (DebugConfig.DebugTraveler) Log.WriteLine("TravelerState.Traveling: if (ESCache.Instance.InWarp) return");
                        Time.Instance.NextTravelerAction = DateTime.UtcNow;
                        return;
                    }

                    if (Destination == null)
                    {
                        Log.WriteLine("TravelerState.Traveling: if (Destination == null) State.CurrentTravelerState = TravelerState.Error;");
                        ChangeTravelerState(TravelerState.Error);
                        break;
                    }

                    if (Destination.SolarSystemId != ESCache.Instance.DirectEve.Session.SolarSystemId)
                    {
                        if (DebugConfig.DebugTraveler) Log.WriteLine("TravelerState.Traveling: NavigateToBookmarkSystem(Destination.SolarSystemId);");
                        NavigateToBookmarkSystem(Destination.SolarSystemId);
                        Time.Instance.NextTravelerAction = DateTime.UtcNow;
                    }
                    else if (Destination.PerformFinalDestinationTask())
                    {
                        _destinationRoute = null;
                        _location = null;
                        _locationName = string.Empty;
                        _locationErrors = 0;
                        ChangeTravelerState(TravelerState.AtDestination);
                    }

                    //Time.Instance.NextTravelerAction = DateTime.UtcNow.AddSeconds(4);
                    break;

                case TravelerState.AtDestination:

                    //do nothing when at destination
                    //Traveler sits in AtDestination when it has nothing to do, NOT in idle.
                    break;

                default:
                    break;
            }
        }

        internal static void TravelerSetAtDestination()
        {
            _destination = null;
            State.CurrentTravelerState = TravelerState.AtDestination;
        }

        /// <summary>
        ///     Set destination to a solar system
        /// </summary>
        public static bool SetStationDestination(long stationId)
        {
            NavigateOnGrid.StationIdToGoto = stationId;
            _location = ESCache.Instance.DirectEve.Navigation.GetLocation(stationId);
            if (DebugConfig.DebugTraveler)
                Log.WriteLine("Location = [" + _location.Name + "]");
            if (_location != null && _location.IsValid)
            {
                _locationErrors = 0;
                if (DebugConfig.DebugTraveler)
                    Log.WriteLine("Setting destination to [" + _location.Name + "]");
                try
                {
                    _location.SetDestination();
                }
                catch (Exception)
                {
                    Log.WriteLine("Set destination to [" + _location + "] failed ");
                }

                return true;
            }

            Log.WriteLine("Error setting station destination [" + stationId + "]");
            _locationErrors++;
            if (_locationErrors > 20)
                return false;
            return false;
        }

        public static void TravelHome(DirectAgent myAgent)
        {
            TravelToAgentsStation(myAgent);
        }

        public static void TravelToAgentsStation(DirectAgent myAgent)
        {
            try
            {
                if (DebugConfig.DebugGotobase)
                {
                    Log.WriteLine("TravelToAgentsStation: myAgent.StationId [" + myAgent.StationId + "]");
                    Log.WriteLine("TravelToAgentsStation: myAgent.SolarSystemId [" + myAgent.SolarSystemId + "]");
                }

                if (myAgent.StationId > 0)
                {
                    if (DebugConfig.DebugGotobase) Log.WriteLine("TravelToAgentsStation: if (DestinationId > 0)");

                    TravelToStationId(myAgent.StationId);
                    return;
                }

                Log.WriteLine("DestinationId [" + myAgent.StationId + "]");
            }
            catch (Exception)
            {
            }
        }

        public static void TravelToBookmark(DirectBookmark bookmark)
        {
            try
            {
                if (DebugConfig.DebugGotobase || DebugConfig.DebugTraveler) Log.WriteLine("bookmark [" + bookmark.Title + "]");
                if (bookmark != null)
                {
                    if (Destination == null)
                    {
                        Log.WriteLine("Bookmark title [" + bookmark.Title + "] Setting Destination");
                        Destination = new BookmarkDestination(bookmark);
                        ChangeTravelerState(TravelerState.Idle);
                        return;
                    }

                    if (DebugConfig.DebugGotobase || DebugConfig.DebugTraveler)
                        Log.WriteLine("TravelToBookmark: if (Destination != null)");

                    _processAtDestinationActions();
                    ProcessState();
                    return;
                }

                Log.WriteLine("Traveler.SolarSystemId [" + Destination.SolarSystemId + "]");
            }
            catch (Exception ex)
            {
                Log.WriteLine("Exception [" + ex + "]");
            }
        }

        public static bool TravelToBookmarkName(string bookmarkName)
        {
            bool travel = false;

            if (ESCache.Instance.BookmarksByLabel(bookmarkName).Count > 0)
            {
                myHomeBookmarks = ESCache.Instance.BookmarksByLabel(bookmarkName).OrderBy(i => Guid.NewGuid()).ToList();

                if (myHomeBookmarks != null && myHomeBookmarks.Any())
                {
                    if (MyTravelToBookmark == null) MyTravelToBookmark = myHomeBookmarks.Where(b =>  b.IsInCurrentSystem).OrderBy(i => Guid.NewGuid()).FirstOrDefault();
                    if (MyTravelToBookmark == null && !ESCache.Instance.DirectEve.Session.IsWspace) MyTravelToBookmark = myHomeBookmarks.OrderBy(i => i.SolarSystem.JumpsHighSecOnly).FirstOrDefault();
                    if (MyTravelToBookmark != null && MyTravelToBookmark.LocationId != null)
                    {
                        TravelToBookmark(MyTravelToBookmark);
                        travel = true;
                        return travel;
                    }
                }

                Log.WriteLine("bookmark not found! We were Looking for bookmark starting with [" + bookmarkName + "] found none.");
            }

            Log.WriteLine("bookmark not found! We were Looking for bookmark starting with [" + bookmarkName + "] found none");
            return travel;
        }

        public static bool TravelToMissionBookmark(DirectAgentMission myMission, string title)
        {
            try
            {
                if (myMission == null)
                {
                    Log.WriteLine("TravelToMissionBookmark: myMission is null");
                    return false;
                }

                if (myMission.State == MissionState.Offered && (myMission.Bookmarks == null || myMission.Bookmarks.Count == 0))
                {
                    ResetStatesToDefaults(myMission);
                    Log.WriteLine("TravelToMissionBookmark [" + myMission.Name + "] is int State [" + myMission.State + "] and should not yet have any bookmarks: logic error?");
                    //ESCache.Instance.CloseQuestor("No Bookmarks found for mission [" + myMission.Name + "] recycling the eve client");
                    CombatMissionsBehavior.ChangeCombatMissionBehaviorState(CombatMissionsBehaviorState.GotoBase, true, null);
                    return false;
                }

                if (myMission.Bookmarks == null || myMission.Bookmarks.Count == 0)
                {
                    ResetStatesToDefaults(myMission);
                    Log.WriteLine("TravelToMissionBookmark [" + myMission.Name + "] is int State [" + myMission.State + "] does notyet have any bookmarks and probably should");
                    //ESCache.Instance.CloseQuestor("No Bookmarks found for mission [" + myMission.Name + "] recycling the eve client");
                    return false;
                }

                if (!myMission.Bookmarks.Any(i => i.Title.Contains(title)))
                {
                    ResetStatesToDefaults(myMission);
                    Log.WriteLine("The mission [" + myMission.Name + "] does not yet have any bookmarks containing [" + title + "]: waiting");
                    return false;
                }

                MissionBookmarkDestination missionDestination = Destination as MissionBookmarkDestination;
                if (Destination == null || missionDestination == null || missionDestination.AgentId != myMission.AgentId || !missionDestination.Title.ToLower().StartsWith(title.ToLower()))
                {
                    DirectAgentMissionBookmark tempAgentMissionBookmark = myMission.Bookmarks.Find(i => i.Title.Contains(title));
                    if (tempAgentMissionBookmark != null)
                    {
                        Log.WriteLine("TravelToMissionBookmark: Setting Destination to [" + tempAgentMissionBookmark.Title + "]");
                        Destination = new MissionBookmarkDestination(tempAgentMissionBookmark);
                    }
                }

                ProcessState();

                if (State.CurrentTravelerState == TravelerState.AtDestination)
                {
                    if (missionDestination != null)
                    {
                        Log.WriteLine("Arrived at RegularMission Bookmark Destination [ " + missionDestination.Title + " ]");
                        Destination = null;
                        missionDestination = null;
                        return true;
                    }

                    Log.WriteLine("destination is null"); //how would this occur exactly?
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                Log.WriteLine("Exception [" + exception + "]");
                return false;
            }
        }

        public static void TravelToSetWaypoint()
        {
            try
            {
                List<long> path = ESCache.Instance.DirectEve.Navigation.GetDestinationPath();
                path.RemoveAll(i => i == 0);
                if (path.Count == 0)
                {
                    if (DirectEve.Interval(1000)) Log.WriteLine("No path set.");
                    ControllerManager.Instance.GetController<ActionQueueController>().RemoveAllActions();
                    Traveler.Destination = null;
                    State.CurrentTravelerState = TravelerState.AtDestination;
                    return;
                }

                long dest = path.Last();
                DirectLocation location = ESCache.Instance.DirectEve.Navigation.GetLocation(dest);
                bool isStationLocation = location.ItemId.HasValue && ESCache.Instance.DirectEve.Stations.TryGetValue((int)location.ItemId.Value, out var _);

                if (!location.SolarSystemId.HasValue)
                {
                    Log.WriteLine("Location has no solarsystem id.");
                    return;
                }

                if (DebugConfig.DebugTraveler) Log.WriteLine($"Location SolarSystemId {location.SolarSystemId} isStationLocation {isStationLocation}");

                // if we can't warp because we are scrambled, prevent next actions
                if (ESCache.Instance.EntitiesOnGrid.Any(i => i.IsWarpScramblingMe))
                    return;

                if (_destination == null)
                {
                    if (isStationLocation)
                    {
                        _destination = new StationDestination(location.ItemId.Value);
                        ESCache.Instance.TaskSetEveAccountAttribute(nameof(EveAccount.TravelerDestinationSystemId), Destination.SolarSystemId);
                    }
                    else
                    {
                        _destination = new SolarSystemDestination(location.SolarSystemId.Value);
                        ESCache.Instance.TaskSetEveAccountAttribute(nameof(EveAccount.TravelerDestinationSystemId), Destination.SolarSystemId);
                    }
                    ChangeTravelerState(TravelerState.Idle);
                    return;
                }

                _processAtDestinationActions();
                ProcessState();
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.ToString());
            }
        }

        public static void TravelToStationId(long destinationId)
        {
            try
            {
                try
                {
                    if (Destination == null)
                    {
                        Log.WriteLine("StationDestination: [" + destinationId + "]");
                        if (destinationId > 0)
                        {
                            Destination = new StationDestination(destinationId);
                            ChangeTravelerState(TravelerState.Idle);
                            return;
                        }

                        return;
                    }

                    try
                    {
                        if (((StationDestination)Destination).StationId != destinationId)
                        {
                            Log.WriteLine("StationDestination: [" + destinationId + "]");
                            if (destinationId > 0)
                            {
                                Destination = new StationDestination(destinationId);
                                ChangeTravelerState(TravelerState.Idle);
                                return;
                            }

                            return;
                        }
                    }
                    catch (Exception)
                    {
                        _destination = null;
                        ESCache.Instance.TaskSetEveAccountAttribute(nameof(EveAccount.TravelerDestinationSystemId), 0);
                    }
                }
                catch (Exception ex)
                {
                    _destination = null;
                    ESCache.Instance.TaskSetEveAccountAttribute(nameof(EveAccount.TravelerDestinationSystemId), 0);
                    Log.WriteLine("Exception [" + ex + "]");
                }

                if (DebugConfig.DebugGotobase)
                    if (Destination != null)
                        Log.WriteLine("Traveler.Destination.SolarSystemId [" + Destination.SolarSystemId + "]");
                if (DebugConfig.DebugGotobase) Log.WriteLine("_processAtDestinationActions();");
                _processAtDestinationActions();
                if (DebugConfig.DebugGotobase) Log.WriteLine("Entering Traveler.ProcessState();");
                ProcessState();
                if (DebugConfig.DebugGotobase) Log.WriteLine("Exiting Traveler.ProcessState();");
            }
            catch (Exception ex)
            {
                _destination = null;
                ESCache.Instance.TaskSetEveAccountAttribute(nameof(EveAccount.TravelerDestinationSystemId), 0);
                Log.WriteLine("Exception [" + ex + "]");
            }
        }

        public static void TravelToSystemId(long destinationId)
        {
            try
            {
                using (ProcessLock pLock = (ProcessLock)CrossProcessLockFactory.CreateCrossProcessLock(1, "TravelToSystemId"))
                {
                    if (_destination == null || ((SolarSystemDestination)_destination != null && ((SolarSystemDestination)_destination).SolarSystemId != destinationId))
                    {
                        Log.WriteLine("SolarSystemDestination: [" + destinationId + "]");
                        _destination = new SolarSystemDestination(destinationId);
                        ESCache.Instance.TaskSetEveAccountAttribute(nameof(EveAccount.TravelerDestinationSystemId), Destination.SolarSystemId);
                        ChangeTravelerState(TravelerState.Idle);
                        return;
                    }

                    if (DebugConfig.DebugGotobase)
                        if (Destination != null)
                            Log.WriteLine("Traveler.Destination.SolarSystemDestination [" + Destination.SolarSystemId + "]");
                    if (DebugConfig.DebugGotobase) Log.WriteLine("_processAtDestinationActions();");
                    _processAtDestinationActions();
                    if (DebugConfig.DebugGotobase) Log.WriteLine("Entering Traveler.ProcessState();");
                    ProcessState();
                    if (DebugConfig.DebugGotobase) Log.WriteLine("Exiting Traveler.ProcessState();");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine("Exception [" + ex + "]");
            }
        }

        private static void _processAtDestinationActions()
        {
            if (State.CurrentTravelerState == TravelerState.AtDestination)
            {
                if (State.CurrentCombatMissionCtrlState == ActionControlState.Error)
                {
                    Log.WriteLine("an error has occurred");
                    if (State.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Traveler)
                        State.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;

                    return;
                }

                if (ESCache.Instance.InSpace)
                {
                    if (State.CurrentHydraState == HydraState.Combat)
                    {
                        Log.WriteLine("Arrived at destination (in space)");
                        return;
                    }

                    Log.WriteLine("Arrived at destination (in space, Questor stopped)");
                    ControllerManager.Instance.SetPause(true);
                    return;
                }

                if (DebugConfig.DebugTraveler) Log.WriteLine("Arrived at destination...");
                if (State.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Traveler)
                {
                    if (State.CurrentCombatMissionBehaviorState != CombatMissionsBehaviorState.Storyline && State.CurrentCombatMissionBehaviorState != CombatMissionsBehaviorState.StorylineReturnToBase)
                        State.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                }
            }
        }

        public static ModuleCache Cloak
        {
            get
            {
                if (ESCache.Instance.Modules.Count > 0 && ESCache.Instance.Modules.Any(i => i.TypeId == (int)TypeID.CovertOpsCloakingDevice))
                {
                    return ESCache.Instance.Modules.Find(i => i.TypeId == (int)TypeID.CovertOpsCloakingDevice);
                }

                return null;
            }
        }

        public static bool BoolRunEveryFrame = false;

        private static void DeactivateCloak()
        {
            var cloak = ESCache.Instance.Modules.FirstOrDefault(m => m.TypeId == 11578);
            if (cloak != null && cloak.IsActive)
            {
                if (cloak.Click())
                {
                    Log.WriteLine($"Deactivating cloak.");
                }
            }
        }

        private static bool ReadyToCloak()
        {
            if (ESCache.Instance.DirectEve.Me.IsJumpCloakActive)
            {
                if (!Defense.CovertOpsCloak.IsInLimboState)
                {
                    if (!Defense.CovertOpsCloak.IsActive)
                    {
                        return true;
                    }

                    return false;
                }

                return false;
            }

            return false;
        }

        /// <summary>
        ///     Navigate to a solar system
        /// </summary>
        /// <param name="solarSystemId"></param>
        private static void NavigateToBookmarkSystem(long solarSystemId)
        {
            //if (Time.Instance.NextTravelerAction > DateTime.UtcNow)
            //{
            //    if (DebugConfig.DebugTraveler)
            //        Log.WriteLine("will continue in [ " + Math.Round(Time.Instance.NextTravelerAction.Subtract(DateTime.UtcNow).TotalSeconds, 0) + " ]sec");
            //    return;
            //}

            //Time.Instance.NextTravelerAction = DateTime.UtcNow.AddSeconds(2);

            if (DebugConfig.DebugTraveler) Log.WriteLine("NavigateToBookmarkSystem: [" + DateTime.UtcNow.ToShortTimeString() + "]");

            _destinationRoute = null;
            _destinationRoute = ESCache.Instance.DirectEve.Navigation.GetDestinationPath();

            if (_destinationRoute == null || _destinationRoute.Count == 0 || _destinationRoute.All(d => d != solarSystemId))
            {
                if (DateTime.UtcNow < _nextGetLocation)
                    if (_destinationRoute != null || (_destinationRoute != null && _destinationRoute.Count == 0))
                        Log.WriteLine("We have no destination");
                    else if (_destinationRoute != null || (_destinationRoute != null && _destinationRoute.All(d => d != solarSystemId)))
                        Log.WriteLine("The destination is not currently set to solarsystemId [" + solarSystemId + "]");

                // We do not have the destination set
                if (DateTime.UtcNow > _nextGetLocation || _location == null)
                {
                    Log.WriteLine("Getting Location of solarSystemId [" + solarSystemId + "]");
                    _nextGetLocation = DateTime.UtcNow.AddSeconds(10);
                    _location = ESCache.Instance.DirectEve.Navigation.GetLocation(solarSystemId);
                    ChangeTravelerState(State.CurrentTravelerState);
                    return;
                }

                if (_location != null && _location.IsValid && _location.CanGetThereFromHereViaTraveler)
                {
                    _locationErrors = 0;
                    Log.WriteLine("Setting destination to [" + _location.Name + "]");
                    try
                    {
                        _location.SetDestination();
                    }
                    catch (Exception)
                    {
                        Log.WriteLine("Set destination to [" + _location + "] failed ");
                    }

                    ChangeTravelerState(State.CurrentTravelerState);
                    return;
                }

                Log.WriteLine("Error setting solar system destination [" + solarSystemId + "] CanGetThereFromHereViaTraveler [" + _location.CanGetThereFromHereViaTraveler + "]");
                _locationErrors++;
                if (_locationErrors > 20)
                {
                    State.CurrentTravelerState = TravelerState.Error;
                    return;
                }

                return;
            }

            _locationErrors = 0;
            if (!ESCache.Instance.InSpace)
            {
                if (ESCache.Instance.InStation)
                    if (TravelerDestination.Undock())
                        Time.Instance.NextActivateModules = DateTime.UtcNow.AddMilliseconds(ESCache.Instance.RandomNumber(4000, 6000));

                // We are not yet in space, wait for it
                return;
            }

            TravelerDestination.UndockAttempts = 0;
            //if (Logging.DebugTraveler) Logging.Log("Traveler", "Destination is set: processing...");

            //
            // Check if we are docking and if so use (or make!) dock bookmark as needed
            //
            if (Settings.Instance.UseDockBookmarks)
                if (State.CurrentInstaStationDockState != InstaStationDockState.Done &&
                    State.CurrentInstaStationDockState != InstaStationDockState.WaitForTraveler)
                {
                    InstaStationDock.ProcessState();
                    if (State.CurrentInstaStationDockState != InstaStationDockState.Done &&
                        State.CurrentInstaStationDockState != InstaStationDockState.WaitForTraveler)
                        return;
                }

            //
            // Check if we are undocking and if so use (or make!) undock bookmark as needed
            //
            if (Settings.Instance.UseUndockBookmarks)
                if (State.CurrentInstaStationUndockState != InstaStationUndockState.Done &&
                    State.CurrentInstaStationUndockState != InstaStationUndockState.WaitForTraveler)
                {
                    InstaStationUnDock.ProcessState();
                    if (State.CurrentInstaStationUndockState != InstaStationUndockState.Done &&
                        State.CurrentInstaStationUndockState != InstaStationUndockState.WaitForTraveler)
                        return;
                }

            // Find the first waypoint
            long waypoint = _destinationRoute.FirstOrDefault();

            //if (Logging.DebugTraveler) Logging.Log("Traveler", "NavigateToBookmarkSystem: getting next way-points locationName");
            _locationName = ESCache.Instance.DirectEve.Navigation.GetLocationName(waypoint);
            if (DebugConfig.DebugTraveler)
                Log.WriteLine("Next Waypoint is: [" + _locationName + "]!");

            if (waypoint > 60000000) // this MUST be a station
            {
                //insert code to handle station destinations here
            }

            if (waypoint < 60000000) // this is not a station, probably a system
            {
                //useful?a
            }

            DirectSolarSystem solarSystemInRoute = ESCache.Instance.DirectEve.SolarSystems[(int)waypoint];

            if (State.CurrentQuestorState == QuestorState.CombatMissionsBehavior)
                if (solarSystemInRoute != null && !solarSystemInRoute.IsHighSecuritySpace && ESCache.Instance.ActiveShip.GroupId != (int)Group.AssaultShip && ESCache.Instance.ActiveShip.GroupId != (int)Group.Shuttle && ESCache.Instance.ActiveShip.GroupId != (int)Group.Frigate && ESCache.Instance.ActiveShip.GroupId != (int)Group.Interceptor && ESCache.Instance.ActiveShip.GroupId != (int)Group.TransportShip && ESCache.Instance.ActiveShip.GroupId != (int)Group.ForceReconShip && ESCache.Instance.ActiveShip.GroupId != (int)Group.StealthBomber && !MissionSettings.AllowNonStorylineCourierMissionsInLowSec)
                {
                    Log.WriteLine("Next Waypoint is: [" + _locationName +
                                  "] which is LOW SEC! This should never happen. Turning off AutoStart and going home. PauseAfterNextDock [true]");
                    ESCache.Instance.PauseAfterNextDock = true;
                    ESCache.Instance.DeactivateScheduleAndCloseAfterNextDock = true;
                    if (State.CurrentQuestorState == QuestorState.CombatMissionsBehavior)
                        State.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                    return;
                }
            // Find the stargate associated with it

            if (ESCache.Instance.Stargates.Count ==0)
            {
                // not found, that cant be true?!?!?!?!
                Log.WriteLine("Error [" + _locationName + "] not found, most likely lag waiting [" + Time.Instance.TravelerNoStargatesFoundRetryDelay_seconds +
                              "] seconds.");
                return;
            }

            // Warp to, approach or jump the stargate
            EntityCache MyNextStargate = ESCache.Instance.StargateByName(_locationName);
            if (MyNextStargate != null)
            {
                if (ESCache.Instance.ActiveShip.Entity.IsCloaked)
                {
                    if (MyNextStargate.Distance < (int)Distances.GateActivationRangeWhileCloaked)
                    {
                        if (ESCache.Instance.InWarp || ESCache.Instance.MyShipEntity.HasInitiatedWarp) return;

                        //bool BoolShipHasCovOpsCloak = ESCache.Instance.Modules.FirstOrDefault(i => i.TypeId == (int)TypeID.CovertOpsCloakingDevice || ESCache.Instance.Modules.Any(i => i.TypeId == 20563)).IsOnline;

                        if (MyNextStargate.Jump())
                        {
                            Log.WriteLine("Jumping to [" + _locationName + "]");
                            //if (BoolShipHasCovOpsCloak)
                            //{
                            //    Log.WriteLine("Covert ops cloak found, adding cloak action.");
                            //    AddActivateCovertOpsCloakAfterJumpAction();
                            //}
                            return;
                        }

                        return;
                    }
                }
                else if (MyNextStargate.Distance < (int)Distances.JumpRange)
                {
                    if (ESCache.Instance.InWarp || ESCache.Instance.MyShipEntity.HasInitiatedWarp) return;

                    //bool BoolShipHasCovOpsCloak = ESCache.Instance.Modules.FirstOrDefault(i => i.TypeId == (int)TypeID.CovertOpsCloakingDevice || ESCache.Instance.Modules.Any(i => i.TypeId == 20563)).IsOnline;

                    if (MyNextStargate.Jump())
                    {
                        Log.WriteLine("Jumping to [" + _locationName + "]");
                        //if (BoolShipHasCovOpsCloak)
                        //{
                        //    Log.WriteLine("Covert ops cloak found, adding cloak action.");
                        //    AddActivateCovertOpsCloakAfterJumpAction();
                        //}
                        return;
                    }

                    return;
                }

                if (MyNextStargate.Distance != 0)
                {
                    if (DebugConfig.DebugTraveler) Log.WriteLine("if (NavigateOnGrid.NavigateToTarget(" + MyNextStargate.Name + ", Traveler, false, 0)) return;");
                    if (NavigateOnGrid.NavigateToTarget(MyNextStargate, 0)) return;
                }
            }
        }

        private static bool ResetStatesToDefaults(DirectAgentMission myMission)
        {
            Log.WriteLine("Traveler: ResetStatesToDefaults");
            if (State.CurrentStorylineState != StorylineState.PreAcceptMission)
                if (myMission != null && !myMission.Name.Contains("Materials for War"))
                {
                    //CombatMissionsBehavior.ChangeCombatMissionBehaviorState(CombatMissionsBehaviorState.Idle);
                }

            _destination = null;
            ESCache.Instance.TaskSetEveAccountAttribute(nameof(EveAccount.TravelerDestinationSystemId), 0);
            State.CurrentAgentInteractionState = AgentInteractionState.Idle;
            State.CurrentArmState = ArmState.Idle;
            State.CurrentUnloadLootState = UnloadLootState.Idle;
            if (ESCache.Instance.InStation)
            {
                if (State.CurrentCombatMissionBehaviorState != CombatMissionsBehaviorState.Storyline &&
                    State.CurrentCombatMissionBehaviorState != CombatMissionsBehaviorState.CourierMission)
                {
                    CombatMissionsBehavior.ChangeCombatMissionBehaviorState(CombatMissionsBehaviorState.GotoBase, true, null);
                }
            }
            //State.CurrentTravelerState = TravelerState.AtDestination;
            NavigateOnGrid.Reset();
            return true;
        }

        #endregion Methods
    }
}