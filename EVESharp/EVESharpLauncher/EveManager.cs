/*
 * ---------------------------------------
 * User: duketwo
 * Date: 31.01.2014
 * Time: 12:00
 *
 * ---------------------------------------
 */

using SharedComponents.EVE;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using SharedComponents.Utility;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SharedComponents.Extensions;

namespace EVESharpLauncher
{
    public class EveManager : IDisposable
    {
        #region Fields

        public static Thread eveManagerDecideThread;
        public static Thread eveSettingsManagerThread;
        private static bool isEveManagerDecideThreadAborting;
        private static bool isEveSettingsManagerThreadAborting;
        private static Thread eveKillThread;

        #endregion Fields

        #region Properties

        public static EveManager Instance { get; } = new EveManager();

        private bool IsAnyEveProcessAlive
        {
            get { return Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(e => e.Pid != 0); }
        }

        #endregion Properties

        #region Methods

        private DateTime NextEveManagerThreadTimeStamp = DateTime.UtcNow.AddSeconds(-1);
        private DateTime NextEveManagerStartEveForTheseAccountsThreadTimeStamp = DateTime.UtcNow.AddSeconds(-1);
        private DateTime NextEveManagerUpdateSlaveThreadTimeStamp = DateTime.UtcNow.AddSeconds(-1);
        private DateTime NextEveManagerHideConsoleWindows = DateTime.UtcNow.AddSeconds(-1);
        private DateTime NextLogAccountsQueuedTimeStamp = DateTime.UtcNow.AddSeconds(-1);
        private DateTime EveManagerStart = DateTime.UtcNow;

        public void Dispose()
        {
            DisposeEveManager();
            DisposeEveSettingsManager();
        }

        public void DisposeEveManager()
        {
            if (!isEveManagerDecideThreadAborting)
                isEveManagerDecideThreadAborting = true;
        }

        public void DisposeEveSettingsManager()
        {
            if (!isEveSettingsManagerThreadAborting)
                isEveSettingsManagerThreadAborting = true;
        }

