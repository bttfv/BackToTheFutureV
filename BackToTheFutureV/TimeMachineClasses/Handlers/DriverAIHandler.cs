using BackToTheFutureV.Story;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Events.SetPedAI += SetPedAI;
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyDown(Keys key)
        {
            
        }

        private void SetPedAI(bool state)
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
                        Driver.Task.GoTo(Vehicle.GetOffsetPosition(new Vector3(0, -2.5f, 0f)));
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

                    if (!Properties.AreTimeCircuitsOn)
                        Events.SetTimeCircuits?.Invoke(true);

                    Step++;
                    break;
                case 5:
                    Properties.DestinationTime = BTTFImportantDates.GetRandom();
                    Events.OnDestinationDateChange?.Invoke();

                    Step++;
                    break;
                case 6:
                    Driver.Task.CruiseWithVehicle(Vehicle, Utils.MphToMs(200), DrivingStyle.Rushed);
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
