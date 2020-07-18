using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using GTA.Math;
using BackToTheFutureV.Entities;

namespace BackToTheFutureV
{
    public enum RCModes
    {
        WithCamera,
        WithoutCamera
    }

    public class RCHandler
    {
        public TimeCircuits TimeCircuits { get; }

        public bool IsRemoteControlling { get; private set; }

        public RCHandler(TimeCircuits circuits)
        {
            TimeCircuits = circuits;
        }

        public void StartRC()
        {
            IsRemoteControlling = true;
        }

        public void Process()
        {

        }
    }
}
