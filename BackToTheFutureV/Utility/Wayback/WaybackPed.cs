using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System;
using System.IO;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    [Serializable]
    internal class WaybackPed
    {
        public Guid Owner { get; }

        public DateTime Time { get; }
        public float FrameTime { get; }

        public Vector3 Position { get; }
        public float Heading { get; }
        public float Speed { get; }
        public bool Visible { get; }

        public WeaponHash Weapon { get; }

        public WaybackPedEvent Event { get; set; } = WaybackPedEvent.Walking;

        public WaybackVehicle WaybackVehicle { get; set; } = null;

        public WaybackPed(Guid owner, Ped ped)
        {
            Owner = owner;
            Time = FusionUtils.CurrentTime;
            FrameTime = Game.LastFrameTime;

            Visible = ped.IsVisible;
            Weapon = ped.Weapons.Current;

            if (ped.IsJumping)
                Event = WaybackPedEvent.Jump;

            if (ped.IsInMeleeCombat)
                Event = WaybackPedEvent.MeleeAttack;

            if (ped.IsSittingInVehicle())
                Event = WaybackPedEvent.DrivingVehicle;

            if (ped.IsEnteringVehicle())
                Event = WaybackPedEvent.EnteringVehicle;

            if (ped.IsLeavingVehicle() || ped.IsJumpingOutOfVehicle)
                Event = WaybackPedEvent.LeavingVehicle;

            Vehicle vehicle = ped.GetUsingVehicle();

            if (vehicle.NotNullAndExists())
                WaybackVehicle = new WaybackVehicle(vehicle);

            if (Event != WaybackPedEvent.Walking)
                return;

            Position = ped.Position;
            Heading = ped.Heading;
            Speed = ped.Speed;
        }

        public void Apply(Ped ped, WaybackPed nextReplica)
        {
            float adjustedRatio = 1 - (FrameTime / Game.LastFrameTime);

            Vehicle vehicle = WaybackVehicle?.Apply(ped, nextReplica.WaybackVehicle == null ? null : nextReplica.WaybackVehicle.VehicleReplica, adjustedRatio);

            if (Event == WaybackPedEvent.Clone)
                return;

            if (vehicle.NotNullAndExists() && !vehicle.IsVisible)
                ped.IsVisible = false;
            else if (ped.IsVisible != Visible)
                ped.IsVisible = Visible;

            if (Weapon != ped.Weapons.Current)
                ped.Weapons.Select(Weapon);

            switch (Event)
            {
                case WaybackPedEvent.EnteringVehicle:
                    if (!ped.IsEnteringVehicle() && vehicle.NotNullAndExists())
                        ped.Task.EnterVehicle(vehicle, VehicleSeat.Driver);

                    break;
                case WaybackPedEvent.LeavingVehicle:
                    if (!ped.IsLeavingVehicle())
                        ped.Task.LeaveVehicle();

                    break;
                case WaybackPedEvent.DrivingVehicle:
                    if (ped.NotNullAndExists() && !ped.IsSittingInVehicle(vehicle))
                        ped.SetIntoVehicle(vehicle, VehicleSeat.Driver);

                    break;
                case WaybackPedEvent.Jump:
                    if (ped.IsTaskActive(TaskType.Jump) | ped.IsTaskActive(TaskType.Melee) | ped.IsTaskActive(TaskType.ScriptedAnimation))
                        break;

                    ped.Task.Jump();
                    break;
                case WaybackPedEvent.MeleeAttack:
                    if (ped.IsTaskActive(TaskType.Jump) | ped.IsTaskActive(TaskType.Melee) | ped.IsTaskActive(TaskType.ScriptedAnimation))
                        break;

                    ped.Task.PlayAnimation("melee@unarmed@streamed_core_fps", MeleeAttacks.SelectRandomElement());
                    break;
                case WaybackPedEvent.Walking:
                    if (ped.IsTaskActive(TaskType.Jump) | ped.IsTaskActive(TaskType.Melee) | ped.IsTaskActive(TaskType.ScriptedAnimation))
                        break;

                    ped.TaskGoStraightTo(FusionUtils.Lerp(Position, nextReplica.Position, adjustedRatio), FusionUtils.Lerp(Speed, nextReplica.Speed, adjustedRatio), FusionUtils.Lerp(Heading, nextReplica.Heading, adjustedRatio), -1, 0.1f);
                    break;
            }
        }

        public static WaybackPed FromData(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                try
                {
                    return (WaybackPed)FusionUtils.BinaryFormatter.Deserialize(stream);
                }
                catch
                {
                    return null;
                }
            }
        }

        public static implicit operator byte[](WaybackPed command)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                FusionUtils.BinaryFormatter.Serialize(stream, command);
                return stream.ToArray();
            }
        }
    }
}
