using System.Windows.Forms;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA;
using GTA.Math;
using GTA.Native;
using KlangRageAudioLibrary;
using Screen = GTA.UI.Screen;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class FreezeHandler : Handler
    {
        private int _gameTimer;
        private int _currentStep;

        private bool _doingFreezingSequence;

        private bool _fuelNotif;

        private float _smokeIndex;

        private float _iceMaxVal;
        private float _iceDisappearVal;
        private float _timeToDisappear = 360f; // 6 minutes

        public FreezeHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.OnReenterCompleted += OnReenterCompleted;
        }

        public void OnReenterCompleted()
        {
            StartFreezeHandling();
        }

        public void StartFreezeHandling(bool fuelNotify = true)
        {
            Stop();

            // Set maximum ice level depending on delorean type
            _iceMaxVal = Mods.Reactor == ReactorType.Nuclear ? 0.4f : 0.15f;

            Function.Call<float>(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, _iceMaxVal);

            Properties.IsFreezed = true;
            _iceDisappearVal = 0;
            _doingFreezingSequence = true;
            _fuelNotif = fuelNotify;
            _currentStep = 0;
        }

        public override void Process()
        {
            if (!Properties.IsFreezed)
            {
                Function.Call<float>(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, 0f);
                return;
            }

            // 0 is no ice
            var iceScale = Function.Call<float>(Hash.GET_VEHICLE_ENVEFF_SCALE, Vehicle);

            if(iceScale > 0f)
            {
                float newIce = Utils.Lerp(_iceMaxVal, 0f, _iceDisappearVal / _timeToDisappear);

                if (newIce <= 0.15f)
                {
                    SFX.IceSmoke?.StopNaturally();

                    foreach (var waterDrop in SFX.IceWaterDrops)
                        waterDrop?.StopNaturally();

                    Sounds.Ice.Stop();
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
                return;

            if (Game.GameTime < _gameTimer) 
                return;

            switch(_currentStep)
            {
                case 0:
                    // Set the ice
                    Function.Call(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, _iceMaxVal);

                    if (Mods.Reactor == ReactorType.Nuclear)
                    {
                        // Spawn the ice particles
                        SFX.IceSmoke?.Play();

                        foreach (var waterDrop in SFX.IceWaterDrops)
                        {
                            UpdateDoorIce(waterDrop.BoneName.Contains("left")
                                ? VehicleDoorIndex.FrontLeftDoor
                                : VehicleDoorIndex.FrontRightDoor, waterDrop);
                        }

                        Sounds.Ice.Play();
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
                    if (Mods.Reactor == ReactorType.Nuclear)
                        Sounds.IceVents.Play();

                    _currentStep++;
                    _gameTimer = Game.GameTime + 1000;
                    break;

                case 3:
                    if (Mods.Reactor == ReactorType.Nuclear)
                        for (; _smokeIndex < 7;)
                        {
                            SFX.IceVentLeftSmoke.Play();
                            SFX.IceVentRightSmoke.Play();

                            _gameTimer = Game.GameTime + 500;

                            _smokeIndex++;

                            return;
                        }

                    _currentStep++;
                    _gameTimer = Game.GameTime + 1000;
                    break;

                case 4:

                    if (_fuelNotif)
                        Events.StartFuelBlink?.Invoke();

                    _doingFreezingSequence = false;
                    break;
            }
        }
        private void UpdateDoorIce(VehicleDoorIndex doorIndex, PtfxEntityBonePlayer waterDrop)
        {
            if (waterDrop.IsPlaying)
                return;

            waterDrop.Play();
            waterDrop.Play();
        }

        public override void KeyPress(Keys key) { }

        public override void Stop()
        {
            _currentStep = 0;
            _gameTimer = 0;
            _smokeIndex = 0;
            _doingFreezingSequence = false;
            Properties.IsFreezed = false;

            Sounds.Ice.Stop();
            Sounds.IceVents.Stop();

            foreach (var waterDrop in SFX.IceWaterDrops)
                waterDrop.StopNaturally();

            SFX.IceSmoke?.StopNaturally();
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}
