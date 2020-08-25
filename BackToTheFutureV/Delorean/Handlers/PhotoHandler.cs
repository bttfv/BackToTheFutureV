using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class PhotoHandler : Handler
    {
        public bool WormholeActive;
        public bool GlowingCoilsActive;
        public bool IceActive;
        public bool FluxCapacitorActive;

        private AnimateProp _coilsProp;

        public PhotoHandler(TimeCircuits circuits) : base(circuits)
        {
            _coilsProp = new AnimateProp(TimeCircuits.Vehicle, ModelHandler.CoilsGlowing, Vector3.Zero, Vector3.Zero);
        }

        public override void Dispose()
        {
            
        }

        public override void KeyPress(Keys key)
        {
            
        }

        public override void Process()
        {
            if (!IsPhotoModeOn)
                return;

            if (WormholeActive && !TimeCircuits.GetHandler<SparksHandler>().IsWormholePlaying)
                TimeCircuits.GetHandler<SparksHandler>().StartWormhole();

            if (!WormholeActive && TimeCircuits.GetHandler<SparksHandler>().IsWormholePlaying)
                TimeCircuits.GetHandler<SparksHandler>().StopWormhole();

            if (GlowingCoilsActive && !_coilsProp.IsSpawned)
            {
                if (Main.CurrentTime.Hour >= 20 || (Main.CurrentTime.Hour >= 0 && Main.CurrentTime.Hour <= 5))
                    _coilsProp.Model = ModelHandler.CoilsGlowingNight;
                else
                    _coilsProp.Model = ModelHandler.CoilsGlowing;

                _coilsProp.SpawnProp();
            }
               
            if (!GlowingCoilsActive && _coilsProp.IsSpawned)
                _coilsProp.DeleteProp();

            if (FluxCapacitorActive && !TimeCircuits.GetHandler<FluxCapacitorHandler>().TimeTravelEffect)
                TimeCircuits.GetHandler<FluxCapacitorHandler>().StartTimeTravelEffect();

            if (!FluxCapacitorActive && TimeCircuits.GetHandler<FluxCapacitorHandler>().TimeTravelEffect)
                TimeCircuits.GetHandler<FluxCapacitorHandler>().StartNormalFluxing();

            if (IceActive && !IsFreezing)
                TimeCircuits.GetHandler<FreezeHandler>().StartFreezeHandling(false);

            if (!IceActive && IsFreezing)
                TimeCircuits.GetHandler<FreezeHandler>().Stop();

            IsPhotoModeOn = WormholeActive | GlowingCoilsActive | FluxCapacitorActive | IceActive;
        }

        public override void Stop()
        {
            
        }
    }
}
