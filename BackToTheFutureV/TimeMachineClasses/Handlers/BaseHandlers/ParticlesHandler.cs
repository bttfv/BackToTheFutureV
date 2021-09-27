using FusionLibrary;
using GTA.Math;
using System.Windows.Forms;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class ParticlesHandler : HandlerPrimitive
    {
        //Hover Mode
        public ParticlePlayerHandler HoverModeSmoke = new ParticlePlayerHandler();

        //Ice
        public ParticlePlayer IceVentLeftSmoke;
        public ParticlePlayer IceVentRightSmoke;
        public ParticlePlayer IceSmoke;
        public ParticlePlayer[] IceWaterDrops;
        public ParticlePlayer MrFusionSmoke;

        //Time travel
        public ParticlePlayer LightExplosion;
        public ParticlePlayer TimeTravelEffect;
        public ParticlePlayerHandler WheelsFire;
        public ParticlePlayerHandler WheelsSparks;

        //Reenter
        public ParticlePlayer Flash;

        //Sparks
        public ParticlePlayer LightningSparks;
        public ParticlePlayer Sparks;

        public ParticlesHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            //Hover Mode
            foreach (CVehicleWheel wheel in Mods.Wheels)
                HoverModeSmoke.Add("cut_trevor1", "cs_meth_pipe_smoke", ParticleType.NonLooped, Vehicle, wheel.Position, new Vector3(-90, 0, 0), 7f);

            LightExplosion = new ParticlePlayer("scr_josh3", "scr_josh3_light_explosion", ParticleType.NonLooped, Vehicle, Vector3.Zero, Vector3.Zero, 4f);
            TimeTravelEffect = new ParticlePlayer("core", "veh_exhaust_spacecraft", ParticleType.Looped, Vehicle, new Vector3(0, 4, 0), Vector3.Zero, 8f);
            Flash = new ParticlePlayer("core", "ent_anim_paparazzi_flash", ParticleType.NonLooped, Vehicle, Vector3.Zero, Vector3.Zero, 50f);
            Sparks = new ParticlePlayer("scr_paletoscore", "scr_paleto_box_sparks", ParticleType.ForceLooped, Props.InvisibleProp, Vector3.Zero, Vector3.Zero, 1.5f) { Interval = 300 };
            WheelsFire = SetupWheelPTFXs("veh_xs_vehicle_mods", "veh_nitrous", ParticleType.Looped, new Vector3(0, -0.25f, -0.15f), new Vector3(0, 0, 0), 1.3f);
            WheelsSparks = SetupWheelPTFXs("des_bigjobdrill", "ent_ray_big_drill_start_sparks", ParticleType.ForceLooped, new Vector3(0, 0, 0.18f), new Vector3(90f, 0, 0), 1f);

            if (!Mods.IsDMC12)
                return;

            //Ice
            IceVentLeftSmoke = new ParticlePlayer("scr_familyscenem", "scr_meth_pipe_smoke", ParticleType.NonLooped, Vehicle, new Vector3(0.5f, -2f, 0.7f), new Vector3(10f, 0, 180f), 10f);
            IceVentRightSmoke = new ParticlePlayer("scr_familyscenem", "scr_meth_pipe_smoke", ParticleType.NonLooped, Vehicle, new Vector3(-0.5f, -2f, 0.7f), new Vector3(10f, 0, 180f), 10f);
            IceSmoke = new ParticlePlayer("core", "ent_amb_dry_ice_area", ParticleType.Looped, Vehicle, new Vector3(0, 0, 0.5f), Vector3.Zero, 3f);
            MrFusionSmoke = new ParticlePlayer("core", "ent_amb_dry_ice_area", ParticleType.Looped, Vehicle, "mr_fusion", Vector3.Zero, Vector3.Zero, 0.5f);

            IceWaterDrops = new ParticlePlayer[3 * 2];

            for (int i = 0, c = 0; i < IceWaterDrops.Length / 2; i++)
            {
                for (int k = 0; k < 2; k++)
                {
                    string dummyName = k == 0 ? "ice_drop_left" : "ice_drop_right";
                    dummyName += i + 1;

                    IceWaterDrops[c] = new ParticlePlayer("scr_apartment_mp", "scr_apa_jacuzzi_drips", ParticleType.Looped, Vehicle, dummyName, Vector3.Zero, Vector3.Zero, 3f);
                    c++;
                }
            }

            //Sparks
            LightningSparks = new ParticlePlayer("core", "ent_ray_finale_vault_sparks", ParticleType.Looped, Vehicle, "bttf_reactorcap", new Vector3(0, 0, 0.1f), Vector3.Zero, 3);
        }

        private ParticlePlayerHandler SetupWheelPTFXs(string particleAssetName, string particleName, ParticleType particleType, Vector3 wheelOffset, Vector3 wheelRot, float size = 3f)
        {
            ParticlePlayerHandler ret = new ParticlePlayerHandler();

            foreach (CVehicleWheel wheel in Mods.Wheels)
            {
                wheel.Reset();
                ret.Add(particleAssetName, particleName, particleType, Vehicle, wheel.GetRelativeOffsetPosition(wheelOffset), wheelRot, size);
            }

            return ret;
        }

        public override void Dispose()
        {
            LightExplosion?.Dispose();
            TimeTravelEffect?.Dispose();
            WheelsFire?.Dispose();
            WheelsSparks?.Dispose();
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
