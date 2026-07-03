namespace FOMServer.Shared.Core.Enums
{
    /// <summary>
    /// Discriminator carried by an <see cref="PacketIdentifier.ID_GAMEMASTER"/>
    /// packet identifying which staff command the client issued. Values mirror the
    /// client's command name-to-id table (CShell.dll FUN_100f8370). Only
    /// <see cref="Spawn"/> is handled server-side so far.
    /// </summary>
    public enum GamemasterCommand : ushort
    {
        Kick = 0x1,
        KickBan = 0x2,
        Locate = 0x3,
        Teleport = 0x4,
        Summon = 0x5,
        Invis = 0x6,
        God = 0x7,
        Shutdown = 0x8,
        Arrest = 0x9,
        WorldAnnounce = 0xa,
        GlobalAnnounce = 0xb,
        Vortex = 0xc,
        Spawn = 0xd,

        // Ids 0xe-0x14 exist on the wire but their command names are not yet
        // confirmed from the client (see "Game Master Commands" in the client vault).

        Delete = 0x15,
        DropInventory = 0x16,
    }
}
