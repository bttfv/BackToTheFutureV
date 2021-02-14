using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Native;
using System.Windows.Forms;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class LightningStrikeHandler : Handler
    {
        private int _nextCheck;
        private int _delay = -1;

        public LightningStrikeHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.StartLightningStrike += PrepareStrike;
        }

        private void PrepareStrike(int delay)
        {
            _delay = Game.GameTime + delay * 1000;
        }

        public override void Process()
        {
            if (Props.Lightnings.IsSequencePlaying && !Vehicle.IsVisible)
                Props.Lightnings.Delete();

            if (_delay > -1 && Game.GameTime > _delay)
                Strike();

            if (!ModSettings.LightningStrikeEvent || World.Weather != Weather.ThunderStorm || Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole || Game.GameTime < _nextCheck)
                return;

            if ((Mods.IsDMC12 && Mods.Hook == HookState.On && Vehicle.GetMPHSpeed() >= 88 && !Properties.IsFlying) | (Vehicle.HeightAboveGround >= 20 && Properties.IsFlying))
            {
                if (Utils.Random.NextDouble() < 0.2)
                    Strike();
                else
                    _nextCheck = Game.GameTime + 10000;
            }
        }

        private void Strike()
        {
            Properties.HasBeenStruckByLightning = true;

            Props.Lightnings.IsSequenceLooped = Properties.AreTimeCircuitsOn;

            Props.Lightnings.Play();

            if (Properties.AreTimeCircuitsOn)
            {
                Events.SetSIDLedsState?.Invoke(true, true);

                Properties.PhotoFluxCapacitorActive = true;
                Properties.PhotoGlowingCoilsActive = true;

                // Time travel by lightning strike
                Sounds.LightningStrike?.Play();

                if (Mods.Hook == HookState.On && !Properties.IsFlying)
                    Events.StartTimeTravel?.Invoke(500);
                else
                    Events.StartTimeTravel?.Invoke(2000);

                TimeMachineClone timeMachineClone = TimeMachine.Clone;
                timeMachineClone.Properties.DestinationTime = timeMachineClone.Properties.DestinationTime.AddYears(70);
                RemoteTimeMachineHandler.AddRemote(timeMachineClone);
            }
            else
            {
                Function.Call(Hash.FORCE_LIGHTNING_FLASH);

                if (Properties.IsFlying)
                    Properties.AreFlyingCircuitsBroken = true;
            }

            Vehicle.EngineHealth -= 700;

            _nextCheck = Game.GameTime + 60000;
            _delay = -1;

            Events.OnLightningStrike?.Invoke();
        }

        public override void KeyDown(Keys key)
        {
            //if (key == Keys.L)
            //    Props.Lightnings.Play();
        }

        public override void Stop()
        {

        }

        public override void Dispose()
        {

        }
    }
}