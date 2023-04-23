using FusionLibrary;
using GTA;
using GTA.Native;
using System.Windows.Forms;
using static BackToTheFutureV.InternalEnums;

namespace BackToTheFutureV
{
    internal class FreezeHandler : HandlerPrimitive
    {
        private int _gameTimer;
        private int _currentStep;

        private bool _wasStruck;

        private bool _doingFreezingSequence;

        private bool _fuelNotif;

        private float _smokeIndex;

        private float _iceDisappearVal;
        private readonly float _timeToDisappear = 90f; // 360 = 6 minutes

        private bool _resuming;

        public FreezeHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnReenterStarted += OnReenterStarted;
            Events.OnReenterEnded += OnReenterEnded;
            Events.SetFreeze += SetFreeze;
        }

        public void SetFreeze(bool state, bool resume = false)
        {
            if (state)
            {
                StartFreezeHandling(false, resume);
            }
            else
            {
                Stop();
            }
        }

        private void OnReenterStarted()
        {
            if (Properties.HasBeenStruckByLightning)
            {
                _wasStruck = true;
            }
            else
            {
                _wasStruck = false;
            }
        }

        public void OnReenterEnded()
        {
            if ((FusionUtils.CurrentTime.AddSeconds(-FusionUtils.CurrentTime.Second) - Properties.DestinationTime).TotalMinutes <= 1)
                StartFreezeHandling();
        }

        public void StartFreezeHandling(bool fuelNotify = true, bool resume = false)
        {
            Stop();

            if (!resume)
            {
                // Set maximum ice level depending on delorean type
                Properties.IceValue = Mods.Reactor == ReactorType.Nuclear ? 0.4f : 0.15f;
            }

            Function.Call<float>(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, Properties.IceValue);

            Properties.IsFreezed = true;

            _resuming = resume;
            _iceDisappearVal = 0;
            _doingFreezingSequence = true;
            _fuelNotif = fuelNotify;
            _currentStep = 0;
        }

        public override void Tick()
        {
            if (!Properties.IsFreezed)
            {
                Function.Call<float>(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, 0f);
                return;
            }

            if (!Vehicle.IsVisible)
            {
                Stop();
                return;
            }

            // 0 is no ice
            float iceScale = Function.Call<float>(Hash.GET_VEHICLE_ENVEFF_SCALE, Vehicle);

            if (iceScale > 0f)
            {
                float newIce = FusionUtils.Lerp(Properties.IceValue, 0f, _iceDisappearVal / _timeToDisappear);

                if (newIce <= 0.15f)
                {
                    Particles?.IceSmoke?.Stop();

                    foreach (ParticlePlayer waterDrop in Particles?.IceWaterDrops)
                    {
                        waterDrop?.Stop();
                    }

                    Sounds.Ice?.Stop();
                    Properties.IsDefrosting = false;
                }

                Function.Call<float>(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, newIce);

                _iceDisappearVal += Game.LastFrameTime;
            }
            else
            {
                Properties.IsFreezed = false;
                Stop();
            }

            if (!_doingFreezingSequence)
            {
                return;
            }

            if (Game.GameTime < _gameTimer)
            {
                return;
            }

            switch (_currentStep)
            {
                case 0:
                    // Set the ice
                    Function.Call(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, Properties.IceValue);

                    if (Mods.Reactor == ReactorType.Nuclear)
                    {
                        // Spawn the ice particles
                        Particles?.IceSmoke?.Play();

                        foreach (ParticlePlayer waterDrop in Particles?.IceWaterDrops)
                        {
                            UpdateDoorIce(waterDrop);
                        }

                        Sounds.Ice?.Play();
                        Properties.IsDefrosting = true;
                    }

                    _gameTimer = Game.GameTime + 2000;
                    _currentStep++;
                    break;

                case 1:
                    _gameTimer = Game.GameTime + 15000;
                    _currentStep++;
                    break;

                case 2:
                    if (Mods.Reactor == ReactorType.Nuclear && !_resuming && !_wasStruck)
                    {
                        Sounds.IceVents?.Play();
                    }

                    _currentStep++;
                    _gameTimer = Game.GameTime + 1000;
                    break;

                case 3:
                    if (Mods.Reactor == ReactorType.Nuclear && !_wasStruck)
                    {
                        for (; _smokeIndex < 7;)
                        {
                            Particles?.IceVentLeftSmoke?.Play();
                            Particles?.IceVentRightSmoke?.Play();

                            _gameTimer = Game.GameTime + 500;

                            _smokeIndex++;

                            return;
                        }
                    }

                    _currentStep++;
                    _gameTimer = Game.GameTime + 1000;
                    break;

                case 4:

                    if (_fuelNotif && !_wasStruck)
                    {
                        Events.StartFuelBlink?.Invoke();
                    }

                    _doingFreezingSequence = false;
                    break;
            }
        }
        private void UpdateDoorIce(ParticlePlayer waterDrop)
        {
            if (waterDrop.IsPlaying)
            {
                return;
            }

            waterDrop.Play();
            waterDrop.Play();
        }

        public override void KeyDown(KeyEventArgs e) { }

        public override void Stop()
        {
            _resuming = false;
            _currentStep = 0;
            _gameTimer = 0;
            _smokeIndex = 0;
            _doingFreezingSequence = false;
            Properties.IsDefrosting = false;
            Properties.IsFreezed = false;

            Sounds.Ice?.Stop(!Vehicle.IsVisible);
            Sounds.IceVents?.Stop(!Vehicle.IsVisible);

            foreach (ParticlePlayer waterDrop in Particles?.IceWaterDrops)
            {
                waterDrop.Stop();
            }

            Particles?.IceSmoke?.Stop();
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}
