using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Vehicles;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;

namespace BackToTheFutureV.Utility
{
    public static class VehicleExtensions
    {
        public static void DeleteCompletely(this Vehicle vehicle)
        {
            if (vehicle.NotNullAndExists())
                foreach (var x in vehicle.Occupants)
                    if (x != Main.PlayerPed)
                        x?.Delete();

            vehicle?.Delete();
        }

        public static bool IsFunctioning(this Vehicle vehicle)
        {
            return vehicle.NotNullAndExists() && vehicle.IsAlive && !vehicle.IsDead;
        }

        public static void SetVisible(this Vehicle vehicle, bool isVisible)
        {
            vehicle.IsVisible = isVisible;
            vehicle.IsCollisionEnabled = isVisible;
            vehicle.IsPositionFrozen = !isVisible;
            vehicle.IsEngineRunning = isVisible;

            foreach (var ped in vehicle.Occupants)
            {
                ped.IsVisible = isVisible;
                ped.CanBeDraggedOutOfVehicle = isVisible;
            }                
        }

        public static void TeleportTo(this Vehicle vehicle, Vector3 position)
        {
            position = vehicle.Position.TransferHeight(position);

            position.RequestCollision();
            vehicle.Position = position;
        }

        public static bool IsTimeMachine(this Vehicle vehicle)
        {
            return TimeMachineHandler.IsVehicleATimeMachine(vehicle);
        }

        public static TimeMachine TransformIntoTimeMachine(this Vehicle vehicle, WormholeType wormholeType = WormholeType.BTTF1)
        {
            return TimeMachineHandler.Create(vehicle, SpawnFlags.Default, wormholeType);
        }

        public static bool SameDirection(this Vehicle vehicle, Vehicle vehicle1)
        {
            return vehicle.Rotation.Z.MostlyNear(vehicle1.Rotation.Z);
        }

        public static bool IsGoingForward(this Vehicle vehicle)
        {
            return vehicle.RelativeVelocity().Y >= 0;
        }

        public static float GetMPHSpeed(this Vehicle vehicle)
        {
            return vehicle.Speed / 0.27777f / 1.60934f;
        }

        public static void SetMPHSpeed(this Vehicle vehicle, float value)
        {
            vehicle.ForwardSpeed = (value * 0.27777f * 1.60934f);
        }

        public static void SetLightsMode(this Vehicle vehicle, LightsMode lightsMode)
        {
            Function.Call(Hash._SET_VEHICLE_LIGHTS_MODE, vehicle, lightsMode);
            Function.Call(Hash.SET_VEHICLE_LIGHTS, vehicle, lightsMode);
        }

        public static void GetLightsState(this Vehicle vehicle, out bool lightsOn, out bool highbeamsOn)
        {
            bool _lightsOn;
            bool _highbeamsOn;

            unsafe
            {
                Function.Call(Hash.GET_VEHICLE_LIGHTS_STATE, vehicle, &_lightsOn, &_highbeamsOn);
            }

            lightsOn = _lightsOn;
            highbeamsOn = _highbeamsOn;
        }

        public static Vector3 RelativeVelocity(this Entity entity)
        {
            return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, entity, true);
        }

        public static bool DecreaseSpeedAndWait(this Vehicle vehicle, float by = 20)
        {
            Vector3 vel = vehicle.RelativeVelocity();

            if (vel.Y >= -2 && vel.Y <= 2)
                return true;

            vehicle.Speed -= by * Game.LastFrameTime;

            return false;
        }

        public static bool CanHoverTransform(this Vehicle vehicle)
        {
            return (vehicle.Bones["misc_c"].Index != 0 && vehicle.Bones["misc_c"].Index != -1 && vehicle.Bones["misc_f"].Index != 0 && vehicle.Bones["misc_f"].Index != -1);
        }

        public static void SetLightsBrightness(this Vehicle vehicle, float brightness)
        {
            Function.Call(Hash.SET_VEHICLE_LIGHT_MULTIPLIER, vehicle, brightness);
        }
    }
}
