using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using BackToTheFutureV.Vehicles;
using GTA;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public abstract class Handler
    {
        public TimeMachine TimeMachine { get; }

        public Vehicle Vehicle => TimeMachine.Vehicle;

        public TimeMachineMods Mods => TimeMachine.Mods;

        public EventsHandler Events => TimeMachine.Events;
        public PropertiesHandler Properties => TimeMachine.Properties;        
        public SoundsHandler Sounds => TimeMachine.Sounds;
        public PropsHandler Props => TimeMachine.Props;
        public PlayersHandler Players => TimeMachine.Players;
        public ScaleformsHandler Scaleforms => TimeMachine.Scaleforms;
        public ParticlesHandler Particles => TimeMachine.Particles;       

        public DMC12 DMC12 => TimeMachine.DMC12;

        public Handler(TimeMachine timeMachine)
        {
            TimeMachine = timeMachine;
        }

        public bool IsPlaying { get; protected set; }

        public abstract void KeyDown(System.Windows.Forms.Keys key);
        public abstract void Process();
        public abstract void Stop();
        public abstract void Dispose();
    }
}
