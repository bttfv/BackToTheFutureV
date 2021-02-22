using BackToTheFutureV.Utility;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers
{
    public class PropsHandler : Handler
    {
        private AnimateProp BTTFDecals;

        //Hover Mode
        public AnimateProp HoverModeWheelsGlow;
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

        //Plutonium gauge
        public AnimateProp GaugeGlow;

        //Speedo
        public AnimateProp Speedo;

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

        //Bucket
        public AnimateProp Bucket;

        //Lightnings
        public AnimatePropsHandler Lightnings;
        public AnimatePropsHandler LightningsOnCar;

        public PropsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            //Wheels
            RRWheels = new AnimatePropsHandler();
            RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lf"));
            RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lr"));
            RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rf"));
            RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rr"));

            if (Vehicle.Bones["wheel_lm1"].Index > 0)
                RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lm1"));

            if (Vehicle.Bones["wheel_rm1"].Index > 0)
                RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rm1"));

            if (Vehicle.Bones["wheel_lm2"].Index > 0)
                RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lm2"));

            if (Vehicle.Bones["wheel_rm2"].Index > 0)
                RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rm2"));

            WhiteSphere = new AnimateProp(Vehicle, ModelHandler.WhiteSphere, Vector3.Zero, Vector3.Zero)
            {
                Duration = 0.25f
            };

            InvisibleProp = new AnimateProp(TimeMachine.Vehicle, ModelHandler.InvisibleProp, new Vector3(0, 3.4f, -0.6f), new Vector3(0, 0, 180));
            InvisibleProp.SpawnProp();

            //Lightnings
            Lightnings = new AnimatePropsHandler() { SequenceSpawn = true, SequenceInterval = 100, IsSequenceRandom = true };
            foreach (CustomModel x in ModelHandler.Lightnings)
                Lightnings.Add(new AnimateProp(Vehicle, x, Vector3.Zero, Vector3.Zero));

            LightningsOnCar = new AnimatePropsHandler() { SequenceSpawn = true, SequenceInterval = 100, IsSequenceRandom = true };
            foreach (CustomModel x in ModelHandler.LightningsOnCar)
                LightningsOnCar.Add(new AnimateProp(Vehicle, x, Vector3.Zero, Vector3.Zero));

            if (!Mods.IsDMC12)
                return;

            BTTFDecals = new AnimateProp(Vehicle, ModelHandler.BTTFDecals, Vector3.Zero, Vector3.Zero);
            BTTFDecals.SpawnProp();

            //Hover Mode
            HoverModeUnderbodyLights = new AnimatePropsHandler() { SequenceSpawn = true, SequenceInterval = 200, IsSequenceLooped = true };
            foreach (CustomModel model in ModelHandler.UnderbodyLights)
                HoverModeUnderbodyLights.Add(new AnimateProp(Vehicle, model, Vector3.Zero, Vector3.Zero));

            HoverModeVentsGlow = new AnimateProp(Vehicle, ModelHandler.VentGlowing, Vector3.Zero, Vector3.Zero);
            HoverModeWheelsGlow = new AnimateProp(Vehicle, ModelHandler.HoverGlowing, Vector3.Zero, Vector3.Zero)
            {
                Duration = 1.8f
            };

            //Fuel
            EmptyGlowing = new AnimateProp(Vehicle, ModelHandler.Empty, Vector3.Zero, Vector3.Zero);
            EmptyOff = new AnimateProp(Vehicle, ModelHandler.EmptyOff, Vector3.Zero, Vector3.Zero);

            //Time travel
            //Coils = new AnimateProp(Vehicle, ModelHandler.CoilsGlowing, Vector3.Zero, Vector3.Zero);
            SeparatedCoils = new AnimatePropsHandler();
            foreach (CustomModel coilModel in ModelHandler.CoilSeparated)
                SeparatedCoils.Add(new AnimateProp(TimeMachine.Vehicle, coilModel, Vector3.Zero, Vector3.Zero));

            //Plutonium gauge
            GaugeGlow = new AnimateProp(Vehicle, ModelHandler.GaugeGlow, Vector3.Zero, Vector3.Zero);

            //Speedo
            Speedo = new AnimateProp(Vehicle, ModelHandler.BTTFSpeedo, "bttf_speedo");
            Speedo.SpawnProp();

            //Compass
            Compass = new AnimateProp(Vehicle, ModelHandler.Compass, "bttf_compass");

            //TFC
            TFCOn = new AnimateProp(Vehicle, ModelHandler.TFCOn, Vector3.Zero, Vector3.Zero);
            TFCOff = new AnimateProp(Vehicle, ModelHandler.TFCOff, Vector3.Zero, Vector3.Zero);
            TFCHandle = new AnimateProp(Vehicle, ModelHandler.TFCHandle, new Vector3(-0.03805999f, -0.0819466f, 0.5508024f), Vector3.Zero);
            TFCHandle[AnimationType.Rotation][AnimationStep.First][Coordinate.Y].Setup(true, false, -45, 0, 1, 135, 1);
            TFCHandle.SpawnProp();

            //TCD
            DiodesOff = new AnimateProp(Vehicle, ModelHandler.DiodesOff, Vector3.Zero, Vector3.Zero);
            DiodesOff.SpawnProp();
            TickingDiodes = new AnimateProp(Vehicle, ModelHandler.TickingDiodes, Vector3.Zero, Vector3.Zero);
            TickingDiodesOff = new AnimateProp(Vehicle, ModelHandler.TickingDiodesOff, Vector3.Zero, Vector3.Zero);
            TickingDiodesOff.SpawnProp();

            //Hoodbox lights
            HoodboxLights = new AnimateProp(Vehicle, ModelHandler.HoodboxLights, "bonnet");

            //Flux capacitor
            FluxBlue = new AnimateProp(Vehicle, ModelHandler.FluxBlueModel, "flux_capacitor");

            //License plate
            LicensePlate = new AnimateProp(Vehicle, ModelHandler.LicensePlate, Vehicle.GetPositionOffset(Vehicle.RearPosition).GetSingleOffset(Coordinate.Z, 0.0275f), new Vector3(30, -90, 90));
            LicensePlate[AnimationType.Rotation][AnimationStep.First][Coordinate.Z].Setup(true, true, 90, 360 * 2 + 90, 1, 1440, 1);
            LicensePlate.SaveAnimation();
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
            Speedo?.Dispose();

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

            //License plate
            LicensePlate?.Dispose(LicensePlate != null && LicensePlate.IsSpawned);

            //Lightnings
            Lightnings?.Dispose();
            LightningsOnCar?.Dispose();
        }

        public override void KeyDown(Keys key)
        {
            //if (key == Keys.L)
            //{
            //    Sounds.Plate.Play();
            //    LicensePlate.Play(false, true);
            //}

            //if (key == Keys.O)
            //    LicensePlate.Delete();
        }

        public override void Process()
        {
            if (Mods.IsDMC12 && Props.LicensePlate.IsPlaying)
            {
                if (LicensePlate[AnimationType.Rotation][AnimationStep.First][Coordinate.Z].StepRatio > 0.1f)
                    LicensePlate[AnimationType.Rotation][AnimationStep.First][Coordinate.Z].StepRatio -= Game.LastFrameTime;
            }
        }

        public override void Stop()
        {

        }
    }
}
