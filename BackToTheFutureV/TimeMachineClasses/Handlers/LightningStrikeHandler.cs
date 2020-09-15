﻿using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using KlangRageAudioLibrary;
using BackToTheFutureV.Entities;
using GTA.Native;
using BackToTheFutureV.Vehicles;
using BackToTheFutureV.TimeMachineClasses.RC;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class LightningStrikeHandler : Handler
    {
        private int _nextCheck;
                
        private int _flashes;
        private int _nextFlash;

        private int _nextForce;

        private bool _hasBeenStruckByLightning;

        public LightningStrikeHandler(TimeMachine timeMachine) : base(timeMachine)
        {
         
        }

        public override void Process()
        {
            if (_hasBeenStruckByLightning && _flashes <= 3)
            {
                if(Game.GameTime > _nextFlash)
                {
                    // Flash the screen
                    ScreenFlash.FlashScreen(0.25f);

                    // Update _flashes count
                    _flashes++;

                    // Update next flash
                    _nextFlash = Game.GameTime + 650;

                    // Dont do it anymore if flashed enough times
                    if (_flashes > 3)
                    {                        
                        _flashes = 0;
                        _hasBeenStruckByLightning = false;
                    }
                }

                if(Properties.IsFlying && Game.GameTime > _nextForce)
                {
                    Vehicle.ApplyForce(Vector3.RandomXYZ() * 3f, Vector3.RandomXYZ());

                    _nextForce = Game.GameTime + 100;
                }

                return;
            }

            if (!ModSettings.LightningStrikeEvent || World.Weather != Weather.ThunderStorm || Properties.TimeTravelPhase > TimeTravelPhase.OpeningWormhole || Game.GameTime < _nextCheck)
                return;
          
            if ((Mods.Hook == HookState.On && Vehicle.GetMPHSpeed() >= 88 && !Properties.IsFlying) | (Vehicle.HeightAboveGround >= 20 && Properties.IsFlying)) 
            {
                if (Utils.Random.NextDouble() < 0.2)
                    Strike();
                else
                    _nextCheck = Game.GameTime + 10000;
            }
        }

        private void Strike()
        {
            Properties.HasBeenStruckByLightning = true;

            if (Properties.AreTimeCircuitsOn)
            {
                // Time travel by lightning strike
                Sounds.LightningStrike.Play();

                if (Mods.Hook == HookState.On && !Properties.IsFlying)
                {                    
                    Events.StartTimeTravel?.Invoke(700);
                    _flashes = 2;
                }
                else
                {
                    Events.StartTimeTravel?.Invoke(2000);
                    _flashes = 0;
                }
                
                TimeMachineClone timeMachineClone = TimeMachine.Clone;
                timeMachineClone.Properties.DestinationTime = timeMachineClone.Properties.DestinationTime.AddYears(70);
                RemoteTimeMachineHandler.AddRemote(timeMachineClone);
            }
            else
                Function.Call(Hash.FORCE_LIGHTNING_FLASH);
           
            if (Properties.IsFlying)
            {
                Properties.AreFlyingCircuitsBroken = true;

                if (Properties.AreTimeCircuitsOn && (!Mods.IsDMC12 || Mods.Hoodbox == ModState.Off))
                    Events.SetTimeCircuitsBroken?.Invoke(true);
            }
                
            Vehicle.EngineHealth -= 700;

            _hasBeenStruckByLightning = true;
            _nextCheck = Game.GameTime + 60000;

            Events.OnLightningStrike?.Invoke();
        }

        public override void KeyDown(Keys key)
        {

        }

        public override void Stop()
        {
            
        }

        public override void Dispose()
        {

        }
    }
}