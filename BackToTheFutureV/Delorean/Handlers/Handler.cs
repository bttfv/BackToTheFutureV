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
        public DeloreanMods Mods => TimeCircuits.Delorean.Mods;

        public float MPHSpeed { get => TimeCircuits.MphSpeed; set => TimeCircuits.MphSpeed = value; }

        public TimeCircuitsScaleform GUI => TimeCircuits.Gui;

        public DateTime DestinationTime { get => TimeCircuits.DestinationTime; set => TimeCircuits.DestinationTime = value; }
        public DateTime PreviousTime { get => TimeCircuits.PreviousTime; set => TimeCircuits.PreviousTime = value; }

        public bool IsOn { get => TimeCircuits.IsOn; set => TimeCircuits.IsOn = value; }
        public bool IsFueled { get => TimeCircuits.IsFueled; set => TimeCircuits.IsFueled = value; }

        public bool IsRemoteControlled { get => TimeCircuits.IsRemoteControlled; set => TimeCircuits.IsRemoteControlled = value; }
       
        public bool IsFreezing { get => TimeCircuits.IsFreezing; set => TimeCircuits.IsFreezing = value; }
        public bool IcePlaying { get => TimeCircuits.IcePlaying; set => TimeCircuits.IcePlaying = value; }

        public bool IsFlying { get => TimeCircuits.IsFlying; set => TimeCircuits.IsFlying = value; }
        public bool IsBoosting { get => TimeCircuits.IsBoosting; set => TimeCircuits.IsBoosting = value; }
        public bool CanConvert { get => TimeCircuits.CanConvert; set => TimeCircuits.CanConvert = value; }
        public bool FlyingCircuitsBroken { get => TimeCircuits.FlyingCircuitsBroken; set => TimeCircuits.FlyingCircuitsBroken = value; }

        public void SetFlyMode(bool open, bool instant = false) => TimeCircuits?.FlyingHandler.SetFlyMode(open, instant);        
        
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
