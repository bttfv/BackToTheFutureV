using FusionLibrary;
using GTA;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal class TCDHandler : HandlerPrimitive
    {
        private static DateTime errorDate = new DateTime(1885, 1, 1, 0, 0, 0);

        private static readonly Dictionary<int, double> _probabilities = new Dictionary<int, double>()
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

        private TCD3DRowHandler destinationSlot;
        private TCD3DRowHandler presentSlot;
        private TCD3DRowHandler previousSlot;

        private bool currentState;

        private int nextTick;

        private DateTime lastTime;
        private int nextCheck;

        public bool IsDoingTimedVisible => destinationSlot.IsDoingTimedVisible;

        public TCDHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            destinationSlot = new TCD3DRowHandler("red", timeMachine);
            destinationSlot.SetVisible(false);

            presentSlot = new TCD3DRowHandler("green", timeMachine);
            presentSlot.SetVisible(false);

            previousSlot = new TCD3DRowHandler("yellow", timeMachine);
            previousSlot.SetVisible(false);

            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
            Events.OnDestinationDateChange += OnDestinationDateChange;
            Events.OnScaleformPriority += OnScaleformPriority;

            Events.OnSparksInterrupted += () => { Particles.LightningSparks?.Stop(); };
            Events.OnTimeTravelStarted += OnTimeTravel;

            Events.SetTimeCircuits += SetTimeCircuitsOn;
            Events.SetTimeCircuitsBroken += SetTimeCircuitsBroken;

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

                destinationSlot.Tick();
            }
        }

        private void RandomDate_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            destinationSlot.SetDate(FusionUtils.RandomDate());
            destinationSlot.Tick();
        }

        private void OnScaleformPriority()
        {
            if (!Properties.IsGivenScaleformPriority || !Properties.AreTimeCircuitsOn)
                return;

            destinationSlot.SetDate(Properties.DestinationTime);
            presentSlot.SetDate(FusionUtils.CurrentTime);
            previousSlot.SetDate(Properties.PreviousTime);
        }

        private void OnTimeCircuitsToggle()
        {
            if (!Properties.IsGivenScaleformPriority)
                return;

            if (Properties.AreTimeCircuitsOn)
            {
                destinationSlot.SetDate(Properties.DestinationTime);
                destinationSlot.SetVisible(false);
                destinationSlot.SetVisibleAt(true, 500, 600);

                previousSlot.SetDate(Properties.PreviousTime);
                previousSlot.SetVisible(false);
                previousSlot.SetVisibleAt(true, 500, 600);

                presentSlot.SetDate(FusionUtils.CurrentTime);
                presentSlot.SetVisible(false);
                presentSlot.SetVisibleAt(true, 500, 600);

                Events.SetSIDLedsState?.Invoke(true);

                nextCheckGlitch = Game.GameTime + 30000;
            }
            else
            {
                destinationSlot.SetVisibleAt(false, 750, 750);
                previousSlot.SetVisibleAt(false, 750, 750);
                presentSlot.SetVisibleAt(false, 750, 750);

                currentState = false;
                Sounds.TCDBeep?.Stop();
                ScaleformsHandler.GUI.SetDiodeState(false);

                Properties.HUDProperties.IsTickVisible = false;

                Props.TickingDiodes?.Delete();
                Props.TickingDiodesOff?.SpawnProp();

                Events.SetSIDLedsState?.Invoke(false);
            }
        }

        private void OnDestinationDateChange(InputType inputType)
        {
            if (TimeMachineHandler.CurrentTimeMachine != TimeMachine)
                return;

            destinationSlot.SetDate(Properties.DestinationTime);

            if (inputType == InputType.Time)
                return;

            destinationSlot.SetVisible(false);
            destinationSlot.SetVisibleAt(true, 500, 600);
        }

        private void OnTimeTravel()
        {
            if (TimeMachineHandler.CurrentTimeMachine == TimeMachine)
                previousSlot.SetDate(Properties.PreviousTime);

            lastTime = FusionUtils.CurrentTime;
            StopGlitch();
            Particles.LightningSparks?.Stop();
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

        public void StopGlitch()
        {
            Sounds.TCDGlitch?.Stop();

            destinationSlot.SetVisible(true);
            glitchEvents.ResetExecution();
            doGlitch = false;

            nextCheckGlitch = Game.GameTime + 120000;
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == ModControls.TCToggle && !Properties.IsRemoteControlled)
                SetTimeCircuitsOn(!Properties.AreTimeCircuitsOn);
        }

        private double GetProbabilityForDamage(int damage)
        {
            KeyValuePair<int, double> selectedProb = new KeyValuePair<int, double>(0, 0);
            int lastDiff = 1000;

            foreach (KeyValuePair<int, double> prob in _probabilities)
            {
                int diff = Math.Abs(prob.Key - damage);
                if (diff < lastDiff)
                {
                    selectedProb = prob;
                    lastDiff = diff;
                }
            }

            return selectedProb.Value;
        }

        public override void Tick()
        {
            if (!Properties.IsGivenScaleformPriority)
                return;

            if (Mods.IsDMC12)
            {
                if (Properties.AreTimeCircuitsOn)
                {
                    if (!IsDoingTimedVisible && Props.DiodesOff.IsSpawned)
                        Props.DiodesOff?.Delete();
                }
                else
                {
                    if (!IsDoingTimedVisible && !Props.DiodesOff.IsSpawned)
                        Props.DiodesOff?.SpawnProp();
                }
            }

            destinationSlot.Tick();
            previousSlot.Tick();
            presentSlot.Tick();

            if (!ModSettings.HideIngameTCDToggle)
                DrawGUI();

            if (FusionUtils.PlayerVehicle == Vehicle && Properties.TimeTravelPhase < TimeTravelPhase.InTime)
                ExternalHUD.Update(Properties.HUDProperties);

            if (!Properties.AreTimeCircuitsOn)
                return;

            UpdateCurrentTimeDisplay();
            TickDiodes();

            if (!Vehicle.IsVisible)
                return;

            if (Mods.IsDMC12 && Properties.ReactorState != ReactorState.Closed && Properties.TimeTravelPhase == TimeTravelPhase.OpeningWormhole)
            {
                if (!doGlitch)
                    StartTimeCircuitsGlitch(true);

                if (!Particles.LightningSparks.IsPlaying)
                    Particles.LightningSparks?.Play();
            }

            HandleGlitching();

            if (Game.GameTime > nextCheckGlitch)
            {
                nextCheckGlitch = Game.GameTime + 60000;

                if (doGlitch || Properties.DestinationTime == errorDate || (Vehicle.Health > 300 && Properties.TimeTravelsCount < 5))
                    return;

                if (Vehicle.Health < 300)
                {
                    if (FusionUtils.Random.NextDouble() < GetProbabilityForDamage((Vehicle.Health < 100 ? 100 : Vehicle.Health)))
                        StartTimeCircuitsGlitch(false);
                }
                else if (Properties.TimeTravelsCount > 4)
                {
                    if (FusionUtils.Random.NextDouble() < 0.25f)
                        StartTimeCircuitsGlitch(true);
                }
            }
        }

        private void DrawGUI()
        {
            if (FusionUtils.HideGUI || FusionUtils.PlayerVehicle != Vehicle || FusionUtils.IsCameraInFirstPerson() || TcdEditer.IsEditing || RCGUIEditer.IsEditing || Properties.IsRemoteControlled)
                return;

            ScaleformsHandler.GUI.SetSpeedoBackground(Properties.ThreeDigitsSpeedo);
            ScaleformsHandler.GUI.SetBackground(ModSettings.TCDBackground);
            ScaleformsHandler.GUI.SetEmpty(Properties.HUDProperties.Empty);
            ScaleformsHandler.GUI.Draw2D();

            if (ModSettings.HideSID || !Mods.IsDMC12)
                return;

            ScaleformsHandler.SID2D?.Draw2D();
        }

        private void UpdateCurrentTimeDisplay()
        {
            if (Game.GameTime > nextCheck)
            {
                DateTime time = FusionUtils.CurrentTime;

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
            if (Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole | TcdEditer.IsEditing | RCGUIEditer.IsEditing)
                return;

            if (!Properties.AreTimeCircuitsOn && Mods.Hoodbox == ModState.On && !Properties.AreHoodboxCircuitsReady)
            {
                if (!TcdEditer.IsEditing && !RCGUIEditer.IsEditing)
                    TextHandler.ShowHelp("NotWarmed");

                return;
            }

            if (!Properties.AreTimeCircuitsOn && Properties.AreTimeCircuitsBroken && Mods.Hoodbox == ModState.Off)
            {
                if (!TcdEditer.IsEditing && !RCGUIEditer.IsEditing)
                    TextHandler.ShowHelp("ChipDamaged");

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

        public void SetTimeCircuitsBroken()
        {
            SetTimeCircuitsOn(false);
            Properties.AreTimeCircuitsBroken = true;
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

                ScaleformsHandler.GUI.SetDiodeState(currentState);

                Properties.HUDProperties.IsTickVisible = currentState;

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