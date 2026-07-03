using FOMServer.World.Core.Items;

namespace FOMServer.World.Application.Items
{
    internal sealed class ItemCatalog : IItemCatalog
    {
        private const byte CategoryAmmunition = 2;
        private const byte CategoryWeapon = 3;

        private readonly Dictionary<ushort, (byte Category, ushort AmmoType)> _defs = ItemCatalogData.Build();

        public bool IsWeapon(ushort type)
            => _defs.TryGetValue(type, out var d) && d.Category == CategoryWeapon;

        public bool IsAmmunition(ushort type)
            => _defs.TryGetValue(type, out var d) && d.Category == CategoryAmmunition;

        public ushort GetAmmoType(ushort weaponType)
            => _defs.TryGetValue(weaponType, out var d) ? d.AmmoType : (ushort)0;
    }
}
