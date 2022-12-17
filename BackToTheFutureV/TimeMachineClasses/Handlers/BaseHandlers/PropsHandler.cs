using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class PropsHandler : HandlerPrimitive
    {
        private readonly AnimateProp BTTFDecals;

        //Hover Mode
        public AnimatePropsHandler HoverModeWheelsGlow;
        public AnimateProp HoverModeVentsGlow;
        public AnimatePropsHandler HoverModeUnderbodyLights;

        //Fuel
        public AnimateProp EmptyGlowing;
        public AnimateProp EmptyOff;

        //Hoodbox
        public AnimateProp HoodboxLights;

        //Time travel
        public AnimateProp Coils;
        public AnimatePropsHandler SeparatedCoils;

        //Flux capacitor
        public AnimateProp FluxBlue;
        public AnimateProp FluxOrange;

        //Plutonium gauge
        public AnimateProp GaugeGlow;

        //Speedo
        public AnimateProp SpeedoCover;

        //Compass
        public AnimateProp Compass;

        //TFC
        public AnimateProp TFCOn;
        public AnimateProp TFCOff;
        public AnimateProp TFCHandle;

        //Time travel
        public AnimateProp WhiteSphere;
        public AnimateProp InvisibleProp;

        //Wheels
        public AnimatePropsHandler RRWheels;

        //TCD
        public AnimateProp DiodesOff;
        public AnimateProp TickingDiodes;
        public AnimateProp TickingDiodesOff;

        //License plate
        public AnimateProp LicensePlate;

        //Lightnings
        public AnimatePropsHandler Lightnings;
        public AnimatePropsHandler LightningsOnCar;

        //Bulova clock
        public AnimateProp BulovaClockHour;
        public AnimateProp BulovaClockMinute;
        public AnimateProp BulovaClockRing;

        public PropsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            //Wheels
            RRWheels = new AnimatePropsHandler();
            RRWheels.Add(new AnimateProp(ModelHandler.RRWheelProp, Vehicle, "wheel_lf"));
            RRWheels.Add(new AnimateProp(ModelHandler.RRWheelProp, Vehicle, "wheel_lr"));
            RRWheels.Add(new AnimateProp(ModelHandler.RRWheelProp, Vehicle, "wheel_rf"));
            RRWheels.Add(new AnimateProp(ModelHandler.RRWheelProp, Vehicle, "wheel_rr"));

            if (Vehicle.Bones["wheel_lm1"].Index > 0)
            {
                RRWheels.Add(new AnimateProp(ModelHandler.RRWheelProp, Vehicle, "wheel_lm1"));
            }

            if (Vehicle.Bones["wheel_rm1"].Index > 0)
            {
                RRWheels.Add(new AnimateProp(ModelHandler.RRWheelProp, Vehicle, "wheel_rm1"));
            }

            if (Vehicle.Bones["wheel_lm2"].Index > 0)
            {
                RRWheels.Add(new AnimateProp(ModelHandler.RRWheelProp, Vehicle, "wheel_lm2"));
            }

            if (Vehicle.Bones["wheel_rm2"].Index > 0)
            {
                RRWheels.Add(new AnimateProp(ModelHandler.RRWheelProp, Vehicle, "wheel_rm2"));
            }

            WhiteSphere = new AnimateProp(ModelHandler.WhiteSphere, Vehicle)
            {
                Duration = 0.25f
            };

            InvisibleProp = new AnimateProp(ModelHandler.InvisibleProp, TimeMachine.Vehicle, new Vector3(0, 3.4f, -0.6f), new Vector3(0, 0, 180));
            InvisibleProp.SpawnProp();

            //Lightnings
            Lightnings = new AnimatePropsHandler() { SequenceSpawn = true, SequenceInterval = 100, IsSequenceRandom = true };
            foreach (CustomModel x in ModelHandler.Lightnings)
            {
                Lightnings.Add(new AnimateProp(x, Vehicle));
            }

            LightningsOnCar = new AnimatePropsHandler() { SequenceSpawn = true, SequenceInterval = 100, IsSequenceRandom = true };
            foreach (CustomModel x in ModelHandler.LightningsOnCar)
            {
                LightningsOnCar.Add(new AnimateProp(x, Vehicle));
            }

            //Hover Mode            
            HoverModeWheelsGlow = new AnimatePropsHandler();
            for (int i = 0; i < Vehicle.Wheels.Count; i++)
            {
                HoverModeWheelsGlow.Add(new AnimateProp(ModelHandler.HoverGlowing, null, Vector3.Zero, new Vector3(0, 90, 0))
                {
                    Duration = 1.8f
                });
            }

            if (!Mods.IsDMC12)
            {
                return;
            }

            BTTFDecals = new AnimateProp(ModelHandler.BTTFDecals, Vehicle);
            BTTFDecals.SpawnProp();

            //Hover Mode
            HoverModeVentsGlow = new AnimateProp(ModelHandler.VentGlowing, Vehicle);
            HoverModeUnderbodyLights = new AnimatePropsHandler() { SequenceSpawn = true, SequenceInterval = 200, IsSequenceLooped = true };
            foreach (CustomModel model in ModelHandler.UnderbodyLights)
            {
                HoverModeUnderbodyLights.Add(new AnimateProp(model, Vehicle));
            }

            //Fuel
            EmptyGlowing = new AnimateProp(ModelHandler.Empty, Vehicle);
            EmptyOff = new AnimateProp(ModelHandler.EmptyOff, Vehicle);

            //Time travel
            //Coils = new AnimateProp(Vehicle, ModelHandler.CoilsGlowing);
            SeparatedCoils = new AnimatePropsHandler();
            foreach (CustomModel coilModel in ModelHandler.CoilSeparated)
            {
                SeparatedCoils.Add(new AnimateProp(coilModel, TimeMachine.Vehicle));
            }

            //Plutonium gauge
            GaugeGlow = new AnimateProp(ModelHandler.GaugeGlow, Vehicle);

            //Speedo
            SpeedoCover = new AnimateProp(ModelHandler.SpeedoCover, Vehicle);

            //Compass
            Compass = new AnimateProp(ModelHandler.Compass, Vehicle, "bttf_compass");

            //TFC
            TFCOn = new AnimateProp(ModelHandler.TFCOn, Vehicle);
            TFCOff = new AnimateProp(ModelHandler.TFCOff, Vehicle);
            TFCHandle = new AnimateProp(ModelHandler.TFCHandle, Vehicle, new Vector3(-0.03805999f, -0.0819466f, 0.5508024f), Vector3.Zero);
            TFCHandle[AnimationType.Rotation][AnimationStep.First][Coordinate.Y].Setup(true, false, -45, 0, 1, 135, 1, false);
            TFCHandle.SpawnProp();

            //TCD
            DiodesOff = new AnimateProp(ModelHandler.DiodesOff, Vehicle);
            DiodesOff.SpawnProp();
            TickingDiodes = new AnimateProp(ModelHandler.TickingDiodes, Vehicle);
            TickingDiodesOff = new AnimateProp(ModelHandler.TickingDiodesOff, Vehicle);
            TickingDiodesOff.SpawnProp();

            //Hoodbox lights
            HoodboxLights = new AnimateProp(ModelHandler.HoodboxLights, Vehicle, "bonnet");

            //Flux capacitor
            FluxBlue = new AnimateProp(ModelHandler.FluxBlueModel, Vehicle, "flux_capacitor");
            FluxOrange = new AnimateProp(ModelHandler.FluxOrangeModel, Vehicle, "flux_capacitor");

            //License plate
            LicensePlate = new AnimateProp(ModelHandler.LicensePlate, Vehicle, Vehicle.GetPositionOffset(Vehicle.RearPosition).GetSingleOffset(Coordinate.Z, 0.0275f), new Vector3(30, -90, 90));
            LicensePlate[AnimationType.Rotation][AnimationStep.First][Coordinate.Z].Setup(true, true, 90, (360 * 2) + 90, 1, 1440, 1, false);
            LicensePlate.SaveAnimation();

            //Bulova clock
            BulovaClockHour = new AnimateProp(ModelHandler.BulovaClockHour, Vehicle, "bulova_clock_ring_hands");
            BulovaClockHour.SetOffset(new Vector3(0, 0.001f, 0));
            BulovaClockMinute = new AnimateProp(ModelHandler.BulovaClockMinute, Vehicle, "bulova_clock_ring_hands");
            BulovaClockRing = new AnimateProp(ModelHandler.BulovaClockRing, Vehicle, "bulova_clock");
            BulovaClockRing[AnimationType.Rotation][AnimationStep.First][Coordinate.Y].Setup(false, true, -10, 10, 1, 360, 1, false);
        }

        public override void Dispose()
        {
            BTTFDecals?.Dispose();

            //Hover Mode
            HoverModeWheelsGlow?.Dispose();
            HoverModeVentsGlow?.Dispose();
            HoverModeUnderbodyLights?.Dispose();

            //Fuel
            EmptyGlowing?.Dispose();
            EmptyOff?.Dispose();

            //Hoodbox
            HoodboxLights?.Dispose();

            //Time travel
            Coils?.Dispose();
            WhiteSphere?.Dispose();

            //Plutonium gauge
            GaugeGlow?.Dispose();

            //Speedo
            SpeedoCover?.Dispose();

            //Compass
            Compass?.Dispose();

            //TFC
            TFCOn?.Dispose();
            TFCOff?.Dispose();
            TFCHandle?.Dispose();

            //Wheels
            RRWheels?.Dispose();

            //TCD
            DiodesOff?.Dispose();
            TickingDiodes?.Dispose();
            TickingDiodesOff?.Dispose();

            //Flux capacitor
            FluxBlue?.Dispose();
            FluxOrange?.Dispose();

            //License plate
            LicensePlate?.Dispose(LicensePlate != null && LicensePlate.IsSpawned);

            //Lightnings
            Lightnings?.Dispose();
            LightningsOnCar?.Dispose();

            //Bulova clock
            BulovaClockHour?.Dispose();
            BulovaClockMinute?.Dispose();
            BulovaClockRing?.Dispose();
        }

        public override void KeyDown(KeyEventArgs e)
        {
            //if (key == Keys.L)
            //{
            //    Sounds.Plate.Play();
            //    LicensePlate.Play(false, true);
            //}

            //if (key == Keys.O)
            //    LicensePlate.Delete();
        }

        public override void Tick()
        {

        }

        public override void Stop()
        {

        }
    }
}
