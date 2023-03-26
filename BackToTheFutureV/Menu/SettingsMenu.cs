using FusionLibrary;
using LemonUI.Menus;
using System;
using System.ComponentModel;

namespace BackToTheFutureV
{
    internal class SettingsMenu : BTTFVMenu
    {
        private readonly NativeCheckboxItem cinematicSpawn;
        private readonly NativeCheckboxItem cutsceneMode;
        private readonly NativeCheckboxItem useInputToggle;
        private readonly NativeCheckboxItem InfiniteFuel;
        private readonly NativeCheckboxItem WaybackSystem;
        private readonly NativeCheckboxItem TimeParadox;
        private readonly NativeCheckboxItem RandomTrains;
        private readonly NativeCheckboxItem RealTime;
        private readonly NativeCheckboxItem YearTraffic;
        private readonly NativeCheckboxItem GlowingWormholeEmitter;
        private readonly NativeCheckboxItem GlowingPlutoniumReactor;

        public SettingsMenu() : base("Settings")
        {
            cinematicSpawn = NewCheckboxItem("CinematicSpawn", ModSettings.CinematicSpawn);
            cutsceneMode = NewCheckboxItem("Cutscene", ModSettings.CutsceneMode);
            useInputToggle = NewCheckboxItem("InputToggle", ModSettings.UseInputToggle);
            InfiniteFuel = NewCheckboxItem("InfinityReactor", ModSettings.InfiniteFuel);
            WaybackSystem = NewCheckboxItem("Wayback", ModSettings.WaybackSystem);
            TimeParadox = NewCheckboxItem("TimeParadox", ModSettings.TimeParadox);
            RandomTrains = NewCheckboxItem("RandomTrains", ModSettings.RandomTrains);
            RealTime = NewCheckboxItem("RealTime", ModSettings.RealTime);
            YearTraffic = NewCheckboxItem("YearTraffic", ModSettings.YearTraffic);
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
            switch (sender)
            {
                case NativeCheckboxItem _ when sender == cinematicSpawn:
                    ModSettings.CinematicSpawn = Checked;
                    break;
                case NativeCheckboxItem _ when sender == cutsceneMode:
                    ModSettings.CutsceneMode = Checked;
                    break;
                case NativeCheckboxItem _ when sender == useInputToggle:
                    ModSettings.UseInputToggle = Checked;
                    break;
                case NativeCheckboxItem _ when sender == RandomTrains:
                    ModSettings.RandomTrains = Checked;
                    FusionUtils.RandomTrains = Checked;
                    break;
                case NativeCheckboxItem _ when sender == RealTime:
                    ModSettings.RealTime = Checked;
                    TimeHandler.RealTime = Checked;
                    break;
                case NativeCheckboxItem _ when sender == YearTraffic:
                    ModSettings.YearTraffic = Checked;
                    TimeHandler.TrafficVolumeYearBased = Checked;
                    break;
                case NativeCheckboxItem _ when sender == TimeParadox:
                    ModSettings.TimeParadox = Checked;
                    break;
                case NativeCheckboxItem _ when sender == GlowingWormholeEmitter:
                    ModSettings.GlowingWormholeEmitter = Checked;
                    break;
                case NativeCheckboxItem _ when sender == GlowingPlutoniumReactor:
                    ModSettings.GlowingPlutoniumReactor = Checked;
                    break;
                case NativeCheckboxItem _ when sender == InfiniteFuel:
                    ModSettings.InfiniteFuel = Checked;
                    break;
                case NativeCheckboxItem _ when sender == WaybackSystem:
                    ModSettings.WaybackSystem = Checked;
                    if (!Checked)
                    {
                        BackToTheFutureV.WaybackSystem.Abort();
                    }
                    break;
            }

            ModSettings.SaveSettings();
        }

        public override void Tick()
        {
            cutsceneMode.Checked = ModSettings.CutsceneMode;
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
