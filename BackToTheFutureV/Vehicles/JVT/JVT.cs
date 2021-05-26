using GTA;
using GTA.Math;
using System.Windows.Forms;

namespace BackToTheFutureV
{
    internal class JVT
    {
        public Vehicle Train => CustomTrain?.Train;
        public Vehicle Tender => CustomTrain?.Carriage(1);

        public CustomTrain CustomTrain { get; private set; }

        public JVT(Vector3 position, bool direction)
        {
            CustomTrain = new CustomTrain(position, direction, 27, 1)
            {
                IsAccelerationOn = true,
                IsAutomaticBrakeOn = true,
                CanBeDriven = true,
                CheckDriver = true
            };

            JVTHandler.Add(this);
        }

        public void KeyDown(KeyEventArgs e)
        {

        }

        public void Tick()
        {
            CustomTrain?.Tick();
        }

        public void Dispose()
        {
            CustomTrain?.DeleteTrain();

            Train?.Delete();
            Tender?.Delete();
        }
    }
}
