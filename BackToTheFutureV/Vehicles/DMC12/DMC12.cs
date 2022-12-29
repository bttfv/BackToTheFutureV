using BackToTheFutureV;
using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using GTA.Native;
using System;

namespace BackToTheFutureV
{
    internal delegate void SetStockSuspensions(bool state);
    internal delegate void SetVoltValue(float value);

    internal class DMC12
    {
        public Vehicle Vehicle { get; }
        public DMC12Mods Mods { get; }

        public bool Disposed { get; private set; }

        private readonly AnimateProp rpmNeedle;
        private readonly AnimateProp speedNeedle;
        private readonly AnimateProp fuelNeedle;
        private readonly AnimateProp tempNeedle;
        private readonly AnimateProp oilNeedle;
        private readonly AnimateProp voltNeedle;
        private readonly AnimateProp doorIndicator;
        private readonly AnimateProp leftFan;
        private readonly AnimateProp rightFan;

        private readonly AnimateProp suspensionLeftFront;
        private readonly AnimateProp suspensionLeftRear;
        private readonly AnimateProp suspensionRightFront;
        private readonly AnimateProp suspensionRightRear;

        private Vector3 InteriorLightOnPose;
        private Vector3 InteriorLightOffPose = new Vector3(0f, 0f, -2500f);

        private float voltLevel = 50f;

        private float rpmRotation;
        private float speedRotation = 17.5f;

        private float fuelRotation = 10;
        private float tempRotation = -10;
        private float oilRotation = 10;
        private float voltRotation = -10;
        private float fanRotation;

        public SetStockSuspensions SetStockSuspensions;
        public SetVoltValue SetVoltValue;

        private bool spawnSuspension;
        private bool isDucking;
        public bool forcedDucking;
        private bool duckEnded;
        private Camera steeringCamera;
        private Camera duckCameraStart;
        private Camera duckCameraEnd;
        private bool fpsEnded;
        private bool fpsSetup;
        private int duckTime;

        private bool _isTimeMachine;
        public bool IsTimeMachine
        {
            get
            {
                if (!_isTimeMachine)
                {
                    _isTimeMachine = Vehicle.IsTimeMachine();
                }

                return _isTimeMachine;
            }
        }

        public DMC12(Vehicle vehicle)
        {
            if (DMC12Handler.GetDeloreanFromVehicle(vehicle) != null)
            {
                return;
            }

            Vehicle = vehicle;

            Mods = new DMC12Mods(Vehicle);

            InteriorLightOnPose = Vehicle.Bones["interiorlight"].Pose;

            rpmNeedle = new AnimateProp(ModelHandler.RPMNeedle, Vehicle, "rpm_needle");
            speedNeedle = new AnimateProp(ModelHandler.SpeedNeedle, Vehicle, "speed_needle");
            fuelNeedle = new AnimateProp(ModelHandler.FuelNeedle, Vehicle, "fuel_needle");
            tempNeedle = new AnimateProp(ModelHandler.TemperatureNeedle, Vehicle, "temperature_needle");
            oilNeedle = new AnimateProp(ModelHandler.OilNeedle, Vehicle, "oil_needle");
            voltNeedle = new AnimateProp(ModelHandler.VoltageNeedle, Vehicle, "voltage_needle");
            doorIndicator = new AnimateProp(ModelHandler.DoorIndicator, Vehicle, Vector3.Zero, Vector3.Zero);
            leftFan = new AnimateProp(ModelHandler.RadiatorFan, Vehicle, "radiator_fan_l");
            rightFan = new AnimateProp(ModelHandler.RadiatorFan, Vehicle, "radiator_fan_r");

            suspensionLeftFront = new AnimateProp(ModelHandler.SuspensionFront, Vehicle, "suspension_lf");
            suspensionLeftRear = new AnimateProp(ModelHandler.SuspensionRear, Vehicle, "suspension_lr");
            suspensionRightFront = new AnimateProp(ModelHandler.SuspensionRightFront, Vehicle, "suspension_rf");
            suspensionRightRear = new AnimateProp(ModelHandler.SuspensionRightRear, Vehicle, "suspension_rr");

            rpmNeedle.SpawnProp();
            speedNeedle.SpawnProp();
            fuelNeedle.SpawnProp();
            tempNeedle.SpawnProp();
            oilNeedle.SpawnProp();
            voltNeedle.SpawnProp();
            doorIndicator.SpawnProp();
            leftFan.SpawnProp();
            rightFan.SpawnProp();

            suspensionLeftFront.SpawnProp();
            suspensionLeftRear.SpawnProp();
            suspensionRightFront.SpawnProp();
            suspensionRightRear.SpawnProp();

            SetStockSuspensions += DSetStockSuspensions;
            SetVoltValue += DSetVoltValue;

            spawnSuspension = true;

            DMC12Handler.AddDelorean(this);
        }

