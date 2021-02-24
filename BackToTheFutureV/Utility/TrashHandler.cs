using FusionLibrary;
using GTA;
using System.Collections.Generic;

namespace BackToTheFutureV.Utility
{
    public static class TrashHandler
    {
        private static List<Model> dumpsterModels { get; } = new List<Model>
        {
            new Model("prop_dumpster_01a"),
            new Model("prop_dumpster_02a"),
            new Model("prop_dumpster_02b"),
            new Model("prop_dumpster_04a"),
            new Model("prop_dumpster_4b"),
            new Model("prop_dumpster_3a")
        };

        public static void Process()
        {
            Prop dumpster = World.GetClosestProp(Utils.PlayerPed.Position, 1.5f, dumpsterModels.ToArray());

            if (dumpster == null || Utils.PlayerVehicle != null || !Game.IsControlJustPressed(Control.Context))
                return;

            InternalInventory.Current.Trash++;

            Utils.DisplayHelpText($"Found only some useless trash... ({InternalInventory.Current.Trash})");
        }
    }
}
