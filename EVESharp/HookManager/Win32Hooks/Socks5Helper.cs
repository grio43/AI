using SharedComponents.WinApiUtil;
//using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static HookManager.Win32Hooks.WSAIoctlController;

namespace HookManager.Win32Hooks
{
    internal class Socks5Helper
    {


        [DllImport("wsock32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int WSAGetLastError();

        [DllImport("wsock32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void WSASetLastError(int set);

        public enum WSockMethod
        {
            WSA,
            LEGACY
        }

        private static bool SendBytes(IntPtr s, byte[] data, WSockMethod method = WSockMethod.WSA, int timeout = 1500)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var t = DateTime.UtcNow.AddMilliseconds(timeout);
                var result = -1;
                while (result == -1)
                {
                    WSABuffer[] buffers = new WSABuffer[1];
                    buffers[0].Length = data.Length;
                    buffers[0].Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);

                    IntPtr overlapped = IntPtr.Zero;
                    int bytesSent = 0;
                    SocketFlags flags = SocketFlags.None;

                    if (method == WSockMethod.WSA)
                        result = WSASend(s, buffers, 1, out bytesSent, flags, overlapped, IntPtr.Zero);
                    if (method == WSockMethod.LEGACY)
                        result = send(s, buffers[0].Pointer, data.Length, 0);

                    var errorcode = WSAGetLastError();

                    WriteTrace($"Socks5 SendBytes result: [{result}] Errorcode: [{errorcode}]");

                    if (errorcode != WSAENOTCONN && errorcode != WSANOERROR)
                    {
                        var msg = "Socks5 SendBytes failed, Error: " + errorcode;
                        WriteTrace(msg);
                        HookManagerImpl.Log(msg);
                        return false;
                    }

                    if (t < DateTime.UtcNow)
                    {
                        var msg = "Socks5 SendBytes WARNING: Timeout reached. Error. Returning false.";
                        WriteTrace(msg);
                        HookManagerImpl.Log(msg);
                        return false;
                    }

                    WriteTrace($"Socks5 SendBytes result (NumBytes): [{bytesSent}] Errorcode: [{errorcode}]");
                    SpinWait.SpinUntil(() => false, 1); // You might want to replace this with a better synchronization mechanism
                }
            }
            catch (Exception ex)
            {
                WriteTrace("Socks5 SendBytes Exception: " + ex.Message);
                return false;
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
            return true;
        }

        private static byte[] ReceiveBytes(IntPtr s, int length, WSockMethod method = WSockMethod.WSA, int timeout = 1500)
        {
            var mem = IntPtr.Zero;
            try
            {
                // retrieve server response
                var response = IntPtr.Zero;
                var t = DateTime.UtcNow.AddMilliseconds(_socketTimeout);
                mem = Marshal.AllocHGlobal(length);
                while (response == IntPtr.Zero)
                {
                    if (method == WSockMethod.WSA)
                        response = WSAReceive(s, length, mem);

                    if (method == WSockMethod.LEGACY)
                    {
                        var r = recv(s, mem, length, 0);
                        if (r == -1)
                            response = IntPtr.Zero;
                        else
                            response = mem;
                    }

                    var errorcode = WSAGetLastError();

                    if (errorcode != WSAEWOULDBLOCK && errorcode != WSANOERROR)
                    {
                        HookManagerImpl.Log("Socks5 ReceiveBytes FAILED response == IntPtr.Zero! Lasterror: " +
                                            errorcode.ToString());
                        WriteTrace("Socks5 ReceiveBytes FAILED response == IntPtr.Zero! Lasterror: " +
                                   errorcode.ToString());
                        return null;
                    }

                    if (t < DateTime.UtcNow)
                    {
                        var msg = "Socks5 ReceiveBytes WARNING: Timeout reached. Error. Returning null.";
                        WriteTrace(msg);
                        HookManagerImpl.Log(msg);
                        return null;
                    }

                    SpinWait.SpinUntil(() => false, 1);
                }
                // Parse the reponse into an byte array and return
                var recvBytes = new byte[length];
                Marshal.Copy(response, recvBytes, 0, length);
                return recvBytes;
            }
            catch (Exception ex)
            {
                WriteTrace("Socks5 ReceiveBytes Exception: " + ex.ToString());
                return null;
            }
            finally
            {
                if (mem != IntPtr.Zero)
                    Marshal.FreeHGlobal(mem);
            }
        }

        // Timeout in milliseconds
        private static int _socketTimeout = 1500;

        public static bool InitSocks5TunnelOnNativeSocket(IntPtr s, string remoteIp, ushort remotePort, string proxyUser, string proxyPass, WSockMethod method = WSockMethod.LEGACY)
        {
            try
            {
                // Send socks 5 request
                byte[] socks5Request = SetUpSocks5RequestByteArray();

                if (!SendBytes(s, socks5Request, method))
                    return false;

                // Retrieve server response
                var recvBytes = ReceiveBytes(s, 2, method);
                if (recvBytes == null)
                    return false;

                if (recvBytes[1] == 255)
                {
                    WriteTrace("Socks5 No authentication method was accepted by the proxy server");
                    return false;
                }

                if (recvBytes[0] != 5)
                {
                    WriteTrace("Socks5 No SOCKS5 proxy");
                    return false;
                }

                // If auth request response, send authenicate request
                if (recvBytes[1] == 2)
                {
                    int length;
                    byte[] authenticateRequest = SetUpAuthenticateRequestByteArray(proxyUser, proxyPass, out length);
                    if (!SendBytes(s, authenticateRequest, method))
                        return false;

                    recvBytes = ReceiveBytes(s, 2, method);
                    if (recvBytes == null)
                        return false;

                    if (recvBytes[1] != 0)
                    {
                        WriteTrace("Socks5 Proxy: incorrect username/password");
                        return false;
                    }
                }

                // Request connect with server
                WriteTrace($"SetUpConnectWithRemoteHostByteArray. RemoteIP [{remoteIp}] RemotePort [{remotePort}]");
                var bindRequest = SetUpConnectWithRemoteHostByteArray(remoteIp, remotePort);
                if (!SendBytes(s, bindRequest, method))
                    return false;

                // Connect response
                recvBytes = ReceiveBytes(s, 10, method);
                if (recvBytes == null)
                    return false;

                if (!VerifyBindResponse(recvBytes))
                {
                    HookManagerImpl.Log("Socks5 VerifyBindResponse failed!");
                    WriteTrace("Socks5 VerifyBindResponse failed!");
                    return false;
                }
                else
                {
                    WriteTrace("Socks5 VerifyBindResponse succeeded!");
                }

                HookManagerImpl.Log("Socks5R [REMOTE_IP] " + remoteIp + " [REMOTE_PORT] " + remotePort.ToString());
                WriteTrace("Socks5R [REMOTE_IP] " + remoteIp + " [REMOTE_PORT] " + remotePort.ToString());
                // Success
                WSASetLastError(0);
                SetLastError(0);
                return true;

            }
            catch (Exception ex)
            {
                WriteTrace("Socks5 Exception: " + ex.ToString());
                HookManagerImpl.Log("Socks5 Exception: " + ex.ToString());
                return false;
            }
        }

        // Method to check if the native socket is connected and valid
        public static bool IsNativeSocketValid(IntPtr nativeSocketHandle)
        {
            try
            {
                // Check if the handle is invalid or closed
                if (nativeSocketHandle == IntPtr.Zero || !IsSocketConnected(nativeSocketHandle))
                {
                    return false;
                }

                // If the handle is not invalid and the connection is active, consider it valid
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Method to check if the socket is connected
        public static bool IsSocketConnected(IntPtr socketHandle)
        {
            const int SOL_SOCKET = 0x0;
            const int SO_ERROR = 0x1007;

            int errorcode = 0;
            int errLen = Marshal.SizeOf(errorcode);

            // Check if the socket has any pending error
            int result = getsockopt(socketHandle, SOL_SOCKET, SO_ERROR, ref errorcode, ref errLen);

            if (result != 0 || errorcode != 0)
            {
                return false;
            }

            return true;
        }

        // Native method for getting socket options
        [DllImport("ws2_32.dll", SetLastError = true)]
        static extern int getsockopt(IntPtr socket, int level, int optname, ref int optval, ref int optlen);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void SetLastError(int errorCode);


        [DllImport("Ws2_32.dll")]
        public static extern int recv(IntPtr s, IntPtr buf, int len, int flags);

        [DllImport("Ws2_32.dll")]
        public static extern int send(IntPtr s, IntPtr buf, int len, int flags);

        [DllImport("ws2_32.dll", SetLastError = true)]


        internal static extern int WSASend(
             IntPtr socketHandle,
             WSABuffer[] buffers,
             int bufferCount,
             out int bytesSent,
             SocketFlags socketFlags,
             IntPtr overlapped,
             IntPtr completionRoutine);

        [DllImport("ws2_32.dll", SetLastError = true)]
        internal static extern int WSARecv(
            IntPtr socketHandle,
            WSABuffer[] buffers,
            int bufferCount,
            out int bytesTransferred,
            ref SocketFlags socketFlags,
            IntPtr overlapped,
            IntPtr completionRoutine);


        private static IntPtr WSAReceive(IntPtr socket, int bufferSize, IntPtr allocatedMemory)
        {
            WSABuffer[] buffers = new WSABuffer[1];
            buffers[0].Length = bufferSize;
            buffers[0].Pointer = allocatedMemory;

            IntPtr overlapped = IntPtr.Zero;
            int bytesReceived = 0;
            SocketFlags flags = SocketFlags.None;

            int result = WSARecv(socket, buffers, 1, out bytesReceived, ref flags, overlapped, IntPtr.Zero);
            var errorcode = WSAGetLastError();

            if (result == (int)SocketError.SocketError)
            {
                if (errorcode != WSAEWOULDBLOCK && errorcode != WSANOERROR)
                {
                    HookManagerImpl.Log("Socks5 WSARecv FAILED response == IntPtr.Zero! Lasterror: " +
                                        errorcode.ToString());
                    return IntPtr.Zero;
                }
            }

            return (bytesReceived > 0) ? allocatedMemory : IntPtr.Zero;
        }

        private static bool VerifyBindResponse(byte[] recvBytes)
        {

            if (recvBytes.Length != 10)
            {
                WriteTrace("VerifyBindResponse: Invalid response length");
                return false;
            }

            //var recvBytes = new byte[10];
            //PyMarshal.Copy(buffer, recvBytes, 0, recvBytes.Length);

            // Log information about each byte in the received response
            string hexSequence = BitConverter.ToString(recvBytes).Replace("-", " ");
            HookManagerImpl.Log($"VerifyBindResponse Hex Sequence: {hexSequence}");
            WriteTrace($"VerifyBindResponse Hex Sequence: {hexSequence}");

            if (recvBytes[0] != 5)
            {
                HookManagerImpl.Log("Proxy: Invalid SOCKS version");
                return false;
            }

            // Check the second byte for specific error codes
            if (recvBytes[1] != 0)
            {
                if (recvBytes[1] >= 1 && recvBytes[1] <= 8)
                {
                    string errorMessage = string.Empty;
                    switch (recvBytes[1])
                    {

                        case 1:
                            errorMessage = "General failure";
                            break;
                        case 2:
                            errorMessage = "Connection not allowed by ruleset";
                            break;
                        case 3:
                            errorMessage = "Network unreachable";
                            break;
                        case 4:
                            errorMessage = "Host unreachable";
                            break;
                        case 5:
                            errorMessage = "Connection refused by destination host";
                            break;
                        case 6:
                            errorMessage = "TTL expired";
                            break;
                        case 7:
                            errorMessage = "Command not supported / Protocol error";
                            break;
                        case 8:
                            errorMessage = "Address type not supported";
                            break;
                        default:
                            errorMessage = $"Unknown error code: {recvBytes[1]}";
                            break;
                    }

                    HookManagerImpl.Log(errorMessage);
                    HookManagerImpl.Log("Proxy: Connection error binding eve server");
                    WriteTrace(errorMessage);
                    return false;
                }
                else
                {
                    HookManagerImpl.Log($"Unknown error code: {recvBytes[1]}");
                    HookManagerImpl.Log("Proxy: Connection error binding eve server");
                    WriteTrace($"Unknown error code: {recvBytes[1]}");
                    return false;
                }
            }

            // The request was successful, so process the address information if available
            var addressType = recvBytes[3];
            if (addressType == 1) // IPv4 address
            {
                byte[] ipv4Bytes = new byte[4];
                Array.Copy(recvBytes, 4, ipv4Bytes, 0, 4);
                string ipv4Address = new IPAddress(ipv4Bytes).ToString();
                HookManagerImpl.Log($"IPv4 Address: {ipv4Address}");
                WriteTrace($"(Server ext.) IPv4 Address: {ipv4Address}");
            }
            else if (addressType == 3) // Domain name
            {
                int domainLength = recvBytes[4];
                byte[] domainBytes = new byte[domainLength];
                Array.Copy(recvBytes, 5, domainBytes, 0, domainLength);
                string domainName = Encoding.ASCII.GetString(domainBytes);
                HookManagerImpl.Log($"Domain Name: {domainName}");
                WriteTrace($"Domain Name: {domainName}");
            }
            else if (addressType == 4) // IPv6 address
            {
                byte[] ipv6Bytes = new byte[16];
                Array.Copy(recvBytes, 4, ipv6Bytes, 0, 16);
                string ipv6Address = new IPAddress(ipv6Bytes).ToString();
                HookManagerImpl.Log($"IPv6 Address: {ipv6Address}");
                WriteTrace($"(Server ext.) IPv6 Address: {ipv6Address}");
            }
            else
            {
                HookManagerImpl.Log($"Unsupported address type: {addressType}");
                WriteTrace($"Unsupported address type: {addressType}");
                return false;
            }
            return true;
        }

        private static IntPtr CreateAddr(string ip, string port)
        {
            var s = Marshal.AllocHGlobal(16);
            var sockAddr = new sockaddr_in();
            sockAddr.sin_addr.S_addr = inet_addr(ip);
            sockAddr.sin_port = htons(Convert.ToUInt16(port));
            sockAddr.sin_family = 2;
            Marshal.StructureToPtr(sockAddr, s, true);
            return s;
        }

        private static IntPtr CreateAddrIPv6(string ip, string port)
        {
            var s = Marshal.AllocHGlobal(Marshal.SizeOf<sockaddr_in6>());
            var sockAddr = new sockaddr_in6
            {
                sin6_family = 23, // AF_INET6 value
                sin6_port = htonsx(Convert.ToUInt16(port)),
                sin6_flowinfo = 0, // You might adjust this according to your requirements
                sin6_scope_id = 0  // Scope ID (interface index)
            };

            // Convert IPv6 address string to byte array
            byte[] ipv6Bytes = IPAddress.Parse(ip).GetAddressBytes();
            sockAddr.sin6_addr.u6_addr8 = ipv6Bytes;

            Marshal.StructureToPtr(sockAddr, s, true);
            return s;
        }

        // Helper method to convert port to network byte order (big-endian)
        private static ushort htonsx(ushort value)
        {
            return BitConverter.IsLittleEndian ? (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8) : value;
        }

        private static void SetAddr(IntPtr addr, string ip, string port)
        {
            var structure = (sockaddr_in)Marshal.PtrToStructure(addr, typeof(sockaddr_in));
            var originalip = new IPAddress(structure.sin_addr.S_addr).ToString();
            var originalport = ntohs(structure.sin_port);

            structure.sin_addr.S_addr = inet_addr(ip);
            structure.sin_port = htons(Convert.ToUInt16(port));
            structure.sin_family = 2;
            Marshal.StructureToPtr(structure, addr, true);
            structure = (sockaddr_in)Marshal.PtrToStructure(addr, typeof(sockaddr_in));
        }


        private static byte[] SetUpAuthenticateRequestByteArray(string username, string password, out int index)
        {
            index = 0;
            int size = 3 + Encoding.Default.GetByteCount(username) + Encoding.Default.GetByteCount(password);
            byte[] authenticateBuffer = new byte[size];

            authenticateBuffer[index++] = 1;
            authenticateBuffer[index++] = (byte)username.Length;

            if (username.Length > 0)
            {
                byte[] rawUsername = Encoding.Default.GetBytes(username);
                Array.Copy(rawUsername, 0, authenticateBuffer, index, rawUsername.Length);
                index += rawUsername.Length;
            }

            authenticateBuffer[index++] = (byte)password.Length;

            if (password.Length > 0)
            {
                byte[] rawPassword = Encoding.Default.GetBytes(password);
                Array.Copy(rawPassword, 0, authenticateBuffer, index, rawPassword.Length);
                index += rawPassword.Length;
            }

            return authenticateBuffer;
        }

        private static byte[] SetUpConnectWithRemoteHostByteArray(string eveIP, ushort evePort)
        {
            byte[] bindWithEveBuffer = new byte[10];
            var iplist = eveIP.Split('.').Select(byte.Parse).ToArray();
            var portbyte = BitConverter.GetBytes(evePort).Reverse().ToArray();

            bindWithEveBuffer[0] = 5; // Version
            bindWithEveBuffer[1] = 1; // Command (Bind)
            bindWithEveBuffer[2] = 0; // Reserved byte
            bindWithEveBuffer[3] = 1; // Address type (IPv4)

            Array.Copy(iplist, 0, bindWithEveBuffer, 4, 4); // Copy IP bytes

            // Copy port bytes
            Array.Copy(portbyte, 0, bindWithEveBuffer, 8, 2);

            // Convert the bytes to a hexadecimal string (if needed)
            string hexString = BitConverter.ToString(bindWithEveBuffer).Replace("-", " ");

            // Print the hexadecimal string (if needed)
            WriteTrace($"SetUpBindWithRemoteHost Hex: {hexString}");

            return bindWithEveBuffer;
        }

        private static byte[] SetUpSocks5RequestByteArray()
        {
            byte[] initialRequest = new byte[4];

            initialRequest[0] = 5;  // Version
            initialRequest[1] = 2;  // Number of authentication methods supported
            initialRequest[2] = 0;  // No authentication
            initialRequest[3] = 2;  // Username/password authentication

            return initialRequest;
        }

        private static String sockAddrInToString(sockaddr_in sin)
        {
            var family = sin.sin_family.ToString();
            var remoteIp = new IPAddress(sin.sin_addr.S_addr).ToString();
            var remotePort = ntohs(sin.sin_port).ToString();
            var w1 = sin.sin_addr.S_un_w.s_w1.ToString();
            var w2 = sin.sin_addr.S_un_w.s_w2.ToString();
            var b1 = sin.sin_addr.S_un_b.s_b1.ToString();
            var b2 = sin.sin_addr.S_un_b.s_b2.ToString();
            var b3 = sin.sin_addr.S_un_b.s_b3.ToString();
            var b4 = sin.sin_addr.S_un_b.s_b4.ToString();
            return "Family: " + family + " Remote Ip: " + remoteIp + " Remote Port: " + remotePort +
                   " w1: " + w1 + " w2: " + w2 + " b1: " + b1 + " b2: " + b2 + " b3: " + b3 + " b4: " + b4; // w,b == zero padding
        }



        private static bool _showDebugLogs = true;
        private static void WriteTrace(string msg)
        {
            if (_showDebugLogs)
            {
                Trace.WriteLine(msg);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OVERLAPPED
        {
            public UIntPtr Internal;
            public UIntPtr InternalHigh;
            public uint Offset;
            public uint OffsetHigh;
            public IntPtr hEvent;
        }
        public static IPAddress ParseIPv4FromIPv6(string ipv6MappedAddress)
        {
            IPAddress ipAddress = IPAddress.Parse(ipv6MappedAddress);

            if (ipAddress.IsIPv4MappedToIPv6)
            {
                IPAddress ipv4Address = ipAddress.MapToIPv4();
                return ipv4Address;
            }

            // If not an IPv4-mapped IPv6 address, return null or throw an exception as needed
            return null; // or throw new ArgumentException("Not an IPv4-mapped IPv6 address");
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SOCKADDR
        {
            public ushort sin_family;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WSABuffer
        {
            public int Length;
            public IntPtr Pointer;
        }
    }
}
