using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace BackToTheFutureV.Story
{
    public class TimedEventManager
    {
        private List<TimedEvent> _timedEvents = new List<TimedEvent>();
        private int _newStep = 0;

        public bool ManageCamera = false;
        public bool IsCustomCameraActive { get; private set; }

        public int IndexOf(TimedEvent timedEvent) => _timedEvents.IndexOf(timedEvent);

        public TimedEvent Add(int startMinute, int startSecond, int startMillisecond, int endMinute, int endSecond, int endMillisecond, float tTimeMultiplier = 1.0f)
        {
            _timedEvents.Add(new TimedEvent(_newStep, new TimeSpan(0, 0, startMinute, startSecond, startMillisecond), new TimeSpan(0, 0, endMinute, endSecond, endMillisecond), tTimeMultiplier));

            _newStep += 1;

            return _timedEvents.Last();
        }

        public TimedEvent Add(TimeSpan startTime, TimeSpan endTime, float tTimeMultiplier = 1.0f)
        {
            _timedEvents.Add(new TimedEvent(_newStep, startTime, endTime, tTimeMultiplier));

            _newStep += 1;

            return _timedEvents.Last();
        }

        public bool AllExecuted(TimeSpan tCurrentTime)
        {
            return _timedEvents.TrueForAll(x => !x.FirstExecution && x.EndTime < tCurrentTime);
        }

        public TimedEvent Last => _timedEvents.Last();
       
        public void RunEvents(TimeSpan tCurrentTime)
        {
            List<TimedEvent> runningEvents = _timedEvents.Where(x => x.Run(tCurrentTime, ManageCamera)).ToList();

            if (runningEvents.Count > 0)
            {
                if (runningEvents.Where(x => x.IsSettingCamera).Count() == 0)
                {
                    ResetCamera();
                }
                else if (!IsCustomCameraActive)
                    IsCustomCameraActive = true;                    
            }
            else
            {
                ResetCamera();
            }

        }

        public void ResetCamera()
        {
            if (IsCustomCameraActive)
            {
                World.RenderingCamera = null;
                World.DestroyAllCameras();
                IsCustomCameraActive = false;
            }
        }

        public void ClearEvents()
        {
            _timedEvents.Clear();
            _newStep = 0;
        }
    }
}
