﻿using FusionLibrary;
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
        public static PhotoMenu PhotoMenu { get; } = new PhotoMenu();
        public static CustomMenu CustomMenuMain { get; } = new CustomMenu() { ForceNew = true };
        public static CustomMenu CustomMenuPresets { get; } = new CustomMenu() { ForceNew = true };
        public static CustomMenu2 CustomMenuGarage { get; } = new CustomMenu2();
        //public static CustomMenu CustomMenu { get; } = new CustomMenu();
        public static GarageMenu GarageMenu { get; } = new GarageMenu();
        public static PresetsMenu PresetsMenu { get; } = new PresetsMenu();
        public static OutatimeMenu OutatimeMenu { get; } = new OutatimeMenu();
        public static MainMenu MainMenu { get; } = new MainMenu();
        public static TimeMachineMenu TimeMachineMenu { get; } = new TimeMachineMenu();

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
        }
    }
}
