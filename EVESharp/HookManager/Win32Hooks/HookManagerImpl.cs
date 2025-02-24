using EasyHook;
using SharedComponents.EVE;
using SharedComponents.IPC;
using SharedComponents.Utility;
using SharedComponents.Utility.AsyncLogQueue;
using SharedComponents.WinApiUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedComponents.Extensions;
using SharedComponents.Py;
using System.Runtime;
using HookManager.PythonHooks;
using SharedComponents.SharedMemory;

namespace HookManager.Win32Hooks
{
    public class HookManagerImpl
    {
        #region Fields

        public static List<string> DirectoryBlackList = new List<string>(new string[] { });
        public static List<string> DirectoryWhiteListRead = new List<string>(new string[] { });
        public static List<string> DirectoryWhiteListWrite = new List<string>(new string[] { });

        public static List<string> ExeNamesToHide =
            new List<string>(new string[] { });

        public static List<string> FileNamesToHide;

        public static List<string> FileWhiteList =
                   new List<string>(new string[] { "comctl32", "kernel32", "dbghelp", "wtsapi", "ntdll", "psapi", "blue", "python27", "libovrrt64_1", "openvr_api", "libovrrt32_1" });

        public AppDomain QAppDomain = null;
        public Thread t = null;
        private static readonly Lazy<HookManagerImpl> _instance = new Lazy<HookManagerImpl>(() => new HookManagerImpl());
        private string _getLogFileName;
        private string _assemblyPath;

        private List<IHook> _controllerList;
        private EveAccount _EveAccount = null;
        private IntPtr? EveHwndValue = null;
        private DateTime LastEveAccountPoll = DateTime.MinValue;
        private IntPtr? _eveSharpCoreHWnd = null;

        public bool IsInjectedInEVE { get; set; }

        #endregion Fields

        #region Constructors

