using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    [Serializable]
    internal class WaybackVehicle
    {
        public VehicleReplica Replica { get; }

        public bool IsTimeMachine { get; }

        public ModsPrimitive Mods { get; }
        public PropertiesHandler Properties { get; }

        public WaybackVehicleEvent Event { get; set; } = WaybackVehicleEvent.None;
        public int TimeTravelDelay { get; set; }

        public WaybackVehicle(TimeMachine timeMachine, WaybackVehicleEvent wvEvent = WaybackVehicleEvent.None, int timeTravelDelay = 0)
        {
            Replica = new VehicleReplica(timeMachine, SpawnFlags.NoOccupants);

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();

            Event = wvEvent;
            TimeTravelDelay = timeTravelDelay;
        }

        public WaybackVehicle(Vehicle vehicle)
        {
            Replica = new VehicleReplica(vehicle, SpawnFlags.NoOccupants);

            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

            if (timeMachine == null)
                return;

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();
        }

        private Vehicle Spawn()
        {
            Vehicle vehicle = Replica.Spawn(SpawnFlags.NoVelocity | SpawnFlags.NoOccupants);

            if (!IsTimeMachine)
                return vehicle;

            TimeMachine timeMachine = TimeMachineHandler.Create(vehicle);

            Mods.ApplyTo(timeMachine);
            Properties.ApplyTo(timeMachine);

            return vehicle;
        }

        public Vehicle TryFindOrSpawn(VehicleReplica nextReplica, float adjustedRatio)
        {
            Vehicle vehicle;

            if (nextReplica == null)
                vehicle = World.GetClosestVehicle(Replica.Position, 3, Replica.Model);
            else
                vehicle = World.GetClosestVehicle(FusionUtils.Lerp(Replica.Position, nextReplica.Position, adjustedRatio), 3, Replica.Model);

            if (!vehicle.NotNullAndExists() || FusionUtils.PlayerPed == vehicle)
                vehicle = Spawn();

            return vehicle;
        }

        public Vehicle Apply(VehicleReplica nextReplica, float adjustedRatio, Ped ped = null)
        {
            Vehicle vehicle = ped?.GetUsingVehicle();

            if (!vehicle.NotNullAndExists())
                vehicle = TryFindOrSpawn(nextReplica, adjustedRatio);

            if (!vehicle.NotNullAndExists())
                return null;

            SpawnFlags spawnFlags = SpawnFlags.NoOccupants;

            if (ped.NotNullAndExists() && (ped.IsEnteringVehicle() || ped.IsLeavingVehicle()))
                spawnFlags |= SpawnFlags.NoPosition;

            if (nextReplica == null)
                Replica.ApplyTo(vehicle, spawnFlags);
            else
                Replica.ApplyTo(vehicle, spawnFlags, nextReplica, adjustedRatio);

            if (!IsTimeMachine)
                return vehicle;

            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

            if (!timeMachine.NotNullAndExists())
                return vehicle;

            Mods.ApplyToWayback(timeMachine);
            Properties.ApplyToWayback(timeMachine);

            if (Event == WaybackVehicleEvent.None)
                return vehicle;

            switch (Event)
            {
                case WaybackVehicleEvent.OnSparksEnded:
                    timeMachine.Events.OnSparksEnded?.Invoke(TimeTravelDelay);
                    break;
                case WaybackVehicleEvent.OpenCloseReactor:
                    timeMachine.Events.SetOpenCloseReactor?.Invoke();
                    break;
                case WaybackVehicleEvent.RefuelReactor:
                    timeMachine.Events.SetRefuel?.Invoke(ped);
                    break;
                case WaybackVehicleEvent.LightningStrike:
                    timeMachine.Events.StartLightningStrike?.Invoke(0);
                    break;
            }

            return vehicle;
        }
    }
}
