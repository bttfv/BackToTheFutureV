using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.Handlers;
using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA;
using NativeUI;

namespace BackToTheFutureV.InteractionMenu
{
    public class TimeMachineMenu : UIMenu
    {
        public UIMenuItem BackToMainMenu { get; }
        public UIMenuItem SpawnMenu { get; }
        public UIMenuItem PhotoMenu { get; }

        public UIMenuCheckboxItem TimeCircuitsOn { get; }
        public UIMenuCheckboxItem CutsceneMode { get; }
        public UIMenuCheckboxItem FlyMode { get; }
        public UIMenuCheckboxItem AltitudeHold { get; }
        public UIMenuCheckboxItem RemoteControl { get; }

        public TimeMachineMenu() : base(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu"), Game.GetLocalizedString("BTTFV_Menu_Description"))
        {
            AddItem(TimeCircuitsOn = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_TimeCircuitsOn"), false, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_TimeCircuitsOn_Description")));
            AddItem(CutsceneMode = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_CutsceneMode"), true, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_CutsceneMode")));
            AddItem(FlyMode = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_HoverMode"), false, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_HoverMode_Description")));
            AddItem(AltitudeHold = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_AltitudeControl"), false, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_AltitudeControl_Description")));
            AddItem(RemoteControl = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_RemoteControl"), false, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_RemoteControl_Description")));

            SpawnMenu = Utils.AttachSubmenu(this, InteractionMenuManager.SpawnMenuContext, Game.GetLocalizedString("BTTFV_Input_SpawnMenu"), "");

            PhotoMenu = Utils.AttachSubmenu(this, InteractionMenuManager.PhotoMenu, Game.GetLocalizedString("BTTFV_Menu_PhotoMenu"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Description"));

            AddItem(BackToMainMenu = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_GoBackToMainMenu"), Game.GetLocalizedString("BTTFV_Menu_GoBackToMainMenu_Description")));

            OnCheckboxChange += TimeMachineMenu_OnCheckboxChange;
            OnItemSelect += TimeMachineMenu_OnItemSelect;
            OnMenuOpen += TimeMachineMenu_OnMenuOpen;
        }

        private void TimeMachineMenu_OnMenuOpen(UIMenu sender)
        {
            SpawnMenu.Enabled = !TimeMachineHandler.CurrentTimeMachine.Properties.IsRemoteControlled;
            PhotoMenu.Enabled = TimeMachineHandler.CurrentTimeMachine.Mods.IsDMC12;
        }

        private void TimeMachineMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == BackToMainMenu)
            {
                Main.MenuPool.CloseAllMenus();
                ModMenuHandler.MainMenu.Visible = true;
            }
        }

        private void TimeMachineMenu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (TimeMachineHandler.CurrentTimeMachine == null)
                return;
            
            if (checkboxItem == TimeCircuitsOn)
                TimeMachineHandler.CurrentTimeMachine.Events.SetTimeCircuits?.Invoke(Checked);
            else if (checkboxItem == CutsceneMode)
                TimeMachineHandler.CurrentTimeMachine.Events.SetCutsceneMode?.Invoke(Checked);
            else if (checkboxItem == RemoteControl)
                RCManager.StopRemoteControl();
            else if (checkboxItem == FlyMode)
                TimeMachineHandler.CurrentTimeMachine.Events.SetFlyMode?.Invoke(Checked);
            else if(checkboxItem == AltitudeHold)
                TimeMachineHandler.CurrentTimeMachine.Events.SetAltitudeHold?.Invoke(Checked);
        }

        public void Process()
        {
            if (TimeMachineHandler.CurrentTimeMachine == null)
                return;

            if(TimeMachineHandler.CurrentTimeMachine.Mods.HoverUnderbody == ModState.On)
            {
                FlyMode.Enabled = true;
                FlyMode.Checked = TimeMachineHandler.CurrentTimeMachine.Properties.IsFlying;

                AltitudeHold.Enabled = true;
                AltitudeHold.Checked = TimeMachineHandler.CurrentTimeMachine.Properties.IsAltitudeHolding;
            }
            else
            {
                FlyMode.Enabled = false;
                FlyMode.Checked = false;

                AltitudeHold.Checked = false;
                AltitudeHold.Enabled = false;
            }

            TimeCircuitsOn.Enabled = !TimeMachineHandler.CurrentTimeMachine.Properties.IsRemoteControlled;
            TimeCircuitsOn.Checked = TimeMachineHandler.CurrentTimeMachine.Properties.AreTimeCircuitsOn;

            CutsceneMode.Enabled = !TimeMachineHandler.CurrentTimeMachine.Properties.IsRemoteControlled;
            CutsceneMode.Checked = TimeMachineHandler.CurrentTimeMachine.Properties.CutsceneMode;

            RemoteControl.Checked = RCManager.RemoteControlling == TimeMachineHandler.CurrentTimeMachine;
            RemoteControl.Enabled = RCManager.RemoteControlling == TimeMachineHandler.CurrentTimeMachine;
        }
    }
}
