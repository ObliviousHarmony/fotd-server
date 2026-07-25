using FOMServer.Master.Core.Networking;
using FOMServer.Master.Core.Players;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Networking;
using FOMServer.Shared.Core.PacketHandlers;
using FOMServer.Shared.Core.Repositories;
using FOMServer.Shared.Interop.FOMNetwork;
using FOMServer.Shared.Interop.FOMNetwork.Constants;
using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Enums.Item;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Interop.FOMNetwork.Structs;
using FOMServer.Shared.Metadata;

namespace FOMServer.Master.Application.PacketHandlers
{
    [PacketHandler]
    internal class CreateCharacterPacketHandler : PacketHandlerBase<CreateCharacterPacket>
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IClientRegistry _clientRegistry;
        private readonly IPlayerRegistry _playerRegistry;
        private readonly IClientPacketSender _clientPacketSender;
        private readonly ILogger<CreateCharacterPacketHandler> _logger;

        public CreateCharacterPacketHandler(
            IPlayerRepository playerRepository,
            IItemRepository itemRepository,
            IClientRegistry clientRegistry,
            IPlayerRegistry playerRegistry,
            IClientPacketSender clientPacketSender,
            ILogger<CreateCharacterPacketHandler> logger
        )
        {
            _playerRepository = playerRepository;
            _itemRepository = itemRepository;
            _clientRegistry = clientRegistry;
            _playerRegistry = playerRegistry;
            _clientPacketSender = clientPacketSender;
            _logger = logger;
        }

        public override void Handle(NetworkAddress sender, in CreateCharacterPacket p)
        {
            var session = _clientRegistry.Get(sender);
            if (session is null)
            {
                _logger.LogWarning("Dropping character creation from '{Sender}' with no registered session", sender);
                return;
            }

            using var response = new PacketWriter<LoginReturnPacket>(sender);
            ref var rData = ref response.Data;
            rData.PlayerId = p.PlayerId;

            if (session.Player is not null)
            {
                rData.Status = LoginReturnPacket.StatusCode.CreateCharacterError;
                _clientPacketSender.Send(response.Build());
                return;
            }

            if (!IsValidAvatar(p.Avatar))
            {
                rData.Status = LoginReturnPacket.StatusCode.CreateCharacterError;
                _clientPacketSender.Send(response.Build());
                return;
            }

            var existing = _playerRepository.GetByName(p.Name);
            if (existing is not null)
            {
                rData.Status = LoginReturnPacket.StatusCode.CreateCharacterError;
                _clientPacketSender.Send(response.Build());
                return;
            }

            var created = _playerRepository.Create(
                new()
                {
                    id = p.PlayerId,
                    name = p.Name,
                    sex = p.Avatar.Sex,
                    race = p.Avatar.Race,
                    face = p.Avatar.Face,
                    hair = p.Avatar.Hair,
                },
                p.Biography
            );

            if (created is null)
            {
                rData.Status = LoginReturnPacket.StatusCode.CreateCharacterError;
                _clientPacketSender.Send(response.Build());
                return;
            }

            if (!CreateClothes(p.PlayerId, p.Avatar))
            {
                _logger.LogError("Failed to create starter clothes for player {PlayerId}", p.PlayerId);

                rData.Status = LoginReturnPacket.StatusCode.CreateCharacterError;
                _clientPacketSender.Send(response.Build());
                return;
            }

            _clientRegistry.BeginLogin(session, p.PlayerId);
            _playerRegistry.Login(session);

            rData.Status = LoginReturnPacket.StatusCode.Success;
            rData.AccountType = AccountType.Prepaid;
            rData.LoginWorldId = WorldId.Manhattan;
            _clientPacketSender.Send(response.Build());
        }

        private bool IsValidAvatar(in AvatarInterop avatar)
        {
            if (!AvatarConstants.IsValidAvatar(avatar.Race, avatar.Sex, avatar.Face, avatar.Hair))
            {
                return false;
            }

            // Only allow for approved starter clothes.
            if (avatar.Sex == AvatarConstants.Sex.Male)
            {
                switch (avatar.Shirt)
                {
                    case ItemType.ExerciseTShirtMale:
                    case ItemType.CommandoJacketMale:
                    case ItemType.RiotTrenchcoatMale:
                    case ItemType.DefenderRobeMale:
                    case ItemType.SquadTShirtMale:
                    case ItemType.AlmJacketMale:
                        break;

                    default:
                        return false;
                }

                switch (avatar.Bottoms)
                {
                    case ItemType.BlueTacticalTrousersMale:
                    case ItemType.BattleTrousersMale:
                    case ItemType.AnarchyTrousersMale:
                    case ItemType.HarmonyTrousersMale:
                    case ItemType.LiberationTrousersMale:
                    case ItemType.NucleoTrousersMale:
                        break;

                    default:
                        return false;
                }

                switch (avatar.Shoes)
                {
                    case ItemType.BlackDressShoesMale:
                    case ItemType.IndirectDesignShoesMale:
                    case ItemType.FearToTreadShoesMale:
                    case ItemType.DiscreetDressShoesMale:
                    case ItemType.MilatechShoesMale:
                    case ItemType.EsporteComfortShoesMale:
                        break;

                    default:
                        return false;
                }
            }
            else if (avatar.Sex == AvatarConstants.Sex.Female)
            {
                switch (avatar.Shirt)
                {
                    case ItemType.DeathDealerTShirtFemale:
                    case ItemType.NeonMiningJacketFemale:
                    case ItemType.GrayDefenseTrenchcoatFemale:
                    case ItemType.ProtectorRobeFemale:
                    case ItemType.AdvocateTShirtFemale:
                    case ItemType.GrayDefenseJacketFemale:
                        break;

                    default:
                        return false;
                }

                switch (avatar.Bottoms)
                {
                    case ItemType.AssassinTrousersFemale907:
                    case ItemType.NeonSkirtFemale:
                    case ItemType.BrownAssaultTrousersFemale:
                    case ItemType.DiplomaticTrousersFemale:
                    case ItemType.PatrolmanTrousersFemale:
                    case ItemType.GrayAssaultTrousersFemale:
                        break;

                    default:
                        return false;
                }

                switch (avatar.Shoes)
                {
                    case ItemType.LizardTechBlueShoesfemale:
                    case ItemType.ScarpaSolidShoesFemale:
                    case ItemType.ZapatoDichromaticBootsFemale:
                    case ItemType.ZapatoLightAnkleBootsFemale:
                    case ItemType.ZapatoStuddedBootsFemale:
                    case ItemType.EsporteRunnerShoesfemale:
                        break;

                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool CreateClothes(uint playerId, AvatarInterop avatar)
        {
            var createdId = _itemRepository.Create(
                new()
                {
                    location_type = ItemLocationType.Inventory,
                    location_id = playerId,
                    slot = ItemSlotType.Shirt,
                    type = avatar.Shirt,
                }
            );
            if (createdId is null)
            {
                return false;
            }

            createdId = _itemRepository.Create(
                new()
                {
                    location_type = ItemLocationType.Inventory,
                    location_id = playerId,
                    slot = ItemSlotType.Bottoms,
                    type = avatar.Bottoms,
                }
            );
            if (createdId is null)
            {
                return false;
            }

            createdId = _itemRepository.Create(
                new()
                {
                    location_type = ItemLocationType.Inventory,
                    location_id = playerId,
                    slot = ItemSlotType.Shoes,
                    type = avatar.Shoes,
                }
            );
            if (createdId is null)
            {
                return false;
            }

            return true;
        }
    }
}
