using FOMServer.Shared.Interop.FOMNetwork.Enums;
using FOMServer.Shared.Interop.FOMNetwork.Packets;
using FOMServer.Shared.Interop.FOMNetwork.Structs;

namespace FOMServer.World.Application.World
{
    /// <summary>
    /// Temporary world sync data that places one terminal of every service
    /// type (1-15) in a ring so each can be identified in the client. The
    /// prop values are exemplar rows from the original game's database.
    /// </summary>
    internal static class DummyWorldServices
    {
        private const short RingY = -78;

        private static readonly ServiceDefinition[] s_definitions =
        [
            new(
                WorldServiceType.RepairStation,
                748,
                546,
                Models.NfTerminal1,
                Skins.NfTerminal1,
                RenderStyles.Dot3Bump,
                800
            ),
            new(
                WorldServiceType.WorldMarket,
                470,
                620,
                Models.MarketingTerminal,
                Skins.Market2,
                RenderStyles.Default,
                250
            ),
            new(
                WorldServiceType.StorageAccess,
                172,
                646,
                Models.StorageAccess,
                Skins.StorageAccess,
                RenderStyles.Default,
                100
            ),
            new(WorldServiceType.ChemicalRefinery, -124, 655, Models.ChemLab, Skins.ChemLab, RenderStyles.Default, 120),
            new(
                WorldServiceType.ApartmentEntry,
                -520,
                626,
                Models.ApartmentEntry,
                Skins.ApartmentEntry,
                RenderStyles.Default,
                100
            ),
            new(
                WorldServiceType.PrionManager,
                -812,
                427,
                Models.ColonyControl,
                Skins.ColonyControl,
                RenderStyles.Default,
                100
            ),
            new(WorldServiceType.MedicalUnit, -869, 96, Models.Medic, Skins.Medic, RenderStyles.Default, 100),
            new(
                WorldServiceType.Production,
                -879,
                -285,
                Models.ProductionTerminal,
                Skins.ProductionTerminal,
                RenderStyles.Default,
                100
            ),
            new(
                WorldServiceType.SchematicMarket,
                -676,
                -592,
                Models.MarketingTerminal,
                Skins.Market1,
                RenderStyles.Default,
                250
            ),
            new(
                WorldServiceType.ItemRecycler,
                -76,
                -614,
                Models.NfTerminal2,
                Skins.NfTerminal2,
                RenderStyles.Dot3Bump,
                550
            ),
            new(
                WorldServiceType.VortexTerminal,
                297,
                -606,
                Models.PortalTerminal,
                Skins.Portal,
                RenderStyles.Default,
                170
            ),
            new(
                WorldServiceType.StandardMarket,
                873,
                3,
                Models.MarketingTerminal,
                Skins.Market1,
                RenderStyles.Default,
                250
            ),
            new(
                WorldServiceType.ClothingMarket,
                850,
                275,
                Models.MarketingTerminal,
                Skins.Market1,
                RenderStyles.Default,
                250
            ),
        ];

        public static WorldServiceType GetFromId(uint id)
        {
            return (WorldServiceType)id - 100;
        }

        public static void WriteTo(ref WorldObjectsPacket p)
        {
            p.Action = WorldObjectsPacket.WorldObjectsAction.WorldSync;

            p.Sync.NumServices = (uint)s_definitions.Length;
            for (var i = 0; i < s_definitions.Length; ++i)
            {
                var definition = s_definitions[i];
                ref var service = ref p.Sync.Services[i];

                service.Id = (uint)definition.Type;
                service.Type = definition.Type;
                service.ModelPaths = definition.Model;
                service.SkinPaths = definition.Skins;
                service.RenderStylePaths = definition.RenderStyle;
                service.Scale = definition.Scale;
                service.MoveToFloor = 1;
                service.IsSolid = 1;

                service.NumPlacements = 1;
                service.PlacementIds[0] = 100 + (uint)definition.Type;
                service.Placements[0] = new PositionRotationInterop
                {
                    Pos = new PositionInterop
                    {
                        X = definition.X,
                        Y = RingY,
                        Z = definition.Z,
                    },
                    Rot = 0,
                };
            }
        }

        private readonly record struct ServiceDefinition(
            WorldServiceType Type,
            short X,
            short Z,
            string Model,
            string Skins,
            string RenderStyle,
            ushort Scale
        );

        private static class Models
        {
            public const string NfTerminal1 = "Models/Props/Terminals/nf_terminal1.ltb";
            public const string NfTerminal2 = "Models/Props/Terminals/nf_terminal2.ltb";
            public const string MarketingTerminal = "Models/Props/Terminals/marketing_terminal.ltb";
            public const string StorageAccess = "Models/Props/Terminals/storageaccess.ltb";
            public const string ChemLab = "Models/Props/Terminals/chemlab.ltb";
            public const string ApartmentEntry = "Models/Props/Terminals/apartment_entry.ltb";
            public const string ColonyControl = "Models/Props/Terminals/colony_control.ltb";
            public const string Medic = "Models/Props/Terminals/medic.ltb";
            public const string ProductionTerminal = "Models/Props/Terminals/productionterminal.ltb";
            public const string PortalTerminal = "Models/Props/Terminals/portal_terminal.ltb";
        }

        private static class Skins
        {
            public const string NfTerminal1 =
                "skins/Props/Terminals/nf_terminal1_normal.dtx;skins/Props/Terminals/nf_terminal1.dtx";
            public const string NfTerminal2 =
                "skins/Props/Terminals/nf_terminal2_normal.dtx;skins/Props/Terminals/nf_terminal2.dtx";
            public const string Market1 =
                "skins/Props/Terminals/mt_1_1.dtx;skins/Props/Terminals/mt_1_2.dtx;skins/Props/Terminals/mt_1_3.dtx";
            public const string Market2 =
                "skins/Props/Terminals/mt_2_1.dtx;skins/Props/Terminals/mt_2_2.dtx;skins/Props/Terminals/mt_2_3.dtx";
            public const string StorageAccess = "skins/Props/Terminals/storageaccess.dtx";
            public const string ChemLab = "skins/Props/Terminals/chemlab.dtx";
            public const string ApartmentEntry = "skins/Props/Terminals/apartment_entry.dtx";
            public const string ColonyControl = "skins/Props/Terminals/colony_control.dtx";
            public const string Medic = "skins/Props/Terminals/medic.dtx";
            public const string ProductionTerminal = "skins/Props/Terminals/production_terminal.dtx";
            public const string Portal = "skins/Props/Terminals/portal_0.dtx";
        }

        private static class RenderStyles
        {
            public const string Default = "RS/default.ltb";
            public const string Dot3Bump = "RS/Dot3bump_2pass.ltb";
        }
    }
}
