using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class DriverAIHandler : HandlerPrimitive
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

        public override void KeyDown(KeyEventArgs e)
        {

        }

        private void StartAI(bool state)
        {
            if (state)
            {
                DriverTask = (DriverTaskType)Utils.Random.Next(5, 5);

                Driver = Vehicle.GetPedOnSeat(VehicleSeat.Driver);

                if (Driver == null)
                    Driver = Vehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

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
            GTA.UI.Screen.ShowSubtitle($"{Step}");

            switch (Step)
            {
                case 0:
                    if (!Properties.IsFueled)
                        Driver.Task.GoStraightTo(Vehicle.GetOffsetPosition(new Vector3(0, -2.5f, 0f)), -1, Vehicle.Heading);
                    else
                        Step = 3;

                    Step++;
                    break;
                case 1:
                    if (!FuelHandler.IsPedInPosition(Vehicle, Driver))
                        break;

                    Events.SetRefuel?.Invoke(Driver);

                    Step++;
                    break;
                case 2:
                    if (!Properties.IsFueled)
                        break;

                    Driver.Task.EnterVehicle(Vehicle, VehicleSeat.Driver);

                    Step++;
                    break;
                case 3:
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
                case 4:
                    Events.SimulateInputDate?.Invoke(BTTFImportantDates.GetRandom());

                    Step++;
                    break;
                case 5:

                    //Vector3 position = Driver.Position.Around(500);

                    //position = World.GetSafeCoordForPed(position, false);

                    //Function.Call(Hash.TASK_VEHICLE_GOTO_NAVMESH, Driver, Vehicle, position.X, position.Y, position.Z, 30.0f, 156, 5.0f);

                    Driver.Task.CruiseWithVehicle(Vehicle, MathExtensions.ToMS(200), DrivingStyle.AvoidTrafficExtremely);
                    Function.Call(Hash.SET_DRIVER_ABILITY, Driver, 1.0f);
                    Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, Driver, 1.0f);

                    Step++;
                    break;
            }
        }

        public override void Tick()
        {
            if (!IsPlaying)
                return;

            switch (DriverTask)
            {
                case DriverTaskType.LeaveVehicle:

                    Driver.Task.LeaveVehicle();

                    Driver = null;

                    Stop();

                    break;
                case DriverTaskType.ParkAndLeave:

                    Driver.Task.ParkVehicle(Vehicle, World.GetNextPositionOnStreet(Vehicle.Position, true), Vehicle.Heading);

                    Stop();

                    break;
                case DriverTaskType.DriveAround:
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
            IsPlaying = false;
            Step = 0;
            DriverTask = DriverTaskType.Off;
        }
    }
}
