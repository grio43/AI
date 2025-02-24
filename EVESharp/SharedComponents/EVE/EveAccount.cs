/*
/*
 * ---------------------------------------
 * User: duketwo
 * Date: 24.12.2013
 * Time: 16:26
 *
 * ---------------------------------------
 */

using EasyHook;
using Newtonsoft.Json;
//using ServiceStack;
//using ServiceStack.Text;
using SharedComponents.CurlUtil;
using SharedComponents.Events;
using SharedComponents.Extensions;
using SharedComponents.IPC;
using SharedComponents.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;
using System.Diagnostics.Tracing;

namespace SharedComponents.EVE
{
    public static class EveAccountStartPriority
    {
        public const float ReQueueAfterFailurePriority = LowPriority;
        public const float NumberOnePriority = 1;
        public const float ManuallyStartedPriority = 2;
        public const float InCapsulePriority = 500;
        public const float LowPriority = 300;
        public const float NormalPriority = 200;
        public const float DockedAndAloneInLocalPriority = 185;
        public const float HighPriority = 175;
        public const float InMissionPriority = 150;
        public const float InAbyssalDeadspacePriority = 125;
    }

    /// <summary>
    ///     Description of EveAccountData.
    /// </summary>
    [Serializable]
    public partial class EveAccount : ViewModelBase //INotifyPropertyChanged, IDisposable, ILaunchTarget
    {
        #region Fields

        public readonly int MAX_MEMORY = 3500;

        public readonly int MAX_STARTS = 20;

        #endregion Fields

        #region Methods

        /*
        private void Wait(int milliseconds)
        {
            //Timer timer1 = new Timer();
            if (milliseconds == 0 || milliseconds < 0) return;
            timer1.Interval = milliseconds;
            timer1.Enabled = true;
            timer1.Start();
            timer1.Tick += (s, e) =>
            {
                timer1.Enabled = false;
                timer1.Stop();
            };
            while (timer1.Enabled)
            {
                Application.DoEvents();
            }
        }
        */

        #endregion Methods

        #region Fields

        private static List<string> _listOfAvailableControllers;
        public static List<string> ListOfAvailableControllers
        {
            get
            {
                if (_listOfAvailableControllers != null)
                    return _listOfAvailableControllers;

                _listOfAvailableControllers = Enum.GetValues(typeof(AvailableControllers)).Cast<AvailableControllers>().Select(v => v.ToString()).ToList();
                return _listOfAvailableControllers;
            }
        }

        public enum AvailableControllers
        {
            DecideWhatToDoController,
            AbyssalController,
            AbyssalDeadspaceController,
            CareerAgentController,
            CombatDontMoveController,
            CourierMissionsController,
            DeepFlowSignaturesController,
            ExplorationNoWeaponsController,
            FindGuriastasHuntPodsController,
            GatherItemsController,
            GatherShipsController,
            //FleetAbyssalDeadspaceController,
            HighSecAnomalyController,
            HighSecCombatSignaturesController,
            HomeFrontController,
            HydraController,
            IndustryController,
            InsuranceFraudController,
            MarketAdjustController,
            MarketAdjustExistingOrdersController,
            MiningController,
            MonitorGridController,
            None,
            PinataController,
            QuestorController,
            SalvageGridController,
            SortBlueprintsController,
            StandingsGrindController,
            Test,
            TransportItemTypesController,
            WormHoleAnomalyController,
            WSpaceScoutController,
            WspaceSiteController,
        }

        [NonSerialized] public static ConcurrentDictionary<int, string> GUIDByPid = new ConcurrentDictionary<int, string>();
        [NonSerialized] public static ConcurrentDictionary<int, string> CharnameByPid = new ConcurrentDictionary<int, string>();

        [NonSerialized] public bool DoNotSessionChange = false;

        [Browsable(true)]
        [NonSerialized] public bool ManuallyStarted = false;

        [NonSerialized] public DateTime LastEveClientLaunched = DateTime.UtcNow.AddDays(-1);
        [NonSerialized] public DateTime LastEveClientPulledFromQueue = DateTime.UtcNow.AddDays(-1);

        [NonSerialized] private static readonly Random rnd = new Random();

        [NonSerialized] private static DateTime lastEveInstanceKilled = DateTime.MinValue;

        [NonSerialized] private static int waitTimeBetweenEveInstancesKills = rnd.Next(15, 25);

        [NonSerialized] private DateTime? _lastResponding;

        [NonSerialized] private DateTime _nextAtWarLogMessage = DateTime.MinValue;

        [NonSerialized] private DateTime _nextOmegaCloneLogMessage = DateTime.MinValue;

