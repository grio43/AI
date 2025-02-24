/*
 * ---------------------------------------
 * User: duketwo
 * Date: 21.06.2014
 * Time: 15:03
 *
 * ---------------------------------------
 */

using EasyHook;
using SharedComponents.Utility;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using SharedComponents.EVE;
using SharedComponents.IPC;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of RegistryController.
    /// </summary>
    public class RegQueryValueExAController : IDisposable, IHook
    {
        #region Fields

        private LocalHook _hook;

        private string _name;

        private HWSettings _hwSettings;

        #endregion Fields

        #region Constructors

        public RegQueryValueExAController(IntPtr address, HWSettings hWSettings)
        {
            try
            {
                _hwSettings = hWSettings;
                if (_hwSettings == null)
                    throw new Exception("HWSettings null.");
                Name = typeof(RegQueryValueExAController).Name;
                _name = string.Format("RegQueryValueExAHook_{0:X}", address.ToInt64());
                _hook = LocalHook.Create(address, new RegQueryValueExADelegate(RegQueryValueExADetour), this);
                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
        private delegate int RegQueryValueExADelegate(UIntPtr hKey, IntPtr lpValueName, int lpReserved, IntPtr lpType, IntPtr lpData, IntPtr lpcbData);

        #endregion Delegates

        [DllImport("kernel32.dll", EntryPoint = "RtlFillMemory", SetLastError = false)]
        static extern void FillMemory(IntPtr destination, uint length, byte fill);

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        internal const int ERROR_MORE_DATA = 234;

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegQueryValueExA(UIntPtr hKey, IntPtr lpValueName, int lpReserved, IntPtr lpType, IntPtr lpData, IntPtr lpcbData);

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        private int RegQueryValueExADetour(UIntPtr hKey, IntPtr lpValueName, int lpReserved, IntPtr lpType, IntPtr lpData, IntPtr lpcbData)
        {
            try
            {
                // Int32 to Uint32 can fail in checked contexts, unchecked does bitwise conversion
                uint bufferSize = unchecked((uint)Marshal.ReadInt32(lpcbData));

                var result = RegQueryValueExA(hKey, lpValueName, lpReserved, lpType, lpData, lpcbData);

                uint bufferSizeAfter = (uint)Marshal.ReadInt32(lpcbData);

                if (lpType == IntPtr.Zero && lpData == IntPtr.Zero || lpcbData == IntPtr.Zero)
                    return result;

                var keyValue = Marshal.PtrToStringAnsi(lpValueName);
                var lpDataString = Marshal.PtrToStringAnsi(lpData);
                var log = false;

                if (keyValue == "MachineHash" || keyValue == "DeviceId" || keyValue == "DeviceIdV2")
                {
                    WCFClient.Instance.GetPipeProxy.RemoteLog(
                            "MachineHash registry value was read. Quitting/disabling this instance.");
                    WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValue(WCFClient.Instance.GUID,
                            nameof(EveAccount.UseScheduler), false);
                    Environment.Exit(0);
                    Environment.FailFast("exit"); // shouldn't reach the code below, failfast terminates instantly.
                        return
                            0x2; // If the lpValueName registry value does not exist, the function returns ERROR_FILE_NOT_FOUND (0x2).
                }

                if (keyValue == "ProductId")
                {
                    log = true;
                    var returnValue = Marshal.PtrToStringAnsi(lpData);
                    var newValue = IntPtr.Zero;
                    try
                    {
                        newValue = Marshal.StringToHGlobalAnsi(_hwSettings.WindowsKey);
                        var size = System.Text.ASCIIEncoding.ASCII.GetByteCount(_hwSettings.WindowsKey);
                            Marshal.WriteInt32(lpcbData, size);
                        if (size > bufferSize)
                        {
                                WCFClient.Instance.GetPipeProxy.RemoteLog(
                                    $"Warning :{keyValue}  size:{size}  bufferSize{bufferSize}");
                                return ERROR_MORE_DATA;
                        }
                        FillMemory(lpData, bufferSize, 0);
                        Marshal.WriteInt32(lpcbData, size);
                        Util.CopyMemory(lpData, newValue, (uint)size);
                    }
                    finally
                    {
                        if (newValue != IntPtr.Zero)
                            Marshal.FreeHGlobal(newValue);
                    }
                }

                if (keyValue == "MachineGuid")
                {
                    log = true;
                    var returnValue = Marshal.PtrToStringAnsi(lpData);
                    var newValue = IntPtr.Zero;
                    try
                    {
                        newValue = Marshal.StringToHGlobalAnsi(_hwSettings.MachineGuid);
                        var size = System.Text.ASCIIEncoding.ASCII.GetByteCount(_hwSettings.MachineGuid);
                            Marshal.WriteInt32(lpcbData, size);
                        if (size > bufferSize)
                        {
                                WCFClient.Instance.GetPipeProxy.RemoteLog(
                                    $"Warning :{keyValue}  size:{size}  bufferSize{bufferSize}");
                                return ERROR_MORE_DATA;
                        }
                        FillMemory(lpData, bufferSize, 0);
                        Marshal.WriteInt32(lpcbData, size);
                        Util.CopyMemory(lpData, newValue, (uint)size);
                    }
                    finally
                    {
                        if (newValue != IntPtr.Zero)
                            Marshal.FreeHGlobal(newValue);
                    }
                }

                if (keyValue == "DriverDesc" || keyValue == "Device Description")
                {
                    log = true;
                    var returnValue = Marshal.PtrToStringAnsi(lpData);
                    var newValue = IntPtr.Zero;
                    try
                    {
                        newValue = Marshal.StringToHGlobalAnsi(_hwSettings.GpuDescription);
                        var size = System.Text.ASCIIEncoding.ASCII.GetByteCount(_hwSettings.GpuDescription);
                            Marshal.WriteInt32(lpcbData, size);
                        if (size > bufferSize)
                        {
                                WCFClient.Instance.GetPipeProxy.RemoteLog(
                                    $"Warning :{keyValue}  size:{size}  bufferSize{bufferSize}");
                                return ERROR_MORE_DATA;
                        }
                        FillMemory(lpData, bufferSize, 0);
                        Marshal.WriteInt32(lpcbData, size);
                        Util.CopyMemory(lpData, newValue, (uint)size);
                    }
                    finally
                    {
                        if (newValue != IntPtr.Zero)
                            Marshal.FreeHGlobal(newValue);
                    }
                }

                if (keyValue == "DriverVersion")
                {
                    log = true;
                    var newValue = IntPtr.Zero;
                    try
                    {
                        newValue = Marshal.StringToHGlobalAnsi(_hwSettings.GpuDriverversionInt);
                        var size = System.Text.ASCIIEncoding.ASCII.GetByteCount(_hwSettings.GpuDriverversionInt);
                            Marshal.WriteInt32(lpcbData, size);
                        if (size > bufferSize)
                        {
                                WCFClient.Instance.GetPipeProxy.RemoteLog(
                                    $"Warning :{keyValue}  size:{size}  bufferSize{bufferSize}");
                                return ERROR_MORE_DATA;
                        }
                        FillMemory(lpData, bufferSize, 0);
                        Marshal.WriteInt32(lpcbData, size);
                        Util.CopyMemory(lpData, newValue, (uint)size);
                    }
                    finally
                    {
                        if (newValue != IntPtr.Zero)
                            Marshal.FreeHGlobal(newValue);
                    }
                }

                if (keyValue == "ProviderName")
                {
                    log = true;
                    var newValue = IntPtr.Zero;
                    try
                    {
                        newValue = Marshal.StringToHGlobalAnsi(_hwSettings.GpuManufacturer);
                        var size = System.Text.ASCIIEncoding.ASCII.GetByteCount(_hwSettings.GpuManufacturer);
                            Marshal.WriteInt32(lpcbData, size);
                        if (size > bufferSize)
                        {
                                WCFClient.Instance.GetPipeProxy.RemoteLog(
                                    $"Warning :{keyValue}  size:{size}  bufferSize{bufferSize}");
                                return ERROR_MORE_DATA;
                        }
                        FillMemory(lpData, bufferSize, 0);
                        Marshal.WriteInt32(lpcbData, size);
                        Util.CopyMemory(lpData, newValue, (uint)size);
                    }
                    finally
                    {
                        if (newValue != IntPtr.Zero)
                            Marshal.FreeHGlobal(newValue);
                    }
                }

                if (keyValue == "DriverDate")
                {
                    log = true;
                    var newValue = IntPtr.Zero;
                    try
                    {
                            var s =
                                $"{_hwSettings.GpuDriverDate.Month}-{_hwSettings.GpuDriverDate.Day}-{_hwSettings.GpuDriverDate.Year}";
                        var size = System.Text.ASCIIEncoding.ASCII.GetByteCount(s);

                            try
                            {
                                //IntPtr ptr = unchecked((IntPtr)(long)(ulong)hKey);
                                //var hkeyName = Win32Registry.GetHKeyName(ptr);
                                //Log.RemoteWriteLine($"DriverDate KeyName: {hkeyName}");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                            Marshal.WriteInt32(lpcbData, size);
                        if (size > bufferSize)
                        {
                                WCFClient.Instance.GetPipeProxy.RemoteLog(
                                    $"Warning :{keyValue}  size:{size}  bufferSize{bufferSize}");
                                return ERROR_MORE_DATA;
                        }
                        newValue = Marshal.StringToHGlobalAnsi(s);
                        FillMemory(lpData, bufferSize, 0);
                        Marshal.WriteInt32(lpcbData, size);
                        Util.CopyMemory(lpData, newValue, (uint)size);
                    }
                    finally
                    {
                        if (newValue != IntPtr.Zero)
                            Marshal.FreeHGlobal(newValue);
                    }
                }

                var lpDataStringAfter = Marshal.PtrToStringAnsi(lpData);
                if (log)
                {
                    //Util.GlobalRemoteLog("[BEFORE] " + lpDataString.ToString());
                    //Util.GlobalRemoteLog("[AFTER] " + lpDataStringAfter.ToString());
                        HookManagerImpl.Log("[BEFORE] " + lpDataString.ToString(), Color.Orange);
                        HookManagerImpl.Log("[AFTER] " + lpDataStringAfter.ToString(), Color.Orange);
                }

                //WCFClient.Instance.GetPipeProxy.RemoteLog($"Key: {keyValue} Value {lpDataString} Changed Value {lpDataStringAfter}");
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.RemoteWriteLine($"Exception: {e}");
                WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValue(WCFClient.Instance.GUID,
                    nameof(EveAccount.UseScheduler), false);
                Environment.Exit(0);
                Environment.FailFast("exit"); // shouldn't reach the code below, failfast terminates instantly.
            }

            return 0x2;
        }

        #endregion Methods
    }
}