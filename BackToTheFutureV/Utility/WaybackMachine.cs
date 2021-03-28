using FusionLibrary;
using FusionLibrary.Extensions;
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
        //public int Gear { get; }
        //public float RPM { get; }
        public float SteeringAngle { get; }
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }
        public Vector3 Velocity { get; }
        public bool Lights { get; }
        public bool Headlights { get; }
        public bool IsVisible { get; }

        public PropertiesHandler Properties { get; }
        public ModsPrimitive Mods { get; }

        public WaybackReplica(TimeMachine timeMachine, int startGameTime)
        {
            Time = Utils.CurrentTime;
            Timestamp = Game.GameTime - startGameTime;

            EngineRunning = timeMachine.Vehicle.IsEngineRunning;
            Throttle = timeMachine.Vehicle.ThrottlePower;
            Brake = timeMachine.Vehicle.BrakePower;

            //Gear = timeMachine.Vehicle.CurrentGear;
            //RPM = timeMachine.Vehicle.CurrentRPM;
            SteeringAngle = timeMachine.Vehicle.SteeringAngle;
            Lights = timeMachine.Vehicle.AreLightsOn;
            Headlights = timeMachine.Vehicle.AreHighBeamsOn;
            IsVisible = timeMachine.Vehicle.IsVisible;

            Position = timeMachine.Vehicle.Position;
            Rotation = timeMachine.Vehicle.Rotation;
            Velocity = timeMachine.Vehicle.Velocity;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();
        }

        public void Apply(TimeMachine timeMachine, int startPlayGameTime, WaybackReplica nextReplica)
        {
            if (timeMachine.Vehicle.IsEngineRunning != EngineRunning)
                timeMachine.Vehicle.IsEngineRunning = EngineRunning;

            timeMachine.Vehicle.ThrottlePower = Throttle;
            timeMachine.Vehicle.BrakePower = Brake;

            //timeMachine.Vehicle.CurrentGear = Gear;
            //timeMachine.Vehicle.CurrentRPM = RPM;
            timeMachine.Vehicle.SteeringAngle = SteeringAngle;
            timeMachine.Vehicle.AreLightsOn = Lights;
            timeMachine.Vehicle.AreHighBeamsOn = Headlights;

            if (timeMachine.Vehicle.IsVisible != IsVisible)
                timeMachine.Vehicle.SetVisible(IsVisible);

            float timeRatio = 0;

            if (Timestamp != nextReplica.Timestamp)
                timeRatio = (Game.GameTime - startPlayGameTime - Timestamp) / (float)(nextReplica.Timestamp - Timestamp);

            timeMachine.Vehicle.PositionNoOffset = Vector3.Lerp(Position, nextReplica.Position, timeRatio);
            timeMachine.Vehicle.Rotation = Vector3.Lerp(Rotation, nextReplica.Rotation, timeRatio);
            timeMachine.Vehicle.Velocity = Vector3.Lerp(Velocity, nextReplica.Velocity, timeRatio);

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

        public WaybackReplica CurrentReplica => WaybackReplicas.FirstOrDefault(x => x.Time >= Utils.CurrentTime);

        public int StartGameTime { get; private set; }
        public int StartPlayGameTime { get; private set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime => WaybackReplicas.LastOrDefault().Time;

        public bool IsRecording { get; private set; } = true;

        public bool IsPlaying { get; private set; } = false;

        public WaybackMachine()
        {
            Tick += WaybackMachine_Tick;
        }

        private void WaybackMachine_Tick(object sender, EventArgs e)
        {
            if (GUID == Guid.Empty || !WaybackMachineHandler.Enabled)
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

        public void Create(TimeMachine timeMachine)
        {
            if (GUID != Guid.Empty)
                return;

            GUID = timeMachine.Properties.GUID;
            TimeMachine = timeMachine;
            StartGameTime = Game.GameTime;

            WaybackReplicas.Add(new WaybackReplica(TimeMachine, StartGameTime));

            StartTime = WaybackReplicas.First().Time;

            WaybackMachineHandler.WaybackMachines.Add(this);
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

            WaybackReplica waybackReplica = CurrentReplica;

            if (waybackReplica == default)
            {
                if (IsPlaying)
                    IsPlaying = false;

                return;
            }
            else if (!IsPlaying)
            {
                StartPlayGameTime = Game.GameTime;
                IsPlaying = true;
            }

            if (waybackReplica == WaybackReplicas.Last())
                waybackReplica.Apply(TimeMachine, StartPlayGameTime, waybackReplica);
            else
                waybackReplica.Apply(TimeMachine, StartPlayGameTime, WaybackReplicas[WaybackReplicas.IndexOf(waybackReplica) + 1]);
        }

        public void Stop()
        {
            if (timeMachine != null)
                timeMachine.WaybackMachine = null;

            timeMachine = null;
            IsRecording = false;
        }
    }
}
