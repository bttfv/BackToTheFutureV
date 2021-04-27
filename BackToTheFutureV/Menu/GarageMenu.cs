using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Native;
using LemonUI.Menus;
using System;
using System.ComponentModel;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class GarageMenu : BTTFVMenu
    {
        private NativeItem transformInto;
        private NativeItem hoverConvert;
        private NativeItem installMrFusion;
        private NativeItem buyPlutonium;
        private NativeItem repairTC;
        private NativeItem repairFC;
        private NativeItem repairEngine;

        private NativeSubmenuItem customMenu;

        public GarageMenu() : base("Garage")
        {
            transformInto = NewItem("Transform");
            hoverConvert = NewItem("InstallHover");
            installMrFusion = NewItem("InstallMrFusion");
            buyPlutonium = NewItem("BuyPlutonium");
            repairTC = NewItem("RepairTC");
            repairFC = NewItem("RepairFC");
            repairEngine = NewItem("RepairEngine");

            customMenu = NewSubmenu(MenuHandler.CustomMenuGarage, "Custom");
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
                    TextHandler.ShowNotification("NotEnoughMoney");
                    return;
                }

                TimeMachineHandler.Create(FusionUtils.PlayerVehicle).Properties.ReactorCharge = 0;
                Game.Player.Money -= 500000;
            }

            if (sender == hoverConvert)
            {
                if (Game.Player.Money < 39995)
                {
                    TextHandler.ShowNotification("NotEnoughMoney");
                    return;
                }

                TimeMachineHandler.CurrentTimeMachine.Mods.HoverUnderbody = ModState.On;
                Game.Player.Money -= 39995;
            }

            if (sender == installMrFusion)
            {
                if (Game.Player.Money < 100000)
                {
                    TextHandler.ShowNotification("NotEnoughMoney");
                    return;
                }

                TimeMachineHandler.CurrentTimeMachine.Mods.Reactor = ReactorType.MrFusion;
                Game.Player.Money -= 100000;
            }

            if (sender == buyPlutonium)
            {
                if (Game.Player.Money < 1500)
                {
                    TextHandler.ShowNotification("NotEnoughMoney");
                    return;
                }

                InternalInventory.Current.Plutonium++;
                Game.Player.Money -= 1500;
            }

            if (sender == repairTC)
            {
                if (Game.Player.Money < 500)
                {
                    TextHandler.ShowNotification("NotEnoughMoney");
                    return;
                }

                if (TimeMachineHandler.CurrentTimeMachine.Repair(true, false, false))
                    Game.Player.Money -= 500;
            }

            if (sender == repairFC)
            {
                if (Game.Player.Money < 1000)
                {
                    TextHandler.ShowNotification("NotEnoughMoney");
                    return;
                }

                if (TimeMachineHandler.CurrentTimeMachine.Repair(false, true, false))
                    Game.Player.Money -= 100;
            }

            if (sender == repairEngine)
            {
                if (Game.Player.Money < 750)
                {
                    TextHandler.ShowNotification("NotEnoughMoney");
                    return;
                }

                if (TimeMachineHandler.CurrentTimeMachine.Repair(false, false, true))
                    Game.Player.Money -= 750;
            }
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {

        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {
            if (sender == hoverConvert && !sender.Enabled && TimeMachineHandler.CurrentTimeMachine.NotNullAndExists() && TimeMachineHandler.CurrentTimeMachine.Properties.AreFlyingCircuitsBroken)
                TextHandler.ShowSubtitle("HoverDamaged");
        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            GarageHandler.WaitForCustomMenu = false;
        }

        public override void Tick()
        {
            if (!FusionUtils.PlayerVehicle.NotNullAndExists())
            {
                Close();
                return;
            }

            Function.Call(Hash.SHOW_HUD_COMPONENT_THIS_FRAME, 3);

            bool active = TimeMachineHandler.CurrentTimeMachine.NotNullAndExists();

            transformInto.Enabled = !active && FusionUtils.CurrentTime >= new DateTime(1985, 10, 25);
            hoverConvert.Enabled = active && FusionUtils.CurrentTime.Year >= 2015 && TimeMachineHandler.CurrentTimeMachine.Mods.HoverUnderbody == ModState.Off && ((TimeMachineHandler.CurrentTimeMachine.Mods.IsDMC12 && !TimeMachineHandler.CurrentTimeMachine.Properties.AreFlyingCircuitsBroken) || TimeMachineHandler.CurrentTimeMachine.Vehicle.CanHoverTransform());
            installMrFusion.Enabled = active && FusionUtils.CurrentTime.Year >= 2015 && TimeMachineHandler.CurrentTimeMachine.Mods.Reactor == ReactorType.Nuclear && TimeMachineHandler.CurrentTimeMachine.Mods.IsDMC12;
            repairTC.Enabled = active && (TimeMachineHandler.CurrentTimeMachine.Properties.AreTimeCircuitsBroken && TimeMachineHandler.CurrentTimeMachine.Mods.Hoodbox == ModState.Off);
            repairFC.Enabled = active && TimeMachineHandler.CurrentTimeMachine.Properties.AreFlyingCircuitsBroken;
            repairEngine.Enabled = active && TimeMachineHandler.CurrentTimeMachine.Vehicle.EngineHealth <= 0;

            buyPlutonium.Enabled = InternalInventory.Current.Plutonium < 5 && FusionUtils.CurrentTime.Year == 1985;
            buyPlutonium.AltTitle = $"{InternalInventory.Current.Plutonium}/5";

            customMenu.Enabled = active && !TimeMachineHandler.CurrentTimeMachine.Constants.FullDamaged;

            Recalculate();
        }
    }
}
