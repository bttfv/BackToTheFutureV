namespace FusionLibrary.Memory
{
    using System;
    using System.Runtime.InteropServices;

    internal static unsafe class GameFunctions
    {
        public delegate int GetBoundIndexForBoneDelegate(fragInstGta* fragInst, int boneIndex);


        public static GetBoundIndexForBoneDelegate GetBoundIndexForBone { get; private set; }


        internal static bool Init()
        {
            byte* address = MemoryFunctions.FindPattern("\x85\xD2\x78\x44\x4C\x8B\x49\x68\x4D\x85\xC9\x74\x29\x49\x8B\x81\x00\x00\x00\x00", "xxxxxxxxxxxxxxxx????");
            if (AssertAddress(address, nameof(GetBoundIndexForBone)))
            {
                GetBoundIndexForBone = Marshal.GetDelegateForFunctionPointer<GetBoundIndexForBoneDelegate>((IntPtr)address);
            }

            return !anyAssertFailed;
        }

        private static bool anyAssertFailed = false;
        private static bool AssertAddress(byte* address, string name)
        {
            if (address == null)
            {
                anyAssertFailed = true;
                return false;
            }

            return true;
        }
    }
}