        private void DSetVoltValue(float value)
        {
            voltLevel = value;
        }

        private void DSetStockSuspensions(bool state)
        {
            spawnSuspension = state;
        }

        private void HandleDucking()
        {
            /*The checks here are as follows:
            First, we check to see if the player is pressing the duck key and make sure that they are fully seated in the vehicle (not playing enter/exit animation).
            Then, we check to see if the DMC-12 is a time machine, is not null and exists, and make sure it doesn't have hover since hover conversion also uses the duck key.
            Finally, we make sure that no ingame menus are open since the duck key on controller is also the select button in menus.
            We also check to see if the player is in first-person since the required animations are completely different.*/
            if (MenuHandler.closingTime < Game.GameTime && ClockHandler.finishTime < Game.GameTime && Vehicle.Driver != null && Vehicle.Driver.IsVisible)
            {
                if ((Game.IsControlPressed(Control.VehicleDuck) || forcedDucking) && FusionUtils.PlayerPed.IsFullyInVehicle() && ((_isTimeMachine && TimeMachineHandler.CurrentTimeMachine.NotNullAndExists() && TimeMachineHandler.CurrentTimeMachine.Mods.HoverUnderbody == InternalEnums.ModState.Off) || !_isTimeMachine) && GarageHandler.Status != InternalEnums.GarageStatus.Busy && !MenuHandler.IsAnyMenuOpen() && !isDucking && !FusionUtils.IsCameraInFirstPerson())
                {
                    FusionUtils.PlayerPed.Task?.PlayAnimation("veh@low@front_ds@idle_duck", "sit", -3.5f, 3.5f, -1, AnimationFlags.Secondary, 1f);
                    isDucking = true;
                    duckEnded = false;
                }
                else if ((Game.IsControlPressed(Control.VehicleDuck) || forcedDucking) && FusionUtils.PlayerPed.IsFullyInVehicle() && ((_isTimeMachine && TimeMachineHandler.CurrentTimeMachine.NotNullAndExists() && TimeMachineHandler.CurrentTimeMachine.Mods.HoverUnderbody == InternalEnums.ModState.Off) || !_isTimeMachine) && isDucking && !FusionUtils.IsCameraInFirstPerson())
                {
                    Function.Call(Hash.SET_ENTITY_ANIM_SPEED, FusionUtils.PlayerPed, "veh@low@front_ds@idle_duck", "sit", 0f);
                    Function.Call(Hash.SET_ENTITY_ANIM_CURRENT_TIME, FusionUtils.PlayerPed, "veh@low@front_ds@idle_duck", "sit", 0f);
                }
                else if (!duckEnded && !(Game.IsControlPressed(Control.VehicleDuck) || forcedDucking) && !FusionUtils.IsCameraInFirstPerson())
                {
                    FusionUtils.PlayerPed.Task?.ClearAnimation("veh@low@front_ds@idle_duck", "sit");
                    isDucking = false;
                    duckEnded = true;
                }

                if ((Game.IsControlPressed(Control.VehicleDuck) || forcedDucking) && FusionUtils.PlayerPed.IsFullyInVehicle() && ((_isTimeMachine && TimeMachineHandler.CurrentTimeMachine.NotNullAndExists() && TimeMachineHandler.CurrentTimeMachine.Mods.HoverUnderbody == InternalEnums.ModState.Off) || !_isTimeMachine) && GarageHandler.Status != InternalEnums.GarageStatus.Busy && !MenuHandler.IsAnyMenuOpen() && !isDucking && FusionUtils.IsCameraInFirstPerson())
                {
                    if (!fpsSetup)
                    {
                        duckCameraStart = World.CreateCamera(FusionUtils.PlayerPed.Bones[Bone.IKHead].Position + new Vector3(0f, 0.03f, 0.12f), GameplayCamera.Rotation, GameplayCamera.FieldOfView);
                        duckCameraEnd = World.CreateCamera(FusionUtils.PlayerPed.Bones[Bone.IKHead].Position + new Vector3(0f, 0.03f, 0.12f), GameplayCamera.Rotation, GameplayCamera.FieldOfView);
                        steeringCamera = World.CreateCamera(Vehicle.Position, Vehicle.Rotation, 50);
                        steeringCamera.AttachToVehicle(Vehicle, "", Vehicle.Bones["steeringwheel"].RelativePosition + new Vector3(0f, -0.25f, 0f), FusionUtils.DirectionToRotation(Vehicle.Bones["steeringwheel"].RelativePosition + new Vector3(0f, -0.25f, 0f), Vehicle.Bones["steeringwheel"].RelativePosition, 0));
                        World.RenderingCamera = duckCameraStart;
                        duckCameraStart.InterpTo(steeringCamera, 50, 0, 0);
                        isDucking = true;
                        fpsSetup = true;
                    }
                }
                else if (isDucking && !(Game.IsControlPressed(Control.VehicleDuck) || forcedDucking) && FusionUtils.IsCameraInFirstPerson())
                {
                    if (!fpsEnded)
                    {
                        steeringCamera.InterpTo(duckCameraEnd, 50, 0, 0);
                        duckTime = Game.GameTime + 50;
                        fpsEnded = true;
                    }
                    if (Game.GameTime > duckTime && fpsEnded)
                    {
                        World.RenderingCamera = null;
                        duckCameraStart?.Delete();
                        duckCameraEnd?.Delete();
                        steeringCamera?.Delete();
                        fpsSetup = false;
                        fpsEnded = false;
                        isDucking = false;
                    }
                }
            }
        }

