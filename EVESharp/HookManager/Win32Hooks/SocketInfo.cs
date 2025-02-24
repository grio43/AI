using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using SharedComponents.Utility;
using SHA256 = System.Security.Cryptography.SHA256;

namespace HookManager.Win32Hooks
{
    class SocketInfo
    {

        public static ConcurrentDictionary<IntPtr,SocketInfo> SocketsInfoDictionary = new ConcurrentDictionary<IntPtr, SocketInfo>();

        public IPAddress SourceIP { get; private set; }
        public int SPort { get; private set; }
        public IPAddress DestIP { get; private set; }
        public int DPort { get; private set; }
        public IntPtr SocketHandle { get; private set; }
        public IPAddress SocksDestIP { get; private set; }
        public int SocksDPort { get; private set; }
        public string SocksDestDomain { get; set; }

        public SocketInfo(IPAddress sourceIP, int sPort, IPAddress destIP, int dPort, IPAddress socksDestIP, int socksDPort, string socksDestDomain, IntPtr socketHandle)
        {
            SourceIP = sourceIP;
            DestIP = destIP;
            DPort = dPort;
            SPort = sPort;
            SocksDestIP = socksDestIP;
            SocksDPort = socksDPort;
            SocketHandle = socketHandle;
            SocksDestDomain = socksDestDomain;
        }

        // Function to generate a hash of IPs and ports
        public string GenerateHash()
        {
            using (var sha256 = SHA256.Create())
            {
                // Concatenate IPs and ports as string
                string data = $"{SourceIP.ToString()}|{DestIP.ToString()}|{DPort}|{SPort}";

                // Compute hash
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));

                // Convert hash bytes to string
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    stringBuilder.Append(hashBytes[i].ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }
    }
}