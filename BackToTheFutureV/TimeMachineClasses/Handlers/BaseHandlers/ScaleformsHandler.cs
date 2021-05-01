using FusionLibrary;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BackToTheFutureV
{
    internal class ScaleformsHandler : HandlerPrimitive
    {
        //TCD 2D
        public static TCD2DScaleform GUI { get; private set; }

        //TCD 3D
        public static Dictionary<string, TCD3DRowScaleform> TCDRowsScaleforms { get; private set; } = new Dictionary<string, TCD3DRowScaleform>()
        {
            { "red", new TCD3DRowScaleform("red") { DrawInPauseMenu = true } },
            { "yellow", new TCD3DRowScaleform("yellow") { DrawInPauseMenu = true } },
            { "green", new TCD3DRowScaleform("green") { DrawInPauseMenu = true } }
        };
        public Dictionary<string, RenderTarget> TCDRowsRT { get; private set; } = new Dictionary<string, RenderTarget>();

        //Wormhole
        public static List<ScaleformGui> WormholeScaleforms { get; private set; } = new List<ScaleformGui>()
        {
            { new ScaleformGui("bttf_wormhole_scaleform") { DrawInPauseMenu = true } },
            { new ScaleformGui("bttf_wormhole_scaleform_blue") { DrawInPauseMenu = true } },
            { new ScaleformGui("bttf_wormhole_scaleform_red") { DrawInPauseMenu = true } }
        };
        public RenderTarget WormholeRT { get; set; }

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

        //RC GUI
        public static RCGUIScaleform RCGUI { get; private set; }

        static ScaleformsHandler()
        {
            GUI = new TCD2DScaleform();
            FluxCapacitor = new ScaleformGui("bttf_flux_scaleform") { DrawInPauseMenu = true };
            Speedo = new ScaleformGui("bttf_3d_speedo") { DrawInPauseMenu = true };

            SID2D = new SIDScaleform(true);
            SID3D = new SIDScaleform(false);

            RCGUI = new RCGUIScaleform();
        }

        public ScaleformsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            if (!Mods.IsDMC12)
                return;

            Events.OnScaleformPriority += OnScaleformPriority;

            //Flux Capacitor            
            FluxCapacitorRT = new RenderTarget(ModelHandler.FluxModel, "bttf_flux", Vehicle, "flux_capacitor");

            FluxCapacitorRT.OnRenderTargetDraw += () =>
            {
                FluxCapacitor.Render2D(new PointF(0.5f, 0.5f), 1f);
            };

            //Speedo
            SpeedoRT = new RenderTarget(ModelHandler.BTTFSpeedo, "bttf_speedo", Vehicle, "bttf_speedo");

            SpeedoRT.OnRenderTargetDraw += () =>
            {
                Speedo.Render2D(new PointF(0.5f, 0.54f), new SizeF(0.9f, 0.9f));
            };

            //SID
            SIDRT = new RenderTarget(ModelHandler.SID, "bttf_sid", Vehicle, "bttf_sid");

            SIDRT.OnRenderTargetDraw += () =>
            {
                SID3D.Draw3D();
            };
        }

        private void OnScaleformPriority()
        {
            if (Constants.HasScaleformPriority)
            {
                SIDRT?.CreateProp();
                FluxCapacitorRT?.CreateProp();
                SpeedoRT.CreateProp();
            }
            else
            {
                SIDRT?.Dispose();
                FluxCapacitorRT?.Dispose();
                SpeedoRT?.Dispose();
            }
        }

        public override void Dispose()
        {
            FluxCapacitorRT?.Dispose();
            SpeedoRT?.Dispose();
            SIDRT?.Dispose();
            TCDRowsRT.Values.ToList().ForEach(x => x?.Dispose());
            WormholeRT?.Dispose();
        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        public override void Tick()
        {

        }

        public override void Stop()
        {

        }
    }
}