        public void Tick()
        {
            if (!Vehicle.IsFunctioning())
            {
                DMC12Handler.RemoveDelorean(this, false);

                return;
            }

            if (!Vehicle.Doors[VehicleDoorIndex.FrontLeftDoor].IsOpen && !Vehicle.Doors[VehicleDoorIndex.FrontRightDoor].IsOpen && Vehicle.Bones["interiorlight"].Pose != InteriorLightOffPose)
            {
                Vehicle.Bones["interiorlight"].Pose = InteriorLightOffPose;
            }
            else if ((Vehicle.Doors[VehicleDoorIndex.FrontLeftDoor].IsOpen || Vehicle.Doors[VehicleDoorIndex.FrontRightDoor].IsOpen) && Vehicle.Bones["interiorlight"].Pose != InteriorLightOnPose)
            {
                Vehicle.Bones["interiorlight"].Pose = InteriorLightOnPose;
            }

            if (FusionUtils.PlayerVehicle == Vehicle)
            {
                Game.DisableControlThisFrame(Control.VehicleHydraulicsControlToggle);
                Function.Call(Hash.ENABLE_VEHICLE_EXHAUST_POPS, Vehicle, false);
                HandleDucking();
            }

            if (!IsTimeMachine)
            {
                VehicleControl.SetDeluxoTransformation(Vehicle, 0);
            }

            if (!Vehicle.IsVisible)
            {
                speedNeedle.Delete();
                rpmNeedle.Delete();
                fuelNeedle.Delete();
                tempNeedle.Delete();
                oilNeedle.Delete();
                voltNeedle.Delete();
                doorIndicator.Delete();
                leftFan.Delete();
                rightFan.Delete();

                suspensionLeftFront?.Delete();
                suspensionLeftRear?.Delete();
                suspensionRightFront?.Delete();
                suspensionRightRear?.Delete();

                return;
            }

            if (Vehicle.IsEngineRunning)
            {
                rpmRotation = Vehicle.CurrentRPM.Remap(0, 1, 0, 245);

                if (IsTimeMachine)
                {
                    if (Vehicle.GetMPHSpeed() < 15)
                    {
                        speedRotation = Vehicle.GetMPHSpeed().Remap(0, 15, 17.5f, 32);
                    }
                    else
                    {
                        speedRotation = Vehicle.GetMPHSpeed().Remap(15, 95, 32, 265);
                    }
                }
                else
                {
                    speedRotation = Vehicle.GetMPHSpeed().Remap(0, 85, 17.5f, 265);
                }

                speedRotation = Math.Min(speedRotation, 265);
                speedRotation = Math.Max(speedRotation, 17.5f);

                float fuelLevel = Vehicle.FuelLevel.Remap(0, Vehicle.HandlingData.PetrolTankVolume, 10, -100);
                float tempLevel = Vehicle.EngineTemperature.Remap(0, 190, -10, 100);
                float oilLevel = Vehicle.OilLevel.Remap(0, Vehicle.HandlingData.OilVolume, 10, -100);

                fuelRotation = FusionUtils.Lerp(fuelRotation, fuelLevel, Game.LastFrameTime);
                tempRotation = FusionUtils.Lerp(tempRotation, tempLevel, Game.LastFrameTime);
                oilRotation = FusionUtils.Lerp(oilRotation, oilLevel, Game.LastFrameTime);
                voltRotation = FusionUtils.Lerp(voltRotation, voltLevel, Game.LastFrameTime);
            }
            else
            {
                rpmRotation = FusionUtils.Lerp(rpmRotation, 0, Game.LastFrameTime);
                speedRotation = FusionUtils.Lerp(speedRotation, 17.5f, Game.LastFrameTime);

                fuelRotation = FusionUtils.Lerp(fuelRotation, 10, Game.LastFrameTime);
                tempRotation = FusionUtils.Lerp(tempRotation, -10, Game.LastFrameTime);
                oilRotation = FusionUtils.Lerp(oilRotation, 10, Game.LastFrameTime);
                voltRotation = FusionUtils.Lerp(voltRotation, -10, Game.LastFrameTime);
            }

            if (Vehicle.EngineTemperature >= 50)
            {
                fanRotation += Game.LastFrameTime * 10.8f * (Vehicle.EngineTemperature - 50);

                leftFan.MoveProp(Vector3.Zero, new Vector3(0, fanRotation, 0));
                rightFan.MoveProp(Vector3.Zero, new Vector3(0, fanRotation, 0));

                if (fanRotation >= 360)
                {
                    fanRotation -= 360;
                }
            }

            rpmNeedle.MoveProp(Vector3.Zero, new Vector3(0, rpmRotation, 0));
            speedNeedle.MoveProp(Vector3.Zero, new Vector3(0, speedRotation, 0));

            fuelNeedle.MoveProp(Vector3.Zero, new Vector3(0, fuelRotation, 0));
            tempNeedle.MoveProp(Vector3.Zero, new Vector3(0, tempRotation, 0));
            oilNeedle.MoveProp(Vector3.Zero, new Vector3(0, oilRotation, 0));
            voltNeedle.MoveProp(Vector3.Zero, new Vector3(0, voltRotation, 0));

            if (spawnSuspension)
            {
                if (!suspensionLeftFront.IsSpawned)
                {
                    suspensionLeftFront.SpawnProp();
                    suspensionLeftRear.SpawnProp();
                    suspensionRightFront.SpawnProp();
                    suspensionRightRear.SpawnProp();
                }
            }
            else
            {
                suspensionLeftFront?.Delete();
                suspensionLeftRear?.Delete();
                suspensionRightFront?.Delete();
                suspensionRightRear?.Delete();
            }

            if (Vehicle.IsEngineRunning && (Vehicle.Doors[VehicleDoorIndex.FrontLeftDoor].IsOpen || Vehicle.Doors[VehicleDoorIndex.FrontRightDoor].IsOpen))
            {
                doorIndicator.SpawnProp();
            }
            else
            {
                doorIndicator.Delete();
            }
        }

        public void Dispose(bool deleteVeh = true)
        {
            rpmNeedle?.Dispose();
            speedNeedle?.Dispose();
            fuelNeedle?.Dispose();
            oilNeedle?.Dispose();
            tempNeedle?.Dispose();
            voltNeedle?.Dispose();
            doorIndicator?.Dispose();
            leftFan?.Dispose();
            rightFan?.Dispose();

            suspensionLeftFront?.Dispose();
            suspensionLeftRear?.Dispose();
            suspensionRightFront?.Dispose();
            suspensionRightRear?.Dispose();

            if (deleteVeh)
            {
                Vehicle?.DeleteCompletely();
            }

            Disposed = true;
        }

        public static implicit operator Vehicle(DMC12 dmc12)
        {
            return dmc12.Vehicle;
        }

        public static implicit operator Entity(DMC12 dmc12)
        {
            return dmc12.Vehicle;
        }
    }
}
