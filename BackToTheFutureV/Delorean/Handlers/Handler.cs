using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackToTheFutureV.Delorean;
using BackToTheFutureV.GUI;
using GTA;

namespace BackToTheFutureV.Delorean.Handlers
{
    public abstract class Handler
    {
        public TimeCircuits TimeCircuits { get; }

        public Vehicle Vehicle => TimeCircuits.Vehicle;
        public DeloreanType DeloreanType => TimeCircuits.DeloreanType;
        public string LowerCaseDeloreanType => TimeCircuits.LowerCaseDeloreanType;
        public TimeCircuitsScaleform GUI => TimeCircuits.Gui;
        public DateTime DestinationTime { get => TimeCircuits.DestinationTime; set => TimeCircuits.DestinationTime = value; }
        public DateTime PreviousTime { get => TimeCircuits.PreviousTime; set => TimeCircuits.PreviousTime = value; }
        public bool IsOn { get => TimeCircuits.IsOn; set => TimeCircuits.IsOn = value; }
        public bool IsRemoteControlled { get => TimeCircuits.IsRemoteControlled; set => TimeCircuits.IsRemoteControlled = value; }
        public bool IsFueled { get => TimeCircuits.IsFueled; set => TimeCircuits.IsFueled = value; }
        public bool IsFreezing { get => TimeCircuits.IsFreezing; set => TimeCircuits.IsFreezing = value; }
        public bool IsFlying => TimeCircuits.IsFlying;
        public bool IcePlaying { get => TimeCircuits.IcePlaying; set => TimeCircuits.IcePlaying = value; }
        public float MPHSpeed { get => TimeCircuits.MphSpeed; set => TimeCircuits.MphSpeed = value; }
        public DeloreanMods Mods => TimeCircuits.Delorean.Mods;
        public bool IsOnTracks { get => TimeCircuits.IsOnTracks; set => TimeCircuits.IsOnTracks = value; }
        public bool IsAttachedToRogersSierra { get => TimeCircuits.IsAttachedToRogersSierra; set => TimeCircuits.IsAttachedToRogersSierra = value; }

        public Handler(TimeCircuits circuits)
        {
            TimeCircuits = circuits;
        }

        public abstract void KeyPress(System.Windows.Forms.Keys key);
        public abstract void Process();
        public abstract void Stop();
        public abstract void Dispose();
    }
}
