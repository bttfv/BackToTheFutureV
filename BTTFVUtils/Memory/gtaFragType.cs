namespace FusionLibrary.Memory
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct gtaFragType
    {
        [FieldOffset(0x00F0)] public fragPhysicsLODGroup* PhysicsLodGroup;
    }
}
