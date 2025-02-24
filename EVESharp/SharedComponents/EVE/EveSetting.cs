/*
/*
 * ---------------------------------------
 * User: duketwo
 * Date: 24.12.2013
 * Time: 16:26
 *
 * ---------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using SharedComponents.SharpLogLite.Model;
using SharedComponents.Utility;

namespace SharedComponents.EVE
{
    /// <summary>
    ///     Description of EveAccountData.
    /// </summary>
    [Serializable]
    public class EveSetting : ViewModelBase
    {
        #region Fields

        private static double? _statisticsAllIsk;

        private static double? _statisticsAllItemHangar;

        private static double? _statisticsAllLp;

        private static double? _statisticsAllLpValue;

        private static double? _statisticsNetWorth;

        #endregion Fields

        #region Constructors

        public EveSetting(string eveDirectory, DateTime last24HourTS)
        {
            Last24HourTS = last24HourTS;
            Proxies = new ConcurrentBindingList<Proxy>();
            DatagridViewHiddenColumns = new List<int>();
            EveDirectory = eveDirectory;
        }

        public EveSetting()
        {
            Proxies = new ConcurrentBindingList<Proxy>();
        }

        #endregion Constructors

        #region Properties

        public static double StatisticsAllIsk
        {
            get
            {
                if (_statisticsAllIsk == null)
                {
                    _statisticsAllIsk = 0;
                    foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List)
                    {
                        Thread.Sleep(1);
                        _statisticsAllIsk += eA.WalletBalance;
                    }

                    return _statisticsAllIsk ?? 0;
                }

                return 0;
            }
        }

        public static double StatisticsAllItemHangar
        {
            get
            {
                if (_statisticsAllItemHangar != null)
                {
                    _statisticsAllItemHangar = 0;
                    foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List)
                    {
                        Thread.Sleep(1);
                        if (eA.IskPerLp == 0) eA.IskPerLp = 800;
                        _statisticsAllItemHangar += eA.ItemHangarValue;
                    }

                    return _statisticsAllItemHangar ?? 0;
                }

                return 0;
            }
        }

        public static double StatisticsAllLp
        {
            get
            {
                if (_statisticsAllLp != null)
                {
                    _statisticsAllLp = 0;
                    foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List)
                    {
                        Thread.Sleep(1);
                        _statisticsAllLp += eA.LoyaltyPoints;
                    }

                    return _statisticsAllLp ?? 0;
                }

                return 0;
            }
        }

        public static double StatisticsAllLpValue
        {
            get
            {
                if (_statisticsAllLpValue != null)
                {
                    _statisticsAllLpValue = 0;
                    foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List)
                    {
                        Thread.Sleep(1);
                        _statisticsAllLpValue += eA.LpValue;
                    }

                    return _statisticsAllLpValue ?? 0;
                }

                return 0;
            }
        }

        private static int? _eveSharpLauncherThreads;

        public static int? EveSharpLauncherThreads
        {
            get
            {
                if (_eveSharpLauncherThreads == null)
                {
                    _eveSharpLauncherThreads = Process.GetCurrentProcess().Threads.Count;
                    return _eveSharpLauncherThreads;
                }

                return _eveSharpLauncherThreads;
            }
        }

        public static double StatisticsNetWorth
        {
            get
            {
                if (_statisticsNetWorth != null)
                {
                    _statisticsNetWorth = 0;
                    foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List)
                    {
                        Thread.Sleep(1);
                        _statisticsNetWorth = _statisticsNetWorth + eA.LpValue + eA.WalletBalance + eA.ItemHangarValue;
                    }

                    return _statisticsNetWorth ?? 0;
                }

                return 0;
            }
        }

        public int BackgroundFramesPerSecondLimit = 8;

        public int FramesPerSecondLimit => 20;

        public List<int> DatagridViewHiddenColumns
        {
            get { return GetValue(() => DatagridViewHiddenColumns); }
            set { SetValue(() => DatagridViewHiddenColumns, value); }
        }

        public string EveDirectory
        {
            get { return GetValue(() => EveDirectory); }
            set { SetValue(() => EveDirectory, value); }
        }

        public string GmailPassword
        {
            get { return GetValue(() => GmailPassword); }
            set { SetValue(() => GmailPassword, value); }
        }

        public bool DisableTLSVerifcation
        {
            get { return GetValue(() => DisableTLSVerifcation); }
            set { SetValue(() => DisableTLSVerifcation, value); }
        }

        public string Pastebin
        {
            get { return GetValue(() => Pastebin); }
            set { SetValue(() => Pastebin, value); }
        }

        public string WCFPipeName
        {
            get { return GetValue(() => WCFPipeName); }
            set { SetValue(() => WCFPipeName, value); }
        }

        public string GmailUser
        {
            get { return GetValue(() => GmailUser); }
            set { SetValue(() => GmailUser, value); }
        }

        public bool? KillUnresponsiveEvEs
        {
            get { return GetValue(() => KillUnresponsiveEvEs); }
            set { SetValue(() => KillUnresponsiveEvEs, value); }
        }

        public DateTime Last24HourTS
        {
            get { return GetValue(() => Last24HourTS); }
            set { SetValue(() => Last24HourTS, value); }
        }

        public DateTime LastEmptyStandbyList
        {
            get { return GetValue(() => LastEmptyStandbyList); }
            set { SetValue(() => LastEmptyStandbyList, value); }
        }

        public DateTime LastEmptyWorkingSets
        {
            get { return GetValue(() => LastEmptyWorkingSets); }
            set { SetValue(() => LastEmptyWorkingSets, value); }
        }

        public DateTime LastBackupXMLTS
        {
            get { return GetValue(() => LastBackupXMLTS); }
            set { SetValue(() => LastBackupXMLTS, value); }
        }

        public DateTime LastHourTS
        {
            get { return GetValue(() => LastHourTS); }
            set { SetValue(() => LastHourTS, value); }
        }


        public bool UseTorSocksProxy
        {
            get { return GetValue(() => UseTorSocksProxy); }
            set { SetValue(() => UseTorSocksProxy, value); }
        }

        public bool AlwaysClearNonEveSharpCCPData
        {
            //get { return GetValue(() => AlwaysClearNonEveSharpCCPData); }
            //set { SetValue(() => AlwaysClearNonEveSharpCCPData, value); }
            get
            {
                return true;
            }
        }

        public bool AutoUpdateEve
        {
            get { return GetValue(() => AutoUpdateEve); }
            set { SetValue(() => AutoUpdateEve, value); }
        }

        public bool AutoStartScheduler
        {
            get { return GetValue(() => AutoStartScheduler); }
            set { SetValue(() => AutoStartScheduler, value); }
        }

        public bool BlockEveTelemetry
        {
            get { return GetValue(() => BlockEveTelemetry); }
            set { SetValue(() => BlockEveTelemetry, value); }
        }

        public ConcurrentBindingList<Proxy> Proxies
        {
            get { return GetValue(() => Proxies); }
            set { SetValue(() => Proxies, value); }
        }

        public string ReceiverEmailAddress
        {
            get { return GetValue(() => ReceiverEmailAddress); }
            set { SetValue(() => ReceiverEmailAddress, value); }
        }

        public bool SharpLogLite
        {
            get { return GetValue(() => SharpLogLite); }
            set { SetValue(() => SharpLogLite, value); }
        }

        public int TimeBetweenEVELaunchesMin
        {
            get { return GetValue(() => TimeBetweenEVELaunchesMin); }
            set { SetValue(() => TimeBetweenEVELaunchesMin, value); }
        }
        public int TimeBetweenEVELaunchesMax
        {
            get { return GetValue(() => TimeBetweenEVELaunchesMax); }
            set { SetValue(() => TimeBetweenEVELaunchesMax, value); }
        }



        public bool ToggleHideShowOnMinimize => false;

        public string UrlToPullEveSharpCode
        {
            get { return GetValue(() => UrlToPullEveSharpCode); }
            set { SetValue(() => UrlToPullEveSharpCode, value); }
        }


        public bool IsSchedulerRunning
        {
            get { return GetValue(() => IsSchedulerRunning); }
            set { SetValue(() => IsSchedulerRunning, value); }
        }

        #endregion Properties

        #region Methods



        public static void ClearEveSettingsStatistics()
        {
            _statisticsAllIsk = null;
            _statisticsAllItemHangar = null;
            _statisticsAllLp = null;
            _statisticsNetWorth = null;
        }

        public static void ClearEveSettingsStatisticsEveryFewSec()
        {
            _eveSharpLauncherThreads = null;
        }

        public static string FormatIsk(double iskToFormat)
        {
            string FormattedIsk;
            if (iskToFormat < 1000)
                FormattedIsk = iskToFormat.ToString("#,#");
            else if (iskToFormat < 1000000)
                FormattedIsk = iskToFormat.ToString("#,##0,K");
            else if (iskToFormat < 1000000000)
                FormattedIsk = iskToFormat.ToString("#,,.##M");
            else if (iskToFormat < 1000000000000)
                FormattedIsk = iskToFormat.ToString("#,,,.##B");
            else
                FormattedIsk = iskToFormat.ToString("#,,,,.##T");

            return FormattedIsk;
        }

        public LogSeverity SharpLogLiteLogSeverity
        {
            get { return GetValue(() => SharpLogLiteLogSeverity); }
            set { SetValue(() => SharpLogLiteLogSeverity, value); }
        }

        public bool RemoteWCFServer
        {
            get { return GetValue(() => RemoteWCFServer); }
            set { SetValue(() => RemoteWCFServer, value); }
        }

        public bool RemoteWCFClient
        {
            get { return GetValue(() => RemoteWCFClient); }
            set { SetValue(() => RemoteWCFClient, value); }
        }

        public string RemoteWCFIpAddress
        {
            get { return GetValue(() => RemoteWCFIpAddress); }
            set { SetValue(() => RemoteWCFIpAddress, value); }
        }

        public string RemoteWCFPort
        {
            get { return GetValue(() => RemoteWCFPort); }
            set { SetValue(() => RemoteWCFPort, value); }
        }

        #endregion Methods
    }
}