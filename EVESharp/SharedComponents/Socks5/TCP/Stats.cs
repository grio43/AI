/*
    Socks5 - A full-fledged high-performance socks5 proxy server written in C#. Plugin support included.
    Copyright (C) 2016 ThrDev

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace SharedComponents.Socks5.TCP
{
    public enum ByteType
    {
        Sent,
        Received
    }

    public enum PacketType
    {
        Sent,
        Received
    }

    public class Stats
    {
        #region Constructors

        public Stats()
        {
            sc = new BandwidthCounter();
            rc = new BandwidthCounter();
        }

        #endregion Constructors

        #region Fields

        private readonly BandwidthCounter rc;
        private readonly BandwidthCounter sc;

        #endregion Fields

        #region Properties

        //per sec.
        public string BytesReceivedPerSec => rc.GetPerSecond();

        public string BytesSentPerSec => sc.GetPerSecond();
        public int ClientsSinceRun { get; private set; }
        public ulong NetworkReceived { get; private set; }
        public ulong NetworkSent { get; private set; }
        public ulong PacketsReceived { get; private set; }
        public ulong PacketsSent { get; private set; }
        public int TotalClients { get; private set; }

        #endregion Properties

        #region Methods

        public void AddBytes(int bytes, ByteType typ)
        {
            if (typ != ByteType.Sent)
            {
                rc.AddBytes((uint)bytes);
                NetworkReceived += (ulong)bytes;
                return;
            }
            sc.AddBytes((uint)bytes);
            NetworkSent += (ulong)bytes;
        }

        public void AddClient()
        {
            TotalClients++;
            ClientsSinceRun++;
        }

        public void AddPacket(PacketType pkt)
        {
            if (pkt != PacketType.Sent)
                PacketsReceived++;
            else
                PacketsSent++;
        }

        public void ResetClients(int count)
        {
            TotalClients = count;
        }

        #endregion Methods
    }
}