/*
 * ---------------------------------------
 * User: duke2
 * Date: 01.21.2025
 * ---------------------------------------
 */

using EasyHook;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace HookManager.Win32Hooks
{
    public class WriteConsoleController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hookA;
        private LocalHook _hookW;

        #endregion Fields

        #region Constructors

        public WriteConsoleController()
        {
            Error = false;
            Name = typeof(WriteConsoleController).Name;

            try
            {
                _hookA = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "WriteConsoleA"),
                    new WriteConsoleADelegate(WriteConsoleADetour),
                    this);

                _hookW = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "WriteConsoleW"),
                    new WriteConsoleWDelegate(WriteConsoleWDetour),
                    this);

                _hookA.ThreadACL.SetExclusiveACL(new Int32[] { });
                _hookW.ThreadACL.SetExclusiveACL(new Int32[] { });

                Error = false;
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
        private delegate bool WriteConsoleADelegate(
            IntPtr hConsoleOutput,
            IntPtr lpBuffer,
            uint nNumberOfCharsToWrite,
            out uint lpNumberOfCharsWritten,
            IntPtr lpReserved);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate bool WriteConsoleWDelegate(
            IntPtr hConsoleOutput,
            IntPtr lpBuffer,
            uint nNumberOfCharsToWrite,
            out uint lpNumberOfCharsWritten,
            IntPtr lpReserved);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            _hookA.Dispose();
            _hookW.Dispose();
        }

        [DllImport("kernel32.dll",
            CharSet = CharSet.Ansi,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool WriteConsoleA(
            IntPtr hConsoleOutput,
            IntPtr lpBuffer,
            uint nNumberOfCharsToWrite,
            out uint lpNumberOfCharsWritten,
            IntPtr lpReserved);

        [DllImport("kernel32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern bool WriteConsoleW(
            IntPtr hConsoleOutput,
            IntPtr lpBuffer,
            uint nNumberOfCharsToWrite,
            out uint lpNumberOfCharsWritten,
            IntPtr lpReserved);

        private static bool WriteConsoleADetour(
            IntPtr hConsoleOutput,
            IntPtr lpBuffer,
            uint nNumberOfCharsToWrite,
            out uint lpNumberOfCharsWritten,
            IntPtr lpReserved)
        {
            try
            {
                if (lpBuffer != IntPtr.Zero && nNumberOfCharsToWrite > 0)
                {
                    var buffer = new byte[nNumberOfCharsToWrite];
                    Marshal.Copy(lpBuffer, buffer, 0, (int)nNumberOfCharsToWrite);
                    var text = Encoding.ASCII.GetString(buffer);
                    Debug.WriteLine("[WriteConsoleA] Output: " + text);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in WriteConsoleA hook: " + e);
            }

            return WriteConsoleA(hConsoleOutput, lpBuffer, nNumberOfCharsToWrite, out lpNumberOfCharsWritten, lpReserved);
        }

        private static bool WriteConsoleWDetour(
            IntPtr hConsoleOutput,
            IntPtr lpBuffer,
            uint nNumberOfCharsToWrite,
            out uint lpNumberOfCharsWritten,
            IntPtr lpReserved)
        {
            try
            {
                if (lpBuffer != IntPtr.Zero && nNumberOfCharsToWrite > 0)
                {
                    var buffer = new char[nNumberOfCharsToWrite];
                    Marshal.Copy(lpBuffer, buffer, 0, (int)nNumberOfCharsToWrite);
                    var text = new string(buffer);
                    Debug.WriteLine("[WriteConsoleW] Output: " + text);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in WriteConsoleW hook: " + e);
            }

            return WriteConsoleW(hConsoleOutput, lpBuffer, nNumberOfCharsToWrite, out lpNumberOfCharsWritten, lpReserved);
        }

        #endregion Methods
    }
}
