using System.Collections.Generic;

namespace FOMServer.World.Application.Items
{
    /// <summary>
    /// Minimal slice of the client item catalog needed for ammo handling: the
    /// combat items (weapons = category 3, ammunition = category 2) and, for each
    /// weapon, the ammo item type it consumes. Generated from the `item-table`
    /// skill definitions (ItemDefinition category @0x08, ammoType @0x64). Items
    /// absent here are treated as neither weapon nor ammo.
    /// </summary>
    internal static class ItemCatalogData
    {
        // type => (category, ammoType). ammoType is 0 for ammo/melee items.
        internal static Dictionary<ushort, (byte Category, ushort AmmoType)> Build() => new()
        {
            [1] = (3, 50),
            [2] = (3, 51),
            [3] = (3, 52),
            [4] = (3, 51),
            [5] = (3, 5),
            [6] = (3, 51),
            [9] = (3, 55),
            [10] = (3, 51),
            [11] = (3, 52),
            [12] = (3, 53),
            [15] = (3, 55),
            [16] = (3, 56),
            [17] = (3, 57),
            [18] = (3, 51),
            [19] = (3, 59),
            [20] = (3, 58),
            [21] = (3, 53),
            [23] = (3, 53),
            [24] = (3, 0),
            [25] = (3, 0),
            [28] = (3, 54),
            [29] = (3, 51),
            [30] = (3, 53),
            [31] = (3, 0),
            [32] = (3, 0),
            [33] = (3, 56),
            [34] = (3, 50),
            [35] = (3, 52),
            [37] = (3, 50),
            [50] = (2, 0),
            [51] = (2, 0),
            [52] = (2, 0),
            [53] = (2, 0),
            [54] = (2, 0),
            [55] = (2, 0),
            [56] = (2, 0),
            [57] = (2, 0),
            [58] = (2, 0),
            [59] = (2, 0),
        };
    }
}
