using FusionLibrary;
using GTA;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.Drawing;

namespace BackToTheFutureV.Menu
{
    public class SoundsSettingsMenu : CustomNativeMenu
    {
        private NativeCheckboxItem playFluxCapacitorSound;
        private NativeCheckboxItem playDiodeSound;
        private NativeCheckboxItem playSpeedoBeep;
        private NativeCheckboxItem playEngineSounds;

        public SoundsSettingsMenu() : base("", Game.GetLocalizedString("BTTFV_Menu_SoundsMenu"))
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

            Shown += SettingsMenu_Shown;
            OnItemCheckboxChanged += SettingsMenu_OnItemCheckboxChanged;

            Add(playFluxCapacitorSound = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_FluxCapacitorSound"), Game.GetLocalizedString("BTTFV_Menu_FluxCapacitorSound_Description"), ModSettings.PlayFluxCapacitorSound));
            Add(playDiodeSound = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_CircuitsBeep"), Game.GetLocalizedString("BTTFV_Menu_CircuitsBeep_Description"), ModSettings.PlayDiodeBeep));
            Add(playSpeedoBeep = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_SpeedoBeep"), Game.GetLocalizedString("BTTFV_Menu_SpeedoBeep_Description"), ModSettings.PlaySpeedoBeep));
            Add(playEngineSounds = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_EngineSounds"), Game.GetLocalizedString("BTTFV_Menu_EngineSounds_Description"), ModSettings.PlayEngineSounds));
        }

        private void SettingsMenu_Shown(object sender, EventArgs e)
        {
            
        }

        private void SettingsMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == playFluxCapacitorSound)
                ModSettings.PlayFluxCapacitorSound = Checked;

            if (sender == playDiodeSound)
                ModSettings.PlayDiodeBeep = Checked;

            if (sender == playSpeedoBeep)
                ModSettings.PlaySpeedoBeep = Checked;

            if (sender == playEngineSounds)
                ModSettings.PlayEngineSounds = Checked;

            ModSettings.SaveSettings();
        }

        public override void Tick()
        {
            
        }
    }
}
