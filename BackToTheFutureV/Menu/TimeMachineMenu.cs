using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.RC;
using FusionLibrary;
using GTA;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.Drawing;

namespace BackToTheFutureV.Menu
{
    public class TimeMachineMenu : CustomNativeMenu
    {
        public NativeItem Repair { get; private set; }
        public NativeCheckboxItem TimeCircuitsOn { get; }
        public NativeCheckboxItem CutsceneMode { get; }
        public NativeCheckboxItem FlyMode { get; }
        public NativeCheckboxItem AltitudeHold { get; }
        public NativeCheckboxItem RemoteControl { get; }
        //public NativeCheckboxItem EscapeMission { get; }

        public NativeSubmenuItem CustomMenu { get; }
        public NativeSubmenuItem PhotoMenu { get; }
        public NativeSubmenuItem TrainMissionMenu { get; }
        public NativeSubmenuItem BackToMain { get; }

        public TimeMachineMenu() : base("", Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu"))
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

            Shown += TimeMachineMenu_Shown;
            OnItemCheckboxChanged += TimeMachineMenu_OnItemCheckboxChanged;
            OnItemActivated += TimeMachineMenu_OnItemActivated;

            Add(TimeCircuitsOn = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_TimeCircuitsOn"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_TimeCircuitsOn_Description")));
            Add(CutsceneMode = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_CutsceneMode"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_CutsceneMode")));
            Add(FlyMode = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_HoverMode"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_HoverMode_Description")));
            Add(AltitudeHold = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_AltitudeControl"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_AltitudeControl_Description")));
            Add(RemoteControl = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_RemoteControl"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_RemoteControl_Description")));
            //Add(EscapeMission = new NativeCheckboxItem("Escape Mission"));

            CustomMenu = AddSubMenu(MenuHandler.CustomMenu);
            CustomMenu.Title = Game.GetLocalizedString("BTTFV_Input_SpawnMenu");

            PhotoMenu = AddSubMenu(MenuHandler.PhotoMenu);
            PhotoMenu.Title = Game.GetLocalizedString("BTTFV_Menu_PhotoMenu");
            PhotoMenu.Description = Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Description");

            TrainMissionMenu = AddSubMenu(MenuHandler.TrainMissionMenu);
            TrainMissionMenu.Title = "Train Mission";

            BackToMain = AddSubMenu(MenuHandler.MainMenu);
            BackToMain.Title = Game.GetLocalizedString("BTTFV_Menu_GoBackToMainMenu");
            BackToMain.Description = Game.GetLocalizedString("BTTFV_Menu_GoBackToMainMenu_Description");
        }

        private void TimeMachineMenu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == Repair)
            {
                TimeMachineHandler.CurrentTimeMachine.Repair();
                Close();
            }
        }

        private void TimeMachineMenu_Shown(object sender, EventArgs e)
        {
            if (TimeMachineHandler.CurrentTimeMachine == null)
            {
                Close();
                return;
            }

            FlyMode.Enabled = TimeMachineHandler.CurrentTimeMachine.Mods.HoverUnderbody == Vehicles.ModState.On;
            AltitudeHold.Enabled = FlyMode.Enabled;

            RemoteControl.Enabled = TimeMachineHandler.CurrentTimeMachine.Properties.IsRemoteControlled;

            TimeCircuitsOn.Enabled = !RemoteControl.Enabled;
            CutsceneMode.Enabled = !RemoteControl.Enabled;

            bool fullDamaged = TimeMachineHandler.CurrentTimeMachine.Properties.FullDamaged;

            if (Repair != null && !fullDamaged)
            {
                Remove(Repair);
                Repair = null;
            }
            else if (Repair == null && fullDamaged)
                Add(0, Repair = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_Restore"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_Restore_Description")));
        }

        private void TimeMachineMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == TimeCircuitsOn)
                TimeMachineHandler.CurrentTimeMachine.Events.SetTimeCircuits?.Invoke(Checked);
            else if (sender == CutsceneMode)
                TimeMachineHandler.CurrentTimeMachine.Events.SetCutsceneMode?.Invoke(Checked);
            else if (sender == RemoteControl && !Checked && TimeMachineHandler.CurrentTimeMachine.Properties.IsRemoteControlled)
                RCManager.StopRemoteControl();
            else if (sender == FlyMode)
                TimeMachineHandler.CurrentTimeMachine.Events.SetFlyMode?.Invoke(Checked);
            else if (sender == AltitudeHold)
                TimeMachineHandler.CurrentTimeMachine.Events.SetAltitudeHold?.Invoke(Checked);
            //else if (sender == EscapeMission)
            //{
            //    if (Checked)
            //        MissionHandler.Escape.Start();
            //    else
            //        MissionHandler.Escape.End();

            //    Close();
            //}
        }

        public override void Tick()
        {
            if (TimeMachineHandler.CurrentTimeMachine == null)
            {
                Close();
                return;
            }

            TimeCircuitsOn.Checked = TimeMachineHandler.CurrentTimeMachine.Properties.AreTimeCircuitsOn;
            CutsceneMode.Checked = TimeMachineHandler.CurrentTimeMachine.Properties.CutsceneMode;
            FlyMode.Checked = TimeMachineHandler.CurrentTimeMachine.Properties.IsFlying;
            AltitudeHold.Checked = TimeMachineHandler.CurrentTimeMachine.Properties.IsAltitudeHolding;
            RemoteControl.Checked = TimeMachineHandler.CurrentTimeMachine.Properties.IsRemoteControlled;

            CustomMenu.Enabled = !TimeMachineHandler.CurrentTimeMachine.Properties.FullDamaged;
            PhotoMenu.Enabled = CustomMenu.Enabled;

            TrainMissionMenu.Enabled = TimeMachineHandler.CurrentTimeMachine.Properties.IsOnTracks;

            //EscapeMission.Enabled = !TimeMachineHandler.CurrentTimeMachine.Properties.IsFlying;
            //EscapeMission.Checked = MissionHandler.Escape.IsPlaying;
        }
    }
}
