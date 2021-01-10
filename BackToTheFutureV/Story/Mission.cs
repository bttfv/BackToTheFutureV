using FusionLibrary;
using System.Windows.Forms;

namespace BackToTheFutureV.Story
{
    public abstract class Mission
    {
        public bool IsPlaying { get; protected set; }

        protected abstract void OnStart();
        protected abstract void OnEnd();
        public abstract void KeyDown(KeyEventArgs key);
        public abstract void Process();

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
    }
}
