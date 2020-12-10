namespace Lidgren.Network.Shared
{
    /// <summary>
    /// Indicates the information in a byte for the client. 
    /// The client will be able to inform the player by a predefined text.
    /// </summary>
    public enum SystemMessageType : byte
    {
        /// <summary>
        /// Target out of range
        /// </summary>
        OutOfRange = 0,

        /// <summary>
        /// Skill interrupted becouse not enough resource
        /// </summary>
        NotEnoughMana = 1,

        /// <summary>
        /// The player died
        /// </summary>
        YouAreDead = 2,

        /// <summary>
        /// The character name is already in use.
        /// </summary>
        InvalidCharacterName = 101,
    }
}
