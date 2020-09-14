using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using KlangRageAudioLibrary;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class DoorInfo
    {
        public bool IsDoorFullyOpen;
        public bool IsDoorOpen;
    }

    public class SoundHandler : Handler
    {
        private readonly AudioPlayer _doorOpenSound;
        private readonly AudioPlayer _doorCloseSound;
        private readonly AudioPlayer _doorOpenColdSound;
        private readonly AudioPlayer _doorCloseColdsound;
        private readonly AudioPlayer _engineOnSound;
        private readonly AudioPlayer _engineOffsound;
        private readonly List<AudioPlayer> _doorSounds;

        private readonly Dictionary<VehicleDoorIndex, DoorInfo> _doorStatus = new Dictionary<VehicleDoorIndex, DoorInfo>()
        {
            {
                VehicleDoorIndex.FrontLeftDoor,
                new DoorInfo()
            },
            {
                VehicleDoorIndex.FrontRightDoor,
                new DoorInfo()
            }
        };

        private bool _engineOn;

        private int _gameTimer;

        public SoundHandler(TimeCircuits circuits) : base(circuits)
        {
            _doorOpenSound = circuits.AudioEngine.Create("general/doorOpen.wav", Presets.Exterior);
            _doorCloseSound = circuits.AudioEngine.Create("general/doorClose.wav", Presets.Exterior);
            _doorOpenColdSound = circuits.AudioEngine.Create("general/doorOpenCold.wav", Presets.Exterior);
            _doorCloseColdsound = circuits.AudioEngine.Create("general/doorCloseCold.wav", Presets.Exterior);
            _engineOffsound = circuits.AudioEngine.Create("general/engine/engineStop.wav", Presets.Exterior);
            _engineOnSound = circuits.AudioEngine.Create("general/engine/engineStart.wav", Presets.Exterior);

            _engineOffsound.SourceBone = "engine";
            _engineOnSound.SourceBone = "engine";

            _doorSounds = new List<AudioPlayer>
            {
                _doorOpenSound, _doorCloseSound, _doorOpenColdSound, _doorCloseColdsound
            };
        }

        public override void Dispose()
        {
            _doorOpenSound.Dispose();
            _doorCloseSound.Dispose();
            _doorOpenColdSound.Dispose();
            _doorCloseColdsound.Dispose();
            _engineOnSound.Dispose();
            _engineOffsound.Dispose();
        }

        public override void Process()
        {
            if (Game.GameTime < _gameTimer) return;
            if (!Vehicle.IsVisible) return;

            foreach (var door in _doorStatus.ToList())
            {
                var doorAngle = Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, Vehicle.Handle, (int) door.Key);

                // Detect door index (d -> driver side) (p - passenger side) for correct 3d sound
                var doorSide = door.Key == VehicleDoorIndex.FrontLeftDoor ? "d" : "p";
                var doorBone = $"handle_{doorSide}side_f";

                _doorSounds.ForEach(x => x.SourceBone = doorBone);
                if (doorAngle > 0 && !door.Value.IsDoorOpen)
                {
                    if (!IcePlaying)
                        _doorOpenSound.Play();
                    else
                        _doorOpenColdSound.Play();

                    _doorStatus[door.Key].IsDoorOpen = true;
                }
                else if (doorAngle == 0)
                {
                    _doorStatus[door.Key].IsDoorOpen = false;
                }

                if (doorAngle > 0.87f)
                {
                    _doorStatus[door.Key].IsDoorFullyOpen = true;
                }
                else if (doorAngle < 0.87f && door.Value.IsDoorFullyOpen)
                {
                    if (!IcePlaying)
                        _doorCloseSound.Play();
                    else
                        _doorCloseColdsound.Play();

                    _doorStatus[door.Key].IsDoorFullyOpen = false;
                }
            }

            if (Vehicle.IsEngineRunning && !_engineOn)
            {
                _engineOnSound.Play();
                _engineOn = true;
            }
            else if (!Vehicle.IsEngineRunning && _engineOn)
            {
                _engineOffsound.Play();
                _engineOn = false;
            }

            _gameTimer = Game.GameTime + 50;
        }

        public override void Stop()
        {
            _doorCloseSound.Stop();
            _doorOpenSound.Stop();
            _doorCloseColdsound.Stop();
            _doorOpenColdSound.Stop();
            _engineOffsound.Stop();
            _engineOnSound.Stop();
        }

        public override void KeyPress(Keys key) {}
    }
}