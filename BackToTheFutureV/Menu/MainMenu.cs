using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.UI;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.Collections.Generic;
using System.Drawing;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.Menu
{
    public class MainMenu : CustomNativeMenu
    {
        private NativeListItem<string> spawnBTTF;

        private NativeSubmenuItem presetsMenu;

        private NativeItem convertIntoTimeMachine;
        
        private NativeSubmenuItem customMenu;
        private NativeSubmenuItem rcMenu;
        private NativeSubmenuItem outatimeMenu;

        private NativeItem deleteCurrent;
        private NativeItem deleteOthers;
        private NativeItem deleteAll;

        private NativeSubmenuItem settingsMenu;

        private readonly List<string> _bttfTypes = new List<string> { Game.GetLocalizedString("BTTFV_Menu_DMC12"), Game.GetLocalizedString("BTTFV_Menu_BTTF1"), Game.GetLocalizedString("BTTFV_Menu_BTTF1H"), Game.GetLocalizedString("BTTFV_Menu_BTTF2"), Game.GetLocalizedString("BTTFV_Menu_BTTF3"), Game.GetLocalizedString("BTTFV_Menu_BTTF3RR") };
       
        public MainMenu() : base("", Game.GetLocalizedString("BTTFV_Menu_Description"))
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

            OnItemActivated += MainMenu_OnItemActivated;

            Add(spawnBTTF = new NativeListItem<string>(Game.GetLocalizedString("BTTFV_Menu_Spawn"), Game.GetLocalizedString("BTTFV_Menu_SpawnDMC12_Description"), _bttfTypes.ToArray()));
            spawnBTTF.ItemChanged += SpawnBTTF_ItemChanged;

            presetsMenu = AddSubMenu(MenuHandler.PresetsMenu);
            presetsMenu.Title = Game.GetLocalizedString("BTTFV_Menu_Spawn_Preset");
            presetsMenu.Description = Game.GetLocalizedString("BTTFV_Menu_Spawn_Preset_Description");

            //AddSeparator();

            Add(convertIntoTimeMachine = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_ConvertToTimeMachine"), Game.GetLocalizedString("BTTFV_Menu_ConvertToTimeMachine_Description")));

            customMenu = AddSubMenu(MenuHandler.CustomMenu);
            customMenu.Title = Game.GetLocalizedString("BTTFV_Menu_Build_Delorean");
            customMenu.Description = Game.GetLocalizedString("BTTFV_Menu_Build_Delorean_Description");

            //AddSeparator();

            rcMenu = AddSubMenu(MenuHandler.RCMenu);
            rcMenu.Title = Game.GetLocalizedString("BTTFV_Menu_RCMenu");
            rcMenu.Description = Game.GetLocalizedString("BTTFV_Menu_RCMenu_Description");

            outatimeMenu = AddSubMenu(MenuHandler.OutatimeMenu);
            outatimeMenu.Title = Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu");
            outatimeMenu.Description = Game.GetLocalizedString("BTTFV_Menu_StatisticsMenu_Description");

            //AddSeparator();

            Add(deleteCurrent = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_RemoveTimeMachine"), Game.GetLocalizedString("BTTFV_Menu_RemoveTimeMachine_Description")));
            Add(deleteOthers = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_RemoveOtherTimeMachines"), Game.GetLocalizedString("BTTFV_Menu_RemoveOtherTimeMachines_Description")));
            Add(deleteAll = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_RemoveAllTimeMachines"), Game.GetLocalizedString("BTTFV_Menu_RemoveAllTimeMachines_Description")));

            //AddSeparator();

            settingsMenu = AddSubMenu(MenuHandler.SettingsMenu);
            settingsMenu.Title = Game.GetLocalizedString("BTTFV_Menu_Settings");            
        }

        private void SpawnBTTF_ItemChanged(object sender, ItemChangedEventArgs<string> e)
        {
            switch (e.Index)
            {
                case 0:
                    spawnBTTF.Description = Game.GetLocalizedString("BTTFV_Menu_SpawnDMC12_Description");
                    break;
                case 1:
                case 2:
                    spawnBTTF.Description = Game.GetLocalizedString("BTTFV_Menu_SpawnBTTF1_Description");
                    break;
                case 3:
                    spawnBTTF.Description = Game.GetLocalizedString("BTTFV_Menu_SpawnBTTF2_Description");
                    break;
                case 4:
                    spawnBTTF.Description = Game.GetLocalizedString("BTTFV_Menu_SpawnBTTF3_Description");
                    break;
                case 5:
                    spawnBTTF.Description = Game.GetLocalizedString("BTTFV_Menu_SpawnBTTF3RR_Description");
                    break;
            }

            Recalculate();
        }

        public override void Tick()
        {
            convertIntoTimeMachine.Enabled = Utils.PlayerVehicle.NotNullAndExists() && !Utils.PlayerVehicle.IsTimeMachine();

            outatimeMenu.Enabled = RemoteTimeMachineHandler.TimeMachineCount > 0;

            rcMenu.Enabled = Utils.PlayerVehicle == null && TimeMachineHandler.TimeMachineCount > 0;
        }

        private void MainMenu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            TimeMachine timeMachine;
            
            if (sender == spawnBTTF)
            {
                if (spawnBTTF.SelectedIndex == 0)
                {
                    Utils.PlayerPed.Task.WarpIntoVehicle(DMC12Handler.CreateDMC12(Utils.PlayerPed.Position, Utils.PlayerPed.Heading), VehicleSeat.Driver);
                    Close();
                    return;
                }

                WormholeType wormholeType = WormholeType.BTTF1;

                switch (spawnBTTF.SelectedIndex)
                {
                    case 3:
                        wormholeType = WormholeType.BTTF2;
                        break;
                    case 4:
                    case 5:
                        wormholeType = WormholeType.BTTF3;
                        break;
                }

                if (ModSettings.CinematicSpawn)
                    timeMachine = TimeMachineHandler.Create(SpawnFlags.ForceReentry | SpawnFlags.New, wormholeType);
                else
                    timeMachine = TimeMachineHandler.Create(SpawnFlags.WarpPlayer | SpawnFlags.New, wormholeType);

                if (spawnBTTF.SelectedIndex == 2)
                {
                    timeMachine.Mods.Hook = HookState.OnDoor;
                    timeMachine.Mods.Plate = PlateType.Empty;
                }
                    
                if (spawnBTTF.SelectedIndex == 5)
                    timeMachine.Mods.Wheel = WheelType.RailroadInvisible;
            }

            if (sender == convertIntoTimeMachine)
                Utils.PlayerVehicle.TransformIntoTimeMachine();

            if (sender == deleteCurrent)
            {
                timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(Utils.PlayerVehicle);

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
                WaybackMachineHandler.Abort();
                Notification.Show(Game.GetLocalizedString("BTTFV_RemovedOtherTimeMachines"));
            }

            if (sender == deleteAll)
            {
                TimeMachineHandler.RemoveAllTimeMachines();
                RemoteTimeMachineHandler.DeleteAll();
                WaybackMachineHandler.Abort();
                Notification.Show(Game.GetLocalizedString("BTTFV_RemovedAllTimeMachines"));
            }

            Close();
        }
    }
}
