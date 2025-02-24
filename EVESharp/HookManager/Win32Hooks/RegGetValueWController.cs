using EasyHook;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using SharedComponents.Utility;
using SharedComponents.IPC;

namespace HookManager.Win32Hooks
{
    public class RegGetValueWController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        /* Retrieves the type and data for the specified registry value. - C# Compliant types*/
        [DllImport("Advapi32.dll", EntryPoint = "RegGetValueW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern Int32 RegGetValue(
        IntPtr hkey,
        string lpSubKey,
        string lpValue,
        RFlags dwFlags,
        out RType pdwType,
        IntPtr pvData,
        ref UInt32 pcbData);

        [DllImport("kernel32.dll", EntryPoint = "RtlFillMemory", SetLastError = false)]
        static extern void FillMemory(IntPtr destination, uint length, byte fill);

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms724884(v=vs.85).aspx
        /// https://docs.microsoft.com/en-us/windows/desktop/api/Winreg/nf-winreg-reggetvaluea
        /// </summary>
        [Flags]
        internal enum RFlags
        {
            /// <summary>
            /// Any - No type restriction. (0x0000ffff)
            /// </summary>
            Any = 65535,

            /// <summary>
            /// Restrict type to REG_NONE. (0x00000001)
            /// </summary>
            RegNone = 1,

            /// <summary>
            /// Do not automatically expand environment strings if the value is of type REG_EXPAND_SZ. (0x10000000)
            /// </summary>
            Noexpand = 268435456,

            /// <summary>
            /// Bytes - Restrict type to REG_BINARY. (0x00000008)
            /// </summary>
            RegBinary = 8,

            /// <summary>
            /// Int32 - Restrict type to 32-bit RRF_RT_REG_BINARY | RRF_RT_REG_DWORD. (0x00000018)
            /// </summary>
            Dword = 24,

            /// <summary>
            /// Int32 - Restrict type to REG_DWORD. (0x00000010)
            /// </summary>
            RegDword = 16,

            /// <summary>
            /// Int64 - Restrict type to 64-bit RRF_RT_REG_BINARY | RRF_RT_REG_DWORD. (0x00000048)
            /// </summary>
            Qword = 72,

            /// <summary>
            /// Int64 - Restrict type to REG_QWORD. (0x00000040)
            /// </summary>
            RegQword = 64,

            /// <summary>
            /// A null-terminated string.
            /// This will be either a Unicode or an ANSI string,
            /// depending on whether you use the Unicode or ANSI functions.
            /// Restrict type to REG_SZ. (0x00000002)
            /// </summary>
            RegSz = 2,

            /// <summary>
            /// A sequence of null-terminated strings, terminated by an empty string (\0).
            /// The following is an example:
            /// String1\0String2\0String3\0LastString\0\0
            /// The first \0 terminates the first string, the second to the last \0 terminates the last string,
            /// and the final \0 terminates the sequence. Note that the final terminator must be factored into the length of the string.
            /// Restrict type to REG_MULTI_SZ. (0x00000020)
            /// </summary>
            RegMultiSz = 32,

            /// <summary>
            /// A null-terminated string that contains unexpanded references to environment variables (for example, "%PATH%").
            /// It will be a Unicode or ANSI string depending on whether you use the Unicode or ANSI functions.
            /// To expand the environment variable references, use the ExpandEnvironmentStrings function.
            /// Restrict type to REG_EXPAND_SZ. (0x00000004)
            /// </summary>
            RegExpandSz = 4,

            /// <summary>
            /// If pvData is not NULL, set the contents of the buffer to zeroes on failure. (0x20000000)
            /// </summary>
            RrfZeroonfailure = 536870912
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms724884(v=vs.85).aspx
        /// </summary>
        internal enum RType
        {
            RegNone = 0,

            RegSz = 1,
            RegExpandSz = 2,
            RegMultiSz = 7,

            RegBinary = 3,
            RegDword = 4,
            RegQword = 11,

            RegQwordLittleEndian = 11,
            RegDwordLittleEndian = 4,
            RegDwordBigEndian = 5,

            RegLink = 6,
            RegResourceList = 8,
            RegFullResourceDescriptor = 9,
            RegResourceRequirementsList = 10,
        }

        internal const int ERROR_MORE_DATA = 234;

        #endregion Fields

        #region Constructors

        public RegGetValueWController()
        {
            Error = false;
            Name = typeof(RegGetValueWController).Name;

            try
            {

                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("Advapi32.dll", "RegGetValueW"),
                    new Delegate(Detour),
                    this);


                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
                Error = false;
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int Delegate(
        IntPtr hkey,
        string lpSubKey,
        string lpValue,
        RFlags dwFlags,
        out RType pdwType,
        IntPtr pvData,
        ref UInt32 pcbData);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods



        public void Dispose()
        {
            _hook.Dispose();
        }

        private static int Detour(
        IntPtr hkey,
        string lpSubKey,
        string lpValue,
        RFlags dwFlags,
        out RType pdwType,
        IntPtr pvData,
        ref UInt32 pcbData)
        {

            var bufferSize = pcbData;
            var result = RegGetValue(hkey, lpSubKey, lpValue, dwFlags, out pdwType, pvData, ref pcbData);
            if (lpValue == "SystemManufacturer" || lpValue == "SystemProductName" || lpValue == "SystemFamily" || lpValue == "SystemSKU" || lpValue == "SystemVersion")
            {
                var orignalValue = Marshal.PtrToStringUni(pvData);
                var newValue = IntPtr.Zero;
                try
                {
                    var s = "To Be Filled By O.E.M.";
                    newValue = Marshal.StringToHGlobalUni(s);
                    var size = System.Text.UnicodeEncoding.Unicode.GetByteCount(s);
                    pcbData = (uint)size + 1;
                    if (size > bufferSize)
                    {
                        WCFClient.Instance.GetPipeProxy.RemoteLog(
                            $"Warning :{lpValue}  size:{size}  bufferSize{bufferSize}");
                        return ERROR_MORE_DATA;
                    }

                    FillMemory(pvData, bufferSize, 0);
                    Util.CopyMemory(pvData, newValue, (uint)size);
                }
                finally
                {
                    if (newValue != IntPtr.Zero)
                        Marshal.FreeHGlobal(newValue);
                }
            }


            return result;
        }

        #endregion Methods
    }
}