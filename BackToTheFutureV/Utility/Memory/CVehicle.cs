using GTA;
using System;
using System.Collections.Generic;

namespace BackToTheFutureV
{
    /// <summary>
    /// Represents CVehicle structure.
    /// </summary>
    internal class CVehicle : INativeMemory
    {
        /// <summary>
        /// Gets the memory address where the <see cref="CVehicle"/> is stored in memory.
        /// </summary>
        public IntPtr MemoryAddress { get; }

        public Dictionary<VehicleWheelBoneId, CWheel> Wheels { get; }

        /// <summary>
        /// Gets a new instance of <see cref="CVehicle"/> of given <see cref="Vehicle"/> memory address.
        /// </summary>
        /// <param name="vehicle">Address of the <see cref="CVehicle"/>.</param>
        public CVehicle(Vehicle vehicle)
        {
            MemoryAddress = vehicle.MemoryAddress;

            Wheels = new Dictionary<VehicleWheelBoneId, CWheel>();

            foreach (VehicleWheel wheel in vehicle.Wheels)
            {
                Wheels.Add(wheel.BoneId, new CWheel(wheel.MemoryAddress));
            }
        }
    }
}
