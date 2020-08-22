using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using System.Globalization;
using System.Drawing;
using BackToTheFutureV.Entities;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Settings;
using KlangRageAudioLibrary;
using BackToTheFutureV.Story;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class TCDSlot
    {
        private static Dictionary<string, Vector3> offsets = new Dictionary<string, Vector3>()
        {
            { "red", new Vector3(-0.01477456f, 0.3175744f, 0.6455771f) },
            { "yellow", new Vector3(-0.01964803f, 0.2769623f, 0.565388f) },
            { "green", new Vector3(-0.01737539f, 0.2979541f, 0.6045464f) }
        };

        public RenderTarget RenderTarget { get; private set; }
        public TCDRowScaleform Scaleform { get; private set; }
        public TimeCircuitsScaleform ScreenTCD { get; private set; }
        public TimeCircuits TimeCircuits { get; private set; }

        public string SlotType { get; private set; }

        private DateTime date;

        public bool IsDoingTimedVisible { get; private set; }
        private bool toggle;
        private int showPropsAt;
        private int showMonthAt;

        private AnimateProp amProp;
        private AnimateProp pmProp;

        public TCDSlot(string slotType, TimeCircuitsScaleform scaleform, TimeCircuits circuits)
        {
            SlotType = slotType;
            ScreenTCD = scaleform;
            TimeCircuits = circuits;
            Scaleform = new TCDRowScaleform(slotType);
            RenderTarget = new RenderTarget(new Model("bttf_3d_row_" + slotType), "bttf_tcd_row_" + slotType, circuits.Vehicle, offsets[slotType], new Vector3(355.9951f, 0.04288517f, 352.7451f));
            RenderTarget.CreateProp();
            Scaleform.DrawInPauseMenu = true;

            amProp = new AnimateProp(circuits.Delorean, new Model($"bttf_{slotType}_am"), Vector3.Zero, Vector3.Zero);
            pmProp = new AnimateProp(circuits.Delorean, new Model($"bttf_{slotType}_pm"), Vector3.Zero, Vector3.Zero);

            RenderTarget.OnRenderTargetDraw += OnRenderTargetDraw;

            date = new DateTime();
        }

        public void SetDate(DateTime dateToSet)
        {
            if (!TcdEditer.IsEditing)
            {
                ScreenTCD.SetDate(SlotType, dateToSet);
            }

            Scaleform.SetDate(dateToSet);
            amProp.SetState(dateToSet.ToString("tt", CultureInfo.InvariantCulture) == "AM");
            pmProp.SetState(dateToSet.ToString("tt", CultureInfo.InvariantCulture) != "AM");

            date = dateToSet;
            toggle = true;
        }

        public void SetVisible(bool toggleTo, bool month = true, bool day = true, bool year = true, bool hour = true, bool minute = true, bool amPm = true)
        {
            if(!TcdEditer.IsEditing)
            {
                ScreenTCD.SetVisible(SlotType, toggleTo, month, day, year, hour, minute, amPm);
            }

            Scaleform.SetVisible(toggleTo, month, day, year, hour, minute);

            if((!toggleTo && amPm) || (toggleTo && !amPm))
            {
                amProp?.DeleteProp();
                pmProp?.DeleteProp();
            }
            else if((!toggleTo && !amPm) || (toggleTo && amPm))
            {
                amProp.SetState(date.ToString("tt", CultureInfo.InvariantCulture) == "AM");
                pmProp.SetState(date.ToString("tt", CultureInfo.InvariantCulture) != "AM");
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
                RenderTarget.Draw();

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
            RenderTarget.Dispose();
            amProp?.DeleteProp();
            pmProp?.DeleteProp();
        }

        private void OnRenderTargetDraw()
        {
            Scaleform.Render2D(new PointF(0.379f, 0.12f), new SizeF(0.75f, 0.27f));
        }
    }

    public class TCDHandler : Handler
    {
        //private DateTime oldDate;
        private DateTime errorDate = new DateTime(1885, 1, 1, 12, 0, 0);

        private TimedEventManager glitchEvents = new TimedEventManager();

        private bool doGlitch;
        //private int nextGlitch;
        //private int glitchCount;

        private TCDSlot destinationSlot;
        private TCDSlot presentSlot;
        private TCDSlot previousSlot;

        private bool currentState;
        private AnimateProp tickingDiodes;
        private AnimateProp tickingDiodesOff;

        private AudioPlayer fluxCapacitor;
        private AudioPlayer beep;
        private int nextTick;

        private DateTime lastTime;
        private int nextCheck;

        public bool IsDoingTimedVisible => destinationSlot.IsDoingTimedVisible;

        public TCDHandler(TimeCircuits circuits) : base(circuits)
        {
            destinationSlot = new TCDSlot("red", GUI, circuits);
            destinationSlot.SetVisible(false);

            presentSlot = new TCDSlot("green", GUI, circuits);
            presentSlot.SetVisible(false);

            previousSlot = new TCDSlot("yellow", GUI, circuits);
            previousSlot.SetVisible(false);

            beep = circuits.AudioEngine.Create("general/timeCircuits/beep.wav", Presets.Interior);
            fluxCapacitor = circuits.AudioEngine.Create("general/fluxCapacitor.wav", Presets.InteriorLoop);

            fluxCapacitor.Volume = 0.1f;

            fluxCapacitor.MinimumDistance = 0.5f;
            beep.MinimumDistance = 0.3f;
            fluxCapacitor.SourceBone = "flux_capacitor";
            beep.SourceBone = "bttf_tcd_green";

            tickingDiodes = new AnimateProp(circuits.Delorean, ModelHandler.TickingDiodes, Vector3.Zero, Vector3.Zero);
            tickingDiodesOff = new AnimateProp(circuits.Delorean, ModelHandler.TickingDiodesOff, Vector3.Zero, Vector3.Zero);
            tickingDiodesOff.SpawnProp();

            TimeCircuits.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
            TimeCircuits.OnDestinationDateChange += OnDestinationDateChange;
            TimeCircuits.OnScaleformPriority += OnScaleformPriority;

            TimeCircuits.OnTimeTravel += OnTimeTravel;
            TimeCircuits.OnTimeTravelComplete += OnTimeTravelComplete;

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
                    DestinationTime = errorDate;

                destinationSlot.SetDate(errorDate);
                destinationSlot.Update();
            }                
        }

        private void RandomDate_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {
                destinationSlot.SetDate(DateTime.Now.Random());
                destinationSlot.Update();
            }
        }

        private void OnScaleformPriority()
        {
            if (IsOn)
                UpdateScaleformDates();
        }

        private void OnTimeCircuitsToggle()
        {
            if (!TimeCircuits.Delorean.IsGivenScaleformPriority)
                return;

            if(IsOn)
            {
                if (ModSettings.PlayFluxCapacitorSound)
                    fluxCapacitor.Play();

                destinationSlot.SetDate(DestinationTime);
                destinationSlot.SetVisible(false);
                destinationSlot.SetVisibleAt(true, 500, 600);

                previousSlot.SetDate(PreviousTime);
                previousSlot.SetVisible(false);
                previousSlot.SetVisibleAt(true, 500, 600);

                presentSlot.SetDate(Utils.GetWorldTime());
                presentSlot.SetVisible(false);
                presentSlot.SetVisibleAt(true, 500, 600);
            }
            else
            {
                if (fluxCapacitor.IsAnyInstancePlaying)
                    fluxCapacitor?.Stop();

                destinationSlot.SetVisibleAt(false, 750, 750);
                previousSlot.SetVisibleAt(false, 750, 750);
                presentSlot.SetVisibleAt(false, 750, 750);

                currentState = false;
                beep?.Stop();
                GUI.CallFunction("SET_DIODE_STATE", false);
                tickingDiodes?.DeleteProp();
                tickingDiodesOff?.SpawnProp();
            }
        }

        private void OnTimeTravelComplete()
        {
            lastTime = Utils.GetWorldTime();
        }

        private void OnDestinationDateChange()
        {
            destinationSlot.SetDate(DestinationTime);
            destinationSlot.SetVisible(false);
            destinationSlot.SetVisibleAt(true, 500, 600);
        }

        private void OnTimeTravel()
        {
            previousSlot.SetDate(PreviousTime);
        }

        public void UpdateScaleformDates()
        {
            destinationSlot.SetDate(DestinationTime);
            previousSlot.SetDate(PreviousTime);
            presentSlot.SetDate(Utils.GetWorldTime());
        }

        public void DoGlitch()
        {
            glitchEvents.ResetExecution();

            doGlitch = true;
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyPress(Keys key)
        {
        }

        public override void Process()
        {
            if (ModSettings.PlayFluxCapacitorSound)
            {
                if (!Vehicle.IsVisible && fluxCapacitor.IsAnyInstancePlaying)
                    fluxCapacitor?.Stop();

                if (!fluxCapacitor.IsAnyInstancePlaying && IsOn && Vehicle.IsVisible)
                    fluxCapacitor.Play();
            }
            else if (fluxCapacitor.IsAnyInstancePlaying)
                fluxCapacitor?.Stop();

            if (!TimeCircuits.Delorean.IsGivenScaleformPriority || Game.Player.Character.Position.DistanceToSquared(Vehicle.Position) > 8f * 8f)
            {
                return;
            }

            destinationSlot.Update();
            previousSlot.Update();
            presentSlot.Update();

            if (!IsOn)
                return;

            UpdateCurrentTimeDisplay();
            TickDiodes();

            if (!Vehicle.IsVisible)
                return;

            HandleGlitching();
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

        private void TickDiodes()
        {
            if (Game.GameTime > nextTick)
            {
                if (Vehicle != null && Vehicle.IsVisible)
                {
                    tickingDiodes?.SetState(currentState);
                    tickingDiodesOff?.SetState(!currentState);
                }

                GUI.CallFunction("SET_DIODE_STATE", currentState);

                if(ModSettings.PlayDiodeBeep && currentState && Vehicle.IsVisible && !beep.IsAnyInstancePlaying)
                    beep.Play(true);

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

            //if (doGlitch && Game.GameTime > nextGlitch)
            //{
            //    if (glitchCount <= 5)
            //    {
            //        if (destinationDisplay.CurrentTime == null)
            //        {
            //            destinationDisplay.CurrentTime = errorDate;

            //            if (Vehicle != null && Vehicle.IsVisible)
            //                destinationDisplay.CreateProps();

            //            GUI.SetVisible("red", true);

            //            nextGlitch = Game.GameTime + 600;
            //        }
            //        else
            //        {
            //            destinationDisplay.CurrentTime = null;

            //            if(Vehicle != null && Vehicle.IsVisible)
            //                destinationDisplay.DeleteAllProps();

            //            GUI.SetVisible("red", false);

            //            nextGlitch = Game.GameTime + 230;
            //        }

            //        glitchCount++;
            //    }
            //    else
            //    {
            //        glitchCount = 0;
            //        doGlitch = false;
            //        TimeCircuits.GetHandler<InputHandler>().InputDate(oldDate);
            //    }
            //}
        }

        public override void Stop()
        {
            fluxCapacitor?.Stop();
            fluxCapacitor?.Dispose();
            destinationSlot.Dispose();
            previousSlot.Dispose();
            presentSlot.Dispose();
            tickingDiodes?.DeleteProp();
            tickingDiodesOff?.DeleteProp();
        }
    }
}