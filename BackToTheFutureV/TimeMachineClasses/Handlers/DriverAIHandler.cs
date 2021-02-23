using BackToTheFutureV.Utility;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Windows.Forms;
using static BackToTheFutureV.Utility.InternalEnums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public enum DriverTaskType
    {
        Off,
        LeaveVehicle,
        ParkAndLeave,
        DriveAround,
        DriveAroundAndTimeTravel,
        TimeTravel
    }

    public class DriverAIHandler : Handler
    {
        public Ped Driver { get; private set; }
        public int Step { get; private set; } = 0;

        public DriverTaskType DriverTask { get; private set; } = DriverTaskType.Off;

        public DriverAIHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnTimeTravelEnded += OnTimeTravelEnded;
            Events.StartDriverAI += StartAI;
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyDown(Keys key)
        {

        }

        private void StartAI(bool state)
        {
            if (state)
            {
                DriverTask = (DriverTaskType)Utils.Random.Next(2, 2);

                IsPlaying = true;
            }
            else
                Stop();
        }

        private void OnTimeTravelEnded()
        {
            if (IsPlaying)
                Stop();
        }

        private void TaskTimeTravel()
        {
            switch (Step)
            {
                case 0:
                    Driver = Vehicle.GetPedOnSeat(VehicleSeat.Driver);

                    if (Driver == null)
                        Driver = Vehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

                    Step++;
                    break;
                case 1:
                    if (!Properties.IsFueled)
                        Driver.Task.GoStraightTo(Vehicle.GetOffsetPosition(new Vector3(0, -2.5f, 0f)), -1, Vehicle.Heading);
                    else
                        Step = 4;

                    Step++;
                    break;
                case 2:
                    if (!FuelHandler.CanRefuel(Vehicle, Driver))
                        break;

                    Events.SetRefuel?.Invoke(Driver);

                    Step++;
                    break;
                case 3:
                    if (!Properties.IsFueled)
                        break;

                    Driver.Task.EnterVehicle(Vehicle, VehicleSeat.Driver);

                    Step++;
                    break;
                case 4:
                    if (!Driver.IsInVehicle(Vehicle))
                        break;

                    if (Properties.AreTimeCircuitsBroken && Mods.Hoodbox == ModState.Off)
                        Mods.Hoodbox = ModState.On;

                    if (Mods.Hoodbox == ModState.On && !Properties.AreHoodboxCircuitsReady)
                        Events.SetHoodboxWarmedUp?.Invoke();

                    if (!Properties.AreTimeCircuitsOn)
                        Events.SetTimeCircuits?.Invoke(true);

                    Step++;
                    break;
                case 5:
                    Events.SimulateInputDate?.Invoke(BTTFImportantDates.GetRandom());

                    Step++;
                    break;
                case 6:
                    Driver.Task.CruiseWithVehicle(Vehicle, MathExtensions.ToMS(200), DrivingStyle.AvoidTrafficExtremely);
                    Function.Call(Hash.SET_DRIVER_ABILITY, Driver, 1.0f);
                    Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, Driver, 1.0f);

                    Step++;
                    break;
            }
        }

        public override void Process()
        {
            if (!IsPlaying)
                return;

            GTA.UI.Screen.ShowSubtitle($"{DriverTask}");

            switch (DriverTask)
            {
                case DriverTaskType.LeaveVehicle:

                    Driver = Vehicle.GetPedOnSeat(VehicleSeat.Driver);

                    if (Driver == null)
                        Driver = Vehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

                    Driver.Task.LeaveVehicle();

                    Driver = null;

                    Stop();

                    break;
                case DriverTaskType.ParkAndLeave:

                    Ped ped = Vehicle.GetPedOnSeat(VehicleSeat.Driver);

                    if (ped == null)
                        ped = Vehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

                    ped.Task.ParkVehicle(Vehicle, World.GetNextPositionOnStreet(Vehicle.Position, true), Vehicle.Heading);

                    //TaskSequence taskSequence = new TaskSequence();
                    //taskSequence.AddTask.ParkVehicle(Vehicle, World.GetNextPositionOnStreet(Vehicle.Position, true), Vehicle.Heading);
                    //taskSequence.AddTask.LeaveVehicle();

                    //ped.Task.PerformSequence(taskSequence);

                    Stop();

                    break;
                case DriverTaskType.DriveAround:

                    Driver = Vehicle.GetPedOnSeat(VehicleSeat.Driver);

                    if (Driver == null)
                        Driver = Vehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

                    Driver.Task.CruiseWithVehicle(Vehicle, MathExtensions.ToMS(50));

                    Driver = null;

                    Stop();

                    break;
                case DriverTaskType.DriveAroundAndTimeTravel:

                    break;
                case DriverTaskType.TimeTravel:

                    TaskTimeTravel();

                    break;
            }
        }

        public override void Stop()
        {
            Driver?.Delete();
            Driver = null;

            IsPlaying = false;
            Step = 0;
            DriverTask = DriverTaskType.Off;
        }
    }
}
