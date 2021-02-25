using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using BackToTheFutureV.Vehicles;
using GTA;

namespace BackToTheFutureV.Players
{
    internal abstract class Player : FusionLibrary.Player
    {
        public TimeMachine TimeMachine { get; }

        public Vehicle Vehicle => TimeMachine.Vehicle;
        public DMC12 DMC12 => TimeMachine.DMC12;

        public TimeMachineMods Mods => TimeMachine.Mods;
        public EventsHandler Events => TimeMachine.Events;
        public PropertiesHandler Properties => TimeMachine.Properties;
        public SoundsHandler Sounds => TimeMachine.Sounds;
        public PropsHandler Props => TimeMachine.Props;
        public PlayersHandler Players => TimeMachine.Players;
        public ScaleformsHandler Scaleforms => TimeMachine.Scaleforms;
        public ParticlesHandler Particles => TimeMachine.Particles;
        public ConstantsHandler Constants => TimeMachine.Constants;

        public Player(TimeMachine timeMachine)
        {
            TimeMachine = timeMachine;
            Entity = timeMachine.Vehicle;
        }
    }
}
