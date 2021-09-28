using BackToTheFutureV;
using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using GTA.Native;

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

        private float voltLevel = 50f;

        private float rpmRotation;
        private float speedRotation;
        private float fuelRotation;
        private float tempRotation;
        private float oilRotation;
        private float voltRotation;
        private float fanRotation;

        public SetStockSuspensions SetStockSuspensions;
        public SetVoltValue SetVoltValue;

        private bool spawnSuspension;

        public DMC12(Vehicle vehicle)
        {
            if (DMC12Handler.GetDeloreanFromVehicle(vehicle) != null)
            {
                return;
            }

            Vehicle = vehicle;

            Vehicle.IsPersistent = true;

            Vehicle.Decorator().DoNotDelete = true;

            Mods = new DMC12Mods(Vehicle);

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

        public void Tick()
        {
            if (!Vehicle.IsFunctioning())
            {
                DMC12Handler.RemoveDelorean(this, false);

                return;
            }

            if (FusionUtils.PlayerVehicle == Vehicle)
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 31, 337, true);
            }

            if (!Vehicle.IsTimeMachine())
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

            float fuelLevel = Vehicle.FuelLevel.Remap(0, Vehicle.HandlingData.PetrolTankVolume, 0, 100);
            float oilLevel = Vehicle.OilLevel.Remap(0, Vehicle.HandlingData.OilVolume, 0, 100);
            float tempLevel = Vehicle.EngineTemperature.Remap(0, 190, 0, 100);

            if (Vehicle.IsEngineRunning)
            {
                // --- RPM --
                rpmRotation = Vehicle.CurrentRPM * 210;

                // --- Speed ---
                float speed = Vehicle.GetMPHSpeed();
                speedRotation = 270 * speed / 95 - 8;

                if (speedRotation > 270)
                {
                    speedRotation = 270;
                }

                fuelRotation = FusionUtils.Lerp(fuelRotation, -fuelLevel, Game.LastFrameTime);
                tempRotation = FusionUtils.Lerp(tempRotation, tempLevel, Game.LastFrameTime);
                oilRotation = FusionUtils.Lerp(oilRotation, -oilLevel, Game.LastFrameTime);
                voltRotation = FusionUtils.Lerp(voltRotation, voltLevel, Game.LastFrameTime);
            }
            else
            {
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

            speedNeedle.MoveProp(Vector3.Zero, new Vector3(0, speedRotation, 0));
            rpmNeedle.MoveProp(Vector3.Zero, new Vector3(0, rpmRotation, 0));
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

            if (Vehicle.IsAnyDoorOpen())
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
