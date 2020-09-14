using BackToTheFutureV.Entities;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Vehicle = vehicle;

            Mods = new DMC12Mods(Vehicle);

            DMC12Handler.AddDelorean(this);

            rpmNeedle = new AnimateProp(Vehicle, "dmc12_rpm_needle", "rpm_needle");
            speedNeedle = new AnimateProp(Vehicle, "dmc12_speed_needle", "speed_needle");
            fuelNeedle = new AnimateProp(Vehicle, "dmc12_fuel_needle", "fuel_needle");
            tempNeedle = new AnimateProp(Vehicle, "dmc12_temperature_needle", "temperature_needle");
            oilNeedle = new AnimateProp(Vehicle, "dmc12_oil_needle", "oil_needle");
            voltNeedle = new AnimateProp(Vehicle, "dmc12_voltage_needle", "voltage_needle");
            doorIndicator = new AnimateProp(Vehicle, "dmc12_door_indicator", Vector3.Zero, Vector3.Zero);
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

            DMC12Handler.AddDelorean(this);

            SetStockSuspensions += eSetStockSuspensions;

            spawnSuspension = true;
        }

        public void eSetStockSuspensions(bool state)
        {
            spawnSuspension = state;
        }

        public void Process()
        {
            if (!Vehicle.IsVisible)
            {
                speedNeedle.DeleteProp();
                rpmNeedle.DeleteProp();
                fuelNeedle.DeleteProp();
                tempNeedle.DeleteProp();
                oilNeedle.DeleteProp();
                voltNeedle.DeleteProp();
                doorIndicator.DeleteProp();
                leftFan.DeleteProp();
                rightFan.DeleteProp();

                suspensionLeftFront?.DeleteProp();
                suspensionLeftRear?.DeleteProp();
                suspensionRightFront?.DeleteProp();
                suspensionRightRear?.DeleteProp();

                return;
            }

            if (Game.Player.Character.Position.DistanceToSquared(Vehicle.Position) > 5f * 5f)
                return;

            if (Vehicle.IsEngineRunning)
            {
                // --- RPM --
                rpmRotation = Vehicle.CurrentRPM * 210;

                // --- Speed ---
                float speed = Vehicle.Speed / 0.27777f / 1.60934f;
                speedRotation = 270 * speed / 95;

                if (speedRotation > 270) speedRotation = 270;

                fuelRotation = Utils.Lerp(fuelRotation, -31.5f, Game.LastFrameTime * 10f);
                tempRotation = Utils.Lerp(tempRotation, 4.5f, Game.LastFrameTime * 10f);
                oilRotation = Utils.Lerp(oilRotation, -5f, Game.LastFrameTime * 10f);
                voltRotation = Utils.Lerp(voltRotation, 5.5f, Game.LastFrameTime * 10f);
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

                leftFan.SpawnProp(Vector3.Zero, new Vector3(16f, fanRotation, 0), false);
                rightFan.SpawnProp(Vector3.Zero, new Vector3(16f, fanRotation, 0), false);

                if (fanRotation >= 360)
                    fanRotation -= 360;
            }

            speedNeedle.SpawnProp(Vector3.Zero, new Vector3(0, speedRotation, 0), false);
            rpmNeedle.SpawnProp(Vector3.Zero, new Vector3(0, rpmRotation, 0), false);
            fuelNeedle.SpawnProp(Vector3.Zero, new Vector3(0, fuelRotation, 0), false);
            tempNeedle.SpawnProp(Vector3.Zero, new Vector3(0, tempRotation, 0), false);
            oilNeedle.SpawnProp(Vector3.Zero, new Vector3(0, oilRotation, 0), false);
            voltNeedle.SpawnProp(Vector3.Zero, new Vector3(0, voltRotation, 0), false);

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
                suspensionLeftFront?.DeleteProp();
                suspensionLeftRear?.DeleteProp();
                suspensionRightFront?.DeleteProp();
                suspensionRightRear?.DeleteProp();
            }

            if (Utils.IsAnyOfFrontDoorsOpen(Vehicle))
                doorIndicator.SpawnProp();
            else
                doorIndicator.DeleteProp();
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
                Vehicle?.Delete();

            Disposed = true;
        }
    }
}
