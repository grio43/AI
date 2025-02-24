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
using System.Net.Sockets;

namespace SharedComponents.Socks5.HTTP
{
    //WARNING: BETA - Doesn't work as well as intended. Use at your own discretion.
    public class Chunked
    {
        #region Fields

        private readonly byte[] totalbuff;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///     Create a new instance of chunked.
        /// </summary>
        /// <param name="f"></param>
        public Chunked(Socket f, byte[] oldbuffer, int size)
        {
            //Find first chunk.
            if (IsChunked(oldbuffer))
            {
                int endofheader = oldbuffer.FindString("\r\n\r\n");
                int endofchunked = oldbuffer.FindString("\r\n", endofheader + 4);
                //
                string chunked = oldbuffer.GetBetween(endofheader + 4, endofchunked);
                //convert chunked data to int.
                int totallen = chunked.FromHex();
                //
                if (totallen > 0)
                {
                    //start a while loop and receive till end of chunk.
                    totalbuff = new byte[65535];
                    RawData = new byte[size];
                    //remove chunk data before adding.
                    oldbuffer = oldbuffer.ReplaceBetween(endofheader + 4, endofchunked + 2, new byte[] { });
                    Buffer.BlockCopy(oldbuffer, 0, RawData, 0, size);
                    if (f.Connected)
                    {
                        int totalchunksize = 0;
                        int received = f.Receive(totalbuff, 0, totalbuff.Length, SocketFlags.None);
                        while ((totalchunksize = GetChunkSize(totalbuff, received)) != -1)
                        {
                            //add data to final byte buffer.
                            byte[] chunkedData = GetChunkData(totalbuff, received);
                            byte[] tempData = new byte[chunkedData.Length + RawData.Length];
                            //get data AFTER chunked response.
                            Buffer.BlockCopy(RawData, 0, tempData, 0, RawData.Length);
                            Buffer.BlockCopy(chunkedData, 0, tempData, RawData.Length, chunkedData.Length);
                            //now add to finalbuff.
                            RawData = tempData;
                            //receive again.
                            if (totalchunksize == -2)
                                break;
                            received = f.Receive(totalbuff, 0, totalbuff.Length, SocketFlags.None);
                        }
                        //end of chunk.
                        Console.WriteLine("Got chunk! Size: {0}", RawData.Length);
                    }
                }
                else
                {
                    RawData = new byte[size];
                    Buffer.BlockCopy(oldbuffer, 0, RawData, 0, size);
                }
            }
        }

        #endregion Constructors

        #region Properties

        public byte[] ChunkedData
        {
            get
            {
                //get size from \r\n\r\n and past.
                int location = RawData.FindString("\r\n\r\n") + 4;
                //size
                int size = RawData.Length - location - 7; //-7 is initial end of chunk data.
                return RawData.ReplaceString("\r\n\r\n", "\r\n\r\n" + size.ToHex().Replace("0x", "") + "\r\n");
            }
        }

        public byte[] RawData { get; }

        #endregion Properties

        #region Methods

        public static byte[] GetChunkData(byte[] buffer, int size)
        {
            //parse out the chunk size and return data.
            return buffer.GetInBetween(buffer.FindString("\r\n") + 2, size);
        }

        public static int GetChunkSize(byte[] buffer, int count)
        {
            //chunk size is first chars till \r\n.
            if (buffer.FindString("\r\n0\r\n\r\n", count - 7) != -1)
                return -2;
            string chunksize = buffer.GetBetween(0, buffer.FindString("\r\n"));
            return chunksize.FromHex();
        }

        public static bool IsChunked(byte[] buffer)
        {
            return IsHTTP(buffer) && buffer.FindString("Transfer-Encoding: chunked\r\n") != -1;
        }

        public static bool IsHTTP(byte[] buffer)
        {
            return buffer.FindString("HTTP/1.1") != -1 && buffer.FindString("\r\n\r\n") != -1;
        }

        #endregion Methods
    }
}