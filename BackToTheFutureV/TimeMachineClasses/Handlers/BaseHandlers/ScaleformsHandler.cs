using BackToTheFutureV.GUI;
using BackToTheFutureV.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Screen = GTA.UI.Screen;

namespace BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers
{
    public class ScaleformsHandler : Handler
    {
        public TimeCircuitsScaleform GUI { get; private set; }

        //Flux Capacitor
        public ScaleformGui FluxCapacitor { get; private set; }
        public RenderTarget FluxCapacitorRT { get; private set; }

        //Speedo
        public ScaleformGui Speedo { get; private set; }
        public RenderTarget SpeedoRT { get; private set; }

        public ScaleformsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            GUI = new TimeCircuitsScaleform("bttf_2d_gui");

            //Flux Capacitor
            FluxCapacitor = new ScaleformGui("bttf_flux_scaleform");
            FluxCapacitorRT = new RenderTarget(ModelHandler.FluxModel, "bttf_flux", Vehicle, "flux_capacitor");
            FluxCapacitor.DrawInPauseMenu = true;

            FluxCapacitorRT.OnRenderTargetDraw += () =>
            {
                FluxCapacitor.Render2D(new PointF(0.5f, 0.5f), 1f);
            };

            FluxCapacitorRT.CreateProp();

            //Speedo
            SpeedoRT = new RenderTarget(ModelHandler.BTTFSpeedo, "bttf_speedo");
            Speedo = new ScaleformGui("bttf_3d_speedo")
            {
                DrawInPauseMenu = true
            };

            SpeedoRT.OnRenderTargetDraw += () =>
            {
                var aspectRatio = Screen.Resolution.Width / (float)Screen.Resolution.Height;

                Speedo.Render2D(new PointF(0.5f, 0.5f), new SizeF(0.9f, 0.9f));
            };
        }

        public override void Dispose()
        {
            FluxCapacitorRT?.Dispose();
        }

        public override void KeyPress(Keys key)
        {
            
        }

        public override void Process()
        {
            
        }

        public override void Stop()
        {
            
        }
    }
}
