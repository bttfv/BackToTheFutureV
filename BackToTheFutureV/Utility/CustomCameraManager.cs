using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;

namespace BackToTheFutureV.Utility
{
    public enum CameraSwitchType
    {
        Instant,
        Animated
    }

    public class CustomCameraManager
    {
        public List<CustomCamera> Cameras { get; private set; } = new List<CustomCamera>();
        public int CurrentCameraIndex { get; private set; } = -1;

        public bool CycleCameras { get; set; } = false;

        private int _CycleInterval = 10000;

        public int CycleInterval
        {
            get
            {
                return _CycleInterval;
            }
            set
            {
                if (CycleCameras)
                {
                    nextChange -= _CycleInterval;
                    nextChange += value;
                }

                _CycleInterval = value;
            }
        }

        private int nextChange = 0;

        public CustomCamera CurrentCamera
        {
            get
            {
                if (CurrentCameraIndex == -1)
                    return null;
                else
                    return Cameras[CurrentCameraIndex];
            }
        }

        public CustomCamera Add(Entity entity, Vector3 positionOffset, Vector3 pointAtOffset, float fieldOfView)
        {
            CustomCamera ret = new CustomCamera(entity, positionOffset, pointAtOffset, fieldOfView);

            Cameras.Add(ret);

            return ret;
        }

        public void Show(int index, CameraSwitchType cameraSwitchType = CameraSwitchType.Instant)
        {
            if (Cameras.Count == 0)
                return;

            if (index > Cameras.Count - 1 || index < 0)
                return;

            CustomCamera customCamera = CurrentCamera;

            Cameras[index].Show(ref customCamera);
            CurrentCameraIndex = index;
        }

        public void ShowNext(CameraSwitchType cameraSwitchType = CameraSwitchType.Instant)
        {
            if (Cameras.Count == 0)
                return;

            if (CurrentCameraIndex == Cameras.Count - 1)
                Abort();
            else
            {
                CustomCamera customCamera = CurrentCamera;

                Cameras[CurrentCameraIndex + 1].Show(ref customCamera, cameraSwitchType);
                CurrentCameraIndex += 1;
            }
        }

        public void ShowPrevious(CameraSwitchType cameraSwitchType = CameraSwitchType.Instant)
        {
            if (Cameras.Count == 0)
                return;

            if (CurrentCameraIndex <= 0)
                Abort();
            else
            {
                CustomCamera customCamera = CurrentCamera;

                Cameras[CurrentCameraIndex - 1].Show(ref customCamera, cameraSwitchType);
                CurrentCameraIndex -= 1;
            }
        }

        public void Stop()
        {
            if (CurrentCameraIndex > -1)
            {
                CurrentCamera.Stop();
                CurrentCameraIndex = -1;
            }
        }

        public void Abort()
        {
            Stop();

            Cameras.ForEach(x =>
            {
                x.Abort();
            });

            World.DestroyAllCameras();
        }

        public void Process()
        {           
            if (CycleCameras)
            {
                if (nextChange < Game.GameTime)
                {
                    ShowNext();
                    nextChange = Game.GameTime + CycleInterval;
                }
            }

            if (CurrentCameraIndex > -1 && !CurrentCamera.Camera.IsActive)
                Stop();
        }
    }
}
