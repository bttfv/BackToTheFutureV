using FusionLibrary;
using FusionLibrary.Extensions;
using GTA.Math;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers
{
    public class ParticlesHandler : Handler
    {
        //Hover Mode
        public List<PtfxEntityPlayer> HoverModeSmoke = new List<PtfxEntityPlayer>();

        //Ice
        public PtfxEntityPlayer IceVentLeftSmoke;
        public PtfxEntityPlayer IceVentRightSmoke;
        public PtfxEntityPlayer IceSmoke;
        public PtfxEntityBonePlayer[] IceWaterDrops;

        //Time travel
        public PtfxEntityPlayer LightExplosion;
        public PtfxEntityPlayer TimeTravelEffect;

        //Reenter
        public PtfxEntityPlayer Flash;

        public ParticlesHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            //Hover Mode
            foreach (var wheelPos in Vehicle.GetWheelPositions())
                HoverModeSmoke.Add(new PtfxEntityPlayer("cut_trevor1", "cs_meth_pipe_smoke", Vehicle, wheelPos.Value, new Vector3(-90, 0, 0), 7f));

            LightExplosion = new PtfxEntityPlayer("scr_josh3", "scr_josh3_light_explosion", Vehicle, Vector3.Zero, Vector3.Zero, 4f);
            TimeTravelEffect = new PtfxEntityPlayer("core", "veh_exhaust_spacecraft", Vehicle, new Vector3(0, 4, 0), Vector3.Zero, 8f, true);
            Flash = new PtfxEntityPlayer("core", "ent_anim_paparazzi_flash", Vehicle, Vector3.Zero, Vector3.Zero, 50f);

            if (!Mods.IsDMC12)
                return;

            //Ice
            IceVentLeftSmoke = new PtfxEntityPlayer("scr_familyscenem", "scr_meth_pipe_smoke", Vehicle, new Vector3(0.5f, -2f, 0.7f), new Vector3(10f, 0, 180f), 10f, false, false);
            IceVentRightSmoke = new PtfxEntityPlayer("scr_familyscenem", "scr_meth_pipe_smoke", Vehicle, new Vector3(-0.5f, -2f, 0.7f), new Vector3(10f, 0, 180f), 10f, false, false);
            IceSmoke = new PtfxEntityPlayer("core", "ent_amb_dry_ice_area", Vehicle, new Vector3(0, 0, 0.5f), Vector3.Zero, 3f, true);

            IceWaterDrops = new PtfxEntityBonePlayer[3 * 2];

            for (int i = 0, c = 0; i < IceWaterDrops.Length / 2; i++)
            {
                for (int k = 0; k < 2; k++)
                {
                    string dummyName = k == 0 ? "ice_drop_left" : "ice_drop_right";
                    dummyName += i + 1;

                    IceWaterDrops[c] = new PtfxEntityBonePlayer("scr_apartment_mp", "scr_apa_jacuzzi_drips", Vehicle, dummyName, Vector3.Zero, Vector3.Zero, 3f, true);
                    c++;
                }
            }
        }

        public override void Dispose()
        {
            LightExplosion?.Dispose();
            TimeTravelEffect?.Dispose();
        }

        public override void KeyDown(Keys key)
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
