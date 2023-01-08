using FusionLibrary;
using GTA;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static System.Windows.Forms.AxHost;

namespace BackToTheFutureV
{
    internal class HoverHandler : HandlerPrimitive
    {        
        public HoverHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Players.HoverModeWheels = new WheelAnimationPlayer(TimeMachine);
            Players.HoverModeWheels.OnPlayerCompleted += OnCompleted;

            HoverVehicle.OnSwitchHoverMode += HoverVehicle_OnSwitchHoverMode;
            HoverVehicle.OnHoverBoost += HoverVehicle_OnHoverBoost;
            HoverVehicle.OnVerticalBoost += HoverVehicle_OnVerticalBoost;
            HoverVehicle.OnHoverLanding += HoverVehicle_OnHoverLanding;

            TimeHandler.OnDayNightChange += OnDayNightChange;

            OnDayNightChange();

            if (HoverVehicle.IsInHoverMode)
                HoverVehicle_OnSwitchHoverMode(true);
        }

        private void HoverVehicle_OnHoverLanding(bool state)
        {
            if (state)
            {
                Players.HoverModeWheels.Play(false);
                Sounds.HoverModeOff?.Play();
            }
            else if (!Players.HoverModeWheels.AreWheelsOpen)
            {
                Players.HoverModeWheels.Play(true);
                Sounds.HoverModeOn?.Play();
            }
        }

        private void HoverVehicle_OnVerticalBoost(bool state, bool up)
        {
            if (state && up)
            {
                Props.HoverModeWheelsGlow?.SpawnProp();
                Sounds.HoverModeUp?.Play();
            }
            else
            {
                Props.HoverModeWheelsGlow?.Delete();
                Sounds.HoverModeUp?.Stop();
            }
        }

        private void HoverVehicle_OnHoverBoost(bool state)
        {
            if (state)
            {
                Props.HoverModeVentsGlow?.SpawnProp();
                Sounds.HoverModeBoost?.Play();
            }
            else
            {
                Props.HoverModeVentsGlow?.Delete();
                Sounds.HoverModeBoost?.Stop();
            }
        }

        private void HoverVehicle_OnSwitchHoverMode(bool state)
        {
            if (!state && !Players.HoverModeWheels.AreWheelsOpen)
            {
                Players.HoverModeWheels.Stop();
                return;
            }
                
            Players.HoverModeWheels.Play(state);

            if (state)
                Sounds.HoverModeOn?.Play();
            else
                Sounds.HoverModeOff?.Play();
        }

        private void OnCompleted()
        {

        }

        private void OnDayNightChange()
        {
            Props.HoverModeVentsGlow?.Delete();

            if (TimeHandler.IsNight)
            {
                Props.HoverModeVentsGlow.SwapModel(ModelHandler.VentGlowingNight);
            }
            else
            {
                Props.HoverModeVentsGlow.SwapModel(ModelHandler.VentGlowing);
            }
        }

        public override void Dispose()
        {

        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        public override void Stop()
        {

        }

        public override void Tick()
        {
            HoverVehicle.SoftLock = Players.HoverModeWheels.IsPlaying;

            if (Mods.IsDMC12)
            {
                bool newValue = Mods.HoverUnderbody == ModState.On && !Properties.AreFlyingCircuitsBroken;

                if (newValue != HoverVehicle.IsHoverModeAllowed)
                {
                    HoverVehicle.IsHoverModeAllowed = newValue;
                }
            }
        }
    }
}
