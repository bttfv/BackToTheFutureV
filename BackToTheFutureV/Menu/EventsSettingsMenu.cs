using LemonUI.Menus;
using System;

namespace BackToTheFutureV
{
    internal class EventsSettingsMenu : BTTFVMenu
    {
        private NativeCheckboxItem LightningStrikeEvent;
        private NativeCheckboxItem EngineStallEvent;
        private NativeCheckboxItem TurbulenceEvent;

        public EventsSettingsMenu() : base("Events")
        {
            Shown += SettingsMenu_Shown;
            OnItemCheckboxChanged += SettingsMenu_OnItemCheckboxChanged;

            LightningStrikeEvent = NewCheckboxItem("Lightning", ModSettings.LightningStrikeEvent);
            EngineStallEvent = NewCheckboxItem("Engine", ModSettings.EngineStallEvent);
            TurbulenceEvent = NewCheckboxItem("Turbulence", ModSettings.TurbulenceEvent);
        }

        private void SettingsMenu_Shown(object sender, EventArgs e)
        {

        }

        private void SettingsMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == LightningStrikeEvent)
                ModSettings.LightningStrikeEvent = Checked;

            if (sender == EngineStallEvent)
                ModSettings.EngineStallEvent = Checked;

            if (sender == TurbulenceEvent)
                ModSettings.TurbulenceEvent = Checked;

            ModSettings.SaveSettings();
        }

        public override void Tick()
        {

        }
    }
}
