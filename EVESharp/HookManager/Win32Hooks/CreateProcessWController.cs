using EasyHook;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using SharedComponents.Extensions;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of CreateProcessController.
    /// </summary>
    public class CreateProcessWController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public CreateProcessWController()
        {
            Error = false;
            Name = nameof(CreateProcessWController);

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "CreateProcessW"),
                    new CreateProcessDelegate(CreateProcessDetour),
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

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate bool CreateProcessDelegate(
            [InAttribute()][MarshalAsAttribute(UnmanagedType.LPWStr)] string LpApplicationName,
            [InAttribute()][MarshalAsAttribute(UnmanagedType.LPWStr)] string lpCommandLine,
            [InAttribute()] IntPtr lpProcessAttributes,
            [InAttribute()] IntPtr lpThreadAttributes,
            [MarshalAsAttribute(UnmanagedType.Bool)] bool bInheritHandles,
            uint dwCreationFlags,
            [InAttribute()] IntPtr lpEnvironment,
            [InAttribute()][MarshalAsAttribute(UnmanagedType.LPWStr)] string lpCurrentDirectory,
            [InAttribute()] ref STARTUPINFOW lpStartupInfo,
            [InAttribute()] ref PROCESS_INFORMATION lpProcessInformation);

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

        [DllImportAttribute("kernel32.dll", EntryPoint = "CreateProcessW")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        public static extern bool CreateProcessW(
            [InAttribute()][MarshalAsAttribute(UnmanagedType.LPWStr)] string LpApplicationName,
            [InAttribute()][MarshalAsAttribute(UnmanagedType.LPWStr)] string lpCommandLine,
            [InAttribute()] IntPtr lpProcessAttributes,
            [InAttribute()] IntPtr lpThreadAttributes,
            [MarshalAsAttribute(UnmanagedType.Bool)] bool bInheritHandles,
            uint dwCreationFlags,
            [InAttribute()] IntPtr lpEnvironment,
            [InAttribute()][MarshalAsAttribute(UnmanagedType.LPWStr)] string lpCurrentDirectory,
            [InAttribute()] ref STARTUPINFOW lpStartupInfo,
            [InAttribute()] ref PROCESS_INFORMATION lpProcessInformation);


        private bool CreateProcessDetour(
            [InAttribute()][MarshalAsAttribute(UnmanagedType.LPWStr)] string LpApplicationName,
            [InAttribute()][MarshalAsAttribute(UnmanagedType.LPWStr)] string lpCommandLine,
            [InAttribute()] IntPtr lpProcessAttributes,
            [InAttribute()] IntPtr lpThreadAttributes,
            [MarshalAsAttribute(UnmanagedType.Bool)] bool bInheritHandles,
            uint dwCreationFlags,
            [InAttribute()] IntPtr lpEnvironment,
            [InAttribute()][MarshalAsAttribute(UnmanagedType.LPWStr)] string lpCurrentDirectory,
            [InAttribute()] ref STARTUPINFOW lpStartupInfo,
            [InAttribute()] ref PROCESS_INFORMATION lpProcessInformation)
        {

            var msg = $"CreateProcW lpApplicationName {LpApplicationName} lpCommandLine {lpCommandLine} lpCurrentDirectory {lpCurrentDirectory}";
            HookManagerImpl.Log(msg);
            Trace.WriteLine(msg);
            var commandLineLower = lpCommandLine.ToLower();
            if (CreateProcessAController.AllowedProcesses.Any(commandLineLower.ContainsIgnoreCase))
            {
                var ret = CreateProcessW(LpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes,
                    bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, ref lpStartupInfo, ref lpProcessInformation);

                return ret;
            }
            return false;
        }

        #endregion Methods


        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct STARTUPINFOW
        {
            public uint cb;
            [MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public string lpReserved;
            [MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public string lpDesktop;
            [MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public string lpTitle;
            public uint dwX;
            public uint dwy;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public ushort wShowWindow;
            public ushort cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

    }
}