using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Drawing;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Menu
{
    public class CustomMenu : CustomNativeMenu
    {
        public bool ForceNew = false;

        private NativeListItem<string> _baseType;
        private NativeListItem<string> _powerSource;
        private NativeListItem<string> _wheelsType;
        private NativeCheckboxItem _canFly;
        private NativeCheckboxItem _hoodBox;
        private NativeCheckboxItem _hook;
        private NativeListItem<string> _plate;
        private NativeListItem<string> _exhaust;
        private NativeListItem<string> _suspensions;
        private NativeListItem<string> _hood;
        private NativeItem _saveConf;
        private NativeItem _confirm;

        private readonly List<string> _listTypes = new List<string> { Game.GetLocalizedString("BTTFV_Menu_BTTF1"), Game.GetLocalizedString("BTTFV_Menu_BTTF2"), Game.GetLocalizedString("BTTFV_Menu_BTTF3") };
        private readonly List<string> _listPowerSources = new List<string> { Game.GetLocalizedString("BTTFV_Input_SpawnMenu_MrFusion"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Nuclear") };
        private readonly List<string> _listWheelTypes = new List<string> { Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wheel_Stock"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wheel_RedWhite"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wheel_Railroads") };
        private readonly List<string> _listPlateTypes = new List<string> { Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Plate_Empty"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Plate_Outatime"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Plate_Futuristic"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Plate_NoTime"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Plate_Timeless"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Plate_Timeless2"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Plate_DMCFactory"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Plate_DMCFactory2") };
        private readonly List<string> _listExhaustTypes = new List<string> { Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Exhaust_Stock"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Exhaust_BTTF"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Exhaust_None") };
        private readonly List<string> _listSuspensionsTypes = new List<string> { Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wheel_Stock"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LiftFrontLowerRear"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LiftFront"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LiftRear"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LiftFrontAndRear"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LowerFrontLiftRear"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LowerFront"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LowerRear"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LowerFrontAndRear") };
        private readonly List<string> _listHoodTypes = new List<string> { Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hood_Stock"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hood_1983"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hood_1981") };

        private bool _save = false;
        private TimeMachine _tempTimeMachine;

        public CustomMenu() : base("", Game.GetLocalizedString("BTTFV_Input_SpawnMenu"))
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");
            
            Shown += SettingsMenu_Shown;
            Closing += CustomMenu_Closing;
            OnItemCheckboxChanged += SettingsMenu_OnItemCheckboxChanged;
            OnItemActivated += CustomMenu_OnItemActivated;

            Add(_baseType = new NativeListItem<string>(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wormhole"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wormhole_Description"), _listTypes.ToArray()));
            _baseType.ItemChanged += ModList_ItemChanged;
            Add(_powerSource = new NativeListItem<string>(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_PowerSource"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_PowerSource_Description"), _listPowerSources.ToArray()));
            _powerSource.ItemChanged += ModList_ItemChanged;
            Add(_wheelsType = new NativeListItem<string>(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wheel"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wheel_Description"), _listWheelTypes.ToArray()));
            _wheelsType.ItemChanged += ModList_ItemChanged;
            Add(_canFly = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hover"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hover_Description")));

            Add(_hoodBox = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_ControlTubes"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_ControlTubes_Description")));

            Add(_hook = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hook"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hook_Description")));

            Add(_plate = new NativeListItem<string>(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_LicPlate"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_LicPlate_Description"), _listPlateTypes.ToArray()));
            _plate.ItemChanged += ModList_ItemChanged;
            Add(_exhaust = new NativeListItem<string>(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Exhaust"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Exhaust_Description"), _listExhaustTypes.ToArray()));
            _exhaust.ItemChanged += ModList_ItemChanged;
            Add(_suspensions = new NativeListItem<string>(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_Description"), _listSuspensionsTypes.ToArray()));
            _suspensions.ItemChanged += ModList_ItemChanged;

            Add(_hood = new NativeListItem<string>(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hood"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hood_Description"), _listHoodTypes.ToArray()));
            _hood.ItemChanged += ModList_ItemChanged;

            Add(_saveConf = new NativeItem(Game.GetLocalizedString("BTTFV_Input_PresetsMenu_Save"), Game.GetLocalizedString("BTTFV_Input_PresetsMenu_Save_Description")));

            Add(_confirm = new NativeItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Confirm")));
        }

        private void CustomMenu_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_save)
                _tempTimeMachine.Vehicle.Delete();
        }

        private void ModList_ItemChanged(object sender, ItemChangedEventArgs<string> e)
        {
            int newIndex = e.Index;

            if (sender == _baseType)
            {
                _tempTimeMachine.Mods.WormholeType = (WormholeType)(newIndex + 1);
            }
            else if (sender == _powerSource)
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
            _baseType.SelectedIndex = (int)(_tempTimeMachine.Mods.WormholeType) - 1;
            _canFly.Checked = ConvertFromModState(_tempTimeMachine.Mods.HoverUnderbody);
            
            if (_tempTimeMachine.Mods.IsDMC12)
            {
                _powerSource.SelectedIndex = (int)_tempTimeMachine.Mods.Reactor;
                _hoodBox.Checked = ConvertFromModState(_tempTimeMachine.Mods.Hoodbox);
                _hook.Checked = _tempTimeMachine.Mods.Hook != HookState.Off;
                _plate.SelectedIndex = (int)(_tempTimeMachine.Mods.Plate) + 1;
                _exhaust.SelectedIndex = (int)(_tempTimeMachine.Mods.Exhaust) + 1;
                _suspensions.SelectedIndex = (int)_tempTimeMachine.Mods.SuspensionsType;
                _hood.SelectedIndex = (int)_tempTimeMachine.Mods.Hood + 1;

                _canFly.Enabled = !_tempTimeMachine.Properties.IsFlying;// && _tempTimeMachine.Mods.SuspensionsType == SuspensionsType.Stock;
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

                if (_tempTimeMachine.Properties.FullDamaged)
                {
                    Close();
                    return;
                }

                _powerSource.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _canFly.Enabled = _tempTimeMachine.Vehicle.CanHoverTransform() && _tempTimeMachine.Vehicle.Model != ModelHandler.DeluxoModel;
                _hoodBox.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _hook.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _plate.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _exhaust.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _suspensions.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _powerSource.Enabled = _tempTimeMachine.Mods.IsDMC12;
                _powerSource.Enabled = _tempTimeMachine.Mods.IsDMC12;
            }
            
            LoadVehicleType();
        }

        private void CustomMenu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == _baseType | sender == _confirm)
            {
                _save = true;
                Close();
            }
            else if (sender == _saveConf)
            {
                string _name = Game.GetUserInput(WindowTitle.EnterMessage20, "", 20);

                if (_name == null || _name == "")
                    return;

                _tempTimeMachine.Clone.Save(_name);
            }
        }

        private void SettingsMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == _canFly)
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
