using BackToTheFutureV.Utility;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers
{
    public delegate void OnScaleformPriority();
    public delegate void OnTimeCircuitsToggle();
    public delegate void OnDestinationDateChange();
    public delegate void OnWormholeStarted();
    public delegate void OnTimeTravelStarted();    
    public delegate void OnTimeTravelCompleted();
    public delegate void OnReenter();
    public delegate void OnReenterCompleted();
    public delegate void OnTimeTravelInterrupted();
    public delegate void OnLightningStrike();   
    public delegate void OnHoverTransformation();
    public delegate void OnRCToggle();
    public delegate void OnHoverUnderbodyToggle(bool reload = false);
    public delegate void OnWormholeTypeChanged();
    public delegate void OnReactorTypeChanged();
    public delegate void OnVehicleSpawned();
    public delegate void OnHoodboxReady();
    public delegate void OnMissionChange();

    public delegate void SetRCMode(bool state, bool instant = false);
    public delegate void SetTimeCircuits(bool state);
    public delegate void SetTimeCircuitsBroken(bool state);
    public delegate void StartTimeCircuitsGlitch(bool softGlitch);
    public delegate void SetCutsceneMode(bool state);    
    public delegate void SetFlyMode(bool state, bool instant = false);
    public delegate void SetAltitudeHold(bool state);
    public delegate void SetFreeze(bool state, bool resume = false);
    public delegate void StartTimeTravel(int delay = 0);
    public delegate void StartFuelBlink();
    public delegate void SetStopTracks(int delay = 0);
    public delegate void SetRefuel(Ped ped);
    public delegate void SetPedAI(bool state);
    public delegate void SetEngineStall(bool state);
    public delegate void StartLightningStrike(int delay);

    public class EventsHandler : Handler
    {
        public OnScaleformPriority OnScaleformPriority;
        public OnTimeCircuitsToggle OnTimeCircuitsToggle;
        public OnDestinationDateChange OnDestinationDateChange;
        public OnWormholeStarted OnWormholeStarted;
        public OnTimeTravelStarted OnTimeTravelStarted;        
        public OnTimeTravelCompleted OnTimeTravelCompleted;
        public OnReenter OnReenter;
        public OnReenterCompleted OnReenterCompleted;
        public OnTimeTravelInterrupted OnTimeTravelInterrupted;
        public OnLightningStrike OnLightningStrike;        
        public OnHoverTransformation OnHoverTransformation;
        public OnRCToggle OnRCToggle;
        public OnHoverUnderbodyToggle OnHoverUnderbodyToggle;
        public OnWormholeTypeChanged OnWormholeTypeChanged;
        public OnReactorTypeChanged OnReactorTypeChanged;
        public OnVehicleSpawned OnVehicleSpawned;
        public OnHoodboxReady OnHoodboxReady;
        public OnMissionChange OnMissionChange;

        public SetRCMode SetRCMode;
        public SetTimeCircuits SetTimeCircuits;
        public SetTimeCircuitsBroken SetTimeCircuitsBroken;
        public StartTimeCircuitsGlitch StartTimeCircuitsGlitch;
        public SetCutsceneMode SetCutsceneMode;
        public SetFlyMode SetFlyMode;
        public SetAltitudeHold SetAltitudeHold;
        public SetFreeze SetFreeze;
        public StartTimeTravel StartTimeTravel;
        public StartFuelBlink StartFuelBlink;
        public SetStopTracks SetStopTracks;
        public SetRefuel SetRefuel;
        public SetPedAI StartDriverAI;
        public SetWheelie SetWheelie;
        public SetEngineStall SetEngineStall;
        public StartLightningStrike StartLightningStrike;

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
