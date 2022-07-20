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

        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == transformInto)
            {
                if (Game.Player.Money < 500000)
                {
                    TextHandler.Me.ShowNotification("NotEnoughMoney");
                    return;
                }

                //Create(FusionUtils.PlayerVehicle).Properties.ReactorCharge = 0;
                Game.Player.Money -= 500000;
                GarageHandler.Transform = true;
                Visible = false;
            }

            if (sender == hoverConvert)
            {
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
            }

            if (sender == installMrFusion)
            {
                if (Game.Player.Money < 100000)
                {
                    TextHandler.Me.ShowNotification("NotEnoughMoney");
                    return;
                }

                GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                CurrentTimeMachine.CustomCameraManager.Show((int)TimeMachineCamera.ReactorCustom, FusionEnums.CameraSwitchType.Instant, 1250);
                CurrentTimeMachine.Mods.Reactor = ReactorType.MrFusion;
                Game.Player.Money -= 100000;
            }

            if (sender == buyPlutonium)
            {
                if (Game.Player.Money < 1500)
                {
                    TextHandler.Me.ShowNotification("NotEnoughMoney");
                    return;
                }

                GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                InternalInventory.Current.Plutonium++;
                Game.Player.Money -= 1500;
            }

            if (sender == repairTC)
            {
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
            }

            if (sender == repairFC)
            {
                if (Game.Player.Money < 1000)
                {
                    TextHandler.Me.ShowNotification("NotEnoughMoney");
                    return;
                }

                GarageSounds[FusionUtils.Random.Next(1, 4)].Play();
                if (CurrentTimeMachine.Repair(false, true, false))
                {
                    Game.Player.Money -= 100;
                }
            }

            if (sender == repairEngine)
            {
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
            }

            if (sender == washCar)
            {
                if (Game.Player.Money < 10)
                {
                    TextHandler.Me.ShowNotification("NotEnoughMoney");
                    return;
                }

                CurrentTimeMachine.Vehicle.Wash();
                Game.Player.Money -= 10;
            }
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {

        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {
            if (sender == repairTC && !sender.Enabled && CurrentTimeMachine.NotNullAndExists() && CurrentTimeMachine.Properties.AreTimeCircuitsBroken)
            {
                TextHandler.Me.ShowSubtitle("UnableRepairTC");
            }

            if (sender == repairFC && !sender.Enabled && CurrentTimeMachine.NotNullAndExists() && CurrentTimeMachine.Properties.AreFlyingCircuitsBroken)
            {
                TextHandler.Me.ShowSubtitle("UnableRepairFC");
            }

            if (sender == repairEngine && !sender.Enabled && CurrentTimeMachine.NotNullAndExists() && CurrentTimeMachine.Vehicle.EngineHealth <= 0 && CurrentTimeMachine.Mods.Wheels.Burst)
            {
                TextHandler.Me.ShowSubtitle("UnableRepairEngine");
            }
        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            GarageHandler.WaitForCustomMenu = false;

            foreach (AudioPlayer audioPlayer in GarageSounds)
            {
                audioPlayer.SourceEntity = CurrentTimeMachine;
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
            repairTC.Enabled = active && FusionUtils.CurrentTime.Year >= 1952 && (CurrentTimeMachine.Properties.AreTimeCircuitsBroken || (FusionUtils.CurrentTime.Year >= 1985 && CurrentTimeMachine.Mods.Hoodbox == ModState.On && CurrentTimeMachine.Mods.IsDMC12));
            repairFC.Enabled = active && FusionUtils.CurrentTime.Year >= 2015 && CurrentTimeMachine.Properties.AreFlyingCircuitsBroken;
            repairEngine.Enabled = active && FusionUtils.CurrentTime.Year >= 1912 && (CurrentTimeMachine.Vehicle.EngineHealth <= 0 && CurrentTimeMachine.Mods.Wheels.Burst);
            washCar.Enabled = active && CurrentTimeMachine.Vehicle.DirtLevel > 0;

            buyPlutonium.Enabled = InternalInventory.Current.Plutonium < 5 && FusionUtils.CurrentTime.Year == 1985;
            buyPlutonium.AltTitle = $"{InternalInventory.Current.Plutonium}/5";

            customMenu.Enabled = active && !CurrentTimeMachine.Constants.FullDamaged;

            Recalculate();
        }
    }
}
