using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class WaybackVehicle
    {
        public VehicleReplica Replica { get; }

        public bool IsTimeMachine { get; private set; }

        public PropertiesHandler Properties { get; }

        public WaybackVehicleEvent Event { get; set; } = WaybackVehicleEvent.None;
        public int TimeTravelDelay { get; set; }

        public WaybackVehicle(TimeMachine timeMachine, WaybackVehicleEvent waybackVehicleEvent, int timeTravelDelay = 0)
        {
            Replica = new VehicleReplica(timeMachine, SpawnFlags.NoDriver);

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();

            Event |= waybackVehicleEvent;
            TimeTravelDelay = timeTravelDelay;
        }

        public WaybackVehicle(Vehicle vehicle)
        {
            Replica = new VehicleReplica(vehicle, SpawnFlags.NoDriver);

            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

            if (timeMachine == null)
            {
                return;
            }

            IsTimeMachine = true;

            Properties = timeMachine.Properties.Clone();
        }

        private Vehicle Spawn()
        {
            Vehicle vehicle = Replica.Spawn(SpawnFlags.Default);

            vehicle.SetPlayerLights(true);

            if (!IsTimeMachine)
            {
                return vehicle;
            }

            TimeMachine timeMachine = TimeMachineHandler.Create(vehicle);

            Properties.ApplyTo(timeMachine);

            return vehicle;
        }

        public Vehicle TryFindOrSpawn(VehicleReplica nextReplica, float adjustedRatio)
        {
            Vehicle vehicle;

            if (nextReplica == null)
            {
                vehicle = World.GetClosestVehicle(Replica.Position, 1f, Replica.Model);
            }
            else
            {
                vehicle = World.GetClosestVehicle(FusionUtils.Lerp(Replica.Position, nextReplica.Position, adjustedRatio), 1f, Replica.Model);
            }

            if (!vehicle.NotNullAndExists() || FusionUtils.PlayerPed == vehicle)
            {
                vehicle = Spawn();
            }

            return vehicle;
        }

        public Vehicle Apply(VehicleReplica nextReplica, float adjustedRatio, Ped ped = null)
        {
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

            TimeMachine timeMachine;

            if (!IsTimeMachine || !Properties.IsOnTracks)
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
            else if (Properties.IsOnTracks)
            {
                timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

                timeMachine.Events.SetTrainSpeed?.Invoke(Replica.Speed * (Replica.RunningDirection != RunningDirection.Backward ? 1 : -1));
            }

            if (!IsTimeMachine)
            {
                return vehicle;
            }

            timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(vehicle);

            if (timeMachine == null)
            {
                timeMachine = TimeMachineHandler.Create(vehicle);

                timeMachine.Properties.IsWayback = true;

                Properties.ApplyTo(timeMachine);

                return vehicle;
            }

            if (!timeMachine.NotNullAndExists())
            {
                return vehicle;
            }

            Properties.ApplyToWayback(timeMachine);

            if (Event == WaybackVehicleEvent.None)
            {
                return vehicle;
            }

            return vehicle;
        }
    }
}