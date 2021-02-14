using BackToTheFutureV.Players;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using GTA;
using GTA.Native;
using System.Windows.Forms;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class SparksHandler : Handler
    {
        public SparksHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
            Events.OnWormholeTypeChanged += OnWormholeTypeChanged;
            TimeHandler.OnDayNightChange += OnWormholeTypeChanged;

            Events.On88MphSpeed += On88MphSpeed;
            Events.OnStartTimeTravelSequenceAtSpeed += OnStartTimeTravelSequenceAtSpeed;
            Events.OnPlayDiodeSoundAtSpeed += OnPlayDiodeSoundAtSpeed;

            OnWormholeTypeChanged();
        }

        public void On88MphSpeed(bool over)
        {
            if (!Properties.AreTimeCircuitsOn && !Properties.IsPhotoModeOn)
                return;

            if (over)
                WaypointScript.LoadWaypointPosition(true);
        }

        public void OnStartTimeTravelSequenceAtSpeed(bool over)
        {
            if (!Properties.AreTimeCircuitsOn && !Properties.IsPhotoModeOn)
                return;

            if (over)
            {
                PlayerSwitch.Disable = true;

                Properties.TimeTravelPhase = TimeTravelPhase.OpeningWormhole;

                //Function.Call(Hash.SPECIAL_ABILITY_LOCK, CommonSettings.PlayerPed.Model);
                Function.Call(Hash.SPECIAL_ABILITY_DEACTIVATE_FAST, Game.Player);
                Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, false);
            }
        }

        public void OnPlayDiodeSoundAtSpeed(bool over)
        {
            if (!Properties.AreTimeCircuitsOn && !Properties.IsPhotoModeOn)
                return;

            if (over)
                Sounds.DiodesGlowing?.Play();
            else
            {
                Sounds.DiodesGlowing?.Stop();

                if (Properties.TimeTravelPhase == TimeTravelPhase.InTime)
                    return;

                if (Players.Wormhole.IsPlaying)
                {
                    Sounds.WormholeInterrupted?.Play();
                    Events.OnTimeTravelInterrupted?.Invoke();
                }

                Stop();
            }
        }

        public void OnWormholeTypeChanged()
        {
            Players.Wormhole?.Dispose();
            Players.Wormhole = new WormholeAnimationPlayer(TimeMachine);
        }

        private void OnTimeCircuitsToggle()
        {
            if (!Properties.AreTimeCircuitsOn && Players.Wormhole != null && Players.Wormhole.IsPlaying)
                Stop();
        }

        public override void Process()
        {
            if (!Properties.AreTimeCircuitsOn && !Properties.IsPhotoModeOn)
                return;

            if (!Vehicle.IsVisible)
            {
                if (Players.Wormhole.IsPlaying)
                    Stop();

                return;
            }

            Players.Wormhole?.Process();

            if (Constants.OverStartTimeTravelSequenceAtSpeed)
            {
                if (!Utils.IsPadShaking)
                    Utils.SetPadShake(Constants.WormholeLengthTime, 100);

                if (Properties.IsFueled)
                {
                    if (!Players.Wormhole.IsPlaying)
                    {
                        Players.Wormhole?.Play(true);

                        if (ModSettings.GlowingWormholeEmitter)
                            Mods.GlowingEmitter = ModState.On;
                    }

                    if (!Sounds.Sparks.IsAnyInstancePlaying && !Sounds.SparkStabilized.IsAnyInstancePlaying)
                    {
                        if (Mods.HoverUnderbody == ModState.On)
                            Properties.CanConvert = false;

                        Sounds.Sparks?.Play();
                    }

                    if (Mods.WormholeType == WormholeType.BTTF3 && Game.GameTime >= Constants.StabilizationSoundAtTime && Constants.Over88MphSpeed)
                    {
                        Sounds.Sparks?.Stop();

                        if (!Sounds.SparkStabilized.IsAnyInstancePlaying)
                            Sounds.SparkStabilized?.Play();
                    }

                    if (Game.GameTime >= Constants.TimeTravelAtTime && Constants.Over88MphSpeed)
                        SparksEnded();
                }
                else
                {
                    if (!Players.Wormhole.IsPlaying)
                        Players.Wormhole.Play(false);

                    if (!Sounds.SparksEmpty.IsAnyInstancePlaying)
                        Sounds.SparksEmpty?.Play();
                }
            }
        }

        public override void Stop()
        {
            Utils.StopPadShake();

            Sounds.Sparks?.Stop(true);

            Sounds.SparkStabilized?.Stop(true);
            Sounds.SparksEmpty?.Stop(true);
            Sounds.DiodesGlowing?.Stop(true);

            Mods.GlowingEmitter = ModState.Off;

            if (Players.Wormhole.IsPlaying)
                Players.Wormhole?.Stop();

            if (Mods.HoverUnderbody == ModState.On)
                Properties.CanConvert = true;

            Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, true);
            PlayerSwitch.Disable = false;

            Properties.TimeTravelPhase = TimeTravelPhase.Completed;
        }

        public override void Dispose()
        {

        }

        public override void KeyDown(Keys key)
        {

        }

        private void SparksEnded()
        {
            Stop();

            Properties.TimeTravelPhase = TimeTravelPhase.InTime;

            PlayerSwitch.Disable = true;

            Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, false);

            Function.Call(Hash.DETACH_VEHICLE_FROM_ANY_TOW_TRUCK, Vehicle.Handle);

            Events.StartTimeTravel?.Invoke();
        }
    }
}