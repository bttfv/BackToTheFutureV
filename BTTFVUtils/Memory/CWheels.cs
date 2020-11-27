namespace FusionLibrary.Memory
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CWheel
    {
        [FieldOffset(0x0008)] public float camber;
        [FieldOffset(0x0030)] public NativeVector3 offset;
        [FieldOffset(0x0164)] public float rotation; 
    }
}
