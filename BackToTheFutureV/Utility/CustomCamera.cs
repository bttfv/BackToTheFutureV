using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;

namespace BackToTheFutureV.Utility
{
    public class CustomCamera
    {
        public Entity Entity { get; private set; }
        public Vector3 PositionOffset { get; private set; }
        public Vector3 PointAtOffset { get; private set; }
        public float FieldOfView { get; private set; }

        public Camera Camera { get; private set; }

        public CustomCamera(Entity entity, Vector3 positionOffset, Vector3 pointAtOffset, float fieldOfView)
        {
            Entity = entity;
            PositionOffset = positionOffset;
            PointAtOffset = pointAtOffset;
            FieldOfView = fieldOfView;
        }

        public void Show(ref CustomCamera OldCamera, CameraSwitchType cameraSwitchType = CameraSwitchType.Instant)
        {
            if (Camera == null || Camera.Exists() == false)
            {
                Camera = World.CreateCamera(Entity.Position, Entity.Rotation, FieldOfView);

                Camera.AttachTo(Entity, PositionOffset);
                Camera.PointAt(Entity, PointAtOffset);
            }

            if (OldCamera == null || OldCamera.Camera == null || OldCamera.Camera.Exists() == false)
                World.RenderingCamera = Camera;
            else
            {
                Camera.IsActive = true;
                OldCamera.Camera.IsActive = false;
                
                if (cameraSwitchType == CameraSwitchType.Animated)
                    OldCamera.Camera.InterpTo(Camera, 900, 1, 1);
                else
                    World.RenderingCamera = Camera;
            }
        }

        public void Stop()
        {
            Camera.IsActive = false;

            World.RenderingCamera = null;
        }

        public void Abort()
        {
            if (Camera != null)
            {
                Camera.Delete();
                Camera = null;
            }
        }
    }
}
