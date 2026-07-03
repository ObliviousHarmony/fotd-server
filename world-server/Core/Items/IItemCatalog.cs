namespace FOMServer.World.Core.Items
{
    /// <summary>
    /// Read-only lookup over the (combat) item catalog: enough to classify an item
    /// as a weapon or ammunition and resolve which ammo type a weapon consumes.
    /// </summary>
    internal interface IItemCatalog
    {
        /// <summary>True if the item type is a weapon (category 3).</summary>
        bool IsWeapon(ushort type);

        /// <summary>True if the item type is ammunition (category 2).</summary>
        bool IsAmmunition(ushort type);

        /// <summary>
        /// The ammo item type the given weapon consumes, or 0 if the weapon has no
        /// ammo (melee/tool) or the type is unknown.
        /// </summary>
        ushort GetAmmoType(ushort weaponType);
    }
}
