using BackToTheFutureV.TimeMachineClasses;
using FusionLibrary;
using GTA;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.Drawing;

namespace BackToTheFutureV.Menu
{
    public class PhotoMenu : CustomNativeMenu
    {
        private NativeCheckboxItem Wormhole;
        private NativeCheckboxItem Coils;
        private NativeCheckboxItem Ice;
        //private NativeCheckboxItem FluxCapacitor;
        private NativeCheckboxItem EngineStall;
        private NativeItem LightningStrike;
        private NativeSliderItem StrikeDelay;
        private NativeCheckboxItem HideHUD;

        private TimeMachine TimeMachine => TimeMachineHandler.CurrentTimeMachine;

        public PhotoMenu() : base("", Game.GetLocalizedString("BTTFV_Menu_PhotoMenu"))
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

            Shown += PhotoMenu_Shown;
            OnItemCheckboxChanged += PhotoMenu_OnItemCheckboxChanged;
            OnItemActivated += PhotoMenu_OnItemActivated;
            OnItemValueChanged += PhotoMenu_OnItemValueChanged;
                
            Add(Wormhole = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Wormhole"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Wormhole_Description")));
            Add(Coils = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Coils"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Coils_Description")));
            Add(Ice = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Ice"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_Ice_Description")));
            //Add(FluxCapacitor = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_FluxCapacitor"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_FluxCapacitor_Description")));
            Add(EngineStall = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_EngineStall"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_EngineStall_Description")));
            Add(LightningStrike = new NativeItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_LightningStrike"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_LightningStrike_Description")));
            Add(StrikeDelay = new NativeSliderItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_StrikeDelay"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_StrikeDelay_Description"), 60, 3));
            Add(HideHUD = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_HideHUD"), Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_HideHUD_Description")));
        }

        private void PhotoMenu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {
            if (sender == StrikeDelay)
                StrikeDelay.Title = $"{Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_StrikeDelay")}: {StrikeDelay.Value}";
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
            //FluxCapacitor.Enabled = TimeMachine.Mods.IsDMC12;
            EngineStall.Enabled = TimeMachine.Mods.IsDMC12;

            StrikeDelay.Title = $"{Game.GetLocalizedString("BTTFV_Menu_PhotoMenu_StrikeDelay")}: {StrikeDelay.Value}";
        }

        private void PhotoMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == Wormhole)
                TimeMachine.Properties.PhotoWormholeActive = Checked;

            if (sender == Coils)
                TimeMachine.Properties.PhotoGlowingCoilsActive = Checked;

            if (sender == Ice)
                TimeMachine.Events.SetFreeze(!TimeMachine.Properties.IsFreezed);

            //if (sender == FluxCapacitor)
            //    TimeMachine.Properties.PhotoFluxCapacitorActive = Checked;

            if (sender == EngineStall)
                TimeMachine.Properties.PhotoEngineStallActive = Checked;

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
            //FluxCapacitor.Checked = TimeMachine.Properties.PhotoFluxCapacitorActive;
            EngineStall.Checked = TimeMachine.Properties.IsEngineStalling;
            LightningStrike.Enabled = !TimeMachine.Properties.IsPhotoModeOn;

            HideHUD.Checked = Utils.HideGUI;
        }
    }
}
