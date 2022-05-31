using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class MenuHandler
    {
        public static ControlsMenu ControlsMenu { get; } = new ControlsMenu();
        public static SoundsSettingsMenu SoundsSettingsMenu { get; } = new SoundsSettingsMenu();
        public static EventsSettingsMenu EventsSettingsMenu { get; } = new EventsSettingsMenu();
        public static TCDMenu TCDMenu { get; } = new TCDMenu();
        public static SettingsMenu SettingsMenu { get; } = new SettingsMenu();
        public static RCMenu RCMenu { get; } = new RCMenu();
        public static OverrideMenu OverrideMenu { get; } = new OverrideMenu();
        public static PhotoMenu PhotoMenu { get; } = new PhotoMenu();
        public static CustomMenu CustomMenuMain { get; } = new CustomMenu() { ForceNew = true };
        public static CustomMenu CustomMenuPresets { get; } = new CustomMenu() { ForceNew = true };
        public static CustomMenu2 CustomMenuGarage { get; } = new CustomMenu2();
        public static GarageMenu GarageMenu { get; } = new GarageMenu();
        public static PresetsMenu PresetsMenu { get; } = new PresetsMenu();
        public static OutatimeMenu OutatimeMenu { get; } = new OutatimeMenu();
        public static MainMenu MainMenu { get; } = new MainMenu();
        public static TimeMachineMenu TimeMachineMenu { get; } = new TimeMachineMenu();

        public static bool UnlockPhotoMenu { get; private set; }
        public static bool UnlockSpawnMenu { get; private set; }

        public static void Tick()
        {
            if (!TcdEditer.IsEditing && !RCGUIEditer.IsEditing && GarageHandler.Status == GarageStatus.Idle)
            {
                if ((ModControls.CombinationsForInteractionMenu && Game.IsEnabledControlPressed(ModControls.InteractionMenu1) && Game.IsControlPressed(ModControls.InteractionMenu2)) || (!ModControls.CombinationsForInteractionMenu && Game.IsControlPressed(ModControls.InteractionMenu1)))
                {
                    if (TimeMachineHandler.CurrentTimeMachine != null)
                    {
                        if (TimeMachineHandler.CurrentTimeMachine.Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole)
                        {
                            return;
                        }
                    }

                    if (FusionUtils.PlayerPed.IsGoingIntoCover)
                    {
                        FusionUtils.PlayerPed.Task.StandStill(1);
                    }

                    if (RemoteTimeMachineHandler.IsRemoteOn)
                    {
                        TimeMachineMenu.Visible = true;

                        return;
                    }
                    else if (CustomNativeMenu.ObjectPool.AreAnyVisible)
                    {
                        return;
                    }

                    if (TimeMachineHandler.CurrentTimeMachine != null)
                    {
                        TimeMachineMenu.Visible = true;
                    }
                    else
                    {
                        MainMenu.Visible = true;
                    }
                }
            }
        }

        public static void KeyDown(KeyEventArgs e)
        {
            if (TcdEditer.IsEditing || RCGUIEditer.IsEditing || GarageHandler.Status != GarageStatus.Idle)
            {
                return;
            }

            if ((ModControls.UseControlForMainMenu && e.Control && e.KeyCode == ModControls.MainMenu) || (!ModControls.UseControlForMainMenu && !e.Control && e.KeyCode == ModControls.MainMenu))
            {
                if (TimeMachineHandler.CurrentTimeMachine != null)
                {
                    if (TimeMachineHandler.CurrentTimeMachine.Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole)
                    {
                        return;
                    }
                }

                CustomNativeMenu.ObjectPool.HideAll();

                MainMenu.Visible = true;
            }

            if (e.Alt && e.KeyCode == Keys.D1)
            {
                string hash = Game.GetUserInput(WindowTitle.EnterMessage20, "", 20).ToLower().GetSHA256Hash();

                switch (hash)
                {
                    case "c3cca7029c38959a99b7aa57c37f0b05b663fd624a8f7dbc6424e44320b84206":
                        UnlockSpawnMenu = !UnlockSpawnMenu;
                        break;
                    case "fbff03e5367d548c10cb18965f950df472a8dc408d003f557ce974ddc2658ade":
                        UnlockPhotoMenu = !UnlockPhotoMenu;
                        break;
                }
            }
        }
    }
}