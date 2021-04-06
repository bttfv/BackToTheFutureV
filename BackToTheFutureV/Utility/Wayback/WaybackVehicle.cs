using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.Enums;

namespace BackToTheFutureV
{
    [Serializable]
    internal class WaybackVehicle
    {
        public VehicleReplica Vehicle { get; }

        public bool IsTimeMachine { get; }

        public ModsPrimitive Mods { get; }
        public PropertiesHandler Properties { get; }

        public WaybackVehicleEvent Event { get; set; } = WaybackVehicleEvent.None;
        public int TimeTravelDelay { get; set; }

        public WaybackVehicle(TimeMachine timeMachine, WaybackVehicleEvent wvEvent = WaybackVehicleEvent.None, int timeTravelDelay = 0)
        {
            Vehicle = new VehicleReplica(timeMachine, SpawnFlags.NoOccupants);

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();

            Event = wvEvent;
            TimeTravelDelay = timeTravelDelay;
        }

        public WaybackVehicle(Vehicle vehicle)
        {
            Vehicle = new VehicleReplica(vehicle, SpawnFlags.NoOccupants);

            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

            if (timeMachine == null)
                return;

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();
        }

        private Vehicle Spawn()
        {
            Vehicle vehicle = Vehicle.Spawn(SpawnFlags.NoVelocity | SpawnFlags.NoOccupants);

            if (!IsTimeMachine)
                return vehicle;

            TimeMachine timeMachine = TimeMachineHandler.Create(vehicle);

            Mods.ApplyTo(timeMachine);
            Properties.ApplyTo(timeMachine);

            return vehicle;
        }

        public Vehicle TryFindOrSpawn(VehicleReplica vehicleReplica, float adjustedRatio)
        {
            Vehicle vehicle = World.GetClosestVehicle(Utils.Lerp(Vehicle.Position, vehicleReplica.Position, adjustedRatio), 1, Vehicle.Model);

            if (!vehicle.NotNullAndExists())
                vehicle = Spawn();

            return vehicle;
        }

        public Vehicle Apply(Ped ped, WaybackVehicle nextReplica, float adjustedRatio)
        {
            if (nextReplica == null)
                nextReplica = this;

            Vehicle vehicle = ped.GetUsingVehicle();

            if (!vehicle.NotNullAndExists())
                vehicle = TryFindOrSpawn(nextReplica.Vehicle, adjustedRatio);

            if (!vehicle.NotNullAndExists())
                return vehicle;

            TimeMachine timeMachine = null;

            if (IsTimeMachine)
            {
                timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

                if (timeMachine.NotNullAndExists() && timeMachine.Properties.TimeTravelPhase == TimeTravelPhase.Reentering)
                    return vehicle;
            }

            if (ped.IsEnteringVehicle() || ped.IsLeavingVehicle())
                Vehicle.ApplyTo(vehicle, SpawnFlags.NoOccupants | SpawnFlags.ForcePosition, nextReplica.Vehicle, adjustedRatio);
            else
                Vehicle.ApplyTo(vehicle, SpawnFlags.NoOccupants, nextReplica.Vehicle, adjustedRatio);

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
            }

            return vehicle;
        }
    }

}
