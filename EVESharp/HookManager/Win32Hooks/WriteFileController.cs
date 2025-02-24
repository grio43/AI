
/*
 * ---------------------------------------
 * User: duke2
 * Date: 01.21.2025
 * ---------------------------------------
 */


using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace HookManager.Win32Hooks
{
    public class WriteFileController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;
        private HashSet<IntPtr> _filteredHandles;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        // Constants for standard handles
        private const int STD_OUTPUT_HANDLE = -11;

        #endregion Fields

        #region Constructors

        public WriteFileController()
        {
            Error = false;
            Name = nameof(WriteFileController);

            // Initialize the set of filtered handles
            _filteredHandles = new HashSet<IntPtr>();

            // Add STD_OUTPUT_HANDLE by default
            _filteredHandles.Add(GetStdHandle(STD_OUTPUT_HANDLE));

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "WriteFile"),
                    new WriteFileDelegate(WriteFileDetour),
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

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate bool WriteFileDelegate(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

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

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        private bool WriteFileDetour(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped)
        {
            try
            {
                // Check if the handle is in the filtered set
                if (_filteredHandles.Contains(hFile) || GetStdHandle(STD_OUTPUT_HANDLE) == hFile)
                {
                    Debug.WriteLine($"\"----------------- [Filtered WriteFile] Handle: {hFile}");

                    if (lpBuffer != IntPtr.Zero && nNumberOfBytesToWrite > 0)
                    {
                        var buffer = new byte[nNumberOfBytesToWrite];
                        Marshal.Copy(lpBuffer, buffer, 0, (int)nNumberOfBytesToWrite);
                        var text = Encoding.Default.GetString(buffer);
                        Debug.WriteLine($"-----------------Data: {text}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("\"----------------- Exception in WriteFile hook: " + e);
            }

            // Call the original WriteFile function
            return WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, out lpNumberOfBytesWritten, lpOverlapped);
        }

        #endregion Methods
    }
}
