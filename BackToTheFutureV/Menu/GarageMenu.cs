using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Native;
using KlangRageAudioLibrary;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class GarageMenu : BTTFVMenu
    {
        private readonly NativeItem transformInto;
        private readonly NativeItem hoverConvert;
        private readonly NativeItem installMrFusion;
        private readonly NativeItem buyPlutonium;
        private readonly NativeItem repairTC;
        private readonly NativeItem repairFC;
        private readonly NativeItem repairEngine;
        private readonly NativeItem washCar;

        public static AudioPlayer[] GarageSounds { get; } = { Main.CommonAudioEngine.Create("general/garage/tireChange.wav", Presets.No3D), Main.CommonAudioEngine.Create("general/garage/drill1.wav", Presets.No3D), Main.CommonAudioEngine.Create("general/garage/drill2.wav", Presets.No3D), Main.CommonAudioEngine.Create("general/garage/drill3.wav", Presets.No3D) };

        private readonly NativeSubmenuItem customMenu;

        public GarageMenu() : base("Garage")
        {
            transformInto = NewItem("Transform");
            hoverConvert = NewItem("InstallHover");
            installMrFusion = NewItem("InstallMrFusion");
            buyPlutonium = NewItem("BuyPlutonium");
            repairTC = NewItem("RepairTC");
            repairFC = NewItem("RepairFC");
            repairEngine = NewItem("RepairEngine");
            washCar = NewItem("WashCar");

            customMenu = NewSubmenu(MenuHandler.CustomMenuGarage);
            customMenu.Activated += CustomMenu_Activated;
        }

        private void CustomMenu_Activated(object sender, EventArgs e)
        {
            GarageHandler.WaitForCustomMenu = true;
        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {
            FusionUtils.HideGUI = true;
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            switch (sender)
            {
                case NativeItem item when item == transformInto:
                    if (Game.Player.Money < 500000)
                    {
                        TextHandler.Me.ShowNotification("NotEnoughMoney");
                        return;
                    }
                    Game.Player.Money -= 500000;
                    GarageHandler.Transform = true;
                    Visible = false;
                    break;

                case NativeItem item when item == hoverConvert:
                    if (Game.Player.Money < 39995)
                    {
                        TextHandler.Me.ShowNotification("NotEnoughMoney");
                        return;
                    }
                    GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                    CurrentTimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.HoverUnderbodyCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                    CurrentTimeMachine.Mods.HoverUnderbody = ModState.On;
                    CurrentTimeMachine.Mods.Plate = PlateType.BTTF2;
                    CurrentTimeMachine.Mods.WormholeType = WormholeType.BTTF2;
                    Game.Player.Money -= 39995;
                    break;

                case NativeItem item when item == installMrFusion:
                    if (Game.Player.Money < 100000)
                    {
                        TextHandler.Me.ShowNotification("NotEnoughMoney");
                        return;
                    }
                    GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                    CurrentTimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.ReactorCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                    CurrentTimeMachine.Mods.Reactor = ReactorType.MrFusion;
                    Game.Player.Money -= 100000;
                    break;

                case NativeItem item when item == buyPlutonium:
                    if (Game.Player.Money < 1500)
                    {
                        TextHandler.Me.ShowNotification("NotEnoughMoney");
                        return;
                    }
                    InternalInventory.Current.Plutonium++;
                    Game.Player.Money -= 1500;
                    break;

                case NativeItem item when item == repairTC:
                    if (Game.Player.Money < 500)
                    {
                        TextHandler.Me.ShowNotification("NotEnoughMoney");
                        return;
                    }
                    GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                    if (CurrentTimeMachine.Repair(true, false, false))
                    {
                        Game.Player.Money -= 500;
                    }
                    break;

                case NativeItem item when item == repairFC:
                    if (Game.Player.Money < 1000)
                    {
                        TextHandler.Me.ShowNotification("NotEnoughMoney");
                        return;
                    }
                    GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                    if (CurrentTimeMachine.Repair(false, true, false))
                    {
                        Game.Player.Money -= 1000;
                    }
                    break;

                case NativeItem item when item == repairEngine:
                    if (Game.Player.Money < 750)
                    {
                        TextHandler.Me.ShowNotification("NotEnoughMoney");
                        return;
                    }
                    GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                    if (CurrentTimeMachine.Repair(false, false, true))
                    {
                        Game.Player.Money -= 750;
                    }
                    break;

                case NativeItem item when item == washCar:
                    if (Game.Player.Money < 10)
                    {
                        TextHandler.Me.ShowNotification("NotEnoughMoney");
                        return;
                    }
                    CurrentTimeMachine.Vehicle.Wash();
                    Game.Player.Money -= 10;
                    break;
            }
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {

        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {
            switch (sender)
            {
                case NativeItem item when item == repairTC && !sender.Enabled && CurrentTimeMachine.NotNullAndExists() && CurrentTimeMachine.Properties.AreTimeCircuitsBroken:
                    TextHandler.Me.ShowSubtitle("UnableRepairTC");
                    break;
                case NativeItem item when item == repairFC && !sender.Enabled && CurrentTimeMachine.NotNullAndExists() && CurrentTimeMachine.Properties.AreFlyingCircuitsBroken:
                    TextHandler.Me.ShowSubtitle("UnableRepairFC");
                    break;
                case NativeItem item when item == repairEngine && !sender.Enabled && CurrentTimeMachine.NotNullAndExists() && CurrentTimeMachine.Vehicle.EngineHealth <= 0 && CurrentTimeMachine.Mods.Wheels.Burst:
                    TextHandler.Me.ShowSubtitle("UnableRepairEngine");
                    break;
            }
        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            if (FusionUtils.PlayerVehicle.IsAnyDoorOpen())
            {
                Function.Call(Hash.SET_VEHICLE_DOORS_SHUT, FusionUtils.PlayerVehicle, false);
            }

            GarageHandler.WaitForCustomMenu = false;

            foreach (AudioPlayer audioPlayer in GarageSounds)
            {
                audioPlayer.SourceEntity = CurrentTimeMachine;
            }

            if (CurrentTimeMachine.NotNullAndExists() && CurrentTimeMachine.Vehicle.EngineHealth <= 0 && CurrentTimeMachine.Mods.Wheels.Burst && !Items.Contains(repairEngine))
            {
                Add(6, repairEngine);
            }
            else if (Items.Contains(repairEngine) && CurrentTimeMachine.NotNullAndExists() && !(CurrentTimeMachine.Vehicle.EngineHealth <= 0 && CurrentTimeMachine.Mods.Wheels.Burst))
            {
                Remove(repairEngine);
            }

            if (!CurrentTimeMachine.NotNullAndExists() && Items.Contains(washCar))
            {
                Remove(washCar);
            }
            else if (CurrentTimeMachine.NotNullAndExists() && !Items.Contains(washCar) && !Items.Contains(repairEngine))
            {
                Add(6, washCar);
            }
            else if (CurrentTimeMachine.NotNullAndExists() && !Items.Contains(washCar) && Items.Contains(repairEngine))
            {
                Add(7, washCar);
            }
        }

        public override void Tick()
        {
            if (!FusionUtils.PlayerVehicle.NotNullAndExists())
            {
                Visible = false;
                return;
            }

            Function.Call(Hash.SHOW_HUD_COMPONENT_THIS_FRAME, 3);

            bool active = CurrentTimeMachine.NotNullAndExists();

            transformInto.Enabled = !active && FusionUtils.CurrentTime >= new DateTime(1985, 10, 25);
            hoverConvert.Enabled = active && FusionUtils.CurrentTime.Year >= 2015 && CurrentTimeMachine.Mods.HoverUnderbody == ModState.Off && ((CurrentTimeMachine.Mods.IsDMC12 && !CurrentTimeMachine.Properties.AreFlyingCircuitsBroken) || CurrentTimeMachine.Vehicle.CanHoverTransform());
            installMrFusion.Enabled = active && FusionUtils.CurrentTime.Year >= 2015 && CurrentTimeMachine.Mods.Reactor == ReactorType.Nuclear && CurrentTimeMachine.Mods.IsDMC12;
            repairTC.Enabled = active && ((FusionUtils.CurrentTime.Year >= 1952 && CurrentTimeMachine.Properties.AreTimeCircuitsBroken && CurrentTimeMachine.Mods.Hoodbox == ModState.Off) || (FusionUtils.CurrentTime.Year >= 1985 && CurrentTimeMachine.Mods.Hoodbox == ModState.On));
            repairFC.Enabled = active && FusionUtils.CurrentTime.Year >= 2015 && CurrentTimeMachine.Properties.AreFlyingCircuitsBroken;
            repairEngine.Enabled = active && FusionUtils.CurrentTime.Year >= 1912 && CurrentTimeMachine.Vehicle.EngineHealth <= 0 && CurrentTimeMachine.Mods.Wheels.Burst;
            washCar.Enabled = active && CurrentTimeMachine.Mods.IsDMC12 && CurrentTimeMachine.Vehicle.DirtLevel >= 1f;

            buyPlutonium.Enabled = InternalInventory.Current.Plutonium < 12 && FusionUtils.CurrentTime.Year > 1985 && FusionUtils.CurrentTime.Year < 2015;
            buyPlutonium.AltTitle = $"{InternalInventory.Current.Plutonium}/12";

            customMenu.Enabled = active && !CurrentTimeMachine.Constants.FullDamaged;

            Recalculate();
        }
    }
}
