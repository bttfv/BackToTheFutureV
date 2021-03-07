using FusionLibrary;
using GTA.Math;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers
{
    internal class ParticlesHandler : Handler
    {
        //Hover Mode
        public List<PtfxEntityPlayer> HoverModeSmoke = new List<PtfxEntityPlayer>();

        //Ice
        public PtfxEntityPlayer IceVentLeftSmoke;
        public PtfxEntityPlayer IceVentRightSmoke;
        public PtfxEntityPlayer IceSmoke;
        public PtfxEntityBonePlayer[] IceWaterDrops;
        public PtfxEntityBonePlayer MrFusionSmoke;

        //Time travel
        public PtfxEntityPlayer LightExplosion;
        public PtfxEntityPlayer TimeTravelEffect;
        public List<PtfxEntityPlayer> WheelsFire;
        public List<PtfxEntityPlayer> WheelsSparks;

        //Reenter
        public PtfxEntityPlayer Flash;

        //Sparks
        public PtfxEntityBonePlayer LightningSparks;
        public PtfxEntityPlayer Sparks;

        public ParticlesHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            //Hover Mode
            foreach (CVehicleWheel wheel in Mods.Wheels)
                HoverModeSmoke.Add(new PtfxEntityPlayer("cut_trevor1", "cs_meth_pipe_smoke", Vehicle, wheel.Position, new Vector3(-90, 0, 0), 7f));

            LightExplosion = new PtfxEntityPlayer("scr_josh3", "scr_josh3_light_explosion", Vehicle, Vector3.Zero, Vector3.Zero, 4f);
            TimeTravelEffect = new PtfxEntityPlayer("core", "veh_exhaust_spacecraft", Vehicle, new Vector3(0, 4, 0), Vector3.Zero, 8f, true);
            Flash = new PtfxEntityPlayer("core", "ent_anim_paparazzi_flash", Vehicle, Vector3.Zero, Vector3.Zero, 50f);
            Sparks = new PtfxEntityPlayer("scr_paletoscore", "scr_paleto_box_sparks", Props.InvisibleProp, Vector3.Zero, Vector3.Zero, 1.5f, true, true, 300);
            WheelsFire = SetupWheelPTFXs("veh_xs_vehicle_mods", "veh_nitrous", new Vector3(0, -0.25f, -0.15f), new Vector3(0, 0, 0), 1.3f);
            WheelsSparks = SetupWheelPTFXs("des_bigjobdrill", "ent_ray_big_drill_start_sparks", new Vector3(0, 0, 0.18f), new Vector3(90f, 0, 0), 1f, true);

            if (!Mods.IsDMC12)
                return;

            //Ice
            IceVentLeftSmoke = new PtfxEntityPlayer("scr_familyscenem", "scr_meth_pipe_smoke", Vehicle, new Vector3(0.5f, -2f, 0.7f), new Vector3(10f, 0, 180f), 10f, false, false);
            IceVentRightSmoke = new PtfxEntityPlayer("scr_familyscenem", "scr_meth_pipe_smoke", Vehicle, new Vector3(-0.5f, -2f, 0.7f), new Vector3(10f, 0, 180f), 10f, false, false);
            IceSmoke = new PtfxEntityPlayer("core", "ent_amb_dry_ice_area", Vehicle, new Vector3(0, 0, 0.5f), Vector3.Zero, 3f, true);
            MrFusionSmoke = new PtfxEntityBonePlayer("core", "ent_amb_dry_ice_area", Vehicle, "mr_fusion", Vector3.Zero, Vector3.Zero, 0.5f, true);

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

            //Sparks
            LightningSparks = new PtfxEntityBonePlayer("core", "ent_ray_finale_vault_sparks", Vehicle, "bttf_reactorcap", new Vector3(0, 0, 0.1f), Vector3.Zero, 3, true);
        }

        private List<PtfxEntityPlayer> SetupWheelPTFXs(string particleAssetName, string particleName, Vector3 wheelOffset, Vector3 wheelRot, float size = 3f, bool doLoopHandling = false)
        {
            List<PtfxEntityPlayer> ret = new List<PtfxEntityPlayer>();

            foreach (CVehicleWheel wheel in Mods.Wheels)
                ret.Add(new PtfxEntityPlayer(particleAssetName, particleName, Vehicle, wheel.GetRelativeOffsetPosition(wheelOffset), wheelRot, size, true, doLoopHandling));

            return ret;
        }

        public override void Dispose()
        {
            LightExplosion?.Dispose();
            TimeTravelEffect?.Dispose();
            WheelsFire?.ForEach(x => x?.Dispose());
            WheelsSparks?.ForEach(x => x?.Dispose());
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
