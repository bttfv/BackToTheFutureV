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
using System.Text;
using System.Threading.Tasks;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Utility
{
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

        public WaybackReplica(TimeMachine timeMachine)
        {
            Time = Utils.CurrentTime;
            Timestamp = Game.GameTime;

            EngineRunning = timeMachine.Vehicle.IsEngineRunning;

            try
            {
                Throttle = VehicleControl.GetThrottle(timeMachine.Vehicle);
                Brake = VehicleControl.GetBrake(timeMachine.Vehicle);
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
        }

        public void Apply(TimeMachine timeMachine)
        {
            if (timeMachine.Vehicle.IsEngineRunning != EngineRunning)
                timeMachine.Vehicle.IsEngineRunning = EngineRunning;

            VehicleControl.SetThrottle(timeMachine, Throttle);
            VehicleControl.SetBrake(timeMachine, Brake);
            timeMachine.Vehicle.CurrentGear = Gear;
            timeMachine.Vehicle.CurrentRPM = RPM;
            timeMachine.Vehicle.SteeringAngle = SteeringAngle;
            timeMachine.Vehicle.AreLightsOn = Lights;
            timeMachine.Vehicle.AreHighBeamsOn = Headlights;

            if (timeMachine.Vehicle.IsVisible != IsVisible)
                timeMachine.Vehicle.SetVisible(IsVisible);

            timeMachine.Vehicle.PositionNoOffset = Position;
            timeMachine.Vehicle.Rotation = Rotation;
            timeMachine.Vehicle.Velocity = Velocity;

            Properties.ApplyToWayback(timeMachine);
            Mods.ApplyToWayback(timeMachine);

            timeMachine.Properties.IsWaybackPlaying = true;
        }
    }

    public class WaybackMachine : Script
    {
        public List<WaybackReplica> WaybackReplicas { get; } = new List<WaybackReplica>();

        public Guid GUID { get; private set; } = Guid.Empty;
        
        public TimeMachine TimeMachine { get; set; }

        public WaybackReplica CurrentReplica => WaybackReplicas.FirstOrDefault(x => x.Time >= Utils.CurrentTime);

        public DateTime StartTime { get; set; }
        public DateTime EndTime => WaybackReplicas.LastOrDefault().Time;
        
        public bool IsRecording { get; private set; } = true;

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

            WaybackReplicas.Add(new WaybackReplica(TimeMachine));

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

            //if (Utils.Distance2DBetween(TimeMachine, Utils.PlayerPed) > 300)
            //    return;

            WaybackReplicas.Add(new WaybackReplica(TimeMachine));
        }

        private void Play()
        {
            if (!TimeMachine.NotNullAndExists())
                return;

            WaybackReplica waybackReplica = CurrentReplica;

            if (waybackReplica == default)
                return;
           
            waybackReplica.Apply(TimeMachine);
        }

        public void Stop()
        {
            if (TimeMachine.NotNullAndExists() && TimeMachine.Properties.IsWaybackPlaying)
                TimeMachine.Properties.IsWaybackPlaying = false;

            TimeMachine = null;
            IsRecording = false;
        }
    }
}