        public HookManagerImpl()
        {
            _controllerList = new List<IHook>();

            try
            {
                var files = Directory.GetFiles(AssemblyPath, "*.exe", SearchOption.TopDirectoryOnly);
                foreach (var f in files)
                {
                    var k = Path.GetFileNameWithoutExtension(f.ToLower());
                    ExeNamesToHide.Add(k);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            PyHook = new PyHook();
            AsyncLogQueue = new AsyncLogQueue();
            LogEntry le = new LogEntry("HM instance created.", "", null);
            AsyncLogQueue.Enqueue(le);
        }

        #endregion Constructors

        #region Properties

        public PyHook PyHook;

        public readonly AsyncLogQueue AsyncLogQueue;

        public static HookManagerImpl Instance => _instance.Value;

        public string AssemblyPath
        {
            get
            {
                if (_assemblyPath == null)
                    _assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return _assemblyPath;
            }
        }

        public string CharName { get; set; }
        public string CurrentProcName => _currentProcName ?? (_currentProcName = Process.GetCurrentProcess().ProcessName.ToLower());

        public EveAccount EveAccount
        {
            get
            {
                if (_EveAccount == null || LastEveAccountPoll.AddMilliseconds(1000) < DateTime.UtcNow)
                {
                    LastEveAccountPoll = DateTime.UtcNow;
                    try
                    {

                        if (string.IsNullOrEmpty(WCFClient.Instance.GUID))
                        {
                            Cache.Instance.Log("EveAccount: WCFClient.Instance.GUID IsNullOrEmpty?!");
                        }

                        var ret = WCFClient.Instance.GetPipeProxy.GetEveAccount(WCFClient.Instance.GUID);
                        if (ret != null)
                        {
                            _EveAccount = ret;
                            return _EveAccount;
                        }

                        Cache.Instance.Log("HookManagerImpl: EveAccount: GetEveAccount returned null");
                        Cache.Instance.Log("GUID: [" + WCFClient.Instance.GUID + "] Not Found?");
                        return null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Debug.WriteLine(e);
                    }
                }

                return _EveAccount;
            }
        }

        public IntPtr EveHWnd
        {
            get
            {
                if (EveHwndValue == null || !WinApiUtil.IsValidHWnd((IntPtr)EveHwndValue))
                    try
                    {
                        EveHwndValue = (IntPtr)Instance.EveAccount.EveHWnd;
                    }
                    catch (Exception)
                    {
                        if (EveHwndValue != null)
                            return EveHwndValue.Value;
                        return IntPtr.Zero;
                    }

                return (IntPtr)EveHwndValue;
            }
        }

        public IntPtr EVESharpCoreFormHWnd
        {
            get
            {
                if (_eveSharpCoreHWnd == null || !WinApiUtil.IsValidHWnd((IntPtr)_eveSharpCoreHWnd))
                    try
                    {
                        _eveSharpCoreHWnd = (IntPtr)Instance.EveAccount.EVESharpCoreFormHWnd;
                    }
                    catch (Exception)
                    {
                        if (_eveSharpCoreHWnd != null)
                            return (IntPtr)_eveSharpCoreHWnd;
                        return IntPtr.Zero;
                    }

                return (IntPtr)_eveSharpCoreHWnd;
            }
        }

        public bool EveWndShown { get; set; }
        public bool IsInjectedIntoEve => CurrentProcName == "exefile";
        public string PipeName { get; set; }

        private string GetLogFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_getLogFileName) || string.IsNullOrEmpty(Instance.CharName))
                {
                    var logPath = Instance.AssemblyPath + Path.DirectorySeparatorChar + "Logs" + Path.DirectorySeparatorChar + Instance.CharName;
                    if (!Directory.Exists(logPath))
                        Directory.CreateDirectory(logPath);
                    var fileName = logPath + Path.DirectorySeparatorChar + "HookManager.log";
                    _getLogFileName = fileName;
                }
                return _getLogFileName;
            }
        }

        private string _currentProcName { get; set; }

        #endregion Properties

        #region Methods

        public static bool IsBacklistedDirectory(string path)
        {
            if (path == string.Empty || path == null || path == "") return false;
            foreach (var dir in DirectoryBlackList)
                if (path.Contains(dir))
                    return true;
            return false;
        }

        public static bool IsWhiteListedFileName(string lpModName)
        {
            var fileExtPos = lpModName.LastIndexOf(".");
            var lpModNameWithoutExtension = fileExtPos >= 0 ? lpModName.Substring(0, fileExtPos) : lpModName;
            return FileWhiteList.Contains(lpModNameWithoutExtension.ToLower()) ? true : false;
        }

        public static bool IsWhiteListedReadDirectory(string path)
        {
            if (path == string.Empty || path == null || path == "") return false;
            foreach (var dir in DirectoryWhiteListRead)
                if (path.Contains(dir))
                    return true;
            return false;
        }

        public static bool IsWhiteListedWriteDirectory(string path)
        {
            if (path == string.Empty || path == null || path == "") return false;
            foreach (var dir in DirectoryWhiteListWrite)
                if (path.Contains(dir))
                    return true;
            return false;
        }

        public static void Log(string text, Color? col = null, [CallerMemberName] string memberName = "")
        {
            try
            {
                Instance.AsyncLogQueue.File = Instance.GetLogFileName;
                Instance.AsyncLogQueue.Enqueue(new LogEntry(text, memberName, col));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static bool NeedsToBeCloaked(string lpModName)
        {
            //HookManager.Log.RemoteWriteLine($"lpModName {lpModName}");
            var found = FileNamesToHide.Where(e => lpModName.EndsWith(e)).ToList();
            return found.Any() ? true : false;
        }

        public void AddController(IHook controller)
        {
            if (!_controllerList.Contains(controller))
            {
                WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + EveAccount.MaskedCharacterName + "] AddController  [" + controller.Name + "] Error (if any) [" + controller.Error + "]");
                Debug.WriteLine($"Adding controller {controller} Error {controller.Error}");
                _controllerList.Add(controller);
            }
        }

        public void CloseQuestorWindow()
        {
            if (!WinApiUtil.IsValidHWnd(Instance.EVESharpCoreFormHWnd))
                return;

            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (WinApiUtil.IsValidHWnd(Instance.EVESharpCoreFormHWnd) && timeout > DateTime.UtcNow)
            {
                WinApiUtil.CloseWindow(Instance.EVESharpCoreFormHWnd);
                Thread.Sleep(5);
            }
        }

        public bool HooksInitialized()
        {
            foreach (var controller in _controllerList)
                if (controller != null)
                    if (controller.Error)
                    {
                        var msg = $"Hook error! Controllername [{controller.Name}]";
                        WCFClient.Instance.GetPipeProxy.RemoteLog(msg);
                        Log(msg);
                        return false;
                    }
            return true;
        }

        public String GetControllersWithHookErrors()
        {
            var ret = String.Empty;

            foreach (var controller in _controllerList)
                if (controller != null && controller.Error)
                    ret += " " + controller.Name;
            return ret;
        }

        private SharedArray<IntPtr> _recvFuncPtr;
        private SharedArray<IntPtr> _sendFuncPtr;

        private Stopwatch watch = null;
        private long elapsedMs = 0;

        public void InitEVEHooks()
        {
            try
            {
                watch = Stopwatch.StartNew();
                EnvVars.SetEnvironment(EveAccount.HWSettings);
                Util.CheckCreateDirectorys(EveAccount.HWSettings.WindowsUserLogin);

                StopTheWatchLogResultsAndRestartTimer();

                var questorLauncherDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                FileNamesToHide = Directory.GetFiles(questorLauncherDir, "*.*").ToList();

                Debug.WriteLine("Inithooks stage #1");

                DirectoryWhiteListRead.Add(Environment.GetFolderPath(Environment.SpecialFolder.Windows)); // win folder
                DirectoryWhiteListRead.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)); // folder of current assembly
                DirectoryWhiteListWrite.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)); // folder of current assembly
                DirectoryWhiteListRead.Add(EveAccount.GetAppDataFolder());
                DirectoryWhiteListWrite.Add(EveAccount.GetAppDataFolder());
                DirectoryWhiteListRead.Add(EveAccount.GetPersonalFolder());
                DirectoryWhiteListWrite.Add(EveAccount.GetPersonalFolder());
                DirectoryWhiteListRead.Add(EveAccount.GetEveRootPath()); // eve folder
                //DirectoryWhiteListRead.Add(EveAccount.GetEveRootPath(true));

                StopTheWatchLogResultsAndRestartTimer();
                Debug.WriteLine("Inithooks stage #2");

                Util.LoadLibrary("blue.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("python27.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("WS2_32.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("Kernel32.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("advapi32.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("Iphlpapi.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("DbgHelp.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("_ctypes.pyd", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("wininet.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("psapi.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("fastprox.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("user32.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");

                Util.LoadLibrary("d3d11.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("dxgi.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("gdi32.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                Util.LoadLibrary("wsock32.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");

                Util.LoadLibrary("MemMan.dll", "HookManager [" + EveAccount.MaskedCharacterName + "]");
                StopTheWatchLogResultsAndRestartTimer();
                /**
                try
                {
                    WCFClient.Instance.GetPipeProxy.RemoteLog($"-------- RecvFuncPtr: var recvHandle = NativeAPI.GetProcAddress(NativeAPI.GetModuleHandle(\"MemMan.dll\"), \"RecvPacket\");");
                    var recvHandle = NativeAPI.GetProcAddress(NativeAPI.GetModuleHandle("MemMan.dll"), "RecvPacket");
                    _recvFuncPtr = new SharedArray<IntPtr>(Process.GetCurrentProcess().Id + nameof(UsedSharedMemoryNames.RecvFuncPointer), 1);
                    _recvFuncPtr[0] = recvHandle;

                    Debug.WriteLine($"-------- RecvFuncPtr: {recvHandle}");
                    WCFClient.Instance.GetPipeProxy.RemoteLog($"-------- RecvFuncPtr: {recvHandle}");
                    if (recvHandle == IntPtr.Zero || recvHandle.ToString() == 0.ToString())
                    {
                        WCFClient.Instance.GetPipeProxy.RemoteLog($"-------- _recvFuncPtr: We are probably missing MenMan.dll in the evesharp launcher directory: _recvFuncPtr cannot be 0");
                        Environment.Exit(0);
                        Environment.FailFast("exit");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    WCFClient.Instance.GetPipeProxy.RemoteLog("Exception [" + ex + "]");
                }

                try
                {
                    WCFClient.Instance.GetPipeProxy.RemoteLog($"-------- SendFuncPtr: var sendHandle = NativeAPI.GetProcAddress(NativeAPI.GetModuleHandle(\"MemMan.dll\"), \"SendPacket\");");
                    var sendHandle = NativeAPI.GetProcAddress(NativeAPI.GetModuleHandle("MemMan.dll"), "SendPacket");
                    _sendFuncPtr = new SharedArray<IntPtr>(Process.GetCurrentProcess().Id + nameof(UsedSharedMemoryNames.SendFuncPointer), 1);
                    _sendFuncPtr[0] = sendHandle;

                    Debug.WriteLine($"-------- SendFuncPtr: {sendHandle}");
                    WCFClient.Instance.GetPipeProxy.RemoteLog($"-------- SendFuncPtr: {sendHandle}");
                    if (sendHandle == IntPtr.Zero || sendHandle.ToString() == 0.ToString())
                    {
                        WCFClient.Instance.GetPipeProxy.RemoteLog($"-------- _sendFuncPtr: We are probably missing MenMan.dll in the evesharp launcher directory: _sendFuncPtr cannot be 0");
                        Environment.Exit(0);
                        Environment.FailFast("exit");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    WCFClient.Instance.GetPipeProxy.RemoteLog("Exception [" + ex + "]");
                }
                **/

                //AddController(new RestartEveSharpCoreHookController());
                Debug.WriteLine("Inithooks stage #3");
                Util.CheckCreateDirectorys(EveAccount.HWSettings.WindowsUserLogin);

                Debug.WriteLine("Inithooks stage #4");

                AddController(new SHGetFolderPathAController(EveAccount.GetPersonalFolder(), EveAccount.GetAppDataFolder()));
                AddController(new SHGetFolderPathWController(EveAccount.GetPersonalFolder(), EveAccount.GetAppDataFolder()));

                try
                {
                    EnvVars.SetEnvironment(EveAccount.HWSettings);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    throw;
                }

                Debug.WriteLine("Inithooks stage #5");

                //proxy
                var p = EveAccount.HWSettings.Proxy;

                if (p == null)
                {
                    WCFClient.Instance.GetPipeProxy.RemoteLog("Error: Proxy == null.");
                    Environment.Exit(0);
                    Environment.FailFast("exit");
                    return;
                }

                if (!EveAccount.UseLocalInternetConnection)
                {
                    AddController(new WinSockConnectController(LocalHook.GetProcAddress("WS2_32.dll", "connect"), p.Ip, p.Socks5Port, p.Username, p.Password));
                }
                // ConnectEx hook, disabled until we want to enable GRPC
                AddController(new WSAIoctlController(p.Ip, p.Socks5Port, p.Username, p.Password));

                if (!Guid.TryParse(EveAccount.HWSettings.MachineGuid, out _) || string.IsNullOrEmpty(EveAccount.HWSettings.MachineGuid))
                {
                    MessageBox.Show("Hook error (0): EveAccount.HWSettings.MachineGuid [" + EveAccount.HWSettings.MachineGuid + "]");
                    Environment.Exit(0);
                    Environment.FailFast("exit");
                    return;
                }

                AddController(new RegQueryValueExAController(LocalHook.GetProcAddress("advapi32.dll", "RegQueryValueExA"), EveAccount.HWSettings));
                AddController(new RegQueryValueExWController(LocalHook.GetProcAddress("advapi32.dll", "RegQueryValueExW"), EveAccount.HWSettings));
                //AddController(new WriteConsoleController());
                //AddController(new WriteFileController());
                AddController(new PyFileWriteObjectController());

                if (EveAccount.HWSettings.SystemReservedMemory == 0)
                {
                    MessageBox.Show("Hook error (1)");
                    Environment.Exit(0);
                    Environment.FailFast("exit");
                    return;
                }

                var globalMemoryStatusExMem = EveAccount.HWSettings.TotalPhysRam - EveAccount.HWSettings.SystemReservedMemory;
                var getPhysicallyInstalledSystemMem = EveAccount.HWSettings.TotalPhysRam;

                AddController(new GetPhysInstalledMemController(getPhysicallyInstalledSystemMem));
                AddController(new GlobalMemoryStatusController(LocalHook.GetProcAddress("kernel32.dll", "GlobalMemoryStatusEx"), globalMemoryStatusExMem));

                AddController(new GetAdaptersInfoController(LocalHook.GetProcAddress("Iphlpapi.dll", "GetAdaptersInfo"), EveAccount.HWSettings.NetworkAdapterGuid,
                EveAccount.HWSettings.MacAddress, EveAccount.HWSettings.NetworkAddress, EveAccount.HWSettings.NetworkAdapterName));


                AddController(new GetAdaptersAddressesController(LocalHook.GetProcAddress("Iphlpapi.dll", "GetAdaptersAddresses"), EveAccount.HWSettings.NetworkAdapterGuid,
                EveAccount.HWSettings.MacAddress, EveAccount.HWSettings.NetworkAddress, EveAccount.HWSettings.NetworkAdapterName));


                AddController(new EnumDisplayDevicesAController(EveAccount.HWSettings));
                AddController(new EnumDisplayDevicesWController(EveAccount.HWSettings));
                AddController(new DX11Controller(EveAccount.HWSettings));

                //AddController(new InternetConnectAController());
                //AddController(new InternetConnectWController());

                AddController(new IsDebuggerPresentController());
                AddController(new LoadLibraryAController());
                AddController(new LoadLibraryWController());
                AddController(new GetModuleHandleWController());
                AddController(new GetModuleHandleAController());
                AddController(new EnumProcessesControllerPSAPI());
                AddController(new EnumProcessesControllerK32());
                AddController(new MiniWriteDumpController());

                AddController(new CreateFileWController());
                AddController(new CreateFileAController());
                AddController(new CryptHashDataController());
                //AddController(new CryptCreateHashController());

                AddController(new CryptEncryptController());
                AddController(new CryptDecryptController());

                if (Util.GetRealWindowsVersion().Build < 22000)
                {
                    // Fails on Windows 11, FastProx Get has new signature
                    AddController(new IWbemClassObjectGetController());
                }

                AddController(new WSAConnectController());
                AddController(new WSAConnectByListController());
                AddController(new WSAConnectByNameController());
                //AddController(new WSASendToRecvFromController());
                //AddController(new WinsockSendToRecvFromController());
                //AddController(new DX11Present());
                AddController(new DX11CreateDeviceController());
                AddController(new PyInitModule4Controller());
                AddController(new PyEvalGetRestrictedController());
                AddController(new CreateProcessAController());
                AddController(new CreateProcessWController());
                //AddController(new PostSendMessageController());

                //FixME! - When this is enabled detecting the foreground window is not reliable for some reason
                AddController(new GetForegroundWindowController());

                AddController(new GetFocusWindowController());
                AddController(new CreateWindowExWController());
                AddController(new RegisterClassExWController());
                AddController(new DeviceIoControlController());
                AddController(new GetSystemInfoController(EveAccount.HWSettings)); // this is useless atm, we want to add cpuid mid function hooks at some point

                AddController(new GetComputerNameWController(EveAccount.HWSettings.Computername));
                AddController(new GetComputerNameAController(EveAccount.HWSettings.Computername));

                //AddController(new PySetAttributeController());

                AddController(new GetUserNameController(EveAccount.HWSettings.WindowsUserLogin));
                AddController(new RegGetValueWController());

                AddController(new EnumDisplaySettingController(EveAccount.HWSettings)); // is not used to get the resolution / refresh rate, but we keep it for now


                AddController(new GetMonitorInfoController(EveAccount.HWSettings));


                AddController(new GetDeviceCapsController(EveAccount.HWSettings));

                AddController(new DisplayConfigGetDeviceInfoController(EveAccount.HWSettings));


                AddController(new QueryDisplayConfigController(EveAccount.HWSettings)); // is not used to get the displays refresh rate


                AddController(new DXGIGetDisplayModeListController(EveAccount.HWSettings));

                AddController(new GetAddrInfoController());

                if (!HooksInitialized())
                {
                    MessageBox.Show("Hook error (2): if (!HooksInitialized())");
                    Environment.Exit(0);
                    Environment.FailFast("exit");
                }

                var hooksInit = string.Format("HookManager [" + EveAccount.MaskedCharacterName + "] Hooks initialized. [{0}]", EveAccount.MaskedAccountName);
                Log(hooksInit);
                WCFClient.Instance.GetPipeProxy.RemoteLog(hooksInit);
            }
            catch (Exception ex)
            {
                WCFClient.Instance.GetPipeProxy.RemoteLog("Exception [" + ex + "]");
            }
        }

        private void StopTheWatchLogResultsAndRestartTimer()
        {
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;
            //WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + EveAccount.MaskedCharacterName + "][" + module + "] took [" + Math.Round((double)elapsedMs, 0) + "] millseconds");

            watch = Stopwatch.StartNew();
        }

        public void InitWinSockControllerHookOnly(Proxy p)
        {
            Util.LoadLibrary("WS2_32.dll", "HookManager[" + EveAccount.MaskedCharacterName + "]");
            WCFClient.Instance.GetPipeProxy.RemoteLog($"Loaded WS2_32.dll.");
            AddController(new WinSockConnectController(LocalHook.GetProcAddress("WS2_32.dll", "connect"), p.Ip, p.Socks5Port, p.Username, p.Password));
            WCFClient.Instance.GetPipeProxy.RemoteLog($"Added WinSockConnectController.");

            if (!HooksInitialized())
            {
                MessageBox.Show("Hook error (3)");
                Environment.Exit(0);
                Environment.FailFast("exit");
                return;
            }

            const string hooksInit = "Hooks initialized.";
            WCFClient.Instance.GetPipeProxy.RemoteLog(hooksInit);
        }

        public void LaunchAppDomain()
        {
            try
            {
                t = new Thread(delegate ()
                {
                    try
                    {
                        try
                        {
                            CopyEveSharpCoreForEachUserAtStart();
                        }
                        catch (Exception)
                        {
                            //ignore this exception
                        }

                        QAppDomain = AppDomain.CreateDomain("QAppDomain");
                        string assemblyFolder = Instance.AssemblyPath;
                        string exeToRun = assemblyFolder + "\\CoreDomainMiddleware.exe";
                        string[] args = new String[] { Instance.CharName, Instance.PipeName, WCFClient.Instance.GUID }; //, EveAccount.ConnectToTestServer.ToString() };

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        AppDomain.MonitoringIsEnabled = true;

                        WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + EveAccount.MaskedCharacterName + "] -------------------------------------------------------------------------");
                        if (File.Exists(exeToRun))
                        {
                            WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + EveAccount.MaskedCharacterName + "] Launching [" + exeToRun + "] with args [" + args[0] + "][" + args[1] + "] GUID [" + args[2] + "]");
                            QAppDomain.ExecuteAssembly(exeToRun, args);
                        }
                        else
                        {
                            WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + EveAccount.MaskedCharacterName + "] File not found: [" + exeToRun + "]");
                        }

                        WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + EveAccount.MaskedCharacterName + "] -------------------------------------------------------------------------");
                    }
                    catch (Exception ex)
                    {
                        Log("Exception: " + ex);
                    }
                });

                t.Start();
            }
            catch (Exception)
            {
                //ignore this exception
            }
        }

        private void CopyEveSharpCoreForEachUserAtStart()
        {
            try
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string evesharpcoreexe = Path.Combine(path, "evesharpcore.exe");
                string evesharpcorepdb = Path.Combine(path, "evesharpcore.pdb");
                string perToonEvesharpcoreexe = Path.Combine(path, "evesharpcore-" + WCFClient.Instance.CharName + "-" + WCFClient.Instance.GUID + ".exe");
                string perToonEvesharpcorepdb = Path.Combine(path, "evesharpcore-" + WCFClient.Instance.CharName + "-" + WCFClient.Instance.GUID + ".pdb");
                //if (EveAccount.ConnectToTestServer)
                //{
                //    //remove spaces with something like: .Where(c => !Char.IsWhiteSpace(c)).ToString(), then handle the args[] to be correct (with spaces, but remove them when processing?)
                //    perToonEvesharpcoreexe = Path.Combine(path, "evesharpcore-" + WCFClient.Instance.CharName + "-" + WCFClient.Instance.GUID + "-SISI.exe");
                //    perToonEvesharpcorepdb = Path.Combine(path, "evesharpcore-" + WCFClient.Instance.CharName + "-" + WCFClient.Instance.GUID + "-SISI.pdb");
                //}

                try
                {
                    WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + WCFClient.Instance.CharName + "] ----------");
                    WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + WCFClient.Instance.CharName + "] Started Copying [" + evesharpcoreexe + "] to [" + perToonEvesharpcoreexe + "]");
                    File.Copy(evesharpcoreexe, perToonEvesharpcoreexe, true);
                    Thread.Sleep(100);
                    while (IsFileLocked(perToonEvesharpcoreexe))
                    {
                        WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + WCFClient.Instance.CharName + "] waiting for [" + perToonEvesharpcoreexe + "] to finish copying");
                        Thread.Sleep(500);
                    }

                    WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + WCFClient.Instance.CharName + "] Done Copying [" + evesharpcoreexe + "] to [" + perToonEvesharpcoreexe + "]");
                    WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + WCFClient.Instance.CharName + "] ----------");
                    WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + WCFClient.Instance.CharName + "] Started Copying [" + evesharpcorepdb + "] to [" + perToonEvesharpcorepdb + "]");
                    File.Copy(evesharpcorepdb, perToonEvesharpcorepdb, true);
                    Thread.Sleep(100);
                    while (IsFileLocked(perToonEvesharpcorepdb))
                    {
                        WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + WCFClient.Instance.CharName + "] waiting for [" + perToonEvesharpcorepdb + "] to finish copying");
                        Thread.Sleep(500);
                    }

                    WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + WCFClient.Instance.CharName + "] Done Copying [" + evesharpcorepdb + "] to [" + perToonEvesharpcorepdb + "]");
                    WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + WCFClient.Instance.CharName + "] ----------");
                }
                catch (Exception ex)
                {
                    WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + WCFClient.Instance.CharName + "] Exception [" + ex + "]");
                }
            }
            catch (Exception ex)
            {
                WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager: Exception [" + ex + "]");
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

        public void RemoveController(IHook controller)
        {
            _controllerList.Remove(controller);
        }

        public void SendToInjectorLog(string text)
        {
            WCFClient.Instance.GetPipeProxy.RemoteLog($"{text} [{CharName}].");
        }

        public void SetupWindowHooks(int eveWindowThreadId, IntPtr eveHwnd, IntPtr hookmanagerHwnd)
        {
            Hooks.CreateHook(HookType.WH_CBT, eveWindowThreadId).OnHookProcEvent += (code, param, lParam) =>
            {
                var hbct = (HCBT)code;

                switch (hbct)
                {
                    case HCBT.MinMax:
                        var sw = (int)lParam;

                        switch (sw)
                        {
                            case Pinvokes.SW_MINIMIZE:
                            case Pinvokes.SW_FORCEMINIMIZE:

                                break;

                            case Pinvokes.SW_MAXIMIZE:

                                break;

                            default:
                                break;
                        }

                        break;

                    case HCBT.MoveSize:
                        var rect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));

                        //Win32Hooks.HookManager.Log("HCBT.MoveSize");

                        //try
                        //{
                        //    var x = new byte[10];
                        //    Program.IPCMaster.RemoteRequest(x, 50);
                        //}
                        //catch (Exception ex)
                        //{
                        //    Console.WriteLine(ex);
                        //}

                        try
                        {
                            WinApiUtil.DockToParentWindow(rect, eveHwnd, EVESharpCoreFormHWnd, Alignment.TOP);
                            WinApiUtil.DockToParentWindow(rect, eveHwnd, hookmanagerHwnd, Alignment.RIGHTBOT);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }

                        break;

                    default:
                        break;
                }
            };

            DateTime lastMoveUpdate = DateTime.MinValue;
            Hooks.CreateHook(HookType.WH_CALLWNDPROCRET, eveWindowThreadId).OnHookProcEvent += (code, param, lParam) =>
            {
                var cwpretstruct = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
                var msg = (WM)cwpretstruct.message;
                switch (msg)
                {
                    case WM.MOVING:
                    case WM.MOVE:
                        try
                        {
                            if (lastMoveUpdate.AddMilliseconds(30) < DateTime.UtcNow)
                            {
                                lastMoveUpdate = DateTime.UtcNow;
                                var rect = WinApiUtil.GetWindowRect(eveHwnd);
                                WinApiUtil.DockToParentWindow(rect, eveHwnd, EVESharpCoreFormHWnd, Alignment.TOP);
                                WinApiUtil.DockToParentWindow(rect, eveHwnd, hookmanagerHwnd, Alignment.RIGHTBOT);
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                        break;

                    case WM.SYSCOMMAND:
                        var sc = (int)cwpretstruct.wParam;
                        var sysCmd = (SysCommands)sc;
                        switch (sysCmd)
                        {
                            case SysCommands.SC_ICON:
                                //Win32Hooks.HookManager.Log("SC_ICON");
                                try
                                {
                                    WinApiUtil.ShowWindow(eveHwnd);
                                    WinApiUtil.SetForegroundWindow(eveHwnd);
                                    var rectMin = WinApiUtil.GetWindowRect(eveHwnd);
                                    WinApiUtil.DockToParentWindow(rectMin, eveHwnd, EVESharpCoreFormHWnd, Alignment.TOP);
                                    WinApiUtil.DockToParentWindow(rectMin, eveHwnd, hookmanagerHwnd, Alignment.RIGHTBOT);
                                    WinApiUtil.SetWindowTopMost(EVESharpCoreFormHWnd, true);
                                    WinApiUtil.SetWindowTopMost(hookmanagerHwnd, true);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }

                                break;

                            case SysCommands.SC_MAXIMIZE:
                            case SysCommands.SC_RESTORE:
                                //Win32Hooks.HookManager.Log("SC_MAXIMIZE_SC_RESTORE");

                                //try
                                //{
                                //    var x = new byte[1];
                                //    Program.IPCMaster.RemoteRequest(x, 50);
                                //}
                                //catch (Exception ex)
                                //{
                                //    Console.WriteLine(ex);
                                //}

                                try
                                {
                                    var rectMax = WinApiUtil.GetWindowRect(eveHwnd);
                                    WinApiUtil.DockToParentWindow(rectMax, eveHwnd, EVESharpCoreFormHWnd, Alignment.TOP);
                                    WinApiUtil.DockToParentWindow(rectMax, eveHwnd, hookmanagerHwnd, Alignment.RIGHTBOT);
                                    WinApiUtil.SetWindowTopMost(EVESharpCoreFormHWnd, true);
                                    WinApiUtil.SetWindowTopMost(hookmanagerHwnd, true);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                                break;

                            default:
                                break;
                        }

                        break;

                    case WM.SIZE:
                        //try
                        //{
                        //    var x = new byte[1];
                        //    Program.IPCMaster.RemoteRequest(x, 50);
                        //}
                        //catch (Exception ex)
                        //{
                        //    Console.WriteLine(ex);
                        //}
                        break;

                    case WM.KILLFOCUS:
                        //Win32Hooks.HookManager.Log("WM.KILLFOCUS");
                        try
                        {
                            if (!WinApiUtil.IsValidHWnd(eveHwnd))
                                return;

                            if (WinApiUtil.IsValidHWnd(hookmanagerHwnd))
                            {
                                WinApiUtil.SetWindowTopMost(hookmanagerHwnd, false);
                            }

                            if (WinApiUtil.IsValidHWnd(EVESharpCoreFormHWnd))
                            {
                                WinApiUtil.SetWindowTopMost(EVESharpCoreFormHWnd, false);
                            }

                            var fgw = GetForegroundWindowController.GetCurrentForegroundWnd;
                            //var fgw = WinApiUtil.GetForegroundWindow();
                            WinApiUtil.SetHWndInsertAfter(hookmanagerHwnd, fgw);
                            WinApiUtil.SetHWndInsertAfter(EVESharpCoreFormHWnd, fgw);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                        break;

                    case WM.SETFOCUS:

                        //Win32Hooks.HookManager.Log("WM.SETFOCUS");
                        try
                        {
                            WinApiUtil.SetWindowTopMost(hookmanagerHwnd, true);
                            WinApiUtil.SetWindowTopMost(EVESharpCoreFormHWnd, true);
                            WinApiUtil.SetWindowTopMost(hookmanagerHwnd, false);
                            WinApiUtil.SetWindowTopMost(EVESharpCoreFormHWnd, false);
                            WinApiUtil.SetHWndInsertAfter(hookmanagerHwnd, EVESharpCoreFormHWnd);
                            WinApiUtil.SetHWndInsertAfter(eveHwnd, hookmanagerHwnd);
                            WinApiUtil.SetWindowTopMost(hookmanagerHwnd, true);
                            WinApiUtil.SetWindowTopMost(EVESharpCoreFormHWnd, true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                        break;

                    default:
                        break;
                }
            };
        }

        public void UnloadAppDomain()
        {
            try
            {
                CloseQuestorWindow();
                if (QAppDomain != null)
                {
                    AppDomain.Unload(QAppDomain);
                    QAppDomain = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ForceQuit(string msg, Exception ex = null)
        {
            try
            {
                WCFClient.Instance.GetPipeProxy.RemoteLog(msg);
                if (ex != null)
                    WCFClient.Instance.GetPipeProxy.RemoteLog(ex.ToString());

                WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValue(WCFClient.Instance.GUID, nameof(SharedComponents.EVE.EveAccount.UseScheduler), false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            HookManagerImpl.Log(msg, Color.Red);
            Environment.Exit(0);
            Environment.FailFast("");
        }

        public void WaitForEVE()
        {
            var pid = Process.GetCurrentProcess().Id;
            var eveWndOpen = false;
            var started = DateTime.UtcNow;
            WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + HookManagerImpl.Instance.EveAccount.MaskedCharacterName + "] WaitForEVE: pid [" + pid + "] eveWndOpen [" + eveWndOpen + "] GUID [" + WCFClient.Instance.GUID + "]");
            while (!eveWndOpen)
            {
                var eveHWnd = WinApiUtil.GetEveHWnd(pid);
                //WCFClient.Instance.GetPipeProxy.RemoteLog("WaitForEVE: eveHWnd [" + eveHWnd + "]");
                if (eveHWnd != IntPtr.Zero)
                {
                    eveWndOpen = true;

                    WinApiUtil.RemoveWS_EX_NOACTIVATE(eveHWnd);
                    WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager: GUID [" + WCFClient.Instance.GUID + "]");
                    WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValueBlocking(WCFClient.Instance.GUID, nameof(EveAccount.EveHWnd), (Int64)eveHWnd);
                }

                if (started.AddSeconds(45) < DateTime.UtcNow)
                {
                    var msg = "Couldn't find EVE window after waiting 45 seconds. Quitting.";
                    Log(msg, Color.Purple);
                    WCFClient.Instance.GetPipeProxy.RemoteLog("WaitForEVE: msg [" + msg + "]");
                    Console.WriteLine(msg);
                    Debug.WriteLine(msg);
                    Environment.Exit(0);
                    Environment.FailFast("");
                }

                Thread.Sleep(1000);
            }

            EveWndShown = true;

            if (!RegisterClassExWController.WndProcHookadded) // EveWndShow and WNDProc not added yet? Error
            {
                CryptHashDataController.ForceQuit("ERROR: EVE window shown, but no WNDProc hook was added yet.");
            }
        }

        #endregion Methods
    }
}