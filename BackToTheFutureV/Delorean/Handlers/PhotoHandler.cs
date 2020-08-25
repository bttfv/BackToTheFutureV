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

        public PhotoHandler(TimeCircuits circuits) : base(circuits)
        {

        }

        public override void Dispose()
        {
            
        }

        public override void KeyPress(Keys key)
        {
            
        }

        public void SetPhotoMode()
        {
            if (WormholeActive && !TimeCircuits.GetHandler<SparksHandler>().IsWormholePlaying)
                TimeCircuits.GetHandler<SparksHandler>().StartWormhole();

            if (!WormholeActive && TimeCircuits.GetHandler<SparksHandler>().IsWormholePlaying)
                TimeCircuits.GetHandler<SparksHandler>().StopWormhole();

            if (GlowingCoilsActive && !TimeCircuits.GetHandler<SparksHandler>().IsWormholePlaying)
                TimeCircuits.GetHandler<SparksHandler>().StartWormhole();

            if (!GlowingCoilsActive && TimeCircuits.GetHandler<SparksHandler>().IsWormholePlaying)
                TimeCircuits.GetHandler<SparksHandler>().StopWormhole();

            if (FluxCapacitorActive && !TimeCircuits.GetHandler<FluxCapacitorHandler>().TimeTravelEffect)
                TimeCircuits.GetHandler<FluxCapacitorHandler>().StartTimeTravelEffect();

            if (!FluxCapacitorActive && TimeCircuits.GetHandler<FluxCapacitorHandler>().TimeTravelEffect)
                TimeCircuits.GetHandler<FluxCapacitorHandler>().StartNormalFluxing();

            if (IceActive && !IsFreezing)
                TimeCircuits.GetHandler<FreezeHandler>().StartFreezeHandling(false);

            if (!IceActive && IsFreezing)
                TimeCircuits.GetHandler<FreezeHandler>().Stop();
        }

        public override void Process()
        {
            IsPhotoModeOn = WormholeActive | GlowingCoilsActive | IceActive | FluxCapacitorActive;
        }

        public override void Stop()
        {
            
        }
    }
}
