using BackToTheFutureV.Delorean;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.InteractionMenu
{
    public class RCMenu : UIMenu
    {
        public UIMenuDynamicListItem Deloreans { get; }

        public UIMenuCheckboxItem FuelChamberDescription { get; }
        public UIMenuCheckboxItem TimeCircuitsOnDescription { get; }
        public UIMenuItem DestinationTimeDescription { get; }

        public RCMenu() : base(Game.GetLocalizedString("BTTFV_Menu_RCMenu"), Game.GetLocalizedString("BTTFV_Menu_RCMenu_Description"))
        {
            AddItem(Deloreans = new UIMenuDynamicListItem(Game.GetLocalizedString("BTTFV_Menu_RCMenu_Deloreans"), Game.GetLocalizedString("BTTFV_Menu_RCMenu_Deloreans_Description"), "0", ChangeCallback));
            AddItem(FuelChamberDescription = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_RCMenu_FuelChamberFilled"), true, Game.GetLocalizedString("BTTFV_Menu_RCMenu_FuelChamberFilled_Description")));
            AddItem(TimeCircuitsOnDescription = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_RCMenu_TimeCircuitsOn"), true, Game.GetLocalizedString("BTTFV_Menu_RCMenu_TimeCircuitsOn_Description")));
            AddItem(DestinationTimeDescription = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime"), Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime_Description")));

            FuelChamberDescription.Enabled = false;
            TimeCircuitsOnDescription.Enabled = false;
            DestinationTimeDescription.Enabled = false;

            OnItemSelect += RCMenu_OnItemSelect;
            OnListChange += RCMenu_OnListChange;
            OnMenuClose += RCMenu_OnMenuClose;
        }

        private int _selectedIndex;

        private DeloreanTimeMachine _currentTimeMachine;
        private Camera _carCam;

        private void RCMenu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            _selectedIndex = newIndex;

            TrySelectCar(_selectedIndex);
        }

        private string ChangeCallback(UIMenuDynamicListItem sender, UIMenuDynamicListItem.ChangeDirection direction)
        {
            int index = _selectedIndex;

            if(direction == UIMenuDynamicListItem.ChangeDirection.Right)
            {
                if(DeloreanHandler.TimeMachineCount > 0)
                {
                    index++;

                    if (index > DeloreanHandler.TimeMachineCount - 1)
                        index = 0;
                }
            }
            else
            {
                if(DeloreanHandler.TimeMachineCount > 0)
                {
                    index--;

                    if (index < 0)
                        index = DeloreanHandler.TimeMachineCount - 1;
                }
            }

            _selectedIndex = index;

            if (!TrySelectCar(_selectedIndex))
            {
                GTA.UI.Notification.Show(Game.GetLocalizedString("BTTFV_OutOfRange"));
            }

            return _selectedIndex.ToString();
        }

        private void RCMenu_OnMenuClose(UIMenu sender)
        {
            StopPreviewing();
        }

        private void RCMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if(selectedItem == Deloreans)
            {
                if (TrySelectCar(_selectedIndex, false))
                {
                    Function.Call(Hash.CLEAR_FOCUS);
                    RCManager.RemoteControl(_currentTimeMachine);
                    Main.MenuPool.CloseAllMenus();
                }                
            }
        }

        private bool TrySelectCar(int index, bool preview = true)
        {
            DeloreanTimeMachine timeMachine = DeloreanHandler.GetTimeMachineFromIndex(index);

            if(timeMachine != null)
            {
                float dist = timeMachine.Vehicle.Position.DistanceToSquared(Main.PlayerPed.Position);

                if (dist <= RCManager.MAX_DIST * RCManager.MAX_DIST)
                {
                    _currentTimeMachine = timeMachine;

                    if(preview)
                        PreviewCar(timeMachine);

                    return true;
                }
                else
                {
                    if(preview)
                        StopPreviewing();

                    return false;
                }
            }

            return false;
        }

        public void PreviewCar(DeloreanTimeMachine timeMachine)
        {
            _currentTimeMachine = timeMachine;

            if (_carCam != null)
            {
                _carCam?.Delete();
            }

            _carCam = World.CreateCamera(_currentTimeMachine.Vehicle.GetOffsetPosition(new Vector3(0.0f, -5.0f, 3.0f)), World.RenderingCamera.Rotation, 75.0f);
            _carCam.PointAt(_currentTimeMachine.Vehicle);

            World.RenderingCamera = _carCam;

            Function.Call(Hash.CLEAR_FOCUS);
            Function.Call(Hash.SET_FOCUS_ENTITY, _currentTimeMachine.Vehicle);
        }

        public void StopPreviewing()
        {
            Function.Call(Hash.CLEAR_FOCUS);
            _carCam?.Delete();
            _carCam = null;
            World.RenderingCamera = null;
        }

        public void Process()
        {
            if (_selectedIndex != 0 && _selectedIndex > DeloreanHandler.TimeMachineCount - 1)
            {
                _selectedIndex = DeloreanHandler.TimeMachineCount - 1;
            }

            if (DeloreanHandler.TimeMachineCount == 0)
            {
                Deloreans.Enabled = false;
            }
            else
            {
                Deloreans.Enabled = true;

                TrySelectCar(_selectedIndex);

                if(_currentTimeMachine != null)
                {
                    TimeCircuitsOnDescription.Checked = _currentTimeMachine.Circuits.IsOn;
                    FuelChamberDescription.Checked = _currentTimeMachine.Circuits.IsFueled;
                    DestinationTimeDescription.Text = Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime") + " " + _currentTimeMachine.Circuits.DestinationTime.ToString();
                }
                else
                {
                    TimeCircuitsOnDescription.Checked = false;
                    FuelChamberDescription.Checked = false;
                    DestinationTimeDescription.Text = Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime");
                }
            }
        }
    }
}
