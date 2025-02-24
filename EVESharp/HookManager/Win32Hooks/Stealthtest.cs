/*
 * ---------------------------------------
 * User: duketwo
 * Date: 02.06.2014
 * Time: 19:29
 *
 * ---------------------------------------
 */

using EasyHook;
using SharedComponents.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of StealthTest.
    /// </summary>
    public static class StealthTest
    {
        #region Methods

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr GetModuleHandleA(IntPtr lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandleW(IntPtr lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibraryA(IntPtr lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryW(IntPtr lpModuleName);

        public static void Test()
        {
            new Thread(() =>
            {
                HookManagerImpl.Log("[StealthTest] Starting stealth test.");
                HookManagerImpl.Log("[StealthTest] Enumprocs.");

                UInt32 arraySize = 2000;
                var arrayBytesSize = arraySize * sizeof(UInt32);
                var processIds = new UInt32[arraySize];
                var processIdsToReturn = new UInt32[arraySize];
                UInt32 bytesCopied;

                var success = EnumProcesses(processIds, arrayBytesSize, out bytesCopied);

                if (!success) return;
                if (0 == bytesCopied) return;

                var numIdsCopied = Convert.ToInt32(bytesCopied / 4);
                var currentEvePID = Process.GetCurrentProcess().Id;

                var processID = 0;

                var eveProcsA = 0;

                for (var i = 0; i < numIdsCopied; i++)
                {
                    processID = Convert.ToInt32(processIds[i]);
                    try
                    {
                        var p = Process.GetProcessById(processID);
                        if (p != null)
                        {
                            HookManagerImpl.Log("ProcID i: (" + i + ") " + processID + " ExeName: " + p.ProcessName);
                            if (p.ProcessName.Equals("exefile"))
                                eveProcsA++;
                        }
                        else
                            HookManagerImpl.Log("P: " + processIds[i] + " is null.");
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                HookManagerImpl.Log($"Found eve procs: {eveProcsA}");

                success = K32EnumProcesses(processIds, arrayBytesSize, out bytesCopied);

                if (!success) return;
                if (0 == bytesCopied) return;

                numIdsCopied = Convert.ToInt32(bytesCopied / 4);
                currentEvePID = Process.GetCurrentProcess().Id;

                processID = 0;
                var eveProcsB = 0;

                for (var i = 0; i < numIdsCopied; i++)
                {
                    processID = Convert.ToInt32(processIds[i]);
                    try
                    {
                        var p = Process.GetProcessById(processID);
                        if (p != null)
                        {
                            HookManagerImpl.Log("ProcID i: (" + i + ") " + processID + " ExeName: " + p.ProcessName);
                            if (p.ProcessName.Equals("exefile"))
                                eveProcsB++;
                        }
                        else
                            HookManagerImpl.Log("P: " + processIds[i] + " is null.");
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                HookManagerImpl.Log($"Found eve procs: {eveProcsB}");

                HookManagerImpl.Log("[StealthTest] Enumprocesses end.");

                string[] dllNames =
                    {"SharedComponents.dll", "EasyHook.dll", "DomainHandler.dll", "NotExisting.dll", "HookManager.exe", "EVESharpCore.exe", "DirectEve.dll"};

                foreach (var dllName in dllNames)
                {
                    var ptr = LoadLibrary(dllName);
                    if (ptr != IntPtr.Zero)
                        HookManagerImpl.Log("[StealthTest-LoadLibrary] Handle found for " + dllName);
                    else
                        HookManagerImpl.Log("[StealthTest-LoadLibrary] Handle NOT found for " + dllName);

                    ptr = IntPtr.Zero;

                    ptr = GetModuleHandle(dllName);
                    if (ptr != IntPtr.Zero)
                        HookManagerImpl.Log("[StealthTest-GetModuleHandle] Handle found for: " + dllName);
                    else
                        HookManagerImpl.Log("[StealthTest-GetModuleHandle] Handle not found for: " + dllName);

                    ptr = IntPtr.Zero;
                }

                // hook imports test
                var l = new List<Tuple<String, String>>()
                {
                    new Tuple<string, string>("Kernel32.dll", "CreateFileA"),
                    new Tuple<string, string>("Kernel32.dll", "CreateFileW"),
                    new Tuple<string, string>("advapi32.dll", "CryptCreateHash"),
                    new Tuple<string, string>("advapi32.dll", "CryptHashData"),
                    new Tuple<string, string>("kernel32.dll", "K32EnumProcesses"),
                    new Tuple<string, string>("Iphlpapi.dll", "GetAdaptersInfo"),
                    new Tuple<string, string>("Kernel32.dll", "GetModuleHandleA"),
                    new Tuple<string, string>("Kernel32.dll", "GetModuleHandleW"),
                    new Tuple<string, string>("kernel32.dll", "GlobalMemoryStatusEx"),
                    new Tuple<string, string>("wininet.dll", "InternetConnectA"),
                    new Tuple<string, string>("wininet.dll", "InternetConnectW"),
                    new Tuple<string, string>("Kernel32.dll", "IsDebuggerPresent"),
                    new Tuple<string, string>("Kernel32.dll", "LoadLibraryA"),
                    new Tuple<string, string>("Kernel32.dll", "LoadLibraryW"),
                    new Tuple<string, string>("DbgHelp.dll", "MiniDumpWriteDump"),
                    new Tuple<string, string>("advapi32.dll", "RegQueryValueExA"),
                    new Tuple<string, string>("shell32.dll", "SHGetFolderPathA"),
                    new Tuple<string, string>("shell32.dll", "SHGetFolderPathW"),
                    new Tuple<string, string>("WS2_32.dll", "connect"),
                };

                foreach (var k in l)
                {
                    var a = IntPtr.Zero;
                    var b = IntPtr.Zero;
                    var c = IntPtr.Zero;
                    var d = IntPtr.Zero;

                    try
                    {
                        a = Util.GetImportAddress("_ctypes.pyd", k.Item1, k.Item2);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(k.ToString());
                        Console.WriteLine("_ctypes.pyd Ex:" + e);
                    }

                    try
                    {
                        b = Util.GetImportAddress("blue.dll", k.Item1, k.Item2);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(k.ToString());
                        Console.WriteLine("blue.dll Ex:" + e);
                    }

                    try
                    {
                        c = Util.GetImportAddress("exefile.exe", k.Item1, k.Item2);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(k.ToString());
                        Console.WriteLine("exefile.exe Ex: " + e);
                    }

                    try
                    {
                        d = LocalHook.GetProcAddress(k.Item1, k.Item2);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(k.ToString());
                        Console.WriteLine("GetProcAddress Ex: " + e);
                    }

                    HookManagerImpl.Log(string.Format("{0}: {1} - {2} - {3} - {4}", k, a, b, c, d));
                }
            }).Start();
        }

        [DllImport("psapi.dll")]
        private static extern bool EnumProcesses(
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In] [Out] UInt32[] processIds,
            UInt32 arraySizeBytes,
            [MarshalAs(UnmanagedType.U4)] out UInt32 bytesCopied);

        [DllImport("kernel32.dll")]
        private static extern bool K32EnumProcesses(
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In] [Out] UInt32[] processIds,
            UInt32 arraySizeBytes,
            [MarshalAs(UnmanagedType.U4)] out UInt32 bytesCopied);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        #endregion Methods
    }
}