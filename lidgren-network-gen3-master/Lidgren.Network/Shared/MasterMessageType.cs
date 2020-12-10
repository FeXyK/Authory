namespace Lidgren.Network.Shared
{
    public enum MasterMessageType
    {
        GlobalChat,
        WorldChat,
        PrivateChat,

        NewServerConnection,
        NewAccountConnection,
        
        ServerConnectionRequest,
        ConnectionApproved,
        CharacterInfo,
        SuccessfullConnection,

        RequestMaps,
        RemoveMaps,
        NewNodeConnection,
        MapsCreated,
        RequestMap,
        RemoveMap,
        MapsRemoved,
        MapCreated,

        ChannelInfo,
        ChannelSwitchRequest,
        RequestCharacterInfo,
        CharacterNotFound,

        CreateCharacter,
        DeleteCharacter,
        RefreshCharacterList,
        RefreshMapList,
        Information,
        MapChangeRequest,
        LoadReport,
        Shutdown,
    }
}
