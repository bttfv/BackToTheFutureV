using FusionLibrary;
using GTA;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using static BackToTheFutureV.InternalEnums;
using static BackToTheFutureV.InternalExtensions;

namespace BackToTheFutureV
{
    internal class CustomMenu : BTTFVMenu
    {
        private readonly NativeListItem<string> _wormholeType;
        private readonly NativeListItem<string> _wheelsType;
        private readonly NativeCheckboxItem _hook;
        private readonly NativeCheckboxItem _bulova;
        private readonly NativeCheckboxItem _threeDigits;
        private readonly NativeListItem<string> _plate;
        private readonly NativeListItem<string> _exhaust;
        private readonly NativeListItem<string> _suspensions;
        private readonly NativeListItem<string> _hood;

        public CustomMenu() : base("Custom")
        {
            _wormholeType = NewListItem("Wormhole", TextHandler.Me.GetLocalizedText("BTTF1", "BTTF2", "BTTF3"));
            _wormholeType.ItemChanged += ModList_ItemChanged;

            _wheelsType = NewLocalizedListItem("Wheel", "Stock", "Red", "Rail", "DMC");
            _wheelsType.ItemChanged += ModList_ItemChanged;

            _hook = NewCheckboxItem("Hook");
            _bulova = NewCheckboxItem("Bulova");
            _threeDigits = NewCheckboxItem("Speedo");

            _plate = NewLocalizedListItem("Plate", "Empty", "Outatime", "Futuristic", "NoTime", "Timeless", "Timeless2", "DMCFactory", "DMCFactory2");
            _plate.ItemChanged += ModList_ItemChanged;

            _exhaust = NewLocalizedListItem("Exhaust", "Stock", "BTTF", "None");
            _exhaust.ItemChanged += ModList_ItemChanged;

            _suspensions = NewLocalizedListItem("Suspensions", "Stock", "LiftFrontLowerRear", "LiftFront", "LiftRear", "LiftFrontAndRear", "LowerFrontLiftRear", "LowerFront", "LowerRear", "LowerFrontAndRear");
            _suspensions.ItemChanged += ModList_ItemChanged;

            _hood = NewLocalizedListItem("Hood", "Stock", "1983", "1981");
            _hood.ItemChanged += ModList_ItemChanged;
        }

        private void ModList_ItemChanged(object sender, ItemChangedEventArgs<string> e)
        {
            if (!Game.IsControlJustPressed(Control.PhoneLeft) && !Game.IsControlJustPressed(Control.PhoneRight))
            {
                return;
            }

            int newIndex = e.Index;

            if (sender == _wormholeType)
            {
                CurrentTimeMachine.Mods.WormholeType = (WormholeType)(newIndex + 1);
            }
            else if (sender == _wheelsType)
            {
                GarageMenu.GarageSounds[0].Play();
                CurrentTimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.RimCustom, FusionEnums.CameraSwitchType.Instant, 1600);
                switch (newIndex)
                {
                    case 0:
                        if (!CurrentTimeMachine.Mods.IsDMC12 && CurrentTimeMachine.Mods.HoverUnderbody == ModState.On)
                        {
                            CurrentTimeMachine.Mods.Wheel = WheelType.Stock;
                            break;
                        }

                        CurrentTimeMachine.Mods.Wheel = WheelType.Stock;
                        break;
                    case 1:
                        CurrentTimeMachine.Mods.Wheel = WheelType.Red;
                        break;
                    case 2:
                        if (CurrentTimeMachine.Mods.HoverUnderbody == ModState.On && !CurrentTimeMachine.Properties.AreFlyingCircuitsBroken)
                        {
                            if (CurrentTimeMachine.Mods.Wheel == WheelType.Red)
                            {
                                if (CurrentTimeMachine.Mods.IsDMC12)
                                {
                                    CurrentTimeMachine.Mods.Wheel = WheelType.Stock;
                                }
                                else
                                {
                                    CurrentTimeMachine.Mods.Wheel = WheelType.DMC;
                                }
                            }
                            else
                            {
                                CurrentTimeMachine.Mods.Wheel = WheelType.Red;
                            }

                            break;
                        }

                        CurrentTimeMachine.Mods.Wheel = WheelType.RailroadInvisible;
                        break;
                    case 3:
                        if (CurrentTimeMachine.Mods.IsDMC12)
                        {
                            CurrentTimeMachine.Mods.Wheel = WheelType.Stock;
                        }
                        else
                        {
                            CurrentTimeMachine.Mods.Wheel = WheelType.DMC;
                        }

                        break;
                }
            }
            else if (sender == _plate)
            {
                GarageMenu.GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                CurrentTimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.PlateCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                CurrentTimeMachine.Mods.Plate = (PlateType)(newIndex - 1);
            }
            else if (sender == _exhaust)
            {
                GarageMenu.GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                CurrentTimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.ExhaustCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                CurrentTimeMachine.Mods.Exhaust = (ExhaustType)(newIndex - 1);
            }
            else if (sender == _suspensions)
            {
                GarageMenu.GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                CurrentTimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.SuspensionsCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                CurrentTimeMachine.Mods.SuspensionsType = (SuspensionsType)newIndex;
            }
            else if (sender == _hood)
            {
                GarageMenu.GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                CurrentTimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.HoodCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                CurrentTimeMachine.Mods.Hood = (HoodType)(newIndex - 1);
            }
        }

