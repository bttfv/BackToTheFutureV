using FusionLibrary;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using System;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class WaybackMachineReplica
    {
        public DateTime Time { get; }
        public int Timestamp { get; }

        public bool EngineRunning { get; }
        public float Throttle { get; }
        public float Brake { get; }
        public float SteeringAngle { get; }
        public bool Lights { get; }
        public bool Headlights { get; }

        public float[] WheelsRotations { get; }
        public float[] WheelsCompressions { get; }

        public Vector3 Position { get; }
        public Vector3 Rotation { get; }
        public Vector3 Velocity { get; }

        public PropertiesHandler Properties { get; }
        public ModsPrimitive Mods { get; }

        public WaybackEvent Event { get; set; } = WaybackEvent.None;
        public int TimeTravelDelay { get; set; }

        public WaybackMachineReplica(TimeMachine timeMachine, int startGameTime)
        {
            Time = Utils.CurrentTime;
            Timestamp = Game.GameTime - startGameTime;

            EngineRunning = timeMachine.Vehicle.IsEngineRunning;
            Throttle = timeMachine.Vehicle.ThrottlePower;
            Brake = timeMachine.Vehicle.BrakePower;

            SteeringAngle = timeMachine.Vehicle.SteeringAngle;
            Lights = timeMachine.Vehicle.AreLightsOn;
            Headlights = timeMachine.Vehicle.AreHighBeamsOn;

            WheelsRotations = VehicleControl.GetWheelRotations(timeMachine);
            WheelsCompressions = VehicleControl.GetWheelCompressions(timeMachine);

            Position = timeMachine.Vehicle.Position;
            Rotation = timeMachine.Vehicle.Rotation;
            Velocity = timeMachine.Vehicle.Velocity;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();
        }

        public void Apply(TimeMachine timeMachine, int startPlayGameTime, WaybackMachineReplica nextReplica)
        {
            timeMachine.Vehicle.IsEngineRunning = EngineRunning;
            timeMachine.Vehicle.ThrottlePower = Throttle;
            timeMachine.Vehicle.BrakePower = Brake;

            timeMachine.Vehicle.SteeringAngle = SteeringAngle;
            timeMachine.Vehicle.AreLightsOn = Lights;
            timeMachine.Vehicle.AreHighBeamsOn = Headlights;

            float timeRatio = 0;

            if (Timestamp != nextReplica.Timestamp)
                timeRatio = (Game.GameTime - startPlayGameTime - Timestamp) / (float)(nextReplica.Timestamp - Timestamp);

            Vector3 pos = Utils.Lerp(Position, nextReplica.Position, timeRatio);

            GTA.UI.Screen.ShowSubtitle($"{timeMachine.Vehicle.Position.DistanceToSquared(pos)}");

            timeMachine.Vehicle.PositionNoOffset = pos;
            timeMachine.Vehicle.Rotation = Utils.Lerp(Rotation, nextReplica.Rotation, timeRatio, -180, 180);
            timeMachine.Vehicle.Velocity = Utils.Lerp(Velocity, nextReplica.Velocity, timeRatio);

            for (int i = 0; i < WheelsRotations.Length; i++)
            {
                VehicleControl.SetWheelRotation(timeMachine, i, Utils.Lerp(WheelsRotations[i], nextReplica.WheelsRotations[i], timeRatio, -(float)Math.PI, (float)Math.PI));
                VehicleControl.SetWheelCompression(timeMachine, i, Utils.Lerp(WheelsCompressions[i], nextReplica.WheelsCompressions[i], timeRatio));
            }

            Properties.ApplyToWayback(timeMachine);
            Mods.ApplyToWayback(timeMachine);

            switch (Event)
            {
                case WaybackEvent.OnSparksEnded:
                    timeMachine.Events.OnSparksEnded?.Invoke(TimeTravelDelay);
                    break;
                case WaybackEvent.OpenCloseReactor:
                    timeMachine.Events.SetOpenCloseReactor?.Invoke();
                    break;
                case WaybackEvent.RefuelReactor:
                    timeMachine.Events.SetRefuel?.Invoke(null);
                    break;
            }
        }
    }

}
