using System;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;
using static FusionLibrary.FusionEnums;

namespace BackToTheFutureV
{
    internal delegate void OnScaleformPriority();

    internal delegate void OnSIDMaxSpeedReached(bool over);
    internal delegate void OnTimeTravelSpeedReached(bool over);
    internal delegate void OnSparksInterrupted();
    internal delegate void OnWormholeStarted();
    internal delegate void OnSparksEnded(int delay = 0);
    internal delegate void OnTimeTravelStarted();
    internal delegate void OnTimeTravelEnded();
    internal delegate void OnReenterStarted();
    internal delegate void OnReenterEnded();

    internal delegate void OnTimeCircuitsToggle();
    internal delegate void OnDestinationDateChange(InputType inputType);
    internal delegate void OnLightningStrike();
    internal delegate void OnHoverUnderbodyToggle(bool reload = false);
    internal delegate void OnWormholeTypeChanged();
    internal delegate void OnReactorTypeChanged();
    internal delegate void OnVehicleSpawned();

    internal delegate void SetRCMode(bool state, bool instant = false);
    internal delegate void SetTimeCircuits(bool state);
    internal delegate void SetTimeCircuitsBroken();
    internal delegate void SetFlyMode(bool state, bool instant = false);
    internal delegate void SetAltitudeHold(bool state);
    internal delegate void SetHoodboxWarmedUp();
    internal delegate void SetFreeze(bool state, bool resume = false);
    internal delegate void StartTimeTravel(int delay = 0);
    internal delegate void StartFuelBlink();
    internal delegate void SetStopTracks(int delay = 0);
    internal delegate void SetReactorState(ReactorState reactorState);
    internal delegate void SetPedAI(bool state);
    internal delegate void SetEngineStall(bool state);
    internal delegate void StartLightningStrike(int delay);
    internal delegate void SimulateInputDate(DateTime dateTime);
    internal delegate void SimulateHoverBoost(bool state);
    internal delegate void SimulateHoverGoingUpDown(bool state);
    internal delegate void SetSIDLedsState(bool on, bool instant = false);
    internal delegate void StartTrain(bool force);
    internal delegate void SetTrainSpeed(float speed);

    internal class EventsHandler : HandlerPrimitive
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
        public SetFlyMode SetFlyMode;
        public SetAltitudeHold SetAltitudeHold;
        public SetHoodboxWarmedUp SetHoodboxWarmedUp;
        public SetFreeze SetFreeze;
        public StartTimeTravel StartTimeTravel;
        public StartFuelBlink StartFuelBlink;
        public SetStopTracks SetStopTracks;
        public SetReactorState SetReactorState;
        public SetPedAI StartDriverAI;
        public SetEngineStall SetEngineStall;
        public StartLightningStrike StartLightningStrike;
        public SimulateInputDate SimulateInputDate;
        public SimulateHoverBoost SimulateHoverBoost;
        public SimulateHoverGoingUpDown SimulateHoverGoingUpDown;
        public SetSIDLedsState SetSIDLedsState;
        public StartTrain StartTrain;
        public SetTrainSpeed SetTrainSpeed;

        public EventsHandler(TimeMachine timeMachine) : base(timeMachine)
        {

        }

        public override void Dispose()
        {

        }

        public override void KeyDown(KeyEventArgs e)
        {

        }

        public override void Tick()
        {

        }

        public override void Stop()
        {

        }
    }
}