        [NonSerialized] private IDuplexServiceCallback ClientCallback;

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public double MAX_SERIALIZATION_ERRORS
        {
            get
            {
                try
                {
                    var tempMAX_SERIALIZATION_ERRORS = GetValue(() => MAX_SERIALIZATION_ERRORS);
                    if (tempMAX_SERIALIZATION_ERRORS <= 0)
                    {
                        return (3 * 4) + 1.5d;
                    }

                    return tempMAX_SERIALIZATION_ERRORS;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            set { SetValue(() => MAX_SERIALIZATION_ERRORS, value); }
        }

        #endregion Fields

        #region Constructors

        public EveAccount(string accountName, string characterName, string password, DateTime startTime, DateTime endTime, string emailAddress, string emailPassword, int hoursPerDay = 4)
        {
            AccountName = accountName;
            AutoSkillTraining = true;
            Console = false;
            CharacterName = characterName;
            Email = emailAddress;
            EmailPassword = emailPassword;
            Password = password;
            //StartTime = startTime;
            //EndTime = endTime;
            HoursPerDay = hoursPerDay;
            RequireOmegaClone = false;
            SelectedController = nameof(AvailableControllers.CombatDontMoveController);
            StartsPast24H = 0;
            LastEveClientLaunched = DateTime.MinValue;
            UseScheduler = true;
            //Pid = 0;
        }

        public EveAccount()
        {
            if (string.IsNullOrEmpty(GUID))
            {
                GUID = Guid.NewGuid().ToString();
            }

            if (5 > NumOfAbyssalSitesBeforeRestarting)
                NumOfAbyssalSitesBeforeRestarting = 20;
        }

        /**
        public DriverHandler GetDriverHandler(bool headless = false, bool newInstance = false)
        {

            if (newInstance)
                return new DriverHandler(this, headless);

            if (_driverHandler != null && !_driverHandler.IsAlive())
            {
                _driverHandler.Close();
                _driverHandler = null;
            }


            if (_driverHandler == null)
            {
                try
                {
                    _driverHandler = new DriverHandler(this, headless);

                }
                catch (Exception ex)
                {

                    Cache.Instance.Log(ex.StackTrace);
                }
            }


            return _driverHandler;
        }

        public void DisposeDriverHandler()
        {
            try
            {
                if (_driverHandler != null)
                {
                    _driverHandler.Dispose();
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log(ex.ToString());
            }
        }
        **/

        #endregion Constructors

        #region Properties


        [Browsable(false)]
        public string VerificationCode = string.Empty;

        public static string DefaultController => nameof(AvailableControllers.CombatDontMoveController);

        // --------- Pattern Manager Settings Start

        [Browsable(false)]
        public DateTime LastLoginRewardClaim
        {
            get { return GetValue(() => LastLoginRewardClaim); }
            set { SetValue(() => LastLoginRewardClaim, value); }
        }

        [Browsable(false)]
        public DateTime LastRewardRedeem
        {
            get { return GetValue(() => LastRewardRedeem); }
            set { SetValue(() => LastRewardRedeem, value); }
        }

        [Browsable(true)]
        public bool PatternManagerEnabled
        {
            get { return GetValue(() => PatternManagerEnabled); }
            set { SetValue(() => PatternManagerEnabled, value); }
        }

        [Browsable(false)]
        public Guid? StartOnVirtualDesktopId { get; set; }
        [Browsable(true)]
        public int PatternManagerHoursPerWeek
        {
            get { return GetValue(() => PatternManagerHoursPerWeek); }
            set { SetValue(() => PatternManagerHoursPerWeek, value); }
        }

        [Browsable(true)]
        public int PatternManagerDaysOffPerWeek
        {
            get { return GetValue(() => PatternManagerDaysOffPerWeek); }
            set { SetValue(() => PatternManagerDaysOffPerWeek, value); }
        }

        [Browsable(true)]
        public List<int> PatternManagerExcludedHours
        {
            get { return GetValue(() => PatternManagerExcludedHours); }
            set { SetValue(() => PatternManagerExcludedHours, value); }
        }

        [Browsable(true)]
        public DateTime PatternManagerLastUpdate
        {
            get { return GetValue(() => PatternManagerLastUpdate); }
            set { SetValue(() => PatternManagerLastUpdate, value); }
        }

        // --------- Pattern Manager Settings End

        [Browsable(false)]
        [ReadOnly(true)]
        public int AbyssalPocketNumber
        {
            get { return GetValue(() => AbyssalPocketNumber); }
            set { SetValue(() => AbyssalPocketNumber, value); }
        }

        public string AccountName
        {
            get { return GetValue(() => AccountName); }
            set
            {
                if (Cache.IsServer && Pid != 0)
                {
                    Cache.Instance.Log("Can't change the account name until the client has been stopped.");
                    return;
                }

                //if (Cache.IsServer && Cache.Instance.GetXMLFilesLoaded)
                //{
                DeleteCurlCookie();
                //}

                SetValue(() => AccountName, value);
                //if (Cache.IsServer && Cache.Instance.GetXMLFilesLoaded)
                //{
                UniqueID = string.Empty;
                ClearEveAccessToken();
                ClearRefreshToken();
                //}
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public string MaskedAccountName
        {
            get
            {
                if (!string.IsNullOrEmpty(AccountName))
                {
                    return AccountName.Substring(0, 3) + "-MaskedAccountName-";
                }

                return string.Empty;
            }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public string Agent
        {
            get { return GetValue(() => Agent); }
            set { SetValue(() => Agent, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public string AgentCorp
        {
            get { return GetValue(() => AgentCorp); }
            set { SetValue(() => AgentCorp, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public string AgentCorpId
        {
            get { return GetValue(() => AgentCorpId); }
            set { SetValue(() => AgentCorpId, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public float AgentCorpStandings
        {
            get { return GetValue(() => AgentCorpStandings); }
            set { SetValue(() => AgentCorpStandings, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public string AgentDivision
        {
            get { return GetValue(() => AgentDivision); }
            set { SetValue(() => AgentDivision, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public string AgentFaction
        {
            get { return GetValue(() => AgentFaction); }
            set { SetValue(() => AgentFaction, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public string AgentFactionId
        {
            get { return GetValue(() => AgentFactionId); }
            set { SetValue(() => AgentFactionId, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public float AgentFactionStandings
        {
            get { return GetValue(() => AgentFactionStandings); }
            set { SetValue(() => AgentFactionStandings, value); }
        }

        [ReadOnly(true)]
        public string AgentLevel
        {
            get { return GetValue(() => AgentLevel); }
            set { SetValue(() => AgentLevel, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public float AgentStandings
        {
            get { return GetValue(() => AgentStandings); }
            set { SetValue(() => AgentStandings, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime AggressingTargetDate
        {
            get { return GetValue(() => AggressingTargetDate); }
            set { SetValue(() => AggressingTargetDate, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long AggressingTargetId
        {
            get { return GetValue(() => AggressingTargetId); }
            set { SetValue(() => AggressingTargetId, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public bool AllLoggedInAccountsAreNotInLocalWhereIWillLogin
        {
            get
            {
                try
                {
                    if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(i => !string.IsNullOrEmpty(i.AccountName) && !string.IsNullOrEmpty(i.CharacterName) && i.EveProcessExists && !i.IsInAbyss && i.SolarSystem == SolarSystem))
                    {
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return false;
                }
            }
        }

        [Browsable(false)]
        public bool AllowInjectionIntoManuallyLaunchedEve
        {
            get { return GetValue(() => AllowInjectionIntoManuallyLaunchedEve); }
            set { SetValue(() => AllowInjectionIntoManuallyLaunchedEve, value); }
        }

        public bool AllowSimultaneousLogins = false;

        /**
        public double ArmorHitPoints
        {
            get { return GetValue(() => ArmorHitPoints); }
            set { SetValue(() => ArmorHitPoints, value); }
        }

        public double ArmorPct
        {
            get { return GetValue(() => ArmorPct); }
            set { SetValue(() => ArmorPct, value); }
        }
        **/

        public string AssistMyDronesTo
        {
            get { return GetValue(() => AssistMyDronesTo); }
            set { SetValue(() => AssistMyDronesTo, value); }
        }

        public bool AutoSkillTraining
        {
            get { return GetValue(() => AutoSkillTraining); }
            set { SetValue(() => AutoSkillTraining, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool BoolNewSignatureDetected
        {
            get { return GetValue(() => BoolNewSignatureDetected); }
            set { SetValue(() => BoolNewSignatureDetected, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public string BotLogpath
        {
            get
            {
                return Util.AssemblyPath + "\\Logs\\" + CharacterName + "\\";
            }
        }

        public bool BotUsesHydra
        {
            get { return GetValue(() => BotUsesHydra); }
            set { SetValue(() => BotUsesHydra, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public string BotXmlConfigFile
        {
            get
            {
                return Util.AssemblyPath + "\\QuestorSettings\\" + CharacterName + ".xml";
            }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public bool ByPassLoginScreen
        {
            get { return GetValue(() => ByPassLoginScreen); }
            set { SetValue(() => ByPassLoginScreen, value); }
        }

        /**
        public double CapacitorLevel
        {
            get { return GetValue(() => CapacitorLevel); }
            set { SetValue(() => CapacitorLevel, value); }
        }

        public double CapacitorPct
        {
            get { return GetValue(() => CapacitorPct); }
            set { SetValue(() => CapacitorPct, value); }
        }
        **/

        public bool CareerAgentAmarr1MissionsComplete
        {
            get { return GetValue(() => CareerAgentAmarr1MissionsComplete); }
            set { SetValue(() => CareerAgentAmarr1MissionsComplete, value); }
        }

        public bool CareerAgentAmarr2MissionsComplete
        {
            get { return GetValue(() => CareerAgentAmarr2MissionsComplete); }
            set { SetValue(() => CareerAgentAmarr2MissionsComplete, value); }
        }

        public bool CareerAgentAmarr3MissionsComplete
        {
            get { return GetValue(() => CareerAgentAmarr3MissionsComplete); }
            set { SetValue(() => CareerAgentAmarr3MissionsComplete, value); }
        }

        public bool CareerAgentCaldari1MissionsComplete
        {
            get { return GetValue(() => CareerAgentCaldari1MissionsComplete); }
            set { SetValue(() => CareerAgentCaldari1MissionsComplete, value); }
        }

        public bool CareerAgentCaldari2MissionsComplete
        {
            get { return GetValue(() => CareerAgentCaldari2MissionsComplete); }
            set { SetValue(() => CareerAgentCaldari2MissionsComplete, value); }
        }

        public bool CareerAgentCaldari3MissionsComplete
        {
            get { return GetValue(() => CareerAgentCaldari3MissionsComplete); }
            set { SetValue(() => CareerAgentCaldari3MissionsComplete, value); }
        }

        public bool CareerAgentGallente1MissionsComplete
        {
            get { return GetValue(() => CareerAgentGallente1MissionsComplete); }
            set { SetValue(() => CareerAgentGallente1MissionsComplete, value); }
        }

        public bool CareerAgentGallente2MissionsComplete
        {
            get { return GetValue(() => CareerAgentGallente2MissionsComplete); }
            set { SetValue(() => CareerAgentGallente2MissionsComplete, value); }
        }

        public bool CareerAgentGallente3MissionsComplete
        {
            get { return GetValue(() => CareerAgentGallente3MissionsComplete); }
            set { SetValue(() => CareerAgentGallente3MissionsComplete, value); }
        }

        public bool CareerAgentMinmatar1MissionsComplete
        {
            get { return GetValue(() => CareerAgentMinmatar1MissionsComplete); }
            set { SetValue(() => CareerAgentMinmatar1MissionsComplete, value); }
        }

        public bool CareerAgentMinmatar2MissionsComplete
        {
            get { return GetValue(() => CareerAgentMinmatar2MissionsComplete); }
            set { SetValue(() => CareerAgentMinmatar2MissionsComplete, value); }
        }

        public bool CareerAgentMinmatar3MissionsComplete
        {
            get { return GetValue(() => CareerAgentMinmatar3MissionsComplete); }
            set { SetValue(() => CareerAgentMinmatar3MissionsComplete, value); }
        }

        public string CharacterName
        {
            get { return GetValue(() => CharacterName); }
            set
            {
                if (Cache.IsServer && Pid != 0)
                {
                    Cache.Instance.Log("Can't change the character name until the client has been stopped.");
                    return;
                }
                SetValue(() => CharacterName, value);
                UniqueID = string.Empty;
            }
        }

        [Browsable(false)]
        public List<string> CharsOnAccount
        {
            get { return GetValue(() => CharsOnAccount); }
            set { SetValue(() => CharsOnAccount, value); }
        }

        public bool CheckEveServerVersion
        {
            get { return GetValue(() => CheckEveServerVersion); }
            set { SetValue(() => CheckEveServerVersion, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public double ClientSettingsSerializationErrors => StartingTokenTimespan.TotalHours;

        [XmlIgnore]
        [ReadOnly(true)]
        public string CMBCtrlAction
        {
            get { return GetValue(() => CMBCtrlAction); }
            set { SetValue(() => CMBCtrlAction, value); }
        }

        [XmlIgnore]
        public string CodeForEVESSO
        {
            get { return GetValue(() => CodeForEVESSO); }
            set { SetValue(() => CodeForEVESSO, value); }
        }

        //[Browsable(false)]
        //public ClientSetting ClientSetting
        //{
        //    get { return GetValue(() => ClientSetting); }
        //    set { SetValue(() => ClientSetting, value); }
        //}
        public bool ConnectToTestServer
        {
            get { return GetValue(() => ConnectToTestServer); }
            set { SetValue(() => ConnectToTestServer, value); }
        }

        [Browsable(true)]
        public bool Console
        {
            get { return GetValue(() => Console); }
            set
            {
                SetValue(() => Console, value);
                if (value) ShowConsoleWindow();
                else HideConsoleWindow();
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public int CourierItemTypeId
        {
            get { return GetValue(() => CourierItemTypeId); }
            set { SetValue(() => CourierItemTypeId, value); }
        }

        public bool CreateExeFileFirewallRule
        {
            get { return GetValue(() => CreateExeFileFirewallRule); }
            set { SetValue(() => CreateExeFileFirewallRule, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public string CurrentFit
        {
            get { return GetValue(() => CurrentFit); }
            set { SetValue(() => CurrentFit, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime DateTimeLastEntityIdEngaged
        {
            get { return GetValue(() => DateTimeLastEntityIdEngaged); }
            set { SetValue(() => DateTimeLastEntityIdEngaged, value); }
        }

        [Browsable(true)]
        public bool DebugScheduler
        {
            get { return GetValue(() => DebugScheduler); }
            set { SetValue(() => DebugScheduler, value); }
        }

        //[Browsable(false)]
        //public ClientSetting CS => ClientSetting;
        [Browsable(false)]
        public bool DisableHiding
        {
            get { return GetValue(() => DisableHiding); }
            set { SetValue(() => DisableHiding, value); }
        }

        [XmlIgnore]
        [Browsable(true)]
        [ReadOnly(true)]
        public bool DoneLaunchingEveInstance
        {
            get { return GetValue(() => DoneLaunchingEveInstance); }
            set { SetValue(() => DoneLaunchingEveInstance, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public int DumpLootIterations
        {
            get { return GetValue(() => DumpLootIterations); }
            set { SetValue(() => DumpLootIterations, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime DumpLootTimestamp
        {
            get { return GetValue(() => DumpLootTimestamp); }
            set { SetValue(() => DumpLootTimestamp, value); }
        }

        public string Email
        {
            get { return GetValue(() => Email); }
            set { SetValue(() => Email, value); }
        }

        public string EmailPassword
        {
            get { return GetValue(() => EmailPassword); }
            set { SetValue(() => EmailPassword, value); }
        }

        [XmlIgnore]
        public DateTime EndTime
        {
            get
            {
                try
                {
                    if (UseFleetMgr && !IsLeader && !string.IsNullOrEmpty(ChatChannelToPullFleetInvitesFrom))
                    {
                        FleetName = ChatChannelToPullFleetInvitesFrom;
                        //Copy schedule from the leader!
                        foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(i => i.GUID != GUID && i.UseFleetMgr && i.IsLeader && ChatChannelToPullFleetInvitesFrom == i.ChatChannelToPullFleetInvitesFrom))
                        {
                            return eA.EndTime; //there should be only one leader per fleet
                        }
                    }
                }
                catch (Exception) { }

                return GetValue(() => EndTime);
            }
            set { SetValue(() => EndTime, value); }
        }

        private bool IsCachedEndTimeStillValid
        {
            get
            {
                try
                {
                    if (EndTime == DateTime.MinValue)
                    {
                        if (DebugScheduler) Cache.Instance.Log("IsCachedEndTimeStillValid: [" + MaskedAccountName + "][" + MaskedCharacterName + "] if (EndTime == DateTime.MinValue) return false");
                        return false;
                    }

                    if (IsItPastEndTime)
                    {
                        if (DebugScheduler) Cache.Instance.Log("IsCachedEndTimeStillValid: [" + MaskedAccountName + "][" + MaskedCharacterName + "] return false");
                        return false;
                    }

                    if (DebugScheduler) Cache.Instance.Log("IsCachedEndTimeStillValid: if (GetStartTime > EndTime) return true");
                    return true;
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return false;
                }
            }
        }

        private void GenerateEndTime()
        {
            try
            {
                int EndMinutes = rnd.Next(5, 20);
                EndTime = StartTime.AddHours(HoursPerDay);
                EndTime = EndTime.AddMinutes(EndMinutes);

                if (Util.IntervalInMinutes(5, 5, CharacterName) || DebugScheduler)
                    Cache.Instance.Log("GetEndTime: [" + MaskedCharacterName + "] HoursPerDay [" + HoursPerDay + "] StartTime [" + StartTime.ToShortDateString() + "][" + StartTime.ToShortTimeString() + "] EndTime [" + EndTime.ToShortDateString() + "][" + EndTime.ToShortTimeString() + "].");

                //
                // should we make sure the start time is unique(ish) across toons? We can...
                //
                return;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }


        [Browsable(false)]
        [XmlIgnore]
        public DateTime GetEndTime
        {
            get
            {
                try
                {
                    if (IsCachedEndTimeStillValid)
                    {
                        if (DebugScheduler) Cache.Instance.Log("GetEndTime: if (IsCachedEndTimeStillValid)");
                        return EndTime;
                    }

                    if (!UseScheduler)
                    {
                        if (DebugScheduler) Cache.Instance.Log("GetEndTime: if (!UseScheduler)");

                        if (EndTime == DateTime.MinValue)
                            return DateTime.UtcNow.AddDays(1);

                        return EndTime;
                    }

                    if (EndTime != null) Cache.Instance.Log("EndTime [" + EndTime + "] DateTime.UtcNow [" + DateTime.UtcNow + "]");

                    if (IsItPastEndTime)
                    {
                        if (Util.IntervalInMinutes(5, 5, CharacterName) || DebugScheduler)
                        {
                            if (DebugScheduler) Cache.Instance.Log("GetEndTime: if (IsItPastEndTime)");
                        }

                        GenerateNewTimeSpans();
                    }
                    //
                    // should we make sure the start time is unique(ish) across toons? We can...
                    //
                    return EndTime;
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return DateTime.UtcNow.Date.AddDays(-1);
                }
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public List<long> EntityIdsEngagedByOthers
        {
            get
            {
                try
                {
                    List<long> tempListOfEntityIds = new List<long>();
                    foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(i => i.SolarSystem == SolarSystem && !i.IsDocked && i.Pid != 0 && i.DateTimeLastEntityIdEngaged.AddMinutes(2) > DateTime.UtcNow && i.LastEntityIdEngaged != 0 && i.LastEntityIdEngaged > 0))
                    {
                        tempListOfEntityIds.Add(eA.LastEntityIdEngaged);
                    }

                    return tempListOfEntityIds;
                }
                catch (Exception)
                {
                    return new List<long>();
                }
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public string EveClientLogpath
        {
            get
            {
                return WindowsUserProfilePath + "Documents\\EVE\\logs";
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long EveHWnd
        {
            get { return GetValue(() => EveHWnd); }
            set { SetValue(() => EveHWnd, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long EVESharpCoreFormHWnd
        {
            get { return GetValue(() => EVESharpCoreFormHWnd); }
            set { SetValue(() => EVESharpCoreFormHWnd, value); }
        }

        //
        // Good for 3 months?
        //
        [Browsable(true)]
        public string TranquilityRefreshTokenString
        {
            get { return GetValue(() => TranquilityRefreshTokenString); }
            set { SetValue(() => TranquilityRefreshTokenString, value); }
        }

        [Browsable(true)]
        public DateTime TranquilityRefreshTokenValidUntil
        {
            get { return GetValue(() => TranquilityRefreshTokenValidUntil); }
            set { SetValue(() => TranquilityRefreshTokenValidUntil, value); }
        }

        [Browsable(false)]
        public string RefreshTokenString
        {
            get
            {
                if (ConnectToTestServer)
                    return SisiRefreshTokenString;
                return TranquilityRefreshTokenString;
            }
            set
            {
                if (ConnectToTestServer)
                {
                    SisiRefreshTokenString = value;
                }

                TranquilityRefreshTokenString = value;
            }
        }

        [Browsable(false)]
        public string EveAccessTokenString
        {
            get
            {
                if (ConnectToTestServer)
                    return SisiEveAccessTokenString;
                return TranquilityEveAccessTokenString;
            }
            set
            {
                if (ConnectToTestServer)
                {
                    SisiEveAccessTokenString = value;
                }

                TranquilityEveAccessTokenString = value;
            }
        }

        [Browsable(false)]
        public DateTime EveAccessTokenValidUntil
        {
            get
            {
                if (ConnectToTestServer)
                    return SisiEveAccessTokenValidUntil;
                return TranquilityEveAccessTokenValidUntil;
            }
            set
            {
                if (ConnectToTestServer)
                {
                    SisiEveAccessTokenValidUntil = value;
                }

                TranquilityEveAccessTokenValidUntil = value;
            }
        }

        [Browsable(false)]
        public DateTime RefreshTokenValidUntil
        {
            get
            {
                if (ConnectToTestServer)
                    return SisiRefreshTokenValidUntil;
                return TranquilityRefreshTokenValidUntil;
            }
            set
            {
                if (ConnectToTestServer)
                {
                    SisiRefreshTokenValidUntil = value;
                }

                TranquilityRefreshTokenValidUntil = value;
            }
        }

        //
        // see: https://wiki.eveuniversity.org/User:Rayanth/SSO
        // or Documentation\User Rayanth_SSO - EVE University Wiki.html
        // Basic Flow:
        // 1) Request Authentication (we ask CCP to redirect us to the SSO login page and/or ask for credentials)
        //          We send a GET request similar to:
        //            https://login.eveonline.com/v2/oauth/authorize/?response_type=code&redirect_uri=https%3A%2F%2Feve.example.com%2Fredirect&client_id=1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d&scope=esi-characters.read_blueprints.v1&state=foo_bar
        //
        // 2) Authenticate:  With ESI the user enters credentials: with the launcher we pass credentials
        //
        //          We get back for ex: https://eve.example.com/redirect/?code=uHkc5DPnI0CKOxJ_ixVMpg&state=foo_bar
        //
        // 3) POST Request: Authorization: Basic: TranquilityAuthorizationString
        //          Content-Type: application/x-www-form-urlencoded
        //          grant_type: authorization_code
        //          code: The code received in Step 2
        //
        // 4) Recieve Access Token: as JSON
        //      Note: can be verified: we should probably do this before trying to authenticate!
        //      docs on verifying access token: https://docs.esi.evetech.net/docs/sso/validating_eve_jwt.html
        //
        //
        // Refresh_token!
        //

        //
        // Good for only a few hours
        //
        [Browsable(true)]
        public string TranquilityEveAccessTokenString
        {
            get { return GetValue(() => TranquilityEveAccessTokenString); }
            set { SetValue(() => TranquilityEveAccessTokenString, value); }
        }

        //
        // Good for only a few hours
        //
        [Browsable(true)]
        public DateTime TranquilityEveAccessTokenValidUntil
        {
            get { return GetValue(() => TranquilityEveAccessTokenValidUntil); }
            set { SetValue(() => TranquilityEveAccessTokenValidUntil, value); }
        }

        //
        // Good for only a few hours
        //
        [Browsable(true)]
        public string SisiEveAccessTokenString
        {
            get { return GetValue(() => SisiEveAccessTokenString); }
            set { SetValue(() => SisiEveAccessTokenString, value); }
        }

        //
        // Good for only a few hours
        //
        [Browsable(true)]
        public DateTime SisiEveAccessTokenValidUntil
        {
            get { return GetValue(() => SisiEveAccessTokenValidUntil); }
            set { SetValue(() => SisiEveAccessTokenValidUntil, value); }
        }

        //
        // Good for 3 months?
        //
        [Browsable(true)]
        public string SisiRefreshTokenString
        {
            get { return GetValue(() => SisiRefreshTokenString); }
            set { SetValue(() => SisiRefreshTokenString, value); }
        }

        [Browsable(true)]
        public DateTime SisiRefreshTokenValidUntil
        {
            get { return GetValue(() => SisiRefreshTokenValidUntil); }
            set { SetValue(() => SisiRefreshTokenValidUntil, value); }
        }

        //Not used as a chat channel anymore, only used to figure out which toons are part of the same fleet: so this should be set on all toons that are in the same fleet: think of it as the fleet name (should be renamed at some point)
        [Browsable(true)]
        public string ChatChannelToPullFleetInvitesFrom
        {
            get { return GetValue(() => ChatChannelToPullFleetInvitesFrom); }
            set { SetValue(() => ChatChannelToPullFleetInvitesFrom, value); }
        }

        [Browsable(true)]
        public string FleetName
        {
            get { return GetValue(() => FleetName); }
            set { SetValue(() => FleetName, value); }
        }

        [Browsable(true)]
        public bool AllowNonEveSharpLauncherCharacterFleetInvites
        {
            get { return GetValue(() => AllowNonEveSharpLauncherCharacterFleetInvites); }
            set { SetValue(() => AllowNonEveSharpLauncherCharacterFleetInvites, value); }
        }

        [Browsable(true)]
        [ReadOnly(true)]
        public string GUID
        {
            get { return GetValue(() => GUID); }
            set { SetValue(() => GUID, value); }
        }

        [Browsable(false)]
        public bool HardwareSpoofing
        {
            get { return GetValue(() => HardwareSpoofing); }
            set { SetValue(() => HardwareSpoofing, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(true)]
        public string HardwareGUID
        {
            get
            {
                if (HWSettings == null)
                {
                    if (string.IsNullOrEmpty(HWSettings.GUID))
                        return "n/a";

                    return HWSettings.GUID;
                }

                return "n/a";
            }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(true)]
        public string ProxyName
        {
            get
            {
                if (HWSettings == null)
                {
                    if (HWSettings.Proxy == null)
                        return "n/a";

                    if (!HWSettings.Proxy.IsValid)
                        return "n/a";

                    return HWSettings.Proxy.Description;
                }

                return "n/a";
            }
        }

        [Browsable(false)]
        public DateTime AbyssSecondsDailyLastReset
        {
            get { return GetValue(() => AbyssSecondsDailyLastReset); }
            set { SetValue(() => AbyssSecondsDailyLastReset, value); }
        }

        [Browsable(false)]
        public int AbyssSecondsDaily
        {
            get
            {
                if (AbyssSecondsDailyLastReset.Day != DateTime.Now.Day || AbyssSecondsDailyLastReset.Month != DateTime.Now.Month || AbyssSecondsDailyLastReset.Year != DateTime.Now.Year)
                {
                    AbyssSecondsDaily = 0;
                    AbyssSecondsDailyLastReset = DateTime.UtcNow;
                    return 0;
                }
                return GetValue(() => AbyssSecondsDaily);
            }
            set
            {
                SetValue(() => AbyssSecondsDaily, value);
            }
        }

        [Browsable(false)]
        public bool Hidden
        {
            get { return GetValue(() => Hidden); }
            set
            {
                SetValue(() => Hidden, value);
                if (value) HideWindows();
                else ShowWindows();
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long HookmanagerHWnd
        {
            get { return GetValue(() => HookmanagerHWnd); }
            set { SetValue(() => HookmanagerHWnd, value); }
        }

        public int HoursPerDay
        {
            get { return GetValue(() => HoursPerDay); }
            set
            {
                SetValue(() => HoursPerDay, value);
                GenerateNewTimeSpans();
            }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public int HoursPerDayII
        {
            get { return GetValue(() => HoursPerDayII); }
            set
            {
                SetValue(() => HoursPerDayII, value);
                GenerateNewTimeSpans();
            }
        }

        /**
        public double HullHitPoints
        {
            get { return GetValue(() => HullHitPoints); }
            set { SetValue(() => HullHitPoints, value); }
        }

        public double HullPct
        {
            get { return GetValue(() => HullPct); }
            set { SetValue(() => HullPct, value); }
        }
        **/

        [Browsable(false)]
        public HWSettings HWSettings
        {
            get { return GetValue(() => HWSettings); }
            set { SetValue(() => HWSettings, value); }
        }

        [Browsable(true)]
        public bool IgnoreDowntime
        {
            get { return GetValue(() => IgnoreDowntime); }
            set { SetValue(() => IgnoreDowntime, value); }
        }

        [Browsable(true)]
        public bool IgnoreSeralizationErrors
        {
            get { return GetValue(() => IgnoreSeralizationErrors); }
            set { SetValue(() => IgnoreSeralizationErrors, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public string IMAPHost
        {
            get { return GetValue(() => IMAPHost); }
            set { SetValue(() => IMAPHost, value); }
        }

        [Browsable(true)]
        public bool IsInAbyss
        {
            get { return GetValue(() => IsInAbyss); }
            set { SetValue(() => IsInAbyss, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public string Info
        {
            get { return GetValue(() => Info); }
            set { SetValue(() => Info, value); }
        }

        [ReadOnly(true)]
        public bool InMission
        {
            get { return GetValue(() => InMission); }
            set { SetValue(() => InMission, value); }
        }

        [ReadOnly(true)]
        public bool IsAtWar
        {
            get { return GetValue(() => IsAtWar); }
            set { SetValue(() => IsAtWar, value); }
        }

        internal void PushDataToSlaves(bool write = false, bool value = false)
        {
            if (IsLeader)
            {
                //if (Interval(500))
                {
                    try
                    {
                        foreach (var individualEveAccount in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(e => e.UseFleetMgr && !string.IsNullOrEmpty(e.AccountName) && !string.IsNullOrEmpty(e.CharacterName) && ChatChannelToPullFleetInvitesFrom == e.ChatChannelToPullFleetInvitesFrom && !e.IsLeader).OrderBy(i => i.GUID))
                        {
                            Cache.Instance.Log("individualEveAccount [" + individualEveAccount.MaskedCharacterName + "]");
                            if (write) individualEveAccount.LeaderInSpace = !value;
                            Cache.Instance.Log("LeaderInSpace [" + individualEveAccount.LeaderInSpace + "]");
                            if (write) individualEveAccount.LeaderInStation = value;
                            Cache.Instance.Log("LeaderInStation [" + individualEveAccount.LeaderInStation + "]");
                            individualEveAccount.LeaderIsInSystemName = SolarSystem;
                            Cache.Instance.Log("LeaderIsInSystemName [" + individualEveAccount.LeaderIsInSystemName + "]");
                            individualEveAccount.LeaderCharacterName = CharacterName;
                            Cache.Instance.Log("LeaderCharacterName [" + individualEveAccount.LeaderCharacterName + "]");
                            individualEveAccount.LeaderCharacterId = MyCharacterId;
                            Cache.Instance.Log("LeaderCharacterId [" + individualEveAccount.LeaderCharacterId + "]");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("exception [" + ex + "]");
                    }
                }
            }
        }

        [ReadOnly(true)]
        public bool IsDocked
        {
            get { return GetValue(() => IsDocked); }
            set { SetValue(() => IsDocked, value); }
        }

        public double IskPerLp
        {
            get { return GetValue(() => IskPerLp); }
            set { SetValue(() => IskPerLp, value); }
        }

        public bool IsLeader
        {
            get { return GetValue(() => IsLeader); }
            set { SetValue(() => IsLeader, value); }
        }

        public bool InteractedWithEVERecently
        {
            get
            {
                if (LastInteractedWithEVE.AddSeconds(30) > DateTime.UtcNow)
                    return true;

                if (LastInWarp.AddSeconds(30) > DateTime.UtcNow)
                    return true;

                if (IsDocked || IsInAbyss || DateTime.UtcNow.AddSeconds(20) > LastInWarp)
                {
                    if (LastSessionReady.AddSeconds(40) > DateTime.UtcNow)
                        return true;

                    return false;
                }
                else if (LastSessionReady.AddSeconds(20) > DateTime.UtcNow)
                    return true;

                return false;
            }
        }

        public bool IsOmegaClone
        {
            get { return GetValue(() => IsOmegaClone); }
            set { SetValue(() => IsOmegaClone, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public bool IsSafeInPOS
        {
            get { return GetValue(() => IsSafeInPOS); }
            set { SetValue(() => IsSafeInPOS, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        [XmlIgnore]
        public bool IsValidForAutoLogin
        {
            get
            {
                if (string.IsNullOrEmpty(CharacterName))
                    return false;

                if (string.IsNullOrEmpty(AccountName))
                    return false;

                if (string.IsNullOrEmpty(Password))
                    return false;

                return true;
            }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        [XmlIgnore]
        public bool IsValidForManualLogin
        {
            get
            {
                if (string.IsNullOrEmpty(AccountName))
                    return false;

                if (string.IsNullOrEmpty(Password))
                    return false;

                return true;
            }
        }

        public double ItemHangarValue
        {
            get { return GetValue(() => ItemHangarValue); }
            set { SetValue(() => ItemHangarValue, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastAmmoBuy
        {
            get { return GetValue(() => LastAmmoBuy); }
            set { SetValue(() => LastAmmoBuy, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastAmmoBuyAttempt
        {
            get { return GetValue(() => LastAmmoBuyAttempt); }
            set { SetValue(() => LastAmmoBuyAttempt, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastBuyLpItemAttempt
        {
            get { return GetValue(() => LastBuyLpItemAttempt); }
            set { SetValue(() => LastBuyLpItemAttempt, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastBuyLpItems
        {
            get { return GetValue(() => LastBuyLpItems); }
            set { SetValue(() => LastBuyLpItems, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastConsumeBoosterSlot1
        {
            get { return GetValue(() => LastConsumeBoosterSlot1); }
            set { SetValue(() => LastConsumeBoosterSlot1, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastConsumeBoosterSlot10
        {
            get { return GetValue(() => LastConsumeBoosterSlot10); }
            set { SetValue(() => LastConsumeBoosterSlot10, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastConsumeBoosterSlot2
        {
            get { return GetValue(() => LastConsumeBoosterSlot2); }
            set { SetValue(() => LastConsumeBoosterSlot2, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastConsumeBoosterSlot255
        {
            get { return GetValue(() => LastConsumeBoosterSlot255); }
            set { SetValue(() => LastConsumeBoosterSlot255, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastConsumeBoosterSlot3
        {
            get { return GetValue(() => LastConsumeBoosterSlot3); }
            set { SetValue(() => LastConsumeBoosterSlot3, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastConsumeBoosterSlot4
        {
            get { return GetValue(() => LastConsumeBoosterSlot4); }
            set { SetValue(() => LastConsumeBoosterSlot4, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastConsumeBoosterSlotUnknown
        {
            get { return GetValue(() => LastConsumeBoosterSlotUnknown); }
            set { SetValue(() => LastConsumeBoosterSlotUnknown, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public int LastEveCpuUsagePercentage
        {
            get { return GetValue(() => LastEveCpuUsagePercentage); }
            set { SetValue(() => LastEveCpuUsagePercentage, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public int CurrentFpsLimit
        {
            get { return GetValue(() => CurrentFpsLimit); }
            set { SetValue(() => CurrentFpsLimit, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public int AllRunningAccountsEveCpuUsagePercentage
        {
            get
            {
                int allRunningAccountsEveCpuUsagePercentage = Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(i => i.Pid != 0).Sum(i => i.LastEveCpuUsagePercentage);
                return allRunningAccountsEveCpuUsagePercentage;
            }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastCreateContract
        {
            get { return GetValue(() => LastCreateContract); }
            set { SetValue(() => LastCreateContract, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastCreateContractAttempt
        {
            get { return GetValue(() => LastCreateContractAttempt); }
            set { SetValue(() => LastCreateContractAttempt, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LastEntityIdEngaged
        {
            get { return GetValue(() => LastEntityIdEngaged); }
            set { SetValue(() => LastEntityIdEngaged, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public int LastEveProcessPidInject
        {
            get { return GetValue(() => LastEveProcessPidInject); }
            set { SetValue(() => LastEveProcessPidInject, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastInteractedWithEVE
        {
            get { return GetValue(() => LastInteractedWithEVE); }
            set { SetValue(() => LastInteractedWithEVE, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastInWarp
        {
            get { return GetValue(() => LastInWarp); }
            set { SetValue(() => LastInWarp, value); }
        }

        [ReadOnly(true)]
        public string LastMissionName
        {
            get { return GetValue(() => LastMissionName); }
            set { SetValue(() => LastMissionName, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public string LastPcName
        {
            get { return GetValue(() => LastPcName); }
            set { SetValue(() => LastPcName, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastPcNameDateTime
        {
            get { return GetValue(() => LastPcNameDateTime); }
            set { SetValue(() => LastPcNameDateTime, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastPlexBuy
        {
            get { return GetValue(() => LastPlexBuy); }
            set { SetValue(() => LastPlexBuy, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastQuestorStarted
        {
            get { return GetValue(() => LastQuestorStarted); }
            set { SetValue(() => LastQuestorStarted, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastSessionReady
        {
            get { return GetValue(() => LastSessionReady); }
            set { SetValue(() => LastSessionReady, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime NextUpdateStaticSlavesDataToLeaderTimestamp
        {
            get { return GetValue(() => NextUpdateStaticSlavesDataToLeaderTimestamp); }
            set { SetValue(() => NextUpdateStaticSlavesDataToLeaderTimestamp, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime NextUpdateDynamicSlavesDataToLeaderTimestamp
        {
            get { return GetValue(() => NextUpdateDynamicSlavesDataToLeaderTimestamp); }
            set { SetValue(() => NextUpdateDynamicSlavesDataToLeaderTimestamp, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime NextUpdateStaticLeaderInfoToSlavesTimestamp
        {
            get { return GetValue(() => NextUpdateStaticLeaderInfoToSlavesTimestamp); }
            set { SetValue(() => NextUpdateStaticLeaderInfoToSlavesTimestamp, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime NextUpdateDynamicLeaderInfoToSlavesTimestamp
        {
            get { return GetValue(() => NextUpdateDynamicLeaderInfoToSlavesTimestamp); }
            set { SetValue(() => NextUpdateDynamicLeaderInfoToSlavesTimestamp, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public float LastStartEveQueuePriority
        {
            get { return GetValue(() => LastStartEveQueuePriority); }
            set { SetValue(() => LastStartEveQueuePriority, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public string LeaderCharacterId
        {
            get { return GetValue(() => LeaderCharacterId); }
            set { SetValue(() => LeaderCharacterId, value); }
        }

        public string LeaderGUID
        {
            get { return GetValue(() => LeaderGUID); }
            set { SetValue(() => LeaderGUID, value); }
        }

        public string LeaderCharacterName
        {
            get { return GetValue(() => LeaderCharacterName); }
            set { SetValue(() => LeaderCharacterName, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderEntityId
        {
            get { return GetValue(() => LeaderEntityId); }
            set { SetValue(() => LeaderEntityId, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderFleetID
        {
            get { return GetValue(() => LeaderFleetID); }
            set { SetValue(() => LeaderFleetID, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderHomeStationId
        {
            get { return GetValue(() => LeaderHomeStationId); }
            set { SetValue(() => LeaderHomeStationId, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderHomeSystemId
        {
            get { return GetValue(() => LeaderHomeSystemId); }
            set { SetValue(() => LeaderHomeSystemId, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool LeaderInSpace
        {
            get { return GetValue(() => LeaderInSpace); }
            set { SetValue(() => LeaderInSpace, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool LeaderInStation
        {
            get { return GetValue(() => LeaderInStation); }
            set { SetValue(() => LeaderInStation, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderInStationId
        {
            get { return GetValue(() => LeaderInStationId); }
            set { SetValue(() => LeaderInStationId, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool LeaderInWarp
        {
            get { return GetValue(() => LeaderInWarp); }
            set { SetValue(() => LeaderInWarp, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderIsAggressingTargetId
        {
            get { return GetValue(() => LeaderIsAggressingTargetId); }
            set { SetValue(() => LeaderIsAggressingTargetId, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderIsInSystemId
        {
            get { return GetValue(() => LeaderIsInSystemId); }
            set { SetValue(() => LeaderIsInSystemId, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public string LeaderIsInSystemName
        {
            get { return GetValue(() => LeaderIsInSystemName); }
            set { SetValue(() => LeaderIsInSystemName, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long? LeaderIsTargetingId1
        {
            get { return GetValue(() => LeaderIsTargetingId1); }
            set { SetValue(() => LeaderIsTargetingId1, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long? LeaderIsTargetingId10
        {
            get { return GetValue(() => LeaderIsTargetingId10); }
            set { SetValue(() => LeaderIsTargetingId10, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long? LeaderIsTargetingId2
        {
            get { return GetValue(() => LeaderIsTargetingId2); }
            set { SetValue(() => LeaderIsTargetingId2, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long? LeaderIsTargetingId3
        {
            get { return GetValue(() => LeaderIsTargetingId3); }
            set { SetValue(() => LeaderIsTargetingId3, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long? LeaderIsTargetingId4
        {
            get { return GetValue(() => LeaderIsTargetingId4); }
            set { SetValue(() => LeaderIsTargetingId4, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long? LeaderIsTargetingId5
        {
            get { return GetValue(() => LeaderIsTargetingId5); }
            set { SetValue(() => LeaderIsTargetingId5, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long? LeaderIsTargetingId6
        {
            get { return GetValue(() => LeaderIsTargetingId6); }
            set { SetValue(() => LeaderIsTargetingId6, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long? LeaderIsTargetingId7
        {
            get { return GetValue(() => LeaderIsTargetingId7); }
            set { SetValue(() => LeaderIsTargetingId7, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long? LeaderIsTargetingId8
        {
            get { return GetValue(() => LeaderIsTargetingId8); }
            set { SetValue(() => LeaderIsTargetingId8, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long? LeaderIsTargetingId9
        {
            get { return GetValue(() => LeaderIsTargetingId9); }
            set { SetValue(() => LeaderIsTargetingId9, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LeaderLastActivate
        {
            get { return GetValue(() => LeaderLastActivate); }
            set { SetValue(() => LeaderLastActivate, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LeaderLastAlign
        {
            get { return GetValue(() => LeaderLastAlign); }
            set { SetValue(() => LeaderLastAlign, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LeaderLastApproach
        {
            get { return GetValue(() => LeaderLastApproach); }
            set { SetValue(() => LeaderLastApproach, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LeaderLastDock
        {
            get { return GetValue(() => LeaderLastDock); }
            set { SetValue(() => LeaderLastDock, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderLastEntityIdActivate
        {
            get { return GetValue(() => LeaderLastEntityIdActivate); }
            set { SetValue(() => LeaderLastEntityIdActivate, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderLastEntityIdAlign
        {
            get { return GetValue(() => LeaderLastEntityIdAlign); }
            set { SetValue(() => LeaderLastEntityIdAlign, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderLastEntityIdApproach
        {
            get { return GetValue(() => LeaderLastEntityIdApproach); }
            set { SetValue(() => LeaderLastEntityIdApproach, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderLastEntityIdDock
        {
            get { return GetValue(() => LeaderLastEntityIdDock); }
            set { SetValue(() => LeaderLastEntityIdDock, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderLastEntityIdJump
        {
            get { return GetValue(() => LeaderLastEntityIdJump); }
            set { SetValue(() => LeaderLastEntityIdJump, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderLastEntityIdKeepAtRange
        {
            get { return GetValue(() => LeaderLastEntityIdKeepAtRange); }
            set { SetValue(() => LeaderLastEntityIdKeepAtRange, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderLastEntityIdOrbit
        {
            get { return GetValue(() => LeaderLastEntityIdOrbit); }
            set { SetValue(() => LeaderLastEntityIdOrbit, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderLastEntityIdWarp
        {
            get { return GetValue(() => LeaderLastEntityIdWarp); }
            set { SetValue(() => LeaderLastEntityIdWarp, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LeaderLastJump
        {
            get { return GetValue(() => LeaderLastJump); }
            set { SetValue(() => LeaderLastJump, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LeaderLastKeepAtRange
        {
            get { return GetValue(() => LeaderLastKeepAtRange); }
            set { SetValue(() => LeaderLastKeepAtRange, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public int LeaderLastKeepAtRangeDistance
        {
            get { return GetValue(() => LeaderLastKeepAtRangeDistance); }
            set { SetValue(() => LeaderLastKeepAtRangeDistance, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LeaderLastOrbit
        {
            get { return GetValue(() => LeaderLastOrbit); }
            set { SetValue(() => LeaderLastOrbit, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public int LeaderLastOrbitDistance
        {
            get { return GetValue(() => LeaderLastOrbitDistance); }
            set { SetValue(() => LeaderLastOrbitDistance, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LeaderLastRequestRegroup
        {
            get { return GetValue(() => LeaderLastRequestRegroup); }
            set { SetValue(() => LeaderLastRequestRegroup, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LeaderLastStoppedShip
        {
            get { return GetValue(() => LeaderLastStoppedShip); }
            set { SetValue(() => LeaderLastStoppedShip, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LeaderLastWarp
        {
            get { return GetValue(() => LeaderLastWarp); }
            set { SetValue(() => LeaderLastWarp, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool LeaderOnGridWithAsteroidBelt
        {
            get { return GetValue(() => LeaderOnGridWithAsteroidBelt); }
            set { SetValue(() => LeaderOnGridWithAsteroidBelt, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderOnGridWithEntityIdAsteroidBelt
        {
            get { return GetValue(() => LeaderOnGridWithEntityIdAsteroidBelt); }
            set { SetValue(() => LeaderOnGridWithEntityIdAsteroidBelt, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderOnGridWithEntityIdStargate
        {
            get { return GetValue(() => LeaderOnGridWithEntityIdStargate); }
            set { SetValue(() => LeaderOnGridWithEntityIdStargate, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderOnGridWithEntityIdStation
        {
            get { return GetValue(() => LeaderOnGridWithEntityIdStation); }
            set { SetValue(() => LeaderOnGridWithEntityIdStation, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool LeaderOnGridWithStargate
        {
            get { return GetValue(() => LeaderOnGridWithStargate); }
            set { SetValue(() => LeaderOnGridWithStargate, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool LeaderOnGridWithStation
        {
            get { return GetValue(() => LeaderOnGridWithStation); }
            set { SetValue(() => LeaderOnGridWithStation, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public string LeaderRepairGroup
        {
            get { return GetValue(() => LeaderRepairGroup); }
            set { SetValue(() => LeaderRepairGroup, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long TravelerDestinationSystemId
        {
            get { return GetValue(() => TravelerDestinationSystemId); }
            set { SetValue(() => TravelerDestinationSystemId, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public long LeaderTravelerDestinationSystemId
        {
            get { return GetValue(() => LeaderTravelerDestinationSystemId); }
            set { SetValue(() => LeaderTravelerDestinationSystemId, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool LeaderWarpDriveActive
        {
            get { return GetValue(() => LeaderWarpDriveActive); }
            set { SetValue(() => LeaderWarpDriveActive, value); }
        }

        public double LoyaltyPoints
        {
            get { return GetValue(() => LoyaltyPoints); }
            set { SetValue(() => LoyaltyPoints, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public double LpValue
        {
            get
            {
                double _lpValue = IskPerLp * LoyaltyPoints;
                return _lpValue;
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool ManuallyPausedViaUI
        {
            get { return GetValue(() => ManuallyPausedViaUI); }
            set { SetValue(() => ManuallyPausedViaUI, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public string MaskedCharacterName
        {
            get
            {
                if (!string.IsNullOrEmpty(CharacterName))
                {
                    return CharacterName.Substring(0, 3) + "-MaskedCharName-";
                }

                return string.Empty;
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public string MaskedCharacterId
        {
            get
            {
                if (!string.IsNullOrEmpty(MyCharacterId))
                {
                    return MyCharacterId.Substring(0, 3) + "-Masked-";
                }

                return string.Empty;
            }
        }

        [ReadOnly(true)]
        public DateTime MissionStarted
        {
            get { return GetValue(() => MissionStarted); }
            set { SetValue(() => MissionStarted, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public bool MovingItemsThereAreNoMoreItemsToGrabAtPickup
        {
            get { return GetValue(() => MovingItemsThereAreNoMoreItemsToGrabAtPickup); }
            set { SetValue(() => MovingItemsThereAreNoMoreItemsToGrabAtPickup, value); }
        }

        [ReadOnly(true)]
        public string MyCharacterId
        {
            get { return GetValue(() => MyCharacterId); }
            set { SetValue(() => MyCharacterId, value); }
        }

        [ReadOnly(true)]
        public string MyCorp
        {
            get { return GetValue(() => MyCorp); }
            set { SetValue(() => MyCorp, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public string MyCorpId
        {
            get { return GetValue(() => MyCorpId); }
            set { SetValue(() => MyCorpId, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public int MyEntityMode
        {
            get { return GetValue(() => MyEntityMode); }
            set { SetValue(() => MyEntityMode, value); }
        }

        public string MyNote1
        {
            get { return GetValue(() => MyNote1); }
            set { SetValue(() => MyNote1, value); }
        }

        public string MyNote2
        {
            get { return GetValue(() => MyNote2); }
            set { SetValue(() => MyNote2, value); }
        }

        public string MyNote3
        {
            get { return GetValue(() => MyNote3); }
            set { SetValue(() => MyNote3, value); }
        }

        public string MyNote4
        {
            get { return GetValue(() => MyNote4); }
            set { SetValue(() => MyNote4, value); }
        }


        [ReadOnly(true)]
        public DateTime MySkillQueueEnds
        {
            get { return GetValue(() => MySkillQueueEnds); }
            set { SetValue(() => MySkillQueueEnds, value); }
        }

        [ReadOnly(true)]
        public string MySkillTraining
        {
            get { return GetValue(() => MySkillTraining); }
            set { SetValue(() => MySkillTraining, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool NeedHumanIntervention
        {
            get { return GetValue(() => NeedHumanIntervention); }
            set { SetValue(() => NeedHumanIntervention, value); }
        }

        [XmlIgnore]
        public bool NeedRepair
        {
            get { return GetValue(() => NeedRepair); }
            set { SetValue(() => NeedRepair, value); }
        }

        public bool AlwaysRepair
        {
            get { return GetValue(() => AlwaysRepair); }
            set { SetValue(() => AlwaysRepair, value); }
        }

        [Browsable(false)]
        public DateTime NextCacheDeletion
        {
            get { return GetValue(() => NextCacheDeletion); }
            set { SetValue(() => NextCacheDeletion, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime NextWarCheck
        {
            get { return GetValue(() => NextWarCheck); }
            set { SetValue(() => NextWarCheck, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool NotAllItemsCouldBeFitted
        {
            get { return GetValue(() => NotAllItemsCouldBeFitted); }
            set { SetValue(() => NotAllItemsCouldBeFitted, value); }
        }

        public int Num
        {
            get { return GetValue(() => Num); }
            set { SetValue(() => Num, value); }
        }

        [Browsable(true)]
        public int NumOfAbyssalSitesBeforeRestarting
        {
            get { return GetValue(() => NumOfAbyssalSitesBeforeRestarting); }
            set { SetValue(() => NumOfAbyssalSitesBeforeRestarting, value); }
        }

        [Browsable(true)]
        public int AbyssalFilamentsActivated
        {
            get { return GetValue(() => AbyssalFilamentsActivated); }
            set { SetValue(() => AbyssalFilamentsActivated, value); }
        }

        [Browsable(true)]
        public double LootValueGatheredToday
        {
            get { return GetValue(() => LootValueGatheredToday); }
            set { SetValue(() => LootValueGatheredToday, value); }
        }

        [Browsable(true)]
        public int NumOfAbyssalSitesBeforeWaitingInStationForRandomTime
        {
            get { return GetValue(() => NumOfAbyssalSitesBeforeWaitingInStationForRandomTime); }
            set { SetValue(() => NumOfAbyssalSitesBeforeWaitingInStationForRandomTime, value); }
        }

        public int BaseTimeToWaitInStationForRandomTime
        {
            get { return GetValue(() => BaseTimeToWaitInStationForRandomTime); }
            set { SetValue(() => BaseTimeToWaitInStationForRandomTime, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool OtherToonsAreStillLoggingIn
        {
            get
            {
                try
                {
                    return false; //Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(a => a.Running && !a.DoneLaunchingEveInstance && a.CharacterName != CharacterName);
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return false;
                }
            }
        }

        public string Password
        {
            get { return GetValue(() => Password); }
            set
            {
                if (Cache.IsServer && Pid != 0)
                {
                    Cache.Instance.Log("Can't change the password until the client has been stopped.");
                    return;
                }
                SetValue(() => Password, value);
                //ClearTokens();
            }
        }

        public string Pattern
        {
            get
            {
                //if (UseFleetMgr && !IsLeader && !string.IsNullOrEmpty(ChatChannelToPullFleetInvitesFrom))
                //{
                //    //Copy schedule from the leader!
                ///    foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(i => i.GUID != GUID && i.UseFleetMgr && i.IsLeader && ChatChannelToPullFleetInvitesFrom == i.ChatChannelToPullFleetInvitesFrom))
                //    {
                //        return eA.Pattern; //there should be only one leader per fleet
                //    }
                //}

                return GetValue(() => Pattern);
            }
            set { SetValue(() => Pattern, value); }
        }

        [Browsable(false)]
        public DateTime LastPatternTimeUpdate
        {
            get { return GetValue(() => LastPatternTimeUpdate); }
            set { SetValue(() => LastPatternTimeUpdate, value); }
        }

        private DateTime _lastExceptionCheck;
        private int _lastExceptionCheckValue;

        public void ResetExceptions()
        {
            SetValue(() => AmountExceptionsCurrentSession, 0);
            _lastExceptionCheckValue = 0;
        }

        [XmlIgnore]
        [Browsable(true)]
        public int AmountExceptionsCurrentSession
        {
            get
            {
                var val = GetValue(() => AmountExceptionsCurrentSession);
                return val;
            }
            set
            {
                try
                {
                    var interval = 30;
                    var limit = 35;


                    //Cache.Instance.Log($"[{_lastExceptionCheckValue}] AmountExceptionsCurrentSession [{AmountExceptionsCurrentSession}]");
                    if (_lastExceptionCheck.AddSeconds(interval) < DateTime.UtcNow)
                    {
                        _lastExceptionCheck = DateTime.UtcNow;

                        if (Math.Abs(_lastExceptionCheckValue - AmountExceptionsCurrentSession) > limit && !SelectedController.Equals("None"))
                        {
                            Cache.Instance.Log($"[{MaskedCharacterName}] Received more than [{limit}] exceptions in the EVE Client in the last [{interval}] seconds.");
                        }

                        _lastExceptionCheckValue = AmountExceptionsCurrentSession;
                    }

                    SetValue(() => AmountExceptionsCurrentSession, value);
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                }
            }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public int Pid
        {
            get { return GetValue(() => Pid); }
            set { SetValue(() => Pid, value); }
        }

        [Browsable(false)]
        public int AbyssStage
        {
            get { return GetValue(() => AbyssStage); }
            set { SetValue(() => AbyssStage, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public double RamUsage
        {
            get
            {
                try
                {
                    if (!EveProcessExists)
                        return 0;

                    if (Pid != 0)
                    {
                        if (Util.ProcessList == null)
                            return 0;

                        Process p = Array.Find(Util.ProcessList, i => i.Id == Pid);
                        return (double)p.WorkingSet64 / 1024 / 1024;
                    }
                    return 0;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string ShouldldBeRunningReason
        {
            get { return GetValue(() => ShouldldBeRunningReason); }
            set { SetValue(() => ShouldldBeRunningReason, value); }
        }

        [Browsable(true)]
        public int DPSGroup
        {
            get { return GetValue(() => DPSGroup); }
            set { SetValue(() => DPSGroup, value); }
        }

        [Browsable(true)]
        public string RepairGroup
        {
            get { return GetValue(() => RepairGroup); }
            set { SetValue(() => RepairGroup, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool ReplaceMissionsActions
        {
            get { return GetValue(() => ReplaceMissionsActions); }
            set { SetValue(() => ReplaceMissionsActions, value); }
        }

        public bool RequireOmegaClone
        {
            get { return GetValue(() => RequireOmegaClone); }
            set { SetValue(() => RequireOmegaClone, value); }
        }

        [XmlIgnore]
        [Browsable(true)]
        [ReadOnly(false)]
        public bool RestartOfEveClientNeeded
        {
            get { return GetValue(() => RestartOfEveClientNeeded); }
            set { SetValue(() => RestartOfEveClientNeeded, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool RestartOfBotNeeded
        {
            get { return GetValue(() => RestartOfBotNeeded); }
            set { SetValue(() => RestartOfBotNeeded, value); }
        }

        [Browsable(false)]
        public string Salt
        {
            get { return GetValue(() => Salt); }
            set { SetValue(() => Salt, value); }
        }

        [Browsable(false)]
        public string SelectedController
        {
            get
            {
                string ret = GetValue(() => SelectedController);
                if (string.IsNullOrEmpty(ret))
                {
                    SelectedController = DefaultController;
                    return DefaultController;
                }
                return ret;
            }
            set { SetValue(() => SelectedController, value); }
        }

        /**
        public double ShieldHitPoints
        {
            get { return GetValue(() => ShieldHitPoints); }
            set { SetValue(() => ShieldHitPoints, value); }
        }

        public double ShieldPct
        {
            get { return GetValue(() => ShieldPct); }
            set { SetValue(() => ShieldPct, value); }
        }
        **/

        [ReadOnly(true)]
        public string ShipType
        {
            get { return GetValue(() => ShipType); }
            set { SetValue(() => ShipType, value); }
        }

        /**
        [ReadOnly(true)]
        public int ShipTypeId
        {
            get { return GetValue(() => ShipTypeId); }
            set { SetValue(() => ShipTypeId, value); }
        }

        [ReadOnly(true)]
        public int ShipGroupId
        {
            get { return GetValue(() => ShipGroupId); }
            set { SetValue(() => ShipGroupId, value); }
        }

        [ReadOnly(true)]
        public int NumOfLargeRemoteEnergyTransferModules
        {
            get { return GetValue(() => NumOfLargeRemoteEnergyTransferModules); }
            set { SetValue(() => NumOfLargeRemoteEnergyTransferModules, value); }
        }

        [ReadOnly(true)]
        public int NumOfLargeRemoteArmorTransferModules
        {
            get { return GetValue(() => NumOfLargeRemoteArmorTransferModules); }
            set { SetValue(() => NumOfLargeRemoteArmorTransferModules, value); }
        }

        [ReadOnly(true)]
        public int NumOfLargeRemoteShieldTransferModules
        {
            get { return GetValue(() => NumOfLargeRemoteShieldTransferModules); }
            set { SetValue(() => NumOfLargeRemoteShieldTransferModules, value); }
        }

        [ReadOnly(true)]
        public string CapacitorChainLogisticsGiveCapTo
        {
            get { return GetValue(() => CapacitorChainLogisticsGiveCapTo); }
            set { SetValue(() => CapacitorChainLogisticsGiveCapTo, value); }
        }

        [ReadOnly(true)]
        public string CapacitorChainBattleshipsGiveCapTo
        {
            get { return GetValue(() => CapacitorChainBattleshipsGiveCapTo); }
            set { SetValue(() => CapacitorChainBattleshipsGiveCapTo, value); }
        }
        **/

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool ShouldBeStopped
        {
            get
            {
                try
                {
                    if (IsInAbyss)
                        return false;

                    if (InMission)
                        return false;

                    if (!UseScheduler)
                    {
                        return false;
                    }

                    if (!Cache.Instance.EveSettings.IsSchedulerRunning)
                    {
                        return false;
                    }

                    if (IsDocked || IsSafeInPOS)
                    {
                        //new scheduler
                        if (PatternManagerEnabled)
                        {
                            if (ManuallyStarted || PatternEval.IsAnyPatternMatchingDatetime(this, DateTime.UtcNow))
                            {
                                return false;
                            }

                            return true;
                        }

                        //
                        // old scheduler
                        //
                        if (IsItPastStartTime && !IsItPastEndTime)
                        {
                            return false;
                        }

                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return false;
                }
            }
        }

        [XmlIgnore]
        public Dictionary<long, DateTime> CurrentTargets = new Dictionary<long, DateTime>();

        [XmlIgnore]
        public Dictionary<long, DateTime> CurrentKillTarget = new Dictionary<long, DateTime>();

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool ShouldBeStarted
        {
            get
            {
                try
                {
                    if (LastEveClientLaunched.AddSeconds(35) > DateTime.UtcNow)
                    {
                        if (Util.IntervalInMinutes(45, 45, CharacterName) || DebugScheduler)
                            Cache.Instance.Log("ShouldBeStarted [false][" + MaskedAccountName + "][" + MaskedCharacterName + "] if (LastEveClientLaunched.AddSeconds(35) > DateTime.UtcNow)");
                        return false;
                    }

                    if (MyEveAccountIsAlreadyLoggedIn)
                    {
                        if (Util.IntervalInMinutes(45, 45, CharacterName) || DebugScheduler)
                            Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] if (myEveAccountIsAlreadyLoggedIn)");
                        ShouldldBeRunningReason = "MyEveAccountIsAlreadyLoggedIn";
                        return false;
                    }

                    try
                    {
                        if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(i => !string.IsNullOrEmpty(i.AccountName) && !string.IsNullOrEmpty(i.CharacterName) && i.ConnectToTestServer == ConnectToTestServer && i.AccountName != AccountName && i.HWSettings != null && i.HWSettings.GUID != null && i.HWSettings.GUID == HWSettings.GUID && i.Pid != 0))
                        {
                            Thread.Sleep(1);
                            EveAccount accountWithDuplicateHardwareProfile = Cache.Instance.EveAccountSerializeableSortableBindingList.List.FirstOrDefault(i => i.AccountName != AccountName && i.HWSettings.GUID == HWSettings.GUID && i.Pid != 0);
                            Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "] This account is using the same Hardware profile as [" + accountWithDuplicateHardwareProfile.MaskedAccountName + "][" + accountWithDuplicateHardwareProfile.MaskedCharacterName + "] aborting login");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("Has your (spoofed) Hardware been defined for this account?");
                        Cache.Instance.Log("Exception [" + ex + "]");
                    }

                    if (HWSettings.Proxy == null)
                    {
                        Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "]: Proxy is null!?");
                        return false;
                    }

                    if (!HWSettings.Proxy.IsValid)
                    {
                        Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "]: Proxy.IsValid [ false ]");
                        return false;
                    }

                    if (!HWSettings.Proxy.CheckSocks5InternetConnectivity(this))
                    {
                        Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "]: CheckHttpProxyInternetConnectivity [ false ]");
                        return false;
                    }


                    if (ClientSettingsSerializationErrors > MAX_SERIALIZATION_ERRORS && !IgnoreSeralizationErrors)
                    {
                        Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "] Client settings serialization error!");
                        UseScheduler = false;
                        return false;
                    }

                    if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(i => !string.IsNullOrEmpty(i.AccountName) && !string.IsNullOrEmpty(i.CharacterName) && i.Pid != 0 && !i.DoneLaunchingEveInstance && i.MyCharacterId != MyCharacterId && !IsInAbyss && !RestartOfEveClientNeeded && !TestingEmergencyReLogin))
                    {
                        Thread.Sleep(1);
                        EveAccount accountCurrentlyStarting = Cache.Instance.EveAccountSerializeableSortableBindingList.List.FirstOrDefault(i => i.Pid != 0 && !i.DoneLaunchingEveInstance && i.MyCharacterId != MyCharacterId);
                        if (DebugScheduler) Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "] cannot be started. We are still waiting for [" + accountCurrentlyStarting.MaskedCharacterName + "] to finish launching eve");
                        return false;
                    }

                    try
                    {
                        if (!HWSettings.HWSettingsAreProperlyDefined)
                        {
                            Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "] This account returned HWSettingsAreProperlyDefined = [ False ] aborting login");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("Has your (spoofed) Hardware been defined for this account?");
                        Cache.Instance.Log("Exception [" + ex + "]");
                    }

                    //
                    // only check for a character name if Autostart is true. This allows us to have a blank character name while setting up a toon!
                    //
                    if (UseScheduler && string.IsNullOrEmpty(CharacterName))
                    {
                        UseScheduler = false;
                        Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "] A character name is required and was not found: UseScheduler is now False");
                        return false;
                    }

                    if (string.IsNullOrEmpty(AccountName))
                    {
                        UseScheduler = false;
                        Cache.Instance.Log("[" + Num + "] An account name is required and was not found: UseScheduler is now False");
                        return false;
                    }

                    if (string.IsNullOrEmpty(Password))
                    {
                        UseScheduler = false;
                        Cache.Instance.Log("[" + Num + "] A password for the account required and was not found: : UseScheduler is now False");
                        return false;
                    }

                    if (HWSettings == null || HWSettings.Proxy == null)
                    {
                        UseScheduler = false;
                        Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "] HWSettings or HWSettings.Proxy == null. Error.");
                        return false;
                    }

                    if (HWSettings == null || string.IsNullOrWhiteSpace(HWSettings.Computername) ||
                        (HWSettings != null && string.IsNullOrWhiteSpace(HWSettings.Computername)))
                    {
                        UseScheduler = false;
                        Cache.Instance.Log(
                            "[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "] Hardware profile usage is now required. Please setup a different hardware profile for each account or use the same profile for accounts you want to link together.");
                        return false;
                    }

                    if (string.IsNullOrEmpty(HWSettings.LauncherMachineHash))
                    {
                        UseScheduler = false;
                        Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "] LauncherMachineHash missing. You need to open the HWProfile form once to generate a LauncherMachineHash.");
                        return false;
                    }

                    if (string.IsNullOrEmpty(HWSettings.GpuManufacturer) || HWSettings.GpuDriverDate == DateTime.MinValue)
                    {
                        UseScheduler = false;
                        Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "] You need to generate a new GPU profile for this account.");
                        return false;
                    }

                    if (!HWSettings.Proxy.SafeToAttemptEveSso)
                    {
                        Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "] if (!HWSettings.Proxy.SafeToAttemptEveSso) aborting login attempt: slow down there buddy");
                        return false;
                    }

                    if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(a => !string.IsNullOrEmpty(a.AccountName) && !string.IsNullOrEmpty(a.CharacterName) && a.Pid != 0 && !a.DoneLaunchingEveInstance) && !AllowSimultaneousLogins)
                    {
                        Thread.Sleep(1);
                        EveAccount ToonStillLoggingIn = Cache.Instance.EveAccountSerializeableSortableBindingList.List.FirstOrDefault(a => a.Pid != 0 && !a.DoneLaunchingEveInstance);
                        Cache.Instance.Log("Waiting for [" + ToonStillLoggingIn.MaskedCharacterName + "] to finish launching before attempting to login [" + MaskedCharacterName + "]");
                        //NextEveManagerStartEveForTheseAccountsThreadTimeStamp = DateTime.UtcNow.AddSeconds(20);
                        return false;
                    }

                    if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(a => !string.IsNullOrEmpty(a.AccountName) && !string.IsNullOrEmpty(a.CharacterName) && a.Pid != 0 && a.GUID == GUID))
                    {
                        Thread.Sleep(1);
                        EveAccount OtherToonOnAccountLoggedIn = Cache.Instance.EveAccountSerializeableSortableBindingList.List.FirstOrDefault(a => a.Pid != 0 && a.AccountName == AccountName);
                        if (DebugScheduler) Cache.Instance.Log("[" + OtherToonOnAccountLoggedIn.MaskedCharacterName + "] Is already logged in (same GUID): Aborting");
                        //NextEveManagerStartEveForTheseAccountsThreadTimeStamp = DateTime.UtcNow.AddSeconds(20);
                        return false;
                    }

                    if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(a => !string.IsNullOrEmpty(a.AccountName) && !string.IsNullOrEmpty(a.CharacterName) && a.Pid != 0 && a.AccountName == AccountName && a.ConnectToTestServer == ConnectToTestServer))
                    {
                        Thread.Sleep(1);
                        EveAccount OtherToonOnAccountLoggedIn = Cache.Instance.EveAccountSerializeableSortableBindingList.List.FirstOrDefault(a => a.Pid != 0 && a.AccountName == AccountName);
                        if (DebugScheduler) Cache.Instance.Log("[" + OtherToonOnAccountLoggedIn.MaskedCharacterName + "] Is on the same account as [" + MaskedCharacterName + "] and is logged in: Pid [" + OtherToonOnAccountLoggedIn.Pid + "]: Aborting");
                        //NextEveManagerStartEveForTheseAccountsThreadTimeStamp = DateTime.UtcNow.AddSeconds(20);
                        return false;
                    }

                    if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(a => !string.IsNullOrEmpty(a.AccountName) && !string.IsNullOrEmpty(a.CharacterName) && a.Pid != 0 && a.CharacterName == CharacterName && a.ConnectToTestServer == ConnectToTestServer))
                    {
                        Thread.Sleep(1);
                        EveAccount OtherToonOnAccountLoggedIn = Cache.Instance.EveAccountSerializeableSortableBindingList.List.FirstOrDefault(a => a.Pid != 0 && a.AccountName == AccountName);
                        Cache.Instance.Log("[" + OtherToonOnAccountLoggedIn.MaskedCharacterName + "] Is the same character! (different line in evesharp launcher?) and is logged in: Aborting");
                        //NextEveManagerStartEveForTheseAccountsThreadTimeStamp = DateTime.UtcNow.AddSeconds(20);
                        return false;
                    }

                    HWSettings.CleanupAllEveLogsForThisWindowsUserProfile();

                    if (RestartOfEveClientNeeded)
                    {
                        if (Util.IntervalInMinutes(45, 45, CharacterName) || DebugScheduler)
                            Cache.Instance.Log("ShouldBeRunning [true][" + MaskedAccountName + "][" + MaskedCharacterName + "] RestartOfEveClientNeeded is [" + RestartOfEveClientNeeded + "]");
                        ShouldldBeRunningReason = "RestartOfEveClientNeeded";
                        return true;
                    }

                    if (TestingEmergencyReLogin)
                    {
                        if (Util.IntervalInMinutes(45, 45, CharacterName) || DebugScheduler)
                            Cache.Instance.Log("ShouldBeRunning [true] [" + MaskedAccountName + "][" + MaskedCharacterName + "] TestingEmergencyReLogin is [" + TestingEmergencyReLogin + "]");
                        ShouldldBeRunningReason = "TestingEmergencyReLogin";
                        return true;
                    }

                    if (InMission && !string.IsNullOrEmpty(SelectedController) && SelectedController == nameof(AvailableControllers.QuestorController)) //&& LastInteractedWithEVE.AddMinutes(10) > DateTime.UtcNow)
                    {
                        if (Util.IntervalInMinutes(45, 45, CharacterName) || DebugScheduler)
                            Cache.Instance.Log("ShouldBeRunning [true] [" + MaskedAccountName + "][" + MaskedCharacterName + "] if ((InAbyssalDeadspace || InMission) && LastInteractedWithEVE.AddMinutes(10) > DateTime.UtcNow)");
                        ShouldldBeRunningReason = "InMission";
                        return true;
                    }

                    if (IsInAbyss) //&& LastInteractedWithEVE.AddMinutes(10) > DateTime.UtcNow)
                    {
                        if (Util.IntervalInMinutes(45, 45, CharacterName) || DebugScheduler)
                            Cache.Instance.Log("ShouldBeRunning [true] Account [" + MaskedAccountName + "] Character [" + MaskedCharacterName + "] if ((InAbyssalDeadspace || InMission) && LastInteractedWithEVE.AddMinutes(10) > DateTime.UtcNow)");
                        ShouldldBeRunningReason = "InAbyssalDeadspace";
                        return true;
                    }

                    if (StartsPast24H > MAX_STARTS && !IgnoreSeralizationErrors)
                    {
                        if (Util.IntervalInMinutes(45, 45, CharacterName) || DebugScheduler)
                            Cache.Instance.Log("ShouldBeRunning [false] Account [" + MaskedAccountName + "] Character [" + MaskedCharacterName + "] if (StartsPast24H [" + StartsPast24H + "] > MAX_STARTS [" + MAX_STARTS + "])");
                        ShouldldBeRunningReason = string.Empty;
                        return false;
                    }

                    if (!IsInAbyss)
                    {
                        //
                        // If we are at war and its not yet time to check for wars again
                        //
                        if (IsAtWar)
                        {
                            if (Util.IntervalInMinutes(45, 45, CharacterName) || DebugScheduler)
                            {
                                Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] IsAtWar [" + IsAtWar + "] NextWarCheck [" + NextWarCheck + "]");
                            }

                            if (DateTime.UtcNow > NextWarCheck)
                            {
                                //do nothing, potentially login
                            }
                            else return false;
                        }

                        if (!IsOmegaClone && RequireOmegaClone)
                        {
                            if (Util.IntervalInMinutes(45, 45, CharacterName) || DebugScheduler)
                            {
                                Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] IsOmegaClone [" + IsOmegaClone + "] RequireOmegaClone [" + RequireOmegaClone + "]");
                                _nextOmegaCloneLogMessage = DateTime.UtcNow.AddHours(8);
                            }

                            return false;
                        }
                    }

                    if (UseScheduler)
                    {
                        //
                        // is this account scheduled to start using the scheduler (Is_Scheduler_Active)
                        //
                        foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(x => !string.IsNullOrEmpty(x.AccountName) && !string.IsNullOrEmpty(x.CharacterName) && x.GUID != GUID))
                        {
                            Thread.Sleep(1);
                            if (eA.Pid != 0 && eA.AccountName == AccountName && eA.CharacterName != CharacterName)
                            {
                                if (eA.LastInteractedWithEVE.AddMinutes(2) > DateTime.UtcNow)
                                {
                                    //
                                    // if we have another configured toon on the same account actually running...
                                    //
                                    if (Util.IntervalInMinutes(45, 45, CharacterName) || DebugScheduler)
                                        Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] another character [" + eA.MaskedCharacterName + "] on the same account has been logged in within the last 2 min! ");

                                    return false;
                                }
                            }
                        }
                    }

                    if (!UseScheduler)
                    {
                        if (DebugScheduler) Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] if (!UseScheduler)");
                        return false;
                    }

                    if (IsItDuringDowntimeNow)
                    {
                        if (DebugScheduler) Cache.Instance.Log("Downtime is less than 25 minutes from now: waiting");
                        return false;
                    }

                    //new scheduler
                    if (PatternManagerEnabled)
                    {
                        if (PatternEval.IsAnyPatternMatchingDatetime(this, DateTime.UtcNow))
                        {
                            Cache.Instance.Log("ShouldBeRunning [true] Account [" + MaskedAccountName + "] Character [" + MaskedCharacterName + "] if (PatternEval.IsAnyPatternMatchingDatetime(this.FilledPattern, DateTime.UtcNow))");
                            ShouldldBeRunningReason = " CurrentTime [" + DateTime.UtcNow.ToShortTimeString() + "] matches pattern [" + Pattern + "]";
                            return true;
                        }
                        else if (DebugScheduler) Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] !if (PatternEval.IsAnyPatternMatchingDatetime(FilledPattern, DateTime.UtcNow))");

                        return false;
                    }

                    //
                    // old scheduler
                    //
                    if ((IsItPastStartTime && !IsItPastEndTime) || HoursPerDay == 24)
                    {
                        Cache.Instance.Log("ShouldBeRunning [true] Account [" + MaskedAccountName + "] Character [" + MaskedCharacterName + "] if (DateTime.UtcNow >= GetStartTime && DateTime.UtcNow <= GetEndTime)");
                        ShouldldBeRunningReason = " CurrentTime [" + DateTime.UtcNow.ToShortTimeString() + "] is between [" + StartTime.ToShortTimeString() + "] and [" + EndTime.ToShortTimeString() + "]";
                        return true;
                    }
                    else if (DebugScheduler) Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] !if (DateTime.UtcNow [" + DateTime.UtcNow.ToShortDateString() + "][" + DateTime.UtcNow.ToShortTimeString() + "] >= GetStartTime [" + StartTime.ToShortDateString() + "][" + StartTime.ToShortTimeString() + "] && DateTime.UtcNow [" + DateTime.UtcNow.ToShortDateString() + "][" + DateTime.UtcNow.ToShortTimeString() + "] <= GetEndTime [" + EndTime.ToShortDateString() + "][" + EndTime.ToShortTimeString() + "])");

                    return false;
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return false;
                }
            }
        }

        internal bool IsItPastStartTime
        {
            get
            {
                if (DateTime.UtcNow >= StartTime)
                {
                    if (DebugScheduler) Cache.Instance.Log("[" + MaskedCharacterName + "] IsItPastStartTime [" + true + "] if (DateTime.UtcNow [" + DateTime.UtcNow + "] >= StartTime [" + StartTime + "])");
                    return true;
                }

                return false;
            }
        }

        internal bool IsItPastEndTime
        {
            get
            {
                if (DateTime.UtcNow >= EndTime)
                {
                    if (DebugScheduler) Cache.Instance.Log("[" + MaskedCharacterName + "] IsItPastEndTime [" + true + "] if (DateTime.UtcNow [" + DateTime.UtcNow + "] >= EndTime [" + EndTime + "])");
                    return true;
                }

                return false;
            }
        }

        [Browsable(false)]
        [XmlIgnore]
        internal bool IsItDuringDowntimeNow
        {
            get
            {
                if (ConnectToTestServer)
                {
                    //
                    // does this need adjustment for daylight savings time?!
                    //
                    if (DateTime.UtcNow.Hour == 4 && DateTime.UtcNow.Minute > 38)
                    {
                        Cache.Instance.Log("IsItDuringDowntimeNow [" + true + "] ConnectToTestServer [" + ConnectToTestServer + "] if (DateTime.UtcNow.Hour == 5 && DateTime.UtcNow.Minute > 35)");

                        if (IgnoreDowntime)
                        {
                            Cache.Instance.Log("IsItDuringDowntimeNow: IgnoreDowntime [" + true + "]");
                            return false;
                        }

                        return true;
                    }

                    return false;
                }

                //
                // Broken?
                //
                //if (22 > ESCache.Instance.DirectEve.Me.TimeTillDownTime.TotalMinutes)
                //{
                //    Log.WriteLine("IsItDuringDowntimeNow [" + true + "] if (22 > ESCache.Instance.DirectEve.Me.TimeTillDownTime.TotalMinutes [" + ESCache.Instance.DirectEve.Me.TimeTillDownTime.TotalMinutes + "] ServerShutDownTime [" + ESCache.Instance.DirectEve.Me.ServerShutDownTime + "] )");
                //    return true;
                //}

                if (DateTime.UtcNow.Hour == 10 && DateTime.UtcNow.Minute > 38)
                {
                    Cache.Instance.Log("IsItDuringDowntimeNow [" + true + "] if (DateTime.UtcNow.Hour == 10 && DateTime.UtcNow.Minute > 38)");

                    if (IgnoreDowntime)
                    {
                        Cache.Instance.Log("IsItDuringDowntimeNow: IgnoreDowntime [" + true + "]");
                        return false;
                    }

                    return true;
                }

                return false;
            }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter10ChracterId
        {
            get { return GetValue(() => SlaveCharacter10ChracterId); }
            set { SetValue(() => SlaveCharacter10ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter10RepairGroup
        {
            get { return GetValue(() => SlaveCharacter10RepairGroup); }
            set { SetValue(() => SlaveCharacter10RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter11ChracterId
        {
            get { return GetValue(() => SlaveCharacter11ChracterId); }
            set { SetValue(() => SlaveCharacter11ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter11RepairGroup
        {
            get { return GetValue(() => SlaveCharacter11RepairGroup); }
            set { SetValue(() => SlaveCharacter11RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter12ChracterId
        {
            get { return GetValue(() => SlaveCharacter12ChracterId); }
            set { SetValue(() => SlaveCharacter12ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter12RepairGroup
        {
            get { return GetValue(() => SlaveCharacter12RepairGroup); }
            set { SetValue(() => SlaveCharacter12RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter13ChracterId
        {
            get { return GetValue(() => SlaveCharacter13ChracterId); }
            set { SetValue(() => SlaveCharacter13ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter13RepairGroup
        {
            get { return GetValue(() => SlaveCharacter13RepairGroup); }
            set { SetValue(() => SlaveCharacter13RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter14ChracterId
        {
            get { return GetValue(() => SlaveCharacter14ChracterId); }
            set { SetValue(() => SlaveCharacter14ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter14RepairGroup
        {
            get { return GetValue(() => SlaveCharacter14RepairGroup); }
            set { SetValue(() => SlaveCharacter14RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter15ChracterId
        {
            get { return GetValue(() => SlaveCharacter15ChracterId); }
            set { SetValue(() => SlaveCharacter15ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter15RepairGroup
        {
            get { return GetValue(() => SlaveCharacter15RepairGroup); }
            set { SetValue(() => SlaveCharacter15RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter16ChracterId
        {
            get { return GetValue(() => SlaveCharacter16ChracterId); }
            set { SetValue(() => SlaveCharacter16ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter16RepairGroup
        {
            get { return GetValue(() => SlaveCharacter16RepairGroup); }
            set { SetValue(() => SlaveCharacter16RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter17ChracterId
        {
            get { return GetValue(() => SlaveCharacter17ChracterId); }
            set { SetValue(() => SlaveCharacter17ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter17RepairGroup
        {
            get { return GetValue(() => SlaveCharacter17RepairGroup); }
            set { SetValue(() => SlaveCharacter17RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter18ChracterId
        {
            get { return GetValue(() => SlaveCharacter18ChracterId); }
            set { SetValue(() => SlaveCharacter18ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter18RepairGroup
        {
            get { return GetValue(() => SlaveCharacter18RepairGroup); }
            set { SetValue(() => SlaveCharacter18RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter19ChracterId
        {
            get { return GetValue(() => SlaveCharacter19ChracterId); }
            set { SetValue(() => SlaveCharacter19ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter19RepairGroup
        {
            get { return GetValue(() => SlaveCharacter19RepairGroup); }
            set { SetValue(() => SlaveCharacter19RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter20ChracterId
        {
            get { return GetValue(() => SlaveCharacter20ChracterId); }
            set { SetValue(() => SlaveCharacter20ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter20RepairGroup
        {
            get { return GetValue(() => SlaveCharacter20RepairGroup); }
            set { SetValue(() => SlaveCharacter20RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter21ChracterId
        {
            get { return GetValue(() => SlaveCharacter21ChracterId); }
            set { SetValue(() => SlaveCharacter21ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter21RepairGroup
        {
            get { return GetValue(() => SlaveCharacter21RepairGroup); }
            set { SetValue(() => SlaveCharacter21RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter22ChracterId
        {
            get { return GetValue(() => SlaveCharacter22ChracterId); }
            set { SetValue(() => SlaveCharacter22ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter22RepairGroup
        {
            get { return GetValue(() => SlaveCharacter22RepairGroup); }
            set { SetValue(() => SlaveCharacter22RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter23ChracterId
        {
            get { return GetValue(() => SlaveCharacter23ChracterId); }
            set { SetValue(() => SlaveCharacter23ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter23RepairGroup
        {
            get { return GetValue(() => SlaveCharacter23RepairGroup); }
            set { SetValue(() => SlaveCharacter23RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter24ChracterId
        {
            get { return GetValue(() => SlaveCharacter24ChracterId); }
            set { SetValue(() => SlaveCharacter24ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter24RepairGroup
        {
            get { return GetValue(() => SlaveCharacter24RepairGroup); }
            set { SetValue(() => SlaveCharacter24RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter25ChracterId
        {
            get { return GetValue(() => SlaveCharacter25ChracterId); }
            set { SetValue(() => SlaveCharacter25ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter25RepairGroup
        {
            get { return GetValue(() => SlaveCharacter25RepairGroup); }
            set { SetValue(() => SlaveCharacter25RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter26ChracterId
        {
            get { return GetValue(() => SlaveCharacter26ChracterId); }
            set { SetValue(() => SlaveCharacter26ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter26RepairGroup
        {
            get { return GetValue(() => SlaveCharacter26RepairGroup); }
            set { SetValue(() => SlaveCharacter26RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter27ChracterId
        {
            get { return GetValue(() => SlaveCharacter27ChracterId); }
            set { SetValue(() => SlaveCharacter27ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter27RepairGroup
        {
            get { return GetValue(() => SlaveCharacter27RepairGroup); }
            set { SetValue(() => SlaveCharacter27RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter28ChracterId
        {
            get { return GetValue(() => SlaveCharacter28ChracterId); }
            set { SetValue(() => SlaveCharacter28ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter28RepairGroup
        {
            get { return GetValue(() => SlaveCharacter28RepairGroup); }
            set { SetValue(() => SlaveCharacter28RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter29ChracterId
        {
            get { return GetValue(() => SlaveCharacter29ChracterId); }
            set { SetValue(() => SlaveCharacter29ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter29RepairGroup
        {
            get { return GetValue(() => SlaveCharacter29RepairGroup); }
            set { SetValue(() => SlaveCharacter29RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter30ChracterId
        {
            get { return GetValue(() => SlaveCharacter30ChracterId); }
            set { SetValue(() => SlaveCharacter30ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter30RepairGroup
        {
            get { return GetValue(() => SlaveCharacter30RepairGroup); }
            set { SetValue(() => SlaveCharacter30RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter1ChracterId
        {
            get { return GetValue(() => SlaveCharacter1ChracterId); }
            set { SetValue(() => SlaveCharacter1ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter1RepairGroup
        {
            get { return GetValue(() => SlaveCharacter1RepairGroup); }
            set { SetValue(() => SlaveCharacter1RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter2ChracterId
        {
            get { return GetValue(() => SlaveCharacter2ChracterId); }
            set { SetValue(() => SlaveCharacter2ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter2RepairGroup
        {
            get { return GetValue(() => SlaveCharacter2RepairGroup); }
            set { SetValue(() => SlaveCharacter2RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter3ChracterId
        {
            get { return GetValue(() => SlaveCharacter3ChracterId); }
            set { SetValue(() => SlaveCharacter3ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter3RepairGroup
        {
            get { return GetValue(() => SlaveCharacter3RepairGroup); }
            set { SetValue(() => SlaveCharacter3RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter4ChracterId
        {
            get { return GetValue(() => SlaveCharacter4ChracterId); }
            set { SetValue(() => SlaveCharacter4ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter4RepairGroup
        {
            get { return GetValue(() => SlaveCharacter4RepairGroup); }
            set { SetValue(() => SlaveCharacter4RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter5ChracterId
        {
            get { return GetValue(() => SlaveCharacter5ChracterId); }
            set { SetValue(() => SlaveCharacter5ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter5RepairGroup
        {
            get { return GetValue(() => SlaveCharacter5RepairGroup); }
            set { SetValue(() => SlaveCharacter5RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter6ChracterId
        {
            get { return GetValue(() => SlaveCharacter6ChracterId); }
            set { SetValue(() => SlaveCharacter6ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter6RepairGroup
        {
            get { return GetValue(() => SlaveCharacter6RepairGroup); }
            set { SetValue(() => SlaveCharacter6RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter7ChracterId
        {
            get { return GetValue(() => SlaveCharacter7ChracterId); }
            set { SetValue(() => SlaveCharacter7ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter7RepairGroup
        {
            get { return GetValue(() => SlaveCharacter7RepairGroup); }
            set { SetValue(() => SlaveCharacter7RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter8ChracterId
        {
            get { return GetValue(() => SlaveCharacter8ChracterId); }
            set { SetValue(() => SlaveCharacter8ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter8RepairGroup
        {
            get { return GetValue(() => SlaveCharacter8RepairGroup); }
            set { SetValue(() => SlaveCharacter8RepairGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacter9ChracterId
        {
            get { return GetValue(() => SlaveCharacter9ChracterId); }
            set { SetValue(() => SlaveCharacter9ChracterId, value); }
        }

        [Browsable(false)]
        public string SlaveCharacter9RepairGroup
        {
            get { return GetValue(() => SlaveCharacter9RepairGroup); }
            set { SetValue(() => SlaveCharacter9RepairGroup, value); }
        }

        /**
        [Browsable(false)]
        public double SlaveCharacter1Shields
        {
            get { return GetValue(() => SlaveCharacter1Shields); }
            set { SetValue(() => SlaveCharacter1Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter1ShieldPct
        {
            get { return GetValue(() => SlaveCharacter1ShieldPct); }
            set { SetValue(() => SlaveCharacter1ShieldPct, value); }
        }
        **/

        /**
        [Browsable(false)]
        public double SlaveCharacter1Armor
        {
            get { return GetValue(() => SlaveCharacter1Armor); }
            set { SetValue(() => SlaveCharacter1Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter1ArmorPct
        {
            get { return GetValue(() => SlaveCharacter1ArmorPct); }
            set { SetValue(() => SlaveCharacter1ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter1Hull
        {
            get { return GetValue(() => SlaveCharacter1Hull); }
            set { SetValue(() => SlaveCharacter1Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter1HullPct
        {
            get { return GetValue(() => SlaveCharacter1HullPct); }
            set { SetValue(() => SlaveCharacter1HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter1Capacitor
        {
            get { return GetValue(() => SlaveCharacter1Capacitor); }
            set { SetValue(() => SlaveCharacter1Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter1CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter1CapacitorPct); }
            set { SetValue(() => SlaveCharacter1CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter1ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter1ShipTypeID); }
            set { SetValue(() => SlaveCharacter1ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter2ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter2ShipTypeID); }
            set { SetValue(() => SlaveCharacter2ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter3ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter3ShipTypeID); }
            set { SetValue(() => SlaveCharacter3ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter4ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter4ShipTypeID); }
            set { SetValue(() => SlaveCharacter4ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter5ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter5ShipTypeID); }
            set { SetValue(() => SlaveCharacter5ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter6ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter6ShipTypeID); }
            set { SetValue(() => SlaveCharacter6ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter7ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter7ShipTypeID); }
            set { SetValue(() => SlaveCharacter7ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter8ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter8ShipTypeID); }
            set { SetValue(() => SlaveCharacter8ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter9ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter9ShipTypeID); }
            set { SetValue(() => SlaveCharacter9ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter10ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter10ShipTypeID); }
            set { SetValue(() => SlaveCharacter10ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter11ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter11ShipTypeID); }
            set { SetValue(() => SlaveCharacter11ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter12ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter12ShipTypeID); }
            set { SetValue(() => SlaveCharacter12ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter13ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter13ShipTypeID); }
            set { SetValue(() => SlaveCharacter13ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter14ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter14ShipTypeID); }
            set { SetValue(() => SlaveCharacter14ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter15ShipTypeID
        {
            get { return GetValue(() => SlaveCharacter15ShipTypeID); }
            set { SetValue(() => SlaveCharacter15ShipTypeID, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter2Shields
        {
            get { return GetValue(() => SlaveCharacter2Shields); }
            set { SetValue(() => SlaveCharacter2Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter2ShieldPct
        {
            get { return GetValue(() => SlaveCharacter2ShieldPct); }
            set { SetValue(() => SlaveCharacter2ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter2Armor
        {
            get { return GetValue(() => SlaveCharacter2Armor); }
            set { SetValue(() => SlaveCharacter2Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter2ArmorPct
        {
            get { return GetValue(() => SlaveCharacter2ArmorPct); }
            set { SetValue(() => SlaveCharacter2ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter2Hull
        {
            get { return GetValue(() => SlaveCharacter2Hull); }
            set { SetValue(() => SlaveCharacter2Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter2HullPct
        {
            get { return GetValue(() => SlaveCharacter2HullPct); }
            set { SetValue(() => SlaveCharacter2HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter2Capacitor
        {
            get { return GetValue(() => SlaveCharacter2Capacitor); }
            set { SetValue(() => SlaveCharacter2Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter2CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter2CapacitorPct); }
            set { SetValue(() => SlaveCharacter2CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter3Shields
        {
            get { return GetValue(() => SlaveCharacter3Shields); }
            set { SetValue(() => SlaveCharacter3Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter3ShieldPct
        {
            get { return GetValue(() => SlaveCharacter3ShieldPct); }
            set { SetValue(() => SlaveCharacter3ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter3Armor
        {
            get { return GetValue(() => SlaveCharacter3Armor); }
            set { SetValue(() => SlaveCharacter3Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter3ArmorPct
        {
            get { return GetValue(() => SlaveCharacter3ArmorPct); }
            set { SetValue(() => SlaveCharacter3ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter3Hull
        {
            get { return GetValue(() => SlaveCharacter3Hull); }
            set { SetValue(() => SlaveCharacter3Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter3HullPct
        {
            get { return GetValue(() => SlaveCharacter3HullPct); }
            set { SetValue(() => SlaveCharacter3HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter3Capacitor
        {
            get { return GetValue(() => SlaveCharacter3Capacitor); }
            set { SetValue(() => SlaveCharacter3Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter3CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter3CapacitorPct); }
            set { SetValue(() => SlaveCharacter3CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter4Shields
        {
            get { return GetValue(() => SlaveCharacter4Shields); }
            set { SetValue(() => SlaveCharacter4Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter4ShieldPct
        {
            get { return GetValue(() => SlaveCharacter4ShieldPct); }
            set { SetValue(() => SlaveCharacter4ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter4Armor
        {
            get { return GetValue(() => SlaveCharacter4Armor); }
            set { SetValue(() => SlaveCharacter4Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter4ArmorPct
        {
            get { return GetValue(() => SlaveCharacter4ArmorPct); }
            set { SetValue(() => SlaveCharacter4ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter4Hull
        {
            get { return GetValue(() => SlaveCharacter4Hull); }
            set { SetValue(() => SlaveCharacter4Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter4HullPct
        {
            get { return GetValue(() => SlaveCharacter4HullPct); }
            set { SetValue(() => SlaveCharacter4HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter4Capacitor
        {
            get { return GetValue(() => SlaveCharacter4Capacitor); }
            set { SetValue(() => SlaveCharacter4Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter4CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter4CapacitorPct); }
            set { SetValue(() => SlaveCharacter4CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter5Shields
        {
            get { return GetValue(() => SlaveCharacter5Shields); }
            set { SetValue(() => SlaveCharacter5Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter5ShieldPct
        {
            get { return GetValue(() => SlaveCharacter5ShieldPct); }
            set { SetValue(() => SlaveCharacter5ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter5Armor
        {
            get { return GetValue(() => SlaveCharacter5Armor); }
            set { SetValue(() => SlaveCharacter5Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter5ArmorPct
        {
            get { return GetValue(() => SlaveCharacter5ArmorPct); }
            set { SetValue(() => SlaveCharacter5ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter5Hull
        {
            get { return GetValue(() => SlaveCharacter5Hull); }
            set { SetValue(() => SlaveCharacter5Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter5HullPct
        {
            get { return GetValue(() => SlaveCharacter5HullPct); }
            set { SetValue(() => SlaveCharacter5HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter5Capacitor
        {
            get { return GetValue(() => SlaveCharacter5Capacitor); }
            set { SetValue(() => SlaveCharacter5Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter5CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter5CapacitorPct); }
            set { SetValue(() => SlaveCharacter5CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter6Shields
        {
            get { return GetValue(() => SlaveCharacter6Shields); }
            set { SetValue(() => SlaveCharacter6Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter6ShieldPct
        {
            get { return GetValue(() => SlaveCharacter6ShieldPct); }
            set { SetValue(() => SlaveCharacter6ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter6Armor
        {
            get { return GetValue(() => SlaveCharacter6Armor); }
            set { SetValue(() => SlaveCharacter6Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter6ArmorPct
        {
            get { return GetValue(() => SlaveCharacter6ArmorPct); }
            set { SetValue(() => SlaveCharacter6ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter6Hull
        {
            get { return GetValue(() => SlaveCharacter6Hull); }
            set { SetValue(() => SlaveCharacter6Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter6HullPct
        {
            get { return GetValue(() => SlaveCharacter6HullPct); }
            set { SetValue(() => SlaveCharacter6HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter6Capacitor
        {
            get { return GetValue(() => SlaveCharacter6Capacitor); }
            set { SetValue(() => SlaveCharacter6Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter6CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter6CapacitorPct); }
            set { SetValue(() => SlaveCharacter6CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter7Shields
        {
            get { return GetValue(() => SlaveCharacter7Shields); }
            set { SetValue(() => SlaveCharacter7Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter7ShieldPct
        {
            get { return GetValue(() => SlaveCharacter7ShieldPct); }
            set { SetValue(() => SlaveCharacter7ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter7Armor
        {
            get { return GetValue(() => SlaveCharacter7Armor); }
            set { SetValue(() => SlaveCharacter7Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter7ArmorPct
        {
            get { return GetValue(() => SlaveCharacter7ArmorPct); }
            set { SetValue(() => SlaveCharacter7ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter7Hull
        {
            get { return GetValue(() => SlaveCharacter7Hull); }
            set { SetValue(() => SlaveCharacter7Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter7HullPct
        {
            get { return GetValue(() => SlaveCharacter7HullPct); }
            set { SetValue(() => SlaveCharacter7HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter7Capacitor
        {
            get { return GetValue(() => SlaveCharacter7Capacitor); }
            set { SetValue(() => SlaveCharacter7Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter7CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter7CapacitorPct); }
            set { SetValue(() => SlaveCharacter7CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter8Shields
        {
            get { return GetValue(() => SlaveCharacter8Shields); }
            set { SetValue(() => SlaveCharacter8Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter8ShieldPct
        {
            get { return GetValue(() => SlaveCharacter8ShieldPct); }
            set { SetValue(() => SlaveCharacter8ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter8Armor
        {
            get { return GetValue(() => SlaveCharacter8Armor); }
            set { SetValue(() => SlaveCharacter8Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter8ArmorPct
        {
            get { return GetValue(() => SlaveCharacter8ArmorPct); }
            set { SetValue(() => SlaveCharacter8ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter8Hull
        {
            get { return GetValue(() => SlaveCharacter8Hull); }
            set { SetValue(() => SlaveCharacter8Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter8HullPct
        {
            get { return GetValue(() => SlaveCharacter8HullPct); }
            set { SetValue(() => SlaveCharacter8HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter8Capacitor
        {
            get { return GetValue(() => SlaveCharacter8Capacitor); }
            set { SetValue(() => SlaveCharacter8Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter8CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter8CapacitorPct); }
            set { SetValue(() => SlaveCharacter8CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter9Shields
        {
            get { return GetValue(() => SlaveCharacter9Shields); }
            set { SetValue(() => SlaveCharacter9Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter9ShieldPct
        {
            get { return GetValue(() => SlaveCharacter9ShieldPct); }
            set { SetValue(() => SlaveCharacter9ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter9Armor
        {
            get { return GetValue(() => SlaveCharacter9Armor); }
            set { SetValue(() => SlaveCharacter9Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter9ArmorPct
        {
            get { return GetValue(() => SlaveCharacter9ArmorPct); }
            set { SetValue(() => SlaveCharacter9ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter9Hull
        {
            get { return GetValue(() => SlaveCharacter9Hull); }
            set { SetValue(() => SlaveCharacter9Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter9HullPct
        {
            get { return GetValue(() => SlaveCharacter9HullPct); }
            set { SetValue(() => SlaveCharacter9HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter9Capacitor
        {
            get { return GetValue(() => SlaveCharacter9Capacitor); }
            set { SetValue(() => SlaveCharacter9Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter9CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter9CapacitorPct); }
            set { SetValue(() => SlaveCharacter9CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter10Shields
        {
            get { return GetValue(() => SlaveCharacter10Shields); }
            set { SetValue(() => SlaveCharacter10Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter10ShieldPct
        {
            get { return GetValue(() => SlaveCharacter10ShieldPct); }
            set { SetValue(() => SlaveCharacter10ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter10Armor
        {
            get { return GetValue(() => SlaveCharacter10Armor); }
            set { SetValue(() => SlaveCharacter10Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter10ArmorPct
        {
            get { return GetValue(() => SlaveCharacter10ArmorPct); }
            set { SetValue(() => SlaveCharacter10ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter10Hull
        {
            get { return GetValue(() => SlaveCharacter10Hull); }
            set { SetValue(() => SlaveCharacter10Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter10HullPct
        {
            get { return GetValue(() => SlaveCharacter10HullPct); }
            set { SetValue(() => SlaveCharacter10HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter10Capacitor
        {
            get { return GetValue(() => SlaveCharacter10Capacitor); }
            set { SetValue(() => SlaveCharacter10Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter10CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter10CapacitorPct); }
            set { SetValue(() => SlaveCharacter10CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter11Shields
        {
            get { return GetValue(() => SlaveCharacter11Shields); }
            set { SetValue(() => SlaveCharacter11Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter11ShieldPct
        {
            get { return GetValue(() => SlaveCharacter11ShieldPct); }
            set { SetValue(() => SlaveCharacter11ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter11Armor
        {
            get { return GetValue(() => SlaveCharacter11Armor); }
            set { SetValue(() => SlaveCharacter11Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter11ArmorPct
        {
            get { return GetValue(() => SlaveCharacter11ArmorPct); }
            set { SetValue(() => SlaveCharacter11ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter11Hull
        {
            get { return GetValue(() => SlaveCharacter11Hull); }
            set { SetValue(() => SlaveCharacter11Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter11HullPct
        {
            get { return GetValue(() => SlaveCharacter11HullPct); }
            set { SetValue(() => SlaveCharacter11HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter11Capacitor
        {
            get { return GetValue(() => SlaveCharacter11Capacitor); }
            set { SetValue(() => SlaveCharacter11Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter11CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter11CapacitorPct); }
            set { SetValue(() => SlaveCharacter11CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter12Shields
        {
            get { return GetValue(() => SlaveCharacter12Shields); }
            set { SetValue(() => SlaveCharacter12Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter12ShieldPct
        {
            get { return GetValue(() => SlaveCharacter12ShieldPct); }
            set { SetValue(() => SlaveCharacter12ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter12Armor
        {
            get { return GetValue(() => SlaveCharacter12Armor); }
            set { SetValue(() => SlaveCharacter12Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter12ArmorPct
        {
            get { return GetValue(() => SlaveCharacter12ArmorPct); }
            set { SetValue(() => SlaveCharacter12ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter12Hull
        {
            get { return GetValue(() => SlaveCharacter12Hull); }
            set { SetValue(() => SlaveCharacter12Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter12HullPct
        {
            get { return GetValue(() => SlaveCharacter12HullPct); }
            set { SetValue(() => SlaveCharacter12HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter12Capacitor
        {
            get { return GetValue(() => SlaveCharacter12Capacitor); }
            set { SetValue(() => SlaveCharacter12Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter12CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter12CapacitorPct); }
            set { SetValue(() => SlaveCharacter12CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter13Shields
        {
            get { return GetValue(() => SlaveCharacter13Shields); }
            set { SetValue(() => SlaveCharacter13Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter13ShieldPct
        {
            get { return GetValue(() => SlaveCharacter13ShieldPct); }
            set { SetValue(() => SlaveCharacter13ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter13Armor
        {
            get { return GetValue(() => SlaveCharacter13Armor); }
            set { SetValue(() => SlaveCharacter13Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter13ArmorPct
        {
            get { return GetValue(() => SlaveCharacter13ArmorPct); }
            set { SetValue(() => SlaveCharacter13ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter13Hull
        {
            get { return GetValue(() => SlaveCharacter13Hull); }
            set { SetValue(() => SlaveCharacter13Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter13HullPct
        {
            get { return GetValue(() => SlaveCharacter13HullPct); }
            set { SetValue(() => SlaveCharacter13HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter13Capacitor
        {
            get { return GetValue(() => SlaveCharacter13Capacitor); }
            set { SetValue(() => SlaveCharacter13Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter13CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter13CapacitorPct); }
            set { SetValue(() => SlaveCharacter13CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter14Shields
        {
            get { return GetValue(() => SlaveCharacter14Shields); }
            set { SetValue(() => SlaveCharacter14Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter14ShieldPct
        {
            get { return GetValue(() => SlaveCharacter14ShieldPct); }
            set { SetValue(() => SlaveCharacter14ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter14Armor
        {
            get { return GetValue(() => SlaveCharacter14Armor); }
            set { SetValue(() => SlaveCharacter14Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter14ArmorPct
        {
            get { return GetValue(() => SlaveCharacter14ArmorPct); }
            set { SetValue(() => SlaveCharacter14ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter14Hull
        {
            get { return GetValue(() => SlaveCharacter14Hull); }
            set { SetValue(() => SlaveCharacter14Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter14HullPct
        {
            get { return GetValue(() => SlaveCharacter14HullPct); }
            set { SetValue(() => SlaveCharacter14HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter14Capacitor
        {
            get { return GetValue(() => SlaveCharacter14Capacitor); }
            set { SetValue(() => SlaveCharacter14Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter14CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter14CapacitorPct); }
            set { SetValue(() => SlaveCharacter14CapacitorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter15Shields
        {
            get { return GetValue(() => SlaveCharacter15Shields); }
            set { SetValue(() => SlaveCharacter15Shields, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter15ShieldPct
        {
            get { return GetValue(() => SlaveCharacter15ShieldPct); }
            set { SetValue(() => SlaveCharacter15ShieldPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter15Armor
        {
            get { return GetValue(() => SlaveCharacter15Armor); }
            set { SetValue(() => SlaveCharacter15Armor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter15ArmorPct
        {
            get { return GetValue(() => SlaveCharacter15ArmorPct); }
            set { SetValue(() => SlaveCharacter15ArmorPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter15Hull
        {
            get { return GetValue(() => SlaveCharacter15Hull); }
            set { SetValue(() => SlaveCharacter15Hull, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter15HullPct
        {
            get { return GetValue(() => SlaveCharacter15HullPct); }
            set { SetValue(() => SlaveCharacter15HullPct, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter15Capacitor
        {
            get { return GetValue(() => SlaveCharacter15Capacitor); }
            set { SetValue(() => SlaveCharacter15Capacitor, value); }
        }

        [Browsable(false)]
        public double SlaveCharacter15CapacitorPct
        {
            get { return GetValue(() => SlaveCharacter15CapacitorPct); }
            set { SetValue(() => SlaveCharacter15CapacitorPct, value); }
        }
        **/

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter1DPSGroup
        {
            get { return GetValue(() => SlaveCharacter1DPSGroup); }
            set { SetValue(() => SlaveCharacter1DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter2DPSGroup
        {
            get { return GetValue(() => SlaveCharacter2DPSGroup); }
            set { SetValue(() => SlaveCharacter2DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter3DPSGroup
        {
            get { return GetValue(() => SlaveCharacter3DPSGroup); }
            set { SetValue(() => SlaveCharacter3DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter4DPSGroup
        {
            get { return GetValue(() => SlaveCharacter4DPSGroup); }
            set { SetValue(() => SlaveCharacter4DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter5DPSGroup
        {
            get { return GetValue(() => SlaveCharacter5DPSGroup); }
            set { SetValue(() => SlaveCharacter5DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter6DPSGroup
        {
            get { return GetValue(() => SlaveCharacter6DPSGroup); }
            set { SetValue(() => SlaveCharacter6DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter7DPSGroup
        {
            get { return GetValue(() => SlaveCharacter7DPSGroup); }
            set { SetValue(() => SlaveCharacter7DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter8DPSGroup
        {
            get { return GetValue(() => SlaveCharacter8DPSGroup); }
            set { SetValue(() => SlaveCharacter8DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter9DPSGroup
        {
            get { return GetValue(() => SlaveCharacter9DPSGroup); }
            set { SetValue(() => SlaveCharacter9DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter10DPSGroup
        {
            get { return GetValue(() => SlaveCharacter10DPSGroup); }
            set { SetValue(() => SlaveCharacter10DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter11DPSGroup
        {
            get { return GetValue(() => SlaveCharacter11DPSGroup); }
            set { SetValue(() => SlaveCharacter11DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter12DPSGroup
        {
            get { return GetValue(() => SlaveCharacter12DPSGroup); }
            set { SetValue(() => SlaveCharacter12DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter13DPSGroup
        {
            get { return GetValue(() => SlaveCharacter13DPSGroup); }
            set { SetValue(() => SlaveCharacter13DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter14DPSGroup
        {
            get { return GetValue(() => SlaveCharacter14DPSGroup); }
            set { SetValue(() => SlaveCharacter14DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter15DPSGroup
        {
            get { return GetValue(() => SlaveCharacter15DPSGroup); }
            set { SetValue(() => SlaveCharacter15DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter16DPSGroup
        {
            get { return GetValue(() => SlaveCharacter16DPSGroup); }
            set { SetValue(() => SlaveCharacter16DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter17DPSGroup
        {
            get { return GetValue(() => SlaveCharacter17DPSGroup); }
            set { SetValue(() => SlaveCharacter17DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter18DPSGroup
        {
            get { return GetValue(() => SlaveCharacter18DPSGroup); }
            set { SetValue(() => SlaveCharacter18DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter19DPSGroup
        {
            get { return GetValue(() => SlaveCharacter19DPSGroup); }
            set { SetValue(() => SlaveCharacter19DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter20DPSGroup
        {
            get { return GetValue(() => SlaveCharacter20DPSGroup); }
            set { SetValue(() => SlaveCharacter20DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter21DPSGroup
        {
            get { return GetValue(() => SlaveCharacter21DPSGroup); }
            set { SetValue(() => SlaveCharacter21DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter22DPSGroup
        {
            get { return GetValue(() => SlaveCharacter22DPSGroup); }
            set { SetValue(() => SlaveCharacter22DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter23DPSGroup
        {
            get { return GetValue(() => SlaveCharacter23DPSGroup); }
            set { SetValue(() => SlaveCharacter23DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter24DPSGroup
        {
            get { return GetValue(() => SlaveCharacter24DPSGroup); }
            set { SetValue(() => SlaveCharacter24DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter25DPSGroup
        {
            get { return GetValue(() => SlaveCharacter25DPSGroup); }
            set { SetValue(() => SlaveCharacter25DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter26DPSGroup
        {
            get { return GetValue(() => SlaveCharacter26DPSGroup); }
            set { SetValue(() => SlaveCharacter26DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter27DPSGroup
        {
            get { return GetValue(() => SlaveCharacter27DPSGroup); }
            set { SetValue(() => SlaveCharacter27DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter28DPSGroup
        {
            get { return GetValue(() => SlaveCharacter28DPSGroup); }
            set { SetValue(() => SlaveCharacter28DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter29DPSGroup
        {
            get { return GetValue(() => SlaveCharacter29DPSGroup); }
            set { SetValue(() => SlaveCharacter29DPSGroup, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        [ReadOnly(true)]
        public int SlaveCharacter30DPSGroup
        {
            get { return GetValue(() => SlaveCharacter30DPSGroup); }
            set { SetValue(() => SlaveCharacter30DPSGroup, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName1
        {
            get { return GetValue(() => SlaveCharacterName1); }
            set { SetValue(() => SlaveCharacterName1, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName10
        {
            get { return GetValue(() => SlaveCharacterName10); }
            set { SetValue(() => SlaveCharacterName10, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName11
        {
            get { return GetValue(() => SlaveCharacterName11); }
            set { SetValue(() => SlaveCharacterName11, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName12
        {
            get { return GetValue(() => SlaveCharacterName12); }
            set { SetValue(() => SlaveCharacterName12, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName13
        {
            get { return GetValue(() => SlaveCharacterName13); }
            set { SetValue(() => SlaveCharacterName13, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName14
        {
            get { return GetValue(() => SlaveCharacterName14); }
            set { SetValue(() => SlaveCharacterName14, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName15
        {
            get { return GetValue(() => SlaveCharacterName15); }
            set { SetValue(() => SlaveCharacterName15, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName16
        {
            get { return GetValue(() => SlaveCharacterName16); }
            set { SetValue(() => SlaveCharacterName16, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName17
        {
            get { return GetValue(() => SlaveCharacterName17); }
            set { SetValue(() => SlaveCharacterName17, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName18
        {
            get { return GetValue(() => SlaveCharacterName18); }
            set { SetValue(() => SlaveCharacterName18, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName19
        {
            get { return GetValue(() => SlaveCharacterName19); }
            set { SetValue(() => SlaveCharacterName19, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName20
        {
            get { return GetValue(() => SlaveCharacterName20); }
            set { SetValue(() => SlaveCharacterName20, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName21
        {
            get { return GetValue(() => SlaveCharacterName21); }
            set { SetValue(() => SlaveCharacterName21, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName22
        {
            get { return GetValue(() => SlaveCharacterName22); }
            set { SetValue(() => SlaveCharacterName22, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName23
        {
            get { return GetValue(() => SlaveCharacterName23); }
            set { SetValue(() => SlaveCharacterName23, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName24
        {
            get { return GetValue(() => SlaveCharacterName24); }
            set { SetValue(() => SlaveCharacterName24, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName25
        {
            get { return GetValue(() => SlaveCharacterName25); }
            set { SetValue(() => SlaveCharacterName25, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName26
        {
            get { return GetValue(() => SlaveCharacterName26); }
            set { SetValue(() => SlaveCharacterName26, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName27
        {
            get { return GetValue(() => SlaveCharacterName27); }
            set { SetValue(() => SlaveCharacterName27, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName28
        {
            get { return GetValue(() => SlaveCharacterName28); }
            set { SetValue(() => SlaveCharacterName28, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName29
        {
            get { return GetValue(() => SlaveCharacterName29); }
            set { SetValue(() => SlaveCharacterName29, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName30
        {
            get { return GetValue(() => SlaveCharacterName30); }
            set { SetValue(() => SlaveCharacterName30, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName2
        {
            get { return GetValue(() => SlaveCharacterName2); }
            set { SetValue(() => SlaveCharacterName2, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName3
        {
            get { return GetValue(() => SlaveCharacterName3); }
            set { SetValue(() => SlaveCharacterName3, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName4
        {
            get { return GetValue(() => SlaveCharacterName4); }
            set { SetValue(() => SlaveCharacterName4, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName5
        {
            get { return GetValue(() => SlaveCharacterName5); }
            set { SetValue(() => SlaveCharacterName5, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName6
        {
            get { return GetValue(() => SlaveCharacterName6); }
            set { SetValue(() => SlaveCharacterName6, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName7
        {
            get { return GetValue(() => SlaveCharacterName7); }
            set { SetValue(() => SlaveCharacterName7, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName8
        {
            get { return GetValue(() => SlaveCharacterName8); }
            set { SetValue(() => SlaveCharacterName8, value); }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public string SlaveCharacterName9
        {
            get { return GetValue(() => SlaveCharacterName9); }
            set { SetValue(() => SlaveCharacterName9, value); }
        }

        [ReadOnly(true)]
        public string SolarSystem
        {
            get { return GetValue(() => SolarSystem); }
            set { SetValue(() => SolarSystem, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public float StandingUsedToAccessAgent
        {
            get { return GetValue(() => StandingUsedToAccessAgent); }
            set { SetValue(() => StandingUsedToAccessAgent, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public float StartEvePriorityLevel
        {
            get
            {
                if (ShipType == "Capsule")
                    return EveAccountStartPriority.InCapsulePriority;

                if (IsInAbyss)
                    return EveAccountStartPriority.InAbyssalDeadspacePriority;

                if (InMission)
                    return EveAccountStartPriority.InMissionPriority;

                if (RestartOfEveClientNeeded)
                    return EveAccountStartPriority.HighPriority;

                if (IsDocked && ShipType != "Capsule" && AllLoggedInAccountsAreNotInLocalWhereIWillLogin)
                    return EveAccountStartPriority.DockedAndAloneInLocalPriority;

                return EveAccountStartPriority.NormalPriority;
            }
        }

        public int StartHour
        {
            get { return GetValue(() => StartHour); }
            set
            {
                try
                {
                    SetValue(() => StartHour, value);
                    GenerateNewTimeSpans();
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                }
            }
        }

        [Browsable(false)]
        public int StartHourII
        {
            get { return GetValue(() => StartHourII); }
            set
            {
                SetValue(() => StartHourII, value);
                GenerateNewTimeSpans();
            }
        }

        [Browsable(false)]
        public DateTime StartingTokenTime
        {
            get { return GetValue(() => StartingTokenTime); }
            set { SetValue(() => StartingTokenTime, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        public TimeSpan StartingTokenTimespan
        {
            get { return GetValue(() => StartingTokenTimespan); }
            set { SetValue(() => StartingTokenTimespan, value); }
        }

        [Browsable(false)]
        [XmlElement("StartingTokenTimespan")]
        public long StartingTokenTimespanWrapper
        {
            get => StartingTokenTimespan.Ticks;
            set => StartingTokenTimespan = TimeSpan.FromTicks(value);
        }

        [XmlIgnore]
        public int StartsPast24H
        {
            get { return GetValue(() => StartsPast24H); }
            set { SetValue(() => StartsPast24H, value); }
        }

        [XmlIgnore]
        public DateTime StartTime
        {
            get
            {
                if (UseFleetMgr && UseScheduler && !IsLeader && !string.IsNullOrEmpty(FleetName))
                {
                    //Copy schedule from the leader!
                    foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(i => i.GUID != GUID && i.UseFleetMgr && i.IsLeader && FleetName == i.FleetName))
                    {
                        return eA.StartTime; //there should be only one leader per fleet
                    }
                }

                return GetValue(() => StartTime);
            }
            set { SetValue(() => StartTime, value); }
        }

        [XmlIgnore]
        public DateTime LastCachedStartTimeStillValid
        {
            get { return GetValue(() => LastCachedStartTimeStillValid); }
            set { SetValue(() => LastCachedStartTimeStillValid, value); }
        }

        public bool IsCachedStartTimeStillValid
        {
            get
            {
                try
                {
                    //bool DebugIsCachedStartTimeStillValid = true;

                    //if (UseScheduler)
                    //    return false;
                    //
                    // Is StartTime been initialized yet?
                    //
                    if (StartTime == DateTime.MinValue)
                    {
                        //Cache.Instance.Log("IsCachedStartTimeStillValid: [" + AccountName + "][" + CharacterName + "] if (StartTime == DateTime.MinValue)");
                        return false;
                    }

                    if (StartTime == null)
                    {
                        //Cache.Instance.Log("IsCachedStartTimeStillValid: [" + AccountName + "][" + CharacterName + "] if (StartTime == null)");
                        return false;
                    }

                    //
                    // reasons to always keep the old StartTime we previously cached
                    //

                    //if (IsInAbyss)
                    //{
                    //    LastCachedStartTimeStillValid = DateTime.UtcNow;
                    //    //Cache.Instance.Log("IsCachedStartTimeStillValid: [" + AccountName + "][" + CharacterName + "] InAbyssalDeadspace");
                    //    return true;
                    //}

                    //if (InMission)
                    //{
                    //    LastCachedStartTimeStillValid = DateTime.UtcNow;
                    //    //Cache.Instance.Log("IsCachedStartTimeStillValid: [" + AccountName + "][" + CharacterName + "] InMission");
                    //    return true;
                    //}

                    //
                    // reasons to recalculate StartTime
                    //

                    //
                    // It is past the scheduled hour and if we count back the number of hours we are allowed to run (+1)
                    // and it is after the StartTime then we should reset StartTime to be in the future (next day)
                    //

                    if (!IsItPastEndTime)
                    {
                        //Cache.Instance.Log("IsCachedStartTimeStillValid: [" + AccountName + "][" + CharacterName + "] if (DateTime.UtcNow > StartTime.AddHours(HoursPerDay) && !EveProcessExists)");
                        return false;
                    }

                    LastCachedStartTimeStillValid = DateTime.UtcNow;
                    //if (DebugIsCachedStartTimeStillValid) Cache.Instance.Log("IsCachedStartTimeStillValid: true");
                    return true;
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return false;
                }
            }
        }

        private DateTime LastGenerateStartTime = DateTime.MinValue;

        private void GenerateStartTime()
        {
            try
            {
                if (PatternManagerEnabled)
                    return;

                if (LastGenerateStartTime.AddSeconds(Util.GetRandom(10, 15)) > DateTime.UtcNow)
                {
                    if (Util.IntervalInMinutes(5, 5, CharacterName) || DebugScheduler)
                        Cache.Instance.Log("if (LastGenerateStartTime.AddSeconds(Util.GetRandom(10, 15)) > DateTime.UtcNow)");
                }
                //    return;

                LastGenerateStartTime = DateTime.UtcNow;

                int StartMinutes = rnd.Next(5, 20);
                StartTime = DateTime.UtcNow.Date.AddHours(StartHour);
                StartTime = StartTime.AddMinutes(StartMinutes);
                DateTime tempEndTimePlus2HoursCalc = StartTime.AddHours(HoursPerDay);

                if (DateTime.UtcNow > tempEndTimePlus2HoursCalc)
                {
                    StartTime.AddDays(1);
                }

                if (Util.IntervalInMinutes(5, 5, CharacterName) || DebugScheduler)
                    Cache.Instance.Log("GenerateNewTimeSpans: [" + MaskedCharacterName + "] StartHour [" + StartHour + "] StartMinutes [" + StartMinutes + "] StartTime [" + StartTime.ToShortDateString() + "][" + StartTime.ToLongTimeString() + "] EndTime [" + EndTime.ToShortDateString() + "][" + EndTime.ToLongTimeString() + "] tempEndTimePlus2HoursCalc [" + tempEndTimePlus2HoursCalc.ToShortDateString() + "][" + tempEndTimePlus2HoursCalc.ToLongTimeString() + "]");
                //
                // should we make sure the start time is unique(ish) across toons? We can...
                //
                return;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        public DateTime SubEnd
        {
            get { return GetValue(() => SubEnd); }
            set { SetValue(() => SubEnd, value); }
        }

        [Browsable(false)]
        public bool TestingEmergencyReLogin
        {
            get { return GetValue(() => TestingEmergencyReLogin); }
            set { SetValue(() => TestingEmergencyReLogin, value); }
        }

        //[Browsable(false)]
        //public bool AllowHideEveWindow = false;
        //{
        //    get { return GetValue(() => AllowHideEveWindow); }
        //    set { SetValue(() => AllowHideEveWindow, value); }
        //}
        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public double TotalValue
        {
            get
            {
                double _totalValue = WalletBalance + LpValue + ItemHangarValue;
                return _totalValue;
            }
        }

        [ReadOnly(true)]
        public bool TrainingNow
        {
            get { return GetValue(() => TrainingNow); }
            set { SetValue(() => TrainingNow, value); }
        }

        [Browsable(false)]
        public string UniqueID
        {
            get
            {
                string val = GetValue(() => UniqueID);
                if (string.IsNullOrEmpty(val))
                {
                    if (string.IsNullOrEmpty(Salt))
                        Salt = Path.GetRandomFileName().Replace(".", "");
                    val = Util.Sha256(Util.Sha256(CharacterName + AccountName) + Salt);
                    SetValue(() => UniqueID, val);
                }
                return val;
            }
            set { SetValue(() => UniqueID, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public string WindowsUserProfilePath
        {
            get
            {
                return "C:\\Users\\" + HWSettings.WindowsUserLogin + "\\";
            }
        }

        [Browsable(true)]
        public bool UseFleetMgr
        {
            get { return GetValue(() => UseFleetMgr); }
            set { SetValue(() => UseFleetMgr, value); }
        }

        public bool UseScheduler
        {
            get { return GetValue(() => UseScheduler); }
            set { SetValue(() => UseScheduler, value); }
        }

        public double WalletBalance
        {
            get { return GetValue(() => WalletBalance); }
            set { SetValue(() => WalletBalance, value); }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        private string ExeFileFullPath
        {
            get
            {
                string tempEveDirectory = Cache.Instance.EveSettings.EveDirectory;
                if (ConnectToTestServer)
                    tempEveDirectory = tempEveDirectory.Replace("tq", "sisi");

                tempEveDirectory = tempEveDirectory.Replace("bin", "bin64");
                tempEveDirectory = tempEveDirectory.Replace("6464", "64");

                return tempEveDirectory;
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        private bool MyEveAccountIsAlreadyLoggedIn
        {
            get
            {
                try
                {
                    foreach (EveAccount thisEveAccount in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(i => !string.IsNullOrEmpty(i.AccountName) && !string.IsNullOrEmpty(i.CharacterName)))
                    {
                        Thread.Sleep(1);
                        if (thisEveAccount.GUID != GUID && thisEveAccount.AccountName == AccountName && thisEveAccount.Pid != 0 && thisEveAccount.ConnectToTestServer == ConnectToTestServer)
                            return true;

                        continue;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return false;
                }
            }
        }

        #endregion Properties

        #region Methods

        public bool ShouldWeKillEveIfProcessNotAlive
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(CharacterName))
                        return false;

                    if (IsInAbyss)
                        return true;

                    if (!Cache.Instance.EveSettings.KillUnresponsiveEvEs ?? true)
                        return false;

                    if (ManuallyPausedViaUI)
                        return false;

                    if (ManuallyStarted)
                        return false;

                    return true;
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return false;
                }
            }
        }

        public bool CheckEvents()
        {
            if (SelectedController == nameof(AvailableControllers.CombatDontMoveController))
                return true;

            foreach (DirectEvents directEvent in Enum.GetValues(typeof(DirectEvents)))
            {
                int value = (int)directEvent;

                if (value < 0)
                    continue;

                if (!ManuallyPausedViaUI && HasLastDirectEvent(directEvent) && GetLastDirectEvent(directEvent).Value < DateTime.UtcNow.AddMinutes(-Math.Abs(value)))
                {
                    Cache.Instance.Log(string.Format("Stopping account [{0}] because DirectEvent [{1}] hasn't been received within [{2}] minutes.", MaskedAccountName,
                        directEvent, value));
                    return false;
                }
            }
            return true;
        }

        public void CleanupSpecificEveSharpBotLogsForThisEveAccount(string LogSubFolder = "Console", string FileExtensionWithDot = ".log")
        {
            string eveSharpBotLogDirectoryPath = BotLogpath + LogSubFolder;
            if (FileExtensionWithDot != ".csv" && FileExtensionWithDot != ".log")
            {
                Cache.Instance.Log("if (FileExtension != .csv && FileExtension != .log) FileExtension = .log;");
                FileExtensionWithDot = ".log";
            }

            if (!Directory.Exists(eveSharpBotLogDirectoryPath))
            {
                Cache.Instance.Log("Directory: [" + eveSharpBotLogDirectoryPath + "] does not exist.");
                return;
            }

            Cache.Instance.Log("EVE [ " + LogSubFolder + " ] path: [" + eveSharpBotLogDirectoryPath + "] found");

            if (Directory.Exists(eveSharpBotLogDirectoryPath))
            {
                try
                {
                    string FileExtensionWithGlobAndDot = "*" + FileExtensionWithDot;
                    List<FileInfo> EveSharpBotTextFiles = Directory.GetFiles(eveSharpBotLogDirectoryPath, FileExtensionWithGlobAndDot).Select(f => new FileInfo(f)).Where(f => f.LastWriteTimeUtc < DateTime.UtcNow.AddMonths(-1)).ToList();
                    if (EveSharpBotTextFiles.Any()) Cache.Instance.Log("Found [" + EveSharpBotTextFiles.Count + "] old EVE log files to delete");

                    int countInterations = 0;
                    foreach (FileInfo file in EveSharpBotTextFiles)
                    {
                        countInterations++;
                        if (countInterations > 400)
                        {
                            Cache.Instance.Log("CleanupSpecificEveSharpBotLogsForThisEveAccount: countInterations > 400 - we only want to remove 400 files per startup for performance reasons");
                            return;
                        }

                        try
                        {
                            if (file.Extension != FileExtensionWithDot)
                            {
                                Cache.Instance.Log("skipping [" + file.Name + "] Extension [" + file.Extension + "] != [ " + FileExtensionWithDot + " ]");
                                continue;
                            }

                            file.Attributes = FileAttributes.Normal;
                            Cache.Instance.Log("[" + countInterations + "] Removing Log: [" + file.Name + "]");
                            File.Delete(file.FullName);
                        }
                        catch (Exception ex)
                        {
                            Cache.Instance.Log("Exception [" + ex + "]");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                }
            }

            return;
        }


        public void ClearCache()
        {
            Cache.Instance.Log($"Next cache deletion for character {MaskedCharacterName} set to {DateTime.MinValue}");
            NextCacheDeletion = DateTime.MinValue;
        }

        public void ClearRefreshToken()
        {
            try
            {
                if (Cache.Instance == null)
                    return;

                Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] Clearing EVE Refresh Token (these usually last 90 days!)");
                RefreshTokenString = string.Empty;
            }
            catch (Exception)
            {
                //swallow exception
            }
        }

        public void ClearEveAccessToken()
        {
            try
            {
                if (Cache.Instance == null)
                    return;

                Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] Clearing EVEAccessToken (These usually last 500 seconds)");
                EveAccessTokenString = string.Empty;
                EveAccessTokenValidUntil = DateTime.MinValue;
            }
            catch (Exception)
            {
                //swallow exception
            }
        }

        public void ShowAllISBELCookies()
        {
            IsbelEveAccount myIsbelEveAccount = new IsbelEveAccount(AccountName,
                                                       ConnectToTestServer,
                                                       TranquilityEveAccessTokenString,
                                                       TranquilityEveAccessTokenValidUntil,
                                                       TranquilityRefreshTokenString,
                                                       TranquilityRefreshTokenValidUntil,
                                                       SisiEveAccessTokenString,
                                                       SisiEveAccessTokenValidUntil,
                                                       SisiRefreshTokenString,
                                                       SisiRefreshTokenValidUntil);
            myIsbelEveAccount.PrintCookies(myIsbelEveAccount.Cookies);
            return;
        }

        public void DeleteCurlCookie()
        {
            try
            {
                using (var curlWoker = new CurlWorker(AccountName))
                {
                    if (curlWoker.DeleteCurrentSessionCookie(true))
                    {
                        Cache.Instance.Log($"Cookies for Account [{MaskedAccountName}] deleted.");
                    }
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: [" + ex + "]");
            }
        }

        private void CleanupEveProcessExistsTrue()
        {
            RestartOfEveClientNeeded = false;
            LastPcName = Environment.MachineName;
            LastPcNameDateTime = DateTime.UtcNow;
        }

        private void CleanupEveProcessExistsFalse()
        {
            //Pid = 0;
            DoneLaunchingEveInstance = true;
            DoNotSessionChange = false;
        }

        private DateTime NextEveProcessExists = DateTime.UtcNow;

        public bool? _eveProcessExists = null;

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool EveProcessExists
        {
            get
            {
                try
                {
                    if (DateTime.UtcNow > NextEveProcessExists)
                        _eveProcessExists = null;

                    if (_eveProcessExists == null || (_eveProcessExists != null && !(bool)_eveProcessExists))
                    {
                        if (Pid == -1)
                        {
                            NextEveProcessExists = DateTime.UtcNow.AddMilliseconds(500);
                            //Cache.Instance.Log("EveProcessExists: CleanupEveProcessExistsFalse: [" + AccountName + "][" + MaskedCharacterName + "] PID == -1");
                            CleanupEveProcessExistsFalse();
                            _eveProcessExists = false;
                            return (bool)_eveProcessExists;
                        }

                        if (Pid == 0)
                        {
                            NextEveProcessExists = DateTime.UtcNow.AddMilliseconds(500);
                            //Cache.Instance.Log("EveProcessExists: CleanupEveProcessExistsFalse: [" + MaskedAccountName + "][" + MaskedCharacterName + "] PID == 0");
                            CleanupEveProcessExistsFalse();
                            _eveProcessExists = false;
                            return (bool)_eveProcessExists;
                        }

                        if (LastEveClientLaunched.AddSeconds(20) > DateTime.UtcNow)
                        {
                            //
                            // for the first 10 seconds assume the EVE process does exist.
                            //
                            //NextEveProcessExists = DateTime.UtcNow.AddMilliseconds(500);
                            //Cache.Instance.Log("EveProcessExists: CleanupEveProcessExistsTrue: [" + MaskedAccountName + "][" + MaskedCharacterName + "] !if (Util.ProcessList.Any(x => x.Id == Pid))");
                            //CleanupEveProcessExistsTrue();
                            return true;
                        }

                        if (Util.ProcessList.Any(x => x.Id == Pid))
                        {
                            //
                            // we have the pid in the processlist
                            //
                            try
                            {
                                Process __process = Array.Find(Util.ProcessList, x => x.Id == Pid);
                                if (__process != null && __process.ProcessName.ToLower().Contains("exefile".ToLower()))
                                {
                                    NextEveProcessExists = DateTime.UtcNow.AddMilliseconds(500);
                                    CleanupEveProcessExistsTrue();
                                    _eveProcessExists = true;
                                    return (bool)_eveProcessExists;
                                }
                            }
                            catch (Exception ex)
                            {
                                Cache.Instance.Log("Exception [" + ex + "]");
                            }

                            NextEveProcessExists = DateTime.UtcNow.AddMilliseconds(500);
                            Cache.Instance.Log("EveProcessExists: CleanupEveProcessExistsFalse: [" + MaskedAccountName + "][" + MaskedCharacterName + "] !if (Array.Find(Util.ProcessList, x => x.Id == Pid).ProcessName.ToLower().Contains(exefile.ToLower()))");
                            CleanupEveProcessExistsFalse();
                            if (Pid != 0) Pid = 0;
                            _eveProcessExists = false;
                            return (bool)_eveProcessExists;
                        }

                        NextEveProcessExists = DateTime.UtcNow.AddMilliseconds(500);
                        Cache.Instance.Log("EveProcessExists: CleanupEveProcessExistsFalse: [" + MaskedAccountName + "][" + MaskedCharacterName + "] !if (Util.ProcessList.Any(x => x.Id == Pid))");
                        CleanupEveProcessExistsFalse();
                        if (Pid != 0) Pid = 0;
                        _eveProcessExists = false;
                        return (bool)_eveProcessExists;
                    }

                    if ((bool)_eveProcessExists)
                    {
                        CleanupEveProcessExistsTrue();
                        _eveProcessExists = true;
                        return (bool)_eveProcessExists;
                    }

                    //Cache.Instance.Log("EveProcessExists: CleanupEveProcessExistsFalse: [" + MaskedAccountName + "][" + MaskedCharacterName + "] !if ((bool)_eveProcessExists)");
                    //CleanupEveProcessExistsFalse();
                    //if (Pid != 0) Pid = 0;
                    //_eveProcessExists = false;
                    return false;
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return false;
                }
            }
        }

        internal bool UpdateOfPatternNeeded
        {
            get
            {
                if (string.IsNullOrEmpty(Pattern))
                    return true;

                if (PatternManagerLastUpdate.AddHours((7 * 24) - 2) < DateTime.UtcNow)
                    return true;

                return false;
            }
        }

        public void GenerateNewTimeSpans()
        {
            try
            {
                if (PatternManagerEnabled)
                {
                    if (!IsProcessAlive())
                    {
                        if (UpdateOfPatternNeeded)
                        {
                            var newPattern = PatternManager.Instance.GenerateNewPattern(PatternManagerHoursPerWeek, PatternManagerDaysOffPerWeek, PatternManagerExcludedHours);

                            if (Util.IntervalInMinutes(5, 5, CharacterName) || DebugScheduler)
                                Cache.Instance.Log($"PatternManagerLastUpdate for [{MaskedAccountName}][{MaskedCharacterName}] is older than 7 days or is empty. Updating. New Pattern will be [{newPattern}]");
                            Pattern = newPattern;
                            StartsPast24H = 0;
                        }
                    }

                    return;
                }

                StartsPast24H = 0;
                GenerateStartTime();
                GenerateEndTime();
            }
            catch (Exception e)
            {
                Cache.Instance.Log("Exception " + e.StackTrace);
            }
        }

        public IDuplexServiceCallback GetClientCallback()
        {
            return ClientCallback;
        }

        public string GetEveStartParameter()
        {
            try
            {
                if (HWSettings.LauncherMachineHash.Length != 32)
                {
                    HWSettings.LauncherMachineHash = LauncherHash.GetRandomLauncherHash().Item1;
                }

                var ccpXDHash = LauncherHash.CCPMagic(HWSettings.LauncherMachineHash.Replace("-", ""));

                if (ccpXDHash.Length != 19)
                    return string.Empty;

                Cache.Instance.Log($"MD5 MachineHash: [{HWSettings.LauncherMachineHash}] CCP FailEncodedHash: [{ccpXDHash}]");

                string ServerStartupSwitch = String.Empty;

                // New Launcher
                //ServerStartupSwitch = "/server:tranquility.servers.eveonline.com"; //this means: server:Tranquility";

                //Old
                ServerStartupSwitch = ""; //this means: server:Tranquility";
                if (ConnectToTestServer)
                {
                    //New Launcher
                    //ServerStartupSwitch = "/server:singularity.servers.eveonline.com";

                    //Old
                    ServerStartupSwitch = "/server:Singularity";
                }

                Cache.Instance.Log("ServerStartupSwitch [" + ServerStartupSwitch + "]");

                string startParams = string.Empty;



                // New Launcher
                // "C:\Games\EVE Online\tq\bin64\exefile.exe"  /noconsole /server:tranquility.servers.eveonline.com /triplatform=dx12 /ssoToken=XXX /refreshToken=XXX /settingsprofile=Default /deviceID=XXX /autoSelectCharacter:XXX /language=en
                //
                //startParams += "/noconsole" + " "; //We might not want to do this here
                //startParams += ServerStartupSwitch + " " ;
                //startParams += "/triplatform=dx12" + " "; //We might not want to do this here
                //startParams += "/ssoToken=" + EveAccessTokenString + " ";
                //startParams += "/refreshToken=" + RefreshTokenString + " ";
                //startParams += "/settingsProfile=Default" + " " ;
                //startParams += "/deviceID= " + GUID + " "; //this needs a GUID, not the LauncherMachineHash = we already have one we created for this account. might need to generate one for each hardware profile? why?
                //startParams += "/autoSelectCharacter:" + MyCharacterId + " ";
                //startParams += "/language=en" + " ";

                //Old
                startParams += ServerStartupSwitch + " " + "/machineHash=" + HWSettings.LauncherMachineHash + " ";
                startParams += "/ssoToken=" + EveAccessTokenString + " " + "/refreshToken=" + RefreshTokenString;

                if (!string.IsNullOrEmpty(MyCharacterId) && ByPassLoginScreen)
                {
                    startParams += " /character=" + MyCharacterId + " ";
                }

                //if (!string.IsNullOrEmpty(MyCharacterId))
                //{
                //    startParams += " /autoSelectCharacter=" + MyCharacterId + " ";
                //}

                return startParams;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception " + ex);
                return string.Empty;
            }
        }

        public DateTime? GetLastDirectEvent(DirectEvents directEvent)
        {
            return DirectEventHandler.GetLastEventReceived(GUID, directEvent);
        }

        public bool HasLastDirectEvent(DirectEvents directEvent)
        {
            return DirectEventHandler.GetLastEventReceived(GUID, directEvent) != null;
        }

        public void HideConsoleWindow()
        {
            if (Pid != 0)
                foreach (KeyValuePair<IntPtr, string> w in Util.GetVisibleWindows(Pid))
                    if (w.Value.Contains("[CCP]"))
                    {
                        Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] Hiding Console Window");
                        Util.ShowWindow(w.Key, Util.SW_HIDE);
                    }
        }

        public void StartExecuteable(string filename, string parameters = "")
        {
            var args = new string[] { CharacterName, WCFServer.Instance.GetPipeName };
            var processId = -1;
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var injectionFile = Path.Combine(path, "DomainHandler.dll");
            String ChannelName = null;
            RemoteHooking.IpcCreateServer<EVESharpInterface>(ref ChannelName, WellKnownObjectMode.SingleCall);


            if (!String.IsNullOrEmpty(filename) && File.Exists(filename))
            {
                RemoteHooking.CreateAndInject(filename, parameters, (int)InjectionOptions.Default, injectionFile,
                    injectionFile, out processId, ChannelName,
                    args);
                return;
            }

            var openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "exe files | *.exe";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
                RemoteHooking.CreateAndInject(openFileDialog.FileName, parameters, (int)InjectionOptions.Default,
                    injectionFile, injectionFile, out processId,
                    ChannelName, args);
        }

        public void HideWindows()
        {
            if (Pid != 0 && !DisableHiding)
                foreach (KeyValuePair<IntPtr, string> w in Util.GetVisibleWindows(Pid))
                    Util.ShowWindow(w.Key, Util.SW_HIDE);
        }

        public bool IsProcessAlive()
        {
            try
            {
                if (Pid == 0)
                    return false;

                if (Util.ProcessList == null)
                    return false;

                CheckEveAccountSettings();

                Process p = Array.Find(Util.ProcessList, x => x.Id == Pid);
                if (p != null)
                {
                    if (!IsProcessResponding(p) && !ManuallyPausedViaUI)
                    {
                        Cache.Instance.Log("Account [" + MaskedAccountName + "] Character [" + MaskedCharacterName + "] if (!IsProcessResponding(p))");
                        return false;
                    }

                    //
                    // we have been logged in more than 10 sec and its been more than 10 sec since session was ready
                    //

                    if (DoneLaunchingEveInstance && DateTime.UtcNow > LastEveClientLaunched.AddSeconds(90) && DateTime.UtcNow > LastQuestorStarted.AddSeconds(90))
                    {
                        if (!ManuallyPausedViaUI && !CheckEvents())
                        {
                            Cache.Instance.Log("Account [" + MaskedAccountName + "] Character [" + MaskedCharacterName + "] if (!CheckEvents())");
                            return false;
                        }

                        int timeToWait = 35;
                        if (IsInAbyss)
                        {
                            if (UseFleetMgr)
                                return true; //If we are doing destroyer or frigate abyssals we are probably fast enough that if eve has to be restarted we are dead. So do not do that!

                            timeToWait = 6;
                        }

                        if (!ManuallyPausedViaUI && DateTime.UtcNow > LastSessionReady.AddSeconds(10))
                        {
                            Cache.Instance.Log("Account [" + MaskedAccountName + "] Character [" + MaskedCharacterName + "] if (DateTime.UtcNow > LastSessionReady.AddSeconds(" + 10 + "))");
                        }

                        if (!ManuallyPausedViaUI && DateTime.UtcNow > LastSessionReady.AddSeconds(15))
                        {
                            Cache.Instance.Log("Account [" + MaskedAccountName + "] Character [" + MaskedCharacterName + "] if (DateTime.UtcNow > LastSessionReady.AddSeconds(" + 15 + "))");
                        }

                        if (!ManuallyPausedViaUI && DateTime.UtcNow > LastSessionReady.AddSeconds(20))
                        {
                            Cache.Instance.Log("Account [" + MaskedAccountName + "] Character [" + MaskedCharacterName + "] if (DateTime.UtcNow > LastSessionReady.AddSeconds(" + 20 + "))");
                        }

                        if (!ManuallyPausedViaUI && DateTime.UtcNow > LastSessionReady.AddSeconds(timeToWait))
                        {
                            Cache.Instance.Log("Account [" + MaskedAccountName + "] Character [" + MaskedCharacterName + "] if (DateTime.UtcNow > LastSessionReady.AddSeconds(" + timeToWait + ")) return false - IsInAbyss [" + IsInAbyss + "]");
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return false;
            }
        }

        public bool IsProcessResponding(Process p)
        {
            try
            {
                if (p != null)
                {
                    if (_lastResponding == null)
                        _lastResponding = DateTime.UtcNow;

                    if (p.Responding)
                    {
                        _lastResponding = DateTime.UtcNow;
                        return true;
                    }

                    if (!DoneLaunchingEveInstance || LastEveClientLaunched.AddSeconds(90) > DateTime.UtcNow)
                        return _lastResponding.Value.AddSeconds(90) > DateTime.UtcNow;

                    return _lastResponding.Value.AddSeconds(20) > DateTime.UtcNow;
                }
                return false;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return false;
            }
        }

        public bool KillEveProcess(bool force = false)
        {
            if (lastEveInstanceKilled.AddSeconds(waitTimeBetweenEveInstancesKills) < DateTime.UtcNow || !IsProcessAlive() || force)
            {
                lastEveInstanceKilled = DateTime.UtcNow;
                waitTimeBetweenEveInstancesKills = rnd.Next(7, 12);
                if (EveProcessExists)
                {
                    try
                    {
                        Util.TaskKill(Pid, true);
                        Info = string.Empty;
                        Cache.Instance.Log(string.Format($"Stopping Eve process used by character {MaskedCharacterName} on account {MaskedAccountName} ConnectToTestServer [" + ConnectToTestServer + "] with pid {Pid}"));
                    }
                    catch
                    {
                        Cache.Instance.Log("Exception: Couldn't execute taskkill.");
                    }
                    return true;
                }
            }
            return false;
        }

        public void SetClientCallback(IDuplexServiceCallback s)
        {
            ClientCallback = s;
        }

        public void ShowConsoleWindow()
        {
            if (EveProcessExists)
                foreach (KeyValuePair<IntPtr, string> w in Util.GetInvisibleWindows(Pid))
                    if (w.Value.Contains("[CCP]"))
                        Util.ShowWindow(w.Key, Util.SW_SHOWNOACTIVATE);
        }

        public void ShowWindows()
        {
            if (EveProcessExists)
            {
                if (WinApiUtil.WinApiUtil.IsValidHWnd((IntPtr)EveHWnd))
                {
                    WinApiUtil.WinApiUtil.AddToTaskbar((IntPtr)EveHWnd);
                    WinApiUtil.WinApiUtil.SetWindowsPos((IntPtr)EveHWnd, 0, 0);
                    if (WinApiUtil.WinApiUtil.IsValidHWnd((IntPtr)Cache.Instance.MainFormHWnd))
                    {
                        WinApiUtil.WinApiUtil.SetHWndInsertAfter((IntPtr)EveHWnd, (IntPtr)Cache.Instance.MainFormHWnd);
                        WinApiUtil.WinApiUtil.SetHWndInsertAfter((IntPtr)HookmanagerHWnd, (IntPtr)Cache.Instance.MainFormHWnd);
                        WinApiUtil.WinApiUtil.SetHWndInsertAfter((IntPtr)EVESharpCoreFormHWnd, (IntPtr)Cache.Instance.MainFormHWnd);
                    }
                    Util.ShowWindow((IntPtr)EveHWnd, Util.SW_SHOWNOACTIVATE);
                }

                Dictionary<IntPtr, string> list = Util.GetInvisibleWindows(Pid);
                foreach (KeyValuePair<IntPtr, string> w in list)
                    if ((long)w.Key == HookmanagerHWnd
                        || ((long)w.Key == EVESharpCoreFormHWnd)
                        || (w.Value.Contains("exefile.exe"))
                        || (w.Value.Contains("[CCP]") && Console))
                        Util.ShowWindow(w.Key, Util.SW_SHOWNOACTIVATE);

                if (WinApiUtil.WinApiUtil.IsValidHWnd((IntPtr)EveHWnd))
                {
                    WinApiUtil.WinApiUtil.SetWindowsPos((IntPtr)EveHWnd, 0, 0);
                    Util.ShowWindow((IntPtr)EveHWnd, Util.SW_SHOWNOACTIVATE);
                    WinApiUtil.WinApiUtil.ForcePaint((IntPtr)EveHWnd);
                    WinApiUtil.WinApiUtil.ForceRedraw((IntPtr)EveHWnd);
                }
            }
        }

        [XmlIgnore]
        [ReadOnly(true)]
        public bool ThisAccountIsSafeToBeStarted
        {
            get
            {
                try
                {





                    //if (!HWSettings.Proxy.CheckHttpProxyInternetConnectivity(this))
                    //{
                    //    Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "]: CheckHttpProxyInternetConnectivity [ false ]");
                    //    return false;
                    //}



                    //Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "] ThisAccountIsSafeToBeStarted [ true ]");
                    return true;
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return false;
                }
            }
        }

        public bool QueueThisAccountToBeStarted(float priority, string logMessage)
        {
            try
            {
                if (!Cache.StartEveForTheseAccountsQueue.Any() && Pid == 0 && DateTime.UtcNow > LastEveClientPulledFromQueue.AddSeconds(15))
                {
                    LastStartEveQueuePriority = priority;
                    Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] Queued: Priority Level [" + StartEvePriorityLevel + "] EveAccount(s) in the queue [ 1 ][" + logMessage + "]");
                    Cache.StartEveForTheseAccountsQueue.Enqueue(this, priority);
                    return true;
                }

                if (Cache.StartEveForTheseAccountsQueue.All(i => i.GUID != GUID) && Pid == 0 && DateTime.UtcNow > LastEveClientPulledFromQueue.AddSeconds(15))
                {
                    int intStartEveForTheseAccountsQueueCount = Cache.StartEveForTheseAccountsQueue.Count();
                    intStartEveForTheseAccountsQueueCount++;
                    Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] Queued: Priority Level [" + StartEvePriorityLevel + "] EveAccount(s) in the queue [" + intStartEveForTheseAccountsQueueCount + "][" + logMessage + "].");
                    Cache.StartEveForTheseAccountsQueue.Enqueue(this, priority);
                    return true;
                }

                //
                // this account must already be in the queue
                //
                return false;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return false;
            }
        }

        private bool EveAccountAuthenticateToRetrieveSSOToken(IsbelEveAccount myIsbelEveAccount)
        {
            try
            {
                if (!ThisAccountIsSafeToBeStarted)
                {
                    QueueThisAccountToBeStarted(EveAccountStartPriority.NormalPriority, "if (!ThisAccountIsSafeToBeStarted)");
                    return false;
                }

                HWSettings.Proxy.LastEveSsoAttempt = DateTime.UtcNow;
                if (UseLocalInternetConnection)
                    Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] GetSSOToken: is configured to use the Local Internet Connection!");
                else
                    Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] GetSSOToken: is configured to use the proxy connection [" + HWSettings.Proxy.Description + "]");

                Cache.Instance.Log("GetEveAccessToken: [" + MaskedAccountName + "][" + MaskedCharacterName + "] Attempt to retrieve EveAccessToken: ConnectToTestServer [" + ConnectToTestServer + "]");
                LoginResult lr = myIsbelEveAccount.GetSSOToken_EveAccount(this);
                if (lr != LoginResult.Success)
                {
                    StartsPast24H++;
                    Cache.Instance.Log("LoginResult != Success: LoginResult [" + lr + "]");
                    return false;
                }

                if (EveAccessTokenString == null)
                {
                    Cache.Instance.Log("myIsbelEveAccount.AccessToken is null. Error.");
                    StartsPast24H++;
                    //StartProxy();
                    return false;
                }

                if (EveAccessTokenString == "")
                {
                    Cache.Instance.Log("myIsbelEveAccount.AccessToken is empty. Error.");
                    StartsPast24H++;
                    //StartProxy();
                    return false;
                }

                if (DateTime.Now > EveAccessTokenValidUntil)
                {
                    Cache.Instance.Log("myIsbelEveAccount.AccessToken is Expired. Error.");
                    StartsPast24H++;
                    //StartProxy();
                    return false;
                }

                Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] ssoToken retrieved successfully [" + EveAccessTokenString + "]");
                return true;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return false;
            }
        }

        private void AddFirewallRules()
        {
            if (!CreateExeFileFirewallRule)
                return;

            if (!UseLocalInternetConnection)
            {
                string FirewallRuleName = "EVE-TQ";
                if (ConnectToTestServer)
                    FirewallRuleName = "EVE-SISI";

                if (FirewallRuleHelper.AddAnIndividualFwBlockingRule(FirewallRuleName, ExeFileFullPath, "EVE Client"))
                {
                    Cache.Instance.Log("Enabled Windows Firewall and Added Firewall Rule [" + FirewallRuleName + "] to block [" + ExeFileFullPath + "] from using the local connection");
                }

                //if (FirewallRuleHelper.AddAnIndividualFwBlockingRule("BlockEVE-EveLauncher.exe", "C:\\eve-launcher\\Launcher\\evelauncher.exe", "EVE Launcher"))
                //{
                //    Cache.Instance.Log("Enabled Windows Firewall and Added Firewall Rule to block EVE Launcher from using the local connection");
                //}
            }
        }

        public int remoteVersion;
        public int localVersion;

        public bool CheckEveServerVersion_TQ()
        {
            /**
            try
            {
                if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(eA => eA != null && eA.EveProcessExists))
                {
                    //
                    // We have toons already logged in the Eve Client on this PC is in use (and should be the right version!?)
                    //
                    return true;
                }

                //
                // FIXME: this downloading of server status from the webserver should retry if it fails and/or give better end user readable error messages if it fails
                //
                WebClient webClient = new WebClient();
                //string EveServerStatusUrl = "https://c4s.de/eveServerStatus";
                //
                // https://esi.evetech.net/ui/#/Status
                //
                string EveServerStatusUrl = "https://esi.evetech.net/latest/status/?datasource=tranquility";
                object EveServerStatusJSONResult = JSON.parse(webClient.DownloadString(EveServerStatusUrl));
                Dictionary<string, string> EveServerStatusResultStringDictionary = EveServerStatusJSONResult.ToStringDictionary();
                webClient.Dispose();

                if (EveServerStatusResultStringDictionary.ContainsKey("server_version"))
                {
                    if (EveServerStatusResultStringDictionary.ContainsKey("vip") && EveServerStatusResultStringDictionary["vip"].ToLower().Equals("true"))
                        Cache.Instance.Log($"CheckEveVersion: Error: Server is still in VIP-mode.");

                    remoteVersion = EveServerStatusResultStringDictionary["server_version"].ToInt();
                    Cache.Instance.Log("CheckEveServerVersion: Eve Server Version is [" + remoteVersion + "]");
                    localVersion = 0;

                    var evePath = GetEvePath(false);
                    var eveINIFile = Path.Combine(evePath, "start.ini");
                    if (File.Exists(eveINIFile))
                    {
                        var iniR = INIReader.Read(File.ReadAllText(eveINIFile));
                        if (iniR.ContainsKey("build"))
                        {
                            localVersion = iniR["build"].ToInt();
                            Cache.Instance.Log("CheckEveServerVersion: Eve Client Version is [" + localVersion + "]");
                        }
                        else
                        {
                            Cache.Instance.Log($"CheckEveVersion: Error: Key ['build'] does not exist in [{eveINIFile}].");
                        }
                    }
                    else
                    {
                        Cache.Instance.Log($"CheckEveVersion: Error: File [{eveINIFile}] does not exist.");
                    }

                    Cache.Instance.Log($"CheckEveVersion: Remote version: [{remoteVersion}] Local version: [{localVersion}]");

                    if (remoteVersion != localVersion)
                    {
                        try
                        {
                            Cache.Instance.Log("CheckEveServerVersion: Eve Client Version is [" + localVersion + "] does not match Eve Server Version [" + remoteVersion + "]");
                            bool _finished = false;
                            var _lastProgressChanged = DateTime.UtcNow;
                            if (remoteVersion > localVersion && Cache.Instance.EveSettings.AutoUpdateEve)
                            {
                                Cache.Instance.Log("CheckEveServerVersion: AutoUpdateEve [" + Cache.Instance.EveSettings.AutoUpdateEve + "]");
                                var tempFileName = Path.GetRandomFileName();

                                WebClient client = new WebClient();
                                Uri uri = new Uri($"https://patches.c4s.de/{remoteVersion}_patch.zip");
                                Cache.Instance.Log($"CheckEveVersion: Downloading patch from [{uri}] to [{tempFileName}]");
                                client.DownloadFileCompleted += delegate (object sender, AsyncCompletedEventArgs args)
                                {
                                    _finished = true;
                                };
                                client.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs args)
                                {
                                    if (_lastProgressChanged < DateTime.UtcNow)
                                    {
                                        _lastProgressChanged = DateTime.UtcNow.AddSeconds(1);
                                        Cache.Instance.Log($"CheckEveVersion: Downloaded [{args.ProgressPercentage}%]");
                                    }
                                };
                                client.DownloadFileAsync(uri, tempFileName);

                                while (client.IsBusy || !_finished)
                                {
                                    Thread.Sleep(20);
                                }

                                client.Dispose();

                                if (!Util.IsZipValid(tempFileName))
                                {
                                    Cache.Instance.Log("CheckEveVersion: Error: Zip archive is corrupted.");
                                    return false;
                                }

                                var eveTQPath = GetEvePath(false);

                                Cache.Instance.Log($"CheckEveVersion: Extracting archive to [{eveTQPath}]");
                                using (ZipArchive archive = ZipFile.Open(tempFileName, ZipArchiveMode.Read))
                                {
                                    Util.ExtractZipToDirectory(archive, eveTQPath, true);
                                }

                                Cache.Instance.Log($"CheckEveVersion: Successfully extracted the patch version [{remoteVersion}].");
                                Cache.Instance.Log($"CheckEveVersion: Deleting temp file[{tempFileName}].");
                                File.Delete(tempFileName);
                            }
                            else
                            {
                                Cache.Instance.Log("CheckEveVersion: Error: local Eve version [" + localVersion + "] does not match EVE server version [" + remoteVersion + "]");
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Cache.Instance.Log("Exception [" + ex + "]");
                            return false;
                        }
                    }
                }
                else
                {
                    Cache.Instance.Log("CheckEveVersion: JSON error: Server version could not be found!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return true;
            }
            **/
            return true;
        }

        public bool ProcessEveOauth2(IsbelEveAccount myIsbelEveAccount, bool UseLavishLauncherHTTPsLoginProcess)
        {
            if (myIsbelEveAccount.MyRefreshAndAccessTokensIfAny == null)
            {
                Cache.Instance.Log("EveAccessToken is null");
            }

            if (!string.IsNullOrEmpty(EveAccessTokenString))
            {
                Cache.Instance.Log("EveAccessToken Expiration [" + EveAccessTokenValidUntil + "]");
                Cache.Instance.Log("EveAccessTokenString [" + EveAccessTokenString + "]");
            }

            if (UseLavishLauncherHTTPsLoginProcess)
            {
                if (!EveAccountAuthenticateToRetrieveSSOToken(myIsbelEveAccount)) return false;

                if (EveAccessTokenValidUntil > DateTime.Now)
                {
                    Cache.Instance.Log("ProcessEveOauth2: EveAccessTokenString [" + EveAccessTokenString + "]");
                    return false;
                }

                return true;
            }

            var eveAccessToken = GetEveAccessToken();

            if (eveAccessToken == string.Empty)
            {
                Cache.Instance.Log("EveAccessToken is empty. Error.");
                StartsPast24H++;
                return false;
            }

            return true;
        }

        public string GetEveAccessToken()
        {
            try
            {
                //
                // Username Password --> Code --> EveAccessToken (this expires in 5-6 hours) AND a RefreshToken (that effectively never expires if its used semi regularly)
                // or
                // RefreshToken --> EveAccessToken (this expires in 5-6 hours) AND it renews the exiration on the same RefreshToken
                //
                // There is probably some better documentation and flowcharts somewhere we should find and add to this section
                //

                //if (EveAccessTokenValidUntil > DateTime.Now && !String.IsNullOrWhiteSpace(EveAccessTokenString))
                //{
                //    Cache.Instance.Log("Returning cached EveAccessToken: " + EveAccessTokenString);
                //    return EveAccessTokenString;
                //}

                using (var curlWoker = new CurlWorker(this.AccountName))
                {
                    var refreshToken = GetSSORefreshToken();
                    if (string.IsNullOrWhiteSpace(refreshToken))
                    {
                        Cache.Instance.Log("GetSSORefreshToken: refreshToken is empty");
                        return string.Empty;
                    }

                    //if (EveAccessTokenValidUntil > DateTime.Now)
                    //{
                    //    return EveAccessTokenString;
                    //}

                    string postdataRefreshToken = $"client_id=eveLauncherTQ" +
                        $"&grant_type=refresh_token" +
                        $"&refresh_token={HttpUtility.UrlEncode(refreshToken)}";

                    bool includeHeader = false;
                    bool followLocation = true;

                    Cache.Instance.Log("GetEveAccessToken: curlWoker.GetPostPage");
                    Cache.Instance.Log("URI: " + EveUri.GetTokenUri(ConnectToTestServer).ToString());
                    Cache.Instance.Log("refreshToken: [ " + refreshToken + " ]");
                    Cache.Instance.Log("postdataRefreshToken: [ " + postdataRefreshToken + " ]");
                    Cache.Instance.Log("followLocation: [" + followLocation + "] includeheader [" + includeHeader + "]");

                    var resp = curlWoker.GetPostPage(
                        EveUri.GetTokenUri(ConnectToTestServer).ToString(), postdataRefreshToken, HWSettings.Proxy.GetSocks5IpPort(),
                        HWSettings.Proxy.GetUserPassword(), followLocation, includeHeader);

                    using (StreamWriter w = File.CreateText(Util.AssemblyPath + "\\Logs\\" + "\\GetEveAccessToken.txt"))
                    {
                        w.WriteLine(resp);
                    }

                    Cache.Instance.Log("GetEveAccessToken: curlWoker.GetPostPage: resp [" + resp + "]");

                    if (string.IsNullOrWhiteSpace(resp) || string.IsNullOrEmpty(resp))
                    {
                        Cache.Instance.Log("GetEveAccessToken: Are you connected? retry? The Response was empty when trying to get an AccessToken using the RefreshToken");
                        return string.Empty;
                    }

                    if (resp.Contains("Invalid refresh token. Token missing/expired"))
                    {
                        Cache.Instance.Log("GetEveAccessToken: Invalid refresh token. Token missing/expired");
                        Cache.Instance.Log(resp);
                        return string.Empty;
                    }

                    if (resp.Contains("invalid_grant"))
                    {
                        Cache.Instance.Log("GetEveAccessToken: invalid_grant");
                        Cache.Instance.Log(resp);
                        return string.Empty;
                    }

                    if (resp.Contains("error_description"))
                    {
                        Cache.Instance.Log("GetEveAccessToken: error_description");
                        Cache.Instance.Log(resp);
                        return string.Empty;
                    }

                    var tempToken = DeserializeJSON_RefreshTokenAndAccesToken(resp);

                    if (tempToken == null)
                    {
                        Cache.Instance.Log("Refresh token is empty from response: [" + resp + "]");
                        return String.Empty;
                    }

                    Cache.Instance.Log("RAW response: [" + resp + "]");
                    RefreshTokenString = tempToken._authObj.Refresh_token;
                    RefreshTokenValidUntil = DateTime.Now.AddDays(90);
                    EveAccessTokenString = tempToken._authObj.access_token;
                    EveAccessTokenValidUntil = DateTime.Now.AddSeconds(tempToken._authObj.Expires_in);
                    Cache.Instance.Log("RefreshToken: " + RefreshTokenString);
                    Cache.Instance.Log("EveAccessToken: " + EveAccessTokenString);
                    Cache.Instance.Log("EveAccessToken expires " + EveAccessTokenValidUntil);
                    return EveAccessTokenString;
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return string.Empty;
            }
        }

        private string GetAccessTokenFromString(string s)
        {
            try
            {
                IsbelEveAccount.Token thisToken = new IsbelEveAccount.Token(JsonConvert.DeserializeObject<IsbelEveAccount.AuthObj>(s));
                //var at = s.Substring("\"access_token\":\"", "\"");
                Cache.Instance.Log($"Eve access token: {thisToken._authObj.access_token}");
                Cache.Instance.Log($"Eve access token length: {thisToken._authObj.access_token.Length}");
                if (thisToken._authObj.access_token.Length < 100)
                {
                    Cache.Instance.Log("AccessToken length < 100.");
                }

                return thisToken._authObj.access_token;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "][" + s + "]");
                return string.Empty;
            }
        }

        private string GetTokenExpiresInFromString(string s)
        {
            try
            {
                IsbelEveAccount.Token thisToken = new IsbelEveAccount.Token(JsonConvert.DeserializeObject<IsbelEveAccount.AuthObj>(s));
                return thisToken._authObj.Expires_in.ToString();
            }
            catch (Exception ex)
            {
                //var tei = s.Substring("\"expires_in\":", ",");
                //if (tei.Length > 5 && tei.Length < 2)

                Cache.Instance.Log("Exception [" + ex + "][" + s + "]");
                return string.Empty;
            }
        }

        private IsbelEveAccount.Token DeserializeJSON_RefreshTokenAndAccesToken(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                Cache.Instance.Log("GetRefreshTokenFromString: s was empty");
                return null;
            }

            try
            {
                try
                {
                    IsbelEveAccount.Token thisToken = new IsbelEveAccount.Token(JsonConvert.DeserializeObject<IsbelEveAccount.AuthObj>(s));
                    return thisToken;
                }
                catch (Exception ex)
                {
                    //var rt = s.Substring("\"refresh_token\":\"", "\"");
                    //if (rt.Length != 24)
                    //{
                    //    Cache.Instance.Log("Error: RefreshToken length != 24.");
                    //    return string.Empty;
                    //}
                    //
                    //return rt;
                    Cache.Instance.Log("Exception [" + ex + "][" + s + "]");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "][" + s + "]");
                return null;
            }
        }

        private string GetAuthCodeFromString(string s)
        {
            var ac = s.Substring("&code=", "&state=");
            if (ac.Length != 22)
            {
                Cache.Instance.Log("Error: Authcode length != 22.");
                return string.Empty;
            }
            return ac;
        }

        private string GetVerificationToken(string s)
        {
            try
            {
                var vt = s.Substring("__RequestVerificationToken\" type=\"hidden\" value=\"", "\"");
                if (vt.Length != 92)
                {
                    Cache.Instance.Log("Error: Verification token length != 92.");
                    return string.Empty;
                }
                return vt;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return string.Empty;
            }
        }

        public string GetSSORefreshToken()
        {
            try
            {
                Cache.Instance.Log("GetSSORefreshToken: Before: RefreshTokenString [" + RefreshTokenString + "]");
                if (!String.IsNullOrWhiteSpace(RefreshTokenString) && !String.IsNullOrEmpty(RefreshTokenString) && RefreshTokenValidUntil > DateTime.Now)
                {
                    Cache.Instance.Log("GetSSORefreshToken: Returning cached EveAccessToken.RefreshTokenString: [" + RefreshTokenString + "]");
                    try
                    {
                        Cache.Instance.Log("GetSSORefreshToken: Returning cached EveAccessToken.Expiration: [" + RefreshTokenValidUntil + "]");
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("Exception [" + ex + "]");
                    }
                    try
                    {
                        Cache.Instance.Log("GetSSORefreshToken: Returning cached EveAccessToken.TokenString: [" + RefreshTokenString + "]");
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("Exception [" + ex + "]");
                    }

                    return RefreshTokenString;
                }

                using (var curlWoker = new CurlWorker(this.AccountName))
                {
                    Cache.Instance.Log("GetSSORefreshToken: Aquiring a new refresh token");

                    Guid state = Guid.NewGuid();
                    Guid challengeCodeSource = Guid.NewGuid();
                    byte[] challengeCode = System.Text.Encoding.UTF8.GetBytes(challengeCodeSource.ToString().Replace("-", ""));
                    string challengeHash = Base64.Encode(SHA256.GenerateHash(Base64.Encode(challengeCode)));

                    Cache.Instance.Log($"State: {state}");
                    Cache.Instance.Log($"ChallengeHash: {challengeHash}");



                    //bool followLocation = true;
                    //bool includeHeader = true;

                    //Logoff: any current character on session
                    //Cache.Instance.Log("Logoff: any current character on session");
                    //var url = EveUri.GetLogoffUri(ConnectToTestServer, state.ToString(), challengeHash).ToString();
                    //Cache.Instance.Log("URL: " + url);
                    //string resp = curlWoker.GetPostPage(url, string.Empty, HWSettings.Proxy.GetSocks5IpPort(),
                    //    HWSettings.Proxy.GetUserPassword(), followLocation, includeHeader);
                    //Cache.Instance.Log("resp: " + resp);

                    //Login: Gets the login page to extract the verfication token
                    var url = EveUri.GetLoginUri(ConnectToTestServer, state.ToString(), challengeHash).ToString();

                    Cache.Instance.Log($"Open the following URI in the browser (make sure to start firefox WITH the proxy corresponding to the eve account.)");
                    Cache.Instance.Log($"EVELogin Uri: {url}");

                    //resp = curlWoker.GetPostPage(url, string.Empty, HWSettings.Proxy.GetSocks5IpPort(),
                    //    HWSettings.Proxy.GetUserPassword(), followLocation, includeHeader);
                    //Cache.Instance.Log("resp: " + resp);

                    //var verificationToken = GetVerificationToken(resp);

                    //if (string.IsNullOrEmpty(verificationToken))
                    //{
                    //    Cache.Instance.Log("No verification token found in response");
                    //    return string.Empty;
                    //}

                    //Cache.Instance.Log("verificationToken: " + verificationToken);

                    //string postLoginData = $"__RequestVerificationToken={verificationToken}" +
                    //    $"&UserName={HttpUtility.UrlEncode(AccountName)}" +
                    //    $"&Password={HttpUtility.UrlEncode(Password)}" +
                    //    $"&RememberMe=false";

                    //Cache.Instance.Log("URL: " + url);
                    //Cache.Instance.Log("postLoginData: " + postLoginData);

                    //resp = curlWoker.GetPostPage(url, postLoginData, HWSettings.Proxy.GetSocks5IpPort(),
                    //    HWSettings.Proxy.GetUserPassword(), followLocation, includeHeader);

                    //Cache.Instance.Log("resp: " + resp);

                    //if (resp.Contains("Account/Challenge?"))
                    //{
                    //    Cache.Instance.Log("Character name challenge required. Sending challenge.");
                    //    url =
                    //        "https://login.eveonline.com/Account/Challenge?ReturnUrl=%2Foauth%2Fauthorize%2F%3Fclient_id%3DeveLauncherTQ%26lang%3Den%26response_type%3Dtoken%26redirect_uri%3Dhttps%3A%2F%2Flogin.eveonline.com%2Flauncher%3Fclient_id%3DeveLauncherTQ%26scope%3DeveClientToken";
                    //    resp = curlWoker.GetPostPage(url, "Challenge=" + CharacterName + "&RememberCharacterChallenge=false&command=Continue",
                    //        HWSettings.Proxy.GetSocks5IpPort(), HWSettings.Proxy.GetUserPassword(), followLocation, includeHeader);
                    //}

                    //if (resp.Contains("/account/verifytwofactor"))
                    //{
                    //    Cache.Instance.Log("Email challenge required. Opening the challenge window.");
                    //    var challFrm = new AccountChallengeForm();
                    //    challFrm.ShowDialog();
                    //    var challenge = challFrm.Challenge;
                    //    Cache.Instance.Log($"Submitting given challenge: {challenge}");
                    //    url = EveUri.GetVerifyTwoFactorUri(ConnectToTestServer, state.ToString(), challengeHash).ToString();
                    //    resp = curlWoker.GetPostPage(url, "Challenge=" + challenge + "&command=Continue&IsPasswordBreached=False&NumPasswordBreaches=0",
                    //        HWSettings.Proxy.GetSocks5IpPort(), HWSettings.Proxy.GetUserPassword(), followLocation, includeHeader);
                    //}

                    //if (resp.Contains("/oauth/eula"))
                    //{
                    //    // id="eulaHash" name="eulaHash" type="hidden" value="8FE2635FC9F85DE00542597D9D962E8D"
                    //    // https://login.eveonline.com/OAuth/Eula
                    //    // "eulaHash=8FE2635FC9F85DE00542597D9D962E8D&returnUrl=https%3A%2F%2Flogin.eveonline.com%2Foauth%2Fauthorize%2F%3Fclient_id%3DeveLauncherTQ%26lang%3Den%26response_type%3Dtoken%26redirect_uri%3Dhttps%3A%2F%2Flogin.eveonline.com%2Flauncher%3Fclient_id%3DeveLauncherTQ%26scope%3DeveClientToken&action=Accept"
                    //    var eulaHashLine = resp.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                    //        .FirstOrDefault(l => l.Contains("id=\"eulaHash\""));

                        //if (eulaHashLine == null)
                        //{
                        //    Cache.Instance.Log("if(eulaHashLine == null)");
                        //    return String.Empty;
                        //}
                        //else
                        //{
                        //    var eulaHash = eulaHashLine.Substring("name=\"eulaHash\" type=\"hidden\" value=\"", "\"");
                        //    if (eulaHash.Length != 32)
                        //    {
                        //        Cache.Instance.Log("if(eulaHash.Length != 32)");
                        //        return String.Empty;
                        //    }
                        //    else
                        //    {
                        //        Cache.Instance.Log("Accepting Eula. Eula hash: " + eulaHash);
                        //        url = "https://login.eveonline.com/OAuth/Eula";
                        //        var postData = "eulaHash=" + eulaHash +
                        //                       "&returnUrl=https%3A%2F%2Flogin.eveonline.com%2Foauth%2Fauthorize%2F%3Fclient_id%3DeveLauncherTQ%26lang%3Den%26response_type%3Dtoken%26redirect_uri%3Dhttps%3A%2F%2Flogin.eveonline.com%2Flauncher%3Fclient_id%3DeveLauncherTQ%26scope%3DeveClientToken&action=Accept";
                        //        resp = curlWoker.GetPostPage(url, postData, HWSettings.Proxy.GetSocks5IpPort(), HWSettings.Proxy.GetUserPassword(), followLocation, includeHeader);
                        //    }
                        //}
                    //}

                    //string authCode = GetAuthCodeFromString(resp);

                    var challFrm = new AuthCodeForm();
                    challFrm.ShowDialog();
                    var authCode = challFrm.Challenge;
                    Cache.Instance.Log($"authcode you entered was: {authCode}");

                    if (authCode.Length == 0)
                    {
                        Cache.Instance.Log("Unable to get Authcode");
                        return String.Empty;
                    }

                    url = EveUri.GetTokenUri(ConnectToTestServer).ToString();
                    string postDataEveAccessToken = $"grant_type=authorization_code" +
                        $"&client_id=eveLauncherTQ" +
                        $"&redirect_uri=https%3A%2F%2Flogin.eveonline.com%2Flauncher%3Fclient_id%3DeveLauncherTQ" +
                        $"&code={authCode}" +
                        $"&code_verifier={Base64.Encode(challengeCode)}";

                    //bool boolDebug = true;
                    var resp = curlWoker.GetPostPage(url, postDataEveAccessToken, HWSettings.Proxy.GetSocks5IpPort(), HWSettings.Proxy.GetUserPassword(), true, false);

                    using (StreamWriter w = File.CreateText(Util.AssemblyPath + "\\Logs\\" + "\\GetSSORefreshToken.txt"))
                    {
                        w.WriteLine(resp);
                    }

                    Cache.Instance.Log("resp [" + resp + "]");
                    if (string.IsNullOrEmpty(resp))
                    {
                        return String.Empty;
                    }

                    var tempToken = DeserializeJSON_RefreshTokenAndAccesToken(resp);

                    if (tempToken == null)
                    {
                        Cache.Instance.Log("Refresh token is empty from response: [" + resp + "]");
                        return string.Empty;
                    }
                    else
                    {
                        RefreshTokenString = tempToken._authObj.Refresh_token;
                        RefreshTokenValidUntil = DateTime.Now.AddDays(90);
                        EveAccessTokenString = tempToken._authObj.access_token;
                        EveAccessTokenValidUntil = DateTime.Now.AddSeconds(tempToken._authObj.Expires_in);
                        Cache.Instance.Log("Refresh token request was successful: " + RefreshTokenString);
                        Cache.Instance.Log("EveAccess token is [ " + EveAccessTokenString + " ] EveAccesTokenExpires [" + EveAccessTokenValidUntil + "]");
                        return RefreshTokenString;
                    }
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
                return String.Empty;
            }
        }

        [Browsable(false)]
        public string myCharactersLogDirectoryName
        {
            get
            {
                if (string.IsNullOrEmpty(CharacterName))
                    return AccountName;

                return CharacterName;
            }
        }

        //string eveBasePath = GetEveUnderscorePath();
        [Browsable(false)]
        public string appDataCCPEveFolder
        {
            get
            {
                return GetAppDataFolder() + "CCP\\EVE\\";
            }
        }

        [Browsable(false)]
        public string eveSettingFolder
        {
            get
            {
                return GetAppDataFolder() + "CCP\\EVE\\" + GetEveUnderscorePath() + "\\";
            }
        }

        [Browsable(false)]
        public string backupEveSettingFolder
        {
            get
            {
                return GetAppDataFolder() + "CCP\\EVE\\BackupOfSettings-" + GetEveUnderscorePath() + "\\";
            }
        }

        [Browsable(false)]
        public string crashDumpFolder
        {
            get
            {
                return GetAppDataFolder() + "CrashDumps";
            }
        }

        //public string defaultSettingsFolder = GetDefaultSettingsFolder(); // default settings folder to copy
        [Browsable(false)]
        public string eveCacheFolder
        {
            get
            {
                return eveSettingFolder + "cache\\";
            }
        }

        [Browsable(false)]
        public string questorConsoleLogFolder
        {
            get
            {
                return Path.Combine(Util.AssemblyPath, "Logs", myCharactersLogDirectoryName, "Console");
            }
        }

        [Browsable(false)]
        public string sharpLogLiteFolder
        {
            get
            {
                return Path.Combine(Util.AssemblyPath, "Logs", myCharactersLogDirectoryName, "SharpLogLite");
            }
        }

        [Browsable(false)]
        public string logFolder
        {
            get
            {
                return Path.Combine(Util.AssemblyPath, "Logs");
            }
        }

        internal void CopyDefaultEveSettingsIfNeeded()
        {
            if (!Directory.Exists(eveSettingFolder))
            {
                Util.DirectoryCopy(GetDefaultSettingsFolder(), eveSettingFolder, true);
                Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] EveSettingsFolder doesn't exist. Copying default settings");
            }
            else
            {
                Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] EveSettingsFolder already exists. Not copying default settings");
            }
        }

        internal void BackupExistingEveSettings()
        {
            if (Directory.Exists(eveSettingFolder))
            {
                Util.DirectoryCopy(eveSettingFolder, backupEveSettingFolder, true);
                Cache.Instance.Log("Backup [" + eveSettingFolder + "] to [" + backupEveSettingFolder + "]");
            }
        }

        internal void RestoreBackupOfEveSettings()
        {
            if (Directory.Exists(backupEveSettingFolder))
            {
                Util.DirectoryCopy(backupEveSettingFolder, eveSettingFolder, true);
                Cache.Instance.Log("Restore [" + backupEveSettingFolder + "] to [" + eveSettingFolder + "]");
            }
        }

        public void StartEveInject()
        {
            try
            {
                if (EveProcessExists)
                {
                    Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "] StartEveInject: EveProcess already exists! pid [" + Pid + "]");
                    return;
                }

                try
                {
                    // Disable checking for SISI for the moment
                    if (!ConnectToTestServer && CheckEveServerVersion)
                    {
                        //there was an issue downloading the update ZIP from the DukeTwo server 1-27-2021
                        if (!CheckEveServerVersion_TQ()) return;
                    }
                }
                catch (Exception e)
                {
                    Cache.Instance.Log(e.ToString());
                    return;
                }

                AddFirewallRules();

                if (!ThisAccountIsSafeToBeStarted) return;

                IsbelEveAccount myIsbelEveAccount = new IsbelEveAccount(AccountName,
                                                        ConnectToTestServer,
                                                        TranquilityEveAccessTokenString,
                                                        TranquilityEveAccessTokenValidUntil,
                                                        TranquilityRefreshTokenString,
                                                        TranquilityRefreshTokenValidUntil,
                                                        SisiEveAccessTokenString,
                                                        SisiEveAccessTokenValidUntil,
                                                        SisiRefreshTokenString,
                                                        SisiRefreshTokenValidUntil);
                bool UseLavishLauncherHTTPsLoginProcess = false;

                bool readyToStartEve = ProcessEveOauth2(myIsbelEveAccount, UseLavishLauncherHTTPsLoginProcess);

                if (!readyToStartEve)
                {
                    Cache.Instance.Log("AccountName [" + MaskedAccountName + "] ProcessEveOauth2 returned false; ---------------------------------------------------");
                    Cache.Instance.Log("AccountName [" + MaskedAccountName + "] ProcessEveOauth2 returned false; ---------------------------------------------------");
                    Cache.Instance.Log("AccountName [" + MaskedAccountName + "] ProcessEveOauth2 returned false; Stopping EVE login process! We cannot launch eve if we dont have a valid token to login with!");
                    Cache.Instance.Log("AccountName [" + MaskedAccountName + "] ProcessEveOauth2 returned false; ---------------------------------------------------");
                    Cache.Instance.Log("AccountName [" + MaskedAccountName + "] ProcessEveOauth2 returned false; ---------------------------------------------------");
                    return;
                }

                Util.CheckCreateDirectorys(HWSettings.WindowsUserLogin);

                if (Cache.Instance.EveSettings.AlwaysClearNonEveSharpCCPData)
                {
                    ClearNonEveSharpCCPData();
                }

                string[] args = { CharacterName, WCFServer.Instance.GetPipeName, GUID };
                int processId = -1;
                EVESharpCoreFormHWnd = -1;
                EveHWnd = -1;
                HookmanagerHWnd = -1;

                try
                {
                    if (Directory.Exists(logFolder))
                        foreach (string file in Directory.GetFiles(logFolder, "*.log").Where(item => item.EndsWith(".log")))
                            if (File.Exists(file))
                            {
                                long fileSize = new FileInfo(file).Length;
                                if (fileSize > 1024 * 1024)
                                    File.Delete(file);
                            }

                    if (Directory.Exists(questorConsoleLogFolder))
                        foreach (string file in Directory.GetFiles(questorConsoleLogFolder, "*.log").Where(item => item.EndsWith(".log")))
                        {
                            DateTime fileLastwrite = File.GetLastWriteTimeUtc(file);
                            if (fileLastwrite.AddDays(15) < DateTime.UtcNow)
                                File.Delete(file);
                        }

                    if (Directory.Exists(sharpLogLiteFolder))
                        foreach (string file in Directory.GetFiles(sharpLogLiteFolder, "*.log").Where(item => item.EndsWith(".log")))
                        {
                            DateTime fileLastwrite = File.GetLastWriteTimeUtc(file);
                            long fileSize = new FileInfo(file).Length;
                            if (fileLastwrite.AddDays(90) < DateTime.UtcNow || fileSize > 1024 * 1024)
                                File.Delete(file);
                        }

                    if (Directory.Exists(crashDumpFolder))
                        foreach (string file in Directory.GetFiles(crashDumpFolder, "*.dmp").Where(item => item.EndsWith(".dmp")))
                            File.Delete(file);

                    if (Directory.Exists(crashDumpFolder))
                        foreach (string file in Directory.GetFiles(appDataCCPEveFolder, "*.crs").Where(item => item.EndsWith(".crs")))
                            File.Delete(file);

                    foreach (string file in Directory.GetFiles(appDataCCPEveFolder, "*.dmp").Where(item => item.EndsWith(".dmp")))
                        File.Delete(file);

                    if (Directory.Exists(eveSettingFolder))
                        Cache.Instance.Log($"Eve settings directory: {eveCacheFolder}");
                    else
                        Cache.Instance.Log($"Eve settings does not exist!");

                    if (Directory.Exists(eveCacheFolder))
                        Cache.Instance.Log($"Cache directory: {eveCacheFolder}");
                    else
                        Cache.Instance.Log($"Cache directory does not exist!");

                    if (Directory.Exists(eveCacheFolder) && NextCacheDeletion < DateTime.UtcNow)
                    {
                        NextCacheDeletion = DateTime.UtcNow.AddDays(Util.GetRandom(15, 45));
                        Cache.Instance.Log("Clearing EVE cache folder: " + eveCacheFolder + " Next cache deletion will be on: " + NextCacheDeletion);
                        Directory.Delete(eveCacheFolder, true);

                        if (Directory.Exists(GetPersonalFolder() + "Documents\\EVE\\logs\\"))
                            Directory.Delete(GetPersonalFolder() + "Documents\\EVE\\logs\\", true);
                    }
                }
                catch (Exception)
                {
                    Cache.Instance.Log("Couldn't clear cache, crs files, dmp files.");
                }

                CopyDefaultEveSettingsIfNeeded();

                if (!File.Exists(ExeFileFullPath))
                {
                    Cache.Instance.Log("Exefile.exe is missing from [" + ExeFileFullPath + "]");
                    return;
                }

                if (!ExeFileFullPath.ToLower().EndsWith("exefile.exe"))
                {
                    Cache.Instance.Log("Exefile.exe filename not properly set? It has to end with exefile.exe");
                    return;
                }

                CopyDomainHandlerForEachUserAtStart();

                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string injectionFile = Path.Combine(path, "Domainhandler-" + MaskedCharacterName + "-" + GUID + ".dll");
                string ChannelName = null;

                Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] IpcCreateServer: Creating IPC server");
                RemoteHooking.IpcCreateServer<EVESharpInterface>(ref ChannelName, WellKnownObjectMode.SingleCall);

                string startParams = GetEveStartParameter();

                if (startParams.Length == 0)
                {
                    Cache.Instance.Log("Error: startParams.Length == 0.");
                    return;
                }

                Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] CreateAndInject: Starting EVE Client using [" + ExeFileFullPath + " " + startParams + "] and InjectionFile [" + injectionFile + "]");
                RemoteHooking.CreateAndInject(ExeFileFullPath, startParams, (int)InjectionOptions.Default, injectionFile,
                    injectionFile, out processId, ChannelName, args);

                //RemoteHooking.CreateAndInject("C:\\Program Files (x86)\\PuTTY\\putty.exe", "", (int)InjectionOptions.Default, injectionFile,
                //    injectionFile, out processId, ChannelName, args);

                if (processId != -1 && processId != 0)
                {
                    DoneLaunchingEveInstance = false;
                    StartsPast24H++;
                    LastSessionReady = DateTime.UtcNow;
                    AmountExceptionsCurrentSession = 0;
                    Pid = processId;
                    Cache.Instance.Log("[" + Num + "][" + MaskedAccountName + "][" + MaskedCharacterName + "] New EVE ProcessId is [" + Pid + "]");
                    LastEveClientLaunched = DateTime.UtcNow;
                    Info = string.Empty;
                    DirectEventHandler.ClearEvents(CharacterName);
                    GUIDByPid.AddOrUpdate(processId, GUID);
                    CharnameByPid.AddOrUpdate(processId, CharacterName, (key, oldValue) => CharacterName);
                    return;
                }
                else
                {
                    Cache.Instance.Log("ProcessId is zero. Error.");
                    return;
                }
            }
            catch (Exception e)
            {
                Cache.Instance.Log("Exception: " + e);
                return;
            }
        }

        private void CopyDomainHandlerForEachUserAtStart()
        {
            try
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string Domainhandlerdll = Path.Combine(path, "Domainhandler.dll");
                string Domainhandlerpdb = Path.Combine(path, "Domainhandler.pdb");
                string perToonDomainHandlerdll = Path.Combine(path, "Domainhandler-" + MaskedCharacterName + "-" + GUID + ".dll");
                string perToonDomainhandlerpdb = Path.Combine(path, "Domainhandler-" + MaskedCharacterName + "-" + GUID + ".pdb");

                try
                {
                    Cache.Instance.Log("Domainhandler [" + MaskedCharacterName + "] ----------");
                    Cache.Instance.Log("Domainhandler [" + MaskedCharacterName + "] Started Copying [" + Domainhandlerdll + "] to [" + perToonDomainHandlerdll + "]");
                    File.Copy(Domainhandlerdll, perToonDomainHandlerdll, true);
                    Thread.Sleep(100);
                    while (IsFileLocked(perToonDomainHandlerdll))
                    {
                        Cache.Instance.Log("Domainhandler [" + MaskedCharacterName + "] waiting for [" + perToonDomainHandlerdll + "] to finish copying");
                        Thread.Sleep(300);
                    }

                    Cache.Instance.Log("Domainhandler [" + MaskedCharacterName + "] Done Copying [" + Domainhandlerdll + "] to [" + perToonDomainHandlerdll + "]");
                    Cache.Instance.Log("Domainhandler [" + MaskedCharacterName + "] ----------");
                    Cache.Instance.Log("Domainhandler [" + MaskedCharacterName + "] Started Copying [" + Domainhandlerpdb + "] to [" + perToonDomainhandlerpdb + "]");
                    File.Copy(Domainhandlerpdb, perToonDomainhandlerpdb, true);
                    Thread.Sleep(100);
                    while (IsFileLocked(perToonDomainhandlerpdb))
                    {
                        Cache.Instance.Log("Domainhandler [" + MaskedCharacterName + "] waiting for [" + perToonDomainhandlerpdb + "] to finish copying");
                        Thread.Sleep(300);
                    }

                    Cache.Instance.Log("Domainhandler [" + MaskedCharacterName + "] Done Copying [" + Domainhandlerpdb + "] to [" + perToonDomainhandlerpdb + "]");
                    Cache.Instance.Log("Domainhandler [" + MaskedCharacterName + "] ----------");
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Domainhandler [" + MaskedCharacterName + "] Exception [" + ex + "]");
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("HookManager: Exception [" + ex + "]");
            }

            return;
        }

        private bool IsFileLocked(string file)
        {
            FileStream stream = null;

            try
            {
                stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        private void CheckEveAccountSettings()
        {
            //
            // Num
            //
            if (Num == 0)
            {
                int i = 0;
                foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List)
                {
                    Thread.Sleep(1);
                    i++;
                    if (CharacterName == eA.CharacterName)
                    {
                        Num = i;
                        Cache.Instance.Log("Character [" + MaskedCharacterName + "] Num updated to [" + Num + "]");
                    }
                }
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        [ReadOnly(true)]
        public bool UseLocalInternetConnection
        {
            get
            {
                if (HWSettings.Proxy.HttpProxyPort == "0" || string.IsNullOrEmpty(HWSettings.Proxy.HttpProxyPort))
                {
                    if (HWSettings.Proxy.Socks5Port == "0" || string.IsNullOrEmpty(HWSettings.Proxy.Socks5Port))
                    {
                        return true;
                    }

                    Cache.Instance.Log("[" + MaskedAccountName + "][" + MaskedCharacterName + "] HttpProxyPort is set to 0, but Socks5Port is not 0: if you want to use a proxy set LocalEveServerPort and HttpProxyPort: If you want to use the local internet connection set: HttpProxyPort, Socks5Port to 0");
                    return false;
                }

                return false;
            }
        }

        #endregion Methods

        #region pathes

        /* personal folder example */
        // C:\Users\USERNAME\Documents\EVE
        // C:\Users\USERNAME\Documents\

        /* local appdata example */
        // C:\Users\USERNAME\AppData\Local\CCP\EVE\c_eveoffline
        // C:\Users\USERNAME\AppData\Local\

        //		public string GetEveSettingsFolder() {
        //			return "C:\\Users\\";
        //		}

        //public string GetEveUnderscorePath(bool sisi = false)
        //{
        //    var eveBasePath = GetEvePath(sisi);
        //    eveBasePath = eveBasePath.Replace(":\\", "_");
        //    eveBasePath = eveBasePath.Replace("\\", "_").ToLower() + (sisi ? "_singularity" : "_tranquility");
        //    return eveBasePath;
        //}

        public void ClearNonEveSharpCCPData()
        {

            try
            {
                return;
                // Delete %localappdata%/CCP directory and its contents recursively
                string localAppDataCCP = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CCP");
                if (Directory.Exists(localAppDataCCP))
                {
                    Directory.Delete(localAppDataCCP, true);
                    Cache.Instance.Log("Deleted directory: " + localAppDataCCP);
                }

                // Delete %appdata%/EVE Online directory and its contents recursively
                string appDataEVEOnline = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EVE Online");
                if (Directory.Exists(appDataEVEOnline))
                {
                    Directory.Delete(appDataEVEOnline, true);
                    Cache.Instance.Log("Deleted directory: " + appDataEVEOnline);
                }

                // Delete registry key Computer\HKEY_CURRENT_USER\SOFTWARE\CCP\EVE
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\CCP", true))
                {
                    if (key != null)
                    {
                        key.DeleteSubKeyTree("EVE", false);
                        Cache.Instance.Log("Deleted registry key: HKEY_CURRENT_USER\\SOFTWARE\\CCP\\EVE");
                    }
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("An error occurred: " + ex.Message);
            }
        }


        public string GetEveExePath(bool sisi = false)
        {
            return sisi ? Cache.Instance.EveLocation.Replace("tq", "sisi") : Cache.Instance.EveLocation;
        }

        public string GetEvePath(bool sisi = false)
        {
            var eveLocation = sisi ? Cache.Instance.EveLocation.Replace("tq", "sisi") : Cache.Instance.EveLocation;
            return Directory.GetParent(Directory.GetParent(eveLocation).ToString()).ToString();
        }

        //public string GetEveRootPath(bool sisi = false)
        //{
        //    return Directory.GetParent(GetEvePath(sisi)).ToString();
        //}

        public EveAccount Clone()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                if (GetType().IsSerializable)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, this);
                    stream.Position = 0;
                    return (EveAccount)formatter.Deserialize(stream);
                }
                return null;
            }
        }

        public string GetAppDataFolder()
        {
            return "C:\\Users\\" + HWSettings.WindowsUserLogin + "\\AppData\\Local\\";
        }

        public static string GetDefaultSettingsFolder()
        {
            return Cache.Instance.AssemblyPath + "\\Resources\\EveSettings\\default\\";
        }

        public string GetEveRootPath()
        {
            return Directory.GetParent(GetEveExeFilePath()).ToString();
        }

        public string GetEveExeFilePath()
        {
            return Directory.GetParent(Directory.GetParent(ExeFileFullPath).ToString()).ToString();
        }

        public string GetEveUnderscorePath()
        {
            string eveBasePath = GetEveExeFilePath();
            eveBasePath = eveBasePath.Replace(":\\", "_");

            if (!UseLocalInternetConnection)
            {
                if (ConnectToTestServer)
                    eveBasePath = eveBasePath.Replace("\\", "_").ToLower() + "_" + HWSettings.Proxy.Ip;
                else
                    eveBasePath = eveBasePath.Replace("\\", "_").ToLower() + "_" + HWSettings.Proxy.Ip;
            }
            else
            {
                if (ConnectToTestServer)
                    eveBasePath = eveBasePath.Replace("\\", "_").ToLower() + "_singularity";
                else
                    eveBasePath = eveBasePath.Replace("\\", "_").ToLower() + "_tranquility";
            }

            return eveBasePath;
        }

        public string GetPersonalFolder()
        {
            return "C:\\Users\\" + HWSettings.WindowsUserLogin + "\\Documents\\";
        }

        #endregion pathes
    }

    public class EVESharpInterface : MarshalByRefObject
    {
        #region Methods

        public void Ping()
        {
            //Ping
        }

        #endregion Methods
    }
}