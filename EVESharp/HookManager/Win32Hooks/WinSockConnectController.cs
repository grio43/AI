using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharedComponents.Extensions;

namespace HookManager.Win32Hooks
{
    // TODO: transport.address // https://docs.python.org/2/library/socket.html // getpeername // SocketGPS.py

    public class WinSockConnectController : IDisposable, IHook
    {
        #region Fields

        private LocalHook _hook;

        private string _name;

        private string proxyIp, proxyPort, proxyUser, proxyPass;


        #endregion Fields

        #region Constructors

        public WinSockConnectController(IntPtr address, string proxyIp, string proxyPort, string proxyUser, string proxyPass)
        {


            Name = typeof(WinSockConnectController).Name;

            this.proxyIp = proxyIp;
            this.proxyPort = proxyPort;
            this.proxyUser = proxyUser;
            this.proxyPass = proxyPass;

            _checkDisableSocketsTask = new Task(() =>
            {
                while (true)
                {
                    if (Program.DisableWinsockConnectionsSharedArray[0] == true)
                    {
                        CloseAllUsedSockets();
                    }
                    Thread.Sleep(1000);
                }
            });
            _checkDisableSocketsTask.Start();

            try
            {
                //_name = string.Format("WinsockHook_{0:X}", address.ToInt32());
                _name = string.Format("WinsockHook_{0:X}", address.ToInt64());
                _hook = LocalHook.Create(address, new WinsockConnectDelegate(WinsockConnectDetour), this);
                _hook.ThreadACL.SetExclusiveACL(new Int32[] { });
            }
            catch (Exception)
            {
                Error = true;
            }
        }

        #endregion Constructors

        #region Delegates

        private delegate int WinsockConnectDelegate(IntPtr s, IntPtr addr, int addrsize);

        #endregion Delegates

        #region Properties

        public bool Error { get; set; }

        public string Name { get; set; }

        #endregion Properties

        #region Methods

        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int closesocket(IntPtr s);

        [DllImport("WS2_32.dll")]
        public static extern int connect(IntPtr s, IntPtr addr, int addrsize);

        [DllImport("Ws2_32.dll")]
        public static extern ushort htons(ushort hostshort);

        [DllImport("Ws2_32.dll", CharSet = CharSet.Ansi)]
        public static extern uint inet_addr(string cp);

        [DllImport("Ws2_32.dll")]
        public static extern ushort ntohs(ushort netshort);
        [DllImport("Ws2_32.dll")]
        public static extern int send(IntPtr s, IntPtr buf, int len, int flags);

        public void Dispose()
        {
            if (_hook == null)
                return;

            _hook.Dispose();
            _hook = null;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void SetLastError(int errorCode);

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WSAGetLastError();

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void WSASetLastError(int set);

        private void SetAddr(IntPtr addr, string ip, string port)
        {
            var structure = (sockaddr_in)Marshal.PtrToStructure(addr, typeof(sockaddr_in));
            var originalip = new IPAddress(structure.sin_addr.S_addr).ToString();
            var originalport = ntohs(structure.sin_port);

            structure.sin_addr.S_addr = inet_addr(ip);
            structure.sin_port = htons(Convert.ToUInt16(port));
            Marshal.StructureToPtr(structure, addr, true);
            structure = (sockaddr_in)Marshal.PtrToStructure(addr, typeof(sockaddr_in));
        }

        private bool _showDebugLogs = true;
        private void WriteTrace(string msg)
        {
            if (_showDebugLogs)
            {
                Trace.WriteLine(msg);
            }
        }

        private List<IntPtr> _usedSockets = new List<IntPtr>();

        private void CloseAllUsedSockets()
        {
            Log.RemoteWriteLine("Warn: CloseAllUsedSockets");
            foreach (var socket in _usedSockets.ToList())
            {
                closesocket(socket);
                _usedSockets.Remove(socket);
            }
        }

        private static Task _checkDisableSocketsTask = null;




        private static bool IsPortInUse(int port)
                {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] activeTcpListeners = ipGlobalProperties.GetActiveTcpListeners();

            var result = false;
            foreach (IPEndPoint endPoint in activeTcpListeners)
                        {
                //Debug.WriteLine(endPoint.Port);
                if (endPoint.Port == port)
                                {
                    result = true;
                                }
                                }

            return result;
                        }


        private static bool IsPortInUse(string host, int port)
        {
                        try
                        {
                using (var client = new TcpClient(host, port))
                            {
                    // Port is in use and connection was successful
                    return true;
                                }
                                }
            catch (SocketException)
                        {
                // Port is not in use or connection failed
                return false;
                        }
                        }

