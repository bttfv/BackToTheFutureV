using FusionLibrary;
using GTA;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.Drawing;

namespace BackToTheFutureV.Menu
{
    public class EventsSettingsMenu : CustomNativeMenu
    {
        private NativeCheckboxItem LightningStrikeEvent;
        private NativeCheckboxItem EngineStallEvent;
        private NativeCheckboxItem TurbulenceEvent;

        public EventsSettingsMenu() : base("", Game.GetLocalizedString("BTTFV_Menu_EventsMenu"))
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");
            
            Shown += SettingsMenu_Shown;
            OnItemCheckboxChanged += SettingsMenu_OnItemCheckboxChanged;

            Add(LightningStrikeEvent = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_LightningStrikeEvent"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_LightningStrikeEvent_Description"), ModSettings.LightningStrikeEvent));
            Add(EngineStallEvent = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_EngineStallEvent"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_EngineStallEvent_Description"), ModSettings.EngineStallEvent));
            Add(TurbulenceEvent = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_TurbolenceEvent"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_TurbolenceEvent_Description"), ModSettings.TurbulenceEvent));            
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
