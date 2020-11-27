using System;
using FusionLibrary;
using GTA;
using GTA.Math;
using GTA.Native;

namespace FusionLibrary
{
    public delegate void OnExecute(TimedEvent timedEvent);

    public enum CameraType
    {
        Position,        
        Entity,
        Custom
    }

    public class TimedEvent
    {
        public event OnExecute OnExecute;

        public int Step { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public TimeSpan Duration => EndTime - StartTime;
        public bool FirstExecution => _executionCount != 2;
        public bool OneShot { get; private set; } = false;

        public float CurrentFloat { get; private set; }
        public float StartFloat = 0;
        public float EndFloat = 0;
        private bool _setFloat = false;

        public double CurrentSpeed { get; private set; }    
        public int StartSpeed = 0;
        public int EndSpeed = 0;
        private bool _setSpeed = false;

        public bool IsSettingCamera { get; private set; }
        public Entity CameraOnEntity { get; private set; }
        public Vector3 CameraPosition { get; private set; }
        public CameraType _cameraType = CameraType.Position;
        public Vector3 LookAtPosition { get; private set; }
        public Entity LookAtEntity { get; private set; }
        public float FieldOfView { get; private set; }

        public Camera CustomCamera;
        private CameraType _lookAtType = CameraType.Position;    
        
        private int _executionCount = 0;

        private CustomCameraHandler _customCameraManager;
        private int _customCameraIndex = -1;

        public TimedEvent(int tStep, TimeSpan tStartTime, TimeSpan tEndTime, float tTimeMultiplier)
        {
            Step = tStep;

            if (tTimeMultiplier != 1.0f)
            {
                StartTime = new TimeSpan(Convert.ToInt64(Convert.ToSingle(tStartTime.Ticks) * tTimeMultiplier));
                EndTime = new TimeSpan(Convert.ToInt64(Convert.ToSingle(tEndTime.Ticks) * tTimeMultiplier));
            }
            else
            {
                StartTime = tStartTime;
                EndTime = tEndTime;
            }
        }

        public TimedEvent(int tStep, TimeSpan tStartTime, float tTimeMultiplier)
        {
            Step = tStep;

            if (tTimeMultiplier != 1.0f)
            {
                StartTime = new TimeSpan(Convert.ToInt64(Convert.ToSingle(tStartTime.Ticks) * tTimeMultiplier));
            }
            else
            {
                StartTime = tStartTime;                
            }

            EndTime = TimeSpan.Zero;

            OneShot = true;
        }

        public void ResetExecution()
        {
            _executionCount = 0;
        }

        public void SetSpeed(int tStartSpeedMPH, int tEndSpeedMPH)
        {
            StartSpeed = tStartSpeedMPH;
            EndSpeed = tEndSpeedMPH;

            _setSpeed = true;
        }

        public void SetFloat(float tStartFloat, float tEndFloat)
        {
            StartFloat = tStartFloat;
            EndFloat = tEndFloat;

            _setFloat = true;
        }

        public void SetCamera(Entity tOnEntity, Vector3 tCameraOffset, CameraType cameraType, Entity tLookAtEntity, Vector3 tLookAtOffset, CameraType lookAtType, float fieldOfView = -1)
        {
            _lookAtType = lookAtType;
            LookAtEntity = tLookAtEntity;
            LookAtPosition = tLookAtOffset;

            _cameraType = cameraType;
            CameraOnEntity = tOnEntity;
            CameraPosition = tCameraOffset;

            FieldOfView = fieldOfView;
            IsSettingCamera = true;
        }

        public void SetCamera(Vector3 tCameraPosition, Vector3 tLookAtPosition, float fieldOfView = -1)
        {
            _lookAtType = CameraType.Position;
            LookAtEntity = null;
            LookAtPosition = tLookAtPosition;

            _cameraType = CameraType.Position;
            CameraOnEntity = null;
            CameraPosition = tCameraPosition;

            FieldOfView = fieldOfView;
            IsSettingCamera = true;
        }

        public void SetCamera(CustomCameraHandler customCameraManager, int customCameraIndex)
        {
            _lookAtType = CameraType.Custom;
            _cameraType = CameraType.Custom;

            _customCameraManager = customCameraManager;
            _customCameraIndex = customCameraIndex;
            IsSettingCamera = true;
        }

        private void CalculateCurrentSpeed()
        {
            CurrentSpeed = (((EndSpeed - StartSpeed + 1) * 0.44704f) / Duration.TotalSeconds) * Game.LastFrameTime;
        }

        private void CalculateCurrentFloat()
        {
            CurrentFloat = ((EndFloat - StartFloat) / (float)Duration.TotalSeconds) * Game.LastFrameTime;
        }

        private void PlaceCamera()
        {
            switch (_cameraType)
            {
                case CameraType.Entity:
                    if (CustomCamera == null)
                        CustomCamera = World.CreateCamera(CameraOnEntity.GetOffsetPosition(CameraPosition), Vector3.Zero, FieldOfView == -1 ? GameplayCamera.FieldOfView : FieldOfView);
                    
                    CustomCamera.AttachTo(CameraOnEntity, CameraPosition);
                    break;
                case CameraType.Position:
                    if (CustomCamera == null)
                        CustomCamera = World.CreateCamera(CameraPosition, Vector3.Zero, FieldOfView == -1 ? GameplayCamera.FieldOfView : FieldOfView);
                    else
                        CustomCamera.Position = CameraPosition;
                    break;                    
            }            

            switch (_lookAtType)
            {
                case CameraType.Entity:
                    CustomCamera.PointAt(LookAtEntity, LookAtPosition);
                    break;
                case CameraType.Position:
                    CustomCamera.PointAt(LookAtPosition);
                    break;
            }

            if (_cameraType == CameraType.Custom && _lookAtType == CameraType.Custom)
                _customCameraManager.Show(_customCameraIndex);
            else
                World.RenderingCamera = CustomCamera;

            //Disable fake shake of the cars.
            Function.Call((Hash)0x84FD40F56075E816, 0);
        }

        public bool Run(TimeSpan tCurrentTime, bool tManageCamera = false)
        {
            bool ret = StartTime.TotalMilliseconds <= tCurrentTime.TotalMilliseconds && ((OneShot && _executionCount == 0) || tCurrentTime.TotalMilliseconds <= EndTime.TotalMilliseconds);

            if (ret) {
                if (_executionCount < 2)
                    _executionCount += 1;

                if (_setSpeed)
                    CalculateCurrentSpeed();

                if (_setFloat)
                    CalculateCurrentFloat();

                if (tManageCamera && IsSettingCamera && FirstExecution)
                    PlaceCamera();

                OnExecute?.Invoke(this);
            }                

            return ret;
        }
    }
}
