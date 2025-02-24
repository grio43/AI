using EasyHook;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using SharedComponents.EVE;
using SharedComponents.EveMarshal;
using SharedComponents.IPC;
using SharedComponents.Py;
using SharedComponents.Utility;
using SharedComponents.Utility.AsyncLogQueue;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Collections;
using System.Linq;

namespace HookManager.Win32Hooks
{
    public class SymmetricDecryptionController : IHook, IDisposable
    {
        #region Fields

        private LocalHook _hook;

        private Delegate _origiFunc;

        public static AsyncLogQueue AsyncLogQueue = AsyncLogQueue = new AsyncLogQueue();

        #endregion Fields

        #region Constructors

        public SymmetricDecryptionController(IntPtr funcAddr)
        {
            Error = false;
            Name = typeof(SymmetricDecryptionController).Name;

            try
            {
                _hook = LocalHook.Create(
                    funcAddr,
                    new Delegate(Detour),
                    this);

                _origiFunc = Marshal.GetDelegateForFunctionPointer<Delegate>(funcAddr);

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
        private delegate IntPtr Delegate(IntPtr self, IntPtr args);


        [DllImport("MemMan.dll")]
        public static extern void RecvPacket(IntPtr byteArrayPtr, int length);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }
        public string Name { get; set; }

        private long _packetCnt;

        #endregion Properties

        #region Methods
        private void Log(string s)
        {
            WCFClient.Instance.GetPipeProxy.RemoteLog(s);
        }

        public void Dispose()
        {
            _hook.Dispose();
        }

        private IntPtr Detour(IntPtr self, IntPtr args)
        {
            byte[] _recPacketBytes = null;
            //Log($"SymmetricDecryptionController Hook proc!");
            try
            {
                using (var pySharp = new PySharp(false))
                {
                    var argList = new PyObject(pySharp, args, false);
                    //HookManager.Log.RemoteWriteLine(argList.LogObject());

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var res = _origiFunc(self, args);

            try
            {
                using (var pySharp = new PySharp(false))
                {
                    var resultObj = new PyObject(pySharp, res, false);

                    var cbytes = resultObj.GetStringBytes();
                    if (cbytes[0] == 0x78)
                    {
                        cbytes = SharedComponents.EveMarshal.Zlib.Decompress(cbytes);
                    }

                    _recPacketBytes = cbytes;

                    if (cbytes[0] == 0x7E)
                    {

                        try
                        {
                            unsafe
                            {
                                fixed (byte* pointerToFirst = cbytes)
                                {
                                    RecvPacket((IntPtr)pointerToFirst, cbytes.Length);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                        _packetCnt++;
                        if (_packetCnt < 100 || AsyncLogQueue.IsSubscribed) // always capture the first 100 packets
                        {
                            var unMarshal = new Unmarshal();
                            var k = unMarshal.Process(cbytes, null);
                            var pp = new PrettyPrinter();
                            AsyncLogQueue.Enqueue(new LogEntry(pp.Print(k), $"Recv {cbytes.Length, 7}b", null));

                            //var unMarshal = new Unmarshal();
                            //var sw = Stopwatch.StartNew();
                            //var k = unMarshal.Process(cbytes);
                            //sw.Stop();
                            //var elapsedUnMarshal = sw.Elapsed.TotalMilliseconds;
                            //sw.Restart();
                            //var pretty = pp.Print(k);
                            //sw.Stop();
                            //var elapsedPretty = sw.Elapsed.TotalMilliseconds;
                            //AsyncLogQueue.Enqueue(new LogEntry($"elapsedUnMarshal [{elapsedUnMarshal}] elapsedPretty [{elapsedPretty}] CurrentTime [{DateTime.UtcNow:dd-MMM-yy HH:mm:ss:fff}] \n {pretty}", "Recv", null));
                        }
                    }
                    else
                    {
                        AsyncLogQueue.Enqueue(new LogEntry(resultObj.LogObject(), "PACKET_FAILURE", null));
                        HookManager.Log.RemoteWriteLine("Error: PACKET_FAILURE");
                        HookManager.Log.RemoteWriteLine(resultObj.LogObject());
                        //HookManagerImpl.Instance.ForceQuit("Error: PACKET_FAILURE");
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("RECV ERROR: " + e);
                Debug.WriteLine("RECV ERROR: " + e);


                //string hexString = string.Concat(_recPacketBytes.Select(b => b.ToString("X2")));
                //Console.WriteLine(hexString);
                //return res;

                //using (var pySharp = new PySharp(true))
                //{
                //    var blueMarshal = pySharp.Import("blue").Attribute("marshal");
                //    var stringTable = blueMarshal.Attribute("stringTableRev");
                //    var keywordArgs = new Dictionary<string, object>()
                //    {
                //        ["buffer"] = _recPacketBytes,
                //        ["callback"] = PySharp.PyNone,
                //        ["skipCrcCheck"] = true,
                //        ["offset"] = 0,
                //        ["stringTable"] = stringTable
                //    };
                //    var pyObjectMarshal = blueMarshal.CallWithKeywords("Load", keywordArgs);
                //    Console.WriteLine("pyObjectMarshal: " + pyObjectMarshal.IsValid);
                //    int n = 0;
                //    if (pyObjectMarshal.IsValid)
                //    {


                //        var tuple = pyObjectMarshal.ToList();
                //        foreach (var entry in tuple)
                //        {
                //            if (n == 0 && entry["payload"].IsValid)
                //            {

                //                keywordArgs = new Dictionary<string, object>()
                //                {
                //                    ["buffer"] = entry["payload"],
                //                    ["callback"] = PySharp.PyNone,
                //                    ["skipCrcCheck"] = true,
                //                    ["offset"] = 0,
                //                    ["stringTable"] = stringTable
                //                };
                //                var r = blueMarshal.CallWithKeywords("Load", keywordArgs);
                //                if (r.IsValid)
                //                {
                //                    //Console.WriteLine($"{r.LogObject()}");
                //                }
                //            }

                //            Console.WriteLine($" [{n}] ------------------------------------------------------------------------------");
                //            //Console.WriteLine(entry.LogObject());
                //            n++;
                //        }
                //    }

                //Console.WriteLine($"{pyObjectMarshal.LogObject()}");
                //}
            }
            return res;

        }
        #endregion Methods
    }
}