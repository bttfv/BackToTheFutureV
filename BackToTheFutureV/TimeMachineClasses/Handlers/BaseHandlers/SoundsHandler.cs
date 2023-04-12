using GTA;
using GTA.Native;
using KlangRageAudioLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BackToTheFutureV
{
    internal class DoorInfo
    {
        public bool IsDoorFullyOpen;
        public bool IsDoorOpen;
    }

    internal class SoundsHandler : HandlerPrimitive
    {
        public AudioEngine AudioEngine { get; }

        //Hover Mode
        public AudioPlayer HoverModeOn;
        public AudioPlayer HoverModeOff;
        public AudioPlayer HoverModeUp;
        public AudioPlayer HoverModeBoost;

        //Ice
        public AudioPlayer Ice;
        public AudioPlayer IceVents;

        //Fuel
        public AudioPlayer FuelEmpty;
        public AudioPlayer PlutoniumRefuel;

        //Lighting strike
        public AudioPlayer LightningStrike;
        public AudioPlayer Thunder;

        //Plutonium gauge
        public AudioPlayer PlutoniumGauge;

        //RC
        public AudioPlayer RCOn;
        public AudioPlayer RCOff;
        public AudioPlayer RCBrake;
        public AudioPlayer RCAcceleration;
        public AudioPlayer RCSomeSerious;
        public List<AudioPlayer> RCSounds;

        //Car sounds
        private readonly AudioPlayer _doorOpenSound;
        private readonly AudioPlayer _doorCloseSound;
        private readonly AudioPlayer _doorOpenColdSound;
        private readonly AudioPlayer _doorCloseColdsound;
        private readonly AudioPlayer _engineOnSound;
        private readonly AudioPlayer _engineOffsound;
        private readonly List<AudioPlayer> _doorSounds;

        //Clock
        public AudioPlayer Alarm;

        private bool _engineOn;

        private int _gameTimer;

        private readonly Dictionary<VehicleDoorIndex, DoorInfo> _doorStatus = new Dictionary<VehicleDoorIndex, DoorInfo>()
        {
            {
                VehicleDoorIndex.FrontLeftDoor,
                new DoorInfo()
            },
            {
                VehicleDoorIndex.FrontRightDoor,
                new DoorInfo()
            }
        };

        //Speedo
        public AudioPlayer Speedo;

        //Engine restarter
        public AudioPlayer EngineRestarter;
        public AudioPlayer HeadHorn;

        //Sparks
        public AudioPlayer Sparks;
        public AudioPlayer SparksEmpty;
        public AudioPlayer SparkStabilized;
        public AudioPlayer DiodesGlowing;
        public AudioPlayer WormholeInterrupted;

        //Time travel
        public AudioPlayer TimeTravelCutscene;
        public AudioPlayer TimeTravelInstant;

        //Reenter
        public AudioPlayer Reenter;

        //TCD Input
        public AudioPlayer InputEnter;
        public AudioPlayer InputEnterError;
        public Dictionary<int, AudioPlayer> Keypad;
        public AudioPlayer InputOn;
        public AudioPlayer InputOff;

        //TCD
        public AudioPlayer FluxCapacitor;
        public AudioPlayer TCDBeep;
        public AudioPlayer TCDGlitch;

        //Plate
        public AudioPlayer Plate;

        public SoundsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            AudioEngine = new AudioEngine
            {
                BaseSoundFolder = "BackToTheFutureV\\Sounds",
                DefaultSourceEntity = Vehicle
            };

            Events.OnWormholeTypeChanged += OnWormholeTypeChanged;

            OnWormholeTypeChanged();

            //Hover Mode
            HoverModeOn = AudioEngine.Create("bttf2/hover/toHover.wav", Presets.Exterior);
            HoverModeOff = AudioEngine.Create("bttf2/hover/toRegular.wav", Presets.Exterior);
            HoverModeUp = AudioEngine.Create("bttf2/hover/hoverUp.wav", Presets.Exterior);
            HoverModeBoost = AudioEngine.Create("bttf2/hover/boost.wav", Presets.Exterior);

            //RC
            RCOn = AudioEngine.Create("general/rc/on.wav", Presets.Exterior);
            RCOff = AudioEngine.Create("general/rc/off.wav", Presets.Exterior);
            RCBrake = AudioEngine.Create("general/rc/brake.wav", Presets.Exterior);
            RCAcceleration = AudioEngine.Create("general/rc/acceleration.wav", Presets.Exterior);
            RCSomeSerious = AudioEngine.Create("general/rc/someSerious.wav", Presets.Exterior);

            RCSounds = new List<AudioPlayer>
            {
                RCOn, RCOff, RCBrake, RCAcceleration, RCSomeSerious
            };

            foreach (AudioPlayer sound in RCSounds)
            {
                sound.Volume = 0.4f;
                sound.MinimumDistance = 1f;
            }

            //Speedo
            Speedo = AudioEngine.Create("general/speedoTick.wav", Presets.Interior);
            Speedo.Volume = 1f;
            Speedo.MinimumDistance = 0.3f;
            Speedo.SourceBone = "bttf_speedo";

            //Sparks
            SparksEmpty = AudioEngine.Create("general/timeTravel/sparksNoFuel.wav", Presets.ExteriorLoud);
            DiodesGlowing = AudioEngine.Create("general/timeCircuits/SID.wav", Presets.Interior);
            WormholeInterrupted = AudioEngine.Create("general/timeTravel/sparkInterrupt.wav", Presets.ExteriorLoud);
            SparkStabilized = AudioEngine.Create("bttf3/timeTravel/sparksStabilization.wav", Presets.ExteriorLoud);

            //TCD Input
            InputEnter = AudioEngine.Create("general/timeCircuits/tfcEnter.wav", Presets.Interior);
            InputEnterError = AudioEngine.Create("general/timeCircuits/tfcError.wav", Presets.Interior);

            InputEnter.SourceBone = "bttf_tcd";
            InputEnterError.SourceBone = "bttf_tcd";

            Keypad = new Dictionary<int, AudioPlayer>();

            for (int i = 0; i <= 9; i++)
            {
                Keypad[i] = AudioEngine.Create("general/keypad/" + i + ".wav", Presets.Interior);
            }

            foreach (KeyValuePair<int, AudioPlayer> keypad in Keypad)
            {
                keypad.Value.Volume = 0.45f;
                keypad.Value.SourceBone = "bttf_tcd";
            }

            InputOn = AudioEngine.Create("general/timeCircuits/tfcOn.wav", Presets.Interior);
            InputOff = AudioEngine.Create("general/timeCircuits/tfcOff.wav", Presets.Interior);

            InputOn.SourceBone = "bttf_tcd";
            InputOff.SourceBone = "bttf_tcd";

            //TCD
            TCDBeep = AudioEngine.Create("general/timeCircuits/beep.wav", Presets.Interior);
            TCDBeep.MinimumDistance = 0.3f;
            TCDBeep.SourceBone = "bttf_tcd";

            //TCD Glitch
            TCDGlitch = AudioEngine.Create("bttf2/timeCircuits/glitch.wav", Presets.Interior);
            TCDGlitch.SourceBone = "bttf_tcd";

            //Ligtning strike
            LightningStrike = AudioEngine.Create("bttf2/timeTravel/lightingStrike.wav", Presets.ExteriorLoud);
            Thunder = AudioEngine.Create("general/thunder.wav", Presets.ExteriorLoud);

            if (!Mods.IsDMC12)
            {
                return;
            }

            //Ice
            Ice = AudioEngine.Create("general/cold.wav", Presets.ExteriorLoop);
            Ice.FadeOutMultiplier = 0.15f;
            IceVents = AudioEngine.Create("general/vents.wav", Presets.Exterior);

            //Fuel
            FuelEmpty = AudioEngine.Create("bttf1/timeCircuits/plutoniumEmpty.wav", Presets.Interior);
            FuelEmpty.SourceBone = "bttf_tcd";
            PlutoniumRefuel = AudioEngine.Create("bttf1/plutonium_refuel.wav", Presets.Exterior);
            PlutoniumRefuel.SourceBone = "bttf_reactorcap";

            //Plutonium gauge
            PlutoniumGauge = AudioEngine.Create("bttf1/timeCircuits/plutoniumGauges.wav", Presets.Interior);
            PlutoniumGauge.SourceBone = "bttf_tcd";

            //Car sounds
            _doorOpenSound = AudioEngine.Create("general/doorOpen.wav", Presets.Exterior);
            _doorCloseSound = AudioEngine.Create("general/doorClose.wav", Presets.Exterior);
            _doorOpenColdSound = AudioEngine.Create("general/doorOpenCold.wav", Presets.Exterior);
            _doorCloseColdsound = AudioEngine.Create("general/doorCloseCold.wav", Presets.Exterior);
            _engineOffsound = AudioEngine.Create("general/engine/engineStop.wav", Presets.Exterior);
            _engineOnSound = AudioEngine.Create("general/engine/engineStart.wav", Presets.Exterior);

            _engineOffsound.SourceBone = "engine";
            _engineOnSound.SourceBone = "engine";

            _doorSounds = new List<AudioPlayer>
            {
                _doorOpenSound, _doorCloseSound, _doorOpenColdSound, _doorCloseColdsound
            };

            //Engine restarter
            EngineRestarter = AudioEngine.Create("bttf1/engine/restart.wav", Presets.ExteriorLoudLoop);
            EngineRestarter.SourceBone = "engine";
            EngineRestarter.FadeOutMultiplier = 6f;
            EngineRestarter.FadeInMultiplier = 4f;
            EngineRestarter.MinimumDistance = 6f;

            HeadHorn = AudioEngine.Create("general/horn.wav", Presets.Exterior);
            HeadHorn.Volume = 0.5f;

            //Flux capacitor            
            FluxCapacitor = AudioEngine.Create("general/fluxCapacitor.wav", Presets.InteriorLoop);
            FluxCapacitor.Volume = 0.1f;
            FluxCapacitor.MinimumDistance = 0.5f;
            FluxCapacitor.SourceBone = "flux_capacitor";

            //Plate
            Plate = AudioEngine.Create("general/plate.wav", Presets.ExteriorLoud);

            //Alarm
            Alarm = AudioEngine.Create("general/alarm.wav", Presets.InteriorLoop);
            Alarm.SourceBone = "bulova_clock";
            Alarm.Volume = 1.2f;
            Alarm.StartFadeIn = false;
            Alarm.StopFadeOut = false;
        }

        public void OnWormholeTypeChanged()
        {
            TimeTravelCutscene?.Dispose();
            TimeTravelCutscene = AudioEngine.Create($"{Constants.LowerWormholeType}/timeTravel/mode/cutscene.wav", Presets.ExteriorLoud);

            TimeTravelInstant?.Dispose();
            TimeTravelInstant = AudioEngine.Create($"{Constants.LowerWormholeType}/timeTravel/mode/instant.wav", Presets.ExteriorLoud);

            Reenter?.Dispose();
            Reenter = AudioEngine.Create($"{Constants.LowerWormholeType}/timeTravel/reentry.wav", Presets.ExteriorLoud);

            Sparks?.Dispose();
            Sparks = AudioEngine.Create($"{Constants.LowerWormholeType}/timeTravel/sparks.wav", Presets.ExteriorLoudLoop);
            Sparks.FadeOutMultiplier = 2f;
            Sparks.StartFadeIn = false;
        }

        public override void Dispose()
        {
            //Ice
            Ice?.Dispose();
            IceVents?.Dispose();

            //Fuel
            FuelEmpty?.Dispose();
            PlutoniumRefuel?.Dispose();

            //Lightning strike
            LightningStrike?.Dispose();

            //Plutonium gauge
            PlutoniumGauge?.Dispose();

            //RC
            RCOn?.Dispose();
            RCOff?.Dispose();
            RCBrake?.Dispose();
            RCAcceleration?.Dispose();
            RCSomeSerious?.Dispose();

            //Car sounds
            _doorOpenSound?.Dispose();
            _doorCloseSound?.Dispose();
            _doorOpenColdSound?.Dispose();
            _doorCloseColdsound?.Dispose();
            _engineOnSound?.Dispose();
            _engineOffsound?.Dispose();

            //Engine restarter
            EngineRestarter?.Dispose();
            HeadHorn?.Dispose();

            //Sparks
            Sparks?.Dispose();
            SparkStabilized?.Dispose();
            DiodesGlowing?.Dispose();

            //TCD Handler
            InputEnter?.Dispose();
            InputEnterError?.Dispose();
            Keypad?.Values.ToList().ForEach(x => x?.Dispose());
            InputOn?.Dispose();
            InputOff?.Dispose();

            //TCD
            TCDBeep?.Dispose();
            FluxCapacitor?.Dispose();
            TCDGlitch?.Dispose();

            //Time travel
            TimeTravelCutscene?.Dispose();
            TimeTravelInstant?.Dispose();
            Reenter?.Dispose();

            //Plate
            Plate?.Dispose();

            //Alarm
            Alarm?.Dispose();
        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        public override void Tick()
        {
            if (Game.GameTime < _gameTimer || !Vehicle.IsVisible || !Mods.IsDMC12 || GTA.UI.Screen.IsFadedOut)
            {
                return;
            }

            foreach (KeyValuePair<VehicleDoorIndex, DoorInfo> door in _doorStatus.ToList())
            {
                float doorAngle = Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, Vehicle.Handle, (int)door.Key);

                // Detect door index (d -> driver side) (p - passenger side) for correct 3d sound
                string doorSide = door.Key == VehicleDoorIndex.FrontLeftDoor ? "d" : "p";
                string doorBone = $"handle_{doorSide}side_f";

                _doorSounds.ForEach(x => x.SourceBone = doorBone);
                if (doorAngle > 0 && !door.Value.IsDoorOpen)
                {
                    if (!Properties.IsFreezed)
                    {
                        _doorOpenSound.Play();
                    }
                    else
                    {
                        _doorOpenColdSound.Play();
                    }

                    _doorStatus[door.Key].IsDoorOpen = true;
                }
                else if (doorAngle == 0)
                {
                    _doorStatus[door.Key].IsDoorOpen = false;
                }

                if (doorAngle > 0.87f)
                {
                    _doorStatus[door.Key].IsDoorFullyOpen = true;
                }
                else if (doorAngle < 0.87f && door.Value.IsDoorFullyOpen)
                {
                    if (!Properties.IsFreezed)
                    {
                        _doorCloseSound.Play();
                    }
                    else
                    {
                        _doorCloseColdsound.Play();
                    }

                    _doorStatus[door.Key].IsDoorFullyOpen = false;
                }
            }

            if (Vehicle.IsEngineRunning && !_engineOn)
            {
                if (!(GTA.UI.Screen.IsFadingIn || GTA.UI.Screen.IsFadingOut))
                {
                    _engineOffsound.Stop();
                    _engineOnSound.Play();
                }
                _engineOn = true;
            }
            else if (!Vehicle.IsEngineRunning && _engineOn)
            {
                if (!(GTA.UI.Screen.IsFadingIn || GTA.UI.Screen.IsFadingOut))
                {
                    _engineOnSound.Stop();
                    _engineOffsound.Play();
                }
                _engineOn = false;
            }

            _gameTimer = Game.GameTime + 50;
        }

        public override void Stop()
        {
            _doorCloseSound.Stop();
            _doorOpenSound.Stop();
            _doorCloseColdsound.Stop();
            _doorOpenColdSound.Stop();
            _engineOffsound.Stop();
            _engineOnSound.Stop();
        }
    }
}
