using GTA;
using GTA.Math;

namespace FusionLibrary.Extensions
{
    public static class OtherExtensions
    {
        public static bool NotNullAndExists(this Camera camera)
        {
            return camera != null && camera.Exists();
        }

        public static bool IsCameraValid(this Camera camera)
        {
            return camera.NotNullAndExists() && camera.Position != Vector3.Zero;
        }
    }
}
