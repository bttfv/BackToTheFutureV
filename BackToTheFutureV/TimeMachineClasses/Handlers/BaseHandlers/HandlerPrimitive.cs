using GTA;
using System.Windows.Forms;

namespace BackToTheFutureV
{
    internal abstract class HandlerPrimitive
    {
        public TimeMachine TimeMachine { get; }

        public Ped Driver
        {
            get
            {
                return Vehicle.Driver;
            }
        }

        public Vehicle Vehicle
        {
            get
            {
                return TimeMachine.Vehicle;
            }
        }

        public DMC12 DMC12
        {
            get
            {
                return TimeMachine.DMC12;
            }
        }

        public ModsHandler Mods
        {
            get
            {
                return TimeMachine.Mods;
            }
        }

        public EventsHandler Events
        {
            get
            {
                return TimeMachine.Events;
            }
        }

        public PropertiesHandler Properties
        {
            get
            {
                return TimeMachine.Properties;
            }
        }

        public SoundsHandler Sounds
        {
            get
            {
                return TimeMachine.Sounds;
            }
        }

        public PropsHandler Props
        {
            get
            {
                return TimeMachine.Props;
            }
        }

        public PlayersHandler Players
        {
            get
            {
                return TimeMachine.Players;
            }
        }

        public ScaleformsHandler Scaleforms
        {
            get
            {
                return TimeMachine.Scaleforms;
            }
        }

        public ParticlesHandler Particles
        {
            get
            {
                return TimeMachine.Particles;
            }
        }

        public ConstantsHandler Constants
        {
            get
            {
                return TimeMachine.Constants;
            }
        }

        public DecoratorsHandler Decorators
        {
            get
            {
                return TimeMachine.Decorators;
            }
        }

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
