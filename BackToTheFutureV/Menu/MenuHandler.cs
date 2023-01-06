using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal static class MenuHandler
    {
        public static ControlsMenu ControlsMenu { get; } = new ControlsMenu();
        public static SoundsSettingsMenu SoundsSettingsMenu { get; } = new SoundsSettingsMenu();
        public static EventsSettingsMenu EventsSettingsMenu { get; } = new EventsSettingsMenu();
        public static TCDMenu TCDMenu { get; } = new TCDMenu();
        public static SettingsMenu SettingsMenu { get; } = new SettingsMenu();
        public static RCMenu RCMenu { get; } = new RCMenu();
        public static OverrideMenu OverrideMenu { get; } = new OverrideMenu();
        public static PhotoMenu PhotoMenu { get; } = new PhotoMenu();
        public static DoorsMenu DoorsMenu { get; } = new DoorsMenu();
        public static CustomMenu CustomMenu { get; } = new CustomMenu();
        public static GarageMenu GarageMenu { get; } = new GarageMenu();
        public static OutatimeMenu OutatimeMenu { get; } = new OutatimeMenu();
        public static MainMenu MainMenu { get; } = new MainMenu();
        public static TimeMachineMenu TimeMachineMenu { get; } = new TimeMachineMenu();
        public static int closingTime;

        public static bool IsAnyMenuOpen()
        {
            if (ControlsMenu.Visible || SoundsSettingsMenu.Visible || EventsSettingsMenu.Visible || TCDMenu.Visible || SettingsMenu.Visible || RCMenu.Visible || OverrideMenu.Visible || PhotoMenu.Visible || DoorsMenu.Visible || CustomMenu.Visible || GarageMenu.Visible || OutatimeMenu.Visible || MainMenu.Visible || TimeMachineMenu.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

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

                if ((MainMenu.Visible || TimeMachineMenu.Visible || GarageMenu.Visible || PhotoMenu.Visible) && FusionUtils.PlayerVehicle.NotNullAndExists() && (Game.IsControlJustPressed(GTA.Control.VehicleCinCam) || Game.IsControlJustPressed(GTA.Control.VehicleDuck)))
                {
                    closingTime = Game.GameTime + 256;
                }
            }

            if (Game.GameTime < closingTime)
            {
                Game.DisableControlThisFrame(GTA.Control.VehicleCinCam);
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
        }
    }
}
