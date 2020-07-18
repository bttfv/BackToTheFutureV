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
        public OnCompleted OnCompleted { get; set; }

        public bool IsPlaying { get; protected set; }

        public abstract void Play();

        public abstract void Process();

        public abstract void Stop();

        public abstract void Dispose();
    }
}
