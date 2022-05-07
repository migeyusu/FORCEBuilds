using System;
using System.Runtime.InteropServices;

namespace FORCEBuild.Serialization
{
    public static class StructSerializer
    {
        public static byte[] GetBytes(object instance)
        {
            var type = instance.GetType();
            if (!type.IsValueType || type.IsPrimitive)
            {
                throw new ArgumentException($"instance is not struct!", nameof(instance));
            }

            var size = Marshal.SizeOf(type);
            var dataBytes = new byte[size];
            var structPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(instance, structPtr, true);
            Marshal.Copy(structPtr, dataBytes, 0, size);
            Marshal.FreeHGlobal(structPtr);
            return dataBytes;
        }

        public static byte[] GetBytes<T>(this T t) where T : struct
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

        public static object ToStruct(this byte[] data, Type type, int position = 0)
        {
            if (!type.IsValueType || type.IsPrimitive)
            {
                throw new ArgumentException($"type {type} is not struct!", nameof(type));
            }

            return ToStructInternal(data, type, position);
        }

        private static object ToStructInternal(byte[] data, Type type, int position = 0)
        {
            object instance;
            if (position < 0)
            {
                position = 0;
            }

            if (position > 0)
            {
                var size = Marshal.SizeOf(type);
                var structPtr = Marshal.AllocHGlobal(size);
                Marshal.Copy(data, position, structPtr, size);
                instance = Marshal.PtrToStructure(structPtr, type);
                Marshal.FreeHGlobal(structPtr);
            }
            else
            {
                var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    return Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
                }
                finally
                {
                    handle.Free();
                }
            }

            return instance;
        }

        public static T ToStruct<T>(this byte[] data, int position = 0) where T : struct
        {
            var type = typeof(T);
            return (T)ToStructInternal(data, type, position);
        }
    }
}