using BackToTheFutureV.Utility;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class DriverAIHandler : Handler
    {
        public Ped Driver { get; private set; }
        public int Step { get; private set; } = 0;

        public DriverAIHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnTimeTravelCompleted += OnTimeTravelCompleted;
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
                IsPlaying = true;
            else
                Stop();
        }
 
        private void OnTimeTravelCompleted()
        {
            if (IsPlaying)
                Stop();
        }

        public override void Process()
        {
            if (!IsPlaying)
                return;

            switch (Step)
            {
                case 0:
                    Driver = Vehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

                    Step++;
                    break;
                case 1:
                    if (!Properties.IsFueled)
                        Driver.Task.GoStraightTo(Vehicle.GetOffsetPosition(new Vector3(0, -2.5f, 0f)), -1, Vehicle.Heading);
                    else
                        Step = 3;

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

                    if (Properties.AreTimeCircuitsBroken && Mods.Hoodbox == Vehicles.ModState.Off)
                        Mods.Hoodbox = Vehicles.ModState.On;

                    if (Mods.Hoodbox == Vehicles.ModState.On && !Properties.AreHoodboxCircuitsReady)
                        Events.OnHoodboxReady?.Invoke();

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

        public override void Stop()
        {
            Driver?.Delete();
            Driver = null;

            IsPlaying = false;
            Step = 0;
        }
    }
}
