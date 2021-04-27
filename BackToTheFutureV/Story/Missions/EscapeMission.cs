using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Drawing;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class EscapeMission : Mission
    {
        public Vehicle Vehicle { get; private set; }
        public Ped Driver { get; private set; }
        public Ped Shooter { get; private set; }
        public Ped TargetPed => TimeMachine.Vehicle.GetPedOnSeat(VehicleSeat.Driver);

        private PedGroup Peds { get; set; }
        private TimeMachine TimeMachine;

        private int step = -1;
        private int gameTimer;

        private Vector3 plutoniumPos = new Vector3(232.6f, -1988.85f, 18.64f);
        private Blip plutoniumBlip;

        protected override void OnEnd()
        {
            if (TimeMachine != null)
            {
                TimeMachine.Properties.MissionType = MissionType.None;
                TimeMachine.Events.OnTimeTravelStarted -= OnTimeTravelStarted;
            }

            TimeMachine = null;

            Vehicle?.DeleteCompletely();

            if (plutoniumBlip != null && plutoniumBlip.Exists())
                plutoniumBlip.Delete();

            Peds = null;
            step = -1;
        }

        public void StartOn(TimeMachine timeMachine)
        {
            if (IsPlaying)
                return;

            TextHandler.ShowSubtitle("FoundMe");
            TimeMachine = timeMachine;
            Start();
        }

        protected override void OnStart()
        {
            if (TimeMachine == null)
                TimeMachine = TimeMachineHandler.CurrentTimeMachine;

            Model model = new Model("sabregt");

            FusionUtils.LoadAndRequestModel(model);

            Vehicle = World.CreateVehicle(model, TargetPed.GetOffsetPosition(new Vector3(0, -10, 0)));
            Vehicle.PlaceOnNextStreet();

            Vehicle.AddBlip();

            Vehicle.MaxSpeed = (70f).ToMS();

            Driver = Vehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

            Driver.AlwaysKeepTask = true;

            Function.Call(Hash.TASK_VEHICLE_CHASE, Driver, TargetPed);
            Function.Call(Hash.SET_TASK_VEHICLE_CHASE_BEHAVIOR_FLAG, Driver, VehicleDrivingFlags.IgnorePathFinding | VehicleDrivingFlags.AvoidVehicles | VehicleDrivingFlags.DriveBySight | VehicleDrivingFlags.AllowGoingWrongWay | VehicleDrivingFlags.AllowMedianCrossing, true);
            Function.Call(Hash.SET_TASK_VEHICLE_CHASE_IDEAL_PURSUIT_DISTANCE, Driver, 0f);
            Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, Driver, 1.0f);

            Shooter = Vehicle.CreateRandomPedOnSeat(VehicleSeat.Passenger);
            Shooter.Weapons.Give(WeaponHash.Pistol, 999, true, true);
            Shooter.Task.VehicleShootAtPed(TargetPed);

            Peds = new PedGroup
            {
                { Driver, true },
                { Shooter, false }
            };

            TimeMachine.Events.OnTimeTravelStarted += OnTimeTravelStarted;
            TimeMachine.Properties.MissionType = MissionType.Escape;

            gameTimer = Game.GameTime + 90000;
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

            Vehicle.SteeringAngle = FusionUtils.Random.NextDouble() >= 0.5f ? 35 : -35;
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

        public override void Tick()
        {
            if (FusionUtils.CurrentTime.Year == 1985 && !IsPlaying)
            {
                World.DrawMarker(MarkerType.VerticalCylinder, plutoniumPos, Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), Color.Yellow);

                if (plutoniumBlip == null || !plutoniumBlip.Exists())
                {
                    plutoniumBlip = World.CreateBlip(plutoniumPos);
                    plutoniumBlip.Sprite = (BlipSprite)77;
                    plutoniumBlip.Name = "Libyans hideout";
                    plutoniumBlip.DisplayType = BlipDisplayType.BothMapSelectable;
                    plutoniumBlip.IsShortRange = true;
                    plutoniumBlip.IsFriendly = false;
                }

                if (!TimeMachineHandler.CurrentTimeMachine.NotNullAndExists() || TimeMachineHandler.CurrentTimeMachine.Vehicle.Position.DistanceToSquared2D(plutoniumPos) > 0.679f)
                    return;

                InternalInventory.Current.Plutonium = 5;
                StartOn(TimeMachineHandler.CurrentTimeMachine);
            }

            if (!IsPlaying)
            {
                if (plutoniumBlip != null && plutoniumBlip.Exists() && FusionUtils.CurrentTime.Year != 1985)
                    plutoniumBlip.Delete();

                return;
            }

            if (plutoniumBlip != null && plutoniumBlip.Exists())
                plutoniumBlip.Delete();

            switch (step)
            {
                case 0:
                    if (Driver.IsDead || Shooter.IsDead)
                        End();

                    break;
                case 1:
                    if (Vehicle.DistanceToSquared2D(TargetPed, 2) || gameTimer < Game.GameTime)
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

        public override void Abort()
        {
            End();

            if (plutoniumBlip != null && plutoniumBlip.Exists())
                plutoniumBlip.Delete();
        }
    }
}
