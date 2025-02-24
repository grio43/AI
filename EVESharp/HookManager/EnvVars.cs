/*
 * ---------------------------------------
 * User: duketwo
 * Date: 21.06.2014
 * Time: 17:20
 *
 * ---------------------------------------
 */

using HookManager.Win32Hooks;
using SharedComponents.EVE;
using SharedComponents.Utility;
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using EasyHook;

namespace HookManager
{
    public static class StringExtensions
    {
        #region Methods

        public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparisonType)
        {
            var startIndex = 0;
            while (true)
            {
                startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);
                if (startIndex == -1)
                    break;

                originalString = originalString.Substring(0, startIndex) + newValue + originalString.Substring(startIndex + oldValue.Length);

                startIndex += newValue.Length;
            }

            return originalString;
        }

        #endregion Methods
    }

    /// <summary>
    ///     Description of Enviroment.
    /// </summary>
    public class EnvVars
    {
        #region Methods

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibraryA(IntPtr lpModuleName);

        public static void PrintEnvVars()
        {
            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables()) HookManagerImpl.Log(env.Key + " " + env.Value);
        }

        // Delegate for the setenv function
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true)]
        private delegate int _putEnvDele(string s);


        private static void _putEnv(string s)
        {
            NativeAPI.LoadLibrary("api-ms-win-crt-environment-l1-1-0.dll");
            var procAddress = LocalHook.GetProcAddress("api-ms-win-crt-environment-l1-1-0.dll", "_putenv");
            //Debug.WriteLine($"_putenv procAddress: {procAddress}");
            
            if (procAddress == IntPtr.Zero)
                return;
            
            _putEnvDele dele = Marshal.GetDelegateForFunctionPointer<_putEnvDele>(procAddress);
            dele(s);
        }


        private static void PutEnv(string s)
        {
            _putenv(s);
            _putEnv(s);
        }


        public static void SetEnvironment(HWSettings settings)
        {
            var myUsernamePointer = getenv("USERNAME");
            var myUsername = Marshal.PtrToStringAnsi(myUsernamePointer);

            Util.CheckCreateDirectorys(settings.WindowsUserLogin);

            PutEnv("COMPUTERNAME=" + settings.Computername.ToUpper());
            PutEnv("USERDOMAIN=" + settings.Computername.ToUpper());
            PutEnv("USERDOMAIN_ROAMINGPROFILE=" + settings.Computername.ToUpper());
            PutEnv("USERNAME=" + settings.WindowsUserLogin);
            PutEnv(@"TMP=C:\Users\" + settings.WindowsUserLogin + @"\AppData\Local\Temp");
            PutEnv("VISUALSTUDIODIR=");

            if (settings.ProcessorIdent != null && settings.ProcessorIdent != null && settings.ProcessorCoreAmount != null && settings.ProcessorLevel != null)
            {
                PutEnv("PROCESSOR_IDENTIFIER=" + settings.ProcessorIdent);
                PutEnv("PROCESSOR_REVISION=" + settings.ProcessorRev);
                PutEnv("NUMBER_OF_PROCESSORS=" + settings.ProcessorCoreAmount);
                PutEnv("PROCESSOR_LEVEL=" + settings.ProcessorLevel);
            }

            PutEnv(@"USERPROFILE=C:\Users\" + settings.WindowsUserLogin);
            PutEnv(@"HOMEPATH=C:\Users\" + settings.WindowsUserLogin);
            PutEnv(@"LOCALAPPDATA=C:\Users\" + settings.WindowsUserLogin + @"\AppData\Local");
            PutEnv(@"TEMP=C:\Users\" + settings.WindowsUserLogin + @"\AppData\Local\Temp");
            PutEnv(@"APPDATA=C:\Users\" + settings.WindowsUserLogin + @"\AppData\Roaming");

            var pathPointer = getenv("PATH");
            var path = Marshal.PtrToStringAnsi(pathPointer);
            path = path.Replace(myUsername, settings.WindowsUserLogin, StringComparison.InvariantCultureIgnoreCase);
            PutEnv("PATH=" + path);
        }

        [DllImport("msvcr100.dll", SetLastError = true)]
        private static extern bool _putenv(string lpName);

        [DllImport("msvcr100.dll", SetLastError = true)]
        private static extern IntPtr getenv(string lpName);

        #endregion Methods
    }
}