using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Items;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Interop.FOMNetwork.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.World.Core.Players;
using FOMServer.World.Core.Players.Registration;

namespace FOMServer.World.Application.Players.Registration
{
    internal class PlayerLoader : IPlayerLoader
    {
        private readonly IPlayerRepository _playerRepository;

        public PlayerLoader(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public Player? Load(uint id)
        {
            var playerDto = _playerRepository.GetById(id);
            if (playerDto is null)
            {
                return null;
            }

            var attributes = LoadAttributes(id);
            var items = LoadItems(id);
            var quickslots = LoadQuickslots(id);

            var player = new Player(id, attributes, items, quickslots);

            return player;
        }

        private uint[] LoadAttributes(uint id)
        {
            var attributes = new uint[(int)AttributeType.NUM_ATTRIBUTE_TYPES];
            for (var i = 0; i < (int)AttributeType.NUM_ATTRIBUTE_TYPES; ++i)
            {
                attributes[i] = PlayerAttributes.GetMetadata((AttributeType)i).Default;
            }

            attributes[(int)AttributeType.Stamina] = 10000;

            return attributes;
        }

        private IDictionary<uint, Item> LoadItems(uint id)
        {
            Dictionary<uint, Item> loadedItems = [];

            return loadedItems;
        }

        private ItemType[] LoadQuickslots(uint id)
        {
            var quickslots = new ItemType[PlayerConstants.NumQuickslots];
            for (var i = 0; i < quickslots.Length; ++i)
            {
                quickslots[i] = ItemType.Invalid;
            }

            quickslots[1] = ItemType.AdrenalineAutoinjector;

            return quickslots;
        }
    }
}
