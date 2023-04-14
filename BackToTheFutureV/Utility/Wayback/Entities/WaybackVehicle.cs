﻿using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class WaybackVehicle
    {
        public VehicleReplica Replica { get; }

        public WaybackVehicleEvent Event { get; set; } = WaybackVehicleEvent.None;

        public int TimeTravelDelay { get; set; }

        public bool IsTimeMachine { get; }

        public PropertiesHandler Properties { get; }

        public ModsPrimitive Mods { get; }

        public WaybackVehicle(Vehicle vehicle)
        {
            Replica = new VehicleReplica(vehicle);

            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

            if (timeMachine == null)
            {
                return;
            }

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();
        }

        private Vehicle Spawn()
        {
            SpawnFlags _spawnFlag = SpawnFlags.Default;

            if (Replica.Model == ModelHandler.DMC12)
                _spawnFlag = SpawnFlags.NoMods;

            Vehicle vehicle = Replica.Spawn(_spawnFlag);

            vehicle.SetPlayerLights(true);

            if (!IsTimeMachine)
            {
                return vehicle;
            }

            TimeMachine timeMachine = TimeMachineHandler.Create(vehicle);

            Properties.ApplyTo(timeMachine);
            Mods.ApplyTo(timeMachine);

            return vehicle;
        }

        private Vehicle TryFindOrSpawn(VehicleReplica nextReplica, float adjustedRatio)
        {
            Vehicle vehicle = null;

            if (IsTimeMachine)
            {
                TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromGUID(Properties.GUID);

                if (timeMachine.NotNullAndExists())
                    vehicle = timeMachine.Vehicle;
            }
            else if (nextReplica == null)
            {
                vehicle = World.GetClosestVehicle(Replica.Position, 1f, Replica.Model);
            }
            else
            {
                vehicle = World.GetClosestVehicle(FusionUtils.Lerp(Replica.Position, nextReplica.Position, adjustedRatio), 1f, Replica.Model);
            }

            if (!vehicle.NotNullAndExists())
            {
                vehicle = Spawn();
            }

            return vehicle;
        }

        public Vehicle Apply(VehicleReplica nextReplica, float adjustedRatio, Ped ped = null)
        {
            if (FusionUtils.PlayerPed.DistanceToSquared2D(Replica.Position) > 25000)
            {
                Replica.Position.RequestCollision();
            }

            Vehicle vehicle = ped?.GetUsingVehicle();

            if (!vehicle.NotNullAndExists())
            {
                vehicle = TryFindOrSpawn(nextReplica, adjustedRatio);
            }

            if (!vehicle.NotNullAndExists())
            {
                return null;
            }

            SpawnFlags spawnFlags = SpawnFlags.Default;

            if (ped.NotNullAndExists() && (ped.IsEnteringVehicle() || ped.IsLeavingVehicle()))
            {
                spawnFlags |= SpawnFlags.NoPosition;
            }

            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

            if (IsTimeMachine && timeMachine.NotNullAndExists())
            {
                spawnFlags |= SpawnFlags.NoMods;
            }

            if (IsTimeMachine && timeMachine.NotNullAndExists() && Properties.IsOnTracks)
            {
                timeMachine.Events.SetTrainSpeed?.Invoke(Replica.Speed * (Replica.RunningDirection != RunningDirection.Backward ? 1 : -1));
            }
            else
            {
                if (nextReplica == null)
                {
                    Replica.ApplyTo(vehicle, spawnFlags);
                }
                else
                {
                    Replica.ApplyTo(vehicle, spawnFlags, nextReplica, adjustedRatio);
                }
            }

            if (!IsTimeMachine)
            {
                return vehicle;
            }

            if (timeMachine == null)
            {
                timeMachine = TimeMachineHandler.Create(vehicle);

                Properties.ApplyTo(timeMachine);
                Mods.ApplyTo(timeMachine);
            }

            Properties.ApplyToWayback(timeMachine);

            if (Event == WaybackVehicleEvent.RcHandbrakeOn)
            {
                vehicle.IsBurnoutForced = true;
                vehicle.CanTiresBurst = false;
            }

            if (Event == WaybackVehicleEvent.RcHandbrakeOff)
            {
                vehicle.IsBurnoutForced = false;
                vehicle.CanTiresBurst = true;
            }

            if (Event == WaybackVehicleEvent.TimeTravel)
            {
                timeMachine.Events.OnSparksEnded?.Invoke(TimeTravelDelay);
            }

            return vehicle;
        }
    }
}
