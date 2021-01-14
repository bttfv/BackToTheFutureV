using BackToTheFutureV.Players;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Native;
using System.Windows.Forms;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class SparksHandler : Handler
    {        
        private int _startStabilizedSoundAt;
        
        private int _startEffectsAt;
        private int _playDiodeSoundAt;

        private int _timeTravelAtTime;
        private int _wormholeLengthTime;

        private float _cooldownTime = -1;

        private bool _hasHit88;

        private bool _hasPlayedDiodeSound;
        public SparksHandler(TimeMachine timeMachine) : base(timeMachine)
        {            
            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
            Events.OnWormholeTypeChanged += OnWormholeTypeChanged;
            Events.OnReenterCompleted += StartTimeTravelCooldown;
            TimeHandler.OnDayNightChange += OnWormholeTypeChanged;

            OnWormholeTypeChanged();
        }

        public void OnWormholeTypeChanged()
        {
            Players.Wormhole?.Dispose();

            switch (Mods.WormholeType)
            {
                case WormholeType.BTTF1:
                    Players.Wormhole = new WormholeAnimationPlayer(TimeMachine, 5000);
                    _wormholeLengthTime = 5000;
                    _startEffectsAt = 88;
                    _playDiodeSoundAt = 82;
                    break;

                case WormholeType.BTTF2:
                    Players.Wormhole = new WormholeAnimationPlayer(TimeMachine, 2900);
                    _wormholeLengthTime = 2900;
                    _startEffectsAt = 88;
                    _playDiodeSoundAt = 82;
                    break;

                case WormholeType.BTTF3:
                    if (Mods.Wheel == WheelType.RailroadInvisible)
                    {
                        Players.Wormhole = new WormholeAnimationPlayer(TimeMachine, 4200);
                        _wormholeLengthTime = 4200;
                        _startEffectsAt = 82;
                        _playDiodeSoundAt = 80;
                    } else
                    {
                        Players.Wormhole = new WormholeAnimationPlayer(TimeMachine, 4200);
                        _wormholeLengthTime = 4200;
                        _startEffectsAt = 65;
                        _playDiodeSoundAt = 60;
                    }
                    break;
            }
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

            if (_cooldownTime > -1)
            {
                _cooldownTime += Game.LastFrameTime;

                if (_cooldownTime > 30)
                    _cooldownTime = -1;
                else
                    return;
            }

            Players.Wormhole?.Process();

            if (Properties.IsPhotoModeOn)
                return;

            if (Vehicle.GetMPHSpeed() >= _playDiodeSoundAt)
            {
                if (!_hasPlayedDiodeSound && Properties.IsFueled)
                {
                    Sounds.DiodesGlowing?.Play();
                    Events.OnSIDReachMax?.Invoke(_playDiodeSoundAt);
                    _hasPlayedDiodeSound = true;
                }
            }
            else
            {
                if (_hasPlayedDiodeSound)
                    _hasPlayedDiodeSound = false;

                if (Players.Wormhole != null && Players.Wormhole.IsPlaying)
                {
                    Sounds.WormholeInterrupted?.Play();
                    Events.OnTimeTravelInterrupted?.Invoke();

                    Properties.TimeTravelPhase = TimeTravelPhase.Completed;

                    Stop();
                }

                return;
            }

            if (Vehicle.GetMPHSpeed() >= _startEffectsAt && !Properties.BlockSparks)
            {
                if (Players.Wormhole == null)
                    return;

                PlayerSwitch.Disable = true;

                if (Properties.TimeTravelPhase != TimeTravelPhase.OpeningWormhole && !Properties.HasBeenStruckByLightning)
                    Properties.TimeTravelPhase = TimeTravelPhase.OpeningWormhole;

                //Function.Call(Hash.SPECIAL_ABILITY_LOCK, CommonSettings.PlayerPed.Model);
                Function.Call(Hash.SPECIAL_ABILITY_DEACTIVATE_FAST, Game.Player);
                Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, false);

                if (Properties.IsFueled)
                {
                    if (!Players.Wormhole.IsPlaying)
                    {
                        Players.Wormhole?.Play(true);

                        if (ModSettings.GlowingWormholeEmitter)
                            Mods.GlowingEmitter = ModState.On;
                    }

                    if(!Sounds.Sparks.IsAnyInstancePlaying && !Sounds.SparkStabilized.IsAnyInstancePlaying)
                    {
                        if (Mods.HoverUnderbody == ModState.On)
                            Properties.CanConvert = false;

                        Sounds.Sparks?.Play();
                    }

                    if (Vehicle.GetMPHSpeed() >= 88 && !_hasHit88)
                    {
                        _hasHit88 = true;
                        _timeTravelAtTime = Game.GameTime + _wormholeLengthTime + 1000;

                        if (Mods.WormholeType == WormholeType.BTTF3)
                            _startStabilizedSoundAt = Game.GameTime + 1000;
                        
                        Secondary.LoadWaypointPosition(true);
                    }

                    if (Mods.WormholeType == WormholeType.BTTF3 && Game.GameTime > _startStabilizedSoundAt && _hasHit88)
                    {
                        Sounds.Sparks?.Stop();

                        if(!Sounds.SparkStabilized.IsAnyInstancePlaying)
                            Sounds.SparkStabilized?.Play();
                    }

                    if (Game.GameTime > _timeTravelAtTime && _hasHit88)
                        SparksEnded();
                }
                else if(!Properties.IsFueled)
                {
                    if (!Players.Wormhole.IsPlaying)
                        Players.Wormhole.Play(false);

                    if (!Sounds.SparksEmpty.IsAnyInstancePlaying)
                        Sounds.SparksEmpty?.Play();
                }
            }
        }

        public void StartTimeTravelCooldown()
        {
            _cooldownTime = 0;
        }

        public override void Stop()
        {
            Sounds.Sparks?.Stop(true);

            Sounds.SparkStabilized?.Stop(true);
            Sounds.SparksEmpty?.Stop(true);
            Sounds.DiodesGlowing?.Stop(true);
            _startStabilizedSoundAt = 0;

            Mods.GlowingEmitter = ModState.Off;

            if (Players.Wormhole.IsPlaying)
                Players.Wormhole?.Stop();

            if (Mods.HoverUnderbody == ModState.On)
                Properties.CanConvert = true;

            _hasPlayedDiodeSound = false;
            _hasHit88 = false;

            Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, true);
            PlayerSwitch.Disable = false;
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

            PlayerSwitch.Disable = true;

            Function.Call(Hash.ENABLE_SPECIAL_ABILITY, Game.Player, false);

            Function.Call(Hash.DETACH_VEHICLE_FROM_ANY_TOW_TRUCK, Vehicle.Handle);

            Events.StartTimeTravel?.Invoke();
        }
    }
}