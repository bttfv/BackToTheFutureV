using BackToTheFutureV.Settings;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.RC;
using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Control = GTA.Control;

namespace BackToTheFutureV.Menu
{
    public class MenuHandler
    {
        public static ControlsMenu ControlsMenu { get; } = new ControlsMenu();
        public static SoundsSettingsMenu SoundsSettingsMenu { get; } = new SoundsSettingsMenu();
        public static EventsSettingsMenu EventsSettingsMenu { get; } = new EventsSettingsMenu();
        public static TCDMenu TCDMenu { get; } = new TCDMenu();
        public static SettingsMenu SettingsMenu { get; } = new SettingsMenu();
        public static RCMenu RCMenu { get; } = new RCMenu();        
        public static PhotoMenu PhotoMenu { get; } = new PhotoMenu();
        public static CustomMenu CustomMenu { get; } = new CustomMenu();
        public static CustomMenu CustomMenuForced { get; } = new CustomMenu() { ForceNew = true };
        public static PresetsMenu PresetsMenu { get; } = new PresetsMenu();
        public static OutatimeMenu OutatimeMenu { get; } = new OutatimeMenu();
        public static MainMenu MainMenu { get; } = new MainMenu();
        public static TimeMachineMenu TimeMachineMenu { get; } = new TimeMachineMenu();        

        public static void Process()
        {
            if (!TcdEditer.IsEditing)
            {
                if ((ModControls.CombinationsForInteractionMenu && Game.IsEnabledControlPressed(ModControls.InteractionMenu1) && Game.IsControlPressed(ModControls.InteractionMenu2)) || (!ModControls.CombinationsForInteractionMenu && Game.IsControlPressed(ModControls.InteractionMenu1)))
                {
                    if (TimeMachineHandler.CurrentTimeMachine != null)
                    {
                        if (TimeMachineHandler.CurrentTimeMachine.Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole)
                            return;
                    }

                    if (Main.PlayerPed.IsGoingIntoCover)
                        Main.PlayerPed.Task.StandStill(1);

                    if (RCManager.IsRemoteOn)
                    {
                        TimeMachineMenu.Open();
                        return;
                    }
                    else if (Main.ObjectPool.AreAnyVisible)
                        return;

                    if (TimeMachineHandler.CurrentTimeMachine != null)
                        TimeMachineMenu.Open();
                    else
                        MainMenu.Open();
                }
            }

            CustomNativeMenu.ProcessAll();
        }

        public static void KeyDown(KeyEventArgs e)
        {
            if (TcdEditer.IsEditing)
                return;

            if ((ModControls.UseControlForMainMenu && e.Control && e.KeyCode == ModControls.MainMenu) || (!ModControls.UseControlForMainMenu && !e.Control && e.KeyCode == ModControls.MainMenu))
            {
                if (TimeMachineHandler.CurrentTimeMachine != null)
                    if (TimeMachineHandler.CurrentTimeMachine.Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole)
                        return;

                Main.ObjectPool.HideAll();

                MainMenu.Open();
            }
        }
    }
}
