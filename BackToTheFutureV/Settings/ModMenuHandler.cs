using System;
using System.Linq;
using System.Windows.Forms;
using BackToTheFutureV.GUI;
using BackToTheFutureV.InteractionMenu;
using BackToTheFutureV.Settings;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA;
using GTA.UI;
using NativeUI;

namespace BackToTheFutureV
{
    public class ModMenuHandler
    {
        public static UIMenu MainMenu { get; private set; }
        private static UIMenu settingsMenu;
        private static UIMenu tcdMenu;
        private static UIMenu eventsMenu;
        private static UIMenu soundsMenu;

        private static UIMenuItem rcMenu;
        private static UIMenuItem spawnDelorean1;
        private static UIMenuItem spawnDelorean2;
        private static UIMenuItem spawnDelorean3;
        private static UIMenuItem spawnDelorean;
        private static UIMenuItem spawnCustomDelorean;
        private static UIMenuItem spawnPresetDelorean;
        private static UIMenuItem convertToTimeMachine;
        private static UIMenuItem removeAllDeloreans;
        private static UIMenuItem removeOtherDeloreans;
        private static UIMenuItem removeDelorean;

        // Settings
        private static UIMenuCheckboxItem cinematicSpawn;
        private static UIMenuCheckboxItem playFluxCapacitorSound;
        private static UIMenuCheckboxItem playDiodeSound;
        private static UIMenuCheckboxItem playEngineSounds;
        private static UIMenuCheckboxItem useInputToggle;
        private static UIMenuCheckboxItem forceFlyMode;
        private static UIMenuCheckboxItem playSpeedoBeep;
        private static UIMenuListItem tcdBackground;
        private static UIMenuCheckboxItem GlowingWormholeEmitter;
        private static UIMenuCheckboxItem GlowingPlutoniumReactor;
        private static UIMenuCheckboxItem LightningStrikeEvent;
        private static UIMenuCheckboxItem EngineStallEvent;        
        private static UIMenuCheckboxItem TurbulenceEvent;
        private static UIMenuCheckboxItem LandingSystem;

        // TCD stuff
        private static UIMenuItem changeTCD;
        private static UIMenuItem resetToDefaultTCD;

