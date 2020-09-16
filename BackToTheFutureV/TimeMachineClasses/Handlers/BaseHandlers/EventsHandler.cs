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
    public delegate void OnHoverUnderbodyToggle();
    public delegate void OnWormholeTypeChanged();
    public delegate void OnReactorTypeChanged();

    public delegate void SetRCMode(bool state, bool instant = false);
    public delegate void SetTimeCircuits(bool state);
    public delegate void SetTimeCircuitsBroken(bool state);
    public delegate void StartTimeCircuitsGlitch();
    public delegate void SetCutsceneMode(bool state);    
    public delegate void SetFlyMode(bool state, bool instant = false);
    public delegate void SetAltitudeHold(bool state);
    public delegate void SetFreeze(bool state, bool resume = false);
    public delegate void StartTimeTravel(int delay = 0);
    public delegate void StartFuelBlink();
    public delegate void SetRailroadMode(bool state, bool isReentering = false);

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
        public SetRailroadMode SetRailroadMode;

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
