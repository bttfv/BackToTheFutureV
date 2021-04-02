using FusionLibrary;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV
{
    internal class WaybackReplica
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

        public WaybackReplica(TimeMachine timeMachine, int startGameTime)
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

        public void Apply(TimeMachine timeMachine, int startPlayGameTime, WaybackReplica nextReplica)
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

            timeMachine.Vehicle.PositionNoOffset = Utils.Lerp(Position, nextReplica.Position, timeRatio);
            timeMachine.Vehicle.Rotation = Utils.Lerp(Rotation, nextReplica.Rotation, timeRatio, -180, 180);
            timeMachine.Vehicle.Velocity = Utils.Lerp(Velocity, nextReplica.Velocity, timeRatio);

            for (int i = 0; i < WheelsRotations.Length; i++)
            {
                VehicleControl.SetWheelRotation(timeMachine, i, Utils.Lerp(WheelsRotations[i], nextReplica.WheelsRotations[i], timeRatio, -(float)Math.PI, (float)Math.PI));
                VehicleControl.SetWheelCompression(timeMachine, i, Utils.Lerp(WheelsCompressions[i], nextReplica.WheelsCompressions[i], timeRatio));
            }

            Properties.ApplyToWayback(timeMachine);
            Mods.ApplyToWayback(timeMachine);
        }
    }

    internal class WaybackMachine : Script
    {
        public List<WaybackReplica> WaybackReplicas { get; } = new List<WaybackReplica>();

        public Guid GUID { get; private set; } = Guid.Empty;

        private TimeMachine timeMachine;
        public TimeMachine TimeMachine
        {
            get => timeMachine;
            set
            {
                timeMachine = value;
                timeMachine.WaybackMachine = this;
            }
        }

        public WaybackReplica StartReplica => WaybackReplicas.FirstOrDefault(x => x.Time >= Utils.CurrentTime);

        public int ReplicaIndex { get; private set; } = -1;
        public WaybackReplica CurrentReplica => WaybackReplicas[ReplicaIndex];

        public int StartGameTime { get; private set; }
        public int StartPlayGameTime { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public bool IsRecording { get; private set; } = true;

        public bool IsPlaying { get; private set; } = false;

        public WaybackMachine()
        {
            Tick += WaybackMachine_Tick;
        }

        public void Create(TimeMachine timeMachine)
        {
            GUID = timeMachine.Properties.GUID;
            TimeMachine = timeMachine;
            StartGameTime = Game.GameTime;

            WaybackReplicas.Add(new WaybackReplica(TimeMachine, StartGameTime));

            StartTime = WaybackReplicas.First().Time;

            WaybackMachineHandler.WaybackMachines.Add(this);
        }

        private void WaybackMachine_Tick(object sender, EventArgs e)
        {
            if (GUID == Guid.Empty)
                Abort();

            if (Utils.CurrentTime < StartTime)
            {
                if (IsRecording)
                    Stop();

                return;
            }

            if (IsRecording)
                Record();
            else
                Play();
        }

        private void Record()
        {
            if (!TimeMachine.NotNullAndExists())
            {
                Stop();

                return;
            }

            WaybackReplicas.Add(new WaybackReplica(TimeMachine, StartGameTime));
        }

        private void Play()
        {
            if (!TimeMachine.NotNullAndExists())
                return;

            WaybackReplica waybackReplica;

            if (!IsPlaying)
            {
                waybackReplica = StartReplica;

                if (waybackReplica == default)
                    return;

                ReplicaIndex = WaybackReplicas.IndexOf(waybackReplica);
                StartPlayGameTime = Game.GameTime;
                IsPlaying = true;
            }

            if (ReplicaIndex >= WaybackReplicas.Count - 1)
            {
                CurrentReplica.Apply(TimeMachine, StartPlayGameTime, CurrentReplica);

                IsPlaying = false;
            }
            else
                CurrentReplica.Apply(TimeMachine, StartPlayGameTime, WaybackReplicas[ReplicaIndex++]);
        }

        public void Stop()
        {
            if (timeMachine != null)
                timeMachine.WaybackMachine = null;

            if (IsRecording)
                EndTime = WaybackReplicas.Last().Time;

            timeMachine = null;
            IsRecording = false;
        }
    }
}
