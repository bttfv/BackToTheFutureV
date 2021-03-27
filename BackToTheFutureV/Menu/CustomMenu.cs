using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using LemonUI.Menus;
using System;
using static BackToTheFutureV.Utility.InternalEnums;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Menu
{
    internal class CustomMenu : BTTFVMenu
    {
        public bool ForceNew = false;

        private NativeListItem<string> _wormholeType;
        private NativeListItem<string> _reactorType;
        private NativeListItem<string> _wheelsType;
        private NativeCheckboxItem _hoverUnderbody;
        private NativeCheckboxItem _hoodBox;
        private NativeCheckboxItem _hook;
        private NativeCheckboxItem _speedoCover;
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
            Shown += SettingsMenu_Shown;
            Closing += CustomMenu_Closing;
            OnItemCheckboxChanged += SettingsMenu_OnItemCheckboxChanged;
            OnItemActivated += CustomMenu_OnItemActivated;

            _wormholeType = NewListItem("Wormhole", GetLocalizedText("BTTF1"), GetLocalizedText("BTTF2"), GetLocalizedText("BTTF3"));
            _wormholeType.ItemChanged += ModList_ItemChanged;

            _reactorType = NewListItem("Reactor", GetLocalizedItemValueTitle("Reactor", "MrFusion"), GetLocalizedItemValueTitle("Reactor", "Nuclear"));
            _reactorType.ItemChanged += ModList_ItemChanged;

            _wheelsType = NewListItem("Wheel", GetLocalizedItemValueTitle("Wheel", "Stock"), GetLocalizedItemValueTitle("Wheel", "Red"), GetLocalizedItemValueTitle("Wheel", "Rail"), GetLocalizedItemValueTitle("Wheel", "DMC"));
            _wheelsType.ItemChanged += ModList_ItemChanged;
            _hoverUnderbody = NewCheckboxItem("Hover");

            _hoodBox = NewCheckboxItem("ControlTubes");

            _hook = NewCheckboxItem("Hook");

            _speedoCover = NewCheckboxItem("Speedo");

            _plate = NewListItem("Plate", GetLocalizedItemValueTitle("Plate", "Empty"), GetLocalizedItemValueTitle("Plate", "Outatime"), GetLocalizedItemValueTitle("Plate", "Futuristic"), GetLocalizedItemValueTitle("Plate", "NoTime"), GetLocalizedItemValueTitle("Plate", "Timeless"), GetLocalizedItemValueTitle("Plate", "Timeless2"), GetLocalizedItemValueTitle("Plate", "DMCFactory"), GetLocalizedItemValueTitle("Plate", "DMCFactory2"));
            _plate.ItemChanged += ModList_ItemChanged;

            _exhaust = NewListItem("Exhaust", GetLocalizedItemValueTitle("Exhaust", "Stock"), GetLocalizedItemValueTitle("Exhaust", "BTTF"), GetLocalizedItemValueTitle("Exhaust", "None"));
            _exhaust.ItemChanged += ModList_ItemChanged;

            _suspensions = NewListItem("Suspensions", GetLocalizedItemValueTitle("Suspensions", "Stock"), GetLocalizedItemValueTitle("Suspensions", "LiftFrontLowerRear"), GetLocalizedItemValueTitle("Suspensions", "LiftFront"), GetLocalizedItemValueTitle("Suspensions", "LiftRear"), GetLocalizedItemValueTitle("Suspensions", "LiftFrontAndRear"), GetLocalizedItemValueTitle("Suspensions", "LowerFrontLiftRear"), GetLocalizedItemValueTitle("Suspensions", "LowerFront"), GetLocalizedItemValueTitle("Suspensions", "LowerRear"), GetLocalizedItemValueTitle("Suspensions", "LowerFrontAndRear"));
            _suspensions.ItemChanged += ModList_ItemChanged;

            _hood = NewListItem("Hood", GetLocalizedItemValueTitle("Hood", "Stock"), GetLocalizedItemValueTitle("Hood", "1983"), GetLocalizedItemValueTitle("Hood", "1981"));
            _hood.ItemChanged += ModList_ItemChanged;

            _saveConf = NewItem("Save");

            _confirm = NewItem("Confirm");
        }

        private void CustomMenu_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

        private void LoadVehicleType()
        {
            _wormholeType.SelectedIndex = (int)(_tempTimeMachine.Mods.WormholeType) - 1;
            _hoverUnderbody.Checked = ConvertFromModState(_tempTimeMachine.Mods.HoverUnderbody);

            if (_tempTimeMachine.Mods.IsDMC12)
            {
                _reactorType.SelectedIndex = (int)_tempTimeMachine.Mods.Reactor;
                _hoodBox.Checked = ConvertFromModState(_tempTimeMachine.Mods.Hoodbox);
                _hook.Checked = _tempTimeMachine.Mods.Hook != HookState.Off;
                _plate.SelectedIndex = (int)(_tempTimeMachine.Mods.Plate) + 1;
                _exhaust.SelectedIndex = (int)(_tempTimeMachine.Mods.Exhaust) + 1;
                _suspensions.SelectedIndex = (int)_tempTimeMachine.Mods.SuspensionsType;
                _hood.SelectedIndex = (int)_tempTimeMachine.Mods.Hood + 1;
                _speedoCover.Checked = !_tempTimeMachine.Properties.ThreeDigitsSpeedo;

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

        private void SettingsMenu_Shown(object sender, EventArgs e)
        {
            if (MenuHandler.MainMenu.Visible)
                MenuHandler.MainMenu.Close();

            if (MenuHandler.TimeMachineMenu.Visible)
                MenuHandler.TimeMachineMenu.Close();

            if (ForceNew || (Utils.PlayerVehicle == null || !Utils.PlayerVehicle.IsTimeMachine()))
            {
                if (ForceNew)
                    ForceNew = false;

                _tempTimeMachine = TimeMachineHandler.Create(SpawnFlags.WarpPlayer);
            }
            else if (Utils.PlayerVehicle.IsTimeMachine())
            {
                _tempTimeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(Utils.PlayerVehicle);
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
                _speedoCover.Enabled = _tempTimeMachine.Mods.IsDMC12;
            }

            LoadVehicleType();
        }

        private void CustomMenu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == _wormholeType | sender == _confirm)
            {
                _save = true;
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

        private void SettingsMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
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
            else if (sender == _speedoCover)
            {
                _tempTimeMachine.Properties.ThreeDigitsSpeedo = !Checked;
            }
        }

        public override void Tick()
        {
            if (Utils.PlayerVehicle != _tempTimeMachine)
            {
                Close();
                return;
            }

            LoadVehicleType();
        }
    }
}
