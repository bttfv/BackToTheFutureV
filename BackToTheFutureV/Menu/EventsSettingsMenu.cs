using LemonUI.Menus;
using System;
using System.ComponentModel;

namespace BackToTheFutureV
{
    internal class EventsSettingsMenu : BTTFVMenu
    {
        private readonly NativeCheckboxItem LightningStrikeEvent;
        private readonly NativeCheckboxItem EngineStallEvent;
        private readonly NativeCheckboxItem TurbulenceEvent;
        private readonly NativeCheckboxItem TrainEvent;

        public EventsSettingsMenu() : base("Events")
        {
            LightningStrikeEvent = NewCheckboxItem("Lightning", ModSettings.LightningStrikeEvent);
            EngineStallEvent = NewCheckboxItem("Engine", ModSettings.EngineStallEvent);
            TurbulenceEvent = NewCheckboxItem("Turbulence", ModSettings.TurbulenceEvent);
            TrainEvent = NewCheckboxItem("TrainEvent", ModSettings.TrainEvent);
        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {

        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {

        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == LightningStrikeEvent)
            {
                ModSettings.LightningStrikeEvent = Checked;
            }

            if (sender == EngineStallEvent)
            {
                ModSettings.EngineStallEvent = Checked;
            }

            if (sender == TurbulenceEvent)
            {
                ModSettings.TurbulenceEvent = Checked;
            }

            if (sender == TrainEvent)
            {
                ModSettings.TrainEvent = Checked;
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
