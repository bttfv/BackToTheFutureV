using BackToTheFutureV.Delorean.Handlers;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Utility;
using GTA;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BackToTheFutureV.Settings;
using KlangRageAudioLibrary;

namespace BackToTheFutureV.Delorean
{
    public delegate void OnDestinationDateChange();
    public delegate void OnTimeCircuitsToggle();
    public delegate void OnTimeCircuitsToggleComplete();
    public delegate void OnScaleformPriority();
    public delegate void OnEnteredDelorean();
    public delegate void OnTimeTravel();
    public delegate void OnTimeTravelComplete();
    public delegate void OnReentryComplete();

    public class TimeCircuits
    {
        /// <summary>
        /// The Audio Engine of the Time Circuits.
        /// </summary>
        public AudioEngine AudioEngine { get; }

        /// <summary>
        /// The current state of the Time Circuits.
        /// </summary>
        public bool IsOn { get; set; }

        /// <summary>
        /// The Destination Time of the Time Circuits.
        /// </summary>
        public DateTime DestinationTime { get; set; } = new DateTime(2015, 10, 25, 14, 00, 00);

        /// <summary>
        /// The Previous Time of the Time Circuits.
        /// </summary>
        public DateTime PreviousTime { get; set; } = new DateTime(1985, 10, 25, 00, 21, 00);

        /// <summary>
        /// The actual <seealso cref="DMC12"/> of this Time Circuits.
        /// </summary>
        public DeloreanTimeMachine Delorean { get; }

        /// <summary>
        /// The MPH speed of the <seealso cref="DMC12"/>
        /// </summary>
        public float MphSpeed { get => Delorean.MPHSpeed; set => Delorean.MPHSpeed = value; }

        /// <summary>
        /// The vehicle object of the <see cref="DMC12"/>
        /// </summary>
        public Vehicle Vehicle => Delorean.Vehicle;

        /// <summary>
        /// The <seealso cref="BackToTheFutureV.Delorean.DeloreanType"/> of this <seealso cref="DMC12"/>
        /// </summary>
        public DeloreanType DeloreanType => Delorean.DeloreanType;

        /// <summary>
        /// String representing the lower case <seealso cref="BackToTheFutureV.Delorean.DeloreanType"/> of this <seealso cref="DMC12"/>
        /// </summary>
        public string LowerCaseDeloreanType => Delorean.LowerCaseDeloreanType;

        /// <summary>
        /// Whether this Delorean is remote controlled.
        /// </summary>
        public bool IsRemoteControlled { get; set; }

        /// <summary>
        /// Whether this Delorean is on rail tracks.
        /// </summary>
        public bool IsOnTracks { get; set; }

        /// <summary>
        /// Whether this Delorean was on rail tracks.
        /// </summary>
        public bool WasOnTracks { get; set; }

        /// <summary>
        /// Whether this Delorean is attached to Rogers Sierra.
        /// </summary>
        public bool IsAttachedToRogersSierra { get; set; }

        /// <summary>
        /// Whether the Time Circuits are fueled or not.
        /// </summary>
        public bool IsFueled { get; set; } = true;

        /// <summary>
        /// Whether the Time Machine is going through the freezing process
        /// </summary>
        public bool IsFreezing { get; set; }

        /// <summary>
        /// Whether the Time Machine is in hover mode
        /// </summary>
        public bool IsFlying => GetHandler<FlyingHandler>().IsFlying;

        /// <summary>
        /// Whether the boost is active
        /// </summary>
        public bool IsBoosting => GetHandler<FlyingHandler>().IsBoosting;

        /// <summary>
        /// Whether the ice particle spawned around Delorean
        /// </summary>
        public bool IcePlaying { get; set; }

        /// <summary>
        /// Whether delorean is time traveling.
        /// </summary>
        public bool IsTimeTraveling => GetHandler<TimeTravelHandler>().IsTimeTravelling;

        /// <summary>
        /// Whether Delorean is reentering.
        /// </summary>
        public bool IsReentering => GetHandler<ReentryHandler>().IsReentering;

