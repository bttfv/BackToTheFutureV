using FusionLibrary;
using LemonUI.Menus;
using System;

namespace BackToTheFutureV
{
    internal class SettingsMenu : BTTFVMenu
    {
        private NativeCheckboxItem cinematicSpawn;
        private NativeCheckboxItem useInputToggle;
        private NativeCheckboxItem forceFlyMode;
        private NativeCheckboxItem LandingSystem;
        private NativeCheckboxItem InfiniteFuel;
        private NativeCheckboxItem PersistenceSystem;
        private NativeCheckboxItem WaybackSystem;
        private NativeCheckboxItem RandomTrains;
        private NativeCheckboxItem GlowingWormholeEmitter;
        private NativeCheckboxItem GlowingPlutoniumReactor;

        private NativeSubmenuItem SoundsMenu;
        private NativeSubmenuItem EventsMenu;
        private NativeSubmenuItem ControlsMenu;
        private NativeSubmenuItem TCDMenu;

        public SettingsMenu() : base("Settings")
        {
            Shown += SettingsMenu_Shown;
            OnItemCheckboxChanged += SettingsMenu_OnItemCheckboxChanged;

            cinematicSpawn = NewCheckboxItem("CinematicSpawn", ModSettings.CinematicSpawn);
            useInputToggle = NewCheckboxItem("InputToggle", ModSettings.UseInputToggle);
            forceFlyMode = NewCheckboxItem("ForceFly", ModSettings.ForceFlyMode);
            LandingSystem = NewCheckboxItem("LandingSystem", ModSettings.LandingSystem);
            InfiniteFuel = NewCheckboxItem("InfinityReactor", ModSettings.InfiniteFuel);
            PersistenceSystem = NewCheckboxItem("Persistence", ModSettings.PersistenceSystem);
            WaybackSystem = NewCheckboxItem("Wayback", WaybackMachineHandler.Enabled);
            RandomTrains = NewCheckboxItem("RandomTrains", ModSettings.RandomTrains);
            GlowingWormholeEmitter = NewCheckboxItem("GlowingWormhole", ModSettings.GlowingWormholeEmitter);
            GlowingPlutoniumReactor = NewCheckboxItem("GlowingReactor", ModSettings.GlowingPlutoniumReactor);

            SoundsMenu = NewSubmenu(MenuHandler.SoundsSettingsMenu, "Sounds");

            EventsMenu = NewSubmenu(MenuHandler.EventsSettingsMenu, "Events");

            ControlsMenu = NewSubmenu(MenuHandler.ControlsMenu, "Controls");

            TCDMenu = NewSubmenu(MenuHandler.TCDMenu, "TCD");
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

            if (sender == InfiniteFuel)
                ModSettings.InfiniteFuel = Checked;

            if (sender == WaybackSystem)
                WaybackMachineHandler.SetState(Checked);

            ModSettings.SaveSettings();
        }

        public override void Tick()
        {

        }
    }
}
