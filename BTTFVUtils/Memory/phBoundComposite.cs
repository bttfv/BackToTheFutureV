namespace FusionLibrary.Memory
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct phBoundComposite
    {
        [FieldOffset(0x0010)] public eBoundType type;
        [FieldOffset(0x0014)] public float boundingSphereRadius;
        [FieldOffset(0x0020)] public NativeVector3 boundingBoxMax;
        [FieldOffset(0x0030)] public NativeVector3 boundingBoxMin;
        [FieldOffset(0x0040)] public NativeVector3 boundingBoxCenter;
        [FieldOffset(0x0050)] public NativeVector3 center;

        [FieldOffset(0x0070)] public phBound** children;
        [FieldOffset(0x0078)] public NativeMatrix4x4* childrenTransformations1;
        [FieldOffset(0x0080)] public NativeMatrix4x4* childrenTransformations2;
        [FieldOffset(0x0088)] public phBoundAABB* childrenAABBs;
        [FieldOffset(0x0090)] public phBoundFlagEntry* childrenBoundFlags;
        [FieldOffset(0x0098)] public phBoundFlagEntry* childrenBoundFlags2;
        [FieldOffset(0x00A0)] public ushort childrenCount;
        [FieldOffset(0x00A2)] public ushort childrenArraySize;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    internal unsafe struct phBoundAABB
    {
        [FieldOffset(0x0000)] public NativeVector3 min;
        [FieldOffset(0x0010)] public NativeVector3 max;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x8)]
    internal unsafe struct phBoundFlagEntry
    {
        [FieldOffset(0x0000)] public uint unk1;
        [FieldOffset(0x0004)] public uint unk2;
    }
}
