using FusionLibrary;
using GTA;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class CustomMenu2 : BTTFVMenu
    {
        private NativeListItem<string> _wormholeType;
        private NativeListItem<string> _wheelsType;
        private NativeCheckboxItem _hook;
        private NativeCheckboxItem _threeDigits;
        private NativeListItem<string> _plate;
        private NativeListItem<string> _exhaust;
        private NativeListItem<string> _suspensions;
        private NativeListItem<string> _hood;

        private TimeMachine TimeMachine => TimeMachineHandler.CurrentTimeMachine;

        public CustomMenu2() : base("Custom")
        {
            _wormholeType = NewListItem("Wormhole", TextHandler.GetLocalizedText("BTTF1", "BTTF2", "BTTF3"));
            _wormholeType.ItemChanged += ModList_ItemChanged;

            _wheelsType = NewLocalizedListItem("Wheel", "Stock", "Red", "Rail", "DMC");
            _wheelsType.ItemChanged += ModList_ItemChanged;

            _hook = NewCheckboxItem("Hook");

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
                return;

            int newIndex = e.Index;

            if (sender == _wormholeType)
            {
                TimeMachine.Mods.WormholeType = (WormholeType)(newIndex + 1);
            }
            else if (sender == _wheelsType)
            {
                GarageMenu.GarageSounds[0].Play();
                TimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.RimCustom, FusionEnums.CameraSwitchType.Instant, 1600);
                switch (newIndex)
                {
                    case 0:
                        if (!TimeMachine.Mods.IsDMC12 && TimeMachine.Mods.HoverUnderbody == ModState.On)
                        {
                            TimeMachine.Mods.Wheel = WheelType.Stock;
                            break;
                        }

                        TimeMachine.Mods.Wheel = WheelType.Stock;
                        break;
                    case 1:
                        TimeMachine.Mods.Wheel = WheelType.Red;
                        break;
                    case 2:
                        if (TimeMachine.Mods.HoverUnderbody == ModState.On && !TimeMachine.Properties.AreFlyingCircuitsBroken)
                        {
                            if (TimeMachine.Mods.Wheel == WheelType.Red)
                            {
                                if (TimeMachine.Mods.IsDMC12)
                                    TimeMachine.Mods.Wheel = WheelType.Stock;
                                else
                                    TimeMachine.Mods.Wheel = WheelType.DMC;
                            }
                            else
                                TimeMachine.Mods.Wheel = WheelType.Red;

                            break;
                        }

                        TimeMachine.Mods.Wheel = WheelType.RailroadInvisible;
                        break;
                    case 3:
                        if (TimeMachine.Mods.IsDMC12)
                            TimeMachine.Mods.Wheel = WheelType.Stock;
                        else
                            TimeMachine.Mods.Wheel = WheelType.DMC;
                        break;
                }
            }
            else if (sender == _plate)
            {
                GarageMenu.GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                TimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.PlateCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                TimeMachine.Mods.Plate = (PlateType)(newIndex - 1);
            }
            else if (sender == _exhaust)
            {
                GarageMenu.GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                TimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.ExhaustCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                TimeMachine.Mods.Exhaust = (ExhaustType)(newIndex - 1);
            }
            else if (sender == _suspensions)
            {
                GarageMenu.GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                TimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.SuspensionsCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                TimeMachine.Mods.SuspensionsType = (SuspensionsType)newIndex;
            }
            else if (sender == _hood)
            {
                GarageMenu.GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                TimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.HoodCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                TimeMachine.Mods.Hood = (HoodType)(newIndex - 1);
            }
        }

        private void LoadVehicleMods()
        {
            _wormholeType.SelectedIndex = (int)(TimeMachine.Mods.WormholeType) - 1;

            if (TimeMachine.Mods.IsDMC12)
            {
                _hook.Checked = TimeMachine.Mods.Hook != HookState.Off;
                _plate.SelectedIndex = (int)(TimeMachine.Mods.Plate) + 1;
                _exhaust.SelectedIndex = (int)(TimeMachine.Mods.Exhaust) + 1;
                _suspensions.SelectedIndex = (int)TimeMachine.Mods.SuspensionsType;
                _hood.SelectedIndex = (int)TimeMachine.Mods.Hood + 1;
                _threeDigits.Checked = TimeMachine.Properties.ThreeDigitsSpeedo;

                _exhaust.Enabled = TimeMachine.Mods.HoverUnderbody == ModState.Off;
                _suspensions.Enabled = TimeMachine.Mods.HoverUnderbody == ModState.Off;
            }

            switch (TimeMachine.Mods.Wheel)
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
                MenuHandler.MainMenu.Close();

            if (MenuHandler.TimeMachineMenu.Visible)
                MenuHandler.TimeMachineMenu.Close();

            _hook.Enabled = TimeMachine.Mods.IsDMC12;
            _plate.Enabled = TimeMachine.Mods.IsDMC12;
            _exhaust.Enabled = TimeMachine.Mods.IsDMC12;
            _suspensions.Enabled = TimeMachine.Mods.IsDMC12;
            _hood.Enabled = TimeMachine.Mods.IsDMC12;
            _threeDigits.Enabled = TimeMachine.Mods.IsDMC12;

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
                TimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.HookCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                if (Checked)
                    TimeMachine.Mods.Hook = HookState.OnDoor;
                else
                    TimeMachine.Mods.Hook = HookState.Off;
            }
            else if (sender == _threeDigits)
            {
                TimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.DigitalSpeedo, FusionEnums.CameraSwitchType.Instant, 1250);
                TimeMachine.Properties.ThreeDigitsSpeedo = !Checked;
                TimeMachine.Properties.HUDProperties.ThreeDigitsSpeedo = !Checked;
            }
        }

        public override void Tick()
        {
            if (FusionUtils.PlayerVehicle != TimeMachine)
            {
                Close();
                return;
            }

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

        }
    }
}
