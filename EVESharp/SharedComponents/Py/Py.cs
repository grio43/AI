﻿// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

using SharedComponents.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharedComponents.Py
{
    public class Py
    {
        private static bool _initialized;
        private static IntPtr _moduleHandle;
        private static Dictionary<IntPtr, PyType> _types = new Dictionary<IntPtr, PyType>();
        private static Dictionary<string, IntPtr> _structures = new Dictionary<string, IntPtr>();

        private static IntPtr _pyNoneStruct = IntPtr.Zero;

        internal static IntPtr PyNoneStruct
        {
            get
            {
                // This structure has additional caching due to the frequency at which its called
                if (_pyNoneStruct == IntPtr.Zero)
                    _pyNoneStruct = GetStruct("_Py_NoneStruct");

                return _pyNoneStruct;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string lpszLib);

        internal static IntPtr GetStruct(string name)
        {
            IntPtr structure;
            if (!_structures.TryGetValue(name, out structure))
            {
                if (_moduleHandle == IntPtr.Zero)
                    _moduleHandle = LoadLibrary("python27.dll");

                structure = GetProcAddress(_moduleHandle, name);
                if (structure == IntPtr.Zero)
                {
                    Util.GlobalRemoteLog("Structure " + name + " not found!");
                    throw new Exception("Structure " + name + " not found!");
                }

                if (structure != IntPtr.Zero)
                    _structures[name] = structure;
            }
            return structure;
        }

        private static void InitializeStructures()
        {
            _initialized = true;

            _types.Add(GetStruct("PyBaseObject_Type"), PyType.BaseObjectType);
            _types.Add(GetStruct("PyBaseString_Type"), PyType.BaseStringType);
            _types.Add(GetStruct("PyBomb_Type"), PyType.BombType);
            _types.Add(GetStruct("PyBool_Type"), PyType.BoolType);
            _types.Add(GetStruct("PyBuffer_Type"), PyType.BufferType);
            _types.Add(GetStruct("PyByteArrayIter_Type"), PyType.ByteArrayIterType);
            _types.Add(GetStruct("PyByteArray_Type"), PyType.ByteArrayType);
            _types.Add(GetStruct("PyCFrame_Type"), PyType.CFrameType);
            _types.Add(GetStruct("PyCFunction_Type"), PyType.CFunctionType);
            _types.Add(GetStruct("PyCObject_Type"), PyType.CObjectType);
            _types.Add(GetStruct("PyCStack_Type"), PyType.CStackType);
            _types.Add(GetStruct("PyCallIter_Type"), PyType.CallIterType);
            _types.Add(GetStruct("PyCapsule_Type"), PyType.CapsuleType);
            _types.Add(GetStruct("PyCell_Type"), PyType.CellType);
            _types.Add(GetStruct("PyChannel_TypePtr"), PyType.ChannelTypePtr);
            _types.Add(GetStruct("PyClassMethod_Type"), PyType.ClassMethodType);
            _types.Add(GetStruct("PyClass_Type"), PyType.ClassType);
            _types.Add(GetStruct("PyCode_Type"), PyType.CodeType);
            _types.Add(GetStruct("PyComplex_Type"), PyType.ComplexType);
            _types.Add(GetStruct("PyDictItems_Type"), PyType.DictItemsType);
            _types.Add(GetStruct("PyDictIterItem_Type"), PyType.DictIterItemType);
            _types.Add(GetStruct("PyDictIterKey_Type"), PyType.DictIterKeyType);
            _types.Add(GetStruct("PyDictIterValue_Type"), PyType.DictIterValueType);
            _types.Add(GetStruct("PyDictKeys_Type"), PyType.DictKeysType);
            _types.Add(GetStruct("PyDictProxy_Type"), PyType.DictProxyType);
            _types.Add(GetStruct("PyDictValues_Type"), PyType.DictValuesType);
            _types.Add(GetStruct("PyDict_Type"), PyType.DictType);
            _types.Add(GetStruct("PyEllipsis_Type"), PyType.EllipsisType);
            _types.Add(GetStruct("PyEnum_Type"), PyType.EnumType);
            _types.Add(GetStruct("PyExc_TypeError"), PyType.ExcTypeError);
            _types.Add(GetStruct("PyFile_Type"), PyType.FileType);
            _types.Add(GetStruct("PyFlexType_TypePtr"), PyType.FlexTypeTypePtr);
            _types.Add(GetStruct("PyFloat_Type"), PyType.FloatType);
            _types.Add(GetStruct("PyFrame_Type"), PyType.FrameType);
            _types.Add(GetStruct("PyFrozenSet_Type"), PyType.FrozenSetType);
            _types.Add(GetStruct("PyFunction_Type"), PyType.FunctionType);
            _types.Add(GetStruct("PyGen_Type"), PyType.GenType);
            _types.Add(GetStruct("PyGetSetDescr_Type"), PyType.GetSetDescrType);
            _types.Add(GetStruct("PyInstance_Type"), PyType.InstanceType);
            _types.Add(GetStruct("PyInt_Type"), PyType.IntType);
            _types.Add(GetStruct("PyList_Type"), PyType.ListType);
            _types.Add(GetStruct("PyLong_Type"), PyType.LongType);
            _types.Add(GetStruct("PyMemberDescr_Type"), PyType.MemberDescrType);
            _types.Add(GetStruct("PyMemoryView_Type"), PyType.MemoryViewType);
            _types.Add(GetStruct("PyMethod_Type"), PyType.MethodType);
            _types.Add(GetStruct("PyModule_Type"), PyType.ModuleType);
            _types.Add(GetStruct("PyNullImporter_Type"), PyType.NullImporterType);
            _types.Add(GetStruct("PyObject_Type"), PyType.ObjectType);
            _types.Add(GetStruct("PyProperty_Type"), PyType.PropertyType);
            _types.Add(GetStruct("PyRange_Type"), PyType.RangeType);
            _types.Add(GetStruct("PyReversed_Type"), PyType.ReversedType);
            _types.Add(GetStruct("PySTEntry_Type"), PyType.STEntryType);
            _types.Add(GetStruct("PySeqIter_Type"), PyType.SeqIterType);
            _types.Add(GetStruct("PySet_Type"), PyType.SetType);
            _types.Add(GetStruct("PySlice_Type"), PyType.SliceType);
            _types.Add(GetStruct("PyStaticMethod_Type"), PyType.StaticMethodType);
            _types.Add(GetStruct("PyString_Type"), PyType.StringType);
            _types.Add(GetStruct("PySuper_Type"), PyType.SuperType);
            _types.Add(GetStruct("PyTasklet_TypePtr"), PyType.TaskletTypePtr);
            _types.Add(GetStruct("PyTraceBack_Type"), PyType.TraceBackType);
            _types.Add(GetStruct("PyTuple_Type"), PyType.TupleType);
            _types.Add(GetStruct("PyType_Type"), PyType.TypeType);
            _types.Add(GetStruct("PyUnicode_Type"), PyType.UnicodeType);
            _types.Add(GetStruct("PyWrapperDescr_Type"), PyType.WrapperDescrType);
            _types.Add(GetStruct("PyClassMethodDescr_Type"), PyType.ClassMethodDescrType);
            _types.Add(GetStruct("PyMethodDescr_Type"), PyType.MethodDescrType);
            _types.Add(GetStruct("PyMethodWrapper_Type"), PyType.MethodWrapperType);
        }

        internal static PyType GetPyType(IntPtr po)
        {
            return GetPyType(po, false);
        }

        internal static PyType GetPyType(IntPtr po, bool derived)
        {
            if (!_initialized)
                InitializeStructures();

            if (po == IntPtr.Zero)
                return PyType.Invalid;

            PyType type;
            var pyType = Marshal.ReadIntPtr(po.Add(8));
            if (!_types.TryGetValue(pyType, out type))
            {
                type = GetPyType(pyType, true);
                _types.Add(pyType, type);
                return type;
            }

            if (derived)
            {
                var s = type.ToString();
                type = (PyType)Enum.Parse(typeof(PyType), "Derived" + s);
            }

            return type;
        }

        internal static int GetRefCnt(IntPtr po)
        {
            if (po == IntPtr.Zero)
                return 0;

            return Marshal.ReadInt32(po);
        }

        internal static IntPtr GetThreadState()
        {
            return GetStruct("_PyThreadState_Current");
        }

        internal static IntPtr ExchangePyFrame(IntPtr frame)
        {
            var tstate = Marshal.ReadIntPtr(GetThreadState());
            var prevFrame = Marshal.ReadIntPtr(tstate.Add(16));
            Marshal.WriteIntPtr(tstate.Add(16), frame);
            return prevFrame;
        }

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyImport_ImportModule(string module);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyEval_GetGlobals();

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Py_DecRef(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Py_IncRef(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyObject_GetAttrString(IntPtr op, string attr);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Py_CompileString(string code, string filename, int start);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyImport_AddModule(string name);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyGILState_GetThisThreadState();

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyModule_GetDict(IntPtr module);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)] // returns the function pointer of the c function type obj
        public static extern IntPtr PyCFunction_GetFunction(IntPtr module);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyEval_EvalCode(IntPtr pyCodeObj, IntPtr globals, IntPtr locals);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyTuple_New(int size);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyTuple_Pack(int size, IntPtr a, IntPtr b);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyTuple_Pack(int size, IntPtr a, IntPtr b, IntPtr c);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PyObject_Length(IntPtr o);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PyObject_Size(IntPtr o);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyTuple_Pack(int size, IntPtr a, IntPtr b, IntPtr c, IntPtr d);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyObject_GetItem(IntPtr obj, IntPtr key);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PyObject_SetAttrString(IntPtr op, string attr, IntPtr v);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyObject_HasAttrString(IntPtr op, string attr);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyEval_CallObjectWithKeywords(IntPtr op, IntPtr args, IntPtr kw);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyObject_Call(IntPtr op, IntPtr args, IntPtr kw);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyEval_EvalFrame(IntPtr frame);

        // Sadly enough C# doesnt support vararg dllimport's, so we simply made a list of overrides with 7 parameters,
        // if more get added then this needs to be extended
        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Py_BuildValue(string format);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Py_BuildValue(string format, IntPtr parm1);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Py_BuildValue(string format, IntPtr parm1, IntPtr parm2);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Py_BuildValue(string format, IntPtr parm1, IntPtr parm2, IntPtr parm3);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Py_BuildValue(string format, IntPtr parm1, IntPtr parm2, IntPtr parm3, IntPtr parm4);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Py_BuildValue(string format, IntPtr parm1, IntPtr parm2, IntPtr parm3, IntPtr parm4, IntPtr parm5);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Py_BuildValue(string format, IntPtr parm1, IntPtr parm2, IntPtr parm3, IntPtr parm4, IntPtr parm5, IntPtr parm6);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Py_BuildValue(string format, IntPtr parm1, IntPtr parm2, IntPtr parm3, IntPtr parm4, IntPtr parm5, IntPtr parm6,
            IntPtr parm7);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Py_BuildValue(string format, IntPtr parm1, IntPtr parm2, IntPtr parm3, IntPtr parm4, IntPtr parm5, IntPtr parm6,
            IntPtr parm7, IntPtr parm8);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyCallable_Check(IntPtr intptr_0);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyList_New(int size);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PyList_Size(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyList_GetItem(IntPtr op, int index);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyList_SetItem(IntPtr op, int index, IntPtr item);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void PyTuple_SetItem(IntPtr tuple, int index, IntPtr item);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyList_SetSlice(IntPtr op, int low, int high, IntPtr list);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyTuple_Size(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyTuple_GetItem(IntPtr op, int index);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyCode_NewEmpty(string filename, string funcname, int firstlineno);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyFrame_New(IntPtr tstate, IntPtr code, IntPtr globals, IntPtr locals);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyString_Size(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyString_AsString(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyObject_Str(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyString_FromString(string s);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyString_FromStringAndSize(IntPtr s, int len);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern string PyUnicodeUCS2_AsRawUnicodeEscapeString(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyUnicodeUCS2_GetSize(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyUnicodeUCS2_AsUnicode(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        internal static extern IntPtr PyUnicodeUCS2_FromUnicode([MarshalAs(UnmanagedType.LPWStr)] string op, int size);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr fopen(String filename, String mode);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr _wfopen(String filename, String mode);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyMarshal_ReadObjectFromFile(IntPtr file);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyBool_FromLong(int op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyLong_AsLong(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyLong_FromLong(int op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern long PyLong_AsLongLong(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyLong_FromLongLong(long op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyObject_GetIter(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyIter_Next(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint PySet_Size(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong PyLong_AsUnsignedLongLong(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyLong_FromUnsignedLongLong(ulong op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyObject_CallObject(IntPtr obj, IntPtr args);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern double PyFloat_AsDouble(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyFloat_FromDouble(double op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyObject_Dir(IntPtr op);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyObject_CallMethod(IntPtr op, string call, string parm1, IntPtr parm2, IntPtr parm3);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void PyErr_Clear();

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void PyErr_Fetch(out IntPtr ptype, out IntPtr pvalue, out IntPtr ptraceback);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyAnySet_Check(IntPtr set);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PySet_Contains(IntPtr set, IntPtr value);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyDict_Contains(IntPtr dict, IntPtr value);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyErr_Occurred();

        [Obsolete("DO NOT USE, sets sys.trackback (...) and eve is capturing std:error", true)]
        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyErr_Print();

        [Obsolete("DO NOT USE, Eve captures std:error", true)]
        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyErr_PrintEx(int set_sys_last_vars);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyDict_New();

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyDict_GetItemString(IntPtr op, string key);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyDict_GetItem(IntPtr op, IntPtr key);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyDict_SetItem(IntPtr op, IntPtr key, IntPtr val);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int PyDict_DelItem(IntPtr op, IntPtr key);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PyCFunction_NewEx(IntPtr md, IntPtr self, IntPtr name);

        [DllImport("python27.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr PyObject_Repr(IntPtr v);

        [StructLayout(LayoutKind.Sequential)]
        public struct PyMethodDef
        {
            public IntPtr ml_name; /* The name of the built-in function/method */
            public IntPtr ml_meth; /* The C function that implements it */

            public int ml_flags; /* Combination of METH_xxx flags, which mostly
				                       describe the args expected by the C func */

            public IntPtr ml_doc; /* The __doc__ attribute, or NULL */
        };
    }
}