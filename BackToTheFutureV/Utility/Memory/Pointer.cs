using GTA.Math;
using System;

namespace BackToTheFutureV
{
    /// <summary>
    /// Smart pointer wrapper with Get and Set accessors.
    /// </summary>
    /// <remarks>
    /// Based on idea from here: 
    /// https://stackoverflow.com/questions/2980463/how-do-i-assign-by-reference-to-a-class-field-in-c
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public sealed class Pointer<T>
    {
        /// <summary>
        /// The Memory Address of the <see cref="Pointer{T}"/>
        /// </summary>
        public IntPtr MemoryAddress { get; }

        /// <summary>
        /// Creates a new instance of <see cref="Pointer{T}"/>.
        /// </summary>
        /// <param name="address">Memory Address of the pointer.</param>
        /// <param name="offset">Offset that will be added to <see cref="MemoryAddress"/>.</param>
        public Pointer(IntPtr address, int offset = 0)
        {
            MemoryAddress = address + offset;
        }

        /// <summary>
        /// Gets value the <see cref="Pointer{T}"/> points to.
        /// <para>
        /// Supported simple types are:
        /// </para>
        /// <para>
        /// - <see cref="byte"/>,
        /// <see cref="short"/>,
        /// <see cref="int"/>,
        /// <see cref="long"/>,
        /// <see cref="float"/>,
        /// <see cref="string"/>,
        /// <see cref="Vector3"/>
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// If unsuported type is passed, default <see cref="T"/> will be returned.
        /// </para>
        /// Unsuccessful read is logged into file.
        /// </remarks>
        /// <returns>
        /// Value of the <see cref="Pointer{T}"/> if read successfully. 
        /// Otherwise default value of the type <typeparamref name="T"/>
        /// for runtime safety reasons.
        /// </returns>
        public T Get()
        {
            if (!MemoryAddress.MayBeValid())
                return default;

            Type type = typeof(T);

            if (type == typeof(byte))
                return (T)Convert.ChangeType(NativeMemory.ReadByte(MemoryAddress), type);

            if (type == typeof(bool))
                return (T)Convert.ChangeType(NativeMemory.ReadByte(MemoryAddress), type);

            if (type == typeof(short))
                return (T)Convert.ChangeType(NativeMemory.ReadInt16(MemoryAddress), type);

            if (type == typeof(int))
                return (T)Convert.ChangeType(NativeMemory.ReadInt32(MemoryAddress), type);

            if (type == typeof(long))
                return (T)Convert.ChangeType(NativeMemory.ReadAddress(MemoryAddress).ToInt64(), type);

            if (type == typeof(float))
                return (T)Convert.ChangeType(NativeMemory.ReadFloat(MemoryAddress), type);

            if (type == typeof(string))
                return (T)Convert.ChangeType(NativeMemory.ReadString(MemoryAddress), type);

            if (type == typeof(Vector3))
            {
                float[] data = NativeMemory.ReadVector3(MemoryAddress);
                return (T)Convert.ChangeType(new Vector3(data[0], data[1], data[2]), type);
            }

            return default;
        }

        /// <summary>
        /// Creates a new instance of <see cref="INativeMemory"/> from <see cref="MemoryAddress"/>.
        /// </summary>
        /// <remarks>
        /// Unsuccessful read is logged into file.
        /// </remarks>
        /// <returns>
        /// A new instance of <see cref="INativeMemory"/> if read successfully. 
        /// Otherwise default value of the type <see cref="Pointer{T}"/>
        /// for runtime safety reasons.
        /// </returns>
        public T GetPointer()
        {
            if (!MemoryAddress.MayBeValid())
                return default;

            Type type = typeof(T);

            if (type.GetInterface(nameof(INativeMemory)) != null)
            {
                // Create a new instance of INativeMemory
                return (T)Activator.CreateInstance(type, NativeMemory.ReadAddress(MemoryAddress));
            }

            return Activator.CreateInstance<T>();
        }

        public void Set(T value)
        {
            if (!MemoryAddress.MayBeValid())
                return;

            Type type = typeof(T);

            // TODO: Support for long and string

            if (type == typeof(byte))
                NativeMemory.WriteByte(MemoryAddress, (byte)Convert.ChangeType(value, type));

            if (type == typeof(short))
                NativeMemory.WriteInt16(MemoryAddress, (short)Convert.ChangeType(value, type));

            if (type == typeof(int))
                NativeMemory.WriteInt32(MemoryAddress, (int)Convert.ChangeType(value, type));

            if (type == typeof(float))
                NativeMemory.WriteFloat(MemoryAddress, (float)Convert.ChangeType(value, type));

            if (type == typeof(Vector3))
            {
                Vector3 vec = (Vector3)Convert.ChangeType(value, type);

                NativeMemory.WriteVector3(MemoryAddress, vec.ToArray());
            }
        }
    }
}
