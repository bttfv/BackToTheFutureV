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
            switch (sender)
            {
                case NativeCheckboxItem item when item == LightningStrikeEvent:
                    ModSettings.LightningStrikeEvent = Checked;
                    break;
                case NativeCheckboxItem item when item == EngineStallEvent:
                    ModSettings.EngineStallEvent = Checked;
                    break;
                case NativeCheckboxItem item when item == TurbulenceEvent:
                    ModSettings.TurbulenceEvent = Checked;
                    break;
                case NativeCheckboxItem item when item == TrainEvent:
                    ModSettings.TrainEvent = Checked;
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
