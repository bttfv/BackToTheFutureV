﻿using BackToTheFutureV.Entities;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using BackToTheFutureV.Settings;
using KlangRageAudioLibrary;
using BackToTheFutureV.Vehicles;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class FuelHandler : Handler
    {
        private Ped _refuelingPed;

        private int _refuelTimer;
        private int _currentRefuelStep;
        private int _nextBlink;
        private int _nextId;
        private int _blinks;
        private const int TotalBlinks = 16;

        private bool _isBlinking;

        private float _reactorGlowingTime = 0;

        public FuelHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            SetEmpty(false);

            Events.StartFuelBlink += StartFuelBlink;

            if (Mods.IsDMC12)
            {
                OnReactorTypeChanged();
                Events.OnReactorTypeChanged += OnReactorTypeChanged;
            }            
        }

        public void OnReactorTypeChanged()
        {
            Players.Refuel?.Dispose();

            if (Mods.Reactor == ReactorType.Nuclear)
                Players.Refuel = new PlutoniumRefillPlayer(TimeMachine);
            else
                Players.Refuel = new MrFusionRefillPlayer(TimeMachine);
        }


        public void StartFuelBlink()
        {
            if (Properties.IsFueled) 
                return;

            _isBlinking = true;
        }

        public void Refuel(Ped refuelPed)
        {
            Properties.IsRefueling = true;
            _refuelTimer = 0;
            _refuelingPed = refuelPed;
        }

        public override void Process()
        {
            Players.Refuel?.Process();

            // Pulsing animation while refueling for plutonium (bttf1) delorean
            if (Mods.Reactor == ReactorType.Nuclear && ModSettings.GlowingPlutoniumReactor)
            {
                if (!Properties.IsFueled && !Properties.IsRefueling)
                {
                    if (Mods.GlowingReactor == ModState.On)
                        Mods.GlowingReactor = ModState.Off;
                }
                else
                {
                    if (Properties.IsRefueling)
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
            if (_isBlinking && !Properties.IsFueled && Mods.Reactor == ReactorType.Nuclear)
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

                        Sounds.FuelEmpty.Play();
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
                if (Properties.IsFueled)
                {
                    SetEmpty(false);
                    HideEmpty();
                }
                else
                    SetEmpty(true);
            }

            if (Properties.IsRefueling)
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
                            Players.Refuel?.Play();

                            if (Mods.Reactor == ReactorType.Nuclear)
                                Sounds.Refuel.Play();

                            _refuelTimer = Game.GameTime + 2500;
                            _currentRefuelStep++;
                            break;
                        case 1:
                            Players.Refuel?.Play();

                            _refuelTimer = Game.GameTime + 1300;
                            _currentRefuelStep++;
                            break;
                        case 2:
                            Stop();

                            break;
                    }
                }
            }

            if (!CanRefuel(Main.PlayerPed) || Properties.IsRefueling || Properties.IsFueled)
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
            Properties.IsFueled = true;
            Properties.IsRefueling = false;
            _currentRefuelStep = 0;
            _refuelTimer = 0;

            SetEmpty(false);
        }

        public override void Dispose()
        {
            
        }

        private void SetEmpty(bool isOn)
        {
            if (Main.PlayerVehicle == Vehicle)
                Scaleforms.GUI.CallFunction("SET_EMPTY_STATE", !isOn);

            if (Vehicle.IsVisible == false)
                return;

            if (isOn)
            {
                Props.EmptyOff?.DeleteProp();
                Props.EmptyGlowing?.SpawnProp();
            }
            else
            {
                Props.EmptyOff?.SpawnProp();
                Props.EmptyGlowing?.DeleteProp();
            }
        }
        private void HideEmpty()
        {
            if (Main.PlayerVehicle != Vehicle)
                return;

            Scaleforms.GUI.CallFunction("HIDE_EMPTY");
        }
    }
}