using EasyHook;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;

namespace HookManager.Win32Hooks
{
    public class GetUserNameController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;
        private static string _userName;

        #endregion Fields

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool GetUserNameW(IntPtr buffer, ref Int32 length);

        private static IntPtr _userNameBuffer;

        #region Constructors

        public GetUserNameController(string userName)
        {
            Error = false;
            Name = typeof(GetUserNameController).Name;
            _userName = userName;
            try
            {

                _userNameBuffer = Marshal.StringToHGlobalUni(userName);

                _hook = LocalHook.Create(
                    LocalHook.GetProcAddress("advapi32.dll", "GetUserNameW"),
                    new Delegate(DetourW),
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

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate bool Delegate(IntPtr buffer, ref Int32 length);

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

        private static bool DetourW(IntPtr buffer, ref Int32 length)
        {

            //Clear the buffer
            for (int i = 0; i < length; i++)
            {
                Marshal.WriteByte(buffer, i, 0);
            }

            // Overwrite the username
            var userNameLength = _userName.Length;
            for (int i = 0; i < userNameLength; i++)
            {
                Marshal.WriteInt16(buffer, i * 2, Marshal.ReadInt16(_userNameBuffer, i * 2));
            }
            length = userNameLength + 1;
            
            return true;
        }

        #endregion Methods
    }
}