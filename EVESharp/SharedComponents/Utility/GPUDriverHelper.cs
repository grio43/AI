using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.Utility
{
    public class GPUDriverHelpers
    {
        public static long ConvertGpuDriverStringToLong(string s)
        {
            if (s.Contains("."))
            {
                var k = 0;
                UInt16[] res = new UInt16[4];
                foreach (var p in s.Split('.'))
                {
                    var i = UInt16.Parse(p);
                    res[k] = i;
                    k++;
                }
                LARGE_INTEGER largeInt;
                largeInt.QuadPart = 0;
                largeInt.A = res[3];
                largeInt.B = res[2];
                largeInt.C = res[1];
                largeInt.D = res[0];
                return largeInt.QuadPart;
            }
            return 0;
        }

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct LARGE_INTEGER
        {
            [FieldOffset(0)] public Int64 QuadPart;
            [FieldOffset(0)] public UInt16 A;
            [FieldOffset(2)] public UInt16 B;
            [FieldOffset(4)] public UInt16 C;
            [FieldOffset(6)] public UInt16 D;
        }
    }
}
