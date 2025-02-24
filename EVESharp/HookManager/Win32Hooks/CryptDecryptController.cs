/*
 * ---------------------------------------
 * User: duketwo
 * Date: 19.05.2018
 * Time: 14:20
 *
 * ---------------------------------------
 */

using EasyHook;
using SharedComponents.Py;
using SharedComponents.Utility;
using System;
using System.Runtime.InteropServices;
using System.Xml.Serialization.Advanced;
using SharedComponents.EveMarshal;
using SharedComponents.Utility.AsyncLogQueue;
using Zlib = SharedComponents.EveMarshal.Zlib;

namespace HookManager.Win32Hooks
{
    //BOOL WINAPI CryptDecrypt(
    //_In_ HCRYPTKEY  hKey,
    //_In_ HCRYPTHASH hHash,
    //_In_ BOOL       Final,
    //_In_ DWORD      dwFlags,
    //_Inout_ BYTE       *pbData,
    //_Inout_ DWORD      *pdwDataLen
    //);

    /// <summary>
    ///     Description of CryptDecryptController.
    /// </summary>
    public class CryptDecryptController : IDisposable, IHook
    {
        #region Fields

        public const byte ZlibHeader = 0x78;

        private LocalHook _hook;

        private string _name;

        public static AsyncLogQueue AsyncLogQueue = AsyncLogQueue = new AsyncLogQueue();

        #endregion Fields

        #region Constructors

        public CryptDecryptController()
        {
            Name = typeof(CryptDecryptController).Name;
            try
            {
                _name = string.Format("CryptDecrypt{0:X}", LocalHook.GetProcAddress("advapi32.dll", "CryptDecrypt"));
                _hook = LocalHook.Create(LocalHook.GetProcAddress("advapi32.dll", "CryptDecrypt"), new CryptDecryptDelegate(CryptDecryptDetour), this);
                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate bool CryptDecryptDelegate(IntPtr hKey, IntPtr hHash, int Final, uint dwFlags, [In][Out] IntPtr pbData, [In][Out] IntPtr pdwDataLen);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(UnmanagedType.Bool)]
        public static extern bool CryptDecrypt(IntPtr hKey, IntPtr hHash, int Final, uint dwFlags, [In][Out] IntPtr pbData, [In][Out] IntPtr pdwDataLen);

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        private bool CryptDecryptDetour(IntPtr hKey, IntPtr hHash, int Final, uint dwFlags, [In][Out] IntPtr pbData, [In][Out] IntPtr pdwDataLen)
        {
            var result = CryptDecrypt(hKey, hHash, Final, dwFlags, pbData, pdwDataLen);
            var size = Marshal.ReadInt32(pdwDataLen);
            try
            {
                if (result && size != 0 && pbData != IntPtr.Zero && Final == 1)
                {

                    if (AsyncLogQueue.IsSubscribed)
                    {
                        //var sizeKB = size / 1024;
                        //if (sizeKB > 500)
                        //    HookManagerImpl.Log($" Size {sizeKB} kb.");
                        var cbytes = new byte[size];
                        Marshal.Copy(pbData, cbytes, 0, size);
                        if (cbytes[0] == ZlibHeader)
                        {
                            //Util.MeasureTime(() =>
                            //{
                            cbytes = Zlib.Decompress(cbytes);
                            //});
                        }
                        if (cbytes[0] == 0x7E)
                        {
                            var unMarshal = new Unmarshal();
                            var k = unMarshal.Process(cbytes, null);


                            var pp = new PrettyPrinter();
                            AsyncLogQueue.Enqueue(new LogEntry(pp.Print(k), "Recv", null));
                            //HookManagerImpl.Log(k.Type.ToString());
                            //Console.WriteLine(PrettyPrinter.Print(k));
                        }
                    }

                    //using (var pySharp = new PySharp(false))
                    //{
                    //    var services = p.__builtin__.sm.services;
                    //    if (services.IsValid)
                    //    {
                    //        var serviceDict = ((PyObject)services).ToDictionary<string>();
                    //        if (serviceDict.ContainsKey("machoNet") && serviceDict["machoNet"].Attribute("state").ToInt() == 4)
                    //        {
                    //            HookManagerImpl.Log($"Packet received. Size {size / 1024d} kb.");
                    //            var cbytes = new byte[size];
                    //            Marshal.Copy(pbData, cbytes, 0, size);
                    //            if (cbytes[0] == ZlibHeader)
                    //            {
                    //                cbytes = Zlib.Decompress(cbytes);
                    //            }

                    //var blueMarshal = pySharp.Import("blue").Attribute("marshal");
                    //if (blueMarshal.IsValid)
                    //{
                    //    var packet = blueMarshal.Call("Load", cbytes);
                    //    if (packet.IsValid)
                    //    {
                    //        HookManagerImpl.Log(packet.LogObject());
                    //    }
                    //}
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
            }
            return result;
        }

        #endregion Methods
    }
}