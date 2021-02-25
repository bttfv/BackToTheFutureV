using FusionLibrary;
using GTA;
using System.Collections.Generic;

namespace BackToTheFutureV.Utility
{
    internal static class TrashHandler
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

        internal static void Process()
        {
            if (Utils.PlayerPed.IsInVehicle())
                return;

            Prop dumpster = World.GetClosestProp(Utils.PlayerPed.Position, 1.6f, dumpsterModels.ToArray());

            if (dumpster == null || !Game.IsControlJustPressed(Control.Context))
                return;

            InternalInventory.Current.Trash++;

            Utils.DisplayHelpText($"Found only some useless trash... ({InternalInventory.Current.Trash})");
        }
    }
}
