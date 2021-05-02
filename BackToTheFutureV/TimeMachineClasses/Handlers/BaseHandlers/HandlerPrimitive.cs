using GTA;
using System.Windows.Forms;

namespace BackToTheFutureV
{
    internal abstract class HandlerPrimitive
    {
        public TimeMachine TimeMachine { get; }

        public Ped Driver => Vehicle.Driver;

        public Vehicle Vehicle => TimeMachine.Vehicle;
        public DMC12 DMC12 => TimeMachine.DMC12;

        public ModsHandler Mods => TimeMachine.Mods;
        public EventsHandler Events => TimeMachine.Events;
        public PropertiesHandler Properties => TimeMachine.Properties;
        public SoundsHandler Sounds => TimeMachine.Sounds;
        public PropsHandler Props => TimeMachine.Props;
        public PlayersHandler Players => TimeMachine.Players;
        public ScaleformsHandler Scaleforms => TimeMachine.Scaleforms;
        public ParticlesHandler Particles => TimeMachine.Particles;
        public ConstantsHandler Constants => TimeMachine.Constants;
        public DecoratorsHandler Decorators => TimeMachine.Decorators;

        public HandlerPrimitive(TimeMachine timeMachine)
        {
            TimeMachine = timeMachine;
        }

        public bool IsPlaying { get; protected set; }

        public abstract void KeyDown(KeyEventArgs e);
        public abstract void Tick();
        public abstract void Stop();
        public abstract void Dispose();
    }
}
