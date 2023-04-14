﻿using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class HoverMode : Script
    {
        private bool _firstTick = true;
        private NativeInput _flyModeInput;
        private int _nextModeChangeAllowed;
        //private bool _hoverTraffic;

        public HoverMode()
        {
            Aborted += HoverMode_Aborted;
            Tick += HoverMode_Tick;
            KeyDown += HoverMode_KeyDown;
        }

        private void HoverMode_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            /*if (e.KeyCode == System.Windows.Forms.Keys.Q && FusionUtils.PlayerVehicle.NotNullAndExists())
            {
                HoverVehicle hoverVehicle = HoverVehicle.GetFromVehicle(FusionUtils.PlayerVehicle);

                hoverVehicle.IsHoverModeAllowed = !hoverVehicle.IsHoverModeAllowed;
            }

            if (e.KeyCode == System.Windows.Forms.Keys.L)
            {
                _hoverTraffic = !_hoverTraffic;
            }*/

            if (e.KeyCode == ModControls.HoverAltitudeHold && FusionUtils.PlayerVehicle.NotNullAndExists())
            {
                HoverVehicle hoverVehicle = HoverVehicle.GetFromVehicle(FusionUtils.PlayerVehicle);

                if (hoverVehicle == null || !hoverVehicle.IsInHoverMode)
                    return;

                //if (TimeMachineHandler.GetTimeMachineFromVehicle(FusionUtils.PlayerVehicle).NotNullAndExists() && TimeMachineHandler.GetTimeMachineFromVehicle(FusionUtils.PlayerVehicle).Properties.AreFlyingCircuitsBroken)
                //{
                //    return;
                //}

                hoverVehicle.IsAltitudeHolding = !hoverVehicle.IsAltitudeHolding;
                TextHandler.Me.ShowHelp("AltitudeHoldChange", true, TextHandler.Me.GetOnOff(hoverVehicle.IsAltitudeHolding));
            }
        }

        private void HoverMode_Aborted(object sender, EventArgs e)
        {

        }

        private void HoverMode_Tick(object sender, EventArgs e)
        {
            if (!_firstTick)
            {
                foreach (Vehicle Vehicle in World.GetAllVehicles())
                {
                    if (!Vehicle.IsFunctioning() || Vehicle.Model == ModelHandler.DMC12)
                        continue;

                    /*if (_hoverTraffic)
                    {
                        if (Vehicle.IsBoat || Vehicle.IsBicycle || Vehicle.IsBike || Vehicle.IsBlimp || Vehicle.IsAircraft || Vehicle.IsHelicopter || Vehicle.IsMotorcycle)
                            continue;

                        if (Vehicle.Model == ModelHandler.Deluxo || Vehicle.Model == ModelHandler.DMC12)
                            continue;

                        if (Vehicle.Driver.ExistsAndAlive() && Vehicle != FusionUtils.PlayerVehicle)
                        {
                            HoverVehicle hoverVehicle = HoverVehicle.GetFromVehicle(Vehicle);

                            if (!hoverVehicle.IsHoverModeAllowed)
                            {
                                hoverVehicle.IsHoverModeAllowed = true;
                                hoverVehicle.SetMode(true);
                            }
                        }
                    }*/

                    HoverVehicle.GetFromVehicle(Vehicle)?.Tick();
                }

                HoverVehicle.Clean();
            }

            if (Game.IsLoading || !_firstTick)
                return;

            Decorator.Register(BTTFVDecors.AllowHoverMode, DecorType.Bool);
            Decorator.Register(BTTFVDecors.IsInHoverMode, DecorType.Bool);
            Decorator.Register(BTTFVDecors.IsHoverBoosting, DecorType.Bool);
            Decorator.Register(BTTFVDecors.IsHoverLanding, DecorType.Bool);
            Decorator.Register(BTTFVDecors.IsVerticalBoosting, DecorType.Bool);
            Decorator.Register(BTTFVDecors.IsWaitForLanding, DecorType.Bool);
            Decorator.Register(BTTFVDecors.IsAltitudeHolding, DecorType.Bool);
            Decorator.Lock();

            _flyModeInput = new NativeInput(ModControls.Hover);
            _flyModeInput.OnControlLongPressed += OnFlyModeControlJustLongPressed;
            _flyModeInput.OnControlPressed += OnFlyModeControlJustPressed;

            _firstTick = false;
        }

        private void OnFlyModeControlJustLongPressed()
        {
            if (!ModControls.LongPressForHover || _nextModeChangeAllowed > Game.GameTime)
                return;

            //TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(FusionUtils.PlayerVehicle);

            //if (timeMachine != null && timeMachine.Mods.HoverUnderbody != ModState.On && timeMachine.Properties.AreFlyingCircuitsBroken)
            //{
            //    TextHandler.Me.ShowHelp("HoverDamaged");

            //    return;
            //}

            HoverVehicle.GetFromVehicle(FusionUtils.PlayerVehicle)?.SwitchMode();
            _nextModeChangeAllowed = Game.GameTime + 2000;
        }

        private void OnFlyModeControlJustPressed()
        {
            if (ModControls.LongPressForHover || _nextModeChangeAllowed > Game.GameTime)
                return;

            //TimeMachine timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(FusionUtils.PlayerVehicle);

            //if (timeMachine != null && timeMachine.Properties.AreFlyingCircuitsBroken)
            //{
            //    TextHandler.Me.ShowHelp("HoverDamaged");

            //    return;
            //}

            HoverVehicle.GetFromVehicle(FusionUtils.PlayerVehicle)?.SwitchMode();
            _nextModeChangeAllowed = Game.GameTime + 2000;
        }
    }
}
