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
        private NativeItem restoreCar;

        private NativeSubmenuItem customMenu;

        public GarageMenu() : base("Garage")
        {
            transformInto = NewItem("Transform");
            hoverConvert = NewItem("InstallHover");
            installMrFusion = NewItem("InstallMrFusion");
            buyPlutonium = NewItem("BuyPlutonium");
            restoreCar = NewItem("Restore");

            customMenu = NewSubmenu(MenuHandler.CustomMenu2, "Custom");

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

                TimeMachineHandler.Create(FusionUtils.PlayerVehicle);
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

            if (sender == restoreCar)
            {
                if (Game.Player.Money < 1249)
                {
                    TextHandler.ShowNotification("NotEnoughMoney");
                    return;
                }

                TimeMachineHandler.CurrentTimeMachine.Repair();
                Game.Player.Money -= 1249;
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

            transformInto.Enabled = !active;
            hoverConvert.Enabled = active && TimeMachineHandler.CurrentTimeMachine.Mods.HoverUnderbody == ModState.Off && ((TimeMachineHandler.CurrentTimeMachine.Mods.IsDMC12 && !TimeMachineHandler.CurrentTimeMachine.Properties.AreFlyingCircuitsBroken) || TimeMachineHandler.CurrentTimeMachine.Vehicle.CanHoverTransform());
            installMrFusion.Enabled = active && TimeMachineHandler.CurrentTimeMachine.Mods.Reactor == ReactorType.Nuclear && TimeMachineHandler.CurrentTimeMachine.Mods.IsDMC12;
            restoreCar.Enabled = active && (TimeMachineHandler.CurrentTimeMachine.Constants.FullDamaged || TimeMachineHandler.CurrentTimeMachine.Properties.AreTimeCircuitsBroken);

            buyPlutonium.Enabled = InternalInventory.Current.Plutonium < 5;
            buyPlutonium.AltTitle = $"{InternalInventory.Current.Plutonium}/5";

            customMenu.Enabled = active && !TimeMachineHandler.CurrentTimeMachine.Constants.FullDamaged;

            Recalculate();
        }
    }
}
