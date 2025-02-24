/*
 * ---------------------------------------
 * User: duketwo
 * Date: 12.12.2013
 * Time: 12:51
 *
 * ---------------------------------------
 */

using EasyHook;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of IsDebuggerPresent.
    /// </summary>
    public class CreateFileWController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        #endregion Fields

        #region Constructors

        public CreateFileWController()
        {
            Error = false;
            Name = typeof(CreateFileWController).Name;

            try
            {
                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("kernel32.dll", "CreateFileW"),
                    new CreateFileWDelegate(CreateFileWDetour),
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
        private delegate IntPtr CreateFileWDelegate(
            string InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods

        public static bool IsRead(UInt32 InDesiredAccess)
        {
            return InDesiredAccess == 2147483648 || InDesiredAccess == 131072 || InDesiredAccess == 256;
        }

        // GENERIC_READ = 2147483648
        // GENERIC_WRITE = 1073741824
        // GENERIC READ+WRITE = 0xC0000000 -> 3221225472
        // fILE_ATTRIBUTE_TEMPORARY      =  256
        // sTANDARD_RIGHTS_READ      =  131072
        public static bool IsWrite(UInt32 InDesiredAccess)
        {
            return InDesiredAccess == 1073741824 || InDesiredAccess == 3221225472;
        }

        public void Dispose()
        {
            _hook.Dispose();
        }

        [DllImport("kernel32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CreateFileW(
            string InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile);

        private static IntPtr CreateFileWDetour(
            string InFileName,
            UInt32 InDesiredAccess,
            UInt32 InShareMode,
            IntPtr InSecurityAttributes,
            UInt32 InCreationDisposition,
            UInt32 InFlagsAndAttributes,
            IntPtr InTemplateFile)
        {
            if (HookManagerImpl.Instance.EveWndShown)
                try
                {
                    var inFileName = InFileName;
                    var inDesiredAccess = InDesiredAccess;
                    var fileName = Path.GetFileName(inFileName);
                    var pathName = Path.GetDirectoryName(inFileName);
                    if (HookManagerImpl.IsBacklistedDirectory(pathName))
                        HookManagerImpl.Log("[BLACKLISTED_FILE] " + pathName + "\\" + fileName + " [DESIRED_ACCESS] " + inDesiredAccess.ToString());

                    var isRead = IsRead(inDesiredAccess);
                    var isWrite = IsWrite(inDesiredAccess);

                    if (isRead && HookManagerImpl.IsWhiteListedReadDirectory(pathName))
                    {
                        //	HookManager.Log("[Whitelisted] CreateFileWDetour-lpFileName(READ): " + pathName + "\\" + fileName + " Desired Access: " + InDesiredAccess.ToString());
                    }
                    else
                    {
                        if (isWrite && HookManagerImpl.IsWhiteListedWriteDirectory(pathName))
                        {
                            //	HookManager.Log("[Whitelisted] CreateFileWDetour-lpFileName(WRITE): " + pathName + "\\" + fileName + " Desired Access: " + InDesiredAccess.ToString());
                        }
                        else
                        {
                            if (inDesiredAccess != 0)
                            {
                                var t = isRead ? "READ" : "WRITE";
                                HookManagerImpl.Log("[NOT_WHITELISTED_FILE] " + pathName + "\\" + fileName + " [DESIRED_ACCESS] " + t);
                                //return new IntPtr(-1);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    HookManagerImpl.Log("Exception:  " + e.ToString());
                }

            var ret = CreateFileW(InFileName, InDesiredAccess, InShareMode, InSecurityAttributes, InCreationDisposition, InFlagsAndAttributes, InTemplateFile);
            return ret;
        }

        #endregion Methods
    }
}