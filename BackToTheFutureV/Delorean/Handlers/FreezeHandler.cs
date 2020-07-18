using System.Windows.Forms;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using KlangRageAudioLibrary;
using Screen = GTA.UI.Screen;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class FreezeHandler : Handler
    {
        private int _gameTimer;
        private int _currentStep;

        private readonly AudioPlayer _coldAudio;
        private readonly AudioPlayer _ventAudio;

        private readonly PtfxEntityPlayer _leftSmokePtfx;
        private readonly PtfxEntityPlayer _rightSmokePtfx;

        private readonly PtfxEntityPlayer _iceSmoke;

        private readonly PtfxEntityBonePlayer[] _waterDrops;

        private bool _doingFreezingSequence;

        private bool _fuelNotif;

        private float _smokeIndex;

        private float _iceMaxVal;
        private float _iceDisappearVal;
        private float _timeToDisappear = 360f; // 6 minutes

        public FreezeHandler(TimeCircuits circuits) : base(circuits)
        {
            _coldAudio = circuits.AudioEngine.Create("general/cold.wav", Presets.ExteriorLoop);
            _ventAudio = circuits.AudioEngine.Create("general/vents.wav", Presets.Exterior);

            _coldAudio.FadeOutMultiplier = 0.15f;

            _leftSmokePtfx = new PtfxEntityPlayer(
                "scr_familyscenem", 
                "scr_meth_pipe_smoke", 
                Vehicle, 
                new Vector3(0.5f, -2f, 0.7f), 
                new Vector3(10f, 0, 180f), 
                10f, 
                false, 
                false);
            _rightSmokePtfx = new PtfxEntityPlayer(
                "scr_familyscenem", 
                "scr_meth_pipe_smoke", 
                Vehicle, 
                new Vector3(-0.5f, -2f, 0.7f), 
                new Vector3(10f, 0, 180f), 
                10f, 
                false, 
                false);
            _iceSmoke = new PtfxEntityPlayer(
                "core", 
                "ent_amb_dry_ice_area", 
                Vehicle, 
                new Vector3(0, 0, 0.5f),
                Vector3.Zero, 
                3f, 
                true);

            // 3 water drips dummies on each door
            _waterDrops = new PtfxEntityBonePlayer[3 * 2];
            for (int i = 0, c = 0; i < _waterDrops.Length / 2; i++)
            {
                for (int k = 0; k < 2; k++)
                {
                    string dummyName = k == 0 ? "ice_drop_left" : "ice_drop_right";
                    dummyName += i + 1;

                    _waterDrops[c] = new PtfxEntityBonePlayer(
                        "scr_apartment_mp", 
                        "scr_apa_jacuzzi_drips", 
                        Vehicle, 
                        dummyName, 
                        Vector3.Zero, 
                        Vector3.Zero, 
                        3f, 
                        true);
                    c++;
                }
            }

        }

        public void StartFreezeHandling(bool fuelNotify = true)
        {

            // Set maximum ice level depending on delorean type
            _iceMaxVal = Mods.Reactor == ReactorType.Nuclear ? 0.4f : 0.15f;

            Function.Call<float>(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, _iceMaxVal);

            IsFreezing = true;
            _iceDisappearVal = 0;
            _doingFreezingSequence = true;
            _fuelNotif = fuelNotify;
            _currentStep = 0;
        }

        public override void Process()
        {
            if (!IsFreezing)
            {
                Function.Call<float>(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, 0f);
                return;
            }

            if(!Vehicle.IsVisible)
                Stop();

            // 0 is no ice
            var iceScale = Function.Call<float>(Hash.GET_VEHICLE_ENVEFF_SCALE, Vehicle);

            if(iceScale > 0f)
            {
                float newIce = Utils.Lerp(_iceMaxVal, 0f, _iceDisappearVal / _timeToDisappear);

                if (newIce <= 0.15f)
                {
                    _iceSmoke?.StopNaturally();

                    foreach (var waterDrop in _waterDrops)
                        waterDrop?.StopNaturally();

                    _coldAudio.Stop();
                    IcePlaying = false;
                }

                Function.Call<float>(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, newIce);

                _iceDisappearVal += Game.LastFrameTime;
            }
            else
            {
                IsFreezing = false;
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
                        _iceSmoke?.Play();

                        foreach (var waterDrop in _waterDrops)
                        {
                            UpdateDoorIce(waterDrop.BoneName.Contains("left")
                                ? VehicleDoorIndex.FrontLeftDoor
                                : VehicleDoorIndex.FrontRightDoor, waterDrop);
                        }

                        _coldAudio.Play();
                        IcePlaying = true;
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
                        _ventAudio.Play();

                    _currentStep++;
                    _gameTimer = Game.GameTime + 1000;
                    break;

                case 3:
                    if (Mods.Reactor == ReactorType.Nuclear)
                        for (; _smokeIndex < 7;)
                        {
                            _rightSmokePtfx.Play();
                            _leftSmokePtfx.Play();

                            _gameTimer = Game.GameTime + 500;

                            _smokeIndex++;

                            return;
                        }

                    _currentStep++;
                    _gameTimer = Game.GameTime + 1000;
                    break;

                case 4:

                    if(_fuelNotif)
                        TimeCircuits.GetHandler<FuelHandler>().BlinkFuel();

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
            IsFreezing = false;

            _coldAudio.Stop();
            _ventAudio.Stop();

            foreach (var waterDrop in _waterDrops)
                waterDrop.StopNaturally();

            _iceSmoke?.StopNaturally();
        }

        public override void Dispose()
        {
            Stop();

            _coldAudio.Dispose();
            _ventAudio.Dispose();
        }
    }
}
