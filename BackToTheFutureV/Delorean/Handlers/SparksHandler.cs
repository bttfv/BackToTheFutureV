using GTA;
using GTA.Native;
using BackToTheFutureV.Players;
using System.Windows.Forms;
using KlangRageAudioLibrary;
using AudioFlags = KlangRageAudioLibrary.AudioFlags;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class SparksHandler : Handler
    {
        private WormholeAnimationPlayer _wormholeAnims;

        private AudioPlayer _sparksAudio;
        private readonly AudioPlayer _sparksEmptyAudio;
        private readonly AudioPlayer _diodesGlowingSound;
        private readonly AudioPlayer _wormholeInterrupted;

        private int _startStabilizedSoundAt;
        private readonly AudioPlayer _sparkStabilized;

        private int _startEffectsAt;
        private int _playDiodeSoundAt;

        private int _timeTravelAtTime;
        private int _wormholeLengthTime;

        private float _cooldownTime = -1;

        private bool _hasHit88;

        private bool _hasPlayedDiodeSound;
        public SparksHandler(TimeCircuits circuits) : base(circuits)
        {
            _sparksEmptyAudio = 
                circuits.AudioEngine.Create("general/timeTravel/sparksNoFuel.wav", Presets.ExteriorLoud);
            _diodesGlowingSound = circuits.AudioEngine.Create("general/timeCircuits/SID.wav", Presets.Interior);
            _wormholeInterrupted = circuits.AudioEngine.Create("general/timeTravel/sparkInterrupt.wav", Presets.ExteriorLoud);
            _sparkStabilized = circuits.AudioEngine.Create("bttf3/timeTravel/sparksStabilization.wav", Presets.ExteriorLoud);

            LoadRes();
            
            TimeCircuits.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
        }

        public void LoadRes()
        {
            _sparksAudio?.Dispose();
            _wormholeAnims?.Dispose();

            _sparksAudio = TimeCircuits.AudioEngine.Create($"{LowerCaseDeloreanType}/timeTravel/sparks.wav", Presets.ExteriorLoudLoop);
            _sparksAudio.FadeOutMultiplier = 2f;
            _sparksAudio.StartFadeIn = false;

            switch (DeloreanType)
            {
                case DeloreanType.BTTF1:
                    _wormholeAnims = new WormholeAnimationPlayer(TimeCircuits, 5000);
                    _wormholeLengthTime = 5000;
                    _startEffectsAt = 88;
                    _playDiodeSoundAt = 82;
                    break;

                case DeloreanType.BTTF2:
                    _wormholeAnims = new WormholeAnimationPlayer(TimeCircuits, 2900);
                    _wormholeLengthTime = 2900;
                    _startEffectsAt = 88;
                    _playDiodeSoundAt = 82;
                    break;

                case DeloreanType.BTTF3:
                    if (Mods.Wheel == WheelType.RailroadInvisible)
                    {
                        _wormholeAnims = new WormholeAnimationPlayer(TimeCircuits, 4200);
                        _wormholeLengthTime = 4200;
                        _startEffectsAt = 86;
                        _playDiodeSoundAt = 82;
                    } else
                    {                        
                        _wormholeAnims = new WormholeAnimationPlayer(TimeCircuits, 4200);
                        _wormholeLengthTime = 4200;
                        _startEffectsAt = 65;
                        _playDiodeSoundAt = 60;
                    }
                    break;
            }
        }

        private void OnTimeCircuitsToggle()
        {
            if (!IsOn && _wormholeAnims != null &&_wormholeAnims.IsPlaying)
                Stop();
        }

        public override void Process()
        {
            if (!IsOn) return;
            if (!Vehicle.IsVisible)
            {
                if (_wormholeAnims.IsPlaying)
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

            _wormholeAnims?.Process();

            if (MPHSpeed >= _playDiodeSoundAt)
            {
                if (!_hasPlayedDiodeSound && IsFueled)
                {
                    _diodesGlowingSound.Play();
                    _hasPlayedDiodeSound = true;
                }
            }
            else
            {
                if (_wormholeAnims != null && _wormholeAnims.IsPlaying)
                {
                    _wormholeInterrupted.Play();
                    Stop();
                }

                return;
            }

            if (MPHSpeed >= _startEffectsAt)
            {
                if (_wormholeAnims == null)
                    return;

                if (IsFueled)
                {
                    if (!_wormholeAnims.IsPlaying)
                    {
                        _wormholeAnims.Play(true);

                        if (ModSettings.GlowingWormholeEmitter)
                            Mods.GlowingEmitter = ModState.On;
                    }

                    if(!_sparksAudio.IsAnyInstancePlaying && !_sparkStabilized.IsAnyInstancePlaying)
                    {
                        if (Mods.HoverUnderbody == ModState.On)
                            TimeCircuits.GetHandler<FlyingHandler>().CanConvert = false;

                        _sparksAudio.Play();
                    }

                    if (MPHSpeed >= 88 && !_hasHit88)
                    {
                        _hasHit88 = true;
                        _timeTravelAtTime = Game.GameTime + _wormholeLengthTime + 1000;

                        if (DeloreanType == DeloreanType.BTTF3)
                        {
                            _startStabilizedSoundAt = Game.GameTime + 1000;
                        }
                    }

                    if (DeloreanType == DeloreanType.BTTF3 && Game.GameTime > _startStabilizedSoundAt && _hasHit88)
                    {
                        _sparksAudio.Stop();

                        if(!_sparkStabilized.IsAnyInstancePlaying)
                            _sparkStabilized.Play();
                    }

                    if (Game.GameTime > _timeTravelAtTime && _hasHit88)
                        SparksEnded();
                }
                else if(!IsFueled)
                {
                    if (!_wormholeAnims.IsPlaying)
                    {
                        _wormholeAnims.Play(false);
                    }

                    if(!_sparksEmptyAudio.IsAnyInstancePlaying)
                        _sparksEmptyAudio.Play();
                }
            }
        }

        public void StartTimeTravelCooldown()
        {
            _cooldownTime = 0;
        }

        public override void Stop()
        {
            _sparksAudio?.Stop(true);

            _sparkStabilized?.Stop(true);
            _sparksEmptyAudio?.Stop(true);
            _diodesGlowingSound?.Stop(true);
            _startStabilizedSoundAt = 0;

            Mods.GlowingEmitter = ModState.Off;

            if (_wormholeAnims.IsPlaying)
                _wormholeAnims?.Stop();

            if (Mods.HoverUnderbody == ModState.On)
                TimeCircuits.GetHandler<FlyingHandler>().CanConvert = true;

            _hasPlayedDiodeSound = false;
            _hasHit88 = false;
        }

        public override void Dispose()
        {
            _sparksAudio?.Dispose();
            _sparkStabilized?.Dispose();
            _wormholeAnims?.Dispose();
            _diodesGlowingSound?.Dispose();
        }

        public override void KeyPress(Keys key)
        {

        }

        private void SparksEnded()
        {
            Stop();
          
            if (IsOnTracks)
                TimeCircuits.GetHandler<RailroadHandler>().StopTrain();

            Function.Call(Hash.DETACH_VEHICLE_FROM_ANY_TOW_TRUCK, Vehicle.Handle);

            TimeCircuits?.GetHandler<TimeTravelHandler>()?.StartTimeTravelling();
        }
    }
}