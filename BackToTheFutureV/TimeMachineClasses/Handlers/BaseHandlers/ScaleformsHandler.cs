using BackToTheFutureV.GUI;
using BackToTheFutureV.Utility;
using FusionLibrary;
using System.Drawing;
using System.Windows.Forms;
using Screen = GTA.UI.Screen;

namespace BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers
{
    public class ScaleformsHandler : Handler
    {
        public static TimeCircuitsScaleform GUI { get; private set; }

        //Flux Capacitor
        public static ScaleformGui FluxCapacitor { get; private set; }
        public RenderTarget FluxCapacitorRT { get; private set; }

        //Speedo
        public static ScaleformGui Speedo { get; private set; }
        public RenderTarget SpeedoRT { get; private set; }

        //SID
        public static SIDScaleform SID { get; private set; }
        public static SIDScaleform SID3D { get; private set; }

        static ScaleformsHandler()
        {
            GUI = new TimeCircuitsScaleform("bttf_2d_gui");
            FluxCapacitor = new ScaleformGui("bttf_flux_scaleform") { DrawInPauseMenu = true };
            Speedo = new ScaleformGui("bttf_3d_speedo") { DrawInPauseMenu = true };

            SID = new SIDScaleform("sid");
            SID3D = new SIDScaleform("sid_3d");
        }

        public ScaleformsHandler(TimeMachine timeMachine) : base(timeMachine)
        {            
            //Flux Capacitor            
            FluxCapacitorRT = new RenderTarget(ModelHandler.FluxModel, "bttf_flux", Vehicle, "flux_capacitor");

            FluxCapacitorRT.OnRenderTargetDraw += () =>
            {
                FluxCapacitor.Render2D(new PointF(0.5f, 0.5f), 1f);
            };

            FluxCapacitorRT.CreateProp();

            //Speedo
            SpeedoRT = new RenderTarget(ModelHandler.BTTFSpeedo, "bttf_speedo");

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

        public override void KeyDown(Keys key)
        {
            
        }

        public override void Process()
        {
            if (!Mods.IsDMC12 || Utils.PlayerVehicle != Vehicle)
                return;

            SID?.Process();
            SID3D?.Process();
        }

        public override void Stop()
        {
            
        }
    }
}
