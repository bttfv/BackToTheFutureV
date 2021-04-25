using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class CustomMenu : BTTFVMenu
    {
        public bool ForceNew = false;
        public bool FromGarage = false;

        private NativeListItem<string> _wormholeType;
        private NativeListItem<string> _reactorType;
        private NativeListItem<string> _wheelsType;
        private NativeCheckboxItem _hoverUnderbody;
        private NativeCheckboxItem _hoodBox;
        private NativeCheckboxItem _hook;
        private NativeCheckboxItem _threeDigits;
        private NativeListItem<string> _plate;
        private NativeListItem<string> _exhaust;
        private NativeListItem<string> _suspensions;
        private NativeListItem<string> _hood;
        private NativeItem _saveConf;
        private NativeItem _confirm;

        private bool _save = false;
        private TimeMachine _tempTimeMachine;

        public CustomMenu() : base("Custom")
        {
            _wormholeType = NewListItem("Wormhole", TextHandler.GetLocalizedText("BTTF1", "BTTF2", "BTTF3"));
            _wormholeType.ItemChanged += ModList_ItemChanged;

            _reactorType = NewLocalizedListItem("Reactor", "MrFusion", "Nuclear");
            _reactorType.ItemChanged += ModList_ItemChanged;

            _wheelsType = NewLocalizedListItem("Wheel", "Stock", "Red", "Rail", "DMC");
            _wheelsType.ItemChanged += ModList_ItemChanged;

            _hoverUnderbody = NewCheckboxItem("Hover");
            _hoverUnderbody.Selected += _hoverUnderbody_Selected;

            _hoodBox = NewCheckboxItem("ControlTubes");

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

            _saveConf = NewItem("Save");

            _confirm = NewItem("Confirm");
        }

        private void _hoverUnderbody_Selected(object sender, SelectedEventArgs e)
        {
            if (!_hoverUnderbody.Enabled && _tempTimeMachine.Properties.AreFlyingCircuitsBroken)
                TextHandler.ShowSubtitle("HoverDamaged");
        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {
            if (!_save)
                _tempTimeMachine.Vehicle.Delete();
        }

        private void ModList_ItemChanged(object sender, ItemChangedEventArgs<string> e)
        {
            int newIndex = e.Index;

            if (sender == _wormholeType)
            {
                _tempTimeMachine.Mods.WormholeType = (WormholeType)(newIndex + 1);
            }
            else if (sender == _reactorType)
            {
                _tempTimeMachine.Mods.Reactor = (ReactorType)newIndex;
            }
            else if (sender == _wheelsType)
            {
                switch (newIndex)
                {
                    case 0:
                        _tempTimeMachine.Mods.Wheel = WheelType.Stock;
                        break;
                    case 1:
                        _tempTimeMachine.Mods.Wheel = WheelType.Red;
                        break;
                    case 2:
                        _tempTimeMachine.Mods.Wheel = WheelType.RailroadInvisible;
                        break;
                    case 3:
                        if (_tempTimeMachine.Mods.IsDMC12)
                            _tempTimeMachine.Mods.Wheel = WheelType.Stock;
                        else
                            _tempTimeMachine.Mods.Wheel = WheelType.DMC;
                        break;
                }
            }
            else if (sender == _plate)
            {
                _tempTimeMachine.Mods.Plate = (PlateType)(newIndex - 1);
            }
            else if (sender == _exhaust)
            {
                _tempTimeMachine.Mods.Exhaust = (ExhaustType)(newIndex - 1);
            }
            else if (sender == _suspensions)
            {
                _tempTimeMachine.Mods.SuspensionsType = (SuspensionsType)newIndex;
            }
            else if (sender == _hood)
            {
                _tempTimeMachine.Mods.Hood = (HoodType)(newIndex - 1);
            }
        }

        private void LoadVehicleMods()
        {
            _wormholeType.SelectedIndex = (int)(_tempTimeMachine.Mods.WormholeType) - 1;
            _hoverUnderbody.Checked = ConvertFromModState(_tempTimeMachine.Mods.HoverUnderbody);

            if (_tempTimeMachine.Mods.IsDMC12)
            {
                _hoverUnderbody.Enabled = !_tempTimeMachine.Properties.AreFlyingCircuitsBroken || _hoverUnderbody.Checked;

                _reactorType.SelectedIndex = (int)_tempTimeMachine.Mods.Reactor;
                _hoodBox.Checked = ConvertFromModState(_tempTimeMachine.Mods.Hoodbox);
                _hook.Checked = _tempTimeMachine.Mods.Hook != HookState.Off;
                _plate.SelectedIndex = (int)(_tempTimeMachine.Mods.Plate) + 1;
                _exhaust.SelectedIndex = (int)(_tempTimeMachine.Mods.Exhaust) + 1;
                _suspensions.SelectedIndex = (int)_tempTimeMachine.Mods.SuspensionsType;
                _hood.SelectedIndex = (int)_tempTimeMachine.Mods.Hood + 1;
                _threeDigits.Checked = _tempTimeMachine.Properties.ThreeDigitsSpeedo;

                _wheelsType.Enabled = !_tempTimeMachine.Properties.IsFlying;
                _exhaust.Enabled = !_tempTimeMachine.Properties.IsFlying;
                _suspensions.Enabled = !_tempTimeMachine.Properties.IsFlying;
            }

            switch (_tempTimeMachine.Mods.Wheel)
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

            if (!FromGarage)
                return;

            _hoverUnderbody.Enabled = false;
            _reactorType.Enabled = false;
            _hoodBox.Enabled = false;
        }

        private ModState ConvertFromBool(bool value)
        {
            if (value)
                return ModState.On;
            else
                return ModState.Off;
        }

        private bool ConvertFromModState(ModState value)
        {
            if (value == ModState.On)
                return true;

            return false;
        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            if (MenuHandler.MainMenu.Visible)
                MenuHandler.MainMenu.Close();

            if (MenuHandler.TimeMachineMenu.Visible)
                MenuHandler.TimeMachineMenu.Close();

            if (ForceNew || (FusionUtils.PlayerVehicle == null || !FusionUtils.PlayerVehicle.IsTimeMachine()))
            {
                if (ForceNew)
                    ForceNew = false;

                if (ForceNew || FusionUtils.PlayerVehicle == null)
                    _tempTimeMachine = TimeMachineHandler.Create(SpawnFlags.WarpPlayer);
                else
                {
                    _tempTimeMachine = TimeMachineHandler.Create(FusionUtils.PlayerVehicle);
                    _save = true;
                }
            }
            else if (FusionUtils.PlayerVehicle.IsTimeMachine())
            {
                _tempTimeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(FusionUtils.PlayerVehicle);
                _save = true;

                if (_tempTimeMachine.Constants.FullDamaged)
                {
                    Close();
                    return;
                }

                _reactorType.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _hoverUnderbody.Enabled = _tempTimeMachine.Vehicle.CanHoverTransform() || _tempTimeMachine.Vehicle.Model == ModelHandler.DeluxoModel || _tempTimeMachine.Vehicle.Model == ModelHandler.DMC12;
                _hoodBox.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _hook.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _plate.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _exhaust.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _suspensions.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _hood.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _threeDigits.Enabled = _tempTimeMachine.Mods.IsDMC12;
            }

            LoadVehicleMods();
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == _wormholeType | sender == _confirm)
            {
                _save = true;
                GarageHandler.WaitForCustomMenu = false;
                Close();
            }
            else if (sender == _saveConf)
            {
                string _name = Game.GetUserInput(WindowTitle.EnterMessage20, "", 20);

                if (_name == null || _name == "")
                    return;

                _tempTimeMachine.Clone().Save(_name);
            }
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == _hoverUnderbody)
            {
                _tempTimeMachine.Mods.HoverUnderbody = ConvertFromBool(Checked);
            }
            else if (sender == _hoodBox)
            {
                _tempTimeMachine.Mods.Hoodbox = ConvertFromBool(Checked);
            }
            else if (sender == _hook)
            {
                if (Checked)
                    _tempTimeMachine.Mods.Hook = HookState.OnDoor;
                else
                    _tempTimeMachine.Mods.Hook = HookState.Off;
            }
            else if (sender == _threeDigits)
            {
                _tempTimeMachine.Properties.ThreeDigitsSpeedo = !Checked;
                _tempTimeMachine.Properties.HUDProperties.ThreeDigitsSpeedo = !Checked;
            }
        }

        public override void Tick()
        {
            if (FusionUtils.PlayerVehicle != _tempTimeMachine)
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
    }
}
