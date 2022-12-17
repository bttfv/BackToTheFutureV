using GTA;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal delegate void OnGUIChange();

    internal class ModSettings
    {
        private static readonly Version LastCompatibleVersion = new Version(2, 4, 0, 0);
        public static OnGUIChange OnGUIChange { get; set; }

        public static PointF RCGUIPosition { get; set; } = new PointF(0.901f, 0.879f);
        public static float RCGUIScale { get; set; } = 0.15f;

        public static PointF SIDPosition { get; set; } = new PointF(0.947f, 0.437f);
        public static float SIDScale { get; set; } = 0.3f;
        public static bool HideSID { get; set; } = false;
        public static PointF TCDPosition { get; set; } = new PointF(0.88f, 0.75f);
        public static float TCDScale { get; set; } = 0.3f;
        public static TCDBackground TCDBackground { get; set; } = TCDBackground.BTTF1;
        public static bool HideIngameTCDToggle { get; set; } = false;
        public static bool PlayFluxCapacitorSound { get; set; } = true;
        public static bool PlayDiodeBeep { get; set; } = true;
        public static bool PlaySpeedoBeep { get; set; } = true;
        public static bool PlayEngineSounds { get; set; } = true;
        public static bool CinematicSpawn { get; set; } = true;
        public static bool UseInputToggle { get; set; } = false;
        public static bool GlowingWormholeEmitter { get; set; } = true;
        public static bool GlowingPlutoniumReactor { get; set; } = true;
        public static bool InfiniteFuel { get; set; } = false;
        public static bool LightningStrikeEvent { get; set; } = true;
        public static bool EngineStallEvent { get; set; } = true;
        public static bool TurbulenceEvent { get; set; } = true;
        public static bool TerroristsEvent { get; set; } = true;
        public static bool TrainEvent { get; set; } = true;
        public static bool LandingSystem { get; set; } = true;
        public static bool RandomTrains { get; set; } = true;
        public static bool WaybackSystem { get; set; } = false;
        public static bool RealTime { get; set; } = true;
        public static bool YearTraffic { get; set; } = false;
        public static bool TimeParadox { get; set; } = false;
        //public static bool DeluxoProto { get; set; } = true;
        public static int MaxRecordedMachines { get; set; } = 10;

        private static ScriptSettings settings;
        private static readonly CultureInfo info = CultureInfo.CreateSpecificCulture("en-US");

        public static void LoadSettings()
        {
            string path = "./scripts/BackToTheFutureV/settings.ini";

            settings = ScriptSettings.Load(path);

            string savedStringVersion = settings.GetValue<string>("General", "Version", default);

            Version savedVersion = savedStringVersion == default ? null : new Version(savedStringVersion);

            if (savedStringVersion == default || savedVersion < LastCompatibleVersion)
            {
                RemoteTimeMachineHandler.DeleteAll();
                TimeMachineClone.DeleteAll();

                File.Delete(path);

                settings = ScriptSettings.Load(path);

                settings.SetValue("General", "Version", Main.Version);

                SaveSettings();

                ModControls.LoadControls(settings);

                return;
            }
            else if (savedVersion != Main.Version)
            {
                settings.SetValue("General", "Version", Main.Version);
            }

            RCGUIPosition = new PointF(float.Parse(settings.GetValue("RCGUI", "PositionX", RCGUIPosition.X.ToString("G", info)), info), float.Parse(settings.GetValue("RCGUI", "PositionY", RCGUIPosition.Y.ToString("G", info)), info));
            RCGUIScale = float.Parse(settings.GetValue("RCGUI", "Scale", RCGUIScale.ToString("G", info)), info);

            SIDPosition = new PointF(float.Parse(settings.GetValue("SID", "PositionX", SIDPosition.X.ToString("G", info)), info), float.Parse(settings.GetValue("SID", "PositionY", SIDPosition.Y.ToString("G", info)), info));
            SIDScale = float.Parse(settings.GetValue("SID", "Scale", SIDScale.ToString("G", info)), info);
            HideSID = settings.GetValue("SID", "HideSID", HideSID);

            TCDScale = float.Parse(settings.GetValue("TimeCircuits", "Scale", TCDScale.ToString("G", info)), info);
            TCDPosition = new PointF(float.Parse(settings.GetValue("TimeCircuits", "PositionX", TCDPosition.X.ToString("G", info)), info), float.Parse(settings.GetValue("TimeCircuits", "PositionY", TCDPosition.Y.ToString("G", info)), info));
            TCDBackground = (TCDBackground)Enum.Parse(typeof(TCDBackground), settings.GetValue("TimeCircuits", "Background", "BTTF1"));
            UseInputToggle = settings.GetValue("TimeCircuits", "InputMode", UseInputToggle);
            HideIngameTCDToggle = settings.GetValue("TimeCircuits", "HideIngameTCDToggle", HideIngameTCDToggle);
            GlowingWormholeEmitter = settings.GetValue("TimeCircuits", "GlowingWormholeEmitter", GlowingWormholeEmitter);
            GlowingPlutoniumReactor = settings.GetValue("TimeCircuits", "GlowingPlutoniumReactor", GlowingPlutoniumReactor);

            PlayFluxCapacitorSound = settings.GetValue("Sounds", "FluxCapacitor", PlayFluxCapacitorSound);
            PlayDiodeBeep = settings.GetValue("Sounds", "DiodeBeep", PlayDiodeBeep);
            PlayEngineSounds = settings.GetValue("Sounds", "CustomEngine", PlayEngineSounds);
            PlaySpeedoBeep = settings.GetValue("Sounds", "SpeedoBeep", PlaySpeedoBeep);

            CinematicSpawn = settings.GetValue("General", "CinematicSpawn", CinematicSpawn);
            InfiniteFuel = settings.GetValue("General", "InfiniteFuel", InfiniteFuel);
            WaybackSystem = settings.GetValue("General", "WaybackSystem", WaybackSystem);
            TimeParadox = settings.GetValue("General", "TimeParadox", TimeParadox);
            RandomTrains = settings.GetValue("General", "RandomTrains", RandomTrains);
            RealTime = settings.GetValue("General", "RealTime", RealTime);
            YearTraffic = settings.GetValue("General", "YearTraffic", YearTraffic);
            //DeluxoProto = settings.GetValue("General", "DeluxoProto", DeluxoProto);
            MaxRecordedMachines = settings.GetValue("General", "MaxRemote", MaxRecordedMachines);

            LandingSystem = settings.GetValue("Hover", "LandingSystem", LandingSystem);

            LightningStrikeEvent = settings.GetValue("Events", "LightningStrike", LightningStrikeEvent);
            EngineStallEvent = settings.GetValue("Events", "EngineStall", EngineStallEvent);
            TurbulenceEvent = settings.GetValue("Events", "Turbulence", TurbulenceEvent);
            TerroristsEvent = settings.GetValue("Events", "Terrorists", TerroristsEvent);
            TrainEvent = settings.GetValue("Events", "Train", TrainEvent);

            ModControls.LoadControls(settings);

            SaveSettings();

            OnGUIChange?.Invoke();
        }

        public static void SaveSettings()
        {
            settings.SetValue("RCGUI", "PositionX", RCGUIPosition.X.ToString("G", info));
            settings.SetValue("RCGUI", "PositionY", RCGUIPosition.Y.ToString("G", info));
            settings.SetValue("RCGUI", "Scale", RCGUIScale.ToString("G", info));

            settings.SetValue("SID", "PositionX", SIDPosition.X.ToString("G", info));
            settings.SetValue("SID", "PositionY", SIDPosition.Y.ToString("G", info));
            settings.SetValue("SID", "Scale", SIDScale.ToString("G", info));
            settings.SetValue("SID", "HideSID", HideSID);

            settings.SetValue("TimeCircuits", "Scale", TCDScale.ToString("G", info));
            settings.SetValue("TimeCircuits", "PositionX", TCDPosition.X.ToString("G", info));
            settings.SetValue("TimeCircuits", "PositionY", TCDPosition.Y.ToString("G", info));
            settings.SetValue("TimeCircuits", "Background", TCDBackground.ToString());
            settings.SetValue("TimeCircuits", "InputMode", UseInputToggle);
            settings.SetValue("TimeCircuits", "HideIngameTCDToggle", HideIngameTCDToggle);
            settings.SetValue("TimeCircuits", "GlowingWormholeEmitter", GlowingWormholeEmitter);
            settings.SetValue("TimeCircuits", "GlowingPlutoniumReactor", GlowingPlutoniumReactor);

            settings.SetValue("Sounds", "FluxCapacitor", PlayFluxCapacitorSound);
            settings.SetValue("Sounds", "DiodeBeep", PlayDiodeBeep);
            settings.SetValue("Sounds", "CustomEngine", PlayEngineSounds);
            settings.SetValue("Sounds", "SpeedoBeep", PlaySpeedoBeep);

            settings.SetValue("General", "WaybackSystem", WaybackSystem);
            settings.SetValue("General", "TimeParadox", TimeParadox);
            settings.SetValue("General", "InfiniteFuel", InfiniteFuel);
            settings.SetValue("General", "CinematicSpawn", CinematicSpawn);
            settings.SetValue("General", "RandomTrains", RandomTrains);
            settings.SetValue("General", "RealTime", RealTime);
            settings.SetValue("General", "YearTraffic", YearTraffic);
            //settings.SetValue("General", "DeluxoProto", DeluxoProto);
            settings.SetValue("General", "MaxRemote", MaxRecordedMachines);

            settings.SetValue("Hover", "LandingSystem", LandingSystem);

            settings.SetValue("Events", "LightningStrike", LightningStrikeEvent);
            settings.SetValue("Events", "EngineStall", EngineStallEvent);
            settings.SetValue("Events", "Turbulence", TurbulenceEvent);
            settings.SetValue("Events", "Terrorists", TerroristsEvent);
            settings.SetValue("Events", "Train", TrainEvent);

            settings.Save();

            ModControls.SaveControls(settings);
        }
    }
}
