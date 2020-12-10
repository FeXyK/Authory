namespace AuthoryMasterServer
{
    public class Character
    {
        public Account Account { get; set; }
        public int CharacterId { get; set; }
        public string Name { get; set; }


        public float PositionX { get; set; }
        public float PositionZ { get; set; }

        public byte ModelType { get; set; }
        public byte Level { get; set; }
        public long Experience { get; set; }
        public int MapIndex { get; set; }
        public int Health { get; set; }
        public int Mana { get; set; }

        public Character() { }

        public Character(Account account, int characterId, string name, float positionX, float positionZ, byte modelType, byte level, int experience, int mapIndex)
        {
            Account = account;
            CharacterId = characterId;
            Name = name;
            PositionX = positionX;
            PositionZ = positionZ;
            ModelType = modelType;
            Level = level;
            Experience = experience;
            MapIndex = mapIndex;
        }

        public override string ToString()
        {
            return string.Format($"{Account.AccountName}\n{CharacterId}: {Name}, Lv.{Level} Model: {ModelType}\n");
        }
    }
}