        public void KillEveInstances()
        {
            try
            {
                foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(a => !string.IsNullOrEmpty(a.AccountName) && !string.IsNullOrEmpty(a.CharacterName) && a.EveProcessExists))
                {
                    Thread.Sleep(1);
                    eA.KillEveProcess(true);
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        public void KillEveInstancesDelayed()
        {
            try
            {
                if (eveKillThread == null || !eveKillThread.IsAlive)
                {
                    Cache.Instance.Log("Stopping all eve instances delayed.");
                    eveKillThread = new Thread(KillEveInstancesDelayedThread);
                    eveKillThread.Start();
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        private void EnsurePatternsExist()
        {
            try
            {
                foreach (var eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(i => !string.IsNullOrEmpty(i.AccountName) && !string.IsNullOrEmpty(i.CharacterName)))
                {
                    Thread.Sleep(1);
                    if (eA.PatternManagerEnabled && !eA.IsProcessAlive() && String.IsNullOrEmpty(eA.Pattern))
                    {
                        if (eA.PatternManagerDaysOffPerWeek== 0)
                        {
                            eA.PatternManagerDaysOffPerWeek = 1;
                        }

                        if (eA.PatternManagerHoursPerWeek == 0)
                        {
                            eA.PatternManagerHoursPerWeek = 79;
                        }

                        eA.PatternManagerLastUpdate = DateTime.UtcNow;
                        var newPattern = PatternManager.Instance.GenerateNewPattern(eA.PatternManagerHoursPerWeek, eA.PatternManagerDaysOffPerWeek, eA.PatternManagerExcludedHours);
                        Cache.Instance.Log($"PatternManagerLastUpdate for [{eA.MaskedAccountName}][{eA.MaskedCharacterName}] is older than 7 days or is empty. Updating. New Pattern will be [{newPattern}]");
                        eA.Pattern = newPattern;
                    }
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("ex");
            }
        }

        public void StartEveManagerDecide()
        {
            try
            {
                if (eveManagerDecideThread == null || !eveManagerDecideThread.IsAlive)
                {
                    EnsurePatternsExist();
                    Cache.Instance.Log("EveSharpLauncher Scheduler [ Initializing ]");
                    Thread.Sleep(10);
                    //nextEveStart = DateTime.MinValue;
                    eveManagerDecideThread = new Thread(EveManagerDecideThread);
                    eveManagerDecideThread.SetApartmentState(ApartmentState.STA);
                    eveManagerDecideThread.IsBackground = true;
                    eveManagerDecideThread.Priority = ThreadPriority.Lowest;
                    isEveManagerDecideThreadAborting = false;
                    eveManagerDecideThread.Start();
                    if (eveManagerDecideThread.IsAlive)
                    {
                        Cache.Instance.EveSettings.IsSchedulerRunning = true;
                        Cache.Instance.Log("EveSharpLauncher Scheduler [ Started ]");
                    }

                    return;
                }

                if (eveManagerDecideThread != null && eveManagerDecideThread.IsAlive)
                {
                    Cache.Instance.Log("EveSharpLauncher Scheduler [ Running ]");
                    isEveManagerDecideThreadAborting = false;
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        public void StartSettingsManager()
        {
            try
            {
                if (eveSettingsManagerThread == null || !eveSettingsManagerThread.IsAlive)
                {
                    Cache.Instance.Log("Starting EveSharpLauncher IPC [ Initializing ]");
                    eveSettingsManagerThread = new Thread(EveSettingsManagerThread);
                    eveSettingsManagerThread.SetApartmentState(ApartmentState.STA);
                    eveSettingsManagerThread.IsBackground = true;
                    eveSettingsManagerThread.Priority = ThreadPriority.Lowest;

                    isEveSettingsManagerThreadAborting = false;
                    eveSettingsManagerThread.Start();
                    if (eveManagerDecideThread.IsAlive)
                        Cache.Instance.Log("Starting EVESharpLauncher IPC [ Started ]");
                    return;
                }

                if (eveSettingsManagerThread != null && eveSettingsManagerThread.IsAlive)
                {
                    Cache.Instance.Log("Starting EVESharpLauncher IPC [ Started ]");
                    isEveSettingsManagerThreadAborting = false;
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        public void CleanupOldEveSharpLauncherLogs()
        {
            /**
            string eveLogDirectoryPath = "C:\\Users\\" + WindowsUserLogin + "\\Documents\\EVE\\logs\\";
            eveLogDirectoryPath = eveLogDirectoryPath + LogSubFolder;
            string FileExtension = "*.txt";

            if (!Directory.Exists(eveLogDirectoryPath))
            {
                //Cache.Instance.Log("Directory: [" + eveLogDirectoryPath + "] does not exist.");
                return;
            }

            //Cache.Instance.Log("EVE [ " + LogSubFolder + " ] path: [" + eveLogDirectoryPath + "] found");

            if (Directory.Exists(eveLogDirectoryPath))
            {
                try
                {
                    List<FileInfo> EveTextFiles = Directory.GetFiles(eveLogDirectoryPath, FileExtension).Select(f => new FileInfo(f)).Where(f => f.LastWriteTimeUtc < DateTime.UtcNow.AddMonths(-1)).ToList();
                    if (EveTextFiles.Any()) Cache.Instance.Log("Found [" + EveTextFiles.Count + "] old EVE log files to delete");

                    int countInterations = 0;
                    foreach (FileInfo file in EveTextFiles)
                    {
                        countInterations++;
                        if (countInterations > 400)
                        {
                            //Cache.Instance.Log("CleanupSpecificEveLogsForThisWindowsUserProfile: countInterations > 400 - we only want to remove 400 files per startup for performance reasons");
                            return;
                        }

                        try
                        {
                            if (file.Extension != ".txt")
                            {
                                Cache.Instance.Log("skipping [" + file.Name + "] Extension [" + file.Extension + "] != [ .txt ]");
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
            **/
            return;

        }

        private static void UpdateStaticSlavesDataToLeader(EveAccount thisLeaderEveAccount, EveAccount thisSlaveEveAccount, bool AddLogging = false)
        {
            try
            {
                if (thisSlaveEveAccount.NextUpdateStaticSlavesDataToLeaderTimestamp > DateTime.UtcNow)
                    return;

                if (AddLogging) Cache.Instance.Log("AddNewSlaveCharacterToList [" + thisSlaveEveAccount.MaskedCharacterName + "] slaveNameToAdd [" + thisSlaveEveAccount.MaskedCharacterName + "]");
                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName1))
                {
                    Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName1 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                    thisLeaderEveAccount.SlaveCharacterName1 = thisSlaveEveAccount.CharacterName;
                    thisLeaderEveAccount.SlaveCharacter1RepairGroup = thisSlaveEveAccount.RepairGroup;
                    thisLeaderEveAccount.SlaveCharacter1DPSGroup = thisSlaveEveAccount.DPSGroup;
                    thisLeaderEveAccount.SlaveCharacter1ChracterId = thisSlaveEveAccount.MyCharacterId;
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName2))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName1 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName2 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName2 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter2RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter2DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter2ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName3))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName2 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName3 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName3 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter3RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter3DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter3ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName4))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName3 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName4 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName4 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter4RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter4DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter4ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName5))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName4 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName5 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName5 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter5RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter5DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter5ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName6))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName5 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName6 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName6 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter6RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter6DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter6ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName7))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName6 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName7 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName7 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter7RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter7DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter7ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName8))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName7 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName8 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName8 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter8RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter8DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter8ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName9))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName8 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName9 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName9 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter9RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter9DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter9ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName10))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName9 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName10 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName10 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter10RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter10DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter10ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName11))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName10 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName11 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName11 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter11RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter11DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter11ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName12))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName11 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName12 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName12 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter12RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter12DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter12ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName13))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName12 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName13 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName13 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter13RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter13DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter13ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }


                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName14))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName13 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName14 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");//Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        thisLeaderEveAccount.SlaveCharacterName14 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter14RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter14DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter14ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName15))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName14 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName15 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName15 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter15RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter15DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter15ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName16))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName15 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName16 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName16 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter16RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter16DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter16ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName17))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName16 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName17 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName17 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter17RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter17DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter17ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName18))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName17 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName18 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName18 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter18RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter18DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter18ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName19))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName18 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName19 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName19 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter19RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter19DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter19ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName20))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName19 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName20 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName20 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter20RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter20DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter20ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }


                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName21))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName20 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName21 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName21 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter21RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter21DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter21ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }


                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName22))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName21 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName22 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName22 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter22RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter22DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter22ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName23))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName22 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName23 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName23 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter23RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter23DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter23ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName24))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName23 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName24 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName24 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter24RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter24DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter24ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName25))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName24 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName25 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName25 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter25RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter25DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter25ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName26))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName25 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName26 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName26 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter26RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter26DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter26ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName27))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName26 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName27 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName27 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter27RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter27DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter27ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName28))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName27 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName28 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName28 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter28RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter28DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter28ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName29))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName28 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName29 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName29 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter29RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter29DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter29ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName30))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName29 != thisSlaveEveAccount.CharacterName)
                    {
                        Cache.Instance.Log("AddNewSlaveCharacterToList: SlaveCharacterName30 [" + thisSlaveEveAccount.MaskedCharacterName + "][" + thisSlaveEveAccount.RepairGroup + "]");
                        thisLeaderEveAccount.SlaveCharacterName30 = thisSlaveEveAccount.CharacterName;
                        thisLeaderEveAccount.SlaveCharacter30RepairGroup = thisSlaveEveAccount.RepairGroup;
                        thisLeaderEveAccount.SlaveCharacter30DPSGroup = thisSlaveEveAccount.DPSGroup;
                        thisLeaderEveAccount.SlaveCharacter30ChracterId = thisSlaveEveAccount.MyCharacterId;
                    }
                }
            }
            catch (Exception)
            {
                //ignore this exception
            }
            finally
            {
                Thread.Sleep(500);
                thisSlaveEveAccount.NextUpdateStaticSlavesDataToLeaderTimestamp = DateTime.UtcNow.AddSeconds(10);
            }
        }

        private void UpdateDynamicSlavesDataToLeader(EveAccount thisLeaderEveAccount, EveAccount thisSlaveEveAccount)
        {
            try
            {
                if (thisLeaderEveAccount.NextUpdateStaticSlavesDataToLeaderTimestamp > DateTime.UtcNow)
                    return;

                //Cache.Instance.Log("AddNewSlaveCharacterToList [" + thisEveAccount.CharacterName + "] slaveNameToAdd [" + slaveNameToAdd + "] slaveIdAtoAdd [" + slaveIdToAdd + "]");
                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName1))
                {
                    //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName1))");
                    //thisLeaderEveAccount.SlaveCharacter1Armor = thisSlaveEveAccount.ArmorHitPoints;
                    //thisLeaderEveAccount.SlaveCharacter1ArmorPct = thisSlaveEveAccount.ArmorPct;
                    //thisLeaderEveAccount.SlaveCharacter1Shields = thisSlaveEveAccount.ShieldHitPoints;
                    //thisLeaderEveAccount.SlaveCharacter1ShieldPct = thisSlaveEveAccount.ShieldPct;
                    //thisLeaderEveAccount.SlaveCharacter1Hull = thisSlaveEveAccount.HullHitPoints;
                    //thisLeaderEveAccount.SlaveCharacter1HullPct = thisSlaveEveAccount.HullPct;
                    //thisLeaderEveAccount.SlaveCharacter1Capacitor = thisSlaveEveAccount.CapacitorLevel;
                    //thisLeaderEveAccount.SlaveCharacter1CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    return;
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName2))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName1 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter2Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter2ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter2Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter2ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter2Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter2HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter2Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter2CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }

                    return;
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName3))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName2 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter3Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter3ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter3Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter3ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter3Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter3HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter3Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter3CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }

                    return;
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName4))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName3 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter4Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter4ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter4Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter4ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter4Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter4HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter4Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter4CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }

                    return;
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName5))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName4 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter5Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter5ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter5Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter5ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter5Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter5HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter5Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter5CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }

                    return;
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName6))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName5 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter6Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter6ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter6Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter6ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter6Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter6HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter6Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter6CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }

                    return;
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName7))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName6 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter7Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter7ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter7Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter7ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter7Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter7HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter7Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter7CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }

                    return;
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName8))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName7 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter8Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter8ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter8Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter8ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter8Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter8HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter8Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter8CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName9))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName8 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter9Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter9ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter9Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter9ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter9Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter9HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter9Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter9CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName10))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName9 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter10Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter10ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter10Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter10ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter10Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter10HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter10Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter10CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName11))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName10 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter11Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter11ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter11Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter11ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter11Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter11HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter11Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter11CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName12))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName11 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter12Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter12ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter12Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter12ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter12Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter12HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter12Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter12CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName13))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName12 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter13Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter13ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter13Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter13ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter13Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter13HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter13Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter13CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName14))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName13 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter14Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter14ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter14Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter14ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter14Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter14HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter14Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter14CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }
                }

                if (string.IsNullOrEmpty(thisLeaderEveAccount.SlaveCharacterName15))
                {
                    if (thisLeaderEveAccount.SlaveCharacterName14 != thisSlaveEveAccount.CharacterName)
                    {
                        //Cache.Instance.Log("AddNewSlaveCharacterToList: if (string.IsNullOrEmpty(thisEveAccount.SlaveCharacterName2))");
                        //thisLeaderEveAccount.SlaveCharacter15Armor = thisSlaveEveAccount.ArmorHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter15ArmorPct = thisSlaveEveAccount.ArmorPct;
                        //thisLeaderEveAccount.SlaveCharacter15Shields = thisSlaveEveAccount.ShieldHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter15ShieldPct = thisSlaveEveAccount.ShieldPct;
                        //thisLeaderEveAccount.SlaveCharacter15Hull = thisSlaveEveAccount.HullHitPoints;
                        //thisLeaderEveAccount.SlaveCharacter15HullPct = thisSlaveEveAccount.HullPct;
                        //thisLeaderEveAccount.SlaveCharacter15Capacitor = thisSlaveEveAccount.CapacitorLevel;
                        //thisLeaderEveAccount.SlaveCharacter15CapacitorPct = thisSlaveEveAccount.CapacitorPct;
                    }
                }
            }
            catch (Exception)
            {
                //ignore this exception
            }
            finally
            {
                thisLeaderEveAccount.NextUpdateStaticSlavesDataToLeaderTimestamp = DateTime.UtcNow.AddSeconds(2);
            }
        }

        private void UpdateSlavesDataToLeader(EveAccount thisLeaderEveAccount, EveAccount thisSlaveEveAccount)
        {
            try
            {

                //UpdateDynamicSlavesDataToLeader(thisLeaderEveAccount, thisSlaveEveAccount);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        private static void ClearSlaveCharacters(EveAccount thisEveAccount)
        {
            try
            {
                return;
                //Cache.Instance.Log("thisEveAccount [" + thisEveAccount.CharacterName + "] ClearSlaveCharacters");
                thisEveAccount.LeaderCharacterName = string.Empty;
                thisEveAccount.SlaveCharacterName1 = string.Empty;
                thisEveAccount.SlaveCharacterName2 = string.Empty;
                thisEveAccount.SlaveCharacterName3 = string.Empty;
                thisEveAccount.SlaveCharacterName4 = string.Empty;
                thisEveAccount.SlaveCharacterName5 = string.Empty;
                thisEveAccount.SlaveCharacterName6 = string.Empty;
                thisEveAccount.SlaveCharacterName7 = string.Empty;
                thisEveAccount.SlaveCharacterName8 = string.Empty;
                thisEveAccount.SlaveCharacterName9 = string.Empty;
                thisEveAccount.SlaveCharacterName10 = string.Empty;
                thisEveAccount.SlaveCharacterName11 = string.Empty;
                thisEveAccount.SlaveCharacterName12 = string.Empty;
                thisEveAccount.SlaveCharacterName13 = string.Empty;
                thisEveAccount.SlaveCharacterName14 = string.Empty;
                thisEveAccount.SlaveCharacterName15 = string.Empty;
                thisEveAccount.SlaveCharacterName18 = string.Empty;
                thisEveAccount.SlaveCharacterName19 = string.Empty;
                thisEveAccount.SlaveCharacterName20 = string.Empty;
                thisEveAccount.SlaveCharacterName21 = string.Empty;
                thisEveAccount.SlaveCharacterName22 = string.Empty;
                thisEveAccount.SlaveCharacterName23 = string.Empty;
                thisEveAccount.SlaveCharacterName24 = string.Empty;
                thisEveAccount.SlaveCharacterName25 = string.Empty;
                thisEveAccount.SlaveCharacterName26 = string.Empty;
                thisEveAccount.SlaveCharacterName27 = string.Empty;
                thisEveAccount.SlaveCharacterName28 = string.Empty;
                thisEveAccount.SlaveCharacterName29 = string.Empty;
                thisEveAccount.SlaveCharacterName30 = string.Empty;
                thisEveAccount.SlaveCharacter1RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter2RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter3RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter4RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter5RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter6RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter7RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter8RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter9RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter10RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter11RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter12RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter13RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter14RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter15RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter16RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter17RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter18RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter19RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter20RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter21RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter22RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter23RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter24RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter25RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter26RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter27RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter28RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter29RepairGroup = string.Empty;
                thisEveAccount.SlaveCharacter30RepairGroup = string.Empty;
                //thisEveAccount.CapacitorChainBattleshipsGiveCapTo = string.Empty;
                //thisEveAccount.CapacitorChainLogisticsGiveCapTo = string.Empty;
                //thisEveAccount.SlaveCharacter1Armor = -1;
                //thisEveAccount.SlaveCharacter1ArmorPct = -1;
                //thisEveAccount.SlaveCharacter1Shields = -1;
                //thisEveAccount.SlaveCharacter1ShieldPct = -1;
                //thisEveAccount.SlaveCharacter1Hull = -1;
                //thisEveAccount.SlaveCharacter1HullPct = -1;
                //thisEveAccount.SlaveCharacter1Capacitor = -1;
                //thisEveAccount.SlaveCharacter1CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter2Armor = -1;
                //thisEveAccount.SlaveCharacter2ArmorPct = -1;
                //thisEveAccount.SlaveCharacter2Shields = -1;
                //thisEveAccount.SlaveCharacter2ShieldPct = -1;
                //thisEveAccount.SlaveCharacter2Hull = -1;
                //thisEveAccount.SlaveCharacter2HullPct = -1;
                //thisEveAccount.SlaveCharacter2Capacitor = -1;
                //thisEveAccount.SlaveCharacter2CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter3Armor = -1;
                //thisEveAccount.SlaveCharacter3ArmorPct = -1;
                //thisEveAccount.SlaveCharacter3Shields = -1;
                //thisEveAccount.SlaveCharacter3ShieldPct = -1;
                //thisEveAccount.SlaveCharacter3Hull = -1;
                //thisEveAccount.SlaveCharacter3HullPct = -1;
                //thisEveAccount.SlaveCharacter3Capacitor = -1;
                //thisEveAccount.SlaveCharacter3CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter4Armor = -1;
                //thisEveAccount.SlaveCharacter4ArmorPct = -1;
                //thisEveAccount.SlaveCharacter4Shields = -1;
                //thisEveAccount.SlaveCharacter4ShieldPct = -1;
                //thisEveAccount.SlaveCharacter4Hull = -1;
                //thisEveAccount.SlaveCharacter4HullPct = -1;
                //thisEveAccount.SlaveCharacter4Capacitor = -1;
                //thisEveAccount.SlaveCharacter4CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter5Armor = -1;
                //thisEveAccount.SlaveCharacter5ArmorPct = -1;
                //thisEveAccount.SlaveCharacter5Shields = -1;
                //thisEveAccount.SlaveCharacter5ShieldPct = -1;
                //thisEveAccount.SlaveCharacter5Hull = -1;
                //thisEveAccount.SlaveCharacter5HullPct = -1;
                //thisEveAccount.SlaveCharacter5Capacitor = -1;
                //thisEveAccount.SlaveCharacter5CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter6Armor = -1;
                //thisEveAccount.SlaveCharacter6ArmorPct = -1;
                //thisEveAccount.SlaveCharacter6Shields = -1;
                //thisEveAccount.SlaveCharacter6ShieldPct = -1;
                //thisEveAccount.SlaveCharacter6Hull = -1;
                //thisEveAccount.SlaveCharacter6HullPct = -1;
                //thisEveAccount.SlaveCharacter6Capacitor = -1;
                //thisEveAccount.SlaveCharacter6CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter7Armor = -1;
                //thisEveAccount.SlaveCharacter7ArmorPct = -1;
                //thisEveAccount.SlaveCharacter7Shields = -1;
                //thisEveAccount.SlaveCharacter7ShieldPct = -1;
                //thisEveAccount.SlaveCharacter7Hull = -1;
                //thisEveAccount.SlaveCharacter7HullPct = -1;
                //thisEveAccount.SlaveCharacter7Capacitor = -1;
                //thisEveAccount.SlaveCharacter7CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter8Armor = -1;
                //thisEveAccount.SlaveCharacter8ArmorPct = -1;
                //thisEveAccount.SlaveCharacter8Shields = -1;
                //thisEveAccount.SlaveCharacter8ShieldPct = -1;
                //thisEveAccount.SlaveCharacter8Hull = -1;
                //thisEveAccount.SlaveCharacter8HullPct = -1;
                //thisEveAccount.SlaveCharacter8Capacitor = -1;
                //thisEveAccount.SlaveCharacter8CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter9Armor = -1;
                //thisEveAccount.SlaveCharacter9ArmorPct = -1;
                //thisEveAccount.SlaveCharacter9Shields = -1;
                //thisEveAccount.SlaveCharacter9ShieldPct = -1;
                //thisEveAccount.SlaveCharacter9Hull = -1;
                //thisEveAccount.SlaveCharacter9HullPct = -1;
                //thisEveAccount.SlaveCharacter9Capacitor = -1;
                //thisEveAccount.SlaveCharacter9CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter10Armor = -1;
                //thisEveAccount.SlaveCharacter10ArmorPct = -1;
                //thisEveAccount.SlaveCharacter10Shields = -1;
                //thisEveAccount.SlaveCharacter10ShieldPct = -1;
                //thisEveAccount.SlaveCharacter10Hull = -1;
                //thisEveAccount.SlaveCharacter10HullPct = -1;
                //thisEveAccount.SlaveCharacter10Capacitor = -1;
                //thisEveAccount.SlaveCharacter10CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter11Armor = -1;
                //thisEveAccount.SlaveCharacter11ArmorPct = -1;
                //thisEveAccount.SlaveCharacter11Shields = -1;
                //thisEveAccount.SlaveCharacter11ShieldPct = -1;
                //thisEveAccount.SlaveCharacter11Hull = -1;
                //thisEveAccount.SlaveCharacter11HullPct = -1;
                //thisEveAccount.SlaveCharacter11Capacitor = -1;
                //thisEveAccount.SlaveCharacter11CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter12Armor = -1;
                //thisEveAccount.SlaveCharacter12ArmorPct = -1;
                //thisEveAccount.SlaveCharacter12Shields = -1;
                //thisEveAccount.SlaveCharacter12ShieldPct = -1;
                //thisEveAccount.SlaveCharacter12Hull = -1;
                //thisEveAccount.SlaveCharacter12HullPct = -1;
                //thisEveAccount.SlaveCharacter12Capacitor = -1;
                //thisEveAccount.SlaveCharacter12CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter13Armor = -1;
                //thisEveAccount.SlaveCharacter13ArmorPct = -1;
                //thisEveAccount.SlaveCharacter13Shields = -1;
                //thisEveAccount.SlaveCharacter13ShieldPct = -1;
                //thisEveAccount.SlaveCharacter13Hull = -1;
                //thisEveAccount.SlaveCharacter13HullPct = -1;
                //thisEveAccount.SlaveCharacter13Capacitor = -1;
                //thisEveAccount.SlaveCharacter13CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter14Armor = -1;
                //thisEveAccount.SlaveCharacter14ArmorPct = -1;
                //thisEveAccount.SlaveCharacter14Shields = -1;
                //thisEveAccount.SlaveCharacter14ShieldPct = -1;
                //thisEveAccount.SlaveCharacter14Hull = -1;
                //thisEveAccount.SlaveCharacter14HullPct = -1;
                //thisEveAccount.SlaveCharacter14Capacitor = -1;
                //thisEveAccount.SlaveCharacter14CapacitorPct = -1;
                //thisEveAccount.SlaveCharacter15Armor = -1;
                //thisEveAccount.SlaveCharacter15ArmorPct = -1;
                //thisEveAccount.SlaveCharacter15Shields = -1;
                //thisEveAccount.SlaveCharacter15ShieldPct = -1;
                //thisEveAccount.SlaveCharacter15Hull = -1;
                //thisEveAccount.SlaveCharacter15HullPct = -1;
                //thisEveAccount.SlaveCharacter15Capacitor = -1;
                //thisEveAccount.SlaveCharacter15CapacitorPct = -1;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        private static void CopyStaticLeaderInfoToSlaves(EveAccount thisSlaveAccount, EveAccount thisLeaderAccount, bool AddLogging = false)
        {
            try
            {
                if (thisSlaveAccount.NextUpdateStaticLeaderInfoToSlavesTimestamp > DateTime.UtcNow)
                    return;

                thisSlaveAccount.LeaderCharacterName = thisLeaderAccount.CharacterName;
                thisSlaveAccount.LeaderRepairGroup = thisLeaderAccount.RepairGroup;
                thisSlaveAccount.LeaderCharacterId = thisLeaderAccount.MyCharacterId;

                thisSlaveAccount.SlaveCharacter1RepairGroup = thisLeaderAccount.SlaveCharacter1RepairGroup;
                thisSlaveAccount.SlaveCharacter2RepairGroup = thisLeaderAccount.SlaveCharacter2RepairGroup;
                thisSlaveAccount.SlaveCharacter3RepairGroup = thisLeaderAccount.SlaveCharacter3RepairGroup;
                thisSlaveAccount.SlaveCharacter4RepairGroup = thisLeaderAccount.SlaveCharacter4RepairGroup;
                thisSlaveAccount.SlaveCharacter5RepairGroup = thisLeaderAccount.SlaveCharacter5RepairGroup;
                thisSlaveAccount.SlaveCharacter6RepairGroup = thisLeaderAccount.SlaveCharacter6RepairGroup;
                thisSlaveAccount.SlaveCharacter7RepairGroup = thisLeaderAccount.SlaveCharacter7RepairGroup;
                thisSlaveAccount.SlaveCharacter8RepairGroup = thisLeaderAccount.SlaveCharacter8RepairGroup;
                thisSlaveAccount.SlaveCharacter9RepairGroup = thisLeaderAccount.SlaveCharacter9RepairGroup;
                thisSlaveAccount.SlaveCharacter10RepairGroup = thisLeaderAccount.SlaveCharacter10RepairGroup;
                thisSlaveAccount.SlaveCharacter11RepairGroup = thisLeaderAccount.SlaveCharacter11RepairGroup;
                thisSlaveAccount.SlaveCharacter12RepairGroup = thisLeaderAccount.SlaveCharacter12RepairGroup;
                thisSlaveAccount.SlaveCharacter13RepairGroup = thisLeaderAccount.SlaveCharacter13RepairGroup;
                thisSlaveAccount.SlaveCharacter14RepairGroup = thisLeaderAccount.SlaveCharacter14RepairGroup;
                thisSlaveAccount.SlaveCharacter15RepairGroup = thisLeaderAccount.SlaveCharacter15RepairGroup;
                thisSlaveAccount.SlaveCharacter16RepairGroup = thisLeaderAccount.SlaveCharacter16RepairGroup;
                thisSlaveAccount.SlaveCharacter17RepairGroup = thisLeaderAccount.SlaveCharacter17RepairGroup;
                thisSlaveAccount.SlaveCharacter18RepairGroup = thisLeaderAccount.SlaveCharacter18RepairGroup;
                thisSlaveAccount.SlaveCharacter19RepairGroup = thisLeaderAccount.SlaveCharacter19RepairGroup;
                thisSlaveAccount.SlaveCharacter20RepairGroup = thisLeaderAccount.SlaveCharacter20RepairGroup;
                thisSlaveAccount.SlaveCharacter21RepairGroup = thisLeaderAccount.SlaveCharacter21RepairGroup;
                thisSlaveAccount.SlaveCharacter22RepairGroup = thisLeaderAccount.SlaveCharacter22RepairGroup;
                thisSlaveAccount.SlaveCharacter23RepairGroup = thisLeaderAccount.SlaveCharacter23RepairGroup;
                thisSlaveAccount.SlaveCharacter24RepairGroup = thisLeaderAccount.SlaveCharacter24RepairGroup;
                thisSlaveAccount.SlaveCharacter25RepairGroup = thisLeaderAccount.SlaveCharacter25RepairGroup;
                thisSlaveAccount.SlaveCharacter26RepairGroup = thisLeaderAccount.SlaveCharacter26RepairGroup;
                thisSlaveAccount.SlaveCharacter27RepairGroup = thisLeaderAccount.SlaveCharacter27RepairGroup;
                thisSlaveAccount.SlaveCharacter28RepairGroup = thisLeaderAccount.SlaveCharacter28RepairGroup;
                thisSlaveAccount.SlaveCharacter29RepairGroup = thisLeaderAccount.SlaveCharacter29RepairGroup;
                thisSlaveAccount.SlaveCharacter30RepairGroup = thisLeaderAccount.SlaveCharacter30RepairGroup;

                thisSlaveAccount.SlaveCharacterName1 = thisLeaderAccount.SlaveCharacterName1;
                thisSlaveAccount.SlaveCharacterName2 = thisLeaderAccount.SlaveCharacterName2;
                thisSlaveAccount.SlaveCharacterName3 = thisLeaderAccount.SlaveCharacterName3;
                thisSlaveAccount.SlaveCharacterName4 = thisLeaderAccount.SlaveCharacterName4;
                thisSlaveAccount.SlaveCharacterName5 = thisLeaderAccount.SlaveCharacterName5;
                thisSlaveAccount.SlaveCharacterName6 = thisLeaderAccount.SlaveCharacterName6;
                thisSlaveAccount.SlaveCharacterName7 = thisLeaderAccount.SlaveCharacterName7;
                thisSlaveAccount.SlaveCharacterName8 = thisLeaderAccount.SlaveCharacterName8;
                thisSlaveAccount.SlaveCharacterName9 = thisLeaderAccount.SlaveCharacterName9;
                thisSlaveAccount.SlaveCharacterName10 = thisLeaderAccount.SlaveCharacterName10;
                thisSlaveAccount.SlaveCharacterName11 = thisLeaderAccount.SlaveCharacterName11;
                thisSlaveAccount.SlaveCharacterName12 = thisLeaderAccount.SlaveCharacterName12;
                thisSlaveAccount.SlaveCharacterName13 = thisLeaderAccount.SlaveCharacterName13;
                thisSlaveAccount.SlaveCharacterName14 = thisLeaderAccount.SlaveCharacterName14;
                thisSlaveAccount.SlaveCharacterName15 = thisLeaderAccount.SlaveCharacterName15;
                thisSlaveAccount.SlaveCharacterName16 = thisLeaderAccount.SlaveCharacterName16;
                thisSlaveAccount.SlaveCharacterName17 = thisLeaderAccount.SlaveCharacterName17;
                thisSlaveAccount.SlaveCharacterName18 = thisLeaderAccount.SlaveCharacterName18;
                thisSlaveAccount.SlaveCharacterName19 = thisLeaderAccount.SlaveCharacterName19;
                thisSlaveAccount.SlaveCharacterName20 = thisLeaderAccount.SlaveCharacterName20;
                thisSlaveAccount.SlaveCharacterName21 = thisLeaderAccount.SlaveCharacterName21;
                thisSlaveAccount.SlaveCharacterName22 = thisLeaderAccount.SlaveCharacterName22;
                thisSlaveAccount.SlaveCharacterName23 = thisLeaderAccount.SlaveCharacterName23;
                thisSlaveAccount.SlaveCharacterName24 = thisLeaderAccount.SlaveCharacterName24;
                thisSlaveAccount.SlaveCharacterName25 = thisLeaderAccount.SlaveCharacterName25;
                thisSlaveAccount.SlaveCharacterName26 = thisLeaderAccount.SlaveCharacterName26;
                thisSlaveAccount.SlaveCharacterName27 = thisLeaderAccount.SlaveCharacterName27;
                thisSlaveAccount.SlaveCharacterName28 = thisLeaderAccount.SlaveCharacterName28;
                thisSlaveAccount.SlaveCharacterName29 = thisLeaderAccount.SlaveCharacterName29;
                thisSlaveAccount.SlaveCharacterName30 = thisLeaderAccount.SlaveCharacterName30;

                thisSlaveAccount.SlaveCharacter1ChracterId = thisLeaderAccount.SlaveCharacter1ChracterId;
                thisSlaveAccount.SlaveCharacter2ChracterId = thisLeaderAccount.SlaveCharacter2ChracterId;
                thisSlaveAccount.SlaveCharacter3ChracterId = thisLeaderAccount.SlaveCharacter3ChracterId;
                thisSlaveAccount.SlaveCharacter4ChracterId = thisLeaderAccount.SlaveCharacter4ChracterId;
                thisSlaveAccount.SlaveCharacter5ChracterId = thisLeaderAccount.SlaveCharacter5ChracterId;
                thisSlaveAccount.SlaveCharacter6ChracterId = thisLeaderAccount.SlaveCharacter6ChracterId;
                thisSlaveAccount.SlaveCharacter7ChracterId = thisLeaderAccount.SlaveCharacter7ChracterId;
                thisSlaveAccount.SlaveCharacter8ChracterId = thisLeaderAccount.SlaveCharacter8ChracterId;
                thisSlaveAccount.SlaveCharacter9ChracterId = thisLeaderAccount.SlaveCharacter9ChracterId;
                thisSlaveAccount.SlaveCharacter10ChracterId = thisLeaderAccount.SlaveCharacter10ChracterId;
                thisSlaveAccount.SlaveCharacter11ChracterId = thisLeaderAccount.SlaveCharacter11ChracterId;
                thisSlaveAccount.SlaveCharacter12ChracterId = thisLeaderAccount.SlaveCharacter12ChracterId;
                thisSlaveAccount.SlaveCharacter13ChracterId = thisLeaderAccount.SlaveCharacter13ChracterId;
                thisSlaveAccount.SlaveCharacter14ChracterId = thisLeaderAccount.SlaveCharacter14ChracterId;
                thisSlaveAccount.SlaveCharacter15ChracterId = thisLeaderAccount.SlaveCharacter15ChracterId;
                thisSlaveAccount.SlaveCharacter16ChracterId = thisLeaderAccount.SlaveCharacter16ChracterId;
                thisSlaveAccount.SlaveCharacter17ChracterId = thisLeaderAccount.SlaveCharacter17ChracterId;
                thisSlaveAccount.SlaveCharacter18ChracterId = thisLeaderAccount.SlaveCharacter18ChracterId;
                thisSlaveAccount.SlaveCharacter19ChracterId = thisLeaderAccount.SlaveCharacter19ChracterId;
                thisSlaveAccount.SlaveCharacter20ChracterId = thisLeaderAccount.SlaveCharacter20ChracterId;
                thisSlaveAccount.SlaveCharacter21ChracterId = thisLeaderAccount.SlaveCharacter21ChracterId;
                thisSlaveAccount.SlaveCharacter22ChracterId = thisLeaderAccount.SlaveCharacter22ChracterId;
                thisSlaveAccount.SlaveCharacter23ChracterId = thisLeaderAccount.SlaveCharacter23ChracterId;
                thisSlaveAccount.SlaveCharacter24ChracterId = thisLeaderAccount.SlaveCharacter24ChracterId;
                thisSlaveAccount.SlaveCharacter25ChracterId = thisLeaderAccount.SlaveCharacter25ChracterId;
                thisSlaveAccount.SlaveCharacter26ChracterId = thisLeaderAccount.SlaveCharacter26ChracterId;
                thisSlaveAccount.SlaveCharacter27ChracterId = thisLeaderAccount.SlaveCharacter27ChracterId;
                thisSlaveAccount.SlaveCharacter28ChracterId = thisLeaderAccount.SlaveCharacter28ChracterId;
                thisSlaveAccount.SlaveCharacter29ChracterId = thisLeaderAccount.SlaveCharacter29ChracterId;
                thisSlaveAccount.SlaveCharacter30ChracterId = thisLeaderAccount.SlaveCharacter30ChracterId;

                thisSlaveAccount.NextUpdateStaticLeaderInfoToSlavesTimestamp = DateTime.UtcNow.AddSeconds(10);
            }
            catch (Exception)
            {
                //ignore this exception
            }
        }

        private void CopyDynamicLeaderInfoToSlaves(EveAccount thisSlaveAccount, EveAccount thisLeaderAccount)
        {
            try
            {
                if (thisSlaveAccount.NextUpdateDynamicLeaderInfoToSlavesTimestamp > DateTime.UtcNow)
                    return;

                if (DateTime.UtcNow > thisLeaderAccount.AggressingTargetDate.AddMinutes(2))
                    thisSlaveAccount.AggressingTargetId = 0;

                if (!thisLeaderAccount.UseFleetMgr)
                    return;

                thisSlaveAccount.LeaderIsAggressingTargetId = thisLeaderAccount.AggressingTargetId;
                thisSlaveAccount.LeaderEntityId = thisLeaderAccount.LeaderEntityId;
                thisSlaveAccount.LeaderHomeStationId = thisLeaderAccount.LeaderHomeStationId;
                thisSlaveAccount.LeaderHomeSystemId = thisLeaderAccount.LeaderHomeStationId;
                thisSlaveAccount.LeaderInSpace = thisLeaderAccount.LeaderInSpace;
                thisSlaveAccount.LeaderInStation = thisLeaderAccount.LeaderInStation;
                thisSlaveAccount.LeaderInStationId = thisLeaderAccount.LeaderInStationId;
                thisSlaveAccount.LeaderInWarp = thisLeaderAccount.LeaderInWarp;
                thisSlaveAccount.LeaderIsTargetingId1 = thisLeaderAccount.LeaderIsTargetingId1;
                thisSlaveAccount.LeaderIsTargetingId2 = thisLeaderAccount.LeaderIsTargetingId2;
                thisSlaveAccount.LeaderIsTargetingId3 = thisLeaderAccount.LeaderIsTargetingId3;
                thisSlaveAccount.LeaderIsTargetingId4 = thisLeaderAccount.LeaderIsTargetingId4;
                thisSlaveAccount.LeaderIsTargetingId5 = thisLeaderAccount.LeaderIsTargetingId5;
                thisSlaveAccount.LeaderIsTargetingId6 = thisLeaderAccount.LeaderIsTargetingId6;
                thisSlaveAccount.LeaderIsTargetingId7 = thisLeaderAccount.LeaderIsTargetingId7;
                thisSlaveAccount.LeaderIsTargetingId8 = thisLeaderAccount.LeaderIsTargetingId8;
                thisSlaveAccount.LeaderIsTargetingId9 = thisLeaderAccount.LeaderIsTargetingId9;
                thisSlaveAccount.LeaderIsTargetingId10 = thisLeaderAccount.LeaderIsTargetingId10;
                thisSlaveAccount.LeaderLastActivate = thisLeaderAccount.LeaderLastActivate;
                thisSlaveAccount.LeaderLastAlign = thisLeaderAccount.LeaderLastAlign;
                thisSlaveAccount.LeaderLastApproach = thisLeaderAccount.LeaderLastApproach;
                thisSlaveAccount.LeaderLastDock = thisLeaderAccount.LeaderLastDock;
                thisSlaveAccount.LeaderLastEntityIdActivate = thisLeaderAccount.LeaderLastEntityIdActivate;
                thisSlaveAccount.LeaderLastEntityIdAlign = thisLeaderAccount.LeaderLastEntityIdAlign;
                thisSlaveAccount.LeaderLastEntityIdApproach = thisLeaderAccount.LeaderLastEntityIdApproach;
                thisSlaveAccount.LeaderLastEntityIdDock = thisLeaderAccount.LeaderLastEntityIdDock;
                thisSlaveAccount.LeaderLastOrbit = thisLeaderAccount.LeaderLastOrbit;
                thisSlaveAccount.LeaderLastEntityIdJump = thisLeaderAccount.LeaderLastEntityIdJump;
                thisSlaveAccount.LeaderLastEntityIdOrbit = thisLeaderAccount.LeaderLastEntityIdOrbit;
                thisSlaveAccount.LeaderLastEntityIdWarp = thisLeaderAccount.LeaderLastEntityIdWarp;
                thisSlaveAccount.LeaderLastJump = thisLeaderAccount.LeaderLastJump;
                thisSlaveAccount.LeaderIsInSystemId = thisLeaderAccount.LeaderIsInSystemId;
                thisSlaveAccount.LeaderTravelerDestinationSystemId = thisLeaderAccount.TravelerDestinationSystemId;
            }
            catch (Exception)
            {
                //ignore this exception
            }
            finally
            {
                thisSlaveAccount.NextUpdateDynamicLeaderInfoToSlavesTimestamp = DateTime.UtcNow.AddSeconds(3);
            }
        }

        private void CopyLeaderInfoToSlaves(EveAccount thisSlaveAccount, EveAccount thisLeaderAccount)
        {
            try
            {
                //thisSlaveAccount.boolNewSignatureDetected = false;
                //Cache.Instance.Log("CopyLeaderInfoToSlaves: thisSlaveAccount [" + thisSlaveAccount.CharacterName + "] from thisLeaderAccount [" + thisLeaderAccount.CharacterName + "] )");

                //CopyDynamicLeaderInfoToSlaves(thisSlaveAccount, thisLeaderAccount);

                /**
                thisSlaveAccount.SlaveCharacter1Armor = thisLeaderAccount.SlaveCharacter1Armor;
                thisSlaveAccount.SlaveCharacter1ArmorPct = thisLeaderAccount.SlaveCharacter1ArmorPct;
                thisSlaveAccount.SlaveCharacter1Shields = thisLeaderAccount.SlaveCharacter1Shields;
                thisSlaveAccount.SlaveCharacter1ShieldPct = thisLeaderAccount.SlaveCharacter1ShieldPct;
                thisSlaveAccount.SlaveCharacter1Capacitor = thisLeaderAccount.SlaveCharacter1Capacitor;
                thisSlaveAccount.SlaveCharacter1CapacitorPct = thisLeaderAccount.SlaveCharacter1CapacitorPct;
                thisSlaveAccount.SlaveCharacter1Hull = thisLeaderAccount.SlaveCharacter1Hull;
                thisSlaveAccount.SlaveCharacter1HullPct = thisLeaderAccount.SlaveCharacter1HullPct;

                thisSlaveAccount.SlaveCharacter1Armor = thisLeaderAccount.SlaveCharacter2Armor;
                thisSlaveAccount.SlaveCharacter2ArmorPct = thisLeaderAccount.SlaveCharacter2ArmorPct;
                thisSlaveAccount.SlaveCharacter2Shields = thisLeaderAccount.SlaveCharacter2Shields;
                thisSlaveAccount.SlaveCharacter2ShieldPct = thisLeaderAccount.SlaveCharacter2ShieldPct;
                thisSlaveAccount.SlaveCharacter2Capacitor = thisLeaderAccount.SlaveCharacter2Capacitor;
                thisSlaveAccount.SlaveCharacter2CapacitorPct = thisLeaderAccount.SlaveCharacter2CapacitorPct;
                thisSlaveAccount.SlaveCharacter2Hull = thisLeaderAccount.SlaveCharacter2Hull;
                thisSlaveAccount.SlaveCharacter2HullPct = thisLeaderAccount.SlaveCharacter2HullPct;

                thisSlaveAccount.SlaveCharacter3Armor = thisLeaderAccount.SlaveCharacter3Armor;
                thisSlaveAccount.SlaveCharacter3ArmorPct = thisLeaderAccount.SlaveCharacter3ArmorPct;
                thisSlaveAccount.SlaveCharacter3Shields = thisLeaderAccount.SlaveCharacter3Shields;
                thisSlaveAccount.SlaveCharacter3ShieldPct = thisLeaderAccount.SlaveCharacter3ShieldPct;
                thisSlaveAccount.SlaveCharacter3Capacitor = thisLeaderAccount.SlaveCharacter3Capacitor;
                thisSlaveAccount.SlaveCharacter3CapacitorPct = thisLeaderAccount.SlaveCharacter3CapacitorPct;
                thisSlaveAccount.SlaveCharacter3Hull = thisLeaderAccount.SlaveCharacter3Hull;
                thisSlaveAccount.SlaveCharacter3HullPct = thisLeaderAccount.SlaveCharacter3HullPct;

                thisSlaveAccount.SlaveCharacter4Armor = thisLeaderAccount.SlaveCharacter4Armor;
                thisSlaveAccount.SlaveCharacter4ArmorPct = thisLeaderAccount.SlaveCharacter4ArmorPct;
                thisSlaveAccount.SlaveCharacter4Shields = thisLeaderAccount.SlaveCharacter4Shields;
                thisSlaveAccount.SlaveCharacter4ShieldPct = thisLeaderAccount.SlaveCharacter4ShieldPct;
                thisSlaveAccount.SlaveCharacter4Capacitor = thisLeaderAccount.SlaveCharacter4Capacitor;
                thisSlaveAccount.SlaveCharacter4CapacitorPct = thisLeaderAccount.SlaveCharacter4CapacitorPct;
                thisSlaveAccount.SlaveCharacter4Hull = thisLeaderAccount.SlaveCharacter4Hull;
                thisSlaveAccount.SlaveCharacter4HullPct = thisLeaderAccount.SlaveCharacter4HullPct;

                thisSlaveAccount.SlaveCharacter5Armor = thisLeaderAccount.SlaveCharacter5Armor;
                thisSlaveAccount.SlaveCharacter5ArmorPct = thisLeaderAccount.SlaveCharacter5ArmorPct;
                thisSlaveAccount.SlaveCharacter5Shields = thisLeaderAccount.SlaveCharacter5Shields;
                thisSlaveAccount.SlaveCharacter5ShieldPct = thisLeaderAccount.SlaveCharacter5ShieldPct;
                thisSlaveAccount.SlaveCharacter5Capacitor = thisLeaderAccount.SlaveCharacter5Capacitor;
                thisSlaveAccount.SlaveCharacter5CapacitorPct = thisLeaderAccount.SlaveCharacter5CapacitorPct;
                thisSlaveAccount.SlaveCharacter5Hull = thisLeaderAccount.SlaveCharacter5Hull;
                thisSlaveAccount.SlaveCharacter5HullPct = thisLeaderAccount.SlaveCharacter5HullPct;

                thisSlaveAccount.SlaveCharacter6Armor = thisLeaderAccount.SlaveCharacter6Armor;
                thisSlaveAccount.SlaveCharacter6ArmorPct = thisLeaderAccount.SlaveCharacter6ArmorPct;
                thisSlaveAccount.SlaveCharacter6Shields = thisLeaderAccount.SlaveCharacter6Shields;
                thisSlaveAccount.SlaveCharacter6ShieldPct = thisLeaderAccount.SlaveCharacter6ShieldPct;
                thisSlaveAccount.SlaveCharacter6Capacitor = thisLeaderAccount.SlaveCharacter6Capacitor;
                thisSlaveAccount.SlaveCharacter6CapacitorPct = thisLeaderAccount.SlaveCharacter6CapacitorPct;
                thisSlaveAccount.SlaveCharacter6Hull = thisLeaderAccount.SlaveCharacter6Hull;
                thisSlaveAccount.SlaveCharacter6HullPct = thisLeaderAccount.SlaveCharacter6HullPct;

                thisSlaveAccount.SlaveCharacter7Armor = thisLeaderAccount.SlaveCharacter7Armor;
                thisSlaveAccount.SlaveCharacter7ArmorPct = thisLeaderAccount.SlaveCharacter7ArmorPct;
                thisSlaveAccount.SlaveCharacter7Shields = thisLeaderAccount.SlaveCharacter7Shields;
                thisSlaveAccount.SlaveCharacter7ShieldPct = thisLeaderAccount.SlaveCharacter7ShieldPct;
                thisSlaveAccount.SlaveCharacter7Capacitor = thisLeaderAccount.SlaveCharacter7Capacitor;
                thisSlaveAccount.SlaveCharacter7CapacitorPct = thisLeaderAccount.SlaveCharacter7CapacitorPct;
                thisSlaveAccount.SlaveCharacter7Hull = thisLeaderAccount.SlaveCharacter7Hull;
                thisSlaveAccount.SlaveCharacter7HullPct = thisLeaderAccount.SlaveCharacter7HullPct;

                thisSlaveAccount.SlaveCharacter8Armor = thisLeaderAccount.SlaveCharacter8Armor;
                thisSlaveAccount.SlaveCharacter8ArmorPct = thisLeaderAccount.SlaveCharacter8ArmorPct;
                thisSlaveAccount.SlaveCharacter8Shields = thisLeaderAccount.SlaveCharacter8Shields;
                thisSlaveAccount.SlaveCharacter8ShieldPct = thisLeaderAccount.SlaveCharacter8ShieldPct;
                thisSlaveAccount.SlaveCharacter8Capacitor = thisLeaderAccount.SlaveCharacter8Capacitor;
                thisSlaveAccount.SlaveCharacter8CapacitorPct = thisLeaderAccount.SlaveCharacter8CapacitorPct;
                thisSlaveAccount.SlaveCharacter8Hull = thisLeaderAccount.SlaveCharacter8Hull;
                thisSlaveAccount.SlaveCharacter8HullPct = thisLeaderAccount.SlaveCharacter8HullPct;

                thisSlaveAccount.SlaveCharacter9Armor = thisLeaderAccount.SlaveCharacter9Armor;
                thisSlaveAccount.SlaveCharacter9ArmorPct = thisLeaderAccount.SlaveCharacter9ArmorPct;
                thisSlaveAccount.SlaveCharacter9Shields = thisLeaderAccount.SlaveCharacter9Shields;
                thisSlaveAccount.SlaveCharacter9ShieldPct = thisLeaderAccount.SlaveCharacter9ShieldPct;
                thisSlaveAccount.SlaveCharacter9Capacitor = thisLeaderAccount.SlaveCharacter9Capacitor;
                thisSlaveAccount.SlaveCharacter9CapacitorPct = thisLeaderAccount.SlaveCharacter9CapacitorPct;
                thisSlaveAccount.SlaveCharacter9Hull = thisLeaderAccount.SlaveCharacter9Hull;
                thisSlaveAccount.SlaveCharacter9HullPct = thisLeaderAccount.SlaveCharacter9HullPct;

                thisSlaveAccount.SlaveCharacter10Armor = thisLeaderAccount.SlaveCharacter10Armor;
                thisSlaveAccount.SlaveCharacter10ArmorPct = thisLeaderAccount.SlaveCharacter10ArmorPct;
                thisSlaveAccount.SlaveCharacter10Shields = thisLeaderAccount.SlaveCharacter10Shields;
                thisSlaveAccount.SlaveCharacter10ShieldPct = thisLeaderAccount.SlaveCharacter10ShieldPct;
                thisSlaveAccount.SlaveCharacter10Capacitor = thisLeaderAccount.SlaveCharacter10Capacitor;
                thisSlaveAccount.SlaveCharacter10CapacitorPct = thisLeaderAccount.SlaveCharacter10CapacitorPct;
                thisSlaveAccount.SlaveCharacter10Hull = thisLeaderAccount.SlaveCharacter10Hull;
                thisSlaveAccount.SlaveCharacter10HullPct = thisLeaderAccount.SlaveCharacter10HullPct;

                thisSlaveAccount.SlaveCharacter11Armor = thisLeaderAccount.SlaveCharacter11Armor;
                thisSlaveAccount.SlaveCharacter11ArmorPct = thisLeaderAccount.SlaveCharacter11ArmorPct;
                thisSlaveAccount.SlaveCharacter11Shields = thisLeaderAccount.SlaveCharacter11Shields;
                thisSlaveAccount.SlaveCharacter11ShieldPct = thisLeaderAccount.SlaveCharacter11ShieldPct;
                thisSlaveAccount.SlaveCharacter11Capacitor = thisLeaderAccount.SlaveCharacter11Capacitor;
                thisSlaveAccount.SlaveCharacter11CapacitorPct = thisLeaderAccount.SlaveCharacter11CapacitorPct;
                thisSlaveAccount.SlaveCharacter11Hull = thisLeaderAccount.SlaveCharacter11Hull;
                thisSlaveAccount.SlaveCharacter11HullPct = thisLeaderAccount.SlaveCharacter11HullPct;

                thisSlaveAccount.SlaveCharacter12Armor = thisLeaderAccount.SlaveCharacter12Armor;
                thisSlaveAccount.SlaveCharacter12ArmorPct = thisLeaderAccount.SlaveCharacter12ArmorPct;
                thisSlaveAccount.SlaveCharacter12Shields = thisLeaderAccount.SlaveCharacter12Shields;
                thisSlaveAccount.SlaveCharacter12ShieldPct = thisLeaderAccount.SlaveCharacter12ShieldPct;
                thisSlaveAccount.SlaveCharacter12Capacitor = thisLeaderAccount.SlaveCharacter12Capacitor;
                thisSlaveAccount.SlaveCharacter12CapacitorPct = thisLeaderAccount.SlaveCharacter12CapacitorPct;
                thisSlaveAccount.SlaveCharacter12Hull = thisLeaderAccount.SlaveCharacter12Hull;
                thisSlaveAccount.SlaveCharacter12HullPct = thisLeaderAccount.SlaveCharacter12HullPct;

                thisSlaveAccount.SlaveCharacter13Armor = thisLeaderAccount.SlaveCharacter13Armor;
                thisSlaveAccount.SlaveCharacter13ArmorPct = thisLeaderAccount.SlaveCharacter13ArmorPct;
                thisSlaveAccount.SlaveCharacter13Shields = thisLeaderAccount.SlaveCharacter13Shields;
                thisSlaveAccount.SlaveCharacter13ShieldPct = thisLeaderAccount.SlaveCharacter13ShieldPct;
                thisSlaveAccount.SlaveCharacter13Capacitor = thisLeaderAccount.SlaveCharacter13Capacitor;
                thisSlaveAccount.SlaveCharacter13CapacitorPct = thisLeaderAccount.SlaveCharacter13CapacitorPct;
                thisSlaveAccount.SlaveCharacter13Hull = thisLeaderAccount.SlaveCharacter13Hull;
                thisSlaveAccount.SlaveCharacter13HullPct = thisLeaderAccount.SlaveCharacter13HullPct;

                thisSlaveAccount.SlaveCharacter14Armor = thisLeaderAccount.SlaveCharacter14Armor;
                thisSlaveAccount.SlaveCharacter14ArmorPct = thisLeaderAccount.SlaveCharacter14ArmorPct;
                thisSlaveAccount.SlaveCharacter14Shields = thisLeaderAccount.SlaveCharacter14Shields;
                thisSlaveAccount.SlaveCharacter14ShieldPct = thisLeaderAccount.SlaveCharacter14ShieldPct;
                thisSlaveAccount.SlaveCharacter14Capacitor = thisLeaderAccount.SlaveCharacter14Capacitor;
                thisSlaveAccount.SlaveCharacter14CapacitorPct = thisLeaderAccount.SlaveCharacter14CapacitorPct;
                thisSlaveAccount.SlaveCharacter14Hull = thisLeaderAccount.SlaveCharacter14Hull;
                thisSlaveAccount.SlaveCharacter14HullPct = thisLeaderAccount.SlaveCharacter14HullPct;

                thisSlaveAccount.SlaveCharacter15Armor = thisLeaderAccount.SlaveCharacter15Armor;
                thisSlaveAccount.SlaveCharacter15ArmorPct = thisLeaderAccount.SlaveCharacter15ArmorPct;
                thisSlaveAccount.SlaveCharacter15Shields = thisLeaderAccount.SlaveCharacter15Shields;
                thisSlaveAccount.SlaveCharacter15ShieldPct = thisLeaderAccount.SlaveCharacter15ShieldPct;
                thisSlaveAccount.SlaveCharacter15Capacitor = thisLeaderAccount.SlaveCharacter15Capacitor;
                thisSlaveAccount.SlaveCharacter15CapacitorPct = thisLeaderAccount.SlaveCharacter15CapacitorPct;
                thisSlaveAccount.SlaveCharacter15Hull = thisLeaderAccount.SlaveCharacter15Hull;
                thisSlaveAccount.SlaveCharacter15HullPct = thisLeaderAccount.SlaveCharacter15HullPct;
                **/
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        /**
        private bool WeHaveToonsCurrentlyDisconnected
        {
            get
            {
                if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(a => !a.EveProcessExists && (a.InAbyssalDeadspace || a.InMission || a.RestartOfEveClientNeeded || a.TestingEmergencyReLogin)))
                    return true;

                return false;
            }
        }
        **/

        private DateTime LogToonsLeftToLoginLogging = DateTime.UtcNow.AddDays(-1);

        private void LogToonsLeftToLogin()
        {
            try
            {
                if (DateTime.UtcNow > LogToonsLeftToLoginLogging)
                {
                    int intNum = 0;
                    foreach (EveAccount eA in Cache.StartEveForTheseAccountsQueue)
                    {
                        intNum++;
                        Cache.Instance.Log("[" + intNum + "] Account [" + eA.MaskedAccountName + "] Character [" + eA.MaskedCharacterName + "] still waiting to be logged in: InAbyssalDeadspace [" + eA.IsInAbyss + "] InMission [" + eA.InMission + "] isActive [" + eA.UseScheduler + "]");
                    }

                    LogToonsLeftToLoginLogging = DateTime.UtcNow.AddSeconds(30);
                }

                return;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return;
            }
        }

        private int HourToUseToCalcDowntimeComing(bool ConnectToTestServer = false)
        {
            if (ConnectToTestServer)
            {
                return 4;
            }

            return 10;
        }

        private int HourToUseToCalcDowntimeOver(bool ConnectToTestServer = false)
        {
            if (ConnectToTestServer)
            {
                return 5;
            }

            return 11;
        }

        private bool SafeToProceedAfterDownTime(EveAccount Ea)
        {
            try
            {
                if (Ea.IgnoreDowntime)
                    return true;

                if (Ea != null && Ea.UseScheduler)
                {
                    if (DateTime.UtcNow.Hour == HourToUseToCalcDowntimeComing(Ea.ConnectToTestServer) && DateTime.UtcNow.Minute > 40)
                    {
                        Cache.Instance.Log("EveManagerStartEveForTheseAccounts: if (DateTime.UtcNow.Hour == [" + HourToUseToCalcDowntimeComing(Ea.ConnectToTestServer) + "] && DateTime.UtcNow.Minute > 40)");
                        return false;
                    }

                    if (DateTime.UtcNow.Hour == HourToUseToCalcDowntimeOver(Ea.ConnectToTestServer) && DateTime.UtcNow.Minute <= 15)
                    {
                        Cache.Instance.Log("EVEManagerThread: if (DateTime.UtcNow.Hour == [" + HourToUseToCalcDowntimeOver(Ea.ConnectToTestServer) + "] && DateTime.UtcNow.Minute < 15)");
                        //KillEveInstances();
                        //Thread.Sleep(10);
                        return false;
                    }
                }
                else if (Ea != null && !Ea.UseScheduler)
                {
                    if (DateTime.UtcNow.Hour == HourToUseToCalcDowntimeComing(Ea.ConnectToTestServer) && DateTime.UtcNow.Minute > 59)
                    {
                        Cache.Instance.Log("EveManagerStartEveForTheseAccounts: if (DateTime.UtcNow.Hour == [" + HourToUseToCalcDowntimeComing(Ea.ConnectToTestServer) + "] && DateTime.UtcNow.Minute > 40)");
                        return false;
                    }

                    if (DateTime.UtcNow.Hour == HourToUseToCalcDowntimeOver(Ea.ConnectToTestServer) && DateTime.UtcNow.Minute <= 10)
                    {
                        Cache.Instance.Log("EVEManagerThread: if (DateTime.UtcNow.Hour ==  [" + HourToUseToCalcDowntimeOver(Ea.ConnectToTestServer) + "] && DateTime.UtcNow.Minute < 10)");
                        //KillEveInstances();
                        Thread.Sleep(10);
                        return false;
                    }
                }
                else if (Ea == null)
                {
                    if (DateTime.UtcNow.Hour == HourToUseToCalcDowntimeComing() && DateTime.UtcNow.Minute > 59)
                    {
                        Cache.Instance.Log("EveManagerStartEveForTheseAccounts: if (DateTime.UtcNow.Hour == [" + HourToUseToCalcDowntimeComing() + "] && DateTime.UtcNow.Minute > 40)!!!");
                        return false;
                    }

                    if (DateTime.UtcNow.Hour == HourToUseToCalcDowntimeOver() && DateTime.UtcNow.Minute <= 10)
                    {
                        Cache.Instance.Log("EVEManagerThread: if (DateTime.UtcNow.Hour ==  [" + HourToUseToCalcDowntimeOver() + "] && DateTime.UtcNow.Minute < 10)!!!");
                        //KillEveInstances();
                        //Thread.Sleep(10);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return false;
            }
        }

        private void TwentyFourHours()
        {
            //after downtime - we should wait this long before we start any characters

            if (Cache.Instance.EveSettings.Last24HourTS.AddHours(24) < DateTime.UtcNow)
            {
                Cache.Instance.Log(string.Format("EVEManagerThread: Once Per Day: LastHourTS was: [" + Cache.Instance.EveSettings.LastHourTS.ToShortTimeString() + "] now: [" + DateTime.UtcNow.ToShortTimeString() + "]"));
                Cache.Instance.EveSettings.Last24HourTS = DateTime.UtcNow;
            }
        }

        private static bool OncePerInterval()
        {
            try
            {
                int accountNum = 0;

                if (Cache.Instance.EveSettings.LastHourTS.AddHours(1) < DateTime.UtcNow)
                {
                    //Cache.Instance.Log(string.Format("EVEManagerThread: Once Per Hour: LastHourTS was: [" + Cache.Instance.EveSettings.LastHourTS.ToShortTimeString() + "] now: [" + DateTime.UtcNow.ToShortTimeString() + "]"));
                    Cache.Instance.EveSettings.LastHourTS = DateTime.UtcNow;
                    //Cache.Instance.XMLBackupRotate();

                    accountNum = 0;
                    foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(i => i != null && !string.IsNullOrEmpty(i.AccountName) && !string.IsNullOrEmpty(i.CharacterName) && !i.EveProcessExists && !i.SelectedController.Equals(nameof(EveAccount.AvailableControllers.None))).ToList())
                    {
                        Thread.Sleep(1);
                        accountNum++;
                        //Cache.Instance.Log(string.Format("EVEManagerThread: Once Per Hour: Account [" + accountNum + "][" + eA.AccountName + "] Character [" + eA.CharacterName + "]"));
                        //if (eA.EndTime < DateTime.UtcNow && eA.StartTime.AddHours(10) < DateTime.UtcNow)

                        if (eA.ManuallyStarted && DateTime.UtcNow > eA.LastEveClientLaunched.AddHours(4))
                        {
                            Cache.Instance.Log(string.Format("Account [" + accountNum + "][" + eA.MaskedAccountName + "] Character [" + eA.MaskedCharacterName + "] was manually logged in more than 6 hours ago. Resetting manual flags to false."));
                            eA.ManuallyStarted = false;
                            eA.ManuallyPausedViaUI = false;
                            continue;
                        }

                        return true;
                    }

                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return false;
            }
        }

        private void EmptyStandbyListEverySoOften()
        {
            //https://digitalitskills.com/solving-memory-issues-with-rammap-exe-command-line/

            try
            {
                if (Cache.Instance.EveSettings.LastEmptyStandbyList.AddMinutes(15) < DateTime.UtcNow)
                {
                    Cache.Instance.Log(string.Format("EVEManagerThread: Once every 15 min: LastEmptyStandbyList was: [" + Cache.Instance.EveSettings.LastEmptyStandbyList.ToShortTimeString() + "] now: [" + DateTime.UtcNow.ToShortTimeString() + "]"));
                    Cache.Instance.EveSettings.LastEmptyStandbyList = DateTime.UtcNow;
                    try
                    {
                        const string ExeFileToLaunch = "c:\\eveoffline\\questorlauncher\\EmptyStandbyList.exe";
                        if (File.Exists(ExeFileToLaunch))
                        {
                            ProcessStartInfo processStartInfo = new ProcessStartInfo(ExeFileToLaunch, "standbylist")
                            {
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            };
                            Process.Start(processStartInfo);
                            Cache.Instance.Log("Clearing Memory Standby List so we have more memory to use...");
                        }
                        else Cache.Instance.Log("Clearing Memory Standby List: Failed: Missing [" + ExeFileToLaunch + "]");
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("Exception: [" + ex + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return;
            }
        }

        private void EmptyStandbyListWorkingSetsEverySoOften()
        {
            try
            {
                //we do this per process in the bot now. disabling for now: note: this cleared the workingset for EVERY process on the system, including chrome and such!
                return;
                if (Cache.Instance.EveSettings.LastEmptyWorkingSets.AddSeconds(580) < DateTime.UtcNow)
                {
                    Cache.Instance.Log(string.Format("EVEManagerThread: Once every 3 min: LastEmptyWorkingSets was: [" + Cache.Instance.EveSettings.LastEmptyWorkingSets.ToShortTimeString() + "] now: [" + DateTime.UtcNow.ToShortTimeString() + "]"));
                    Cache.Instance.EveSettings.LastEmptyWorkingSets = DateTime.UtcNow;
                    try
                    {
                        const string ExeFileToLaunch = "c:\\eveoffline\\questorlauncher\\EmptyStandbyList.exe";
                        if (File.Exists(ExeFileToLaunch))
                        {
                            ProcessStartInfo processStartInfo = new ProcessStartInfo(ExeFileToLaunch, "workingsets")
                            {
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            };
                            Process.Start(processStartInfo);
                            Cache.Instance.Log("Clearing Memory workingsets List so we have more memory to use...");
                        }
                        else Cache.Instance.Log("Clearing Memory Standby List: Failed: Missing [" + ExeFileToLaunch + "]");
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("Exception: [" + ex + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return;
            }
        }

        private void KillEveAccountEveProcessesAsNeeded()
        {
            try
            {
                foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(x => !string.IsNullOrEmpty(x.AccountName) && !string.IsNullOrEmpty(x.CharacterName)).OrderByDescending(i => i.IsInAbyss).ThenByDescending(i => i.InMission))
                {
                    Thread.Sleep(1);
                    //Cache.Instance.Log("Account [" + eA.AccountName + "] has an EVE Running: IsProcessAlive() [" + eA.IsProcessAlive() + "] RamUsage [" + eA.RamUsage + "] Max_Memory [" + eA.MAX_MEMORY + "]"); //ShouldWeKillEveIfProcessNotAlive [" + eA.ShouldWeKillEveIfProcessNotAlive + "]");

                    if (eA.RamUsage > eA.MAX_MEMORY && (Cache.Instance.EveSettings.KillUnresponsiveEvEs ?? true && !eA.ManuallyPausedViaUI && !eA.IsInAbyss && eA.IsDocked))
                    {
                        //
                        // When docked: check for memory usage and close eve if we get above the memory usage threshold
                        //
                        //eA.RestartOfEveClientNeeded = true;
                        string msg = "Account [" + eA.MaskedAccountName + "] Character [" + eA.MaskedCharacterName + "] RamUsage [" + eA.RamUsage + "] > [" + eA.MAX_MEMORY + "] Restart?";
                        Cache.Instance.Log(msg);
                        //eA.KillEveProcess();
                    }

                    if (eA.SelectedController == nameof(EveAccount.AvailableControllers.WspaceSiteController))
                        continue;

                    if (eA.SelectedController == nameof(EveAccount.AvailableControllers.CombatDontMoveController))
                        continue;

                    if (eA.SelectedController == nameof(EveAccount.AvailableControllers.HydraController))
                        continue;

                    if (eA.SelectedController == nameof(EveAccount.AvailableControllers.None))
                        continue;

                    if (eA.InteractedWithEVERecently)
                        continue;

                    if (!eA.EveProcessExists)
                        continue;

                    if (eA.ClientSettingsSerializationErrors > eA.MAX_SERIALIZATION_ERRORS && !eA.IgnoreSeralizationErrors && eA.IsDocked)
                    {
                        Cache.Instance.Log("Client settings serialization error! Disabling this instance.");
                        eA.UseScheduler = false;
                        eA.KillEveProcess();
                        Thread.Sleep(10);
                        continue;
                    }

                    if (!eA.IsProcessAlive()) //&& eA.ShouldWeKillEveIfProcessNotAlive)
                    {
                        //
                        // check for eve client responsiveness and if we think the client is unresponsive, close eve.
                        //
                        eA.RestartOfEveClientNeeded = true;
                        Cache.Instance.Log("Stopping Eve: Account [" + eA.MaskedAccountName + "] Character[" + eA.MaskedCharacterName + "] as it was not responding");
                        eA.KillEveProcess();
                        Thread.Sleep(10);
                        continue;
                    }

                    if (!eA.ShouldBeStopped && eA.IsDocked && (eA.UseScheduler && !eA.ManuallyStarted) && !string.IsNullOrEmpty(eA.CharacterName) && (Cache.Instance.EveSettings.KillUnresponsiveEvEs ?? true) && !eA.ManuallyPausedViaUI && !eA.ManuallyStarted && !eA.IsInAbyss && eA.IsDocked)
                    {
                        //
                        // When docked: check the schedule and close eve if we are done running for the day.
                        //
                        Cache.Instance.Log("Stopping Eve: Account [" + eA.MaskedAccountName + "] Character [" + eA.MaskedCharacterName + "] to comply with the schedule");
                        //eA.KillEveProcess();
                        //Thread.Sleep(10);
                        continue;
                    }

                    continue;
                }

                return;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return;
            }
        }

        public static DateTime LastSchedulerIteration = DateTime.UtcNow;
        public static DateTime LastIPCIteration = DateTime.UtcNow;

        private void ProcessQueueAndStartEveForTheseEveAccounts()
        {
            try
            {
                //Cache.Instance.Log("EVEManagerThread: NextEveManagerStartEveForTheseAccountsThreadTimeStamp [" + NextEveManagerStartEveForTheseAccountsThreadTimeStamp.ToLongTimeString() + "]");

                if (NextEveManagerStartEveForTheseAccountsThreadTimeStamp > DateTime.UtcNow)
                    return;

                //Cache.Instance.Log("EVEManagerThread: if (NextEveManagerStartEveForTheseAccountsThreadTimeStamp > DateTime.UtcNow)");

                NextEveManagerStartEveForTheseAccountsThreadTimeStamp = DateTime.UtcNow.AddSeconds(5);
                LastIPCIteration = DateTime.UtcNow;

                if (EveManagerStart.AddSeconds(3) > DateTime.UtcNow) // && !WeHaveToonsCurrentlyDisconnected)
                {
                    Cache.Instance.Log("EVEManagerThread: waiting a few seconds before launching any eve clients");
                    NextEveManagerStartEveForTheseAccountsThreadTimeStamp = DateTime.UtcNow.AddSeconds(5);
                    return;
                }

                TwentyFourHours();

                if (!OncePerInterval())
                {
                    NextEveManagerStartEveForTheseAccountsThreadTimeStamp = DateTime.UtcNow.AddSeconds(30);
                    return;
                }

                if (Cache.StartEveForTheseAccountsQueue.Any())
                {
                    Cache.Instance.Log("EVEManagerThread: NextLogAccountsQueuedTimeStamp [" + NextLogAccountsQueuedTimeStamp.ToShortDateString() + NextLogAccountsQueuedTimeStamp.ToLongTimeString() + "]");

                    //if (DateTime.UtcNow > NextLogAccountsQueuedTimeStamp)
                    //    return;

                    Cache.Instance.Log("EVEManagerThread: !if (DateTime.UtcNow > NextLogAccountsQueuedTimeStamp)");

                    //NextLogAccountsQueuedTimeStamp = DateTime.UtcNow.AddMinutes(5);
                    Cache.Instance.Log("StartEveForTheseAccountsQueue has [" + Cache.StartEveForTheseAccountsQueue.Count() + "] Accounts in the Queue");
                    int intAccountToBeStarted = 0;
                    foreach (EveAccount eA in Cache.StartEveForTheseAccountsQueue)
                    {
                        intAccountToBeStarted++;
                        Cache.Instance.Log("[" + intAccountToBeStarted + "] EveAccount [" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] Queued to Start");
                    }
                }

                KillEveAccountEveProcessesAsNeeded();

                PruneTheStartEveForTheseAccountsQueue();

                EveAccount eveAccountToLaunch = new EveAccount();
                if (Cache.StartEveForTheseAccountsQueue.Any(i => i.Pid == 0))
                {
                    LogToonsLeftToLogin();

                    eveAccountToLaunch = Cache.StartEveForTheseAccountsQueue.Dequeue();

                    if (!SafeToProceedAfterDownTime(eveAccountToLaunch))
                    {
                        Cache.Instance.Log("EVEManagerThread: !SafeToProceedAfterDownTime [" + eveAccountToLaunch.MaskedAccountName + "][" + eveAccountToLaunch.MaskedCharacterName + "]");
                        NextEveManagerStartEveForTheseAccountsThreadTimeStamp = DateTime.UtcNow.AddMinutes(1);
                        return;
                    }

                    eveAccountToLaunch.LastEveClientPulledFromQueue = DateTime.UtcNow;
                    if (eveAccountToLaunch.Pid != 0)
                        return;

                    if (!eveAccountToLaunch.ThisAccountIsSafeToBeStarted)
                        return;

                    if (!string.IsNullOrEmpty(eveAccountToLaunch.AccountName) && eveAccountToLaunch.AccountName != "exampleUser")
                    {
                        if (eveAccountToLaunch.LastStartEveQueuePriority == EveAccountStartPriority.ManuallyStartedPriority)
                            eveAccountToLaunch.ManuallyStarted = true;
                        //Launch eve here
                        Cache.Instance.Log("[" + eveAccountToLaunch.Num + "][" + eveAccountToLaunch.MaskedAccountName + "][" + eveAccountToLaunch.MaskedCharacterName + "] StartEveInject: pid [" + eveAccountToLaunch.Pid + "] EveProcessExists [" + eveAccountToLaunch.EveProcessExists + "]");
                        eveAccountToLaunch.StartEveInject();

                        int intWaitForEveProcessRetries = 0;
                        while (!eveAccountToLaunch.EveProcessExists && 100 > intWaitForEveProcessRetries)
                        {
                            intWaitForEveProcessRetries++;
                            Thread.Sleep(100);
                            continue;
                        }

                        if (eveAccountToLaunch.EveProcessExists)
                        {
                            NextUpdateLeaderAndSlaveStaticInfo = DateTime.UtcNow.AddMinutes(1);
                            NextEveManagerStartEveForTheseAccountsThreadTimeStamp = DateTime.UtcNow.AddSeconds(Util.GetRandom(5, 15));
                            LogToonsLeftToLogin();
                            return;
                        }

                        NextEveManagerStartEveForTheseAccountsThreadTimeStamp = DateTime.UtcNow.AddSeconds(3);
                        eveAccountToLaunch.LastEveClientPulledFromQueue = DateTime.UtcNow.AddDays(-1);
                        return;
                    }

                    Cache.Instance.Log("Failed to launch Eve for AccountNum [" + eveAccountToLaunch.Num + "] AccountName was empty?! or hasnt been changed from the default of exampleUser");
                    return;
                }

                //
                // No Accounts yet queued to start
                //
                NextEveManagerStartEveForTheseAccountsThreadTimeStamp = DateTime.UtcNow.AddSeconds(2);
                return;
            }
            catch (ThreadAbortException)
            {
                isEveManagerDecideThreadAborting = true;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        private void PruneTheStartEveForTheseAccountsQueue()
        {
            try
            {
                if (Cache.StartEveForTheseAccountsQueue.Any(i => i.Pid != 0))
                {
                    SharedComponents.FastPriorityQueue.SimplePriorityQueue<EveAccount> tempStartEveForTheseAccountsQueue = Cache.StartEveForTheseAccountsQueue;
                    foreach (EveAccount eA in tempStartEveForTheseAccountsQueue.Where(i => i.Pid != 0))
                    {
                        if (Cache.StartEveForTheseAccountsQueue.Contains(eA))
                        {
                            Cache.StartEveForTheseAccountsQueue.Remove(eA);
                            continue;
                        }

                        continue;
                    }

                    return;
                }

                return;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return;
            }
        }

        private void DecideWhatEveAccountsToQueueToStart()
        {
            try
            {
                if (NextEveManagerThreadTimeStamp > DateTime.UtcNow)
                    return;

                NextEveManagerThreadTimeStamp = DateTime.UtcNow.AddSeconds(1);
                LastSchedulerIteration = DateTime.UtcNow;

                //Cache.Instance.Log("DecideWhatEveAccountsToQueueToStart: Iteration");

                if (EveManagerStart.AddSeconds(10) > DateTime.UtcNow) // && !WeHaveToonsCurrentlyDisconnected)
                {
                    Cache.Instance.Log("EVEManagerThread: waiting a few seconds before processing the schedule");
                    NextEveManagerThreadTimeStamp = DateTime.UtcNow.AddSeconds(10);
                    return;
                }

                //Cache.Instance.Log("EVEManagerThread: SafeToProceedAfterDownTime");

                foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(x => !string.IsNullOrEmpty(x.AccountName) && !string.IsNullOrEmpty(x.CharacterName)).RandomPermutation().OrderByDescending(i => i.IsInAbyss).ThenByDescending(i => i.InMission).ThenByDescending(i => !i.IsDocked))
                {
                    Thread.Sleep(1);
                    if (eA.DebugScheduler) Cache.Instance.Log("DecideWhatEveAccountsToQueueToStart: 1");

                    if (string.IsNullOrEmpty(eA.GUID))
                    {
                        eA.GUID = Guid.NewGuid().ToString();
                    }

                    if (eA.DebugScheduler) Cache.Instance.Log("DecideWhatEveAccountsToQueueToStart: 2");

                    if (Cache.StartEveForTheseAccountsQueue.Any(i => i.GUID == eA.GUID || i.AccountName == eA.AccountName))
                        continue;

                    if (eA.DebugScheduler) Cache.Instance.Log("DecideWhatEveAccountsToQueueToStart: 3");

                    /**
                    if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(x => !string.IsNullOrEmpty(x.AccountName) && !string.IsNullOrEmpty(x.CharacterName))
                        .Any(x => x.UseScheduler &&
                                 !x.PatternManagerEnabled &&
                                 !x.IsCachedStartTimeStillValid &&
                                 (x.StartTime.AddHours(x.HoursPerDay) > DateTime.UtcNow)))
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    **/

                    if (eA.LastEveClientPulledFromQueue.AddSeconds(30) > DateTime.UtcNow)
                        continue;

                    if (eA.DebugScheduler) Cache.Instance.Log("DecideWhatEveAccountsToQueueToStart: 4");

                    if (!SafeToProceedAfterDownTime(eA))
                    {
                        Thread.Sleep(1);
                        Cache.Instance.Log("EVEManagerThread: !SafeToProceedAfterDownTime: Waiting 1 minute");
                        NextEveManagerThreadTimeStamp = DateTime.UtcNow.AddMinutes(1);
                        return;
                    }

                    if (eA.DebugScheduler) Cache.Instance.Log("DecideWhatEveAccountsToQueueToStart: 5");

                    if (!eA.EveProcessExists)
                    {
                        Thread.Sleep(1);
                        if (eA.DebugScheduler) Cache.Instance.Log("[" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] EveProcessExists [" + eA.EveProcessExists + "]");
                        if ( DateTime.UtcNow > eA.LastQuestorStarted.AddSeconds(45))
                        {
                            if (eA.DebugScheduler) Cache.Instance.Log("[" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] if ( DateTime.UtcNow > eA.LastQuestorStarted.AddSeconds(45))");
                            if (eA.ShouldBeStarted)
                            {
                                Cache.Instance.Log("[" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] EveProcessExists [" + eA.EveProcessExists + "] ShouldldBeRunningReason [" + eA.ShouldldBeRunningReason + "] ShouldBeStarted [" + eA.ShouldBeStarted + "]");
                                eA.QueueThisAccountToBeStarted(eA.StartEvePriorityLevel, "if (!eA.EveProcessExists && eA.ShouldBeStarted)");
                                NextEveManagerThreadTimeStamp = DateTime.UtcNow.AddMilliseconds(Util.GetRandom(800, 1700));
                                continue;
                            }

                            if (eA.DebugScheduler) Cache.Instance.Log("False [" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] EveProcessExists [" + eA.EveProcessExists + "] ShouldldBeRunningReason [" + eA.ShouldldBeRunningReason + "] ShouldBeStarted [" + eA.ShouldBeStarted + "]");
                        }
                    }
                    else if (eA.DebugScheduler) Cache.Instance.Log("[" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] EveProcessExists [" + eA.EveProcessExists + "].");

                    if (eA.DebugScheduler) Cache.Instance.Log("DecideWhatEveAccountsToQueueToStart: 6");

                    //Cache.Instance.Log("Not Starting Eve: Account [" + eA.AccountName + "] Character [" + eA.CharacterName + "] Running [" + eA.Running + "] ShouldBeRunning [" + eA.ShouldBeRunning + "] ShouldBeStarted [" + eA.ShouldBeStarted + "]");
                }

                //if (Cache.StartEveForTheseAccountsQueue.Any())
                //    NextEveManagerThreadTimeStamp = DateTime.UtcNow.AddSeconds(7);

                return;
            }
            catch (ThreadAbortException tae)
            {
                Thread.Sleep(1);
                isEveManagerDecideThreadAborting = true;
                Cache.Instance.Log("Exception [" + tae + "]");
            }
            catch (Exception ex)
            {
                Thread.Sleep(1);
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        private void EveManagerDecideThread()
        {
            while (!isEveManagerDecideThreadAborting)
            {
                try
                {
                    DecideWhatEveAccountsToQueueToStart();
                    Thread.Sleep(350);
                }
                catch (Exception)
                {
                    //ignore this exception
                }
            }

            Cache.Instance.EveSettings.IsSchedulerRunning = false;
            Cache.StartEveForTheseAccountsQueue.Clear();
            Cache.Instance.Log("Stopped EveManager: Scheduler Off");
        }

        public DateTime LastRebuildSlaveList = DateTime.MinValue;
        public static DateTime NextUpdateLeaderAndSlaveStaticInfo = DateTime.UtcNow.AddSeconds(10);
        public static DateTime NextUpdateLeaderAndSlaveDynamicInfo = DateTime.UtcNow.AddSeconds(13);

        public static void UpdateLeaderAndSlaveStaticInfo(bool AddLogging = false)
        {
            try
            {
                if (NextUpdateLeaderAndSlaveStaticInfo > DateTime.UtcNow)
                    return;

                NextUpdateLeaderAndSlaveStaticInfo = DateTime.UtcNow.AddMinutes(60);

                foreach (EveAccount thisEveAccount in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(x => !string.IsNullOrEmpty(x.AccountName) && !string.IsNullOrEmpty(x.CharacterName)))
                {
                    ClearSlaveCharacters(thisEveAccount);
                }

                int LeaderNumber = 0;
                foreach (EveAccount thisLeaderEveAccount in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(x => !string.IsNullOrEmpty(x.AccountName) && !string.IsNullOrEmpty(x.CharacterName) && x.IsLeader).OrderBy(i => i.Num))
                {
                    LeaderNumber++;
                    thisLeaderEveAccount.BoolNewSignatureDetected = false;
                    if (AddLogging) Cache.Instance.Log("EveSettingsManagerThread: Found Leader [" + LeaderNumber + "] ChatChannelToPullFleetInvitesFrom [" + thisLeaderEveAccount.ChatChannelToPullFleetInvitesFrom + "][" + thisLeaderEveAccount.MaskedCharacterName + "] ConnectToTestServer [" + thisLeaderEveAccount.ConnectToTestServer + "]");
                    int intSlave = 0;

                    try
                    {
                        //
                        // use the Leader account (if any) to copy targets from to the rest of the accounts to (potentially) use
                        //
                        intSlave = 0;
                        foreach (EveAccount thisSlaveEveAccount in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(e => !string.IsNullOrEmpty(e.AccountName) && !string.IsNullOrEmpty(e.CharacterName) && thisLeaderEveAccount.ChatChannelToPullFleetInvitesFrom == e.ChatChannelToPullFleetInvitesFrom && !e.IsLeader).OrderBy(i => i.GUID))
                        {
                            intSlave++;
                            if (AddLogging) Cache.Instance.Log("EveSettingsManagerThread: Found Slave# [" + intSlave + "] ChatChannelToPullFleetInvitesFrom [" + thisLeaderEveAccount.ChatChannelToPullFleetInvitesFrom + "][" + thisSlaveEveAccount.MaskedCharacterName + "] ConnectToTestServer [" + thisSlaveEveAccount.ConnectToTestServer + "]: UpdateStaticSlavesDataToLeader");
                            UpdateStaticSlavesDataToLeader(thisLeaderEveAccount, thisSlaveEveAccount, AddLogging);
                            continue;
                        }

                        intSlave = 0;
                        foreach (EveAccount thisSlaveEveAccount in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(e => !string.IsNullOrEmpty(e.AccountName) && !string.IsNullOrEmpty(e.CharacterName) && thisLeaderEveAccount.ChatChannelToPullFleetInvitesFrom == e.ChatChannelToPullFleetInvitesFrom && !e.IsLeader).OrderBy(i => i.GUID))
                        {
                            intSlave++;
                            if (AddLogging) Cache.Instance.Log("EveSettingsManagerThread: Found Slave# [" + intSlave + "] ChatChannelToPullFleetInvitesFrom [" + thisLeaderEveAccount.ChatChannelToPullFleetInvitesFrom + "][" + thisSlaveEveAccount.MaskedCharacterName + "] ConnectToTestServer [" + thisSlaveEveAccount.ConnectToTestServer + "]: CopyStaticLeaderInfoToSlaves");
                            CopyStaticLeaderInfoToSlaves(thisSlaveEveAccount, thisLeaderEveAccount, AddLogging);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("Exception: " + ex);
                        return;
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return;
            }
        }

        private void UpdateLeaderAndSlaveDynamicInfo()
        {
            try
            {
                if (NextUpdateLeaderAndSlaveDynamicInfo > DateTime.UtcNow)
                    return;

                NextUpdateLeaderAndSlaveDynamicInfo = DateTime.UtcNow.AddSeconds(3);

                foreach (EveAccount thisLeaderEveAccount in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(i => !string.IsNullOrEmpty(i.AccountName) && !string.IsNullOrEmpty(i.CharacterName) && i.IsLeader && i.SelectedController == nameof(EveAccount.AvailableControllers.HydraController) && i.Pid != 0).OrderBy(i => i.Num))
                {
                    thisLeaderEveAccount.BoolNewSignatureDetected = false;
                    //Cache.Instance.Log("EveSettingsManagerThread: Found Leader [" + thisLeaderAccount.CharacterName + "]");
                    int intSlave = 0;

                    try
                    {
                        intSlave = 0;
                        foreach (EveAccount thisSlaveEveAccount in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(e => !string.IsNullOrEmpty(e.AccountName) && !string.IsNullOrEmpty(e.CharacterName) && thisLeaderEveAccount.ChatChannelToPullFleetInvitesFrom == e.ChatChannelToPullFleetInvitesFrom && !e.IsLeader && e.InteractedWithEVERecently && e.SelectedController == nameof(EveAccount.AvailableControllers.HydraController)).OrderBy(j => j.Num))
                        {
                            intSlave++;
                            //Cache.Instance.Log("EveSettingsManagerThread: Found Slave# [" + intSlave + "][" + thisSlaveAccount.CharacterName + "]");
                            CopyLeaderInfoToSlaves(thisSlaveEveAccount, thisLeaderEveAccount);
                            CopyDynamicLeaderInfoToSlaves(thisSlaveEveAccount, thisLeaderEveAccount);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("Exception: " + ex);
                        return;
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return;
            }
        }

        private void EveSettingsManagerThread()
        {
            while (!isEveSettingsManagerThreadAborting)
            {
                try
                {
                    if (DateTime.UtcNow < NextEveManagerUpdateSlaveThreadTimeStamp)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    NextEveManagerUpdateSlaveThreadTimeStamp = DateTime.UtcNow.AddSeconds(1);

                    //foreach (EveAccount thisEveAccount in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(i => !string.IsNullOrEmpty(i.AccountName) && !string.IsNullOrEmpty(i.CharacterName) && i.LastQuestorStarted.AddSeconds(10) > DateTime.UtcNow && i.UseFleetMgr && i.SelectedController == nameof(EveAccount.AvailableControllers.HydraController))
                    //{
                    //    EveManager.NextUpdateLeaderAndSlaveStaticInfo = DateTime.UtcNow.AddMinutes(-1);
                    //    ClearSlaveCharacters(thisEveAccount);
                    //}

                    UpdateLeaderAndSlaveStaticInfo(true);
                    Thread.Sleep(1);
                    //UpdateLeaderAndSlaveDynamicInfo();
                    //Thread.Sleep(1);
                    EmptyStandbyListEverySoOften();
                    Thread.Sleep(1);
                    EmptyStandbyListWorkingSetsEverySoOften();
                    Thread.Sleep(1);
                    ProcessQueueAndStartEveForTheseEveAccounts();
                    Thread.Sleep(1);

                    if (DateTime.UtcNow > NextEveManagerHideConsoleWindows)
                    {
                        NextEveManagerHideConsoleWindows = DateTime.UtcNow.AddSeconds(24);
                        foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(i => !string.IsNullOrEmpty(i.AccountName) && !string.IsNullOrEmpty(i.CharacterName) && !i.Console))
                            eA.HideConsoleWindow();
                    }

                    //if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(i => i.EveProcessExists && i.boolNewSignatureDetected))
                    //    Cache.Instance.Log("New Signature Found!");
                }
                catch (ThreadAbortException tae)
                {
                    Cache.Instance.Log("Exception: " + tae);
                    isEveSettingsManagerThreadAborting = true;
                }
                catch (InvalidOperationException)
                {
                    //ignore this exception
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception: " + ex);
                    continue;
                }
            }

            Cache.Instance.Log("Stopped EveSettingsManager");
        }

        private void KillEveInstancesDelayedThread()
        {
            while (IsAnyEveProcessAlive && (Cache.Instance.EveSettings.KillUnresponsiveEvEs ?? true))
            {
                foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List)
                {
                    Thread.Sleep(50);
                    if (eA.EveProcessExists && !eA.ManuallyPausedViaUI && !eA.ManuallyStarted)
                    {
                        eA.KillEveProcess();
                    }
                }

                Thread.Sleep(1000);
            }
            Cache.Instance.Log("Finished stopping all eve instances.");
        }

        #endregion Methods
    }

    public class EveManager2 : IDisposable
    {
        #region Fields

        //private static XElement IndividualAccountDataIpcXml = null;
        private static DateTime _lastModifiedDateOfAccountDataIpcFile = DateTime.MinValue;

        private static XElement AccountDataIpcXml;

        #endregion Fields

        #region Properties

        public static string AccountDataIpcFileName //just the filename, no path, without file extension
        {
            get
            {
                try
                {
                    return "AccountDataIpc";
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return "common.xml";
                }
            }
        }

        public static string AccountDataIpcFilePath
        {
            get
            {
                try
                {
                    string FullPathToEveSharpLauncherExe = Assembly.GetExecutingAssembly().Location;
                    string DirectoryWhereEveSharpLauncherWasLaunched = Path.GetDirectoryName(FullPathToEveSharpLauncherExe);
                    string _accountDataIpcFilePath = Path.Combine(DirectoryWhereEveSharpLauncherWasLaunched, "EveSharpSettings", AccountDataIpcFileName + ".xml");
                    return _accountDataIpcFilePath;
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return string.Empty;
                }
            }
        }

        private static DateTime NextLoadSettings { get; set; } = DateTime.UtcNow;

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            //DisposeEveManager();
            //DisposeEveSettingsManager();
        }

        /**
        public void Old_LoadSettings(bool forcereload = false)
        {
            try
            {
                if (DateTime.UtcNow < NextLoadSettings)
                    return;

                NextLoadSettings = DateTime.UtcNow.AddSeconds(10);

                try
                {
                    bool reloadSettings = true;
                    if (File.Exists(AccountDataIpcFilePath))
                    {
                        reloadSettings = _lastModifiedDateOfAccountDataIpcFile != File.GetLastWriteTime(AccountDataIpcFilePath);
                        if (!reloadSettings && forcereload) reloadSettings = true;

                        if (!reloadSettings)
                            return;
                    }
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                }

                if (File.Exists(AccountDataIpcFilePath))
                {
                    using (XmlTextReader reader = new XmlTextReader(AccountDataIpcFilePath))
                    {
                        reader.EntityHandling = EntityHandling.ExpandEntities;
                        AccountDataIpcXml = XDocument.Load(reader).Root;
                    }

                    if (AccountDataIpcXml == null)
                    {
                        Cache.Instance.Log("unable to find [" + AccountDataIpcFilePath + "] FATAL ERROR");
                    }
                    else
                    {
                        try
                        {
                            if (File.Exists(AccountDataIpcFilePath)) _lastModifiedDateOfAccountDataIpcFile = File.GetLastWriteTime(AccountDataIpcFilePath);
                            LoadSettings();
                        }
                        catch (Exception ex)
                        {
                            Cache.Instance.Log("Exception [" + ex + "]");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Problem creating directories for logs [" + ex + "]");
            }
        }

        public void LoadSettings()
        {
            try
            {
                Cache.Instance.Log("Start reading EVESharp settings from xml");

                if (!string.IsNullOrEmpty(AccountDataIpcFilePath) && File.Exists(AccountDataIpcFilePath))
                {
                    Cache.Instance.Log("Loading AccountData IPC XML [" + AccountDataIpcFilePath + "]");
                    AccountDataIpcXml = XDocument.Load(AccountDataIpcFilePath).Root;
                    if (AccountDataIpcXml == null)
                        Cache.Instance.Log("found [" + AccountDataIpcXml + "] but was unable to load it: FATAL ERROR!");
                }
                else
                {
                    //
                    // if the common XML does not exist, load the characters XML into the CommonSettingsXml just so we can simplify the XML element loading stuff.
                    //
                    Cache.Instance.Log("AccountData IPC XML [" + AccountDataIpcFilePath + "] not found.");
                }

                if (AccountDataIpcXml == null)
                    return;
                // this should never happen as we load the characters xml here if the common xml is missing. adding this does quiet some warnings though

                Cache.Instance.Log("Loading Settings from [" + AccountDataIpcFilePath + "]");

                //
                // these are listed by feature and should likely be re-ordered to reflect that
                //

                //
                // Enable / Disable the different types of logging that are available
                //
                //Statistics.WreckLootStatistics = (bool?)CharacterSettingsXml.Element("WreckLootStatistics") ?? true;
                //Statistics.MissionDungeonIdLog = (bool?)CharacterSettingsXml.Element("MissionDungeonIdLog") ?? true;
            }
            catch (Exception exception)
            {
                Cache.Instance.Log("LoadSettings: Exception [" + exception + "]");
            }
        }
        **/

        #endregion Methods
    }
}