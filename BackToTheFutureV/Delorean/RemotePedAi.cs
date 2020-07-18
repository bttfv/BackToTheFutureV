using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackToTheFutureV.Delorean.Handlers;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;

namespace BackToTheFutureV.Delorean
{
    public class RemotePedAI
    {
        public DeloreanTimeMachine Delorean { get; private set; } 

        public Ped RemotePed { get; private set; }

        private int counter = 0;
        private int timer;

        private Vector3 driveToPos;
        private Vector3 refuelOffset;

        private FuelHandler fuelHandler;

        public RemotePedAI(DeloreanTimeMachine del, Ped ped)
        {
            Delorean = del;
            RemotePed = ped;

            fuelHandler = Delorean.Circuits.GetHandler<FuelHandler>();
        }

        public void Dispose()
        {
            RemotePed?.Delete();
        }

        public void Tick()
        {
            if (Delorean.IsInTime) return;
            if (Game.GameTime < timer) return;

            var flyingHandler = Delorean.Circuits.GetHandler<FlyingHandler>();

            switch (counter)
            {
                case 0:
                    // First thing we need to do is stop the vehicle.
                    // For now we're assuming its just a land vehicle not flying

                    if (flyingHandler == null || !flyingHandler.IsFlying)
                    {
                        driveToPos = Delorean.Vehicle.GetOffsetPosition(new Vector3(0, 20, 0));

                        RemotePed.Task.DriveTo(Delorean.Vehicle, driveToPos, 2f, 20f);
                    }
                    else if(Delorean.Vehicle.HeightAboveGround > 2f || Delorean.Vehicle.Velocity.LengthSquared() > 2f)
                    {
                        flyingHandler.GoUpDown(-1);

                        // boost in opposite direction of velocity
                        Vector3 oppDir = new Vector3(-Delorean.Vehicle.Velocity.X, -Delorean.Vehicle.Velocity.Y, 0);
                        float baseMagnitude = oppDir.Length();
                        Vector3 finalDir = oppDir.Normalized * (0.5f * baseMagnitude);
                        Delorean.Vehicle.ApplyForce(finalDir * Game.LastFrameTime);

                        break;
                    }

                    counter++;
                    break;

                case 1:

                    if ((Delorean.Vehicle.Position.DistanceToSquared(driveToPos) < 2f * 2f || Delorean.MPHSpeed == 0.0f) ||
                        (flyingHandler != null && flyingHandler.IsFlying && Delorean.Vehicle.HeightAboveGround <= 5f && Delorean.MPHSpeed == 0.0f))
                    {
                        RemotePed.Task.ClearAll();
                        timer = Game.GameTime + 3000;
                        counter++;
                    }
                    else
                    {
                        timer = Game.GameTime + 100;
                    }

                    break;

                case 2:

                    RemotePed.Task.LeaveVehicle();

                    counter++;
                    timer = Game.GameTime + 1500;
                    break;

                case 3:

                    refuelOffset = new Vector3(-0.03764207f, -2.685903f, 0.8318611f);

                    RemotePed.Task.GoTo(Delorean.Vehicle.GetOffsetPosition(refuelOffset));

                    counter++;
                    break;

                case 4:
                    timer = Game.GameTime + 500;

                    RemotePed.Task.GoTo(Delorean.Vehicle.GetOffsetPosition(refuelOffset));

                    if (Delorean.Vehicle.GetOffsetPosition(refuelOffset).DistanceToSquared(RemotePed.Position) < 1f * 1f)
                    {
                        RemotePed.Task.ClearAll();
                        counter++;
                        timer = 0;
                    }

                    break;

                case 5:

                    fuelHandler.Refuel(RemotePed);

                    RemotePed.Task.LookAt(Delorean.Vehicle.Position);

                    counter++;

                    break;

                case 6:

                    timer = Game.GameTime + 500;

                    if (fuelHandler.IsRefueling)
                        break;

                    // Done refueling, go back into car
                    RemotePed.Task.EnterVehicle(Delorean.Vehicle, VehicleSeat.Driver);

                    counter++;

                    break;

                case 7:

                    timer = Game.GameTime + 500;

                    // Wait for ped to enter car
                    if (RemotePed.CurrentVehicle != Delorean.Vehicle)
                        break;

                    // After ped enters car, set random date
                    Delorean.Circuits.DestinationTime = new DateTime(2015, 03, 25, 06, 30, 0);
                    Delorean.Circuits.OnDestinationDateChange?.Invoke();

                    // Make him drive to 88!
                    Function.Call(Hash.SET_DRIVER_ABILITY, RemotePed.Handle, 1.0f);
                    RemotePed.Task.CruiseWithVehicle(Delorean.Vehicle, 88, DrivingStyle.Rushed);

                    counter++;

                    break;

                case 8:
                    if (flyingHandler != null && flyingHandler.IsFlying)
                    {
                        flyingHandler.Boost();

                        if (Delorean.Vehicle.Position.Z < 50)
                            flyingHandler.GoUpDown(1);
                    }

                    break;
            }
        }
    }
}
