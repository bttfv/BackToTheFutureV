using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class PhotoHandler : Handler
    {
         public PhotoHandler(TimeMachine timeMachine) : base(timeMachine)
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

        }

        public override void Process()
        {
            if (Properties.PhotoWormholeActive && !Players.Wormhole.IsPlaying)
                Players.Wormhole.Play(true);

            if (!Properties.PhotoWormholeActive && Players.Wormhole.IsPlaying && Properties.IsPhotoModeOn)
                Players.Wormhole.Stop();

            if (Properties.PhotoGlowingCoilsActive && !Props.Coils.IsSpawned)
            {
                if (Main.CurrentTime.Hour >= 20 || (Main.CurrentTime.Hour >= 0 && Main.CurrentTime.Hour <= 5))
                    Props.Coils.Model = ModelHandler.CoilsGlowingNight;
                else
                    Props.Coils.Model = ModelHandler.CoilsGlowing;

                Mods.OffCoils = ModState.Off;
                Props.Coils.SpawnProp(false);
            }

            if (!Properties.PhotoGlowingCoilsActive && Props.Coils.IsSpawned)
            {
                Mods.OffCoils = ModState.On;
                Props.Coils.DeleteProp();
            }

            if (Properties.PhotoFluxCapacitorActive && !Properties.IsFluxDoingBlueAnim)
                Events.OnWormholeStarted?.Invoke();

            if (!Properties.PhotoFluxCapacitorActive && Properties.IsFluxDoingBlueAnim && Properties.IsPhotoModeOn)
                Events.OnTimeTravelInterrupted?.Invoke();

            if (Properties.PhotoIceActive && !Properties.IsFreezed)
                Events.SetFreeze?.Invoke(true);

            if (!Properties.PhotoIceActive && Properties.IsFreezed && Properties.IsPhotoModeOn)
                Events.SetFreeze?.Invoke(false);

            Properties.IsPhotoModeOn = Properties.PhotoWormholeActive | Properties.PhotoGlowingCoilsActive | Properties.PhotoFluxCapacitorActive | Properties.PhotoIceActive;
        }

        public override void Stop()
        {
            
        }
    }
}
