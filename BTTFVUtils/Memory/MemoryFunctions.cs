using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FusionLibrary.Memory
{
    public enum Protection : uint
    {
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0x400
    }

    public unsafe class MemoryFunctions
    {
        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize,
            Protection flNewProtect, ref Protection lpflOldProtect);


        public static byte* FindPattern(string pattern, string mask)
        {
            ProcessModule module = Process.GetCurrentProcess().MainModule;

            ulong address = (ulong)module.BaseAddress.ToInt64();
            ulong endAddress = address + (ulong)module.ModuleMemorySize;

            for (; address < endAddress; address++)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (mask[i] != '?' && ((byte*)address)[i] != pattern[i])
                    {
                        break;
                    }
                    else if (i + 1 == pattern.Length)
                    {
                        return (byte*)address;
                    }
                }
            }

            return null;
        }

        public static void SetHeapLimit(int mb)
        {
            // From Heap Limit Adjuster

            int* loc = (int*)(FindPattern("\x83\xC8\x01\x48\x8D\x0D\x00\x00\x00\x00\x41\xB1\x01\x45\x33\xC0", "xxxxxx????xxxxxx") + 17);

            Protection oldProt = Protection.PAGE_EXECUTE;
            VirtualProtect(new IntPtr(loc), 4, Protection.PAGE_EXECUTE_READWRITE, ref oldProt);

            *loc = mb * 1024 * 1024;

            VirtualProtect(new IntPtr(loc), 4, oldProt, ref oldProt);
        }
    }
}
