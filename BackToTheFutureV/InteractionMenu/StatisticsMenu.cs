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
using System.Windows.Forms;

namespace BackToTheFutureV.InteractionMenu
{
    public class StatisticsMenu : UIMenu
    {
        public UIMenuDynamicListItem Deloreans { get; }

        public UIMenuListItem TypeDescription { get; }
        public UIMenuItem DestinationTimeDescription { get; }
        public UIMenuItem LastTimeDescription { get; }
        public UIMenuCheckboxItem ShowBlip { get; }
        public UIMenuItem ForceReenter { get; }

        private readonly List<object> _listTypes = new List<object> { "BTTF1", "BTTF2", "BTTF3" };

        public StatisticsMenu() : base(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Description"))
        {
            AddItem(Deloreans = new UIMenuDynamicListItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Deloreans"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Deloreans_Description"), "0", ChangeCallback));
            AddItem(TypeDescription = new UIMenuListItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Type"), _listTypes, 0, Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Type_Description")));
            AddItem(DestinationTimeDescription = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime"), Game.GetLocalizedString("BTTFV_Menu_RCMenu_DestinationTime_Description")));
            AddItem(LastTimeDescription = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_LastTime"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_LastTime_Description")));
            AddItem(ShowBlip = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_ShowBlip"), false, Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_ShowBlip_Description")));
            AddItem(ForceReenter = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_ForceReenter"), Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_ForceReenter_Description")));

            TypeDescription.Enabled = false;
            DestinationTimeDescription.Enabled = false;
            LastTimeDescription.Enabled = false;

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
                    Vector3 pos = _currentTimeMachine.DeloreanCopy.VehicleInfo.Position;

                    _currentTimeMachine.Blip = World.CreateBlip(pos);
                    _currentTimeMachine.Blip.Sprite = BlipSprite.SlowTime;

                    switch (_currentTimeMachine.DeloreanCopy.Mods.DeloreanType)
                    {
                        case DeloreanType.BTTF1:
                            _currentTimeMachine.Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF1")}";
                            _currentTimeMachine.Blip.Color = BlipColor.NetPlayer22;
                            break;

                        case DeloreanType.BTTF2:
                            _currentTimeMachine.Blip.Name = $"{Game.GetLocalizedString("BTTFV_Menu_BTTF2")}";
                            _currentTimeMachine.Blip.Color = BlipColor.NetPlayer21;
                            break;

                        case DeloreanType.BTTF3:
                            if (_currentTimeMachine.DeloreanCopy.Mods.Wheel == WheelType.RailroadInvisible)
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

        private RemoteDelorean _currentTimeMachine => RemoteDeloreansHandler.GetTimeMachineFromIndex(_selectedIndex);

        private void StatisticsMenu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            _selectedIndex = newIndex;
        }

        private string ChangeCallback(UIMenuDynamicListItem sender, UIMenuDynamicListItem.ChangeDirection direction)
        {
            int index = _selectedIndex;

            if(direction == UIMenuDynamicListItem.ChangeDirection.Right)
            {
                if(RemoteDeloreansHandler.TimeMachineCount > 0)
                {
                    index++;

                    if (index > RemoteDeloreansHandler.TimeMachineCount - 1)
                        index = 0;
                }
            }
            else
            {
                if(RemoteDeloreansHandler.TimeMachineCount > 0)
                {
                    index--;

                    if (index < 0)
                        index = RemoteDeloreansHandler.TimeMachineCount - 1;
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
            if (selectedItem == ForceReenter)
                _currentTimeMachine.DeloreanCopy.Circuits.DestinationTime = Main.CurrentTime;
        }

        public void Process()
        {
            if (_selectedIndex != 0 && _selectedIndex > RemoteDeloreansHandler.TimeMachineCount - 1)
                _selectedIndex = RemoteDeloreansHandler.TimeMachineCount - 1;

            Deloreans.Enabled = RemoteDeloreansHandler.TimeMachineCount != 0;
            ForceReenter.Enabled = Deloreans.Enabled;
            ShowBlip.Enabled = Deloreans.Enabled;

            if (_currentTimeMachine != null)
            {
                TypeDescription.Index = (int)_currentTimeMachine.DeloreanCopy.Mods.DeloreanType - 1;
                DestinationTimeDescription.Text = Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_DestinationTime") + " " + _currentTimeMachine.DeloreanCopy.Circuits.DestinationTime.ToString();
                LastTimeDescription.Text = Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_LastTime") + " " + _currentTimeMachine.DeloreanCopy.Circuits.PreviousTime.ToString();

                ShowBlip.Checked = _currentTimeMachine.Blip != null && _currentTimeMachine.Blip.Exists();
            }
            else
            {
                TypeDescription.Index = 0;
                DestinationTimeDescription.Text = Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_DestinationTime");
                LastTimeDescription.Text = Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_LastTime");

                ShowBlip.Checked = false;
            }
        }
    }
}
