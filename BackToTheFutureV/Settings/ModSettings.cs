using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using GTA;
using GTA.Math;
using System.Globalization;
using BackToTheFutureV.GUI;

namespace BackToTheFutureV
{
    public delegate void OnGUIChange();

    public class ModSettings
    {
        public static PointF TCDPosition { get; set; } = new PointF(0.88f, 0.75f);
        public static float TCDScale { get; set; } = 0.3f;
        public static TCDBackground TCDBackground { get; set; } = TCDBackground.Metal;

        public static OnGUIChange OnGUIChange { get; set; }

        public static bool PlayFluxCapacitorSound { get; set; }
        public static bool PlayDiodeBeep { get; set; }
        public static bool PlaySpeedoBeep { get; set; }
        public static bool PlayEngineSounds { get; set; }
        public static bool CinematicSpawn { get; set; }
        public static bool UseInputToggle { get; set; }
        public static bool ForceFlyMode { get; set; }
        public static bool GlowingWormholeEmitter { get; set; }
        public static bool GlowingPlutoniumReactor { get; set; }
        public static bool LightningStrikeEvent { get; set; }
        public static bool EngineStallEvent { get; set; }
        public static bool TurbulenceEvent { get; set; }
        public static bool LandingSystem { get; set; }
        public static bool PersistenceSystem { get; set; }

        public static void LoadSettings()
        {
            string path = "./scripts/BackToTheFutureV/settings.ini";

            if (!File.Exists(path))
                File.Create(path);

            ScriptSettings settings = ScriptSettings.Load(path);
            CultureInfo info = CultureInfo.CreateSpecificCulture("en-US");

            TCDScale = float.Parse(settings.GetValue("tcd", "scale", TCDScale.ToString("G", info)), info);
            TCDPosition = new PointF(float.Parse(settings.GetValue("tcd", "position_x", TCDPosition.X.ToString("G", info)), info), float.Parse(settings.GetValue("tcd", "position_y", TCDPosition.Y.ToString("G", info)), info));
            TCDBackground = (TCDBackground)Enum.Parse(typeof(TCDBackground), settings.GetValue("tcd", "background", "Metal"));

            OnGUIChange?.Invoke();

            PlayFluxCapacitorSound = settings.GetValue("time_circuits", "PlayFluxCapacitorSound", true);

            PlayDiodeBeep = settings.GetValue("time_circuits", "play_diode_sound", true);
            UseInputToggle = settings.GetValue("time_circuits", "use_input_toggle", true);

            PlayEngineSounds = settings.GetValue("vehicle", "PlayEngineSounds", true);

            CinematicSpawn = settings.GetValue("vehicle", "CinematicSpawn", true);

            PlaySpeedoBeep = settings.GetValue("speedo", "play_speedo_beep", true);

            ForceFlyMode = settings.GetValue("fly_mode", "force_fly_mode", true);

            LandingSystem = settings.GetValue("fly_mode", "LandingSystem", true);

            PersistenceSystem = settings.GetValue("general", "PersistenceSystem", true);

            GlowingWormholeEmitter = settings.GetValue("time_circuits", "GlowingWormholeEmitter", true);
            GlowingPlutoniumReactor = settings.GetValue("time_circuits", "GlowingPlutoniumReactor", true);

            LightningStrikeEvent = settings.GetValue("events", "LightningStrikeEvent", true);
            EngineStallEvent = settings.GetValue("events", "EngineStallEvent", true);
            TurbulenceEvent = settings.GetValue("events", "TurbulenceEvent", true);

            SaveSettings();
        }

        public static void SaveSettings()
        {
            ScriptSettings settings = ScriptSettings.Load("./scripts/BackToTheFutureV/settings.ini");
            CultureInfo info = CultureInfo.CreateSpecificCulture("en-US");

            settings.SetValue("tcd", "position_x", TCDPosition.X.ToString("G", info));
            settings.SetValue("tcd", "position_y", TCDPosition.Y.ToString("G", info));
            settings.SetValue("tcd", "scale", TCDScale.ToString("G", info));
            settings.SetValue("tcd", "background", TCDBackground.ToString());

            settings.SetValue("general", "PersistenceSystem", PersistenceSystem);

            settings.SetValue("time_circuits", "PlayFluxCapacitorSound", PlayFluxCapacitorSound);

            settings.SetValue("time_circuits", "play_diode_sound", PlayDiodeBeep);
            settings.SetValue("time_circuits", "use_input_toggle", UseInputToggle);

            settings.SetValue("speedo", "play_speedo_beep", PlaySpeedoBeep);

            settings.SetValue("vehicle", "PlayEngineSounds", PlayEngineSounds);

            settings.SetValue("vehicle", "CinematicSpawn", CinematicSpawn);

            settings.SetValue("fly_mode", "force_fly_mode", ForceFlyMode);
            settings.SetValue("fly_mode", "LandingSystem", LandingSystem);

            settings.SetValue("time_circuits", "GlowingWormholeEmitter", GlowingWormholeEmitter);
            settings.SetValue("time_circuits", "GlowingPlutoniumReactor", GlowingPlutoniumReactor);

            settings.SetValue("events", "LightningStrikeEvent", LightningStrikeEvent);
            settings.SetValue("events", "EngineStallEvent", EngineStallEvent);
            settings.SetValue("events", "TurbulenceEvent", TurbulenceEvent);

            settings.Save();
        }
    }
}
