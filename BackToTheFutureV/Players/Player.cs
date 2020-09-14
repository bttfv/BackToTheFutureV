using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.Players
{
    public delegate void OnCompleted();

    public abstract class Player
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

        public OnCompleted OnCompleted { get; set; }

        public bool IsPlaying { get; protected set; }

        public abstract void Play();

        public abstract void Process();

        public abstract void Stop();

        public abstract void Dispose();
    }
}
