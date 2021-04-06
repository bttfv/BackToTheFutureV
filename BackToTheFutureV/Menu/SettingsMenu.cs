using FusionLibrary;
using LemonUI.Menus;
using System;
using System.ComponentModel;

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

        public SettingsMenu() : base("Settings")
        {
            cinematicSpawn = NewCheckboxItem("CinematicSpawn", ModSettings.CinematicSpawn);
            useInputToggle = NewCheckboxItem("InputToggle", ModSettings.UseInputToggle);
            forceFlyMode = NewCheckboxItem("ForceFly", ModSettings.ForceFlyMode);
            LandingSystem = NewCheckboxItem("LandingSystem", ModSettings.LandingSystem);
            InfiniteFuel = NewCheckboxItem("InfinityReactor", ModSettings.InfiniteFuel);
            PersistenceSystem = NewCheckboxItem("Persistence", ModSettings.PersistenceSystem);
            WaybackSystem = NewCheckboxItem("Wayback", ModSettings.WaybackSystem);
            RandomTrains = NewCheckboxItem("RandomTrains", ModSettings.RandomTrains);
            GlowingWormholeEmitter = NewCheckboxItem("GlowingWormhole", ModSettings.GlowingWormholeEmitter);
            GlowingPlutoniumReactor = NewCheckboxItem("GlowingReactor", ModSettings.GlowingPlutoniumReactor);

            NewSubmenu(MenuHandler.SoundsSettingsMenu, "Sounds");

            NewSubmenu(MenuHandler.EventsSettingsMenu, "Events");

            NewSubmenu(MenuHandler.ControlsMenu, "Controls");

            NewSubmenu(MenuHandler.TCDMenu, "TCD");
        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            if (Utils.RandomTrains != ModSettings.RandomTrains)
            {
                ModSettings.RandomTrains = Utils.RandomTrains;
                ModSettings.SaveSettings();

                RandomTrains.Checked = Utils.RandomTrains;
            }
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
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
            {
                ModSettings.WaybackSystem = Checked;

                if (!Checked)
                    BackToTheFutureV.WaybackSystem.Abort();
            }

            ModSettings.SaveSettings();
        }

        public override void Tick()
        {

        }

        public override void Menu_OnItemValueChanged(NativeSliderItem sender, EventArgs e)
        {

        }

        public override void Menu_OnItemSelected(NativeItem sender, SelectedEventArgs e)
        {

        }

        public override void Menu_OnItemActivated(NativeItem sender, EventArgs e)
        {

        }

        public override void Menu_Closing(object sender, CancelEventArgs e)
        {

        }
    }
}
