using GTA;
using System;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers
{
    public delegate void OnScaleformPriority();

    public delegate void OnSIDMaxSpeedReached(bool over);
    public delegate void OnTimeTravelSpeedReached(bool over);
    public delegate void OnSparksInterrupted();
    public delegate void OnWormholeStarted();
    public delegate void OnSparksEnded(int delay = 0);
    public delegate void OnTimeTravelStarted();
    public delegate void OnTimeTravelEnded();
    public delegate void OnReenterStarted();
    public delegate void OnReenterEnded();

    public delegate void OnTimeCircuitsToggle();
    public delegate void OnDestinationDateChange();
    public delegate void OnLightningStrike();
    public delegate void OnHoverUnderbodyToggle(bool reload = false);
    public delegate void OnWormholeTypeChanged();
    public delegate void OnReactorTypeChanged();
    public delegate void OnVehicleSpawned();

    public delegate void SetRCMode(bool state, bool instant = false);
    public delegate void SetTimeCircuits(bool state);
    public delegate void SetTimeCircuitsBroken();
    public delegate void SetCutsceneMode(bool state);
    public delegate void SetFlyMode(bool state, bool instant = false);
    public delegate void SetAltitudeHold(bool state);
    public delegate void SetHoodboxWarmedUp();
    public delegate void SetFreeze(bool state, bool resume = false);
    public delegate void StartTimeTravel(int delay = 0);
    public delegate void StartFuelBlink();
    public delegate void SetStopTracks(int delay = 0);
    public delegate void SetRefuel(Ped ped);
    public delegate void SetPedAI(bool state);
    public delegate void SetWheelie(bool goUp);
    public delegate void SetEngineStall(bool state);
    public delegate void StartLightningStrike(int delay);
    public delegate void SimulateInputDate(DateTime dateTime);
    public delegate void SetSIDLedsState(bool on, bool instant = false);

    public class EventsHandler : Handler
    {
        public OnScaleformPriority OnScaleformPriority;

        public OnSIDMaxSpeedReached OnSIDMaxSpeedReached;
        public OnTimeTravelSpeedReached OnTimeTravelSpeedReached;
        public OnSparksInterrupted OnSparksInterrupted;
        public OnWormholeStarted OnWormholeStarted;
        public OnSparksEnded OnSparksEnded;
        public OnTimeTravelStarted OnTimeTravelStarted;
        public OnTimeTravelEnded OnTimeTravelEnded;
        public OnReenterStarted OnReenterStarted;
        public OnReenterEnded OnReenterEnded;

        public OnTimeCircuitsToggle OnTimeCircuitsToggle;
        public OnDestinationDateChange OnDestinationDateChange;
        public OnLightningStrike OnLightningStrike;
        public OnHoverUnderbodyToggle OnHoverUnderbodyToggle;
        public OnWormholeTypeChanged OnWormholeTypeChanged;
        public OnReactorTypeChanged OnReactorTypeChanged;
        public OnVehicleSpawned OnVehicleSpawned;

        public SetRCMode SetRCMode;
        public SetTimeCircuits SetTimeCircuits;
        public SetTimeCircuitsBroken SetTimeCircuitsBroken;
        public SetCutsceneMode SetCutsceneMode;
        public SetFlyMode SetFlyMode;
        public SetAltitudeHold SetAltitudeHold;
        public SetHoodboxWarmedUp SetHoodboxWarmedUp;
        public SetFreeze SetFreeze;
        public StartTimeTravel StartTimeTravel;
        public StartFuelBlink StartFuelBlink;
        public SetStopTracks SetStopTracks;
        public SetRefuel SetRefuel;
        public SetPedAI StartDriverAI;
        public SetWheelie SetWheelie;
        public SetEngineStall SetEngineStall;
        public StartLightningStrike StartLightningStrike;
        public SimulateInputDate SimulateInputDate;
        public SetSIDLedsState SetSIDLedsState;

        public EventsHandler(TimeMachine timeMachine) : base(timeMachine)
        {

        }

        public override void Dispose()
        {

        }

        public override void KeyDown(Keys key)
        {

        }

        public override void Process()
        {

        }

        public override void Stop()
        {

        }
    }
}
