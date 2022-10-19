using LemonUI.Menus;
using System;
using System.ComponentModel;

namespace BackToTheFutureV
{
    internal class SoundsSettingsMenu : BTTFVMenu
    {
        private readonly NativeCheckboxItem playFluxCapacitorSound;
        private readonly NativeCheckboxItem playDiodeSound;
        private readonly NativeCheckboxItem playSpeedoBeep;
        private readonly NativeCheckboxItem playEngineSounds;

        public SoundsSettingsMenu() : base("Sounds")
        {
            playFluxCapacitorSound = NewCheckboxItem("FluxCapacitor", ModSettings.PlayFluxCapacitorSound);
            playDiodeSound = NewCheckboxItem("CircuitsBeep", ModSettings.PlayDiodeBeep);
            playSpeedoBeep = NewCheckboxItem("SpeedoBeep", ModSettings.PlaySpeedoBeep);
            playEngineSounds = NewCheckboxItem("Engine", ModSettings.PlayEngineSounds);
        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {

        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {

        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            switch (sender)
            {
                case NativeCheckboxItem _ when sender == playFluxCapacitorSound:
                    ModSettings.PlayFluxCapacitorSound = Checked;
                    break;
                case NativeCheckboxItem _ when sender == playDiodeSound:
                    ModSettings.PlayDiodeBeep = Checked;
                    break;
                case NativeCheckboxItem _ when sender == playSpeedoBeep:
                    ModSettings.PlaySpeedoBeep = Checked;
                    break;
                case NativeCheckboxItem _ when sender == playEngineSounds:
                    ModSettings.PlayEngineSounds = Checked;
                    break;
            }

            ModSettings.SaveSettings();
        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {

        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }

        public override void Menu_Shown(object sender, EventArgs e)
        {

        }

        public override void Tick()
        {

        }
    }
}
