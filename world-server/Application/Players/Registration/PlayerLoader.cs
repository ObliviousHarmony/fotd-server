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
        private readonly IItemRepository _itemRepository;

        public PlayerLoader(IPlayerRepository playerRepository, IItemRepository itemRepository)
        {
            _playerRepository = playerRepository;
            _itemRepository = itemRepository;
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

            ReadOnlySpan<ItemType> quickslots =
            [
                playerDto.quickslot_1,
                playerDto.quickslot_2,
                playerDto.quickslot_3,
                playerDto.quickslot_4,
            ];

            var player = new Player(
                id,
                playerDto.name,
                playerDto.sex,
                playerDto.race,
                playerDto.face,
                playerDto.hair,
                attributes,
                items[ItemLocationType.Inventory],
                items[ItemLocationType.Equipment],
                quickslots
            );

            return player;
        }

        private uint[] LoadAttributes(uint playerId)
        {
            var attributes = new uint[(int)AttributeType.NUM_ATTRIBUTE_TYPES];
            for (var i = 0; i < (int)AttributeType.NUM_ATTRIBUTE_TYPES; ++i)
            {
                attributes[i] = PlayerAttributes.GetMetadata((AttributeType)i).Default;
            }

            attributes[(int)AttributeType.Stamina] = 10000;
            attributes[(int)AttributeType.MaxStamina] = 10000;

            return attributes;
        }

        private Dictionary<ItemLocationType, Dictionary<uint, Item>> LoadItems(uint playerId)
        {
            var itemDtos = _itemRepository.GetPlayerItems(playerId, WorldId.Manhattan);

            Dictionary<ItemLocationType, Dictionary<uint, Item>> loadedItems = [];

            loadedItems[ItemLocationType.Inventory] = [];
            loadedItems[ItemLocationType.Equipment] = [];

            foreach (var (id, dto) in itemDtos)
            {
                ReadOnlySpan<byte> balanceValues =
                [
                    dto.recipe_balance_1,
                    dto.recipe_balance_2,
                    dto.recipe_balance_3,
                    dto.recipe_balance_4,
                ];

                var item = new Item(
                    id,
                    dto.type,
                    dto.location_type,
                    dto.location_id,
                    dto.slot,
                    dto.value,
                    dto.value_max,
                    dto.durability,
                    dto.durability_loss_factor,
                    dto.security,
                    dto.rarity,
                    dto.creator_player_id,
                    dto.stolen_from_player_id,
                    dto.timeout,
                    dto.attribute_bonus,
                    dto.recipe_variation,
                    balanceValues
                );

                loadedItems[dto.location_type].Add(id, item);
            }

            return loadedItems;
        }
    }
}
