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
    internal class WaybackPedReplica
    {
        private static string[] MeleeAttacks = new string[] { "walking_punch", "running_punch", "long_0_punch", "heavy_punch_a", "heavy_punch_b", "heavy_punch_b_var_1", "short_0_punch" };

        public DateTime Time { get; }
        public int Timestamp { get; }

        public Vector3 Position { get; }
        public float Heading { get; }
        public float Speed { get; }
        public bool Visible { get; }

        public Weapon Weapon { get; }

        public WaybackPedEvent Event { get; } = WaybackPedEvent.Walking;

        public WaybackMachineReplica WaybackMachineReplica { get; } = null;

        public WaybackPedReplica(Ped ped, int startGameTime)
        {
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
                WaybackMachineReplica = new WaybackMachineReplica(ped.GetEnteringVehicle());
                return;
            }

            if (ped.IsLeavingVehicle())
            {
                Event = WaybackPedEvent.LeavingVehicle;
                WaybackMachineReplica = new WaybackMachineReplica(ped.LastVehicle);
                return;
            }

            if (ped.IsSittingInVehicle())
            {
                Event = WaybackPedEvent.DrivingVehicle;
                WaybackMachineReplica = new WaybackMachineReplica(ped.GetUsingVehicle());
                return;
            }
        }

        public void Apply(Ped ped, WaybackPedReplica nextReplica, float adjustedRatio)
        {
            if (Weapon != ped.Weapons.Current)
                ped.Weapons.Select(Weapon);

            if (Visible != ped.IsVisible)
                ped.IsVisible = Visible;

            Vehicle vehicle = null;

            if (WaybackMachineReplica != null)
            {
                if (ped.IsSittingInVehicle())
                {
                    vehicle = ped.CurrentVehicle;

                    WaybackMachineReplica.Apply(vehicle, ped, adjustedRatio, nextReplica);
                }
                else
                    vehicle = WaybackMachineReplica.TryFindOrSpawn(adjustedRatio, nextReplica);
            }

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
