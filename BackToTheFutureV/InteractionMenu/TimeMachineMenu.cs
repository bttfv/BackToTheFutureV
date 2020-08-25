using BackToTheFutureV.Delorean;
using BackToTheFutureV.Delorean.Handlers;
using BackToTheFutureV.Utility;
using GTA;
using NativeUI;

namespace BackToTheFutureV.InteractionMenu
{
    public class TimeMachineMenu : UIMenu
    {
        public UIMenuItem BackToMainMenu { get; }
        public UIMenuItem SpawnMenu { get; }

        public UIMenuCheckboxItem TimeCircuitsOn { get; }
        public UIMenuCheckboxItem CutsceneMode { get; }
        public UIMenuCheckboxItem FlyMode { get; }
        public UIMenuCheckboxItem HoverMode { get; }
        public UIMenuCheckboxItem RemoteControl { get; }

        public TimeMachineMenu() : base(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu"), Game.GetLocalizedString("BTTFV_Menu_Description"))
        {
            AddItem(TimeCircuitsOn = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_TimeCircuitsOn"), false, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_TimeCircuitsOn_Description")));
            AddItem(CutsceneMode = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_CutsceneMode"), true, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_CutsceneMode")));
            AddItem(FlyMode = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_HoverMode"), false, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_HoverMode_Description")));
            AddItem(HoverMode = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_AltitudeControl"), false, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_AltitudeControl_Description")));
            AddItem(RemoteControl = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_RemoteControl"), false, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_RemoteControl_Description")));

            SpawnMenu = Utils.AttachSubmenu(this, InteractionMenuManager.SpawnMenuContext, Game.GetLocalizedString("BTTFV_Input_SpawnMenu"), "");

            Utils.AttachSubmenu(this, InteractionMenuManager.PhotoMenu, Game.GetLocalizedString("BTTFV_Menu_PhotoMenu"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Description"));

            AddItem(BackToMainMenu = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_GoBackToMainMenu"), Game.GetLocalizedString("BTTFV_Menu_GoBackToMainMenu_Description")));

            OnCheckboxChange += TimeMachineMenu_OnCheckboxChange;
            OnItemSelect += TimeMachineMenu_OnItemSelect;
            OnMenuOpen += TimeMachineMenu_OnMenuOpen;
        }

        private void TimeMachineMenu_OnMenuOpen(UIMenu sender)
        {
            SpawnMenu.Enabled = !Main.DisablePlayerSwitching;
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
            if (DeloreanHandler.CurrentTimeMachine == null)
                return;

            TimeTravelHandler timeTravelHandler = DeloreanHandler.CurrentTimeMachine.Circuits.GetHandler<TimeTravelHandler>();

            if (checkboxItem == TimeCircuitsOn)
                DeloreanHandler.CurrentTimeMachine.Circuits.SetTimeCircuitsOn(Checked);
            else if (checkboxItem == CutsceneMode)
                timeTravelHandler.SetCutsceneMode(Checked);
            else if (checkboxItem == RemoteControl)
                RCManager.StopRemoteControl();

            if (DeloreanHandler.CurrentTimeMachine.DeloreanType != DeloreanType.BTTF2)
                return;

            FlyingHandler flyingHandler = DeloreanHandler.CurrentTimeMachine.Circuits.GetHandler<FlyingHandler>();

            if (checkboxItem == FlyMode)
                flyingHandler.SetFlyMode(Checked);
            else if(checkboxItem == HoverMode)
                flyingHandler.SetHoverMode(Checked);
        }

        public void Open()
        {
            Visible = true;
        }

        public void Close()
        {
            Visible = false;
        }

        public void Process()
        {
            if (DeloreanHandler.CurrentTimeMachine == null)
                return;

            if(DeloreanHandler.CurrentTimeMachine.Mods.HoverUnderbody == ModState.On)
            {
                var flyingHandler = DeloreanHandler.CurrentTimeMachine.Circuits.GetHandler<FlyingHandler>();

                FlyMode.Enabled = true;
                FlyMode.Checked = flyingHandler.IsFlying;

                HoverMode.Enabled = true;
                HoverMode.Checked = flyingHandler.IsAltitudeHolding;
            }
            else
            {
                FlyMode.Enabled = false;
                FlyMode.Checked = false;

                HoverMode.Checked = false;
                HoverMode.Enabled = false;
            }

            if (DeloreanHandler.CurrentTimeMachine.Circuits.GetHandler<TCDHandler>().IsDoingTimedVisible)
            {
                TimeCircuitsOn.Enabled = false;
            }
            else
            {
                TimeCircuitsOn.Enabled = !DeloreanHandler.CurrentTimeMachine.Circuits.IsRemoteControlled;
                TimeCircuitsOn.Checked = DeloreanHandler.CurrentTimeMachine.Circuits.IsOn;
            }

            CutsceneMode.Enabled = !DeloreanHandler.CurrentTimeMachine.Circuits.IsRemoteControlled;
            CutsceneMode.Checked = DeloreanHandler.CurrentTimeMachine.Circuits.GetHandler<TimeTravelHandler>().CutsceneMode;

            RemoteControl.Checked = RCManager.RemoteControlling == DeloreanHandler.CurrentTimeMachine;
            RemoteControl.Enabled = RCManager.RemoteControlling == DeloreanHandler.CurrentTimeMachine;
        }
    }
}
