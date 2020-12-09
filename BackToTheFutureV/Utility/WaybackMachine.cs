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

        public BaseProperties Properties { get; }
        public BaseMods Mods { get; }

        public WaybackReplica(TimeMachine timeMachine)
        {
            Time = Utils.CurrentTime;
            Timestamp = Game.GameTime;

            EngineRunning = timeMachine.Vehicle.IsEngineRunning;
            Throttle = VehicleControl.GetThrottle(timeMachine.Vehicle);
            Brake = VehicleControl.GetBrake(timeMachine.Vehicle);            
            Gear = timeMachine.Vehicle.CurrentGear;
            RPM = timeMachine.Vehicle.CurrentRPM;
            SteeringAngle = timeMachine.Vehicle.SteeringAngle;

            Position = timeMachine.Vehicle.Position;
            Rotation = timeMachine.Vehicle.Rotation;
            Velocity = timeMachine.Vehicle.Velocity;

            Properties = timeMachine.Properties.Clone();
            Mods = timeMachine.Mods.Clone();
        }

        public void Apply(TimeMachine timeMachine)
        {
            if (!timeMachine.NotNullAndExists())
                return;

            if (timeMachine.Vehicle.IsEngineRunning != EngineRunning)
                timeMachine.Vehicle.IsEngineRunning = EngineRunning;

            VehicleControl.SetThrottle(timeMachine, Throttle);
            VehicleControl.SetBrake(timeMachine, Brake);
            timeMachine.Vehicle.CurrentGear = Gear;
            timeMachine.Vehicle.CurrentRPM = RPM;
            timeMachine.Vehicle.SteeringAngle = SteeringAngle;

            timeMachine.Vehicle.PositionNoOffset = Position;
            timeMachine.Vehicle.Rotation = Rotation;
            timeMachine.Vehicle.Velocity = Velocity;

            Properties.ApplyToWayback(timeMachine);
            Mods.ApplyTo(timeMachine, true);

            timeMachine.Properties.IsWaybackPlaying = true;
        }
    }

    public class WaybackMachine : Script
    {
        public List<WaybackReplica> WaybackReplicas { get; } = new List<WaybackReplica>();

        public Guid GUID { get; private set; } = Guid.Empty;
        public TimeMachine TimeMachine { get; internal set; }
        public TimeMachineClone TimeMachineClone { get; private set; }

        public WaybackReplica CurrentReplica => WaybackReplicas.FirstOrDefault(x => x.Time >= Utils.CurrentTime);
        public DateTime StartTime => WaybackReplicas.FirstOrDefault().Time;
        public DateTime EndTime => WaybackReplicas.LastOrDefault().Time;
        
        public bool IsRecording { get; internal set; } = true;

        public WaybackMachine()
        {
            //Interval = 1;

            Tick += WaybackMachine_Tick;
        }

        private void WaybackMachine_Tick(object sender, EventArgs e)
        {
            if (Game.IsLoading)
                return;

            if (GUID == Guid.Empty)
                Abort();

            Process();
        }

        public void Create(TimeMachine timeMachine)
        {            
            GUID = timeMachine.Properties.GUID;
            TimeMachine = timeMachine;

            TimeMachineClone = timeMachine.Clone;

            Record();

            WaybackMachineHandler.WaybackMachines.Add(this);
        }

        internal void Process()
        {
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

        internal void Record()
        {
            if (!TimeMachine.NotNullAndExists())
            {
                Stop();

                return;
            }

            if (Utils.Distance2DBetween(TimeMachine, Utils.PlayerPed) > 300 * 300)
                return;

            WaybackReplicas.Add(new WaybackReplica(TimeMachine));
        }

        internal void Play()
        {
            WaybackReplica waybackReplica = CurrentReplica;

            if (waybackReplica == default || (TimeMachine.NotNullAndExists() && TimeMachine.Properties.IsWaybackPlaying && Utils.PlayerVehicle == TimeMachine))
            {
                if (TimeMachine.NotNullAndExists() && TimeMachine.Properties.IsWaybackPlaying)
                    TimeMachine.Properties.IsWaybackPlaying = false;

                return;
            }

            if (!TimeMachine.NotNullAndExists())
                TimeMachine = TimeMachineClone.Spawn(SpawnFlags.Default | SpawnFlags.NoWayback);

            waybackReplica.Apply(TimeMachine);
        }

        internal void Stop()
        {
            TimeMachine = null;
            IsRecording = false;
        }
    }
}
