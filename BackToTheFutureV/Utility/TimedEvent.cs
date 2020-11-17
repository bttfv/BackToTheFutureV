using System;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;

namespace BackToTheFutureV.Story
{
    public delegate void OnExecute(TimedEvent timedEvent);

    public enum CameraType
    {
        Position,        
        Entity
    }

    public class TimedEvent
    {
        public event OnExecute OnExecute;

        public int Step { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public TimeSpan Duration => EndTime - StartTime;
        public bool FirstExecution => _executionCount != 2;

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

        public Camera CustomCamera;
        private CameraType _lookAtType = CameraType.Position;
        private bool _updateCamera = false;
        private bool _disableUpdate = false;        
        
        private int _executionCount = 0;

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

        public void SetCamera(Entity tOnEntity, Vector3 tCameraOffset, CameraType cameraType, Entity tLookAtEntity, Vector3 tLookAtOffset, CameraType lookAtType, bool tUpdateCamera = true)
        {
            _lookAtType = lookAtType;
            LookAtEntity = tLookAtEntity;
            LookAtPosition = tLookAtOffset;

            _cameraType = cameraType;
            CameraOnEntity = tOnEntity;
            CameraPosition = tCameraOffset;

            _updateCamera = tUpdateCamera;
            IsSettingCamera = true;
        }

        public void SetCamera(Vector3 tCameraPosition, Vector3 tLookAtPosition, bool tUpdateCamera = true)
        {
            _lookAtType = CameraType.Position;
            LookAtEntity = null;
            LookAtPosition = tLookAtPosition;

            _cameraType = CameraType.Position;
            CameraOnEntity = null;
            CameraPosition = tCameraPosition;

            _updateCamera = tUpdateCamera;
            IsSettingCamera = true;
        }

        private void CalculateCurrentSpeed()
        {
            CurrentSpeed = (Utils.MphToMs(EndSpeed - StartSpeed + 2) / Duration.TotalSeconds) * Game.LastFrameTime;
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
                        CustomCamera = World.CreateCamera(CameraOnEntity.GetOffsetPosition(CameraPosition), Vector3.Zero, GameplayCamera.FieldOfView);
                    else
                        CustomCamera.Position = CameraOnEntity.GetOffsetPosition(CameraPosition);                    
                    break;
                case CameraType.Position:
                    if (CustomCamera == null)
                        CustomCamera = World.CreateCamera(CameraPosition, Vector3.Zero, GameplayCamera.FieldOfView);
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

            World.RenderingCamera = CustomCamera;

            if (!_updateCamera)
                _disableUpdate = true;
        }

        public bool Run(TimeSpan tCurrentTime, bool tManageCamera = false)
        {            
            bool ret = StartTime.TotalMilliseconds <= tCurrentTime.TotalMilliseconds && tCurrentTime.TotalMilliseconds <= EndTime.TotalMilliseconds;

            if (ret) {
                if (_executionCount < 2)
                    _executionCount += 1;

                if (_setSpeed)
                    CalculateCurrentSpeed();

                if (_setFloat)
                    CalculateCurrentFloat();

                if (tManageCamera && IsSettingCamera && !_disableUpdate)
                    PlaceCamera();

                OnExecute?.Invoke(this);
            }                

            return ret;
        }

    }
}
