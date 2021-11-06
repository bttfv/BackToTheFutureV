using FusionLibrary;
using LemonUI.Menus;
using System;
using System.ComponentModel;

namespace BackToTheFutureV
{
    internal class PhotoMenu : BTTFVMenu
    {
        private readonly NativeCheckboxItem Wormhole;
        private readonly NativeCheckboxItem Coils;
        private readonly NativeCheckboxItem Ice;
        private readonly NativeCheckboxItem FluxCapacitor;
        private readonly NativeCheckboxItem EngineStall;
        private readonly NativeCheckboxItem SIDMax;
        private readonly NativeItem LightningStrike;
        private readonly NativeSliderItem StrikeDelay;
        private readonly NativeCheckboxItem HideHUD;

        private TimeMachine TimeMachine => TimeMachineHandler.CurrentTimeMachine;

        public PhotoMenu() : base("Photo")
        {
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
                TimeMachine.Events.StartLightningStrike?.Invoke(StrikeDelay.Value);
                Visible = false;
            }
        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            Coils.Enabled = TimeMachine.Mods.IsDMC12;
            Ice.Enabled = TimeMachine.Mods.IsDMC12;
            FluxCapacitor.Enabled = TimeMachine.Mods.IsDMC12;
            EngineStall.Enabled = TimeMachine.Mods.IsDMC12;
            SIDMax.Enabled = TimeMachine.Mods.IsDMC12;

            StrikeDelay.Title = $"{GetItemTitle("StrikeDelay")}: {StrikeDelay.Value}";
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == Wormhole)
            {
                TimeMachine.Properties.PhotoWormholeActive = Checked;
            }

            if (sender == Coils)
            {
                TimeMachine.Properties.PhotoGlowingCoilsActive = Checked;
            }

            if (sender == Ice)
            {
                TimeMachine.Events.SetFreeze(!TimeMachine.Properties.IsFreezed);
            }

            if (sender == FluxCapacitor)
            {
                TimeMachine.Properties.PhotoFluxCapacitorActive = Checked;
            }

            if (sender == EngineStall)
            {
                TimeMachine.Events.SetEngineStall?.Invoke(Checked);

                TimeMachine.Properties.PhotoEngineStallActive = Checked;
            }

            if (sender == SIDMax)
            {
                TimeMachine.Properties.PhotoSIDMaxActive = Checked;
            }

            if (sender == HideHUD)
            {
                FusionUtils.HideGUI = Checked;
            }
        }

        public override void Tick()
        {
            if (TimeMachineHandler.CurrentTimeMachine == null)
            {
                Visible = false;

                return;
            }

            Wormhole.Checked = TimeMachine.Properties.PhotoWormholeActive;
            Coils.Checked = TimeMachine.Properties.PhotoGlowingCoilsActive;
            Ice.Checked = TimeMachine.Properties.IsFreezed;
            FluxCapacitor.Checked = TimeMachine.Properties.PhotoFluxCapacitorActive;
            EngineStall.Checked = TimeMachine.Properties.IsEngineStalling;
            SIDMax.Checked = TimeMachine.Properties.PhotoSIDMaxActive;

            LightningStrike.Enabled = !TimeMachine.Properties.IsPhotoModeOn;

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
