using GTA;

namespace BackToTheFutureV.Players
{
    internal abstract class Player : FusionLibrary.Player
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

        public Player(TimeMachine timeMachine)
        {
            TimeMachine = timeMachine;
            Entity = timeMachine.Vehicle;
        }
    }
}
