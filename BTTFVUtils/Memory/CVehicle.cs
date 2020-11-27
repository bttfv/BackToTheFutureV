namespace FusionLibrary.Memory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CVehicle
    {
        [FieldOffset(0x0030)] public fragInstGta* inst;
        [FieldOffset(0x0048)] public CEntityCustomization* customization;
        [FieldOffset(0x0B60)] public CWheel** wheels;
        [FieldOffset(0x0B68)] public int wheelCount;
        [FieldOffset(0x0340)] public float deluxoTransformation;
        [FieldOffset(0x0344)] public float deluxoFlyMode;
        [FieldOffset(0x0364)] public float deluxoWheelLerp;
    }
}
