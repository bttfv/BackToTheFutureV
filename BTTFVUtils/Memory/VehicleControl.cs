using System;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;

namespace FusionLibrary.Memory
{
    public struct WheelDimensions
    {
        public float TyreRadius;
        public float RimRadius;
        public float TyreWidth;
    };

    public unsafe class VehicleControl
    {
        private static int throttlePOffset;
        private static int brakePOffset;
        private static int handbrakeOffset;
        private static int steeringAngleOffset;
        private static int handlingOffset;
        private static int fuelLevelOffset;

        private static int wheelsPtrOffset;
        private static int numWheelsOffset;

        private static int wheelSteeringAngleOffset;
        private static int wheelAngularVelocityOffset;

        private static int deluxoTransformationOffset;
        private static int deluxoFlyModeOffset;

        static VehicleControl()
        {
            byte* addr = MemoryFunctions.FindPattern("\x74\x0A\xF3\x0F\x11\xB3\x1C\x09\x00\x00\xEB\x25", "xxxxxx????xx");
            throttlePOffset = addr == null ? 0 : *(int*)(addr + 6) + 0x10;
            brakePOffset = addr == null ? 0 : *(int*)(addr + 6) + 0x14;
            steeringAngleOffset = addr == null ? 0 : *(int*)(addr + 6) + 8;

            addr = MemoryFunctions.FindPattern("\x44\x88\xA3\x00\x00\x00\x00\x45\x8A\xF4", "xxx????xxx");
            handbrakeOffset = addr == null ? 0 : *(int*)(addr + 3);

            addr = MemoryFunctions.FindPattern("\x3C\x03\x0F\x85\x00\x00\x00\x00\x48\x8B\x41\x20\x48\x8B\x88", "xxxx????xxxxxxx");
            handlingOffset = addr == null ? 0 : *(int*)(addr + 0x16);

            addr = MemoryFunctions.FindPattern("\x3B\xB7\x48\x0B\x00\x00\x7D\x0D", "xx????xx");
            wheelsPtrOffset = addr == null ? 0 : *(int*)(addr + 2) - 8;
            numWheelsOffset = addr == null ? 0 : *(int*)(addr + 2);

            addr = MemoryFunctions.FindPattern("\x0F\x2F\x81\xBC\x01\x00\x00\x0F\x97\xC0\xEB\xDA", "xx???xxxxxxx");
            wheelSteeringAngleOffset = addr == null ? 0 : *(int*)(addr + 3);

            addr = MemoryFunctions.FindPattern("\x74\x26\x0F\x57\xC9", "xxxxx");
            fuelLevelOffset = addr == null ? 0 : *(int*)(addr + 8);

            addr = MemoryFunctions.FindPattern("\x45\x0f\x57\xc9\xf3\x0f\x11\x83\x60\x01\x00\x00\xf3\x0f\x5c", "xxx?xxx???xxxxx");
            wheelAngularVelocityOffset = addr == null ? 0 : (*(int*)(addr + 8)) + 0xc;

            addr = MemoryFunctions.FindPattern("\xF3\x0F\x11\xB3\x00\x00\x00\x00\x44\x88\x00\x00\x00\x00\x00\x48\x85\xC9", "xxxx????xx?????xxx");
            deluxoTransformationOffset = addr == null ? 0 : *(int*)(addr + 4);
            deluxoFlyModeOffset = deluxoTransformationOffset == 0 ? 0 : deluxoTransformationOffset + 4;
        }

        public static void SetWheelSize(Vehicle vehicle, float size)
        {
            var address = vehicle?.MemoryAddress;

            if (address == IntPtr.Zero)
                return;

            var CVeh_0x48 = *(UInt64*)(address + 0x48);
            var CVeh_0x48_0x370 = *(UInt64*)(CVeh_0x48 + 0x370);

            if ((UIntPtr)CVeh_0x48_0x370 == UIntPtr.Zero)
                return;

            *(float*)(CVeh_0x48_0x370 + 0x8) = size;
        }

        public static float GetWheelSize(Vehicle vehicle)
        {
            var address = vehicle?.MemoryAddress;

            if (address == IntPtr.Zero)
                return 1.0f;

            var CVeh_0x48 = *(UInt64*)(address + 0x48);
            var CVeh_0x48_0x370 = *(UInt64*)(CVeh_0x48 + 0x370);

            if ((UIntPtr)CVeh_0x48_0x370 == UIntPtr.Zero)
                return 1.0f;

            return *(float*)(CVeh_0x48_0x370 + 0x8);
        }

        public static ulong GetHandlingPtr(Vehicle vehicle)
        {
            if (handlingOffset == 0) return 0;
            ulong address = (ulong)vehicle.MemoryAddress;
            return *(ulong*)(address + (ulong)handlingOffset);
        }

        public static void SetSuspensionUpperLimit(Vehicle vehicle, float limit)
        {
            ulong ptr = GetHandlingPtr(vehicle);
            *(float*)(*(ulong*)ptr + 0xC8) = limit;
        }

