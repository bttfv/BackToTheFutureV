using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.Enums;
using Control = GTA.Control;

namespace BackToTheFutureV
{
    [Serializable]
    internal class WaybackPed
    {
        public Guid Owner { get; }

        public DateTime Time { get; }
        public float Timestamp { get; }

        public Vector3 Position { get; }
        public float Heading { get; }
        public float Speed { get; }
        public bool Visible { get; }

        public WeaponHash Weapon { get; }

        public WaybackPedEvent Event { get; } = WaybackPedEvent.Walking;

        public WaybackVehicle WaybackVehicle { get; set; } = null;

        public WaybackPed(Guid owner, Ped ped, float startGameTime)
        {
            Owner = owner;
            Time = Utils.CurrentTime;
            Timestamp = Game.GameTime - startGameTime;

            Position = ped.Position;
            Heading = ped.Heading;
            Speed = ped.Speed;
            Visible = ped.IsVisible;

            Weapon = ped.Weapons.Current;

            if (ped.IsFullyOutVehicle())
            {
                if (Game.IsControlJustPressed(Control.Jump))
                {
                    Event = WaybackPedEvent.Jump;
                    return;
                }

                if (Game.IsControlJustPressed(Control.MeleeAttack1))
                {
                    Event = WaybackPedEvent.MeleeAttack;
                    return;
                }
            }

            if (ped.IsEnteringVehicle())
            {
                Event = WaybackPedEvent.EnteringVehicle;
                WaybackVehicle = new WaybackVehicle(ped.GetEnteringVehicle());
                return;
            }

            if (ped.IsLeavingVehicle())
            {
                Event = WaybackPedEvent.LeavingVehicle;
                WaybackVehicle = new WaybackVehicle(ped.LastVehicle);
                return;
            }

            if (ped.IsSittingInVehicle())
            {
                Event = WaybackPedEvent.DrivingVehicle;
                WaybackVehicle = new WaybackVehicle(ped.GetUsingVehicle());
                return;
            }
        }

        public void Apply(Ped ped, WaybackPed nextReplica, float adjustedRatio)
        {
            if (Weapon != ped.Weapons.Current)
                ped.Weapons.Select(Weapon);

            if (Visible != ped.IsVisible)
                ped.IsVisible = Visible;

            Vehicle vehicle = null;

            if (WaybackVehicle != null)
            {
                if (ped.IsSittingInVehicle())
                {
                    vehicle = ped.CurrentVehicle;

                    WaybackVehicle.Apply(vehicle, ped, nextReplica, adjustedRatio);
                }
                else
                    vehicle = WaybackVehicle.TryFindOrSpawn(adjustedRatio, nextReplica);

                if (vehicle.NotNullAndExists())
                    WaybackVehicle.Apply(vehicle, ped, nextReplica, adjustedRatio);
            }

            if (vehicle.NotNullAndExists())
                ped.IsVisible = vehicle.IsVisible;

            switch (Event)
            {
                case WaybackPedEvent.EnteringVehicle:
                    if (vehicle.NotNullAndExists())
                        ped.Task.EnterVehicle(vehicle, VehicleSeat.Driver);
                    break;
                case WaybackPedEvent.LeavingVehicle:
                    ped.Task.LeaveVehicle();
                    break;
                case WaybackPedEvent.DrivingVehicle:
                    if (ped.NotNullAndExists() && !ped.IsSittingInVehicle(vehicle))
                        ped.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                    break;
                case WaybackPedEvent.Jump:
                    ped.Task.Jump();
                    break;
                case WaybackPedEvent.MeleeAttack:
                    ped.Task.PlayAnimation("melee@unarmed@streamed_core_fps", MeleeAttacks.SelectRandomElement());
                    break;
                case WaybackPedEvent.Walking:
                    if (ped.IsTaskActive(TaskType.Jump) | ped.IsTaskActive(TaskType.Melee) | ped.IsTaskActive(TaskType.ScriptedAnimation))
                        break;
                    ped.TaskGoStraightTo(Utils.Lerp(Position, nextReplica.Position, adjustedRatio), Utils.Lerp(Speed, nextReplica.Speed, adjustedRatio), Utils.Lerp(Heading, nextReplica.Heading, adjustedRatio), -1, 0.1f);
                    break;
            }
        }
    }
}
