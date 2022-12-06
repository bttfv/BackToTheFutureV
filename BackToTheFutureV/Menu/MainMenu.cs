using BackToTheFutureV;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class MainMenu : BTTFVMenu
    {
        private readonly NativeListItem<string> spawnBTTF;
        private readonly NativeSubmenuItem presetsMenu;
        private readonly NativeItem convertIntoTimeMachine;
        private readonly NativeSubmenuItem customMenu;

        private readonly NativeSubmenuItem rcMenu;
        private readonly NativeSubmenuItem outatimeMenu;
        private readonly NativeSubmenuItem doorsMenu;

        private readonly NativeItem deleteCurrent;
        private readonly NativeItem deleteOthers;
        private readonly NativeItem deleteAll;

        public MainMenu() : base("Main")
        {
            spawnBTTF = NewListItem("Spawn", TextHandler.Me.GetLocalizedText("DMC12", "BTTF1", "BTTF1H", "BTTF2", "BTTF3", "BTTF3RR"));
            spawnBTTF.ItemChanged += SpawnBTTF_ItemChanged;
            spawnBTTF.Description = GetItemValueDescription("Spawn", "DMC12");

            presetsMenu = NewSubmenu(MenuHandler.PresetsMenu);

            convertIntoTimeMachine = NewItem("Convert");

            customMenu = NewSubmenu(MenuHandler.CustomMenuMain);

            rcMenu = NewSubmenu(MenuHandler.RCMenu);
            outatimeMenu = NewSubmenu(MenuHandler.OutatimeMenu);
            doorsMenu = NewSubmenu(MenuHandler.DoorsMenu);
            NewSubmenu(MenuHandler.SettingsMenu);

            deleteCurrent = NewItem("Remove");
            deleteOthers = NewItem("RemoveOther");
            deleteAll = NewItem("RemoveAll");
        }

        private void SpawnBTTF_ItemChanged(object sender, ItemChangedEventArgs<string> e)
        {
            switch (e.Index)
            {
                case 0:
                    spawnBTTF.Description = GetItemValueDescription(sender, "DMC12");
                    break;
                case 1:
                case 2:
                    spawnBTTF.Description = GetItemValueDescription(sender, "BTTF1");
                    break;
                case 3:
                    spawnBTTF.Description = GetItemValueDescription(sender, "BTTF2");
                    break;
                case 4:
                    spawnBTTF.Description = GetItemValueDescription(sender, "BTTF3");
                    break;
                case 5:
                    spawnBTTF.Description = GetItemValueDescription(sender, "BTTF3RR");
                    break;
            }

            Recalculate();
        }

        public override void Tick()
        {
            convertIntoTimeMachine.Enabled = FusionUtils.PlayerVehicle.IsFunctioning() && !FusionUtils.PlayerVehicle.IsTimeMachine() && !Game.IsMissionActive;

            doorsMenu.Enabled = FusionUtils.PlayerPed?.GetClosestVehicle(5f)?.Model == ModelHandler.DMC12 && !FusionUtils.PlayerPed.GetClosestVehicle(5f).IsConsideredDestroyed;

            outatimeMenu.Enabled = RemoteTimeMachineHandler.RemoteTimeMachineCount > 0 && !Game.IsMissionActive;

            rcMenu.Enabled = FusionUtils.PlayerVehicle == null && TimeMachineHandler.TimeMachineCount > 0 && !Game.IsMissionActive;
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            TimeMachine timeMachine;

            if (sender == spawnBTTF)
            {
                if (spawnBTTF.SelectedIndex == 0)
                {
                    FusionUtils.PlayerPed.Task.WarpIntoVehicle(DMC12Handler.CreateDMC12(FusionUtils.PlayerPed.Position, FusionUtils.PlayerPed.Heading), VehicleSeat.Driver);
                    Visible = false;
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
                {
                    timeMachine = TimeMachineHandler.Create(SpawnFlags.ForceReentry | SpawnFlags.New, wormholeType);
                }
                else
                {
                    timeMachine = TimeMachineHandler.Create(SpawnFlags.WarpPlayer | SpawnFlags.New, wormholeType);
                }

                if (spawnBTTF.SelectedIndex == 2)
                {
                    timeMachine.Mods.Hook = HookState.OnDoor;
                    timeMachine.Mods.Plate = PlateType.Empty;
                    timeMachine.Mods.Bulova = ModState.On;

                    timeMachine.Properties.ReactorCharge = 0;
                }

                if (spawnBTTF.SelectedIndex == 5)
                {
                    timeMachine.Mods.Wheel = WheelType.RailroadInvisible;
                }
            }

            if (sender == convertIntoTimeMachine)
            {
                FusionUtils.PlayerVehicle.TransformIntoTimeMachine();
            }

            if (sender == deleteCurrent)
            {
                timeMachine = TimeMachineHandler.GetTimeMachineFromVehicle(FusionUtils.PlayerVehicle);

                if (timeMachine == null)
                {
                    TextHandler.Me.ShowNotification("NotSeated");
                    return;
                }

                TimeMachineHandler.RemoveTimeMachine(timeMachine);
            }

            if (sender == deleteOthers)
            {
                TimeMachineHandler.RemoveAllTimeMachines(true);
                RemoteTimeMachineHandler.DeleteAll();
                TextHandler.Me.ShowNotification("RemovedOtherTimeMachines");
            }

            if (sender == deleteAll)
            {
                TimeMachineHandler.RemoveAllTimeMachines();
                RemoteTimeMachineHandler.DeleteAll();
                TextHandler.Me.ShowNotification("RemovedAllTimeMachines");
            }

            Visible = false;
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {

        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {

        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {

        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            if (RemoteTimeMachineHandler.IsRemoteOn && Items.Contains(deleteAll))
            {
                Remove(deleteCurrent);
                Remove(deleteOthers);
                Remove(deleteAll);
            }
            else if (!Items.Contains(deleteAll) && !Items.Contains(spawnBTTF))
            {
                Add(4, deleteCurrent);
                Add(5, deleteOthers);
                Add(6, deleteAll);
            }
            else if (!Items.Contains(deleteAll) && Items.Contains(spawnBTTF))
            {
                Add(8, deleteCurrent);
                Add(9, deleteOthers);
                Add(10, deleteAll);
            }

            if (!MenuHandler.UnlockSpawnMenu || Game.IsMissionActive)
            {
                Remove(spawnBTTF);
                Remove(presetsMenu);
                Remove(customMenu);
                Remove(convertIntoTimeMachine);
            }
            else if (!Items.Contains(spawnBTTF))
            {
                Add(0, spawnBTTF);
                Add(1, presetsMenu);
                Add(2, customMenu);
                Add(3, convertIntoTimeMachine);
            }
        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }
    }
}
