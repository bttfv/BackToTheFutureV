using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LemonUI.Menus;
using BackToTheFutureV.Vehicles;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Utility;
using BackToTheFutureV.TimeMachineClasses.RC;
using GTA.UI;
using LemonUI.Elements;
using System.Drawing;

namespace BackToTheFutureV.Menu
{
    public class MainMenu : CustomNativeMenu
    {
        private NativeItem spawnDMC12;
        private NativeItem spawnBTTF1;
        private NativeItem spawnBTTF2;
        private NativeItem spawnBTTF3;

        private NativeSubmenuItem presetsMenu;

        private NativeItem convertIntoTimeMachine;
        
        private NativeSubmenuItem customMenu;
        private NativeSubmenuItem rcMenu;
        private NativeSubmenuItem outatimeMenu;

        private NativeItem deleteCurrent;
        private NativeItem deleteOthers;
        private NativeItem deleteAll;

        private NativeSubmenuItem settingsMenu;        

        public MainMenu() : base("", Game.GetLocalizedString("BTTFV_Menu_Description"))
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

            OnItemActivated += MainMenu_OnItemActivated;

            Add(spawnDMC12 = new NativeItem($"{Game.GetLocalizedString("BTTFV_Menu_Spawn")} {Game.GetLocalizedString("BTTFV_Menu_DMC12")}", Game.GetLocalizedString("BTTFV_Menu_SpawnDMC12_Description")));
            Add(spawnBTTF1 = new NativeItem($"{Game.GetLocalizedString("BTTFV_Menu_Spawn")} {Game.GetLocalizedString("BTTFV_Menu_BTTF1")}", Game.GetLocalizedString("BTTFV_Menu_SpawnBTTF1_Description")));
            Add(spawnBTTF2 = new NativeItem($"{Game.GetLocalizedString("BTTFV_Menu_Spawn")} {Game.GetLocalizedString("BTTFV_Menu_BTTF2")}", Game.GetLocalizedString("BTTFV_Menu_SpawnBTTF2_Description")));
            Add(spawnBTTF3 = new NativeItem($"{Game.GetLocalizedString("BTTFV_Menu_Spawn")} {Game.GetLocalizedString("BTTFV_Menu_BTTF3")}", Game.GetLocalizedString("BTTFV_Menu_SpawnBTTF3_Description")));

            presetsMenu = AddSubMenu(MenuHandler.PresetsMenu);
            presetsMenu.Title = Game.GetLocalizedString("BTTFV_Menu_Spawn_Preset");
            presetsMenu.Description = Game.GetLocalizedString("BTTFV_Menu_Spawn_Preset_Description");

            Add(convertIntoTimeMachine = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_ConvertToTimeMachine"), Game.GetLocalizedString("BTTFV_Menu_ConvertToTimeMachine_Description")));

            customMenu = AddSubMenu(MenuHandler.CustomMenu);
            customMenu.Title = Game.GetLocalizedString("BTTFV_Menu_Build_Delorean");
            customMenu.Description = Game.GetLocalizedString("BTTFV_Menu_Build_Delorean_Description");

            rcMenu = AddSubMenu(MenuHandler.RCMenu);
            rcMenu.Title = Game.GetLocalizedString("BTTFV_Menu_RCMenu");
            rcMenu.Description = Game.GetLocalizedString("BTTFV_Menu_RCMenu_Description");

            outatimeMenu = AddSubMenu(MenuHandler.OutatimeMenu);
            outatimeMenu.Title = Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu");
            outatimeMenu.Description = Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Description");

            Add(deleteCurrent = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_RemoveTimeMachine"), Game.GetLocalizedString("BTTFV_Menu_RemoveTimeMachine_Description")));
            Add(deleteOthers = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_RemoveOtherTimeMachines"), Game.GetLocalizedString("BTTFV_Menu_RemoveOtherTimeMachines_Description")));
            Add(deleteAll = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_RemoveAllTimeMachines"), Game.GetLocalizedString("BTTFV_Menu_RemoveAllTimeMachines_Description")));

            settingsMenu = AddSubMenu(MenuHandler.SettingsMenu);
            settingsMenu.Title = Game.GetLocalizedString("BTTFV_Menu_Settings");            
            
            Main.ObjectPool.Add(this);
        }

        public override void Tick()
        {
            convertIntoTimeMachine.Enabled = Main.PlayerVehicle != null && !Main.PlayerVehicle.IsTimeMachine();

            outatimeMenu.Enabled = RemoteTimeMachineHandler.TimeMachineCount > 0;

            rcMenu.Enabled = Main.PlayerVehicle == null && TimeMachineHandler.TimeMachineCount > 0;
        }

        private void MainMenu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == spawnDMC12)
                Main.PlayerPed.Task.WarpIntoVehicle(DMC12Handler.CreateDMC12(Main.PlayerPed.Position, Main.PlayerPed.Heading), VehicleSeat.Driver);

            if (sender == spawnBTTF1)
            {
                if (ModSettings.CinematicSpawn)
                    TimeMachineHandler.SpawnWithReentry(WormholeType.BTTF1);
                else
                    TimeMachineHandler.Spawn(WormholeType.BTTF1, true);
            }

            if (sender == spawnBTTF2)
            {
                if (ModSettings.CinematicSpawn)
                    TimeMachineHandler.SpawnWithReentry(WormholeType.BTTF2);
                else
                    TimeMachineHandler.Spawn(WormholeType.BTTF2, true);
            }

            if (sender == spawnBTTF3)
            {
                if (ModSettings.CinematicSpawn)
                    TimeMachineHandler.SpawnWithReentry(WormholeType.BTTF3);
                else
                    TimeMachineHandler.Spawn(WormholeType.BTTF3, true);
            }

            if (sender == convertIntoTimeMachine)
                TimeMachineHandler.CreateTimeMachine(Main.PlayerVehicle, WormholeType.BTTF1);

            if (sender == deleteCurrent)
            {
                var timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(Main.PlayerVehicle);

                if (timeMachine == null)
                {
                    Notification.Show(Game.GetLocalizedString("BTTFV_NotSeatedInTimeMachine"));
                    return;
                }

                TimeMachineHandler.RemoveTimeMachine(timeMachine);
            }

            if (sender == deleteOthers)
            {
                TimeMachineHandler.RemoveAllTimeMachines(true);
                RemoteTimeMachineHandler.DeleteAll();
                Notification.Show(Game.GetLocalizedString("BTTFV_RemovedOtherTimeMachines"));
            }

            if (sender == deleteAll)
            {
                TimeMachineHandler.RemoveAllTimeMachines();
                RemoteTimeMachineHandler.DeleteAll();
                Notification.Show(Game.GetLocalizedString("BTTFV_RemovedAllTimeMachines"));
            }

            Close();
        }
    }
}
