namespace FusionLibrary.Memory
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct fragPhysicsLODGroup
    {
        [FieldOffset(0x0010)] public fragPhysicsLOD* Lod1;
        [FieldOffset(0x0018)] public fragPhysicsLOD* Lod2;
        [FieldOffset(0x0020)] public fragPhysicsLOD* Lod3;
    }
}