        public static void Initialize()
        {
            MainMenu = new UIMenu("BackToTheFutureV", Game.GetLocalizedString("BTTFV_Menu_Description"));
            MainMenu.SetBannerType("./scripts/BackToTheFutureV/BTTFV.png");

            MainMenu.AddItem(spawnDelorean = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_Spawn") + " " + Game.GetLocalizedString("BTTFV_Menu_DMC12"), Game.GetLocalizedString("BTTFV_Menu_SpawnDMC12_Description")));
            MainMenu.AddItem(spawnDelorean1 = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_Spawn") + " " + Game.GetLocalizedString("BTTFV_Menu_BTTF1"), Game.GetLocalizedString("BTTFV_Menu_SpawnBTTF1_Description")));
            MainMenu.AddItem(spawnDelorean2 = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_Spawn") + " " + Game.GetLocalizedString("BTTFV_Menu_BTTF2"), Game.GetLocalizedString("BTTFV_Menu_SpawnBTTF2_Description")));
            MainMenu.AddItem(spawnDelorean3 = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_Spawn") + " " + Game.GetLocalizedString("BTTFV_Menu_BTTF3"), Game.GetLocalizedString("BTTFV_Menu_SpawnBTTF3_Description")));

            spawnPresetDelorean = Utils.AttachSubmenu(MainMenu, InteractionMenuManager.PresetsMenu, Game.GetLocalizedString("BTTFV_Menu_Spawn_Preset"), Game.GetLocalizedString("BTTFV_Menu_Spawn_Preset_Description"));
            spawnCustomDelorean = Utils.AttachSubmenu(MainMenu, InteractionMenuManager.SpawnMenu, Game.GetLocalizedString("BTTFV_Menu_Build_Delorean"), Game.GetLocalizedString("BTTFV_Menu_Build_Delorean_Description"));

            MainMenu.AddItem(convertToTimeMachine = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_ConvertToTimeMachine"), Game.GetLocalizedString("BTTFV_Menu_ConvertToTimeMachine_Description")));

            rcMenu = Utils.AttachSubmenu(MainMenu, InteractionMenuManager.RCMenu, Game.GetLocalizedString("BTTFV_Menu_RCMenu"), Game.GetLocalizedString("BTTFV_Menu_RCMenu_Description"));

            MainMenu.AddItem(removeDelorean = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_RemoveTimeMachine"), Game.GetLocalizedString("BTTFV_Menu_RemoveTimeMachine_Description")));
            MainMenu.AddItem(removeOtherDeloreans = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_RemoveOtherTimeMachines"), Game.GetLocalizedString("BTTFV_Menu_RemoveOtherTimeMachines_Description")));
            MainMenu.AddItem(removeAllDeloreans = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_RemoveAllTimeMachines"), Game.GetLocalizedString("BTTFV_Menu_RemoveAllTimeMachines_Description")));

            MainMenu.OnMenuOpen += MainMenu_OnMenuOpen;
            MainMenu.OnItemSelect += MainMenu_OnItemSelect;
            Main.MenuPool.Add(MainMenu);

            settingsMenu = Main.MenuPool.AddSubMenu(MainMenu, Game.GetLocalizedString("BTTFV_Menu_Settings"), Game.GetLocalizedString("BTTFV_Menu_Settings_Description"));
            settingsMenu.SetBannerType("./scripts/BackToTheFutureV/BTTFV.png");  

            settingsMenu.AddItem(cinematicSpawn = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_CinematicSpawn"), ModSettings.CinematicSpawn, Game.GetLocalizedString("BTTFV_Menu_CinematicSpawn_Description")));
            settingsMenu.AddItem(useInputToggle = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_InputToggle"), ModSettings.UseInputToggle, Game.GetLocalizedString("BTTFV_Menu_InputToggle_Description")));
            settingsMenu.AddItem(forceFlyMode = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_ForceFlyMode"), ModSettings.ForceFlyMode, Game.GetLocalizedString("BTTFV_Menu_ForceFlyMode_Description")));
            settingsMenu.AddItem(LandingSystem = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_LandingSystem"), ModSettings.LandingSystem, Game.GetLocalizedString("BTTFV_Menu_LandingSystem_Description")));
            settingsMenu.AddItem(GlowingWormholeEmitter = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_GlowingWormholeEmitter"), ModSettings.GlowingWormholeEmitter, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_GlowingWormholeEmitter_Description")));
            settingsMenu.AddItem(GlowingPlutoniumReactor = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_GlowingPlutoniumReactor"), ModSettings.GlowingPlutoniumReactor, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_GlowingPlutoniumReactor_Description")));

            settingsMenu.OnItemSelect += SettingsMenu_OnItemSelect;
            settingsMenu.OnCheckboxChange += SettingsMenu_OnCheckboxChange;

            soundsMenu = Main.MenuPool.AddSubMenu(settingsMenu, Game.GetLocalizedString("BTTFV_Menu_SoundsMenu"), Game.GetLocalizedString("BTTFV_Menu_SoundsMenu_Description"));
            soundsMenu.SetBannerType("./scripts/BackToTheFutureV/BTTFV.png");

            soundsMenu.AddItem(playFluxCapacitorSound = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_FluxCapacitorSound"), ModSettings.PlayFluxCapacitorSound, Game.GetLocalizedString("BTTFV_Menu_FluxCapacitorSound_Description")));
            soundsMenu.AddItem(playDiodeSound = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_CircuitsBeep"), ModSettings.PlayDiodeBeep, Game.GetLocalizedString("BTTFV_Menu_CircuitsBeep_Description")));
            soundsMenu.AddItem(playSpeedoBeep = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_SpeedoBeep"), ModSettings.PlaySpeedoBeep, Game.GetLocalizedString("BTTFV_Menu_SpeedoBeep_Description")));
            soundsMenu.AddItem(playEngineSounds = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_EngineSounds"), ModSettings.PlayEngineSounds, Game.GetLocalizedString("BTTFV_Menu_EngineSounds_Description")));

            soundsMenu.OnCheckboxChange += SettingsMenu_OnCheckboxChange;

            eventsMenu = Main.MenuPool.AddSubMenu(settingsMenu, Game.GetLocalizedString("BTTFV_Menu_EventsMenu"), Game.GetLocalizedString("BTTFV_Menu_EventsMenu_Description"));

            eventsMenu.SetBannerType("./scripts/BackToTheFutureV/BTTFV.png");
            eventsMenu.AddItem(LightningStrikeEvent = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_LightningStrikeEvent"), ModSettings.LightningStrikeEvent, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_LightningStrikeEvent_Description")));
            eventsMenu.AddItem(EngineStallEvent = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_EngineStallEvent"), ModSettings.EngineStallEvent, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_EngineStallEvent_Description")));
            eventsMenu.AddItem(TurbulenceEvent = new UIMenuCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_TurbolenceEvent"), ModSettings.TurbulenceEvent, Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_TurbolenceEvent_Description")));

            eventsMenu.OnCheckboxChange += SettingsMenu_OnCheckboxChange;
            
            tcdMenu = Main.MenuPool.AddSubMenu(settingsMenu, Game.GetLocalizedString("BTTFV_Menu_TCDMenu"), Game.GetLocalizedString("BTTFV_Menu_TCDMenu_Description"));
            tcdMenu.SetBannerType("./scripts/BackToTheFutureV/BTTFV.png");

            tcdMenu.AddItem(tcdBackground = new UIMenuListItem(Game.GetLocalizedString("BTTFV_Menu_TCDBackground"), Enum.GetValues(typeof(TCDBackground)).Cast<object>().ToList(), 0, Game.GetLocalizedString("BTTFV_Menu_TCDBackground_Description")));
            tcdMenu.AddItem(changeTCD = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_TCDEditMode"), Game.GetLocalizedString("BTTFV_Menu_TCDEditMode_Description")));
            tcdMenu.AddItem(resetToDefaultTCD = new UIMenuItem(Game.GetLocalizedString("BTTFV_Menu_TCDReset"), Game.GetLocalizedString("BTTFV_Menu_TCDReset_Description")));

            tcdBackground.OnListChanged += TcdBackground_OnListChanged;
            tcdMenu.OnItemSelect += TcdMenu_OnItemSelect;
        }

        private static void SettingsMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {

        }

        private static void MainMenu_OnMenuOpen(UIMenu sender)
        {
            if (TimeMachineClone.ListPresets().Count > 0)
            {
                MainMenu.ReleaseMenuFromItem(spawnPresetDelorean);
                MainMenu.BindMenuToItem(InteractionMenuManager.PresetsMenu, spawnPresetDelorean);
            }
            else
            {
                MainMenu.ReleaseMenuFromItem(spawnPresetDelorean);
                MainMenu.BindMenuToItem(InteractionMenuManager.SpawnMenu, spawnPresetDelorean);
            }

            rcMenu.Enabled = Main.PlayerVehicle == null;

            if (Main.PlayerVehicle != null)
            {
                convertToTimeMachine.Enabled = TimeMachineHandler.GetTimeMachineFromVehicle(Main.PlayerVehicle) == null;
            }
            else
                convertToTimeMachine.Enabled = false;
        }

        private static void TcdBackground_OnListChanged(UIMenuListItem sender, int newIndex)
        {
            TCDBackground item = (TCDBackground)tcdBackground.Items[newIndex];
            ModSettings.TCDBackground = item;
            ModSettings.SaveSettings();
        }

        private static void SettingsMenu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (checkboxItem == playDiodeSound)
            {
                ModSettings.PlayDiodeBeep = Checked;
            }
            else if (checkboxItem == useInputToggle)
            {
                ModSettings.UseInputToggle = Checked;
            }
            else if (checkboxItem == forceFlyMode)
            {
                ModSettings.ForceFlyMode = Checked;
            }
            else if (checkboxItem == playSpeedoBeep)
            {
                ModSettings.PlaySpeedoBeep = Checked;
            }
            else if (checkboxItem == GlowingWormholeEmitter)
            {
                ModSettings.GlowingWormholeEmitter = Checked;
            }
            else if (checkboxItem == GlowingPlutoniumReactor)
            {
                ModSettings.GlowingPlutoniumReactor = Checked;
            }
            else if (checkboxItem == playFluxCapacitorSound)
            {
                ModSettings.PlayFluxCapacitorSound = Checked;
            }
            else if (checkboxItem == playEngineSounds)
            {
                ModSettings.PlayEngineSounds = Checked;
            }
            else if (checkboxItem == cinematicSpawn)
            {
                ModSettings.CinematicSpawn = Checked;
            }
            else if (checkboxItem == LightningStrikeEvent)
            {
                ModSettings.LightningStrikeEvent = Checked;
            }
            else if (checkboxItem == EngineStallEvent)
            {
                ModSettings.EngineStallEvent = Checked;
            }
            else if (checkboxItem == TurbulenceEvent)
            {
                ModSettings.TurbulenceEvent = Checked;
            }
            else if (checkboxItem == LandingSystem)
            {
                ModSettings.LandingSystem = Checked;
            }

            ModSettings.SaveSettings();
        }

        private static void TcdMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == changeTCD)
            {
                TcdEditer.SetEditMode(true);

                Main.MenuPool.CloseAllMenus();
            }
            else if (selectedItem == resetToDefaultTCD)
            {
                TcdEditer.ResetToDefault();
            }
        }

        private static void MainMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == spawnPresetDelorean && TimeMachineClone.ListPresets().Count == 0)
                Notification.Show(Game.GetLocalizedString("BTTFV_Menu_Presets_Not_Found"));

            if (selectedItem == spawnCustomDelorean || selectedItem == spawnPresetDelorean)
                return;

            if (selectedItem == spawnDelorean)
            {
                DMC12Handler.CreateDMC12(Main.PlayerPed.Position, Main.PlayerPed.Heading);
                Main.MenuPool.CloseAllMenus();
            }

            if (selectedItem == spawnDelorean1)
            {
                if (ModSettings.CinematicSpawn)
                    TimeMachineHandler.SpawnWithReentry(WormholeType.BTTF1);
                else
                    TimeMachineHandler.Spawn(WormholeType.BTTF1);
                Main.MenuPool.CloseAllMenus();
            }

            if (selectedItem == spawnDelorean2)
            {
                if (ModSettings.CinematicSpawn)
                    TimeMachineHandler.SpawnWithReentry(WormholeType.BTTF2);
                else
                    TimeMachineHandler.Spawn(WormholeType.BTTF2);
                Main.MenuPool.CloseAllMenus();
            }

            if (selectedItem == spawnDelorean3)
            {
                if (ModSettings.CinematicSpawn)
                    TimeMachineHandler.SpawnWithReentry(WormholeType.BTTF3);
                else
                    TimeMachineHandler.Spawn(WormholeType.BTTF3);
                Main.MenuPool.CloseAllMenus();
            }

            if (selectedItem == convertToTimeMachine)
            {
                TimeMachineHandler.CreateTimeMachine(Main.PlayerVehicle, WormholeType.BTTF1);
                Main.MenuPool.CloseAllMenus();
            }

            if (selectedItem == removeOtherDeloreans)
            {
                TimeMachineHandler.RemoveAllTimeMachines(true);
                RemoteTimeMachineHandler.DeleteAll();
                Notification.Show(Game.GetLocalizedString("BTTFV_RemovedOtherTimeMachines"));
            }

            if (selectedItem == removeAllDeloreans)
            {
                TimeMachineHandler.RemoveAllTimeMachines();
                RemoteTimeMachineHandler.DeleteAll();
                Notification.Show(Game.GetLocalizedString("BTTFV_RemovedAllTimeMachines"));
            }

            if(selectedItem == removeDelorean)
            {
                var delorean = TimeMachineHandler.GetTimeMachineFromVehicle(Main.PlayerVehicle);

                if(delorean == null)
                {
                    Notification.Show(Game.GetLocalizedString("BTTFV_NotSeatedInTimeMachine"));
                    return;
                }

                TimeMachineHandler.RemoveTimeMachine(delorean);
            }
        }

        public static void KeyDown(KeyEventArgs key)
        {
            if(key.KeyCode == Keys.F8 && key.Control && !TcdEditer.IsEditing)
            {
                if(TimeMachineHandler.CurrentTimeMachine != null)
                    if (TimeMachineHandler.CurrentTimeMachine.Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole)
                        return;

                MainMenu.Visible = true;
            }

            //if (key.KeyCode == Keys.F7 && key.Control && !TcdEditer.IsEditing)
            //    InteractionMenuManager.TrainMissionMenu.Visible = true;
        }
    }
}
