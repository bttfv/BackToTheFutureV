using FusionLibrary;
using LemonUI.Menus;
using System;
using System.ComponentModel;

namespace BackToTheFutureV
{
    internal class PhotoMenu : BTTFVMenu
    {
        private readonly NativeSubmenuItem OverrideMenu;
        private readonly NativeCheckboxItem Wormhole;
        private readonly NativeCheckboxItem Coils;
        private readonly NativeCheckboxItem Ice;
        private readonly NativeCheckboxItem FluxCapacitor;
        private readonly NativeCheckboxItem EngineStall;
        private readonly NativeCheckboxItem SIDMax;
        private readonly NativeItem LightningStrike;
        private readonly NativeSliderItem StrikeDelay;
        private readonly NativeCheckboxItem HideHUD;


        public PhotoMenu() : base("Photo")
        {
            OverrideMenu = NewSubmenu(MenuHandler.OverrideMenu, "Override");
            Wormhole = NewCheckboxItem("Wormhole");
            Coils = NewCheckboxItem("Coils");
            Ice = NewCheckboxItem("Ice");
            FluxCapacitor = NewCheckboxItem("FluxCapacitor");
            SIDMax = NewCheckboxItem("SIDMax");
            EngineStall = NewCheckboxItem("Engine");
            LightningStrike = NewItem("LightningStrike");
            StrikeDelay = NewSliderItem("StrikeDelay", 60, 3);
            HideHUD = NewCheckboxItem("HideHUD");
        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {
            if (sender == StrikeDelay)
            {
                StrikeDelay.Title = $"{GetItemTitle("StrikeDelay")}: {StrikeDelay.Value}";
            }
        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == LightningStrike)
            {
                CurrentTimeMachine.Events.StartLightningStrike?.Invoke(StrikeDelay.Value);
                Visible = false;
            }
        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            Coils.Enabled = CurrentTimeMachine.Mods.IsDMC12;
            Ice.Enabled = CurrentTimeMachine.Mods.IsDMC12;
            FluxCapacitor.Enabled = CurrentTimeMachine.Mods.IsDMC12;
            EngineStall.Enabled = CurrentTimeMachine.Mods.IsDMC12;
            SIDMax.Enabled = CurrentTimeMachine.Mods.IsDMC12;

            StrikeDelay.Title = $"{GetItemTitle("StrikeDelay")}: {StrikeDelay.Value}";
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == Wormhole)
            {
                CurrentTimeMachine.Properties.PhotoWormholeActive = Checked;
            }

            if (sender == Coils)
            {
                CurrentTimeMachine.Properties.PhotoGlowingCoilsActive = Checked;
            }

            if (sender == Ice)
            {
                CurrentTimeMachine.Events.SetFreeze(!CurrentTimeMachine.Properties.IsFreezed);
            }

            if (sender == FluxCapacitor)
            {
                CurrentTimeMachine.Properties.PhotoFluxCapacitorActive = Checked;
            }

            if (sender == EngineStall)
            {
                CurrentTimeMachine.Events.SetEngineStall?.Invoke(Checked);

                CurrentTimeMachine.Properties.PhotoEngineStallActive = Checked;
            }

            if (sender == SIDMax)
            {
                CurrentTimeMachine.Properties.PhotoSIDMaxActive = Checked;
            }

            if (sender == HideHUD)
            {
                FusionUtils.HideGUI = Checked;
            }
        }

        public override void Tick()
        {
            if (CurrentTimeMachine == null)
            {
                Visible = false;

                return;
            }

            Wormhole.Checked = CurrentTimeMachine.Properties.PhotoWormholeActive;
            Coils.Checked = CurrentTimeMachine.Properties.PhotoGlowingCoilsActive;
            Ice.Checked = CurrentTimeMachine.Properties.IsFreezed;
            FluxCapacitor.Checked = CurrentTimeMachine.Properties.PhotoFluxCapacitorActive;
            EngineStall.Checked = CurrentTimeMachine.Properties.IsEngineStalling;
            SIDMax.Checked = CurrentTimeMachine.Properties.PhotoSIDMaxActive;

            LightningStrike.Enabled = !CurrentTimeMachine.Properties.IsPhotoModeOn;

            HideHUD.Checked = FusionUtils.HideGUI;
        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {

        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {

        }
    }
}
