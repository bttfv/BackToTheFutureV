namespace FusionLibrary.Memory
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct fragPhysicsLOD
    {
        [FieldOffset(0x00C0)] private char** groupNames;
        [FieldOffset(0x00C8)] public Group** Groups;

        [FieldOffset(0x011A)] public byte GroupsCount;

        [FieldOffset(0x011D)] public byte ChildrenCount;

        public string[] GetGroupNames()
        {
            string[] names = new string[GroupsCount];

            for (int i = 0; i < GroupsCount; i++)
            {
                names[i] = Marshal.PtrToStringAnsi(new System.IntPtr(groupNames[i]));
            }

            return names;
        }



        [StructLayout(LayoutKind.Explicit)]
        public struct Group
        {
            [FieldOffset(0x004E)] public byte BoundIndex;
        }
    }
}
