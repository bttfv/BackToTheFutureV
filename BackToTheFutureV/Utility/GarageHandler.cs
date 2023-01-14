using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using GTA.Native;
using KlangRageAudioLibrary;
using System.Collections.Generic;
using System.Drawing;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class GarageInfo
    {
        public static List<GarageInfo> List { get; } = new List<GarageInfo>();

        static GarageInfo()
        {
            List.Add(new GarageInfo(GarageDoor.MichaelBeverlyHills, new Vector3(-814.3945f, 183.2831f, 71.48192f), (Hash)30769481, new Vector3(-810.57f, 187.76f, 71.62f), new Vector3(-0.31f, 0.20f, 110.14f), new Vector3(-806.37f, 186.49f, 74.63f), new Vector3(-31f, 0, 72.63f), new Vector3(-821.42f, 183.65f, 72.34f), new Vector3(3.84f, 0, -70.04f)));
            List.Add(new GarageInfo(GarageDoor.FranklinHills, new Vector3(19.32741f, 545.967f, 177.6564f), (Hash)2052512905, new Vector3(22.4f, 544.26f, 175.17f), new Vector3(-0.26f, 0.16f, 60.97f), new Vector3(24.2f, 539.52f, 177.9f), new Vector3(-30.55f, 0, 14.05f), new Vector3(13.57f, 549.12f, 175.94f), new Vector3(1.76f, 0, -120f)));
            List.Add(new GarageInfo(GarageDoor.TrevorCountryside, new Vector3(1972.225f, 3824.236f, 33.78944f), (Hash)67910261, new Vector3(1968.75f, 3821.97f, 31.54f), new Vector3(-0.38f, 0.18f, -59.2f), new Vector3(1963.65f, 3821.06f, 33.63f), new Vector3(-20.47f, 0, -81.32f), new Vector3(1976.77f, 3826.86f, 32.45f), new Vector3(-2.46f, 0, 119.5f)));
            List.Add(new GarageInfo(GarageDoor.FranklinAunt, new Vector3(-27.77285f, -1430.651f, 28.82552f), (Hash)703855057, new Vector3(-25.54566f, -1426.892f, 30.27997f), new Vector3(0f, 0f, -180f), new Vector3(-25.2972f, -1436.952f, 30.6545f), new Vector3(0, 0, 1.480996f), new Vector3(-25.2972f, -1436.952f, 30.6545f), new Vector3(0, 0, 1.480996f)));
        }

        public GarageDoor GarageDoor { get; }

        public Vector3 Position { get; }
        public Hash Model { get; }

        public Vector3 VehiclePosition { get; }
        public Vector3 VehicleRotation { get; }

        public Vector3 InsideCameraPosition { get; }
        public Vector3 InsideCameraRotation { get; }

        public Vector3 OutsideCameraPosition { get; }
        public Vector3 OutsideCameraRotation { get; }

        public bool IsDoorClosed => DoorHandler.IsDoorClosed(GarageDoor);

        public float GetDoorOpenRatio => DoorHandler.GetDoorOpenRatio(GarageDoor);

        public DoorState GetDoorState => DoorHandler.GetDoorState(GarageDoor);

        public void SetDoorState(DoorState doorState)
        {
            DoorHandler.SetDoorState(GarageDoor, doorState);
        }

        public bool IsPlayerNear => FusionUtils.PlayerPed.Position.DistanceToSquared2D(Position) <= 1000;

        public bool IsVehicleEntirelyInside(Vehicle vehicle)
        {
            return vehicle.IsEntirelyInGarage(GarageDoor);
        }

        public GarageInfo(GarageDoor garageDoor, Vector3 position, Hash model, Vector3 vehiclePosition, Vector3 vehicleRotation, Vector3 insideCameraPosition, Vector3 insideCameraRotation, Vector3 outsideCameraPosition, Vector3 outsideCameraRotation)
        {
            GarageDoor = garageDoor;
            Position = position;
            Model = model;
            VehiclePosition = vehiclePosition;
            VehicleRotation = vehicleRotation;
            InsideCameraPosition = insideCameraPosition;
            InsideCameraRotation = insideCameraRotation;
            OutsideCameraPosition = outsideCameraPosition;
            OutsideCameraRotation = outsideCameraRotation;
        }

        public void Abort()
        {
            DoorHandler.SetDoorState(GarageDoor, DoorState.Unlocked);
            DoorHandler.RemoveDoor(GarageDoor);
        }

        public void Tick()
        {
            if (DoorHandler.FindDoor(Position, Model) != (Hash)GarageDoor || DoorHandler.GetDoorPendingState(GarageDoor) != DoorState.Unknown)
            {
                DoorState pendingState = DoorHandler.GetDoorPendingState(GarageDoor);

                DoorHandler.RemoveDoor(GarageDoor);
                DoorHandler.RegisterDoor(GarageDoor, Model, Position);
                if (pendingState != DoorState.Unknown)
                {
                    DoorHandler.SetDoorState(GarageDoor, pendingState);
                }
            }

            if (DoorHandler.GetDoorState(GarageDoor) == DoorState.Unknown || DoorHandler.GetDoorPendingState(GarageDoor) == DoorState.Unknown)
            {
                if ((IsPlayerNear && GarageHandler.Status != GarageStatus.Idle && GarageHandler.Status != GarageStatus.Opening) || RemoteTimeMachineHandler.IsRemoteOn)
                {
                    DoorHandler.SetDoorState(GarageDoor, DoorState.Locked);
                }
                else
                {
                    DoorHandler.SetDoorState(GarageDoor, DoorState.Unlocked);
                }
            }
        }

        public void PlaceVehicle(Vehicle vehicle)
        {
            vehicle.PositionNoOffset = VehiclePosition;
            vehicle.Rotation = VehicleRotation;
            vehicle.PlaceOnGround();
        }

        public void Lock()
        {
            DoorHandler.SetDoorState(GarageDoor, DoorState.Locked);
        }

        public void Unlock()
        {
            DoorHandler.SetDoorState(GarageDoor, DoorState.Unlocked);
        }

        public Camera CreateInsideCamera()
        {
            return World.CreateCamera(InsideCameraPosition, InsideCameraRotation, 50);
        }

        public Camera CreateOutsideCamera()
        {
            return World.CreateCamera(OutsideCameraPosition, OutsideCameraRotation, 50);
        }
    }

    internal static class GarageHandler
    {
        private static Vehicle Vehicle => FusionUtils.PlayerVehicle;

        private static int gameTime;

        private static Camera garageCamera;

        public static GarageStatus Status { get; private set; } = GarageStatus.Idle;

        private static readonly AudioPlayer garageSound = Main.CommonAudioEngine.Create("story/garage.wav", Presets.No3D);

        private static bool isTimeMachine;

        private static bool _placeDamaged;

        public static bool WaitForCustomMenu;

        public static bool Transform;

        public static void Abort()
        {
            GarageInfo.List.ForEach(x => x.Abort());

            if (Status == GarageStatus.Idle)
            {
                return;
            }

            if (Vehicle.NotNullAndExists())
            {
                Function.Call(Hash.SET_VEHICLE_ENGINE_ON, Vehicle, Vehicle.IsEngineRunning, true, false);
            }

            FusionUtils.HideGUI = false;
            DestroyCamera();
            Status = GarageStatus.Idle;
        }

        public static void Tick()
        {
            if (Game.IsMissionActive || !Vehicle.NotNullAndExists() || RemoteTimeMachineHandler.IsRemoteOn)
            {
                if (Status != GarageStatus.Idle)
                {
                    Abort();
                }

                return;
            }

            foreach (GarageInfo garageInfo in GarageInfo.List)
            {
                if (!garageInfo.IsPlayerNear)
                {
                    continue;
                }

                garageInfo.Tick();

                if (Vehicle.HasTowArm)
                {
                    TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(Vehicle.TowedVehicle);

                    if (timeMachine.NotNullAndExists())
                    {
                        World.DrawMarker(MarkerType.VerticalCylinder, garageInfo.OutsideCameraPosition.SetToGroundHeight(), Vector3.Zero, Vector3.Zero, new Vector3(3, 3, 3), Color.Red);
                        garageInfo.SetDoorState(DoorState.Locked);

                        if (Vehicle.Position.DistanceToSquared2D(garageInfo.OutsideCameraPosition) <= 30)
                        {
                            if (_placeDamaged)
                            {
                                if (GTA.UI.Screen.IsFadedOut)
                                {
                                    Vehicle.DetachTowedVehicle();
                                    garageInfo.PlaceVehicle(timeMachine);

                                    Vehicle.IsPersistent = true;
                                    Vehicle.PlaceOnNextStreet();

                                    FusionUtils.PlayerPed.PositionNoOffset = garageInfo.OutsideCameraPosition.SetToGroundHeight();
                                    FusionUtils.PlayerPed.Heading = FusionUtils.PlayerPed.Position.GetDirectionTo(timeMachine.Vehicle.Position).ToHeading();

                                    garageInfo.SetDoorState(DoorState.Unlocked);

                                    _placeDamaged = false;

                                    return;
                                }
                            }
                            else
                            {
                                ScreenFade.FadeOut(1000, 1000, 1000);
                                _placeDamaged = true;
                            }
                        }
                    }
                }

                if (!garageInfo.IsVehicleEntirelyInside(Vehicle) && Status == GarageStatus.Idle)
                {
                    continue;
                }

                if (Status == GarageStatus.Busy)
                {
                    Game.DisableControlThisFrame(Control.VehicleMoveLeftRight);
                    Game.DisableControlThisFrame(Control.VehicleMoveUpDown);
                    Game.DisableControlThisFrame(Control.VehicleAccelerate);
                    Game.DisableControlThisFrame(Control.VehicleBrake);
                    Game.DisableControlThisFrame(Control.VehicleExit);
                }

                switch (Status)
                {
                    case GarageStatus.Idle:
                        GTA.UI.Screen.ShowHelpTextThisFrame("Press ~INPUT_CONTEXT~ to open garage menu.");

                        if (Game.IsControlJustPressed(Control.Context))
                        {
                            //FusionUtils.HideGUI = true;
                            GTA.UI.Hud.IsRadarVisible = false;

                            isTimeMachine = Vehicle.IsTimeMachine();

                            Vehicle.TaskDrive().Add(DriveAction.BrakeUntilTimeEndsOrCarStops, 2000).Start();
                            Function.Call(Hash.SET_VEHICLE_ENGINE_ON, Vehicle, false, false, true);

                            //SetupCamera(garageInfo.CreateInsideCamera());
                            MenuHandler.GarageMenu.Visible = true;
                            MenuHandler.CustomMenu.CloseOnInvalidClick = false;

                            garageInfo.Lock();
                            Status = GarageStatus.Busy;
                        }

                        break;
                    case GarageStatus.Opening:
                        if (Game.GameTime < gameTime)
                        {
                            break;
                        }

                        FusionUtils.HideGUI = false;
                        GTA.UI.Hud.IsRadarVisible = true;
                        TimeMachineHandler.CurrentTimeMachine?.Particles.IceSmoke?.Stop();
                        DestroyCamera();

                        Status = GarageStatus.Idle;
                        break;
                    case GarageStatus.Busy:
                        if (MenuHandler.GarageMenu.Visible || WaitForCustomMenu)
                        {
                            break;
                        }

                        SetupCamera(garageInfo.CreateOutsideCamera());
                        garageInfo.PlaceVehicle(Vehicle);

                        TimeMachine timeMachine;

                        if (Transform)
                        {
                            timeMachine = TimeMachineHandler.Create(Vehicle);

                            if (timeMachine.Mods.IsDMC12)
                            {
                                timeMachine.Properties.ReactorCharge = 1;
                            }

                            Transform = false;
                        }

                        if (!isTimeMachine && Vehicle.IsTimeMachine())
                        {
                            garageSound.SourceEntity = FusionUtils.PlayerPed;
                            garageSound.Volume = 0.5f;
                            garageSound.Play();

                            TimeMachineHandler.GetTimeMachineFromVehicle(Vehicle)?.Particles.IceSmoke?.Play();
                        }

                        Function.Call(Hash.SET_VEHICLE_ENGINE_ON, Vehicle, true, true, false);
                        Vehicle.TaskDrive().Create().Add(DriveAction.AccelerateWeak, 1000).Start();

                        garageInfo.Unlock();

                        gameTime = Game.GameTime + 2500;
                        Status = GarageStatus.Opening;
                        break;
                }
            }
        }

        private static void SetupCamera(Camera camera)
        {
            garageCamera?.Delete();
            garageCamera = null;

            garageCamera = camera;
            World.RenderingCamera = garageCamera;
        }

        private static void DestroyCamera()
        {
            garageCamera?.Delete();
            garageCamera = null;

            World.RenderingCamera = null;
        }
    }
}
