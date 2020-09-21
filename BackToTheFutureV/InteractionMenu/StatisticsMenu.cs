using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA;
using GTA.Math;
using GTA.Native;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackToTheFutureV.InteractionMenu
{
    public class StatisticsMenu : UIMenu
    {
        public UIMenuDynamicListItem TimeMachines { get; }

        public UIMenuListItem TypeDescription { get; }
        public UIMenuItem DestinationTimeDescription { get; }
        public UIMenuItem LastTimeDescription { get; }
        public UIMenuCheckboxItem Spawned { get; }
        public UIMenuCheckboxItem ShowBlip { get; }
        public UIMenuItem ForceReenter { get; }

        private readonly List<object> _listTypes = new List<object> { "BTTF1", "BTTF2", "BTTF3" };

        public StatisticsMenu() : base(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu"), Game.GetLocalizedString("BTTFV_Menu_Description"))
        {
            AddItem(TimeMachines = new UIMenuDynamicListItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Deloreans"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Deloreans_Description"), "0", ChangeCallback));
            AddItem(TypeDescription = new UIMenuListItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Type"), _listTypes, 0, Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Type_Description")));
            AddItem(DestinationTimeDescription = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime"), Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime_Description")));
            AddItem(LastTimeDescription = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_LastTime"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_LastTime_Description")));
            AddItem(Spawned = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Spawned"), false, Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Spawned_Description")));
            AddItem(ShowBlip = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_ShowBlip"), false, Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_ShowBlip_Description")));
            AddItem(ForceReenter = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_ForceReenter"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_ForceReenter_Description")));

            TypeDescription.Enabled = false;            
            DestinationTimeDescription.Enabled = false;
            LastTimeDescription.Enabled = false;
            Spawned.Enabled = false;

            OnItemSelect += StatisticsMenu_OnItemSelect;
            OnListChange += StatisticsMenu_OnListChange;
            OnMenuClose += StatisticsMenu_OnMenuClose;
            OnCheckboxChange += StatisticsMenu_OnCheckboxChange;
        }

        private void StatisticsMenu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (checkboxItem == ShowBlip)
            {
                if (Checked)
                {
                    Vector3 pos = _currentTimeMachine.TimeMachineClone.Vehicle.Position;

                    _currentTimeMachine.Blip = World.CreateBlip(pos);
                    _currentTimeMachine.Blip.Sprite = BlipSprite.SlowTime;

                    switch (_currentTimeMachine.TimeMachineClone.Mods.WormholeType)
                    {
                        case WormholeType.BTTF1:
                            _currentTimeMachine.Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF1")}";
                            _currentTimeMachine.Blip.Color = BlipColor.NetPlayer22;
                            break;

                        case WormholeType.BTTF2:
                            _currentTimeMachine.Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF2")}";
                            _currentTimeMachine.Blip.Color = BlipColor.NetPlayer21;
                            break;

                        case WormholeType.BTTF3:
                            if (_currentTimeMachine.TimeMachineClone.Mods.Wheel == WheelType.RailroadInvisible)
                            {
                                _currentTimeMachine.Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF3RR")}";
                                _currentTimeMachine.Blip.Color = BlipColor.Orange;
                            }
                            else
                            {
                                _currentTimeMachine.Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF3")}";
                                _currentTimeMachine.Blip.Color = BlipColor.Red;
                            }
                            break;
                    }
                }
                else
                    _currentTimeMachine.Blip.Delete();
            }
        }

        private int _selectedIndex;

        private RemoteTimeMachine _currentTimeMachine => RemoteTimeMachineHandler.GetTimeMachineFromIndex(_selectedIndex);

        private void StatisticsMenu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            _selectedIndex = newIndex;
        }

        private string ChangeCallback(UIMenuDynamicListItem sender, UIMenuDynamicListItem.ChangeDirection direction)
        {
            int index = _selectedIndex;

            if(direction == UIMenuDynamicListItem.ChangeDirection.Right)
            {
                if(RemoteTimeMachineHandler.TimeMachineCount > 0)
                {
                    index++;

                    if (index > RemoteTimeMachineHandler.TimeMachineCount - 1)
                        index = 0;
                }
            }
            else
            {
                if(RemoteTimeMachineHandler.TimeMachineCount > 0)
                {
                    index--;

                    if (index < 0)
                        index = RemoteTimeMachineHandler.TimeMachineCount - 1;
                }
            }

            _selectedIndex = index;

            return _selectedIndex.ToString();
        }

        private void StatisticsMenu_OnMenuClose(UIMenu sender)
        {
            
        }

        private void StatisticsMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == ForceReenter && !Spawned.Checked)
            {
                _currentTimeMachine.Spawn(ReenterType.Forced);

                Spawned.Checked = _currentTimeMachine.Spawned;
            }                
        }

        public void Process()
        {
            if (_selectedIndex != 0 && _selectedIndex > RemoteTimeMachineHandler.TimeMachineCount - 1)
                _selectedIndex = RemoteTimeMachineHandler.TimeMachineCount - 1;

            TimeMachines.Enabled = RemoteTimeMachineHandler.TimeMachineCount != 0;
            ForceReenter.Enabled = TimeMachines.Enabled;
            ShowBlip.Enabled = TimeMachines.Enabled;

            if (_currentTimeMachine != null)
            {
                TypeDescription.Index = (int)_currentTimeMachine.TimeMachineClone.Mods.WormholeType - 1;
                DestinationTimeDescription.Text = Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime") + " " + _currentTimeMachine.TimeMachineClone.Properties.DestinationTime.ToString();
                LastTimeDescription.Text = Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_LastTime") + " " + _currentTimeMachine.TimeMachineClone.Properties.PreviousTime.ToString();

                Spawned.Checked = _currentTimeMachine.Spawned;

                ShowBlip.Checked = _currentTimeMachine.Blip != null && _currentTimeMachine.Blip.Exists();
            }
            else
            {
                TypeDescription.Index = 0;
                DestinationTimeDescription.Text = Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime");
                LastTimeDescription.Text = Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_LastTime");

                Spawned.Checked = false;

                ShowBlip.Checked = false;
            }
        }
    }
}
