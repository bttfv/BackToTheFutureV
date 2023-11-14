using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Chrono;
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
                Properties.StruckByLightningInstant = true;

                _instant = true;
                Strike();
                return;
            }

            Properties.StruckByLightning = true;

            _delay = Game.GameTime + (delay * 1000);
        }

        public override void Tick()
        {
            if (_delay > -1 && Game.GameTime > _delay)
            {
                Strike();
            }

            if (!ModSettings.LightningStrikeEvent || World.Weather != Weather.ThunderStorm || Game.GameTime < _nextCheck || Constants.ReadyForLightningRun)
            {
                return;
            }

            if (Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole)
            {
                _nextCheck = Game.GameTime + 10000;
                return;
            }

            if ((Mods.IsDMC12 && Mods.Hook == HookState.On && Vehicle.GetMPHSpeed() >= Constants.TimeTravelAtSpeed && !Properties.IsFlying && Vehicle.IsOutInTheOpen()) /*|| (Constants.DeluxoProto && Vehicle.IsExtraOn(1) && Vehicle.Mods.DashboardColor == (VehicleColor)70 && !Properties.IsFlying)*/ || (Vehicle.HeightAboveGround >= 20 && Properties.IsFlying))
            {
                if (FusionUtils.Random.NextDouble() < 0.3)
                {
                    Strike();
                }
                else
                {
                    _nextCheck = Game.GameTime + 10000;
                }
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
                WaypointScript.LoadWaypointPosition(true);

                if (Mods.Hook == HookState.On && !Properties.IsFlying /*|| (Constants.DeluxoProto && Vehicle.IsExtraOn(1) && !Properties.IsFlying)*/)
                {
                    Events.OnSparksEnded?.Invoke(_instant ? 250 : 500);
                }
                else
                {
                    Events.OnSparksEnded?.Invoke(_instant ? 250 : 2000);

                    if (!Properties.IsWayback)
                    {
                        TimeMachineClone timeMachineClone = TimeMachine.Clone();

                        int _cloneYears = GameClock.Now.Year - timeMachineClone.Properties.DestinationTime.Year;
                        GameClockDateTime _tempDest = timeMachineClone.Properties.DestinationTime;

                        if (_cloneYears == 0)
                        {
                            if (_tempDest.Year + 70 < 9999)
                            {
                                timeMachineClone.Properties.DestinationTime = new GameClockDateTime(GameClockDate.FromYmd(_tempDest.Year + 70, _tempDest.Month, _tempDest.Day), _tempDest.Time);
                            }
                            else
                            {
                                timeMachineClone.Properties.DestinationTime = new GameClockDateTime(GameClockDate.FromYmd(_tempDest.Year - 70, _tempDest.Month, _tempDest.Day), _tempDest.Time);
                            }
                        }
                        else
                        {
                            if (_tempDest.Year + (_cloneYears * 2) < 9999)
                            {
                                timeMachineClone.Properties.DestinationTime = new GameClockDateTime(GameClockDate.FromYmd(_tempDest.Year + (_cloneYears * 2), _tempDest.Month, _tempDest.Day), _tempDest.Time);
                            }
                            else
                            {
                                timeMachineClone.Properties.DestinationTime = new GameClockDateTime(GameClockDate.FromYmd(_tempDest.Year + (_cloneYears / 2), _tempDest.Month, _tempDest.Day), _tempDest.Time);
                            }
                        }

                        timeMachineClone.Vehicle.Position = timeMachineClone.Vehicle.Position.InvertCoordinate(FusionEnums.Coordinate.X).InvertCoordinate(FusionEnums.Coordinate.Y).SetToGroundHeight();
                        timeMachineClone.Properties.PreviousTime = GameClock.Now;
                        RemoteTimeMachineHandler.AddRemote(timeMachineClone);
                    }
                }

                Events.OnLightningStrike?.Invoke();
            }
            else
            {
                World.ForceLightningFlash();

                if (Properties.IsFlying)
                {
                    Properties.AreFlyingCircuitsBroken = true;
                }
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
            {
                return;
            }

            Properties.StruckByLightning = false;
            Properties.StruckByLightningInstant = false;

            Sounds.LightningStrike?.Stop();
            Sounds.Thunder?.Stop();

            Props.Lightnings?.Delete();
            Props.LightningsOnCar?.Delete();

            if (Mods.IsDMC12)
            {
                Particles.LightningSparks?.Stop();
            }

            IsPlaying = false;
        }

        public override void Dispose()
        {

        }
    }
}
