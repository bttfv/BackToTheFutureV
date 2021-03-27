using LemonUI.Menus;
using System;

namespace BackToTheFutureV.Menu
{
    internal class SoundsSettingsMenu : BTTFVMenu
    {
        private NativeCheckboxItem playFluxCapacitorSound;
        private NativeCheckboxItem playDiodeSound;
        private NativeCheckboxItem playSpeedoBeep;
        private NativeCheckboxItem playEngineSounds;

        public SoundsSettingsMenu() : base("Sounds")
        {
            Shown += SettingsMenu_Shown;
            OnItemCheckboxChanged += SettingsMenu_OnItemCheckboxChanged;

            playFluxCapacitorSound = NewCheckboxItem("FluxCapacitor", ModSettings.PlayFluxCapacitorSound);
            playDiodeSound = NewCheckboxItem("CircuitsBeep", ModSettings.PlayDiodeBeep);
            playSpeedoBeep = NewCheckboxItem("SpeedoBeep", ModSettings.PlaySpeedoBeep);
            playEngineSounds = NewCheckboxItem("Engine", ModSettings.PlayEngineSounds);
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