        private int WinsockConnectDetour(IntPtr s, IntPtr sockAddr, int addrsize)
                        {
            WriteTrace($"WinsockConnectDetour called. Socket Handle [{s.ToString("X")}]");

            if (sockAddr == IntPtr.Zero || addrsize == 0)
                        {
                WriteTrace($"WinsockConnectDetour called. Socket Handle [{s.ToString("X")}] sockAddr == IntPtr.Zero || addrsize == 0");
                                    return -1;
                                }

                            try
                            {
                if (!_usedSockets.Contains(s))
                                {
                    _usedSockets.Add(s);
                                }
                            }
                            catch (Exception ex)
                            {
                WriteTrace("Exception in WinsockConnectDetour: " + ex.Message);
                                return -1;
                            }

            if (Program.DisableWinsockConnectionsSharedArray[0] == true)
                            {
                WriteTrace("Program.DisableWinsockConnectionsSharedArray[0] == true");
                                return -1;
                            }

            var allocatedMemory = new List<IntPtr>();
                        try
                        {
                //WriteTrace($"WS2 (ConAttempt) (1)");
                // retrieve remote ip
                var structure = (sockaddr_in)Marshal.PtrToStructure(sockAddr, typeof(sockaddr_in));
                //var remoteIp = GetDestIp(new IPAddress(structure.sin_addr.S_addr).ToString());
                var remoteIp = new IPAddress(structure.sin_addr.S_addr).ToString();
                var remotePort = ntohs(structure.sin_port);

                //WriteTrace($"WS2 (ConAttempt) Remote IP: {remoteIp} Remote Port: {remotePort} Handle [{s.ToString("X")}]");

                if (!remoteIp.Contains("127.0.0.1"))
                        {
                    //allocatedMemory.Add(test);
                    SetAddr(sockAddr, proxyIp, proxyPort.ToString());
                    var result = connect(s, sockAddr, addrsize);
                    Socks5Helper.InitSocks5TunnelOnNativeSocket(s, remoteIp, remotePort, proxyUser, proxyPass);
                    HookManagerImpl.Log("WS2 [REMOTE_IP] " + remoteIp + " [REMOTE_PORT] " + remotePort.ToString());
                    WriteTrace("WS2 [REMOTE_IP] " + remoteIp + " [REMOTE_PORT] " + remotePort.ToString());


                    if (!SocketInfo.SocketsInfoDictionary.ContainsKey(s))
                    {
                        var ep = WSAIoctlController.GetLocalEndPoint(s);
                        if (ep != null)
                        {
                            var socketInfo = new SocketInfo(
                                ep.Address,
                                ep.Port,
                                IPAddress.Parse(proxyIp),
                                int.Parse(proxyPort),
                                IPAddress.Parse(remoteIp),
                                remotePort,
                                string.Empty,
                                s
                            );
                            SocketInfo.SocketsInfoDictionary.AddOrUpdate(s, socketInfo);
                        }
                                }

                        // success
                        WSASetLastError(0);
                        SetLastError(0);

                        return 0;
                    }
                else
                {

                    if (IsPortInUse(remotePort))
                    {
                        //HookManagerImpl.Log("WS2IU [REMOTE_IP] " + remoteIp + " [REMOTE_PORT] " +
                        //                    remotePort.ToString());
                        //WriteTrace("WS2IU [REMOTE_IP] " + remoteIp + " [REMOTE_PORT] " + remotePort.ToString());

                        var ret = connect(s, sockAddr, addrsize);
                        return ret;
                }
                    WSASetLastError(10061);
                    SetLastError(0);
                    return -1;
                        }
                    }
            catch (Exception e)
                    {
                WriteTrace(e.ToString());
            }
            finally
            {
                // clean memory
                try
                {
                    foreach (var ptr in allocatedMemory)
                        Marshal.FreeHGlobal(ptr);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    WriteTrace(e.ToString());
                }

            }
            return -1;
        }

        #endregion Methods

        #region Structs

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        public struct sockaddr_in
        {
            public const int Size = 16;

            public short sin_family;
            public ushort sin_port;

            public struct in_addr
            {
                public uint S_addr;

                public struct _S_un_b
                {
                    public byte s_b1, s_b2, s_b3, s_b4;
                }

                public _S_un_b S_un_b;

                public struct _S_un_w
                {
                    public ushort s_w1, s_w2;
                }

                public _S_un_w S_un_w;
            }

            public in_addr sin_addr;
        }

        #endregion Structs
    }
}