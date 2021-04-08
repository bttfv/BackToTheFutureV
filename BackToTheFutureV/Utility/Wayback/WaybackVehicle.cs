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
        public VehicleReplica VehicleReplica { get; }

        public bool IsTimeMachine { get; }

        public ModsPrimitive Mods { get; }
        public PropertiesHandler Properties { get; }

        public WaybackVehicleEvent Event { get; set; } = WaybackVehicleEvent.None;
        public int TimeTravelDelay { get; set; }

        public WaybackVehicle(TimeMachine timeMachine, WaybackVehicleEvent wvEvent = WaybackVehicleEvent.None, int timeTravelDelay = 0)
        {
            VehicleReplica = new VehicleReplica(timeMachine, SpawnFlags.NoOccupants);

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();

            Event = wvEvent;
            TimeTravelDelay = timeTravelDelay;
        }

        public WaybackVehicle(Vehicle vehicle)
        {
            VehicleReplica = new VehicleReplica(vehicle, SpawnFlags.NoOccupants);

            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

            if (timeMachine == null)
                return;

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();
        }

        private Vehicle Spawn()
        {
            Vehicle vehicle = VehicleReplica.Spawn(SpawnFlags.NoVelocity | SpawnFlags.NoOccupants);

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
                vehicle = World.GetClosestVehicle(VehicleReplica.Position, 3, VehicleReplica.Model);
            else
                vehicle = World.GetClosestVehicle(FusionUtils.Lerp(VehicleReplica.Position, nextReplica.Position, adjustedRatio), 3, VehicleReplica.Model);

            if (!vehicle.NotNullAndExists() || FusionUtils.PlayerPed == vehicle)
                vehicle = Spawn();

            return vehicle;
        }

        public Vehicle Apply(Ped ped, VehicleReplica nextReplica, float adjustedRatio)
        {
            Vehicle vehicle = ped.GetUsingVehicle();

            if (!vehicle.NotNullAndExists())
                vehicle = TryFindOrSpawn(nextReplica, adjustedRatio);

            if (!vehicle.NotNullAndExists())
                return vehicle;

            TimeMachine timeMachine = null;

            if (IsTimeMachine)
            {
                timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

                if (timeMachine.NotNullAndExists() && timeMachine.Properties.TimeTravelPhase == TimeTravelPhase.Reentering)
                    return vehicle;
            }

            if (nextReplica == null)
            {
                if (ped.IsEnteringVehicle() || ped.IsLeavingVehicle())
                    VehicleReplica.ApplyTo(vehicle, SpawnFlags.NoOccupants | SpawnFlags.NoPosition);
                else
                    VehicleReplica.ApplyTo(vehicle, SpawnFlags.NoOccupants);
            }
            else
            {
                if (ped.IsEnteringVehicle() || ped.IsLeavingVehicle())
                    VehicleReplica.ApplyTo(vehicle, SpawnFlags.NoOccupants | SpawnFlags.NoPosition, nextReplica, adjustedRatio);
                else
                    VehicleReplica.ApplyTo(vehicle, SpawnFlags.NoOccupants, nextReplica, adjustedRatio);
            }

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