        private void LoadVehicleMods()
        {
            _wormholeType.SelectedIndex = (int)CurrentTimeMachine.Mods.WormholeType - 1;

            if (CurrentTimeMachine.Mods.IsDMC12)
            {
                _hook.Checked = CurrentTimeMachine.Mods.Hook != HookState.Off;
                _bulova.Checked = CurrentTimeMachine.Mods.Bulova != ModState.Off;
                _plate.SelectedIndex = (int)CurrentTimeMachine.Mods.Plate + 1;
                _exhaust.SelectedIndex = (int)CurrentTimeMachine.Mods.Exhaust + 1;
                _suspensions.SelectedIndex = (int)CurrentTimeMachine.Mods.SuspensionsType;
                _hood.SelectedIndex = (int)CurrentTimeMachine.Mods.Hood + 1;
                _threeDigits.Checked = CurrentTimeMachine.Mods.Speedo != ModState.Off || CurrentTimeMachine.Properties.ThreeDigit2D;
                _exhaust.Enabled = CurrentTimeMachine.Mods.HoverUnderbody == ModState.Off;
                _suspensions.Enabled = CurrentTimeMachine.Mods.HoverUnderbody == ModState.Off;
            }

            switch (CurrentTimeMachine.Mods.Wheel)
            {
                case WheelType.Stock:
                case WheelType.StockInvisible:
                    _wheelsType.SelectedIndex = 0;
                    break;
                case WheelType.Red:
                case WheelType.RedInvisible:
                    _wheelsType.SelectedIndex = 1;
                    break;
                case WheelType.RailroadInvisible:
                    _wheelsType.SelectedIndex = 2;
                    break;
                case WheelType.DMC:
                case WheelType.DMCInvisible:
                    _wheelsType.SelectedIndex = 3;
                    break;
            }
        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            if (MenuHandler.MainMenu.Visible)
            {
                MenuHandler.MainMenu.Visible = false;
            }

            if (MenuHandler.TimeMachineMenu.Visible)
            {
                MenuHandler.TimeMachineMenu.Visible = false;
            }

            _hook.Enabled = CurrentTimeMachine.Mods.IsDMC12;
            _bulova.Enabled = CurrentTimeMachine.Mods.IsDMC12;
            _plate.Enabled = CurrentTimeMachine.Mods.IsDMC12;
            _exhaust.Enabled = CurrentTimeMachine.Mods.IsDMC12;
            _suspensions.Enabled = CurrentTimeMachine.Mods.IsDMC12;
            _wheelsType.Enabled = CurrentTimeMachine.Vehicle.IsAutomobile;
            _hood.Enabled = CurrentTimeMachine.Mods.IsDMC12;

            LoadVehicleMods();
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {

        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == _hook)
            {
                GarageMenu.GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                CurrentTimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.HookCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                if (Checked)
                {
                    CurrentTimeMachine.Mods.Hook = HookState.OnDoor;
                }
                else
                {
                    CurrentTimeMachine.Mods.Hook = HookState.Off;
                }
            }
            else if (sender == _threeDigits)
            {
                if (CurrentTimeMachine.Mods.IsDMC12)
                {
                    CurrentTimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.DigitalSpeedo, FusionEnums.CameraSwitchType.Instant, 1250);
                    CurrentTimeMachine.Mods.Speedo = ConvertFromBool(Checked);
                }
                else
                {
                    CurrentTimeMachine.Properties.ThreeDigit2D = Checked;
                }
            }
            else if (sender == _bulova)
            {
                CurrentTimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.DigitalSpeedo, FusionEnums.CameraSwitchType.Instant, 1250);
                CurrentTimeMachine.Mods.Bulova = ConvertFromBool(Checked);
            }
        }

        public override void Tick()
        {
            if (FusionUtils.PlayerVehicle != CurrentTimeMachine)
            {
                Visible = false;
                return;
            }
            // Maybe have to do another check to make sure there are no nulls in time machine properties too?
            LoadVehicleMods();
        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {

        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {
            FusionUtils.HideGUI = false;
        }
    }
}
