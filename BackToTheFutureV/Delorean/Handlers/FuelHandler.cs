using BackToTheFutureV.Entities;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using BackToTheFutureV.Settings;
using KlangRageAudioLibrary;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class FuelHandler : Handler
    {
        public bool IsRefueling { get; private set; }

        private AudioPlayer _emptySound;
        private AudioPlayer _refuelSound;

        private Players.Player _refuelPlayer;

        private Ped _refuelingPed;

        private int _refuelTimer;
        private int _currentRefuelStep;
        private int _nextBlink;
        private int _nextId;
        private int _blinks;
        private const int TotalBlinks = 16;

        private bool _isBlinking;

        private float _reactorGlowingTime = 0;

        private AnimateProp _emptyGlowing;
        private  AnimateProp _emptyOff;

        public FuelHandler(TimeCircuits circuits) : base(circuits)
        {
            LoadRes();

            SetEmpty(false);
        }

        public void LoadRes()
        {
            _refuelPlayer?.Dispose();
            _emptySound?.Dispose();
            _refuelSound?.Dispose();

            if (Mods.Reactor == ReactorType.Nuclear)
            {
                _refuelPlayer = new PlutoniumRefillPlayer(TimeCircuits);
                _emptySound = TimeCircuits.AudioEngine.Create("bttf1/timeCircuits/plutoniumEmpty.wav", Presets.Interior);
                _refuelSound = TimeCircuits.AudioEngine.Create("bttf1/refuel.wav", Presets.Exterior);

                _emptySound.SourceBone = "bttf_tcd_green";
            }
            else
                _refuelPlayer = new MrFusionRefillPlayer(TimeCircuits);

            _emptyGlowing = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.Empty), Vector3.Zero, Vector3.Zero);
            _emptyOff = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.EmptyOff), Vector3.Zero, Vector3.Zero);
        }

        public void BlinkFuel()
        {
            if (IsFueled) 
                return;

            _isBlinking = true;
        }

        public void Refuel(Ped refuelPed)
        {
            IsRefueling = true;
            _refuelTimer = 0;
            _refuelingPed = refuelPed;
        }

        public override void Process()
        {
            _refuelPlayer.Process();

            // Pulsing animation while refueling for plutonium (bttf1) delorean
            if (Mods.Reactor == ReactorType.Nuclear && ModSettings.GlowingPlutoniumReactor)
            {
                if (!IsFueled && !IsRefueling)
                {
                    if (Mods.GlowingReactor == ModState.On)
                        Mods.GlowingReactor = ModState.Off;
                }
                else
                {
                    if (IsRefueling)
                    {
                        _reactorGlowingTime += Game.LastFrameTime;

                        if (_reactorGlowingTime > 0.25f)
                        {
                            Mods.GlowingReactor = Mods.GlowingReactor == ModState.On ? ModState.Off : ModState.On;

                            _reactorGlowingTime = 0;
                        }
                    }
                    else if (Mods.GlowingReactor != ModState.On)
                    {
                        Mods.GlowingReactor = ModState.On;

                        _reactorGlowingTime = 0;
                    }
                }
            }
            else if (Mods.GlowingReactor !=  ModState.Off)
                Mods.GlowingReactor = ModState.Off;

            // Empty animation

            // For BTTF1
            if (_isBlinking && !IsFueled && Mods.Reactor == ReactorType.Nuclear)
            {
                // Blink anim end
                if (_blinks > TotalBlinks && Game.GameTime > _nextBlink)
                {
                    _isBlinking = false;
                    _blinks = 0;

                    SetEmpty(true);
                }
                // Keep beeping
                else if (_blinks <= TotalBlinks && Game.GameTime > _nextBlink)
                {
                    // ID is kinda off/on
                    if (_nextId == 0)
                    {
                        SetEmpty(true);

                        _nextId = 1;
                        _nextBlink = Game.GameTime + 640;

                        _emptySound.Play();

                    }
                    else
                    {
                        SetEmpty(false);

                        _nextId = 0;
                        _nextBlink = Game.GameTime + 200;
                    }
                    _blinks++;
                }
            }
            // For BTTF2/3
            // Fueled -> EMPTY indicator isn't glowing, EMPTY in GUI is hidden
            // Not Fueled -> Glowing EMPTY indicator inside car and in GUI
            else
            {
                if (IsFueled)
                {
                    SetEmpty(false);
                    HideEmpty();
                }
                else
                    SetEmpty(true);
            }

            if (IsRefueling)
            {
                if(_refuelingPed == Main.PlayerPed && Main.PlayerVehicle == null)
                {
                    Game.DisableAllControlsThisFrame();
                    Game.EnableControlThisFrame(GTA.Control.LookUpDown);
                    Game.EnableControlThisFrame(GTA.Control.LookLeftRight);
                    Game.EnableControlThisFrame(GTA.Control.NextCamera);
                    Game.EnableControlThisFrame(GTA.Control.LookBehind);
                }

                if (Game.GameTime > _refuelTimer)
                {
                    switch (_currentRefuelStep)
                    {
                        case 0:
                            _refuelPlayer.Play();
                            if (Mods.Reactor == ReactorType.Nuclear)
                                _refuelSound.Play();

                            _refuelTimer = Game.GameTime + 2500;
                            _currentRefuelStep++;
                            break;

                        case 1:
                            _refuelPlayer.Play();

                            _refuelTimer = Game.GameTime + 1300;
                            _currentRefuelStep++;
                            break;

                        case 2:
                            Stop();

                            break;
                    }
                }
            }

            if (!CanRefuel(Main.PlayerPed) || IsRefueling || IsFueled)
                return;

            if (!TcdEditer.IsEditing)
                Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Refuel_Hotkey"));

            if (Game.IsControlJustPressed(GTA.Control.Context))
                Refuel(Main.PlayerPed);
        }

        public bool CanRefuel(Ped ped)
        {
            if(ped.CurrentVehicle == null)
            {
                var bootPos = Vehicle.Bones["mr_fusion"].Position;

                Vector3 dir;
                float angle, dist;

                if(ped == Main.PlayerPed)
                {
                    dir = bootPos - GameplayCamera.Position;
                    angle = Vector3.Angle(dir, GameplayCamera.Direction);
                    dist = Vector3.Distance(bootPos, Main.PlayerPed.Position);
                }
                else
                {
                    dir = bootPos - ped.Position;
                    angle = Vector3.Angle(dir, ped.ForwardVector);
                    dist = Vector3.Distance(bootPos, ped.Position);
                }

                if (angle < 45 && dist < 1.5f)
                {
                    return true;
                }
            }
            return false;
        }

        public override void KeyPress(Keys key) { }

        public override void Stop()
        {
            IsFueled = true;
            IsRefueling = false;
            _currentRefuelStep = 0;
            _refuelTimer = 0;

            SetEmpty(false);
        }

        public override void Dispose()
        {
            _emptySound?.Dispose();
            _refuelSound?.Dispose();
            _emptyGlowing?.DeleteProp();
            _emptyOff?.DeleteProp();
            _refuelPlayer?.Dispose();
        }

        private void SetEmpty(bool isOn)
        {
            if (Main.PlayerVehicle == Vehicle)
                GUI.CallFunction("SET_EMPTY_STATE", !isOn);

            if (Vehicle.IsVisible == false)
                return;

            if (isOn)
            {
                _emptyOff?.DeleteProp();
                _emptyGlowing.SpawnProp();
            }
            else
            {
                _emptyOff.SpawnProp();
                _emptyGlowing?.DeleteProp();
            }
        }
        private void HideEmpty()
        {
            if (Main.PlayerVehicle != Vehicle)
                return;

            GUI.CallFunction("HIDE_EMPTY");
        }
    }
}