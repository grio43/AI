using EasyHook;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace HookManager.Win32Hooks
{
    public class PostSendMessageController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hookW;
        private LocalHook _hookA;

        private LocalHook _sendHookW;
        private LocalHook _sendHookA;

        #endregion Fields

        [DllImport("User32.Dll", EntryPoint = "PostMessageW", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool PostMessageW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.Dll", EntryPoint = "PostMessageA", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool PostMessageA(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);


        [DllImport("User32.Dll", EntryPoint = "SendMessageW", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SendMessageW(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        [DllImport("User32.Dll", EntryPoint = "SendMessageA", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SendMessageA(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        #region Constructors

        public PostSendMessageController()
        {
            Error = false;
            Name = typeof(PostSendMessageController).Name;

            try
            {
                //_hookW = LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "PostMessageW"), new SendPostMessageWDelegate(DetourW), this);
                //_hookA = LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "PostMessageA"), new SendPostMessageADelegate(DetourA), this);
                //_sendHookA = LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "SendMessageA"), new SendPostMessageADelegate(SendDetourA), this);
                _sendHookW = LocalHook.Create(LocalHook.GetProcAddress("user32.dll", "SendMessageW"), new SendPostMessageWDelegate(SendDetourW), this);


                //_hookW.ThreadACL.SetExclusiveACL(new Int32[] { });
                //_hookA.ThreadACL.SetExclusiveACL(new Int32[] { });
                //_sendHookA.ThreadACL.SetExclusiveACL(new Int32[] { });
                _sendHookW.ThreadACL.SetExclusiveACL(new Int32[] { });
                Error = false;
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        
        private delegate bool SendPostMessageADelegate(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        private delegate bool SendPostMessageWDelegate(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            _hookW.Dispose();
        }

        private bool DetourW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            Console.WriteLine($"PostMessageW! Uint [{msg}]");
            return PostMessageW(hWnd, msg, wParam, lParam);
        }

        private bool DetourA(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            Console.WriteLine($"PostMessageA! proc! Uint [{msg}]");
            return PostMessageA(hWnd, msg, wParam, lParam);
        }

        private bool SendDetourW(IntPtr hWnd, int msg, int wParam, IntPtr lParam)
        {
            Console.WriteLine($"SendMessageW! Uint [{msg}]");
            return SendMessage(hWnd, msg, wParam, lParam);
        }

        private bool SendDetourA(IntPtr hWnd, int msg, int wParam, IntPtr lParam)
        {
            Console.WriteLine($"SendMessageA! proc! Uint [{msg}]");
            return SendMessageA(hWnd, msg, wParam, lParam);
        }

        #endregion Methods
    }
}