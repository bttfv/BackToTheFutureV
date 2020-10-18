using BackToTheFutureV.Memory;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackToTheFutureV.Story.Missions
{
    public class EscapeMission : Mission
    {
        public Vehicle Vehicle { get; private set; }
        public Ped Driver { get; private set; }
        public Ped Shooter { get; private set; }
        public Ped TargetPed => TimeMachine.Vehicle.GetPedOnSeat(VehicleSeat.Driver);

        private PedGroup Peds { get; set; }
        private TimeMachine TimeMachine;

        private int step = -1;
        private int gameTimer;

        protected override void OnEnd()
        {
            if (TimeMachine != null)
            {
                TimeMachine.Properties.MissionType = MissionType.None;
                TimeMachine.Events.OnTimeTravelStarted -= OnTimeTravelStarted;
            }

            TimeMachine = null;

            if (Vehicle.IsFunctioning())
            {
                Vehicle?.AttachedBlip?.Delete();
                Driver?.Delete();
                Shooter?.Delete();
                Vehicle?.Delete();
            }

            Peds?.Delete();
            Peds = null;

            step = -1;
        }

        public void StartOn(TimeMachine timeMachine)
        {
            GTA.UI.Screen.ShowSubtitle(Game.GetLocalizedString("BTTFV_Mission_Escape_FoundMe"));
            TimeMachine = timeMachine;
            Start();
        }

        protected override void OnStart()
        {
            if (TimeMachine == null)
                TimeMachine = TimeMachineHandler.CurrentTimeMachine;

            Vehicle = World.CreateVehicle("sabregt", TargetPed.GetOffsetPosition(new Vector3(0, -10, 0)));
            Vehicle.Heading = TargetPed.Heading;
            Vehicle.AddBlip();

            Vehicle.MaxSpeed = Utils.MphToMs(70);

            Driver = Vehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

            Function.Call(Hash.TASK_VEHICLE_CHASE, Driver, TargetPed);
            Function.Call(Hash.SET_TASK_VEHICLE_CHASE_IDEAL_PURSUIT_DISTANCE, Driver, 0f);
            Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, Driver, 1.0f);

            Shooter = Vehicle.CreateRandomPedOnSeat(VehicleSeat.Passenger);
            Shooter.Weapons.Give(WeaponHash.Pistol, 999, true, true);
            Shooter.Task.VehicleShootAtPed(TargetPed);

            Peds = new PedGroup();

            Peds.Add(Driver, true);
            Peds.Add(Shooter, false);

            TimeMachine.Events.OnTimeTravelStarted += OnTimeTravelStarted;
            TimeMachine.Properties.MissionType = MissionType.Escape;

            step = 0;
        }

        public void OnTimeTravelStarted()
        {
            if (!IsPlaying)
                return;

            gameTimer = Game.GameTime + 5000;
            step = 1;

            if (TimeMachine.Properties.TimeTravelType == TimeTravelType.Instant)
                End();
        }

        private void StopVehicle()
        {
            Shooter.Task.ClearAll();
            Driver.Task.ClearAll();

            Vehicle.SteeringAngle = Utils.Random.NextDouble() >= 0.5f ? 35 : -35;
            Vehicle.IsHandbrakeForcedOn = true;
            Vehicle.Speed = Vehicle.Speed / 2;

            VehicleControl.SetBrake(Vehicle, 1f);

            gameTimer = Game.GameTime + 1250;
            step = 2;
        }

        private void ExplodeVehicle()
        {
            Vehicle.Explode();
            Vehicle.AttachedBlip?.Delete();
            End();
        }

        public override void Process()
        {
            if (!IsPlaying)
                return;

            switch (step)
            {
                case 1:
                    if (Vehicle.Position.DistanceToSquared2D(TargetPed.Position) <= 1 || gameTimer < Game.GameTime)
                        StopVehicle();

                    break;
                case 2:
                    if (gameTimer < Game.GameTime)
                        ExplodeVehicle();

                    break;
            }
        }

        public override void KeyDown(KeyEventArgs key)
        {
            
        }
    }
}
