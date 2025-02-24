﻿using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace SharedComponents.Utility
{
    /// <summary>
    ///     This is an IDisposable wrapper for SecureString, which provides secure byte array access
    ///     http://codereview.stackexchange.com/questions/107860/converting-a-securestring-to-a-byte-array
    /// </summary>
    /// <summary>
    ///     This is an IDispoable wrapper for secure byte[] arrays, wiping pre-existing data when altered, Disposed or GC'd. It
    ///     is designed similar to SecureStringWrapper for convenience.
    /// </summary>
    public sealed class SecureBytesWrapper : CriticalFinalizerObject, IDisposable
    {
        #region Destructors

        ~SecureBytesWrapper()
        {
            Dispose();
        }

        #endregion Destructors

        #region Fields

        private const string hexChars = "0123456789abcdef";
        private static byte[] _hexTable;
        private byte[] _Bytes;
        private bool _disposed;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///     Initialize without a byte array. "Bytes" can be set later.
        /// </summary>
        public SecureBytesWrapper()
        {
        }

        /// <summary>
        ///     Initialize with byte array via the SecureStringWrapper, but possibly convert from hex string...
        /// </summary>
        /// <param name="ssw"></param>
        /// <param name="convertFromHex"></param>
        public SecureBytesWrapper(SecureStringWrapper ssw, bool convertFromHex)
        {
            if (!convertFromHex)
            {
                CopyBytes(ssw.ToByteArray());
                return;
            }
            if (_hexTable == null)
            {
                _hexTable = new byte[256];
                _hexTable['0'] = 0;
                _hexTable['1'] = 1;
                _hexTable['2'] = 2;
                _hexTable['3'] = 3;
                _hexTable['4'] = 4;
                _hexTable['5'] = 5;
                _hexTable['6'] = 6;
                _hexTable['7'] = 7;
                _hexTable['8'] = 8;
                _hexTable['9'] = 9;
                _hexTable['a'] = 10;
                _hexTable['b'] = 11;
                _hexTable['c'] = 12;
                _hexTable['d'] = 13;
                _hexTable['e'] = 14;
                _hexTable['f'] = 15;
            }

            using (SecureBytesWrapper temp = new SecureBytesWrapper(ssw, false))
            {
                _Bytes = new byte[temp.Bytes.Length / 2];
                for (int i = 0; i < temp.Bytes.Length; i += 2)
                {
                    byte b1 = temp.Bytes[i];
                    byte b2 = temp.Bytes[i + 1];
                    _Bytes[i / 2] = (byte)(_hexTable[b1] * 16 + _hexTable[b2]);
                }
            }
        }

        #endregion Constructors

        #region Properties

        public byte[] Bytes
        {
            get => _Bytes;
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("SecureBytesWrapper");
                Destroy();
                _Bytes = value;
            }
        }

        /// <summary>
        ///     Determine if there are any bytes in the array...
        /// </summary>
        public bool HasData => _Bytes != null && _Bytes.Length > 0;

        #endregion Properties

        #region Methods

        /// <summary>
        ///     Duplicates the given byte array, instead of co-opting it.
        /// </summary>
        /// <param name="copyFrom"></param>
        public void CopyBytes(byte[] copyFrom)
        {
            Bytes = new byte[copyFrom.Length];
            Buffer.BlockCopy(copyFrom, 0, Bytes, 0, copyFrom.Length);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Destroy();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Purely for similarity to SecureStringWrapper
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return _Bytes;
        }

        private void Destroy()
        {
            if (_Bytes == null) return;

            for (int i = 0; i < _Bytes.Length; i++)
                _Bytes[i] = 0;
            _Bytes = null;
        }

        #endregion Methods
    }

    public sealed class SecureStringWrapper : CriticalFinalizerObject, IDisposable
    {
        #region Properties

        public bool HasData
        {
            get
            {
                if (_bytes != null)
                    return _bytes.Length > 0;
                return secureString.Length > 0;
            }
        }

        #endregion Properties

        #region Destructors

        ~SecureStringWrapper()
        {
            Dispose();
        }

        #endregion Destructors

        #region Fields

        private const string hexChars = "0123456789abcdef";
        private readonly Encoding encoding;
        private readonly SecureString secureString;
        private byte[] _bytes;
        private bool _disposed;

        #endregion Fields

        #region Constructors

        public SecureStringWrapper(SecureString secureString)
            : this(secureString, Encoding.UTF8)
        {
        }

        public SecureStringWrapper(SecureString secureString, Encoding encoding)
        {
            if (secureString == null)
                throw new ArgumentNullException("secureString");

            this.encoding = encoding ?? Encoding.UTF8;
            this.secureString = secureString;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Encode an arbitrary byte array as a hexadecimal string, into a SecureString
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static SecureStringWrapper ConvertToHex(byte[] bytes)
        {
            using (SecureString ss = new SecureString())
            {
                using (SecureStringWrapper ssw = new SecureStringWrapper(ss))
                {
                    // convert to hex
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        char c1 = hexChars[bytes[i] / 16];
                        char c2 = hexChars[bytes[i] % 16];
                        ss.AppendChar(c1);
                        ss.AppendChar(c2);
                    }
                    ss.MakeReadOnly();

                    return new SecureStringWrapper(ss.Copy());
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Destroy();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        public unsafe byte[] ToByteArray()
        {
            if (_bytes != null)
                return _bytes;

            int maxLength = encoding.GetMaxByteCount(secureString.Length);

            IntPtr bytes = IntPtr.Zero;
            IntPtr str = IntPtr.Zero;

            try
            {
                bytes = Marshal.AllocHGlobal(maxLength);
                str = Marshal.SecureStringToBSTR(secureString);

                char* chars = (char*)str.ToPointer();
                byte* bptr = (byte*)bytes.ToPointer();
                int len = encoding.GetBytes(chars, secureString.Length, bptr, maxLength);

                _bytes = new byte[len];
                for (int i = 0; i < len; ++i)
                {
                    _bytes[i] = *bptr;
                    bptr++;
                }

                return _bytes;
            }
            finally
            {
                if (bytes != IntPtr.Zero)
                    Marshal.FreeHGlobal(bytes);
                if (str != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(str);
            }
        }

        private void Destroy()
        {
            if (_bytes == null) return;

            for (int i = 0; i < _bytes.Length; i++)
                _bytes[i] = 0;
            _bytes = null;
        }

        #endregion Methods
    }
}