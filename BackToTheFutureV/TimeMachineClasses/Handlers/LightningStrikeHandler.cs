﻿using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Native;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class LightningStrikeHandler : HandlerPrimitive
    {
        private int _nextCheck;
        private int _delay = -1;
        private bool _instant;

        public LightningStrikeHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.StartLightningStrike += StartLightningStrike;
            Events.OnTimeTravelStarted += Stop;
        }

        private void StartLightningStrike(int delay)
        {
            if (delay == -1)
            {
                _instant = true;
                Strike();
                return;
            }

            _delay = Game.GameTime + delay * 1000;
        }

        public override void Tick()
        {
            if (_delay > -1 && Game.GameTime > _delay)
            {
                if (ModSettings.WaybackSystem && TimeMachineHandler.CurrentTimeMachine == TimeMachine)
                    WaybackSystem.CurrentPlayerRecording.LastRecord.Vehicle.Event |= WaybackVehicleEvent.LightningStrike;

                Strike();
            }

            if (!ModSettings.LightningStrikeEvent || World.Weather != Weather.ThunderStorm || Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole || Game.GameTime < _nextCheck)
                return;

            if ((Mods.IsDMC12 && Mods.Hook == HookState.On && Vehicle.GetMPHSpeed() >= 88 && !Properties.IsFlying) | (Vehicle.HeightAboveGround >= 20 && Properties.IsFlying))
            {
                if (FusionUtils.Random.NextDouble() < 0.3)
                {
                    if (ModSettings.WaybackSystem && TimeMachineHandler.CurrentTimeMachine == TimeMachine)
                        WaybackSystem.CurrentPlayerRecording.LastRecord.Vehicle.Event |= WaybackVehicleEvent.LightningStrike;

                    Strike();
                }
                else
                    _nextCheck = Game.GameTime + 10000;
            }
        }

        private void Strike()
        {
            if (!_instant)
            {
                Sounds.Thunder?.Play();

                Props.Lightnings.IsSequenceLooped = Properties.AreTimeCircuitsOn;
                Props.Lightnings.Play();
            }

            Props.LightningsOnCar.IsSequenceLooped = Properties.AreTimeCircuitsOn;
            Props.LightningsOnCar.Play();

            if (Properties.AreTimeCircuitsOn)
            {
                IsPlaying = true;

                DMC12?.SetVoltValue?.Invoke(100);

                Properties.HasBeenStruckByLightning = true;

                Particles.LightningSparks?.Play();

                Sounds.LightningStrike?.Play();

                Events.SetSIDLedsState?.Invoke(true, true);

                Properties.PhotoFluxCapacitorActive = true;
                Properties.PhotoGlowingCoilsActive = true;

                if (Mods.Hook == HookState.On && !Properties.IsFlying)
                    Events.OnSparksEnded?.Invoke(_instant ? 250 : 500);
                else
                    Events.OnSparksEnded?.Invoke(_instant ? 250 : 2000);

                TimeMachineClone timeMachineClone = TimeMachine.Clone();
                timeMachineClone.Properties.DestinationTime = timeMachineClone.Properties.DestinationTime.AddYears(70);
                timeMachineClone.Properties.PreviousTime = FusionUtils.CurrentTime;
                RemoteTimeMachineHandler.AddRemote(timeMachineClone);

                Events.OnLightningStrike?.Invoke();
            }
            else
            {
                Function.Call(Hash.FORCE_LIGHTNING_FLASH);

                if (Properties.IsFlying)
                    Properties.AreFlyingCircuitsBroken = true;
            }

            _nextCheck = Game.GameTime + 60000;
            _instant = false;
            _delay = -1;
        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        public override void Stop()
        {
            if (!IsPlaying)
                return;

            Sounds.LightningStrike?.Stop();
            Sounds.Thunder?.Stop();

            Props.Lightnings?.Delete();
            Props.LightningsOnCar?.Delete();

            if (Mods.IsDMC12)
                Particles.LightningSparks?.Stop();

            IsPlaying = false;
        }

        public override void Dispose()
        {

        }
    }
}