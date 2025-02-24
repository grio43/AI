using System;
using System.Collections.Generic;
using System.IO;

namespace SharedComponents.EveMarshal
{

    public static class EvEMarshal
    {
        public static byte[] Process(PyObjectMarshal obj)
        {
            var ret = new MemoryStream(100);
            ret.WriteByte(Unmarshal.HeaderByte);
            // we have no support for save lists right now
            ret.WriteByte(0x00);
            ret.WriteByte(0x00);
            ret.WriteByte(0x00);
            ret.WriteByte(0x00);
            obj.Encode(new BinaryWriter(ret));
            return ret.ToArray();
        }

        public static PyTuple Tuple(params PyObjectMarshal[] objs)
        {
            return new PyTuple(new List<PyObjectMarshal>(objs));
        }

        public static PyDict Dict(params object[] objs)
        {
            if (objs.Length % 2 == 1)
                throw new ArgumentException("Expected pair arguments");

            var ret = new PyDict(new Dictionary<PyObjectMarshal, PyObjectMarshal>(objs.Length / 2));
            for (int i = 0; i < (objs.Length/2); i++)
            {
                var key = objs[i];
                var val = objs[i + 1];
                if (!(key is string))
                    throw new ArgumentException("Expected string");
                if (!(val is PyObjectMarshal))
                    throw new ArgumentException("Expected PyObjectMarshal");
                ret.Dictionary.Add(new PyString(key as string), val as PyObjectMarshal);
            }
            return ret;
        }
    }

}