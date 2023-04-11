using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Native;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class WaybackPed
    {
        public PedReplica Replica { get; }

        public WaybackPedEvent Event { get; set; } = WaybackPedEvent.None;

        public bool SwitchPed { get; set; }

        public bool SwitchedClothes { get; set; }

        public bool SwitchedWeapons { get; set; }

        public WaybackPed(Ped ped)
        {
            Replica = new PedReplica(ped);

            if (ped.IsWalking)
                Event = WaybackPedEvent.Walking;

            if (ped.IsRunning)
                Event = WaybackPedEvent.Running;

            if (ped.IsJumping)
                Event = WaybackPedEvent.Jump;

            if (ped.IsInMeleeCombat)
                Event = WaybackPedEvent.MeleeAttack;

            if (ped.IsClimbing)
                Event = WaybackPedEvent.Climb;

            if (ped.IsSittingInVehicle() && ped.SeatIndex == VehicleSeat.Driver)
                Event = WaybackPedEvent.DrivingVehicle;

            if (ped.IsEnteringVehicle() && !ped.IsTaskActive(TaskType.InVehicleSeatShuffle))
                Event = WaybackPedEvent.EnteringVehicle;

            if (ped.IsEnteringVehicle() && ped.IsTaskActive(TaskType.InVehicleSeatShuffle))
                Event = WaybackPedEvent.ShufflingVehicle;

            if (ped.IsLeavingVehicle() || ped.IsJumpingOutOfVehicle)
                Event = WaybackPedEvent.LeavingVehicle;
        }

        public void Apply(Ped ped, Vehicle vehicle, PedReplica nextReplica, float adjustedRatio)
        {
            if (!vehicle.NotNullAndExists() || !ped.IsInVehicle(vehicle))
                nextReplica.ApplyTo(ped);

            if (Event == WaybackPedEvent.None)
                return;

            switch (Event)
            {
                case WaybackPedEvent.EnteringVehicle:
                    if (vehicle.NotNullAndExists() && !ped.IsEnteringVehicle())
                        ped.Task.EnterVehicle(vehicle);

                    break;
                case WaybackPedEvent.ShufflingVehicle:
                    if (vehicle.NotNullAndExists() && !ped.IsTaskActive(TaskType.InVehicleSeatShuffle))
                        ped.Task.ShuffleToNextVehicleSeat(vehicle);

                    break;
                case WaybackPedEvent.LeavingVehicle:
                    if (!ped.IsLeavingVehicle())
                        ped.Task.LeaveVehicle();

                    break;
                case WaybackPedEvent.DrivingVehicle:
                    if (vehicle.NotNullAndExists() && !ped.IsEnteringVehicle() && !ped.IsTaskActive(TaskType.InVehicleSeatShuffle) && ped.SeatIndex != VehicleSeat.Driver)
                        ped.SetIntoVehicle(vehicle, VehicleSeat.Driver);

                    break;
                case WaybackPedEvent.Jump:
                    if (ped.IsTaskActive(TaskType.Jump) || ped.IsTaskActive(TaskType.ScriptedAnimation) || ped.IsClimbing)
                        break;

                    ped.Task.Jump();
                    break;
                case WaybackPedEvent.MeleeAttack:
                    if (ped.IsTaskActive(TaskType.Jump) || ped.IsTaskActive(TaskType.ScriptedAnimation) || ped.IsClimbing)
                        break;

                    ped.Task.PlayAnimation("melee@unarmed@streamed_core_fps", MeleeAttacks.SelectRandomElement());
                    break;
                case WaybackPedEvent.Climb:
                    if (ped.IsTaskActive(TaskType.Jump) || ped.IsTaskActive(TaskType.ScriptedAnimation) || ped.IsClimbing)
                        break;

                    ped.Task.Climb();
                    break;
                case WaybackPedEvent.Running:
                    if (ped.IsTaskActive(TaskType.Jump) || ped.IsTaskActive(TaskType.ScriptedAnimation) || ped.IsClimbing)
                        break;

                    Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, ped, 1.15f);
                    ped.Task.RunTo(FusionUtils.Lerp(Replica.Position, nextReplica.Position, adjustedRatio), true);
                    break;
                case WaybackPedEvent.Walking:
                    if (ped.IsTaskActive(TaskType.Jump) || ped.IsTaskActive(TaskType.ScriptedAnimation) || ped.IsClimbing || ped.IsRunning)
                        break;

                    ped.TaskGoStraightTo(FusionUtils.Lerp(Replica.Position, nextReplica.Position, adjustedRatio), FusionUtils.Lerp(Replica.Speed, nextReplica.Speed, adjustedRatio), FusionUtils.Lerp(Replica.Heading, nextReplica.Heading, adjustedRatio));
                    break;
            }
        }
    }
}
