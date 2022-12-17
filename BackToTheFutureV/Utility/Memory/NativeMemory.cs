using GTA.Math;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BackToTheFutureV
{
    /// <summary>
    /// Contains various game memory addresses.
    /// </summary>
    /// <remarks>
    /// VER_1_0_2372_0_STEAM
    /// </remarks>
    internal static unsafe class NativeMemory
    {
        // Scan addresses

        public static readonly IntPtr GameSettingsAddr;
        public static readonly IntPtr IsGameWindowFocusedAddr;
        public static readonly IntPtr CViewportGameAddr;

        // GameSettings -> Audio
        public const int SoundVolumeOffset = 0x0;
        public const int MusicVolumeOffset = 0x4;
        public const int DialogBoostOffset = 0x8c;
        public const int SoundOutputModeOffset = 0xC;
        public const int RadioStationOffset = 0x14;
        public const int FrontSurroundSpeakersModeOffset = 0x94;
        public const int RearSurroundSpeakersModeOffset = 0x98;
        public const int SelfRadioModeOffset = 0x280;
        public const int AutoMusicScanOffset = 0x284;
        public const int MuteSoundOnFocusLostOffset = 0x288;
        public const int IsGamePausedOffset = 0x2B9;

        // GameSettings -> Camera
        public const int TransportCameraHeightOffset = 0x24;

        // GameSettings -> Video
        public const int RadarOffset = 0x54;
        public const int IsUserInterfaceVisibleOffset = 0x58;
        public const int CrosshairTargetOffset = 0x5C;
        public const int SimpleReticuleSizeOffset = 0x60;
        public const int IsGpsRouteVisibleOffset = 0x64;
        public const int SafezoneSizeOffset = 0x68;

        // Vehicle
        public const int VehicleAudioOffset = 0x970;

        // Audio
        public const int EnvironmentGroupOffset = 0x38;
        public const int ReverbOffset = 0x38;
        public const int EchoOffset = 0x3C;
        public const int EnvironmentReverbOffset = 0xB8;

        /// <summary>
        /// Scans game memory.
        /// </summary>
        static NativeMemory()
        {
            unsafe
            {
                Pattern pattern;
                IntPtr address;
                int offset;

                // GameSettings

                pattern = new Pattern(
                    "\x4c\x8d\x15\x00\x00\x00\x00\x48\x63\xc1\x45\x8b\x04\x82",
                    "xxx????xxxxxxx");
                address = pattern.Get(3);
                offset = *(int*)address;
                GameSettingsAddr = address + offset + 4 + 0x1C;

                // IsGameWindowFocused
                pattern = new Pattern(
                    "\x88\x05\x00\x00\x00\x00\xf6\xd8\x1a\xc0",
                    "xx????xxxx");
                address = pattern.Get(2);
                offset = *(int*)address;
                IsGameWindowFocusedAddr = address + offset + 4;

                // ViewportGame

                pattern = new Pattern(
                    "\x48\x8b\x0d\x00\x00\x00\x00\x48\x83\xc1\x00\xf3\x0f\x10\x9b\x00\x00\x00\x00\xf3\x0f\x10\x93",
                    "xxx????xxx?xxxx????xxxx");
                address = pattern.Get(3);
                offset = *(int*)address;
                CViewportGameAddr = address + offset + 4;
            }
        }

        /// <summary>
        /// Safe get method that gets value from base address and offset.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="baseAddr">Base address to get value from.</param>
        /// <param name="offset">Memory offset from base address.</param>
        /// <returns>
        /// Value from given pointer. 
        /// In case if base address is nullptr, it returns default value.
        /// </returns>
        public static unsafe T Get<T>(IntPtr baseAddr, int offset = 0)
        {
            IntPtr addr = baseAddr + offset;
            Type tType = typeof(T);
            T defaultValue;

            // TODO: Move to extension
            if (tType.GetInterface(nameof(INativeMemory)) != null)
            {
                defaultValue = (T)Activator.CreateInstance(tType, *(IntPtr*)addr.ToPointer());
            }
            else
            {
                defaultValue = Activator.CreateInstance<T>();
            }

            if (baseAddr == IntPtr.Zero)
            {
                return defaultValue;
            }

            if (baseAddr == null)
            {
                return defaultValue;
            }

            // Addresses below 0x1000 most likely are NullPtr
            if (baseAddr.ToInt64() <= 0x1000)
            {
                return defaultValue;
            }

            if (tType == typeof(byte))
            {
                return (T)Convert.ChangeType(Marshal.ReadByte(addr), typeof(byte));
            }
            else if (tType == typeof(short))
            {
                return (T)Convert.ChangeType(Marshal.ReadInt16(addr), typeof(short));
            }
            else if (tType == typeof(int))
            {
                return (T)Convert.ChangeType(Marshal.ReadInt32(addr), typeof(int));
            }
            else if (tType == typeof(long))
            {
                return (T)Convert.ChangeType(Marshal.ReadInt64(addr), typeof(long));
            }
            else if (tType == typeof(float))
            {
                return (T)Convert.ChangeType(*(float*)addr, tType);
            }
            else if (tType == typeof(bool))
            {
                return (T)Convert.ChangeType(*(bool*)addr, tType);
            }
            else if (tType == typeof(Vector3))
            {
                float x = Get<float>(baseAddr);
                float y = Get<float>(baseAddr, 0x4);
                float z = Get<float>(baseAddr, 0x8);

                return (T)Convert.ChangeType(new Vector3(x, y, z), typeof(Vector3));
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Reads a single 8-bit value from the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <returns>The value at the address.</returns>
        public static byte ReadByte(IntPtr address)
        {
            return *(byte*)address.ToPointer();
        }
        /// <summary>
        /// Reads a single 16-bit value from the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <returns>The value at the address.</returns>
        public static short ReadInt16(IntPtr address)
        {
            return *(short*)address.ToPointer();
        }
        /// <summary>
        /// Reads a single 32-bit value from the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <returns>The value at the address.</returns>
        public static int ReadInt32(IntPtr address)
        {
            return *(int*)address.ToPointer();
        }
        /// <summary>
        /// Reads a single floating-point value from the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <returns>The value at the address.</returns>
        public static float ReadFloat(IntPtr address)
        {
            return *(float*)address.ToPointer();
        }
        /// <summary>
        /// Reads a null-terminated UTF-8 string from the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <returns>The string at the address.</returns>
        public static string ReadString(IntPtr address)
        {
            return PtrToStringUTF8(address);
        }
        /// <summary>
        /// Reads a single 64-bit value from the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <returns>The value at the address.</returns>
        public static IntPtr ReadAddress(IntPtr address)
        {
            return new IntPtr(*(void**)address.ToPointer());
        }
        /// <summary>
        /// Reads a 4x4 floating-point matrix from the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <returns>All elements of the matrix in row major arrangement.</returns>
        public static float[] ReadMatrix(IntPtr address)
        {
            float* data = (float*)address.ToPointer();
            return new float[16] { data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12], data[13], data[14], data[15] };
        }
        /// <summary>
        /// Reads a 3-component floating-point vector from the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <returns>All elements of the vector.</returns>
        public static float[] ReadVector3(IntPtr address)
        {
            float* data = (float*)address.ToPointer();
            return new float[3] { data[0], data[1], data[2] };
        }

        /// <summary>
        /// Writes a single 8-bit value to the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteByte(IntPtr address, byte value)
        {
            byte* data = (byte*)address.ToPointer();
            *data = value;
        }
        /// <summary>
        /// Writes a single 16-bit value to the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteInt16(IntPtr address, short value)
        {
            short* data = (short*)address.ToPointer();
            *data = value;
        }
        /// <summary>
        /// Writes a single 32-bit value to the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteInt32(IntPtr address, int value)
        {
            int* data = (int*)address.ToPointer();
            *data = value;
        }
        /// <summary>
        /// Writes a single floating-point value to the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteFloat(IntPtr address, float value)
        {
            float* data = (float*)address.ToPointer();
            *data = value;
        }
        /// <summary>
        /// Writes a 4x4 floating-point matrix to the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <param name="value">The elements of the matrix in row major arrangement to write.</param>
        public static void WriteMatrix(IntPtr address, float[] value)
        {
            float* data = (float*)address.ToPointer();
            for (int i = 0; i < value.Length; i++)
            {
                data[i] = value[i];
            }
        }
        /// <summary>
        /// Writes a 3-component floating-point to the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <param name="value">The vector components to write.</param>
        public static void WriteVector3(IntPtr address, float[] value)
        {
            float* data = (float*)address.ToPointer();
            data[0] = value[0];
            data[1] = value[1];
            data[2] = value[2];
        }

        /// <summary>
        /// Sets a single bit in the 32-bit value at the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <param name="bit">The bit index to change.</param>
        public static void SetBit(IntPtr address, int bit)
        {
            if (bit < 0 || bit > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(bit), "The bit index has to be between 0 and 31");
            }

            int* data = (int*)address.ToPointer();
            *data |= 1 << bit;
        }
        /// <summary>
        /// Clears a single bit in the 32-bit value at the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <param name="bit">The bit index to change.</param>
        public static void ClearBit(IntPtr address, int bit)
        {
            if (bit < 0 || bit > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(bit), "The bit index has to be between 0 and 31");
            }

            int* data = (int*)address.ToPointer();
            *data &= ~(1 << bit);
        }
        /// <summary>
        /// Checks a single bit in the 32-bit value at the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The memory address to access.</param>
        /// <param name="bit">The bit index to check.</param>
        /// <returns><see langword="true" /> if the bit is set, <see langword="false" /> if it is unset.</returns>
        public static bool IsBitSet(IntPtr address, int bit)
        {
            if (bit < 0 || bit > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(bit), "The bit index has to be between 0 and 31");
            }

            int* data = (int*)address.ToPointer();
            return (*data & (1 << bit)) != 0;
        }

        public static string PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return string.Empty;
            }

            byte* data = (byte*)ptr.ToPointer();

            // Calculate length of null-terminated string
            int len = 0;
            while (data[len] != 0)
            {
                ++len;
            }

            return PtrToStringUTF8(ptr, len);
        }

        public static string PtrToStringUTF8(IntPtr ptr, int len)
        {
            if (len < 0)
            {
                throw new ArgumentException(null, nameof(len));
            }

            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            if (len == 0)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString((byte*)ptr.ToPointer(), len);
        }
    }
}