        public static void SetSuspensionLowerLimit(Vehicle vehicle, float limit)
        {
            ulong ptr = GetHandlingPtr(vehicle);
            *(float*)(*(ulong*)ptr + 0xCC) = limit;
        }

        public static ulong GetWheelsPtr(Vehicle vehicle)
        {
            if (wheelsPtrOffset == 0) return 0;
            ulong address = (ulong)vehicle.MemoryAddress;
            return *(ulong*)(address + (ulong)wheelsPtrOffset);
        }

        public static sbyte GetNumWheels(Vehicle vehicle)
        {
            if (numWheelsOffset == 0) return 0;
            sbyte* address = (sbyte*)((ulong)vehicle.MemoryAddress + (ulong)numWheelsOffset);
            return *address;
        }

        public static void SetThrottle(Vehicle vehicle, float throttle)
        {
            if (throttlePOffset == 0) return;
            float* address = (float*)((ulong)vehicle.MemoryAddress + (ulong)throttlePOffset);
            *address = throttle;
        }

        public static float GetThrottle(Vehicle vehicle)
        {
            if (throttlePOffset == 0) return -1f;
            float* address = (float*)((ulong)vehicle.MemoryAddress + (ulong)throttlePOffset);
            return *address;
        }

        public static void SetBrake(Vehicle vehicle, float brake)
        {
            if (brakePOffset == 0) return;
            float* address = (float*)((ulong)vehicle.MemoryAddress + (ulong)brakePOffset);
            *address = brake;
        }

        public static float GetBrake(Vehicle vehicle)
        {
            if (brakePOffset == 0) return 0.0f;
            float* address = (float*)((ulong)vehicle.MemoryAddress + (ulong)brakePOffset);
            return *address;
        }

        public static void SetFuelLevel(Vehicle vehicle, float fuelLevel)
        {
            if (fuelLevelOffset == 0) return;
            float* address = (float*)((ulong)vehicle.MemoryAddress + (ulong)fuelLevelOffset);
            *address = fuelLevel;
        }

        public static float GetFuelLevel(Vehicle vehicle)
        {
            if (fuelLevelOffset == 0) return 0;
            float* address = (float*)((ulong)vehicle.MemoryAddress + (ulong)fuelLevelOffset);
            return *address;
        }

        public static void SetSteeringAngle(Vehicle vehicle, float angle)
        {
            if (steeringAngleOffset == 0) return;
            float* address = (float*)((ulong)vehicle.MemoryAddress + (ulong)steeringAngleOffset);
            *address = angle;
        }

        public static float GetSteeringAngle(Vehicle vehicle)
        {
            if (steeringAngleOffset == 0) return -999f;
            float* address = (float*)((ulong)vehicle.MemoryAddress + (ulong)steeringAngleOffset);
            return *address;
        }

        public static void SetDeluxoTransformation(Vehicle v, float transformation)
        {
            if (deluxoTransformationOffset == 0) return;
            float* address = (float*)((ulong)v.MemoryAddress + (ulong)deluxoTransformationOffset);
            *address = transformation;
        }

        public static float GetDeluxoTransformation(Vehicle v)
        {
            if (deluxoTransformationOffset == 0) return -1f;
            float* address = (float*)((ulong)v.MemoryAddress + (ulong)deluxoTransformationOffset);
            return *address;
        }

        public static void SetDeluxoFlyMode(Vehicle v, float mode)
        {
            if (deluxoFlyModeOffset == 0) return;
            float* address = (float*)((ulong)v.MemoryAddress + (ulong)deluxoFlyModeOffset);
            *address = mode;
        }

        public static float GetDeluxoFlyMode(Vehicle v)
        {
            if (deluxoFlyModeOffset == 0) return -1f;
            float* address = (float*)((ulong)v.MemoryAddress + (ulong)deluxoFlyModeOffset);
            return *address;
        }

        public static float GetMaxSteeringAngle(Vehicle vehicle)
        {
            ulong handlingAddr = GetHandlingPtr(vehicle);
            if (handlingAddr == 0) return 0f;
            float* addr = (float*)(handlingAddr + (ulong)0x0080);
            return *addr;
        }

        /*
         *  0 - front left
         *  1 - front right
         *  2 - rear left
         *  3 - rear right
         */
        public static float[] GetWheelRotationSpeeds(Vehicle handle)
        {
            ulong wheelPtr = GetWheelsPtr(handle);
            sbyte numWheels = GetNumWheels(handle);

            float[] speeds = new float[numWheels];

            if (wheelAngularVelocityOffset == 0) return speeds;

            for (sbyte i = 0; i < numWheels; i++)
            {
                ulong wheelAddr = *(ulong*)(wheelPtr + 0x008 * (ulong)i);
                speeds[i] = -*(float*)(wheelAddr + (ulong)wheelAngularVelocityOffset);
            }
            return speeds;
        }

