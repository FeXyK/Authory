namespace AuthoryServer.Entities
{
    public enum ModelType : byte
    {
        StandardPlayer = 0,
        BlackPlayer = 1,
        WhitePlayer = 2,
        RedPlayer = 3,
        GreenPlayer = 4,
        BluePlayer = 5,

        MeleeNPC = 50,
        WizardNPC = 51,
        RangerNPC = 52,

        TeleportResource = 254,
    }
}