        /// <summary>
        /// State of hoodbox control tubes.
        /// </summary>
        public bool IsWarmedUp => GetHandler<HoodboxHandler>().IsWarmedUp;

        /// <summary>
        /// Whether Delorean was struck by lighting.
        /// </summary>
        public bool HasBeenStruckByLightning => GetHandler<LightningStrikeHandler>().HasBeenStruckByLightning;

        /// <summary>
        /// Delegate called when the destination date is input.
        /// </summary>
        public OnDestinationDateChange OnDestinationDateChange { get; set; }

        /// <summary>
        /// Delegate called when the Time Circuits' state is toggled.
        /// </summary>
        public OnTimeCircuitsToggle OnTimeCircuitsToggle { get; set; }

        /// <summary>
        /// Delegate called when the Player gets close to this Delorean, giving it priority and updating all the values.
        /// </summary>
        public OnScaleformPriority OnScaleformPriority { get; set; }

        /// <summary>
        /// Delegate called when the Player enters this Delorean.
        /// </summary>wa
        public OnEnteredDelorean OnEnteredDelorean { get; set; }

        /// <summary>
        /// Called when the Delorean time travels.
        /// </summary>
        public OnTimeTravel OnTimeTravel { get; set; }

        /// <summary>
        /// Called when the Delorean time travel sequence is complete.
        /// </summary>
        public OnTimeTravelComplete OnTimeTravelComplete { get; set; }

        /// <summary>
        /// Called when the Delorean reenters from time travel.
        /// </summary>
        public OnReentryComplete OnReentryComplete { get; set; }

        public TimeCircuitsScaleform Gui { get; private set; }
        
        private bool showGui = true;

        private readonly Dictionary<string, Handler> registeredHandlers = new Dictionary<string, Handler>();

        private readonly AudioPlayer inputOnTimeCircuits;
        private readonly AudioPlayer inputOff;

        public TimeCircuits(DeloreanTimeMachine delorean)
        {
            AudioEngine = new AudioEngine
            {
                BaseSoundFolder = "BackToTheFutureV\\Sounds",
                DefaultSourceEntity = delorean
            };

            Delorean = delorean;

            inputOnTimeCircuits = AudioEngine.Create("general/timeCircuits/tfcOn.wav", Presets.Interior);
            inputOff = AudioEngine.Create("general/timeCircuits/tfcOff.wav", Presets.Interior);

            inputOnTimeCircuits.SourceBone = "bttf_tcd_green";
            inputOff.SourceBone = "bttf_tcd_green";

            //inputOff.Volume = 2f;
            //inputOnTimeCircuits.Volume = 2f;

            Gui = new TimeCircuitsScaleform("bttf_2d_gui");

            RegisterHandlers();
        }

        /// <summary>
        /// Returns a <seealso cref="Handler"/> based on the type provided.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetHandler<T>()
        {
            if (registeredHandlers.TryGetValue(typeof(T).Name, out Handler handler))
            {
                return (T)(object)handler;
            }

            return default(T);
        }

        /// <summary>
        /// Stops all handlers attached to this Time Circuits object.
        /// </summary>
        public void StopAllHandlers()
        {
            foreach (var handler in registeredHandlers.Values)
            {
                handler.Stop();
            }
        }

        /// <summary>
        /// Disposes all the handlers attached to this Time Circuits object, 
        /// as well as disposing other related resources.
        /// </summary>
        public void DisposeAllHandlers()
        {
            foreach (var handler in registeredHandlers.Values)
            {
                handler.Dispose();
            }

            inputOnTimeCircuits.Dispose();
            inputOff.Dispose();            
        }

