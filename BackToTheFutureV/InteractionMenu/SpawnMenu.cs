using BackToTheFutureV.Delorean;
using BackToTheFutureV.Delorean.Handlers;
using GTA;
using GTA.Math;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.InteractionMenu
{
    public class SpawnMenu : UIMenu
    {
        public bool ForceNew = false;

        private UIMenuListItem _baseType;
        private UIMenuListItem _powerSource;
        private UIMenuListItem _wheelsType;
        private UIMenuCheckboxItem _canFly;
        private UIMenuCheckboxItem _hoodBox;
        private UIMenuCheckboxItem _hook;
        private UIMenuListItem _plate;
        private UIMenuListItem _exhaust;
        private UIMenuListItem _suspensions;
        private UIMenuItem _saveConf;
        private UIMenuItem _confirm;

        private readonly List<object> _listTypes = new List<object> { "BTTF1", "BTTF2", "BTTF3" };
        private readonly List<object> _listPowerSources = new List<object> {Game.GetLocalizedString("BTTFV_Input_SpawnMenu_MrFusion"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Nuclear") };
        private readonly List<object> _listWheelTypes = new List<object> { Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wheel_Stock"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wheel_RedWhite"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wheel_Railroads") };
        private readonly List<object> _listPlateTypes = new List<object> { Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Plate_Empty"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Plate_Outatime"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Plate_Futuristic") };
        private readonly List<object> _listExhaustTypes = new List<object> { Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Exhaust_Stock"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Exhaust_BTTF"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Exhaust_None") };
        private readonly List<object> _listSuspensionsTypes = new List<object> { Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wheel_Stock"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LiftFrontLowerRear"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LiftFront"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LiftRear"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LiftFrontAndRear"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LowerFrontLiftRear"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LowerFront"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LowerRear"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_LowerFrontAndRear") };

        private bool _save = false;
        private DeloreanTimeMachine _tempTimeMachine;
        private int _wheelIndex;

        public SpawnMenu() : base(Game.GetLocalizedString("BTTFV_Input_SpawnMenu"), Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Description"))
        {
            AddItem(_baseType = new UIMenuListItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wormhole"), _listTypes, 0, Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wormhole_Description")));
            
            AddItem(_powerSource = new UIMenuListItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_PowerSource"), _listPowerSources, 0, Game.GetLocalizedString("BTTFV_Input_SpawnMenu_PowerSource_Description")));

            AddItem(_wheelsType = new UIMenuListItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wheel"), _listWheelTypes, 0, Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Wheel_Description")));

            AddItem(_canFly = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hover"), false, Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hover_Description")));

            AddItem(_hoodBox = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_ControlTubes"), false, Game.GetLocalizedString("BTTFV_Input_SpawnMenu_ControlTubes_Description")));

            AddItem(_hook = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hook"), false, Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Hook_Description")));

            AddItem(_plate = new UIMenuListItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_LicPlate"), _listPlateTypes, 0, Game.GetLocalizedString("BTTFV_Input_SpawnMenu_LicPlate_Description")));

            AddItem(_exhaust = new UIMenuListItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Exhaust"), _listExhaustTypes, 0, Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Exhaust_Description")));

            AddItem(_suspensions = new UIMenuListItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions"), _listSuspensionsTypes, 0, Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Suspensions_Description")));

            AddItem(_saveConf = new UIMenuItem(Game.GetLocalizedString("BTTFV_Input_PresetsMenu_Save"), Game.GetLocalizedString("BTTFV_Input_PresetsMenu_Save_Description")));

            AddItem(_confirm = new UIMenuItem(Game.GetLocalizedString("BTTFV_Input_SpawnMenu_Confirm")));

            OnListChange += SpawnMenu_OnListChange;
            OnCheckboxChange += SpawnMenu_OnCheckboxChange;
            OnItemSelect += SpawnMenu_OnItemSelect;
            OnMenuOpen += SpawnMenu_OnMenuOpen;
            OnMenuClose += SpawnMenu_OnMenuClose;
        }

        private void SpawnMenu_OnMenuOpen(UIMenu sender)
        {
            if (ForceNew || (Main.PlayerVehicle == null || !DeloreanHandler.IsVehicleATimeMachine(Main.PlayerVehicle)))
            {
                if (ForceNew)
                    ForceNew = false;

                Vector3 spawnPos = Main.PlayerPed.Position;

                if (Main.PlayerVehicle != null)
                    spawnPos = Main.PlayerVehicle.Position.Around(5f);

                _tempTimeMachine = (DeloreanTimeMachine)DMC12.CreateDelorean(spawnPos, Main.PlayerPed.Heading, DeloreanType.BTTF1);

                Main.PlayerPed.SetIntoVehicle(_tempTimeMachine.Vehicle, VehicleSeat.Driver);

                _tempTimeMachine.Vehicle.PlaceOnGround();

                _tempTimeMachine.MPHSpeed = 1;
            }                
            else if (DeloreanHandler.IsVehicleATimeMachine(Main.PlayerVehicle))
            {
                _tempTimeMachine = DeloreanHandler.GetTimeMachineFromVehicle(Main.PlayerVehicle);
                _save = true;

                _wheelsType.Enabled = !_tempTimeMachine.Circuits.IsFlying;                
            }
                
            LoadVehicleType();
        }

        private void LoadVehicleType()
        {
            _baseType.Index = (int)(_tempTimeMachine.DeloreanType) - 1;
            _powerSource.Index = (int)_tempTimeMachine.Mods.Reactor;
            _canFly.Checked = ConvertFromModState(_tempTimeMachine.Mods.HoverUnderbody);
            _hoodBox.Checked = ConvertFromModState(_tempTimeMachine.Mods.Hoodbox);
            _hook.Checked = _tempTimeMachine.Mods.Hook != HookState.Off;
            _plate.Index = (int)(_tempTimeMachine.Mods.Plate) + 1;
            _exhaust.Index = (int)(_tempTimeMachine.Mods.Exhaust) + 1;
            _suspensions.Index = (int)_tempTimeMachine.Mods.SuspensionsType;

            switch (_tempTimeMachine.Mods.Wheel)
            {
                case WheelType.Stock:
                    _wheelsType.Index = 0;
                    break;
                case WheelType.Red:
                    _wheelsType.Index = 1;
                    break;
                case WheelType.RailroadInvisible:
                    _wheelsType.Index = 2;
                    break;
            }

            _wheelIndex = _wheelsType.Index;
        }

        private void SpawnMenu_OnMenuClose(UIMenu sender)
        {
            if (!_save)
                _tempTimeMachine.Vehicle.Delete();
        }

        private void SpawnMenu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (checkboxItem == _canFly)
            {
                _tempTimeMachine.Mods.HoverUnderbody = ConvertFromBool(Checked);

                _exhaust.Index = 2;
                _suspensions.Index = 0;
            }
            else if (checkboxItem == _hoodBox)
            {               
                _tempTimeMachine.Mods.Hoodbox = ConvertFromBool(Checked);
            }
            else if (checkboxItem == _hook)
            {
                if (Checked)
                    _tempTimeMachine.Mods.Hook = HookState.OnDoor;
                else
                    _tempTimeMachine.Mods.Hook = HookState.Off;
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

        private void SpawnMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == _baseType | selectedItem == _confirm)
            {
                _save = true;
                Main.MenuPool.CloseAllMenus();
            }
            else if (selectedItem == _saveConf)
            {
                string _name = Game.GetUserInput(WindowTitle.EnterMessage20, "", 20);

                if (_name == null || _name == "")
                    return;

                _tempTimeMachine.Mods.Save(_name);
            }
        }

        private void SpawnMenu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == _baseType)
            {
                _tempTimeMachine.Mods.DeloreanType = (DeloreanType)(newIndex + 1);
            }
            else if (listItem == _powerSource)
            {
                _tempTimeMachine.Mods.Reactor = (ReactorType)newIndex;
            }
            else if (listItem == _wheelsType)
            {
                if (listItem.Enabled == false || _tempTimeMachine.Circuits.IsFlying || _tempTimeMachine.Circuits.GetHandler<FlyingHandler>().IsPlayingAnim)
                {
                    listItem.Index = _wheelIndex;
                    return;
                }

                switch (newIndex)
                {
                    case 0:
                        _tempTimeMachine.Mods.Wheel = WheelType.Stock;
                        _canFly.Enabled = true;
                        break;
                    case 1:
                        _tempTimeMachine.Mods.Wheel = WheelType.Red;
                        _canFly.Enabled = true;
                        break;
                    case 2:
                    {
                        _tempTimeMachine.Mods.Wheel = WheelType.RailroadInvisible;
                        _tempTimeMachine.Mods.HoverUnderbody = ModState.Off;

                        _canFly.Checked = false;
                        _canFly.Enabled = false;
                        break;
                    }
                }

                _wheelIndex = newIndex;
            }
            else if (listItem == _plate)
            {
                _tempTimeMachine.Mods.Plate = (PlateType)(newIndex - 1);
            }
            else if (listItem == _exhaust)
            {
                _tempTimeMachine.Mods.Exhaust = (ExhaustType)(newIndex - 1);
                _canFly.Checked = false;
            }
            else if (listItem == _suspensions)
            {                
                _tempTimeMachine.Mods.SuspensionsType = (SuspensionsType)newIndex;

                if (_tempTimeMachine.Mods.SuspensionsType != SuspensionsType.Stock)
                    _canFly.Checked = false;
            }
        }
    }
}
