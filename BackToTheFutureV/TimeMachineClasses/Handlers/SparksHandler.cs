using FusionLibrary;
using GTA;
using GTA.Native;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class SparksHandler : HandlerPrimitive
    {
        public SparksHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;

            Events.OnSIDMaxSpeedReached += OnSIDMaxSpeedReached;
            Events.OnTimeTravelSpeedReached += OnTimeTravelSpeedReached;
            Events.OnSparksEnded += OnSparksEnded;
        }

        public void OnTimeTravelSpeedReached(bool over)
        {
            if (!Properties.AreTimeCircuitsOn)
            {
                return;
            }

            if (over)
            {
                PlayerSwitch.Disable = true;

                Properties.TimeTravelPhase = TimeTravelPhase.OpeningWormhole;

                Function.Call(Hash.SPECIAL_ABILITY_DEACTIVATE_FAST, Game.Player);
                Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, false);

                if (Properties.IsFueled)
                {
                    DMC12?.SetVoltValue?.Invoke(100);
                    WaypointScript.LoadWaypointPosition(true);
                }
            }
            else
            {
                if (Players.Wormhole.IsPlaying)
                {
                    DMC12?.SetVoltValue?.Invoke(50);
                    Sounds.WormholeInterrupted?.Play();
                }

                Stop();
            }
        }

        public void OnSIDMaxSpeedReached(bool over)
        {
            if (!Properties.AreTimeCircuitsOn || !over)
            {
                return;
            }

            Sounds.DiodesGlowing?.Play();
        }

        private void OnTimeCircuitsToggle()
        {
            if (!Properties.AreTimeCircuitsOn && Players.Wormhole.IsPlaying)
            {
                Stop();
            }
        }

        public override void Tick()
        {
            if (!Properties.AreTimeCircuitsOn && !Properties.IsPhotoModeOn)
            {
                if (Sounds.SparkStabilized.IsAnyInstancePlaying)
                {
                    Sounds.SparkStabilized?.Stop(true);
                }

                if (Sounds.SparksEmpty.IsAnyInstancePlaying)
                {
                    Sounds.SparksEmpty?.Stop(true);
                }

                if (Sounds.Sparks.IsAnyInstancePlaying)
                {
                    Sounds.Sparks?.Stop(true);
                }

                return;
            }

            Players.Wormhole?.Tick();

            if (Constants.OverWormholeAtSpeed)
            {
                if (!FusionUtils.IsPadShaking && TimeMachineHandler.CurrentTimeMachine == TimeMachine)
                {
                    FusionUtils.SetPadShake(Constants.WormholeLengthTime, 100);
                }

                if (Properties.IsFueled)
                {
                    if (!Players.Wormhole.IsPlaying)
                    {
                        Players.Wormhole?.Play(true);

                        if (ModSettings.GlowingWormholeEmitter)
                        {
                            Mods.GlowingEmitter = ModState.On;
                        }
                    }

                    if (!Sounds.Sparks.IsAnyInstancePlaying && !Sounds.SparkStabilized.IsAnyInstancePlaying)
                    {
                        if (Mods.HoverUnderbody == ModState.On)
                        {
                            Properties.CanConvert = false;
                        }

                        Sounds.Sparks?.Play();
                    }

                    if (Mods.WormholeType == WormholeType.BTTF3 && Game.GameTime >= Constants.StabilizationSoundAtTime && Constants.OverTimeTravelAtSpeed)
                    {
                        Sounds.Sparks?.Stop();

                        if (!Sounds.SparkStabilized.IsAnyInstancePlaying)
                        {
                            Sounds.SparkStabilized?.Play();
                        }
                    }

                    if (Game.GameTime >= Constants.TimeTravelAtTime && Constants.OverTimeTravelAtSpeed)
                    {
                        Events.OnSparksEnded?.Invoke();
                    }
                }
                else
                {
                    if (!Players.Wormhole.IsPlaying)
                    {
                        Players.Wormhole.Play(false);
                    }

                    if (!Sounds.SparksEmpty.IsAnyInstancePlaying)
                    {
                        Sounds.SparksEmpty?.Play();
                    }
                }
            }
            else
            {
                if (Sounds.SparkStabilized.IsAnyInstancePlaying)
                {
                    Sounds.SparkStabilized?.Stop(true);
                }

                if (Sounds.SparksEmpty.IsAnyInstancePlaying)
                {
                    Sounds.SparksEmpty?.Stop(true);
                }

                if (Sounds.Sparks.IsAnyInstancePlaying)
                {
                    Sounds.Sparks?.Stop(true);
                }
            }
        }

        public override void Stop()
        {
            if (TimeMachineHandler.CurrentTimeMachine == TimeMachine)
            {
                FusionUtils.StopPadShake();
            }

            Players.Wormhole?.Stop();
            Events.OnSparksInterrupted?.Invoke();

            Sounds.Sparks?.Stop(true);

            Sounds.SparkStabilized?.Stop(true);
            Sounds.SparksEmpty?.Stop(true);
            Sounds.DiodesGlowing?.Stop(true);

            Mods.GlowingEmitter = ModState.Off;

            if (Players.Wormhole.IsPlaying)
            {
                Players.Wormhole?.Stop();
            }

            if (Mods.HoverUnderbody == ModState.On)
            {
                Properties.CanConvert = true;
            }

            Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, true);
            PlayerSwitch.Disable = false;

            Properties.TimeTravelPhase = TimeTravelPhase.Completed;
        }

        public override void Dispose()
        {

        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        private void OnSparksEnded(int delay = 0)
        {
            Stop();

            Properties.TimeTravelPhase = TimeTravelPhase.InTime;

            PlayerSwitch.Disable = true;

            Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, false);

            Function.Call(Hash.DETACH_VEHICLE_FROM_ANY_TOW_TRUCK, Vehicle.Handle);

            Events.StartTimeTravel?.Invoke(delay);
        }
    }
}
