using BackToTheFutureV.TimeMachineClasses;
using FusionLibrary;
using LemonUI.Menus;
using System;

namespace BackToTheFutureV.Menu
{
    internal class PhotoMenu : BTTFVMenu
    {
        private NativeCheckboxItem Wormhole;
        private NativeCheckboxItem Coils;
        private NativeCheckboxItem Ice;
        private NativeCheckboxItem FluxCapacitor;
        private NativeCheckboxItem EngineStall;
        private NativeCheckboxItem SIDMax;
        private NativeItem LightningStrike;
        private NativeSliderItem StrikeDelay;
        private NativeCheckboxItem HideHUD;

        private TimeMachine TimeMachine => TimeMachineHandler.CurrentTimeMachine;

        public PhotoMenu() : base("Photo")
        {
            Shown += PhotoMenu_Shown;
            OnItemCheckboxChanged += PhotoMenu_OnItemCheckboxChanged;
            OnItemActivated += PhotoMenu_OnItemActivated;
            OnItemValueChanged += PhotoMenu_OnItemValueChanged;

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

        private void PhotoMenu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {
            if (sender == StrikeDelay)
                StrikeDelay.Title = $"{GetLocalizedItemTitle("StrikeDelay")}: {StrikeDelay.Value}";
        }

        private void PhotoMenu_OnItemActivated(NativeItem sender, EventArgs e)
        {
            if (sender == LightningStrike)
            {
                TimeMachine.Events.StartLightningStrike?.Invoke(StrikeDelay.Value);
                Close();
            }
        }

        private void PhotoMenu_Shown(object sender, EventArgs e)
        {
            Coils.Enabled = TimeMachine.Mods.IsDMC12;
            Ice.Enabled = TimeMachine.Mods.IsDMC12;
            FluxCapacitor.Enabled = TimeMachine.Mods.IsDMC12;
            EngineStall.Enabled = TimeMachine.Mods.IsDMC12;
            SIDMax.Enabled = TimeMachine.Mods.IsDMC12;

            StrikeDelay.Title = $"{GetLocalizedItemTitle("StrikeDelay")}: {StrikeDelay.Value}";
        }

        private void PhotoMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == Wormhole)
                TimeMachine.Properties.PhotoWormholeActive = Checked;

            if (sender == Coils)
                TimeMachine.Properties.PhotoGlowingCoilsActive = Checked;

            if (sender == Ice)
                TimeMachine.Events.SetFreeze(!TimeMachine.Properties.IsFreezed);

            if (sender == FluxCapacitor)
                TimeMachine.Properties.PhotoFluxCapacitorActive = Checked;

            if (sender == EngineStall)
            {
                TimeMachine.Events.SetEngineStall?.Invoke(Checked);

                TimeMachine.Properties.PhotoEngineStallActive = Checked;
            }

            if (sender == SIDMax)
                TimeMachine.Properties.PhotoSIDMaxActive = Checked;

            if (sender == HideHUD)
                Utils.HideGUI = Checked;
        }

        public override void Tick()
        {
            if (TimeMachineHandler.CurrentTimeMachine == null)
            {
                Close();

                return;
            }

            Wormhole.Checked = TimeMachine.Properties.PhotoWormholeActive;
            Coils.Checked = TimeMachine.Properties.PhotoGlowingCoilsActive;
            Ice.Checked = TimeMachine.Properties.IsFreezed;
            FluxCapacitor.Checked = TimeMachine.Properties.PhotoFluxCapacitorActive;
            EngineStall.Checked = TimeMachine.Properties.IsEngineStalling;
            SIDMax.Checked = TimeMachine.Properties.PhotoSIDMaxActive;

            LightningStrike.Enabled = !TimeMachine.Properties.IsPhotoModeOn;

            HideHUD.Checked = Utils.HideGUI;
        }
    }
}
