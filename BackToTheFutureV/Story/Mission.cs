using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.Story
{
    public abstract class Mission
    {
        public bool IsPlaying { get; protected set; }

        public abstract void OnStart();
        public abstract void OnEnd();
        public abstract void Process();

        public TimedEventManager TimedEventManager = new TimedEventManager();
        public TimeSpan CurrentTime = new TimeSpan();

        public void Start()
        {
            if (!IsPlaying)
            {
                OnStart();

                MissionHandler.Add(this);
                IsPlaying = true;
            }         
        }

        public void End()
        {
            if (IsPlaying)
            {
                OnEnd();

                IsPlaying = false;
                MissionHandler.Delete(this);
            }            
        }
    }
}
