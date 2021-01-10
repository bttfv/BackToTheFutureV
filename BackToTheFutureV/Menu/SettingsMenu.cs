using BackToTheFutureV.Utility;
using FusionLibrary;
using GTA;
using LemonUI.Elements;
using LemonUI.Menus;
using System;
using System.Drawing;

namespace BackToTheFutureV.Menu
{
    public class SettingsMenu : CustomNativeMenu
    {
        private NativeCheckboxItem cinematicSpawn;
        private NativeCheckboxItem useInputToggle;        
        private NativeCheckboxItem forceFlyMode;
        private NativeCheckboxItem LandingSystem;
        private NativeCheckboxItem PersistenceSystem;
        private NativeCheckboxItem WaybackSystem;
        private NativeCheckboxItem RandomTrains;
        private NativeCheckboxItem GlowingWormholeEmitter;
        private NativeCheckboxItem GlowingPlutoniumReactor;

        private NativeSubmenuItem SoundsMenu;
        private NativeSubmenuItem EventsMenu;
        private NativeSubmenuItem ControlsMenu;
        private NativeSubmenuItem TCDMenu;

        public SettingsMenu() : base("", Game.GetLocalizedString("BTTFV_Menu_Settings"))
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

            Shown += SettingsMenu_Shown;
            OnItemCheckboxChanged += SettingsMenu_OnItemCheckboxChanged;

            Add(cinematicSpawn = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_CinematicSpawn"), Game.GetLocalizedString("BTTFV_Menu_CinematicSpawn_Description"), ModSettings.CinematicSpawn));
            Add(useInputToggle = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_InputToggle"), Game.GetLocalizedString("BTTFV_Menu_InputToggle_Description"), ModSettings.UseInputToggle));
            Add(forceFlyMode = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_ForceFlyMode"), Game.GetLocalizedString("BTTFV_Menu_ForceFlyMode_Description"), ModSettings.ForceFlyMode));
            Add(LandingSystem = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_LandingSystem"), Game.GetLocalizedString("BTTFV_Menu_LandingSystem_Description"), ModSettings.LandingSystem));
            Add(PersistenceSystem = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_PersistenceSystem"), Game.GetLocalizedString("BTTFV_Menu_PersistenceSystem_Description"), ModSettings.PersistenceSystem));
            Add(WaybackSystem = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_WaybackSystem"), Game.GetLocalizedString("BTTFV_Menu_WaybackSystem_Description"), WaybackMachineHandler.Enabled));
            Add(RandomTrains = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_RandomTrains"), Game.GetLocalizedString("BTTFV_Menu_RandomTrains_Description"), ModSettings.RandomTrains));
            Add(GlowingWormholeEmitter = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_GlowingWormholeEmitter"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_GlowingWormholeEmitter_Description"), ModSettings.GlowingWormholeEmitter));
            Add(GlowingPlutoniumReactor = new NativeCheckboxItem(Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_GlowingPlutoniumReactor"), Game.GetLocalizedString("BTTFV_Menu_TimeMachineMenu_GlowingPlutoniumReactor_Description"), ModSettings.GlowingPlutoniumReactor));

            SoundsMenu = AddSubMenu(MenuHandler.SoundsSettingsMenu);
            SoundsMenu.Title = Game.GetLocalizedString("BTTFV_Menu_SoundsMenu");
            SoundsMenu.Description = Game.GetLocalizedString("BTTFV_Menu_SoundsMenu_Description");

            EventsMenu = AddSubMenu(MenuHandler.EventsSettingsMenu);
            EventsMenu.Title = Game.GetLocalizedString("BTTFV_Menu_EventsMenu");
            EventsMenu.Description = Game.GetLocalizedString("BTTFV_Menu_EventsMenu_Description");

            ControlsMenu = AddSubMenu(MenuHandler.ControlsMenu);
            ControlsMenu.Title = Game.GetLocalizedString("BTTFV_Menu_ControlsMenu");
            ControlsMenu.Description = Game.GetLocalizedString("BTTFV_Menu_ControlsMenu_Description");

            TCDMenu = AddSubMenu(MenuHandler.TCDMenu);
            TCDMenu.Title = Game.GetLocalizedString("BTTFV_Menu_TCDMenu");
            TCDMenu.Description = Game.GetLocalizedString("BTTFV_Menu_TCDMenu_Description");
        }

        private void SettingsMenu_Shown(object sender, EventArgs e)
        {
            if (Utils.RandomTrains != ModSettings.RandomTrains)
            {
                ModSettings.RandomTrains = Utils.RandomTrains;
                ModSettings.SaveSettings();

                RandomTrains.Checked = Utils.RandomTrains;
            }
        }

        private void SettingsMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == cinematicSpawn)
                ModSettings.CinematicSpawn = Checked;

            if (sender == useInputToggle)
                ModSettings.UseInputToggle = Checked;

            if (sender == forceFlyMode)
                ModSettings.ForceFlyMode = Checked;

            if (sender == LandingSystem)
                ModSettings.LandingSystem = Checked;

            if (sender == PersistenceSystem)
                ModSettings.PersistenceSystem = Checked;

            if (sender == RandomTrains)
            {
                ModSettings.RandomTrains = Checked;

                Utils.RandomTrains = Checked;
            }                

            if (sender == GlowingWormholeEmitter)
                ModSettings.GlowingWormholeEmitter = Checked;

            if (sender == GlowingPlutoniumReactor)
                ModSettings.GlowingPlutoniumReactor = Checked;

            if (sender == WaybackSystem)
                WaybackMachineHandler.SetState(Checked);

            ModSettings.SaveSettings();
        }

        public override void Tick()
        {
            
        }
    }
}