        public static WheelDimensions[] GetWheelDimensions(Vehicle handle)
        {
            ulong wheelPtr = GetWheelsPtr(handle);
            sbyte numWheels = GetNumWheels(handle);

            WheelDimensions[] dimensionsSet = new WheelDimensions[numWheels];
            ulong offTyreRadius = 0x110;
            ulong offRimRadius = 0x114;
            ulong offTyreWidth = 0x118;

            for (sbyte i = 0; i < numWheels; i++)
            {
                ulong wheelAddr = *(ulong*)(wheelPtr + 0x008 * (ulong)i);

                WheelDimensions dimensions = new WheelDimensions();
                dimensions.TyreRadius = *(float*)(wheelAddr + offTyreRadius);
                dimensions.RimRadius = *(float*)(wheelAddr + offRimRadius);
                dimensions.TyreWidth = *(float*)(wheelAddr + offTyreWidth);
                dimensionsSet[i] = dimensions;
            }

            return dimensionsSet;
        }

        public static float[] GetTyreSpeeds(Vehicle handle)
        {
            int numWheels = GetNumWheels(handle);
            float[] rotationSpeed = GetWheelRotationSpeeds(handle);
            WheelDimensions[] dimensionsSet = GetWheelDimensions(handle);
            float[] wheelSpeeds = new float[numWheels];

            for (int i = 0; i < numWheels; i++)
            {
                wheelSpeeds[i] = rotationSpeed[i] * dimensionsSet[i].TyreRadius;
            }

            return wheelSpeeds;
        }

        public static float[] GetWheelSteeringAngles(Vehicle vehicle)
        {
            ulong wheelPtr = GetWheelsPtr(vehicle);
            sbyte numWheels = GetNumWheels(vehicle);

            float[] array = new float[numWheels];

            if (wheelSteeringAngleOffset == 0) return array;

            for(sbyte i = 0; i < numWheels; i++)
            {
                ulong wheelAddr = *(ulong*)(wheelPtr + 0x008 * (ulong)i);
                array[i] = *(float*)(wheelAddr + (ulong)wheelSteeringAngleOffset);
            }

            return array;
        }

        public static float GetLargestSteeringAngle(Vehicle v)
        {
            float largestAngle = 0.0f;
            float[] angles = GetWheelSteeringAngles(v);

            foreach(float angle in angles)
            {
                if (Math.Abs(angle) > Math.Abs(largestAngle))
                {
                    largestAngle = angle;
                }
            }

            return largestAngle;
        }

        public static float CalculateReduction(Vehicle vehicle)
        {
            Vector3 vel = vehicle.Velocity;
            Vector3 pos = vehicle.Position;
            Vector3 motion = vehicle.GetOffsetPosition(new Vector3(pos.X + vel.X, pos.Y + vel.Y, pos.Z + vel.Z));
            //if (motion.Y > 3)
            //{
            //    mult = (0.15f + ((float)Math.Pow((1.0f / 1.13f), ((float)Math.Abs(motion.Y) - 7.2f))));
            //    if (mult != 0) { mult = (float)Math.Floor(mult * 1000) / 1000; }
            //    if (mult > 1) { mult = 1; }
            //}
            //mult = (1 + (mult - 1) * 1.0f);

            var remap = vel.Length().Remap(0, 30, 0, 0.6f);
            return remap > 0.6f ? 0.6f : remap;
        }

        public static float CalculateDesiredHeading(Vehicle vehicle, float steeringAngle, float steeringMax, float desiredHeading, float reduction)
        {
            float correction = desiredHeading * reduction;

            Vector3 speedVector = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, vehicle, true);

            if (Math.Abs(speedVector.Y) > 3.0f)
            {
                Vector3 velocityWorld = vehicle.Velocity;
                Vector3 positionWorld = vehicle.Position;
                Vector3 travelWorld = velocityWorld + positionWorld;

                float steeringAngleRelX = speedVector.Y * -(float)Math.Sin(steeringAngle);
                float steeringAngleRelY = speedVector.Y * (float)Math.Cos(steeringAngle);
                Vector3 steeringWorld = vehicle.GetOffsetPosition(new Vector3(steeringAngleRelX, steeringAngleRelY, 0.0f));

                Vector3 travelNorm = (travelWorld - positionWorld).Normalized;
                Vector3 steerNorm = (steeringWorld - positionWorld).Normalized;
                float travelDir = (float)Math.Atan2(travelNorm.Y, travelNorm.X) + desiredHeading * reduction;
                float steerDir = (float)Math.Atan2(steerNorm.Y, steerNorm.X);

                correction = 2.0f * (float)Math.Atan2(Math.Sin(travelDir - steerDir), (float)Math.Cos(travelDir - steerDir));
            }
            if (correction > steeringMax)
                correction = steeringMax;
            if (correction < -steeringMax)
                correction = -steeringMax;

            return correction;
        }

        public static void GetControls(float limitRadians, out bool handbrake, out float throttle, out bool brake, out float steer)
        {
            handbrake = Game.IsControlJustPressed(Control.VehicleHandbrake);
            throttle = -Game.GetDisabledControlValueNormalized(Control.MoveUp);

            brake = Game.IsControlJustPressed(Control.MoveDown);
            float left = Game.GetDisabledControlValueNormalized(Control.MoveLeft).Remap(0, 1f, 0, limitRadians);
            steer = -left;
        }
    }
}
