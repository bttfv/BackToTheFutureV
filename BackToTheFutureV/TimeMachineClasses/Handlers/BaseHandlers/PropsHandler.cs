using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers
{
    public class PropsHandler : Handler
    {
        private AnimateProp _BTTFDecals;

        //Hover Mode
        public AnimateProp HoverModeWheelsGlow;
        public AnimateProp HoverModeVentsGlow;
        public List<AnimateProp> HoverModeUnderbodyLights = new List<AnimateProp>();

        //Fuel
        public AnimateProp EmptyGlowing;
        public AnimateProp EmptyOff;

        //Hoodbox
        public AnimateProp HoodboxLights;

        //Time travel
        public AnimateProp Coils;

        //Plutonium gauge
        public AnimateProp GaugeGlow;

        //Speedo
        public AnimateProp Speedo;

        //Compass
        private readonly AnimateProp _compass;

        //TFC
        public AnimateProp TFCOn;
        public AnimateProp TFCOff;
        public AnimateProp TFCHandle;

        //Time travel
        public AnimateProp WhiteSphere;

        //Wheels
        public List<AnimateProp> RRWheels = new List<AnimateProp>();

        public PropsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            //Wheels
            RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lf"));
            RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lr"));
            RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rf"));
            RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rr"));

            HoverModeWheelsGlow = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.HoverGlowing), Vector3.Zero, Vector3.Zero)
            {
                Duration = 1.7f
            };

            if (!Mods.IsDMC12)
                return;

            _BTTFDecals = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.BTTFDecals), Vector3.Zero, Vector3.Zero);
            _BTTFDecals.SpawnProp();

            //Hover Mode
            for (int i = 1; i < 6; i++)
            {
                if (!ModelHandler.UnderbodyLights.TryGetValue(i, out var model))
                    continue;

                ModelHandler.RequestModel(model);
                var prop = new AnimateProp(Vehicle, model, Vector3.Zero, Vector3.Zero);
                HoverModeUnderbodyLights.Add(prop);
            }

            HoverModeVentsGlow = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.VentGlowing), Vector3.Zero, Vector3.Zero);

            //Fuel
            EmptyGlowing = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.Empty), Vector3.Zero, Vector3.Zero);
            EmptyOff = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.EmptyOff), Vector3.Zero, Vector3.Zero);

            //Time travel
            Coils = new AnimateProp(Vehicle, ModelHandler.CoilsGlowing, Vector3.Zero, Vector3.Zero);

            //Plutonium gauge
            GaugeGlow = new AnimateProp(Vehicle, ModelHandler.GaugeGlow, Vector3.Zero, Vector3.Zero);

            //Speedo
            Speedo = new AnimateProp(Vehicle, ModelHandler.BTTFSpeedo, "bttf_speedo");
            Speedo.SpawnProp(false);

            //Compass
            _compass = new AnimateProp(Vehicle, ModelHandler.Compass, "bttf_compass");

            //TFC
            TFCOn = new AnimateProp(Vehicle, ModelHandler.TFCOn, Vector3.Zero, Vector3.Zero);
            TFCOff = new AnimateProp(Vehicle, ModelHandler.TFCOff, Vector3.Zero, Vector3.Zero);
            TFCHandle = new AnimateProp(Vehicle, ModelHandler.TFCHandle, new Vector3(-0.03805999f, -0.0819466f, 0.5508024f), Vector3.Zero);
            TFCHandle.SpawnProp(false);
        }

        public override void Dispose()
        {
            _BTTFDecals?.Dispose();

            //Hover Mode
            HoverModeWheelsGlow?.Dispose();
            HoverModeVentsGlow?.Dispose();

            foreach (var prop in HoverModeUnderbodyLights)
                prop?.Dispose();

            //Fuel
            EmptyGlowing?.DeleteProp();
            EmptyOff?.DeleteProp();

            //Hoodbox
            HoodboxLights?.DeleteProp();

            //Time travel
            Coils?.Dispose();

            //Plutonium gauge
            GaugeGlow?.Dispose();

            //Speedo
            Speedo?.Dispose();

            //Compass
            _compass?.Dispose();

            //TFC
            TFCOn?.Dispose();
            TFCOff?.Dispose();
            TFCHandle?.Dispose();

            //Wheels
            RRWheels?.ForEach(x => x?.Dispose());
        }

        public override void KeyPress(Keys key)
        {
            
        }

        public override void Process()
        {
            if (!Mods.IsDMC12)
                return;

            _compass?.SpawnProp(Vector3.Zero, new Vector3(0, 0, Vehicle.Heading), false);

            if (_compass.Prop.IsVisible != Vehicle.IsVisible)
                _compass.Prop.IsVisible = Vehicle.IsVisible;
        }

        public override void Stop()
        {
            
        }
    }
}
