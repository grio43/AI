/*
 * ---------------------------------------
 * User: duketwo
 * Date: 21.06.2014
 * Time: 17:05
 *
 * ---------------------------------------
 */

using EasyHook;
using SharedComponents.EVE;
using SharedComponents.IPC;
using SharedComponents.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace HookManager.Win32Hooks
{
    /// <summary>
    ///     Description of CryptHashDataController.
    /// </summary>
    public class CryptHashDataController : IDisposable, IHook
    {
        #region Fields

        //#TODO - FIXME - replace rcode hard coded list with a text file so that recompiles arent needed for rcodes
        //private static string[] arrayLogFile = File.ReadAllLines(LOGFILE_PATH);
        //private static List<string> RcodeWhitelist2 = new List<string>(arrayLogFile);

        public static List<string> RcodeWhitelist = new List<string>()
        {
            "8809f83932f3f583e6350b393b42cf7ea2a584aee25fd218cbb96e0f7ff579b5",
            "84fcf1999f51dd27615bc2139d0de47975c8bf0df4c687aada36e978e8f0547a",
            "00da28b9d0fdac376e4b5e2ddbbf21212fb4c7466b97abb35371ffa84f9e7443",
            "928c42c2b6982b6604d4f90ceb9f92815ef85580d402794310fb7e2dd5db54a7",
            "69187287ca4765b641aac0cc7f69e15a61aa71a27eb30ee59d6e0556396c60ce",
            "29f390b146732833f36d63ec639429afbed043954008e7bce7b55f3d154cd008",
            "48634103d8b6c1c8a212d1a5cf280f51aa277e85b446d5ae04c2123af56aae52",
            "c9fb57229353b118db15fa9a43e17aaa09b8b6372f50b8255ed4bd28ef2e1961",
            "51b5a1f7eef9657c31a4e4103717556dff649220e6a8a73bef6b36b3331f2a39",
            "ec7850513e2f672138e45773d1d20239da3e3c4affc29512bdf26611629b9963",
            "8efe6081cf4dbf94afb4fceea39bf8fb9cc832fd1ae0424cef63816b60e3820e",
            "02c202b51c8687d06f53246357c5b5c8d151ff101de1dddb35d91f5f3861057d",
            "82ce9bc108b337f708721be4b4f9abd7ed36b89370700b852125c28dc0a474e4",
            "84f5870683d50ddbc54832c61f5c3108fc260cf267000e6ea6d813c9783ef035",
            "34156f14bc38a7635b2e8af2c46beabf9b055fe94e58bc7adde03e6a629d1487", //28th May 2019 Invsation release
            "aea618420223be714297ceebbfa246d21984780eadea213094829d03e8365672",
            "93ebb3c99accca6adcdef63a4017df433ac2eedb13f790d6cec9291741342285",
            "a430192a4a9907a8dbd9e66329ab31983df836f0fedfcd3a20807595edc56e4e",
            "57f6ed25fb87cc8af97f039b11bb1147a4d81a2c9cf265236d6a182018331094",
            "a540c00f18f6b577bd8f01ac28acd0f998c8f4aaa60f7c90df982511f980afa5",
            "400d7f13ac959dd426fc3043f3736f2b0d060db35a2e7fd6f2099e025f1bf3b1",
            "04380156beabcf6d491cb5a420ace470a09014db13cfd5fec242d974bb10834e",
            "a76ca09ef10c6818183376df7bfdc08d134f79cd7161e85cf1e01437b90dfa4a",
            "63a89f9cdb994bbacac3f82024c6aec0d094e0f6fee8fc905e85330d3118cf48",
            "54ee45c280812f563fd045bcc7561aefb5a98c6c54c6cd352f1ef2cb775879c7",
            "516db61ab7c657af59410b2360f2d5618c8446a8c0e72e5ebe024222691bc0e2",
            "023f3bba2bd29f385d7d38777557c05cffed85f6bc5cd101188b4db65d1ecfbf",
            "b0557e49c51340d545edce778298a9eb6669e48f73a5e3939817afea6645dfa2",
            "1e35b16a667ad90de39d560103cc2851a502ef8be6429e75b37f46ffeff10cd4",
            "14b2013ef492156b591fc55c67785b266f506d779082c9607b7d202b74994930", // 04.05.2021, 2nd
            "ef6a69a07ff58ad543daaa6db17a8d7dbd075e26b1e356f81ce05bb94cb658c0",
            "8b2bfc0ea4280b22d1f567b5dfae20a21725a38b60b16a930576955f678ee259",
            "68d9200fc6ccf17a505088896dda7f2a9598dcd4506852945360b9e893129b65",
            "20473bc052ea1637402c0441a334ddac00d0d2b04cf3c4e7ccd4a3fb0a04f837",
            "5a52f51d572f9a0424e85f8212c83fcbb7e4cd11454eef01870f821b57a9b626",
            "31278e803de5fba5cc7b84d989b65d878d5fe5e4b51d171a1aa7b293e046f199",
            "c1d5ab0bf4f5ecf58c5ab71566d1a6459527abe79306a110985f13bd725865db", //11-10-2021
            "94cde1848b4abe6be445799c4d83e280bd5481c32c7b9b78ebac874d392e6c4f",
            "264ed2d4d0b766e9e043ec3d24620a1b0605b41acac386f19f4ed0a52b7223d1", //2-22-2021 SISI
            "2368e254fb0cf34adbc9c3a17a5167b48342f301015cd0f155420aabe2d074b1", //3-2022
            "59d70ae7969c035bce0e46db3dda3e00de0f077b01d1b5fa0bedb49c44d57373", //10-18-2022
            "d29954308fc0ad3de1f75ccb11b3773c3d1b4da225fd7de1ecc97cf1d18676e6", //10-2022
            "1576a4a05c986739f3c10cf0f3c947821352b84aa59192702757c97508916fb7", // sisi 20.10.22 overview columns
            "67d26734c03b2170d8396d83b178f6c9710662b958cee4d1235d11b58b1cd11a",
            "80dead70f89596676bc5ebf9c8151fcd152209d0388db1b0853fdecaeaa730fe",
            "86fabbfe294871949b511d16409f202190fe1404dd028b80950fa8ee111026ec",
            "03d80cf40e9ede2fdbff2c0c2424466eeed48d4635e061df97a5a6379114525f",
            "0dc6c1a7b20fac1e4746d4bbabc49152e625f29eacc3dba2ec527c821a1e5fd4", //11-25-2022 - sisi
            "201ce30d8d65bdfc1024a39e90f06ec8a606146d41b40a77ee502954ca7c2b48", // 1-24-2023
            "4ef575ee9d912471656dbc87d8c060d3a466a03c8e0126356e687740fcaafa87", // 3-2-2023 - sisi
            "9b7ce2c5d72a1b7540aaca4982f980d25185dfad5548be5456eea0e1c2302fc7", // 6-13-2023 - added only one line from previous rcode: varnames_11['volumetricQuality'] = varnames_3.Get(varnames_3.GFX_VOLUMETRIC_QUALITY)
        };

        public static ConcurrentStack<Tuple<string, string>> RemoteExecsStack = new ConcurrentStack<Tuple<string, string>>();
        private LocalHook _hook;

        private string _name;

        #endregion Fields

        #region Constructors

        public CryptHashDataController()
        {
            Name = typeof(CryptHashDataController).Name;

            try
            {
                _name = string.Format("CryptDataHash_{0:X}", LocalHook.GetProcAddress("advapi32.dll", "CryptHashData"));
                _hook = LocalHook.Create(LocalHook.GetProcAddress("advapi32.dll", "CryptHashData"), new CryptHashDataDelegate(CryptHashDataDetour), this);
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
        private delegate bool CryptHashDataDelegate(IntPtr hHash, IntPtr pbData, Int32 dwDataLen, uint dwFlags);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("advapi32.dll")]
        public static extern bool CryptHashData(IntPtr hHash, IntPtr pbData, Int32 dwDataLen, uint dwFlags);

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        private bool CryptHashDataDetour(IntPtr hHash, IntPtr pbData, Int32 dwDataLen, uint dwFlags)
        {
            var result = CryptHashData(hHash, pbData, dwDataLen, dwFlags);

            if (dwDataLen < 56)
                return result;

            var headerBytes = new byte[56];
            try
            {
                Marshal.Copy(pbData, headerBytes, 0, 56);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var headerHex = Util.ByteToHex(headerBytes);
            if (headerHex.Contains("7E0000000013FF"))
                try
                {
                    var tmp = new byte[dwDataLen];
                    Marshal.Copy(pbData, tmp, 0, dwDataLen);
                    var bytes = new byte[tmp.Length - 11];
                    Buffer.BlockCopy(tmp, 11, bytes, 0, bytes.Length);
                    var p = Marshal.AllocHGlobal(bytes.Length);
                    Marshal.Copy(bytes, 0, p, bytes.Length);
                    var file = Path.GetRandomFileName();
                    File.WriteAllBytes(file, bytes);
                    var t = RunDecompRcodeEx(file);
                    File.Delete(file);
                    if (t != null)
                    {
                        RemoteExecsStack.Push(t);
                        WriteRcode(t); // write rcode
                        if (!RcodeWhitelist.Contains(t.Item1))
                        {
                            // force quit
                            var msg = string.Format("[RCODE] received with UNKNOWN hash [{0}] Account [{1}]", t.Item1,
                                HookManagerImpl.Instance.EveAccount.AccountName);
                            ForceQuit(msg);
                        }
                        else
                        {
                            HookManagerImpl.Log(string.Format("[RCODE] received with known hash [{0}]", t.Item1), Color.LawnGreen);
                        }
                    }
                    else
                    {
                        // force quit
                        var msg = string.Format("[RCODE] error 1. Accountame [{0}]", HookManagerImpl.Instance.EveAccount.MaskedAccountName);
                        ForceQuit(msg);
                    }
                }
                catch (Exception e)
                {
                    // force quit
                    var msg = string.Format("[RCODE] error 2. Account [{0}]", HookManagerImpl.Instance.EveAccount.MaskedAccountName);
                    ForceQuit(msg, e);
                }
            return result;
        }

        public static void ForceQuit(string msg, Exception ex = null)
        {
            try
            {
                WCFClient.Instance.GetPipeProxy.RemoteLog(msg);
                if (ex != null)
                    WCFClient.Instance.GetPipeProxy.RemoteLog(ex.ToString());

                WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValue(WCFClient.Instance.GUID, nameof(EveAccount.UseScheduler), false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            HookManagerImpl.Log(msg, Color.Red);
            Environment.Exit(0);
            Environment.FailFast("");
        }

        public static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
        }

        public static Tuple<string, string> RunDecompRcodeEx(string file)
        {
            var process = new Process();
            var d = Path.Combine(Util.AssemblyPath.ToString(), "Resources\\DecompRcodeEx\\DecompRcodeEx.exe");
            process.StartInfo.FileName = d;
            process.StartInfo.Arguments = "-i " + file;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.Start();
            process.BeginErrorReadLine();
            var output = process.StandardOutput.ReadToEnd();
            output = Util.RemoveFirstLines(output, 1);
            process.WaitForExit();
            return new Tuple<string, string>(Util.Sha256(output), output);
        }

        public static void WriteRcode(Tuple<string, string> t)
        {
            var d = Path.Combine(Util.AssemblyPath.ToString(), "Resources\\DecompRcodeEx\\Rcodes");

            if (!Directory.Exists(d))
                Directory.CreateDirectory(d);

            var rCodeFilename = Path.Combine(d, t.Item1) + ".pyc";
            if (!File.Exists(rCodeFilename))
                File.WriteAllText(rCodeFilename, t.Item2);
        }

        #endregion Methods
    }
}