        /// <summary>
        /// Called on every game update.
        /// </summary>
        public void Tick()
        {
            foreach (var entry in registeredHandlers)
            {
                entry.Value.Process();
            }

            if (Main.HideGui || Main.PlayerVehicle != Delorean || !Delorean.IsGivenScaleformPriority ||
                Utils.IsPlayerUseFirstPerson() || TcdEditer.IsEditing) 
                return;

            Gui.SetBackground(ModSettings.TCDBackground);
            Gui.Render2D(ModSettings.TCDPosition, new SizeF(ModSettings.TCDScale * (1501f / 1100f) / GTA.UI.Screen.AspectRatio, ModSettings.TCDScale));
        }

        /// <summary>
        /// Called when a key is pressed.
        /// </summary>
        /// <param name="key">Key that is pressed.</param>
        public void KeyDown(Keys key)
        {
            foreach (var entry in registeredHandlers)
            {
                entry.Value.KeyPress(key);
            }

            if (Main.PlayerVehicle != Vehicle) return;

            if (key == Keys.Add && !IsRemoteControlled)
            {
                SetTimeCircuitsOn(!IsOn);
            }

            if (key == Keys.N)
            {
                showGui = !showGui;
            }
        }

        public void SetTimeCircuitsOn(bool on)
        {
            if (IsTimeTraveling | IsReentering | TcdEditer.IsEditing)
                return;

            if (!IsOn && Delorean.Mods.Hoodbox == ModState.On && !IsWarmedUp)
            {
                if (!TcdEditer.IsEditing)
                    Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Hoodbox_Warmup_TfcError"));
                return;
            }

            if (!IsOn && HasBeenStruckByLightning)
            {
                if (!TcdEditer.IsEditing)
                    Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Chip_Damaged"));
                return;
            }

            if(GetHandler<TCDHandler>().IsDoingTimedVisible)
                return;

            IsOn = on;

            if (IsOn)
            {
                inputOnTimeCircuits.Play();
            }                
            else
            {
                inputOff.Play();
            }
                
            OnTimeCircuitsToggle?.Invoke();
        }

        public void DisposeAndRemoveHandler<T>()
        {
            if (!registeredHandlers.TryGetValue(typeof(T).Name, out var handler)) 
                return;

            handler.Dispose();
            registeredHandlers.Remove(typeof(T).Name);
        }

        private void RegisterHandlers()
        {
            registeredHandlers.Add("SpeedoHandler", new SpeedoHandler(this));
            registeredHandlers.Add("TimeTravelHandler", new TimeTravelHandler(this));
            registeredHandlers.Add("FluxCapacitorHandler", new FluxCapacitorHandler(this));
            registeredHandlers.Add("SoundHandler", new SoundHandler(this));
            registeredHandlers.Add("FreezeHandler", new FreezeHandler(this));
            registeredHandlers.Add("TCDHandler", new TCDHandler(this));
            registeredHandlers.Add("PlutoniumGaugeHandler", new PlutoniumGaugeHandler(this));
            registeredHandlers.Add("TFCHandler", new TFCHandler(this));
            registeredHandlers.Add("InputHandler", new InputHandler(this));            
            registeredHandlers.Add("RcHandler", new RcHandler(this));
            registeredHandlers.Add("FuelHandler", new FuelHandler(this));
            registeredHandlers.Add("ReentryHandler", new ReentryHandler(this));
            registeredHandlers.Add("RailroadHandler", new RailroadHandler(this));
            registeredHandlers.Add("StarterHandler", new StarterHandler(this));
            registeredHandlers.Add("TimeCircuitsErrorHandler", new TimeCircuitsErrorHandler(this));
            registeredHandlers.Add("HookHandler", new HookHandler(this));
            registeredHandlers.Add("HoodboxHandler", new HoodboxHandler(this));
            registeredHandlers.Add("SparksHandler", new SparksHandler(this));
            registeredHandlers.Add("FlyingHandler", new FlyingHandler(this));
            registeredHandlers.Add("LightningStrikeHandler", new LightningStrikeHandler(this));
            registeredHandlers.Add("EngineHandler", new EngineHandler(this));
            //registeredHandlers.Add("CoilsIndicatorHandler", new CoilsIndicatorHandler(this));
        }
    }
}