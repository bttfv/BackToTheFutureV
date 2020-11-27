namespace FusionLibrary.Memory
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size = 0x70)]
    internal unsafe struct phBound
    {
        [FieldOffset(0x0010)] public eBoundType type;
        [FieldOffset(0x0014)] public float boundingSphereRadius;
        [FieldOffset(0x0020)] public NativeVector3 boundingBoxMax;
        [FieldOffset(0x0030)] public NativeVector3 boundingBoxMin;
        [FieldOffset(0x0040)] public NativeVector3 boundingBoxCenter;
        [FieldOffset(0x0050)] public NativeVector3 center;
    }

    internal enum eBoundType : byte
    {
        Sphere = 0,
        Capsule = 1,
        Box = 3,
        Geometry = 4,
        BVH = 8, // Bounding Volume Hierarchy
        Composite = 10,
        Disc = 12,
        Cylinder = 13,
    }
}
