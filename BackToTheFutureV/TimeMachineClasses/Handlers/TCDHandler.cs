using BackToTheFutureV.GUI;
using BackToTheFutureV.Settings;
using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using BackToTheFutureV.Vehicles;
using FusionLibrary;
using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using static FusionLibrary.Enums;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class TCDSlot
    {
        private static Dictionary<string, Vector3> offsets = new Dictionary<string, Vector3>()
        {
            { "red", new Vector3(-0.01477456f, 0.3175744f, 0.6455771f) },
            { "yellow", new Vector3(-0.01964803f, 0.2769623f, 0.565388f) },
            { "green", new Vector3(-0.01737539f, 0.2979541f, 0.6045464f) }
        };

        private static Dictionary<string, TCDRowScaleform> TCDRowsScaleforms = new Dictionary<string, TCDRowScaleform>()
        {
            { "red", new TCDRowScaleform("red") { DrawInPauseMenu = true } },
            { "yellow", new TCDRowScaleform("yellow") { DrawInPauseMenu = true } },
            { "green", new TCDRowScaleform("green") { DrawInPauseMenu = true } }
        };

        public RenderTarget RenderTarget { get; private set; }       
        public TimeMachine TimeMachine { get; private set; }

        public string SlotType { get; private set; }

        private DateTime date;

        public bool IsDoingTimedVisible { get; private set; }
        private bool toggle;
        private int showPropsAt;
        private int showMonthAt;

        private AnimateProp amProp;
        private AnimateProp pmProp;

        public TCDSlot(string slotType, TimeMachine timeMachine)
        {
            SlotType = slotType;
            TimeMachine = timeMachine;

            if (TimeMachine.Mods.IsDMC12)
            {                
                RenderTarget = new RenderTarget(new Model("bttf_3d_row_" + slotType), "bttf_tcd_row_" + slotType, TimeMachine.Vehicle, offsets[slotType], new Vector3(355.9951f, 0.04288517f, 352.7451f));
                RenderTarget.CreateProp();
             
                amProp = new AnimateProp(TimeMachine.Vehicle, new Model($"bttf_{slotType}_am"), Vector3.Zero, Vector3.Zero);
                pmProp = new AnimateProp(TimeMachine.Vehicle, new Model($"bttf_{slotType}_pm"), Vector3.Zero, Vector3.Zero);

                RenderTarget.OnRenderTargetDraw += OnRenderTargetDraw;
            }

            date = new DateTime();
        }

        public void SetDate(DateTime dateToSet)
        {
            if (!TcdEditer.IsEditing)
            {
                ScaleformsHandler.GUI.SetDate(SlotType, dateToSet);
            }

            TCDRowsScaleforms[SlotType]?.SetDate(dateToSet);
            amProp?.SetState(dateToSet.ToString("tt", CultureInfo.InvariantCulture) == "AM");
            pmProp?.SetState(dateToSet.ToString("tt", CultureInfo.InvariantCulture) != "AM");

            date = dateToSet;
            toggle = true;
        }

        public void SetVisible(bool toggleTo, bool month = true, bool day = true, bool year = true, bool hour = true, bool minute = true, bool amPm = true)
        {
            if(!TcdEditer.IsEditing)
            {
                ScaleformsHandler.GUI.SetVisible(SlotType, toggleTo, month, day, year, hour, minute, amPm);
            }

            TCDRowsScaleforms[SlotType]?.SetVisible(toggleTo, month, day, year, hour, minute);

            if((!toggleTo && amPm) || (toggleTo && !amPm))
            {
                amProp?.Delete();
                pmProp?.Delete();
            }
            else if((!toggleTo && !amPm) || (toggleTo && amPm))
            {
                amProp?.SetState(date.ToString("tt", CultureInfo.InvariantCulture) == "AM");
                pmProp?.SetState(date.ToString("tt", CultureInfo.InvariantCulture) != "AM");
            }

            toggle = toggleTo;
        }

        public void SetVisibleAt(bool toggle, int showPropsAt, int showMonthAt)
        {
            this.toggle = toggle;
            this.showPropsAt = Game.GameTime + showPropsAt;
            this.showMonthAt = Game.GameTime + showMonthAt;
            this.IsDoingTimedVisible = true;
        }

        public void Update()
        {
            if (toggle || IsDoingTimedVisible)
                RenderTarget?.Draw();

            if (!IsDoingTimedVisible)
            {
                SetVisible(toggle);

                return;
            }

            if (Game.GameTime > showPropsAt)
            {
                SetVisible(toggle, false);
            }

            if(Game.GameTime > showMonthAt)
            {
                SetVisible(toggle);
            }

            if(Game.GameTime > showPropsAt && Game.GameTime > showMonthAt)
            {
                IsDoingTimedVisible = false;
            }
        }

        public void Dispose()
        {
            RenderTarget?.Dispose();
            amProp?.Delete();
            pmProp?.Delete();
        }

        private void OnRenderTargetDraw()
        {
            TCDRowsScaleforms[SlotType]?.Render2D(new PointF(0.379f, 0.12f), new SizeF(0.75f, 0.27f));
        }
    }

    public class TCDHandler : Handler
    {
        private DateTime errorDate = new DateTime(1885, 1, 1, 0, 0, 0);

        private readonly Dictionary<int, double> _probabilities = new Dictionary<int, double>()
        {
            {
                300, 0.1
            },
            {
                270, 0.2
            },
            {
                240, 0.3
            },
            {
                210, 0.4
            },
            {
                180, 0.5
            },
            {
                150, 0.6
            },
            {
                120, 0.7
            },
            {
                100, 0.8
            }
        };

        private int nextCheckGlitch;

        private TimedEventHandler glitchEvents = new TimedEventHandler();
        private bool softGlitch;

        private bool doGlitch;

        private TCDSlot destinationSlot;
        private TCDSlot presentSlot;
        private TCDSlot previousSlot;

        private bool currentState;

        private int nextTick;

        private DateTime lastTime;
        private int nextCheck;

        public bool IsDoingTimedVisible => destinationSlot.IsDoingTimedVisible;

        public TCDHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            destinationSlot = new TCDSlot("red", timeMachine);
            destinationSlot.SetVisible(false);

            presentSlot = new TCDSlot("green", timeMachine);
            presentSlot.SetVisible(false);

            previousSlot = new TCDSlot("yellow", timeMachine);
            previousSlot.SetVisible(false);

            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
            Events.OnDestinationDateChange += OnDestinationDateChange;
            Events.OnScaleformPriority += OnScaleformPriority;

            Events.OnTimeTravelStarted += OnTimeTravel;
            Events.OnTimeTravelCompleted += OnTimeTravelComplete;

            Events.SetTimeCircuits += SetTimeCircuitsOn;
            Events.SetTimeCircuitsBroken += SetTimeCircuitsBroken;

            Events.StartTimeCircuitsGlitch += StartTimeCircuitsGlitch;

            int _time = 0;

            for (int i = 0; i < 7; i++)
            {
                glitchEvents.Add(0, 0, _time, 0, 0, _time + 499);
                glitchEvents.Last.OnExecute += Blank_OnExecute;

                _time += 500;

                glitchEvents.Add(0, 0, _time, 0, 0, _time + 199);
                glitchEvents.Last.OnExecute += RandomDate_OnExecute;

                _time += 200;

                glitchEvents.Add(0, 0, _time, 0, 0, _time + 499);
                glitchEvents.Last.OnExecute += ErrorDate_OnExecute;

                _time += 500;
            }

            nextCheckGlitch = Game.GameTime + 120000;
        }

        private void Blank_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
                destinationSlot.SetVisible(false);
        }

        private void ErrorDate_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {

                if (timedEvent.Step == glitchEvents.EventsCount - 1)
                {
                    if (!softGlitch)
                        Properties.DestinationTime = errorDate;

                    destinationSlot.SetDate(Properties.DestinationTime);
                }
                else
                    destinationSlot.SetDate(errorDate);

                destinationSlot.Update();
            }                
        }

        private void RandomDate_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution) 
                return;

            destinationSlot.SetDate(Utils.RandomDate());
            destinationSlot.Update();
        }

        private void OnScaleformPriority()
        {
            if (Properties.AreTimeCircuitsOn)
                UpdateScaleformDates();
        }

        private void OnTimeCircuitsToggle()
        {
            if (!Properties.IsGivenScaleformPriority)
                return;

            if(Properties.AreTimeCircuitsOn)
            {
                destinationSlot.SetDate(Properties.DestinationTime);
                destinationSlot.SetVisible(false);
                destinationSlot.SetVisibleAt(true, 500, 600);

                previousSlot.SetDate(Properties.PreviousTime);
                previousSlot.SetVisible(false);
                previousSlot.SetVisibleAt(true, 500, 600);

                presentSlot.SetDate(Utils.GetWorldTime());
                presentSlot.SetVisible(false);
                presentSlot.SetVisibleAt(true, 500, 600);

                nextCheckGlitch = Game.GameTime + 30000;
            }
            else
            {
                destinationSlot.SetVisibleAt(false, 750, 750);
                previousSlot.SetVisibleAt(false, 750, 750);
                presentSlot.SetVisibleAt(false, 750, 750);

                currentState = false;
                Sounds.TCDBeep?.Stop();
                ScaleformsHandler.GUI.CallFunction("SET_DIODE_STATE", false);

                ExternalHUD.IsTickVisible = false;
                RemoteHUD.IsTickVisible = false;

                Props.TickingDiodes?.Delete();
                Props.TickingDiodesOff?.SpawnProp();
            }
        }

        private void OnTimeTravelComplete()
        {
            lastTime = Utils.GetWorldTime();
        }

        private void OnDestinationDateChange()
        {
            destinationSlot.SetDate(Properties.DestinationTime);
            destinationSlot.SetVisible(false);
            destinationSlot.SetVisibleAt(true, 500, 600);
        }

        private void OnTimeTravel()
        {
            if (!Properties.AreTimeCircuitsBroken)
                previousSlot.SetDate(Properties.PreviousTime);
        }

        public void UpdateScaleformDates()
        {
            destinationSlot.SetDate(Properties.DestinationTime);
            previousSlot.SetDate(Properties.PreviousTime);
            presentSlot.SetDate(Utils.GetWorldTime());
        }

        public void StartTimeCircuitsGlitch(bool softGlitch)
        {
            // Set TCD error stuff
            Sounds.TCDGlitch?.Play();

            glitchEvents.ResetExecution();

            this.softGlitch = softGlitch;
            doGlitch = true;

            nextCheckGlitch = Game.GameTime + 120000;
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyDown(Keys key)
        {
            if (Utils.PlayerVehicle != Vehicle) return;

            if (key == ModControls.TCToggle && !Properties.IsRemoteControlled)
                SetTimeCircuitsOn(!Properties.AreTimeCircuitsOn);
        }

        private double GetProbabilityForDamage(int damage)
        {
            KeyValuePair<int, double> selectedProb = new KeyValuePair<int, double>(0, 0);
            int lastDiff = 1000;

            foreach (var prob in _probabilities)
            {
                var diff = Math.Abs(prob.Key - damage);
                if (diff < lastDiff)
                {
                    selectedProb = prob;
                    lastDiff = diff;
                }
            }

            return selectedProb.Value;
        }

        public override void Process()
        {
            if (Properties.AreTimeCircuitsBroken && Mods.IsDMC12 && !Utils.PlayerPed.IsInVehicle() && !Properties.FullDamaged)
            {
                var dist = Utils.PlayerPed.Position.DistanceToSquared(Vehicle.Bones["bonnet"].Position);

                if (!(dist <= 2f * 2f))
                    return;

                Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Repair_TimeCircuits"));

                if (Game.IsControlJustPressed(GTA.Control.Context))
                    Mods.Hoodbox = ModState.On;
            }

            if (!Properties.IsGivenScaleformPriority || Game.Player.Character.Position.DistanceToSquared(Vehicle.Position) > 8f * 8f)
                return;

            destinationSlot.Update();
            previousSlot.Update();
            presentSlot.Update();

            if (!ModSettings.HideIngameTCDToggle)
                DrawGUI();

            if (!Properties.AreTimeCircuitsOn)
                return;

            UpdateCurrentTimeDisplay();
            TickDiodes();

            if (!Vehicle.IsVisible)
                return;

            HandleGlitching();                       

            if (Game.GameTime > nextCheckGlitch)
            {
                nextCheckGlitch = Game.GameTime + 60000;

                if (doGlitch || Properties.DestinationTime == errorDate || (Vehicle.Health > 300 && Properties.TimeTravelsCount < 5))
                    return;

                if (Vehicle.Health < 300)
                {                    
                    if (Utils.Random.NextDouble() < GetProbabilityForDamage((Vehicle.Health < 100 ? 100 : Vehicle.Health)))
                        StartTimeCircuitsGlitch(true);
                }
                else if (Properties.TimeTravelsCount > 4)
                {
                    if (Utils.Random.NextDouble() < 0.25f)
                        StartTimeCircuitsGlitch(true);
                }
            }
        }

        private void DrawGUI()
        {
            if (Utils.HideGUI || Utils.PlayerVehicle != Vehicle || !Properties.IsGivenScaleformPriority || Utils.IsPlayerUseFirstPerson() || TcdEditer.IsEditing)
                return;

            ScaleformsHandler.GUI.SetBackground(ModSettings.TCDBackground);
            ScaleformsHandler.GUI.Render2D(ModSettings.TCDPosition, new SizeF(ModSettings.TCDScale * (1501f / 1100f) / GTA.UI.Screen.AspectRatio, ModSettings.TCDScale));
        }

        private void UpdateCurrentTimeDisplay()
        {
            if (Game.GameTime > nextCheck)
            {
                var time = Utils.GetWorldTime();

                if (Math.Abs((time - lastTime).TotalMilliseconds) > 600 && !presentSlot.IsDoingTimedVisible)
                {
                    if (Vehicle != null)
                        presentSlot.SetDate(time);

                    lastTime = time;
                }

                nextCheck = Game.GameTime + 500;
            }
        }

        public void SetTimeCircuitsOn(bool on)
        {
            if (Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole | TcdEditer.IsEditing)
                return;

            if (!Properties.AreTimeCircuitsOn && Mods.Hoodbox == ModState.On && !Properties.AreHoodboxCircuitsReady)
            {
                if (!TcdEditer.IsEditing)
                    Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Hoodbox_Warmup_TfcError"));

                return;
            }

            if (!Properties.AreTimeCircuitsOn && Properties.AreTimeCircuitsBroken)
            {
                if (!TcdEditer.IsEditing)
                    Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Chip_Damaged"));

                return;
            }

            if (IsDoingTimedVisible)
                return;

            Properties.AreTimeCircuitsOn = on;

            if (on)
                Sounds.InputOn?.Play();
            else
                Sounds.InputOff?.Play();

            Events.OnTimeCircuitsToggle?.Invoke();
        }

        public void SetTimeCircuitsBroken(bool state)
        {
            Properties.AreTimeCircuitsBroken = state;

            if (!state)
                return;
            
            Properties.AreTimeCircuitsOn = false;
            Events.OnTimeCircuitsToggle?.Invoke();
        }

        private void TickDiodes()
        {
            if (Game.GameTime > nextTick)
            {
                if (Vehicle != null && Vehicle.IsVisible)
                {
                    Props.TickingDiodes?.SetState(currentState);
                    Props.TickingDiodesOff?.SetState(!currentState);
                }

                ScaleformsHandler.GUI.CallFunction("SET_DIODE_STATE", currentState);

                ExternalHUD.IsTickVisible = currentState;
                RemoteHUD.IsTickVisible = currentState;

                if (ModSettings.PlayDiodeBeep && currentState && Vehicle.IsVisible && !Sounds.TCDBeep.IsAnyInstancePlaying)
                    Sounds.TCDBeep?.Play(true);

                nextTick = Game.GameTime + 500;
                currentState = !currentState;
            }
        }

        private void HandleGlitching()
        {
            if (doGlitch)
            {
                glitchEvents.RunEvents();

                if (glitchEvents.AllExecuted())
                    doGlitch = false;
            }
        }

        public override void Stop()
        {
            destinationSlot.Dispose();
            previousSlot.Dispose();
            presentSlot.Dispose();
        }
    }
}