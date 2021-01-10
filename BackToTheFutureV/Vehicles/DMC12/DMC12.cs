using BackToTheFutureV.Utility;
using FusionLibrary;
using FusionLibrary.Extensions;
using FusionLibrary.Memory;
using GTA;
using GTA.Math;
using GTA.Native;

namespace BackToTheFutureV.Vehicles
{
    public delegate void SetStockSuspensions(bool state);

    public class DMC12
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

        private float rpmRotation;
        private float speedRotation;
        private float fuelRotation;
        private float tempRotation;
        private float oilRotation;
        private float voltRotation;
        private float fanRotation;

        public SetStockSuspensions SetStockSuspensions;

        private bool spawnSuspension;

        public DMC12(Vehicle vehicle)
        {
            if (DMC12Handler.GetDeloreanFromVehicle(vehicle) != null)
                return;

            Vehicle = vehicle;

            Vehicle.IsPersistent = true;

            Mods = new DMC12Mods(Vehicle);

            rpmNeedle = new AnimateProp(Vehicle, ModelHandler.RPMNeedle, "rpm_needle");
            speedNeedle = new AnimateProp(Vehicle, ModelHandler.SpeedNeedle, "speed_needle");
            fuelNeedle = new AnimateProp(Vehicle, ModelHandler.FuelNeedle, "fuel_needle");
            tempNeedle = new AnimateProp(Vehicle, ModelHandler.TemperatureNeedle, "temperature_needle");
            oilNeedle = new AnimateProp(Vehicle, ModelHandler.OilNeedle, "oil_needle");
            voltNeedle = new AnimateProp(Vehicle, ModelHandler.VoltageNeedle, "voltage_needle");
            doorIndicator = new AnimateProp(Vehicle, ModelHandler.DoorIndicator, Vector3.Zero, Vector3.Zero);
            leftFan = new AnimateProp(Vehicle, ModelHandler.RadiatorFan, "radiator_fan_l");
            rightFan = new AnimateProp(Vehicle, ModelHandler.RadiatorFan, "radiator_fan_r");

            suspensionLeftFront = new AnimateProp(Vehicle, ModelHandler.SuspensionFront, "suspension_lf");
            suspensionLeftRear = new AnimateProp(Vehicle, ModelHandler.SuspensionRear, "suspension_lr");
            suspensionRightFront = new AnimateProp(Vehicle, ModelHandler.SuspensionRightFront, "suspension_rf");
            suspensionRightRear = new AnimateProp(Vehicle, ModelHandler.SuspensionRightRear, "suspension_rr");

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

            SetStockSuspensions += eSetStockSuspensions;

            spawnSuspension = true;

            DMC12Handler.AddDelorean(this);
        }

        public void eSetStockSuspensions(bool state)
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

            if (Vehicle.IsEngineRunning)
            {
                // --- RPM --
                rpmRotation = Vehicle.CurrentRPM * 210;

                // --- Speed ---
                float speed = Vehicle.GetMPHSpeed();
                speedRotation = 270 * speed / 95 - 8;

                if (speedRotation > 270)
                    speedRotation = 270;

                fuelRotation = Utils.Lerp(fuelRotation, -50f, Game.LastFrameTime * 10f);
                tempRotation = Utils.Lerp(tempRotation, 50f, Game.LastFrameTime * 10f);
                oilRotation = Utils.Lerp(oilRotation, -50f, Game.LastFrameTime * 10f);
                voltRotation = Utils.Lerp(voltRotation, 50f, Game.LastFrameTime * 10f);
            }
            else
            {
                fuelRotation = Utils.Lerp(fuelRotation, 0, Game.LastFrameTime * 15f);
                tempRotation = Utils.Lerp(tempRotation, 0, Game.LastFrameTime * 15f);
                oilRotation = Utils.Lerp(oilRotation, 0, Game.LastFrameTime * 15f);
                voltRotation = Utils.Lerp(voltRotation, 0, Game.LastFrameTime * 15f);
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

        public static implicit operator Vehicle(DMC12 dmc12) => dmc12.Vehicle;
        public static implicit operator Entity(DMC12 dmc12) => dmc12.Vehicle;
    }
}
