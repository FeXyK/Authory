namespace Lidgren.Network.Shared
{
    public enum MessageType
    {
        Disconnect = 2,
        PlayerMovement = 10,
        MobMovementToPosition = 12,
        MovementSpeed = 13,
        PositionCorrection = 14,
        Action = 15,

        MobTargetInfo = 17,
        ClearTargetInfo = 18,


        LevelUp = 20,

        Teleport = 50,

        GridResourceEntityFullUpdate = 59,
        GridFullEntityUpdate = 60,
        MobRespawn = 65,

        AttributeUpdate = 70,

        PlayerID = 103,

        PlayerUpdate = 104,
        FullEntityUpdate = 105,
        MobUpdate = 106,
        Death = 107,

        DamageInfo = 108,

        SkillInterrupt = 109,
        Casting = 110,
        SkillInfo = 111,
        SkillInfoAlternativePosition = 112,
        SkillRequest = 113,
        SkillRequestRawPosition = 114,

        BuffApply = 115,
        BuffRefresh = 116,
        BuffRemove = 117,

        UpdateMaxExperience = 249,
        UpdateExperience = 250,
        Respawn = 253,


        SystemInfo = 254,
        Interact = 255,
    }
}