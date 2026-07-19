using System.Runtime.InteropServices;
using FOMServer.Shared.Core.Enums;
using FOMServer.Shared.Core.Packets.Types.Item;
using FOMServer.Shared.Metadata;

namespace FOMServer.Shared.Core.Packets
{
    [PacketId(PacketIdentifier.ID_WORLDSERVICE)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WorldService
    {
        public ActionType Action;
        public uint PlayerId;
        public uint Id;

        public uint ServiceId;
        public WorldServiceType ServiceType;

        public ItemList Storage;

        public enum ActionType : byte
        {
            Invalid = 0, // WORLD_SERVICE_ACTION_INVALID
            Open = 1, // WORLD_SERVICE_ACTION_OPEN
            OpenCloning = 2, // WORLD_SERVICE_ACTION_OPEN_CLONING
            OpenedStorage = 3, // WORLD_SERVICE_ACTION_OPENED_STORAGE
            OpenUnknown = 4, // WORLD_SERVICE_ACTION_OPEN_UNKNOWN
            Opened = 5, // WORLD_SERVICE_ACTION_OPENED
            Dev = 6, // WORLD_SERVICE_ACTION_DEV
            ManagerRequest = 7, // WORLD_SERVICE_ACTION_MANAGER_REQUEST
            ManagerReturn = 8, // WORLD_SERVICE_ACTION_MANAGER_RETURN
            ManagerUpdate = 9, // WORLD_SERVICE_ACTION_MANAGER_UPDATE
            ActivateControl = 10, // WORLD_SERVICE_ACTION_ACTIVATE_CONTROL
            OpenedPrisoner = 11, // WORLD_SERVICE_ACTION_OPENED_PRISONER
            PrisonBailSubmit = 12, // WORLD_SERVICE_ACTION_PRISON_BAIL_SUBMIT
            PrisonBailRequest = 13, // WORLD_SERVICE_ACTION_PRISON_BAIL_REQUEST
            PrisonBailReturn = 14, // WORLD_SERVICE_ACTION_PRISON_BAIL_RETURN
            PrisonManagerRequest = 15, // WORLD_SERVICE_ACTION_PRISON_MANAGER_REQUEST
            PrisonerManagerReturn = 16, // WORLD_SERVICE_ACTION_PRISON_MANAGER_RETURN
            PrisonManagerRelease = 17, // WORLD_SERVICE_ACTION_PRISON_MANAGER_RELEASE
            Close = 18, // WORLD_SERVICE_ACTION_PRISON_CLOSE
        }
    }
}
