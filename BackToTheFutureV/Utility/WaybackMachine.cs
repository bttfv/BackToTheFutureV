using BackToTheFutureV.TimeMachineClasses;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackToTheFutureV.Utility
{
    public class WaybackInfo
    {
        public DateTime Date { get; }
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }
        public float Speed { get; }
        public float SteeringAngle { get; }
        public PedInfo PedInfo { get; }
        public bool StartPoint { get; }

        public WaybackInfo(Vehicle vehicle, bool startPoint = false)
        {
            Date = Main.CurrentTime;
            Position = vehicle.Position;
            Rotation = vehicle.Rotation;
            Speed = vehicle.Speed+2;
            SteeringAngle = vehicle.SteeringAngle;
            StartPoint = startPoint;

            if (!vehicle.IsSeatFree(VehicleSeat.Driver))
                PedInfo = new PedInfo(vehicle.GetPedOnSeat(VehicleSeat.Driver));
        }

        public void ApplyTo(Vehicle vehicle)
        {
            vehicle.Position = Position;
            vehicle.Rotation = Rotation;
            vehicle.Speed = Speed;
            vehicle.SteeringAngle = SteeringAngle;
        }
    }

    public enum CurrentStatus
    {
        NotSpawned,
        Running,
        Stopped,
        Recording
    }

    public class WaybackMachine
    {
        private List<WaybackInfo> WaybackInfos = new List<WaybackInfo>();

        public WaybackMachine(Vehicle vehicle)
        {
            Vehicle = vehicle;

            VehicleInfo = new VehicleInfo(vehicle);

            TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(Vehicle);

            IsTimeMachine = timeMachine != null;

            if (IsTimeMachine)
            {
                TimeMachineClone = timeMachine.Clone;
                timeMachine.Events.OnTimeTravelCompleted += Duplicate;
            }                
        }

        public WaybackMachine(WaybackMachine waybackMachine)
        {
            Vehicle = null;

            CurrentStatus = CurrentStatus.NotSpawned;

            VehicleInfo = waybackMachine.VehicleInfo;

            IsTimeMachine = waybackMachine.IsTimeMachine;

            if (IsTimeMachine)
            {
                TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(Vehicle);
                TimeMachineClone = timeMachine.Clone;
                timeMachine.Events.OnTimeTravelCompleted += Duplicate;
            }

            WaybackInfos.AddRange(waybackMachine.WaybackInfos);
        }

        public Vehicle Vehicle { get; private set; }
        public bool IsTimeMachine { get; private set; }
        public TimeMachineClone TimeMachineClone { get; private set; }
        public VehicleInfo VehicleInfo { get; private set; }
        public CurrentStatus CurrentStatus { get; private set; } = CurrentStatus.Recording;        
        private Ped Driver { get; set; }
        private WaybackInfo NextWaybackInfo { get; set; }
        private TaskSequence TaskSequence = new TaskSequence();

        public void Process()
        {
            if (CurrentStatus != CurrentStatus.NotSpawned && (Vehicle == null || !Vehicle.Exists() || Vehicle.IsDead || !Vehicle.IsAlive))
            {
                CurrentStatus = CurrentStatus.NotSpawned;

                Vehicle = null;

                NextWaybackInfo = null;
            }

            if (NextWaybackInfo == null || NextWaybackInfo == default)
            {
                NextWaybackInfo = WaybackInfos.FirstOrDefault(x => x.Date > Main.CurrentTime);

                if (NextWaybackInfo == default && CurrentStatus != CurrentStatus.NotSpawned)
                {
                    GTA.UI.Screen.ShowSubtitle(CurrentStatus.ToString() + $" {IsTimeMachine}");

                    Record();

                    return;
                }
            }

            if (NextWaybackInfo == null || NextWaybackInfo == default)
                return;

            string test = $"{NextWaybackInfo.Date} {NextWaybackInfo.Position} {NextWaybackInfo.Speed} {NextWaybackInfo.StartPoint}";
           
            GTA.UI.Screen.ShowSubtitle(CurrentStatus.ToString() + $" {test}");

            if (Main.CurrentTime >= NextWaybackInfo.Date)
            {
                NextWaybackInfo = WaybackInfos.ElementAtOrDefault(WaybackInfos.IndexOf(NextWaybackInfo) + 1);

                if (NextWaybackInfo == null || NextWaybackInfo == default)
                    return;

                Play();
            }                
        }

        private void Play()
        {
            if (CurrentStatus == CurrentStatus.NotSpawned)
            {
                if (IsTimeMachine)
                    Vehicle = TimeMachineClone.Spawn();
                else
                    Vehicle = VehicleInfo.Spawn(NextWaybackInfo.Position, 0);

                NextWaybackInfo.ApplyTo(Vehicle);

                CurrentStatus = CurrentStatus.Stopped;

                NextWaybackInfo = null;

                return;
            }

            if (CurrentStatus == CurrentStatus.Stopped && NextWaybackInfo.Speed == 0)
                return;

            if (NextWaybackInfo.Speed > 0)
            {
                if (Driver == null || !Driver.Exists())
                {
                    Driver = NextWaybackInfo.PedInfo.Spawn(Vehicle, VehicleSeat.Driver);
                    Driver.Task.PerformSequence(TaskSequence);
                }

                if (TaskSequence.IsClosed)
                    TaskSequence = new TaskSequence();

                TaskSequence.AddTask.DriveTo(Vehicle, NextWaybackInfo.Position, 100, NextWaybackInfo.Speed, DrivingStyle.Rushed);

                //Function.Call(Hash.TASK_VEHICLE_GOTO_NAVMESH, Driver, Vehicle, NextWaybackInfo.Position.X, NextWaybackInfo.Position.Y, NextWaybackInfo.Position.Z, NextWaybackInfo.Speed, 156, 5.0f);

                CurrentStatus = CurrentStatus.Running;

                NextWaybackInfo = null;

                return;
            }

            if (NextWaybackInfo.Speed == 0 && CurrentStatus == CurrentStatus.Running)
            {
                Driver.Task.ClearAllImmediately();

                CurrentStatus = CurrentStatus.Stopped;

                NextWaybackInfo = null;

                return;
            }

            NextWaybackInfo = null;
        }

        private void Record()
        {
            CurrentStatus = CurrentStatus.Recording;

            if (Main.PlayerVehicle != Vehicle)
                return;

            if (WaybackInfos.Count > 0 && WaybackInfos.Last()?.Speed == 0 && Vehicle.Speed == 0)
                return;

            WaybackInfos.Add(new WaybackInfo(Vehicle, WaybackInfos.Count == 0));
        }

        private void Duplicate()
        {
            WaybackMachineHandler.Add(new WaybackMachine(this));
        }
    }
}
