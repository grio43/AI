/*
 * ---------------------------------------
 * User: duketwo
 * Date: 07.03.2014
 * Time: 15:50
 *
 * ---------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using SharedComponents.IPC;
using SharedComponents.Utility;
using SharedComponents.Utility.AsyncLogQueue;

namespace SharedComponents.EVE
{
    /// <summary>
    ///     Description of Cache.
    /// </summary>
    public class Cache
    {
        public delegate void IsShuttingDownDelegate();
        private static readonly Cache _instance = new Cache();
        public static int CacheInstances = 0;

        public static bool IsShuttingDown = false;
        public static bool IsServer = false;

        private string _assemblyPath = null;

        public SerializeableSortableBindingList<EveAccount> EveAccountSerializeableSortableBindingList;

        public SerializeableSortableBindingList<EveSetting> EveSettingsSerializeableSortableBindingList;

        public static FastPriorityQueue.SimplePriorityQueue<EveAccount> StartEveForTheseAccountsQueue = new FastPriorityQueue.SimplePriorityQueue<EveAccount>();


        private Cache()
        {
        }

        public EveSetting EveSettings
        {
            get
            {
                try
                {
                    if (!IsServer)
                        return WCFClient.Instance.GetPipeProxy.GetEVESettings();

                    if (!EveSettingsSerializeableSortableBindingList.List.Any())
                        EveSettingsSerializeableSortableBindingList.List.Add(new EveSetting("C:\\eveoffline\\bin\\exefile.exe", DateTime.MinValue));
                    return EveSettingsSerializeableSortableBindingList.List[0];
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception: " + ex);
                    return null;
                }
            }
        }
        public string AssemblyPath
        {
            get
            {
                if (_assemblyPath == null)
                    _assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return _assemblyPath;
            }
            set => _assemblyPath = value;
        }

        public string EveLocation => Instance.EveSettings.EveDirectory;

        public bool IsMainFormMinimized { get; set; }

        public Int64 MainFormHWnd { get; set; }

        public static Cache Instance
        {
            get
            {
                if (IsServer && !_isInitialized)
                    _instance.LoadSettingsFromPersistentStorage();
                return _instance;
            }
        }

        public static event IsShuttingDownDelegate IsShuttingDownEvent;

        public static void BroadcastShutdown()
        {
            IsShuttingDownEvent?.Invoke();
        }

        public bool GetXMLFilesLoaded => _xmlFilesLoaded;

        private static volatile bool _isInitialized;
        private static bool _xmlFilesLoaded;
        private static object initLock = new object();

        public void LoadSettingsFromPersistentStorage()
        {
            if (_isInitialized)
                return;

            lock (initLock)
            {
                if (_isInitialized)
                    return;

                _isInitialized = true;

                if (!IsServer)
                    return;

                InitTokenSource();

                string accountDataXML = "AccountData.xml";
                string eveSettingsXML = "EveSettings.xml";
                string questorLauncherSettingsPath = AssemblyPath + "\\EVESharpSettings\\";
                string accountDataXMLWithPath = questorLauncherSettingsPath + accountDataXML;
                if (!File.Exists(accountDataXMLWithPath))
                {
                    Cache.Instance.Log("Missing [" + accountDataXMLWithPath + "]");
                    if (File.Exists(questorLauncherSettingsPath + "AcccountData.xml"))
                    {
                        File.Copy(questorLauncherSettingsPath + "AcccountData.xml", accountDataXMLWithPath);
                    }
                }

                string eveSettingsXMLWithPath = questorLauncherSettingsPath + eveSettingsXML;
                if (!File.Exists(eveSettingsXMLWithPath))
                {
                    Cache.Instance.Log("Missing [" + eveSettingsXMLWithPath + "]");
                }

                string accountDataXMLWithPathDefunct = questorLauncherSettingsPath + accountDataXML + ".defunct";
                string eveSettingsXMLWithPathDefunct = questorLauncherSettingsPath + eveSettingsXML + ".defunct";
                string accountDataXMLWithPathBackup = questorLauncherSettingsPath + accountDataXML + "[" + DateTime.Now.DayOfWeek + "][" + System.Environment.MachineName + "].bak";
                string eveSettingsXMLWithPathBackup = questorLauncherSettingsPath + eveSettingsXML + "[" + DateTime.Now.DayOfWeek + "][" + System.Environment.MachineName + "].bak";

                try
                {
                    EveAccountSerializeableSortableBindingList = new SerializeableSortableBindingList<EveAccount>(accountDataXMLWithPath, 30000, true, 60000);
                    _xmlFilesLoaded = true;
                }
                catch (Exception e)
                {
                    tryBackupAccountDataSettingsFiles(e, accountDataXMLWithPath, accountDataXMLWithPathDefunct, accountDataXMLWithPathBackup);
                }

                try
                {
                    EveSettingsSerializeableSortableBindingList = new SerializeableSortableBindingList<EveSetting>(eveSettingsXMLWithPath, 30000, true, 60000);
                    _xmlFilesLoaded = true;
                }
                catch (Exception e)
                {
                    tryBackupEveSettingsFiles(e, eveSettingsXMLWithPath, eveSettingsXMLWithPathDefunct, eveSettingsXMLWithPathBackup);
                    //Console.WriteLine(e);
                    //MessageBox.Show(e.ToString(), "Error!");
                    //throw new Exception("Couldn't load the XML files!");
                }

                CreateBackupXMLFiles();
            }
        }

        private string _getLogFileName;
        private string GetLogFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_getLogFileName))
                {
                    var dir = Path.Combine(Cache.Instance.AssemblyPath, "Logs");
                    var fileName = Path.Combine(dir, string.Format("{0:MM-dd-yyyy}", DateTime.Today) + "-EVESharpLauncher.log");
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    _getLogFileName = fileName;
                }
                return _getLogFileName;
            }
        }

        private static AsyncLogQueue _asyncLogQueue;
        private static readonly object _asyncLogQueueLock = new object();
        public static AsyncLogQueue AsyncLogQueue
        {
            get
            {
                try
                {
                    lock (_asyncLogQueueLock)
                    {
                        if (_asyncLogQueue == null)
                            _asyncLogQueue = new AsyncLogQueue();
                        return _asyncLogQueue;
                    }
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception: " + ex);
                    return null;
                }
            }
        }

        public void Log(string text, Color? col = null, [CallerMemberName] string memberName = "")
        {
            if (!IsServer)
                return;

            try
            {
                //OnMessage?.Invoke("[" + String.Format("{0:dd-MMM-yy HH:mm:ss:fff}", DateTime.UtcNow) + "] [" + memberName + "] " + text.ToString(), col);
                AsyncLogQueue.File = GetLogFileName;
                AsyncLogQueue.Enqueue(new LogEntry(text, memberName, col));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private CancellationTokenSource ts = null;
        public void InitTokenSource()
        {
            try
            {
                if (ts == null)
                {
                    ts = new CancellationTokenSource();
                    Task.Run(() =>
                    {
                        while (!ts.Token.IsCancellationRequested && !IsShuttingDown)
                        {
                            int d = 300000;
                            ts.Token.WaitHandle.WaitOne(d);
                            try
                            {
                                if (!_isInitialized)
                                    continue;

                                lock (initLock)
                                {
                                    foreach (EveAccount e in EveAccountSerializeableSortableBindingList.List)
                                    {
                                        DateTime now = DateTime.UtcNow;
                                        DateTime dt = e.StartingTokenTime;
                                        if (dt.Year != now.Year || dt.Month != now.Month || dt.Day != now.Day)
                                        {
                                            e.StartingTokenTime = now;
                                            e.StartingTokenTimespan = TimeSpan.Zero;
                                        }
                                        if (e.EveProcessExists)
                                            e.StartingTokenTimespan = e.StartingTokenTimespan.Add(TimeSpan.FromMilliseconds(d));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }, ts.Token);
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
                return;
            }
        }


        public void ClearCache()
        {
            Cache.Instance._eveSharpCoreCompileTime = null;
            Cache.Instance._eveSharpLauncherCompileTime = null;
            Cache.Instance._hookManagerCompileTime = null;
            Cache.Instance._sharedComponentsCompileTime = null;
        }

        private DateTime? _eveSharpCoreCompileTime = null;

        public DateTime? EveSharpCoreCompileTime
        {
            get
            {
                try
                {
                    if (_eveSharpCoreCompileTime == null)
                    {
                        _eveSharpCoreCompileTime = File.GetLastWriteTime(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EVESharpCore.exe"));
                        if (_eveSharpCoreCompileTime != null)
                            return (DateTime)_eveSharpCoreCompileTime;

                        return DateTime.MinValue;
                    }

                    return (DateTime)_eveSharpCoreCompileTime;
                }
                catch (Exception)
                {
                    return DateTime.MinValue;
                }
            }
        }

        private DateTime? _eveSharpLauncherCompileTime;

        public DateTime? EveSharpLauncherCompileTime
        {
            get
            {
                try
                {
                    if (_eveSharpLauncherCompileTime == null)
                    {
                        _eveSharpLauncherCompileTime = File.GetLastWriteTime(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "EVESharpLauncher.exe"));
                        if (_eveSharpLauncherCompileTime != null)
                            return (DateTime)_eveSharpLauncherCompileTime;

                        return DateTime.MinValue;
                    }

                    return (DateTime)_eveSharpLauncherCompileTime;
                }
                catch (Exception)
                {
                    return DateTime.MinValue;
                }
            }
        }

        private DateTime? _hookManagerCompileTime;

        public DateTime? HookManagerCompileTime
        {
            get
            {
                try
                {
                    if (_hookManagerCompileTime == null)
                    {
                        _hookManagerCompileTime = File.GetLastWriteTime(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "HookManager.exe"));
                        if (_hookManagerCompileTime != null)
                            return (DateTime)_hookManagerCompileTime;

                        return DateTime.MinValue;
                    }

                    return (DateTime)_hookManagerCompileTime;
                }
                catch (Exception)
                {
                    return DateTime.MinValue;
                }
            }
        }

        private DateTime? _sharedComponentsCompileTime;

        public DateTime? SharedComponentsCompileTime
        {
            get
            {
                try
                {
                    if (_sharedComponentsCompileTime == null)
                    {
                        _sharedComponentsCompileTime = File.GetLastWriteTime(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SharedComponents.dll"));
                        if (_sharedComponentsCompileTime != null)
                            return (DateTime)_sharedComponentsCompileTime;

                        return DateTime.MinValue;
                    }

                    return (DateTime)_sharedComponentsCompileTime;
                }
                catch (Exception)
                {
                    return DateTime.MinValue;
                }
            }
        }

        public bool AnyAccountsLinked(bool verbose = false)
        {
            try
            {
                bool linksFound = false;
                IEnumerable<EveAccount> list = Instance.EveAccountSerializeableSortableBindingList.List.Where(a => a != null && a.HWSettings != null);
                List<EveAccount> links = new List<EveAccount>();
                foreach (EveAccount eA in list)
                {
                    if (links.Contains(eA))
                        continue;

                    IEnumerable<EveAccount> t = list.Where(a => a != eA && a.HWSettings.CheckEquality(eA.HWSettings));
                    if (t.Any())
                        foreach (EveAccount r in t)
                        {
                            linksFound = true;
                            if (verbose)
                                Instance.Log($"{eA.MaskedCharacterName} is linked with {r.MaskedCharacterName}");
                            links.Add(r);
                            links.Add(eA);
                        }
                }

                if (verbose && !linksFound)
                    Instance.Log("No linked accounts were found.");

                return linksFound;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
                return false;
            }
        }

        public void CreateBackupXMLFiles()
        {
            // save the backup  here "AccountData.xml.bak" , "EveSettings.xml.bak"
            EveAccountSerializeableSortableBindingList.List.XmlSerialize(EveAccountSerializeableSortableBindingList.FilePathName + ".bak");
            EveSettingsSerializeableSortableBindingList.List.XmlSerialize(EveSettingsSerializeableSortableBindingList.FilePathName + ".bak");
        }





        public void tryBackupEveSettingsFiles(Exception e,
            string eveSettingsXMLWithPath,
            string eveSettingsXMLWithPathDefunct,
            string eveSettingsXMLWithPathBackup)
        {
            // try to load the previous saved backup here
            // if exists, delete "AcccountData.xml.defunct", "EveSettings.xml.defunct"
            // move "AcccountData.xml", "EveSettings.xml" to "AcccountData.xml.defunct", "EveSettings.xml.defunct"
            // copy ..  "AcccountData.xml.bak" ->  "AcccountData.xml"
            // copy ..  "EveSettings.xml.bak" ->  "EveSettings.xml"

            Console.WriteLine(e);
            MessageBox.Show(e.ToString(), "Error!");
            Log(e.ToString());
            if (File.Exists(eveSettingsXMLWithPathBackup))
                try
                {
                    if (File.Exists(eveSettingsXMLWithPathDefunct))
                        File.Delete(eveSettingsXMLWithPathDefunct);

                    File.Move(eveSettingsXMLWithPath, eveSettingsXMLWithPathDefunct);

                    File.Copy(eveSettingsXMLWithPathBackup, eveSettingsXMLWithPath);

                    EveSettingsSerializeableSortableBindingList = new SerializeableSortableBindingList<EveSetting>(eveSettingsXMLWithPath, 30000, true, 60000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw new Exception("Couldn't load the xml files. Even the backup xml files are faulty.");
                }
            else
                throw new Exception("Couldn't load the xml files. There aren't any backup xml files.");
        }

        public void tryBackupAccountDataSettingsFiles(Exception e,
            string accountDataXMLWithPath,
            string accountDataXMLWithPathDefunct,
            string accountDataXMLWithPathBackup)
        {
            // try to load the previous saved backup here
            // if exists, delete "AcccountData.xml.defunct", "EveSettings.xml.defunct"
            // move "AcccountData.xml", "EveSettings.xml" to "AcccountData.xml.defunct", "EveSettings.xml.defunct"
            // copy ..  "AcccountData.xml.bak" ->  "AcccountData.xml"
            // copy ..  "EveSettings.xml.bak" ->  "EveSettings.xml"

            Console.WriteLine(e);
            MessageBox.Show(e.ToString(), "Error!");
            Log(e.ToString());
            if (File.Exists(accountDataXMLWithPathBackup))
                try
                {
                    if (File.Exists(accountDataXMLWithPathDefunct))
                        File.Delete(accountDataXMLWithPathDefunct);

                    File.Move(accountDataXMLWithPath, accountDataXMLWithPathDefunct);

                    File.Copy(accountDataXMLWithPathBackup, accountDataXMLWithPath);

                    EveAccountSerializeableSortableBindingList = new SerializeableSortableBindingList<EveAccount>(accountDataXMLWithPath, 30000, true, 60000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw new Exception("Couldn't load the xml files. Even the backup xml files are faulty.");
                }
            else
                throw new Exception("Couldn't load the xml files. There aren't any backup xml files.");
        }

        ~Cache()
        {
            Interlocked.Decrement(ref CacheInstances);
        }
    }
}