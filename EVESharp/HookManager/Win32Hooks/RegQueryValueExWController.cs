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
    public class RegQueryValueExWController : IDisposable, IHook
    {
        #region Fields

        private LocalHook _hook;

        private string _name;

        private HWSettings _hwSettings;

        #endregion Fields

        #region Constructors

        public RegQueryValueExWController(IntPtr address, HWSettings hWSettings)
        {
            try
            {
                _hwSettings = hWSettings;
                if (_hwSettings == null)
                    throw new Exception("HWSettings null.");
                Name = typeof(RegQueryValueExAController).Name;
                _name = string.Format("RegQueryValueExWHook_{0:X}", address.ToInt64());
                _hook = LocalHook.Create(address, new RegQueryValueExWDelegate(RegQueryValueExWDetour), this);
                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int RegQueryValueExWDelegate(UIntPtr hKey, IntPtr lpValueName, int lpReserved, IntPtr lpType, IntPtr lpData, IntPtr lpcbData);

        #endregion Delegates

        [DllImport("kernel32.dll", EntryPoint = "RtlFillMemory", SetLastError = false)]
        static extern void FillMemory(IntPtr destination, uint length, byte fill);

        internal const int ERROR_MORE_DATA = 234;

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int RegQueryValueExW(UIntPtr hKey, IntPtr lpValueName, int lpReserved, IntPtr lpType, IntPtr lpData, IntPtr lpcbData);

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        private int RegQueryValueExWDetour(UIntPtr hKey, IntPtr lpValueName, int lpReserved, IntPtr lpType,
            IntPtr lpData, IntPtr lpcbData)
        {
            try
            {
                var result = RegQueryValueExW(hKey, lpValueName, lpReserved, lpType, lpData, lpcbData);

                //try
                //{
                //    if (lpcbData != IntPtr.Zero)
                //    {
                //        uint bufferSize = (uint)Marshal.ReadInt32(lpcbData);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Util.GlobalRemoteLog($"{ex}");
                //}

                if (lpType == IntPtr.Zero && lpData == IntPtr.Zero || lpcbData == IntPtr.Zero)
                    return result;

                var keyValue = Marshal.PtrToStringUni(lpValueName);
                var lpDataString = Marshal.PtrToStringUni(lpData);

                //if (keyValue == "MachineHash" || keyValue == "ProductId" || keyValue == "MachineGuid"
                //    || keyValue == "DriverDesc" || keyValue == "Device Description" || keyValue == "DriverVersion"
                //    || keyValue == "ProviderName"
                //    || keyValue == "DriverDate")

                uint bufferSize = (uint)Marshal.ReadInt32(lpcbData);

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
                    var returnValue = Marshal.PtrToStringUni(lpData);
                    var newValue = IntPtr.Zero;
                    try
                    {
                        newValue = Marshal.StringToHGlobalUni(_hwSettings.WindowsKey);
                        var size = System.Text.UnicodeEncoding.Unicode.GetByteCount(_hwSettings.WindowsKey);
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
                    var returnValue = Marshal.PtrToStringUni(lpData);
                    var newValue = IntPtr.Zero;
                    try
                    {
                        newValue = Marshal.StringToHGlobalUni(_hwSettings.MachineGuid);
                        var size = System.Text.UnicodeEncoding.Unicode.GetByteCount(_hwSettings.MachineGuid);
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
                    var returnValue = Marshal.PtrToStringUni(lpData);
                    var newValue = IntPtr.Zero;
                    try
                    {
                        newValue = Marshal.StringToHGlobalUni(_hwSettings.GpuDescription);
                        var size = System.Text.UnicodeEncoding.Unicode.GetByteCount(_hwSettings.GpuDescription);
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
                        newValue = Marshal.StringToHGlobalUni(_hwSettings.GpuDriverversionInt);
                        var size = System.Text.UnicodeEncoding.Unicode.GetByteCount(_hwSettings.GpuDriverversionInt);
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
                        newValue = Marshal.StringToHGlobalUni(_hwSettings.GpuManufacturer);
                        var size = System.Text.UnicodeEncoding.Unicode.GetByteCount(_hwSettings.GpuManufacturer);
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
                        var size = System.Text.UnicodeEncoding.Unicode.GetByteCount(s);
                            Marshal.WriteInt32(lpcbData, size);
                        if (size > bufferSize)
                        {
                                WCFClient.Instance.GetPipeProxy.RemoteLog(
                                    $"Warning :{keyValue}  size:{size}  bufferSize{bufferSize}");
                                return ERROR_MORE_DATA;
                        }
                        newValue = Marshal.StringToHGlobalUni(s);
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

                var lpDataStringAfter = Marshal.PtrToStringUni(lpData);
                if (log)
                {

                    HookManagerImpl.Log("[BEFORE] " + lpDataString.ToString(), Color.Orange);
                    HookManagerImpl.Log("[AFTER] " + lpDataStringAfter.ToString(), Color.Orange);
                }

                //WCFClient.Instance.GetPipeProxy.RemoteLog($"EX | Key: {keyValue} Value {lpDataString} Changed Value {lpDataStringAfter}");
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