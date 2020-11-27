namespace FusionLibrary.Memory
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CEntitySkeleton
    {
        [FieldOffset(0x0000)] public crSkeletonData* skeletonData;
        [FieldOffset(0x0008)] public NativeMatrix4x4* entityMatrix;
        [FieldOffset(0x0010)] public NativeMatrix4x4* desiredBonesMatricesArray;
        [FieldOffset(0x0018)] public NativeMatrix4x4* currentBonesMatricesArray;
        [FieldOffset(0x0020)] public int bonesCount;
    }
}
