using FusionLibrary;
using GTA;
using System.Collections.Generic;
using System.Linq;

namespace BackToTheFutureV
{
    internal class InternalInventory
    {
        private static List<InternalInventory> InternalInventories = new List<InternalInventory>();
        public static InternalInventory Current => InternalInventories.DefaultIfEmpty(new InternalInventory()).SingleOrDefault(x => x.Ped == FusionUtils.PlayerPed.Model);

        public Model Ped { get; }

        private int _trash;
        public int Trash
        {
            get => _trash;
            set
            {
                if (value >= 0 && value <= 5)
                    _trash = value;
            }
        }

        private int _plutonium = 1;
        public int Plutonium
        {
            get => _plutonium;
            set
            {
                if (value >= 0 && value <= 5)
                    _plutonium = value;
            }
        }

        private InternalInventory()
        {
            if (InternalInventories.SingleOrDefault(x => x.Ped == FusionUtils.PlayerPed.Model) != default)
                return;

            Ped = FusionUtils.PlayerPed.Model;
            InternalInventories.Add(this);
        }
    }
}
