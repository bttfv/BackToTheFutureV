using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV.Utility
{
    public enum WaybackEventType
    {
        None,
        Refuel,
        Door
    }

    public class WaybackEvent
    {
        public WaybackEventType Type { get; private set; }
        public VehicleDoorIndex DoorIndex { get; }
        public bool DoorIsOpen { get; }

        public WaybackEvent()
        {
            Type = WaybackEventType.None;
        }

        public WaybackEvent(WaybackEventType type)
        {
            Type = type;
        }

        public WaybackEvent(VehicleDoorIndex doorIndex, bool isOpen)
        {
            Type = WaybackEventType.Door;
            DoorIndex = doorIndex;
            DoorIsOpen = isOpen;
        }

        public WaybackEvent Clone()
        {
            WaybackEvent waybackEvent;

            switch (Type)
            {
                case WaybackEventType.Door:
                    waybackEvent = new WaybackEvent(DoorIndex, DoorIsOpen);
                    break;
                default:
                    waybackEvent = new WaybackEvent(Type);
                    break;
            }

            return waybackEvent;
        }

        public void Apply(TimeMachine timeMachine)
        {
            switch (Type)
            {
                case WaybackEventType.Refuel:
                    timeMachine.Events.SetRefuel?.Invoke(null);
                    break;
                case WaybackEventType.Door:
                    if (DoorIsOpen)
                        timeMachine.Vehicle.Doors[DoorIndex].Open(false, true);
                    else
                        timeMachine.Vehicle.Doors[DoorIndex].Close(true);
                    break;
            }
        }

        public void Reset()
        {
            Type = WaybackEventType.None;
        }
    }

    public class WaybackReplica
    {
        public DateTime Time { get; }
        public int Timestamp { get; }
        public bool EngineRunning { get; }
        public float Throttle { get; }
        public float Brake { get; }
        public int Gear { get; }
        public float RPM { get; }
        public float SteeringAngle { get; }
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }
        public Vector3 Velocity { get; }
        public float[] Wheels { get; }
        public bool Lights { get; }
        public bool Headlights { get; }
        public bool IsVisible { get; }

        public BaseProperties Properties { get; }
        public BaseMods Mods { get; }
        public WaybackEvent WaybackEvent { get; }


        public WaybackReplica(TimeMachine timeMachine, int startGameTime, WaybackEvent waybackEvent)
        {
            Time = Utils.CurrentTime;
            Timestamp = Game.GameTime - startGameTime;

            EngineRunning = timeMachine.Vehicle.IsEngineRunning;

            try
            {
                Throttle = VehicleControl.GetThrottle(timeMachine.Vehicle);
                Brake = VehicleControl.GetBrake(timeMachine.Vehicle);
                Wheels = VehicleControl.GetWheelRotations(timeMachine.Vehicle);
            }
            catch { }
            
            Gear = timeMachine.Vehicle.CurrentGear;
            RPM = timeMachine.Vehicle.CurrentRPM;
            SteeringAngle = timeMachine.Vehicle.SteeringAngle;
            Lights = timeMachine.Vehicle.AreLightsOn;
            Headlights = timeMachine.Vehicle.AreHighBeamsOn;
            IsVisible = timeMachine.Vehicle.IsVisible;                       

            Position = timeMachine.Vehicle.Position;
            Rotation = timeMachine.Vehicle.Rotation;
            Velocity = timeMachine.Vehicle.Velocity;

            Properties = timeMachine.Properties.Clone(true);
            Mods = timeMachine.Mods.Clone();

            WaybackEvent = waybackEvent.Clone();
        }

        public void Apply(TimeMachine timeMachine, int startPlayGameTime, WaybackReplica nextReplica)
        {
            if (timeMachine.Vehicle.IsEngineRunning != EngineRunning)
                timeMachine.Vehicle.IsEngineRunning = EngineRunning;

            VehicleControl.SetThrottle(timeMachine, Throttle);
            VehicleControl.SetBrake(timeMachine, Brake);
            VehicleControl.SetWheelRotations(timeMachine, Wheels);

            timeMachine.Vehicle.CurrentGear = Gear;
            timeMachine.Vehicle.CurrentRPM = RPM;
            timeMachine.Vehicle.SteeringAngle = SteeringAngle;
            timeMachine.Vehicle.AreLightsOn = Lights;
            timeMachine.Vehicle.AreHighBeamsOn = Headlights;

            if (timeMachine.Vehicle.IsVisible != IsVisible)
                timeMachine.Vehicle.SetVisible(IsVisible);

            float timeRatio = 0;

            if (Timestamp != nextReplica.Timestamp)
                timeRatio = (float)(Game.GameTime - startPlayGameTime - Timestamp) / (float)(nextReplica.Timestamp - Timestamp);

            timeMachine.Vehicle.PositionNoOffset = Vector3.Lerp(Position, nextReplica.Position, timeRatio);
            timeMachine.Vehicle.Rotation = Vector3.Lerp(Rotation, nextReplica.Rotation, timeRatio);
            timeMachine.Vehicle.Velocity = Vector3.Lerp(Velocity, nextReplica.Velocity, timeRatio);

            Properties.ApplyToWayback(timeMachine);
            Mods.ApplyToWayback(timeMachine);

            if (WaybackEvent.Type != WaybackEventType.None)
                WaybackEvent.Apply(timeMachine);
        }
    }

    public class WaybackMachine : Script
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

        public WaybackEvent NextEvent { get; set; } = new WaybackEvent();

        public WaybackMachine()
        {
            Tick += WaybackMachine_Tick;
        }

        private void WaybackMachine_Tick(object sender, EventArgs e)
        {
            if (GUID == Guid.Empty)
                Abort();

            if (!WaybackMachineHandler.Enabled || Utils.CurrentTime < StartTime)
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
            GUID = timeMachine.Properties.GUID;
            TimeMachine = timeMachine;
            StartGameTime = Game.GameTime;

            WaybackReplicas.Add(new WaybackReplica(TimeMachine, StartGameTime, NextEvent));

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

            //if (TimeMachine.Vehicle.Position.DistanceToSquared(Utils.PlayerPed.Position) < 300f*300f)
            //    return;

            WaybackReplicas.Add(new WaybackReplica(TimeMachine, StartGameTime, NextEvent));

            NextEvent.Reset();
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
                
            WaybackReplica nextReplica = WaybackReplicas.SkipWhile(x => x != waybackReplica).Skip(1).DefaultIfEmpty(waybackReplica).FirstOrDefault();
           
            waybackReplica.Apply(TimeMachine, StartPlayGameTime, nextReplica);
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
