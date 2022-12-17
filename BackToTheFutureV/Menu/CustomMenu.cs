using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using static BackToTheFutureV.InternalEnums;
using static BackToTheFutureV.InternalExtensions;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class CustomMenu : BTTFVMenu
    {
        public bool ForceNew = false;

        private readonly NativeListItem<string> _wormholeType;
        private readonly NativeListItem<string> _reactorType;
        private readonly NativeListItem<string> _wheelsType;
        private readonly NativeCheckboxItem _hoverUnderbody;
        private readonly NativeCheckboxItem _hoodBox;
        private readonly NativeCheckboxItem _hook;
        private readonly NativeCheckboxItem _bulova;
        private readonly NativeCheckboxItem _threeDigits;
        private readonly NativeListItem<string> _plate;
        private readonly NativeListItem<string> _exhaust;
        private readonly NativeListItem<string> _suspensions;
        private readonly NativeListItem<string> _hood;
        private readonly NativeItem _saveConf;
        private readonly NativeItem _confirm;

        public CustomMenu() : base("Custom")
        {
            _wormholeType = NewListItem("Wormhole", TextHandler.Me.GetLocalizedText("BTTF1", "BTTF2", "BTTF3"));
            _wormholeType.ItemChanged += ModList_ItemChanged;

            _reactorType = NewLocalizedListItem("Reactor", "MrFusion", "Nuclear");
            _reactorType.ItemChanged += ModList_ItemChanged;

            _wheelsType = NewLocalizedListItem("Wheel", "Stock", "Red", "Rail", "DMC");
            _wheelsType.ItemChanged += ModList_ItemChanged;

            _hoverUnderbody = NewCheckboxItem("Hover");
            _hoverUnderbody.Selected += HoverUnderbody_Selected;

            _hoodBox = NewCheckboxItem("ControlTubes");

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

            _saveConf = NewItem("Save");

            _confirm = NewItem("Confirm");
        }

        private void HoverUnderbody_Selected(object sender, SelectedEventArgs e)
        {
            if (!_hoverUnderbody.Enabled && CurrentTimeMachine.Properties.AreFlyingCircuitsBroken)
            {
                TextHandler.Me.ShowSubtitle("HoverDamaged");
            }
        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {

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
            else if (sender == _reactorType)
            {
                CurrentTimeMachine.Mods.Reactor = (ReactorType)newIndex;
            }
            else if (sender == _wheelsType)
            {
                switch (newIndex)
                {
                    case 0:
                        CurrentTimeMachine.Mods.Wheel = WheelType.Stock;
                        break;
                    case 1:
                        CurrentTimeMachine.Mods.Wheel = WheelType.Red;
                        break;
                    case 2:
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
                CurrentTimeMachine.Mods.Plate = (PlateType)(newIndex - 1);
            }
            else if (sender == _exhaust)
            {
                CurrentTimeMachine.Mods.Exhaust = (ExhaustType)(newIndex - 1);
            }
            else if (sender == _suspensions)
            {
                CurrentTimeMachine.Mods.SuspensionsType = (SuspensionsType)newIndex;
            }
            else if (sender == _hood)
            {
                CurrentTimeMachine.Mods.Hood = (HoodType)(newIndex - 1);
            }
        }

        private void LoadVehicleMods()
        {
            _wormholeType.SelectedIndex = (int)CurrentTimeMachine.Mods.WormholeType - 1;
            _hoverUnderbody.Checked = ConvertFromModState(CurrentTimeMachine.Mods.HoverUnderbody);

            if (CurrentTimeMachine.Mods.IsDMC12)
            {
                _hoverUnderbody.Enabled = !CurrentTimeMachine.Properties.AreFlyingCircuitsBroken || _hoverUnderbody.Checked;
                _reactorType.SelectedIndex = (int)CurrentTimeMachine.Mods.Reactor;
                _hoodBox.Checked = ConvertFromModState(CurrentTimeMachine.Mods.Hoodbox);
                _hook.Checked = CurrentTimeMachine.Mods.Hook != HookState.Off;
                _bulova.Checked = CurrentTimeMachine.Mods.Bulova != ModState.Off;
                _plate.SelectedIndex = (int)CurrentTimeMachine.Mods.Plate + 1;
                _exhaust.SelectedIndex = (int)CurrentTimeMachine.Mods.Exhaust + 1;
                _suspensions.SelectedIndex = (int)CurrentTimeMachine.Mods.SuspensionsType;
                _hood.SelectedIndex = (int)CurrentTimeMachine.Mods.Hood + 1;
                _threeDigits.Checked = CurrentTimeMachine.Mods.Speedo != ModState.Off;
                _wheelsType.Enabled = !CurrentTimeMachine.Properties.IsFlying;
                _exhaust.Enabled = !CurrentTimeMachine.Properties.IsFlying;
                _suspensions.Enabled = !CurrentTimeMachine.Properties.IsFlying;
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

            if (ForceNew || FusionUtils.PlayerVehicle == null || !FusionUtils.PlayerVehicle.IsTimeMachine())
            {
                if (ForceNew || FusionUtils.PlayerVehicle == null)
                {
                    TimeMachineHandler.Create(SpawnFlags.WarpPlayer);
                }
                else
                {
                    TimeMachineHandler.Create(FusionUtils.PlayerVehicle);
                }
            }
            else if (FusionUtils.PlayerVehicle.IsTimeMachine())
            {
                if (CurrentTimeMachine.Constants.FullDamaged)
                {
                    Visible = false;
                    return;
                }

                _reactorType.Enabled = CurrentTimeMachine.Mods.IsDMC12;
                _hoverUnderbody.Enabled = CurrentTimeMachine.Vehicle.CanHoverTransform() || CurrentTimeMachine.Vehicle.Model == ModelHandler.DeluxoModel || CurrentTimeMachine.Vehicle.Model == ModelHandler.DMC12;
                _hoodBox.Enabled = CurrentTimeMachine.Mods.IsDMC12;
                _hook.Enabled = CurrentTimeMachine.Mods.IsDMC12;
                _bulova.Enabled = CurrentTimeMachine.Mods.IsDMC12;
                _plate.Enabled = CurrentTimeMachine.Mods.IsDMC12;
                _exhaust.Enabled = CurrentTimeMachine.Mods.IsDMC12;
                _suspensions.Enabled = CurrentTimeMachine.Mods.IsDMC12;
                _wheelsType.Enabled = CurrentTimeMachine.Vehicle.IsAutomobile;
                _hood.Enabled = CurrentTimeMachine.Mods.IsDMC12;
                _threeDigits.Enabled = CurrentTimeMachine.Mods.IsDMC12;
            }

            Script.Yield();

            LoadVehicleMods();
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == _wormholeType | sender == _confirm)
            {
                GarageHandler.WaitForCustomMenu = false;
                Visible = false;
            }
            else if (sender == _saveConf)
            {
                string _name = Game.GetUserInput(WindowTitle.EnterMessage20, "", 20);

                if (_name == null || _name == "")
                {
                    return;
                }

                CurrentTimeMachine.Clone().Save(_name);
            }
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == _hoverUnderbody)
            {
                CurrentTimeMachine.Mods.HoverUnderbody = ConvertFromBool(Checked);
            }
            else if (sender == _hoodBox)
            {
                CurrentTimeMachine.Mods.Hoodbox = ConvertFromBool(Checked);
            }
            else if (sender == _hook)
            {
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
                CurrentTimeMachine.Mods.Speedo = ConvertFromBool(Checked);
            }
            else if (sender == _bulova)
            {
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

            LoadVehicleMods();
        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {

        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }
    }
}
