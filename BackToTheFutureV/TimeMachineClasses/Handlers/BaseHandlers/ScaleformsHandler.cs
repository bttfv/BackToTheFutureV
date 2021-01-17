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
        public static SIDScaleform SID2D { get; private set; }
        public static SIDScaleform SID3D { get; private set; }
        public RenderTarget SIDRT { get; private set; }

        static ScaleformsHandler()
        {
            GUI = new TimeCircuitsScaleform("bttf_2d_gui");
            FluxCapacitor = new ScaleformGui("bttf_flux_scaleform") { DrawInPauseMenu = true };
            Speedo = new ScaleformGui("bttf_3d_speedo") { DrawInPauseMenu = true };

            SID2D = new SIDScaleform("bttf_2d_sid");
            SID3D = new SIDScaleform("bttf_3d_sid");
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
                Speedo.Render2D(new PointF(0.5f, 0.5f), new SizeF(0.9f, 0.9f));
            };

            if (!Mods.IsDMC12)
                return;

            //SID
            SIDRT = new RenderTarget(ModelHandler.SID, "bttf_sid", Vehicle, "bttf_sid");

            SIDRT.OnRenderTargetDraw += () =>
            {
                SID3D.Draw3D();

                //GTA.World.DrawLine(SIDRT.Prop.WorldPosition, SIDRT.Prop.Prop.ForwardVector + SIDRT.Prop.WorldPosition, Color.Blue);
            };

            SIDRT.CreateProp();
        }

        public override void Dispose()
        {
            FluxCapacitorRT?.Dispose();
            SpeedoRT?.Dispose();
            SIDRT?.Dispose();
        }

        public override void KeyDown(Keys key)
        {
            if (!Mods.IsDMC12 || Utils.PlayerVehicle != Vehicle)
                return;

            switch (key)
            {
                case Keys.D1:
                    SID3D.x += 0.001f;
                    break;
                case Keys.D2:
                    SID3D.x -= 0.001f;
                    break;
                case Keys.D3:
                    SID3D.y += 0.001f;
                    break;
                case Keys.D4:
                    SID3D.y -= 0.001f;
                    break;
                case Keys.D5:
                    SID3D.scale += 0.001f;
                    break;
                case Keys.D6:
                    SID3D.scale -= 0.001f;
                    break;
            }
        }

        public override void Process()
        {
            if (!Mods.IsDMC12 || Utils.PlayerVehicle != Vehicle)
                return;

            SID2D?.Process();
            SID2D.CallFunction("setBackground", 2);

            SID3D?.Process();
            SID3D.CallFunction("setBackground", 2);
        }

        public override void Stop()
        {
            
        }
    }
}
