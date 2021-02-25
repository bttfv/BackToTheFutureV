using FusionLibrary;
using GTA;

namespace BackToTheFutureV.Utility
{
    internal static class TrashHandler
    {
        private static Model[] dumpsterModels { get; } = new Model[]
        {
            new Model("prop_bin_01a"),
            new Model("prop_bin_02a"),
            new Model("prop_bin_03a"),
            new Model("prop_bin_04a"),
            new Model("prop_bin_05a"),
            new Model("prop_bin_06a"),
            new Model("prop_bin_07a"),
            new Model("prop_bin_07b"),
            new Model("prop_bin_07c"),
            new Model("prop_bin_07d"),
            new Model("prop_bin_08a"),
            new Model("prop_bin_02open"),
            new Model("prop_bin_09a"),
            new Model("prop_bin_10a"),
            new Model("prop_bin_10b"),
            new Model("prop_bin_11a"),
            new Model("prop_bin_11b"),
            new Model("prop_bin_12a"),
            new Model("prop_bin_13a"),
            new Model("prop_bin_14a"),
            new Model("prop_bin_14b"),
            new Model("prop_bin_beach_01a"),
            new Model("prop_bin_beach_01d"),
            new Model("prop_bin_delpiero"),
            new Model("prop_bin_delpiero_b"),
            new Model("prop_dumpster_01a"),
            new Model("prop_dumpster_02a"),
            new Model("prop_dumpster_02b"),
            new Model("prop_dumpster_04a"),
            new Model("prop_dumpster_3a"),
            new Model("prop_dumpster_3step"),
            new Model("prop_dumpster_4a"),
            new Model("prop_dumpster_4b"),
            new Model("prop_recyclebin_01a"),
            new Model("prop_recyclebin_02_c"),
            new Model("prop_recyclebin_02_d"),
            new Model("prop_recyclebin_02a"),
            new Model("prop_recyclebin_02b"),
            new Model("prop_recyclebin_03_a"),
            new Model("prop_recyclebin_04_a"),
            new Model("prop_recyclebin_04_b"),
            new Model("prop_recyclebin_05_a"),
            new Model("prop_skip_01a"),
            new Model("prop_skip_02a"),
            new Model("prop_skip_03"),
            new Model("prop_skip_04")
        };

        internal static void Process()
        {
            if (Utils.PlayerPed.IsInVehicle())
                return;

            Prop dumpster = World.GetClosestProp(Utils.PlayerPed.Position, 1.6f, dumpsterModels);

            if (dumpster == null || !Game.IsControlJustPressed(Control.Context))
                return;

            InternalInventory.Current.Trash++;

            Utils.DisplayHelpText($"Found only some useless trash... ({InternalInventory.Current.Trash})");
        }
    }
}
