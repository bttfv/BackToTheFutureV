using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using static FusionLibrary.Enums;

namespace FusionLibrary.Extensions
{
    public static class EntityExtensions
    {
        public static bool NotNullAndExists(this Entity entity)
        {
            return entity != null && entity.Exists();
        }

        public static Vector3 RelativeVelocity(this Entity entity)
        {
            return Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, entity, true);
        }

        public static bool IsGoingForward(this Entity entity)
        {
            return entity.RelativeVelocity().Y >= 0;
        }

        public static void AttachToPhysically(this Entity entity1, Entity toEntity, Vector3 offset, Vector3 rotation)
        {
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY_PHYSICALLY, entity1, toEntity, 0, 0, offset.X, offset.Y, offset.Z, 0, 0, 0, rotation.X, rotation.Y, rotation.Z, 1000000.0f, true, true, false, false, 2);
        }

        public static float GetKineticEnergy(this Vehicle vehicle)
        {
            return 0.5f * HandlingData.GetByVehicleModel(vehicle.Model).Mass * (float)Math.Pow(vehicle.Speed, 2);
        }

        public static void TeleportTo(this Vehicle vehicle, Vector3 position)
        {
            position = vehicle.Position.TransferHeight(position);

            position.RequestCollision();
            vehicle.Position = position;
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

        public static bool IsTimeMachine2(this Vehicle vehicle)
        {
            return vehicle.Model == Utils.DMC12 && vehicle.Mods[VehicleModType.TrimDesign].Index != 0;
        }

        public static float GetMPHSpeed(this Vehicle vehicle)
        {
            return vehicle.Speed.ToMPH();
        }

        public static void SetMPHSpeed(this Vehicle vehicle, float value)
        {
            vehicle.ForwardSpeed = value.ToMS();
        }

        public static bool CanHoverTransform(this Vehicle vehicle)
        {
            return (vehicle.Bones["misc_c"].Index != 0 && vehicle.Bones["misc_c"].Index != -1 && vehicle.Bones["misc_f"].Index != 0 && vehicle.Bones["misc_f"].Index != -1);
        }

        public static void SetLightsBrightness(this Vehicle vehicle, float brightness)
        {
            Function.Call(Hash.SET_VEHICLE_LIGHT_MULTIPLIER, vehicle, brightness);
        }

        public static bool SameDirection(this Vehicle vehicle, Vehicle vehicle1)
        {
            return vehicle.Rotation.Z.MostlyNear(vehicle1.Rotation.Z);
        }

        public static void DeleteCompletely(this Vehicle vehicle)
        {
            if (vehicle.NotNullAndExists())
                foreach (var x in vehicle.Occupants)
                    if (x != Utils.PlayerPed)
                        x?.Delete();

            vehicle?.Delete();
        }

        public static bool IsFunctioning(this Vehicle vehicle)
        {
            return vehicle.NotNullAndExists() && vehicle.IsAlive && !vehicle.IsDead;
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

        public static bool DecreaseSpeedAndWait(this Vehicle vehicle, float by = 20)
        {
            Vector3 vel = vehicle.RelativeVelocity();

            if (vel.Y >= -2 && vel.Y <= 2)
                return true;

            vehicle.Speed -= by * Game.LastFrameTime;

            return false;
        }

        public static bool IsTrain(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash.IS_THIS_MODEL_A_TRAIN, vehicle.Model.Hash);
        }

        public static void SetTrainCruiseSpeed(this Vehicle vehicle, float speed)
        {
            if (!vehicle.IsTrain())
                return;

            Function.Call(Hash.SET_TRAIN_CRUISE_SPEED, vehicle, speed);
        }

        public static void SetTrainCruiseMPHSpeed(this Vehicle vehicle, float speed)
        {
            if (!vehicle.IsTrain())
                return;

            vehicle.SetTrainCruiseSpeed(speed.ToMS());
        }

        public static void SetTrainSpeed(this Vehicle vehicle, float speed)
        {
            if (!vehicle.IsTrain())
                return;

            Function.Call(Hash.SET_TRAIN_SPEED, vehicle, speed);
        }

        public static void SetTrainMPHSpeed(this Vehicle vehicle, float speed)
        {
            if (!vehicle.IsTrain())
                return;

            vehicle.SetTrainSpeed(speed.ToMS());
        }

        public static void Derail(this Vehicle vehicle)
        {
            if (!vehicle.IsTrain())
                return;
            
            Function.Call(Hash.SET_RENDER_TRAIN_AS_DERAILED, vehicle, true);
        }

        public static Vehicle GetTrainCarriage(this Vehicle vehicle, int index)
        {
            if (!vehicle.IsTrain())
                return null;

            return Function.Call<Vehicle>(Hash.GET_TRAIN_CARRIAGE, vehicle, index);
        }

        public static void SetTrainPosition(this Vehicle vehicle, Vector3 pos)
        {
            Function.Call(Hash.SET_MISSION_TRAIN_COORDS, vehicle, pos.X, pos.Y, pos.Z);
        }

        public static unsafe Vector3 GetBoneOriginalTranslation(this Vehicle vehicle, int index)
        {
            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            NativeVector3 v = veh->inst->archetype->skeleton->skeletonData->bones[index].translation;
            return v;
        }

        public static unsafe Quaternion GetBoneOriginalRotation(this Vehicle vehicle, int index)
        {
            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            NativeVector4 v = veh->inst->archetype->skeleton->skeletonData->bones[index].rotation;
            return v;
        }

        public static unsafe int GetBoneIndex(this Vehicle vehicle, string boneName)
        {
            if (vehicle == null)
                return -1;

            CVehicle* veh = (CVehicle*)vehicle.MemoryAddress;
            crSkeletonData* skelData = veh->inst->archetype->skeleton->skeletonData;
            uint boneCount = skelData->bonesCount;

            for (uint i = 0; i < boneCount; i++)
            {
                if (skelData->GetBoneNameForIndex(i) == boneName)
                    return unchecked((int)i);
            }

            return -1;
        }

        public static Dictionary<string, Vector3> GetWheelPositions(this Vehicle vehicle)
        {
            Dictionary<string, Vector3> ret = new Dictionary<string, Vector3>();

            if (vehicle.Bones["wheel_lf"].Index > 0)
                ret.Add("wheel_lf", vehicle.Bones["wheel_lf"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_rf"].Index > 0)
                ret.Add("wheel_rf", vehicle.Bones["wheel_rf"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_lr"].Index > 0)
                ret.Add("wheel_lr", vehicle.Bones["wheel_lr"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_rr"].Index > 0)
                ret.Add("wheel_rr", vehicle.Bones["wheel_rr"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_lm1"].Index > 0)
                ret.Add("wheel_lm1", vehicle.Bones["wheel_lm1"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_rm1"].Index > 0)
                ret.Add("wheel_rm1", vehicle.Bones["wheel_rm1"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_lm2"].Index > 0)
                ret.Add("wheel_lm2", vehicle.Bones["wheel_lm1"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            if (vehicle.Bones["wheel_rm2"].Index > 0)
                ret.Add("wheel_rm2", vehicle.Bones["wheel_rm2"].RelativePosition.GetSingleOffset(Coordinate.Z, -0.05f));

            return ret;
        }
    }
}
