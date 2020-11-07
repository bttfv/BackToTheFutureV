using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using KlangRageAudioLibrary;
using Screen = GTA.UI.Screen;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class TimeCircuitsErrorHandler : Handler
    {
        private readonly DateTime _errorDate = new DateTime(1885, 1, 1, 12, 0, 0);

        private readonly Dictionary<int, double> _probabilities = new Dictionary<int, double>()
        {
            {
                300, 0.1
            },
            {
                270, 0.2
            },
            {
                240, 0.3
            },
            {
                210, 0.4
            },
            {
                180, 0.5
            },
            {
                150, 0.6
            },
            {
                120, 0.7
            },
            {
                100, 0.8
            }
        };

        private int nextCheck;

        public TimeCircuitsErrorHandler(TimeMachine timeMachine) : base(timeMachine)
        {

        }

        private double GetProbabilityForDamage(int damage)
        {
            KeyValuePair<int, double> selectedProb = new KeyValuePair<int, double>(0, 0);
            int lastDiff = 1000;

            foreach(var prob in _probabilities)
            {
                var diff = Math.Abs(prob.Key - damage);
                if (diff < lastDiff)
                {
                    selectedProb = prob;
                    lastDiff = diff;
                }
            }

            return selectedProb.Value;
        }

        public override void Process()
        {
            if (Vehicle == null || !Vehicle.Exists() || !Properties.AreTimeCircuitsOn || Properties.DestinationTime == _errorDate || (Vehicle.Health > 300 && Properties.TimeTravelsCount < 5))
                return;

            if(Game.GameTime > nextCheck)
            {             
                if (Vehicle.Health < 300)
                {
                    var randomNum = Utils.Random.NextDouble();

                    if (randomNum < GetProbabilityForDamage((Vehicle.Health < 100 ? 100 : Vehicle.Health)))
                    {
                        // Set TCD error stuff
                        Sounds.TCDGlitch?.Play();

                        Events.StartTimeCircuitsGlitch?.Invoke(false);

                        nextCheck = Game.GameTime + 60000;
                    }
                    else
                    {
                        // Update check
                        nextCheck = Game.GameTime + 3000;
                    }
                } 
                else if (Properties.TimeTravelsCount > 4)
                {
                    if (Utils.Random.NextDouble() < 0.25f)
                    {
                        // Set TCD error stuff
                        Sounds.TCDGlitch?.Play();

                        Events.StartTimeCircuitsGlitch?.Invoke(true);

                        nextCheck = Game.GameTime + 120000;
                    }
                    else
                    {
                        // Update check
                        nextCheck = Game.GameTime + 60000;
                    }
                }
            }
        }

        public override void Dispose()
        {
            
        }

        public override void KeyDown(Keys key)
        {
        }

        public override void Stop()
        {
        }
    }
}