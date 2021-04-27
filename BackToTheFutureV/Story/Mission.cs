using FusionLibrary;
using System.Windows.Forms;

namespace BackToTheFutureV
{
    internal abstract class Mission
    {
        public bool IsPlaying { get; protected set; }

        protected abstract void OnStart();
        protected abstract void OnEnd();
        public abstract void KeyDown(KeyEventArgs key);
        public abstract void Tick();

        public TimedEventHandler TimedEventManager = new TimedEventHandler();

        public Mission()
        {
            MissionHandler.Add(this);
        }

        public void Start()
        {
            if (!IsPlaying)
            {
                IsPlaying = true;
                OnStart();
            }
        }

        public void End()
        {
            if (IsPlaying)
            {
                OnEnd();
                IsPlaying = false;
            }
        }

        public abstract void Abort();
    }
}
