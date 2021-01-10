using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using GTA;

namespace BackToTheFutureV.Players
{
    public delegate void OnCompleted();

    public abstract class Player : FusionLibrary.Player
    {
        public TimeMachine TimeMachine { get; }

        public TimeMachineMods Mods => TimeMachine.Mods;

        public PropertiesHandler Properties => TimeMachine.Properties;
        public SoundsHandler Sounds => TimeMachine.Sounds;
        public PropsHandler Props => TimeMachine.Props;
        public PlayersHandler Players => TimeMachine.Players;
        public ScaleformsHandler Scaleforms => TimeMachine.Scaleforms;

        public Vehicle Vehicle { get; }

        public Player(TimeMachine timeMachine)
        {
            TimeMachine = timeMachine;
            Vehicle = timeMachine.Vehicle;
        }

        public Player()
        {

        }

        protected Player(Vehicle vehicle)
        {
            Vehicle = vehicle;
        }
    }
}
