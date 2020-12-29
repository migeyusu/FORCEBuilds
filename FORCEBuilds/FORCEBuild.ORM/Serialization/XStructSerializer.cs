using System;
using System.Runtime.InteropServices;

namespace FORCEBuild.Persistence.Serialization
{
    public static class XStructSerializer
    {
        public static byte[] ToBytes<T>(this T t) where T:struct 
        {
            var type = t.GetType();
            var size = Marshal.SizeOf(type);
            var dataBytes = new byte[size];
            var structPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(t, structPtr, true);
            Marshal.Copy(structPtr, dataBytes, 0, size);
            Marshal.FreeHGlobal(structPtr);
            return dataBytes;
        }

        public static T ToStruct<T>(this byte[] datas) where T : struct
        {
            var type = typeof(T);
            var size = Marshal.SizeOf(type);
            var structPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(datas, 0, structPtr, size);
            var t = (T) Marshal.PtrToStructure(structPtr, type);
            Marshal.FreeHGlobal(structPtr);
            return t;
        }
    }

}
