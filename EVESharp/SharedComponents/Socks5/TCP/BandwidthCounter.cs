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

using System;

namespace SharedComponents.Socks5.TCP
{
    public class BandwidthCounter
    {
        #region Classes

        /// <summary>
        ///     Class to manage an adapters current transfer rate
        /// </summary>
        private class MiniCounter
        {
            #region Fields

            public ulong bytes;
            public ulong gbytes;
            public ulong kbytes;
            public ulong mbytes;
            public ulong pbytes;
            public ulong tbytes;
            private DateTime lastRead = DateTime.Now;

            #endregion Fields

            #region Methods

            /// <summary>
            ///     Adds bits(total misnomer because bits per second looks a lot better than bytes per second)
            /// </summary>
            /// <param name="count">The number of bits to add</param>
            public void AddBytes(ulong count)
            {
                bytes += count;
                while (bytes > 1024)
                {
                    kbytes++;
                    bytes -= 1024;
                }
                while (kbytes > 1024)
                {
                    mbytes++;
                    kbytes -= 1024;
                }
                while (mbytes > 1024)
                {
                    gbytes++;
                    mbytes -= 1024;
                }
                while (gbytes > 1024)
                {
                    tbytes++;
                    gbytes -= 1024;
                }
                while (tbytes > 1024)
                {
                    pbytes++;
                    tbytes -= 1024;
                }
            }

            /// <summary>
            ///     Returns the bits per second since the last time this function was called
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if (pbytes > 0)
                {
                    double ret = pbytes + (double)tbytes / 1024;
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " PB";
                }
                if (tbytes > 0)
                {
                    double ret = tbytes + (double)gbytes / 1024;
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " TB";
                }
                if (gbytes > 0)
                {
                    double ret = gbytes + (double)mbytes / 1024;
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " GB";
                }
                if (mbytes > 0)
                {
                    double ret = mbytes + (double)kbytes / 1024;
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " MB";
                }
                if (kbytes > 0)
                {
                    double ret = kbytes + (double)bytes / 1024;
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " KB";
                }
                else
                {
                    double ret = bytes;
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " B";
                }
            }

            #endregion Methods
        }

        #endregion Classes

        #region Fields

        private ulong bytes;
        private ulong gbytes;
        private ulong kbytes;
        private ulong mbytes;
        private ulong pbytes;
        private MiniCounter perSecond = new MiniCounter();
        private ulong tbytes;

        #endregion Fields

        #region Methods

        /// <summary>
        ///     Adds bytes to the total transfered
        /// </summary>
        /// <param name="count">Byte count</param>
        public void AddBytes(ulong count)
        {
            // overflow max
            perSecond.AddBytes(count);
            bytes += count;
            while (bytes > 1024)
            {
                kbytes++;
                bytes -= 1024;
            }
            while (kbytes > 1024)
            {
                mbytes++;
                kbytes -= 1024;
            }
            while (mbytes > 1024)
            {
                gbytes++;
                mbytes -= 1024;
            }
            while (gbytes > 1024)
            {
                tbytes++;
                gbytes -= 1024;
            }
            while (tbytes > 1024)
            {
                pbytes++;
                tbytes -= 1024;
            }
        }

        /// <summary>
        ///     Accesses the current transfer rate, returning the text
        /// </summary>
        /// <returns></returns>
        public string GetPerSecond()
        {
            string s = perSecond + "/s";
            perSecond = new MiniCounter();
            return s;
        }

        /// <summary>
        ///     Prints out a relevant string for the bits transfered
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (pbytes > 0)
            {
                double ret = pbytes + (double)tbytes / 1024;
                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " Pb";
            }
            if (tbytes > 0)
            {
                double ret = tbytes + (double)gbytes / 1024;

                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " TB";
            }
            if (gbytes > 0)
            {
                double ret = gbytes + (double)mbytes / 1024;
                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " GB";
            }
            if (mbytes > 0)
            {
                double ret = mbytes + (double)kbytes / 1024;

                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " MB";
            }
            if (kbytes > 0)
            {
                double ret = kbytes + (double)bytes / 1024;

                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " KB";
            }
            else
            {
                string s = bytes.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " b";
            }
        }

        #endregion Methods
    }
}