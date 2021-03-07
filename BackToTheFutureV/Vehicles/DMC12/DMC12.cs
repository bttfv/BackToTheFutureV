using BackToTheFutureV.Utility;
using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using GTA.Native;

namespace BackToTheFutureV.Vehicles
{
    internal delegate void SetStockSuspensions(bool state);
    internal delegate void SetVoltValue(float value);

    internal class DMC12
    {
        public Vehicle Vehicle { get; }
        public DMC12Mods Mods { get; }

        public bool Disposed { get; private set; }

        private AnimateProp rpmNeedle;
        private AnimateProp speedNeedle;
        private AnimateProp fuelNeedle;
        private AnimateProp tempNeedle;
        private AnimateProp oilNeedle;
        private AnimateProp voltNeedle;
        private AnimateProp doorIndicator;
        private AnimateProp leftFan;
        private AnimateProp rightFan;

        private AnimateProp suspensionLeftFront;
        private AnimateProp suspensionLeftRear;
        private AnimateProp suspensionRightFront;
        private AnimateProp suspensionRightRear;

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
                return;

            Vehicle = vehicle;

            Vehicle.IsPersistent = true;

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

            SetStockSuspensions += dSetStockSuspensions;
            SetVoltValue += dSetVoltValue;

            spawnSuspension = true;

            DMC12Handler.AddDelorean(this);
        }

        private void dSetVoltValue(float value)
        {
            voltLevel = value;
        }

        private void dSetStockSuspensions(bool state)
        {
            spawnSuspension = state;
        }

        public void Process()
        {
            if (!Vehicle.IsFunctioning())
            {
                DMC12Handler.RemoveDelorean(this, false);

                return;
            }

            if (Utils.PlayerVehicle == Vehicle)
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 31, 337, true);

            if (!Vehicle.IsTimeMachine())
                VehicleControl.SetDeluxoTransformation(Vehicle, 0);

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

            if (Game.Player.Character.Position.DistanceToSquared(Vehicle.Position) > 5f * 5f)
                return;

            float fuelLevel = Utils.Clamp(Vehicle.FuelLevel, Vehicle.HandlingData.PetrolTankVolume, 100);
            float oilLevel = Utils.Clamp(Vehicle.OilLevel, Vehicle.OilVolume, 100);
            float tempLevel = Utils.Clamp(Vehicle.EngineTemperature, 190, 100);

            if (Vehicle.IsEngineRunning)
            {
                // --- RPM --
                rpmRotation = Vehicle.CurrentRPM * 210;

                // --- Speed ---
                float speed = Vehicle.GetMPHSpeed();
                speedRotation = 270 * speed / 95 - 8;

                if (speedRotation > 270)
                    speedRotation = 270;

                fuelRotation = Utils.Lerp(fuelRotation, -fuelLevel, Game.LastFrameTime);
                tempRotation = Utils.Lerp(tempRotation, tempLevel, Game.LastFrameTime);
                oilRotation = Utils.Lerp(oilRotation, -oilLevel, Game.LastFrameTime);
                voltRotation = Utils.Lerp(voltRotation, voltLevel, Game.LastFrameTime);
            }
            else
            {
                fuelRotation = Utils.Lerp(fuelRotation, 10, Game.LastFrameTime);
                tempRotation = Utils.Lerp(tempRotation, -10, Game.LastFrameTime);
                oilRotation = Utils.Lerp(oilRotation, 10, Game.LastFrameTime);
                voltRotation = Utils.Lerp(voltRotation, -10, Game.LastFrameTime);
            }

            if (Vehicle.EngineTemperature >= 50)
            {
                fanRotation += Game.LastFrameTime * 10.8f * (Vehicle.EngineTemperature - 50);

                leftFan.MoveProp(Vector3.Zero, new Vector3(16f, fanRotation, 0));
                rightFan.MoveProp(Vector3.Zero, new Vector3(16f, fanRotation, 0));

                if (fanRotation >= 360)
                    fanRotation -= 360;
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

            if (Utils.IsAnyOfFrontDoorsOpen(Vehicle))
                doorIndicator.SpawnProp();
            else
                doorIndicator.Delete();
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
                Vehicle?.DeleteCompletely();

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
