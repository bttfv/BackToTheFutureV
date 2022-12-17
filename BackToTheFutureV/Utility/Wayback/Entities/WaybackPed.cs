using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class WaybackPed
    {
        public PedReplica Replica { get; }

        public WaybackPedEvent Event { get; set; } = WaybackPedEvent.None;

        public bool SwitchPed { get; set; }

        public WaybackPed(Ped ped)
        {
            Replica = new PedReplica(ped);

            if (ped.IsWalking)
            {
                Event = WaybackPedEvent.Walking;
            }

            if (ped.IsJumping)
            {
                Event = WaybackPedEvent.Jump;
            }

            /*if (ped.IsInMeleeCombat)
            {
                Event = WaybackPedEvent.MeleeAttack;
            }*/

            if (ped.IsClimbing)
            {
                Event = WaybackPedEvent.Climb;
            }

            if (ped.IsSittingInVehicle())
            {
                Event = WaybackPedEvent.DrivingVehicle;
            }

            if (ped.IsEnteringVehicle())
            {
                Event = WaybackPedEvent.EnteringVehicle;
            }

            if (ped.IsLeavingVehicle() || ped.IsJumpingOutOfVehicle)
            {
                Event = WaybackPedEvent.LeavingVehicle;
            }
        }

        public void Apply(Ped ped, Vehicle vehicle, PedReplica nextReplica, float adjustedRatio)
        {
            if (Event == WaybackPedEvent.None)
            {
                return;
            }

            switch (Event)
            {
                case WaybackPedEvent.EnteringVehicle:
                    if (vehicle.NotNullAndExists() && !ped.IsEnteringVehicle())
                    {
                        ped.Task.EnterVehicle(vehicle, VehicleSeat.Driver);
                    }

                    break;
                case WaybackPedEvent.LeavingVehicle:
                    if (!ped.IsLeavingVehicle())
                    {
                        ped.Task.LeaveVehicle();
                    }

                    break;
                case WaybackPedEvent.DrivingVehicle:
                    if (vehicle.NotNullAndExists() && !ped.IsSittingInVehicle(vehicle))
                    {
                        ped.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                    }

                    break;
                case WaybackPedEvent.Jump:
                    if (ped.IsTaskActive(TaskType.Jump) | ped.IsTaskActive(TaskType.Melee) | ped.IsClimbing)
                    {
                        break;
                    }

                    ped.Task.Jump();
                    break;
                case WaybackPedEvent.MeleeAttack:
                    if (ped.IsTaskActive(TaskType.Jump) | ped.IsTaskActive(TaskType.Melee) | ped.IsClimbing)
                    {
                        break;
                    }

                    ped.Task.PlayAnimation("melee@unarmed@streamed_core_fps", MeleeAttacks.SelectRandomElement());
                    break;
                case WaybackPedEvent.Climb:
                    if (ped.IsTaskActive(TaskType.Jump) | ped.IsTaskActive(TaskType.Melee) | ped.IsClimbing)
                    {
                        break;
                    }

                    ped.Task.Climb();
                    break;
                case WaybackPedEvent.Walking:
                    if (ped.IsTaskActive(TaskType.Jump) | ped.IsTaskActive(TaskType.Melee) | ped.IsClimbing)
                    {
                        break;
                    }

                    ped.TaskGoStraightTo(FusionUtils.Lerp(Replica.Position, nextReplica.Position, adjustedRatio), FusionUtils.Lerp(Replica.Speed, nextReplica.Speed, adjustedRatio), FusionUtils.Lerp(Replica.Heading, nextReplica.Heading, adjustedRatio));
                    break;
            }
        }
    }
}
