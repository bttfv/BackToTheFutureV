namespace FusionLibrary.Memory
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct fragInstGta
    {
        [FieldOffset(0x0010)] public phArchetypeDamp* archetype;

        [FieldOffset(0x0078)] public gtaFragType* FragType;

        [FieldOffset(0x00B8)] public uint CurrentPhysicsLod;
    }
}
