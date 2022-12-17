using GTA.Math;
using System;

namespace BackToTheFutureV
{
    /// <summary>
    /// Wrapper for CWheel structure.
    /// </summary>
    internal class CWheel : INativeMemory
    {
        /// <summary>
        /// Gets the memory address where the <see cref="CWheel"/> is stored in memory.
        /// </summary>
        public IntPtr MemoryAddress { get; }

        /// <summary>
        /// Camber of the <see cref="CWheel"/>.
        /// </summary>
        /// <remarks>
        /// - Its not rototation but something else, acts very weird.
        /// </remarks>
        public Pointer<Vector3> Camber { get; }

        /// <summary>
        /// Relative position of the <see cref="CWheel"/>.
        /// </summary>
        public Pointer<Vector3> RelativePosition { get; }

        /// <summary>
        /// Wheel speed.
        /// </summary>
        /// <remarks>
        /// - Zero when is not touching surface.
        /// </remarks>
        public Pointer<float> Speed { get; }

        /// <summary>
        /// Wheel speed, in different units than <see cref="Speed"/>.
        /// </summary>
        /// <remarks>
        /// - Being set both while touches surface and when doesn't.
        /// <para>
        /// - Can be set while not touching air, otherwise game will overwrie it.
        /// </para>
        /// </remarks>
        public Pointer<float> ActualSpeed { get; }

        /// <summary>
        /// Brake force applied to the <see cref="CWheel"/>.
        /// </summary>
        public Pointer<float> BrakeForce { get; }

        /// <summary>
        /// Gets a new instance of <see cref="CWheel"/> from given memory address.
        /// </summary>
        /// <param name="memoryAddress">Address of the <see cref="CWheel"/>.</param>
        public CWheel(IntPtr memoryAddress)
        {
            MemoryAddress = memoryAddress;

            // TODO: Remove hardcoded offsets

            Camber = new Pointer<Vector3>(MemoryAddress, 0x0);
            RelativePosition = new Pointer<Vector3>(MemoryAddress, 0x30);
            Speed = new Pointer<float>(MemoryAddress, 0xC4);
            ActualSpeed = new Pointer<float>(MemoryAddress, 0x170);
            BrakeForce = new Pointer<float>(MemoryAddress, 0x1D0);
        }
    }
}
