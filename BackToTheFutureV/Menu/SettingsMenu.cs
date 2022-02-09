using FusionLibrary;
using LemonUI.Menus;
using System;
using System.ComponentModel;

namespace BackToTheFutureV
{
    internal class SettingsMenu : BTTFVMenu
    {
        private readonly NativeCheckboxItem cinematicSpawn;
        private readonly NativeCheckboxItem useInputToggle;
        private readonly NativeCheckboxItem forceFlyMode;
        private readonly NativeCheckboxItem LandingSystem;
        private readonly NativeCheckboxItem InfiniteFuel;
        private readonly NativeCheckboxItem PersistenceSystem;
        private readonly NativeCheckboxItem WaybackSystem;
        private readonly NativeCheckboxItem RandomTrains;
        private readonly NativeCheckboxItem RealTime;
        private readonly NativeCheckboxItem GlowingWormholeEmitter;
        private readonly NativeCheckboxItem GlowingPlutoniumReactor;

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
            RealTime = NewCheckboxItem("RealTime", ModSettings.RealTime);
            GlowingWormholeEmitter = NewCheckboxItem("GlowingWormhole", ModSettings.GlowingWormholeEmitter);
            GlowingPlutoniumReactor = NewCheckboxItem("GlowingReactor", ModSettings.GlowingPlutoniumReactor);

            NewSubmenu(MenuHandler.SoundsSettingsMenu);

            NewSubmenu(MenuHandler.EventsSettingsMenu);

            NewSubmenu(MenuHandler.ControlsMenu);

            NewSubmenu(MenuHandler.TCDMenu);
        }

        public override void Menu_Shown(object sender, EventArgs e)
        {
            if (FusionUtils.RandomTrains != ModSettings.RandomTrains)
            {
                ModSettings.RandomTrains = FusionUtils.RandomTrains;
                ModSettings.SaveSettings();

                RandomTrains.Checked = FusionUtils.RandomTrains;
            }
        }

        public override void Menu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == cinematicSpawn)
            {
                ModSettings.CinematicSpawn = Checked;
            }

            if (sender == useInputToggle)
            {
                ModSettings.UseInputToggle = Checked;
            }

            if (sender == forceFlyMode)
            {
                ModSettings.ForceFlyMode = Checked;
            }

            if (sender == LandingSystem)
            {
                ModSettings.LandingSystem = Checked;
            }

            if (sender == PersistenceSystem)
            {
                ModSettings.PersistenceSystem = Checked;
            }

            if (sender == RandomTrains)
            {
                ModSettings.RandomTrains = Checked;

                FusionUtils.RandomTrains = Checked;
            }

            if (sender == RealTime)
            {
                ModSettings.RealTime = Checked;

                TimeHandler.RealTime = Checked;
            }

            if (sender == GlowingWormholeEmitter)
            {
                ModSettings.GlowingWormholeEmitter = Checked;
            }

            if (sender == GlowingPlutoniumReactor)
            {
                ModSettings.GlowingPlutoniumReactor = Checked;
            }

            if (sender == InfiniteFuel)
            {
                ModSettings.InfiniteFuel = Checked;
            }

            if (sender == WaybackSystem)
            {
                ModSettings.WaybackSystem = Checked;

                if (!Checked)
                {
                    BackToTheFutureV.WaybackSystem.Abort();
                }
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
