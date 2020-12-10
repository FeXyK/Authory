using System;
using System.Collections.Generic;
using Lidgren.Network;
using Lidgren.Network.Shared;
using AuthoryServer.Entities;

namespace AuthoryServer
{
    public class PlayerEntity : Entity
    {
        private const int MAX_BUFF_COUNT = 20;

        public long Uid { get; private set; }
        public NetConnection Connection { get; private set; }

        public bool InCombat { get; set; }
        public bool IsTargetable { get; private set; } = false;
        public bool isDead { get; set; }

        public ActionType Action { get; set; }

        public long Experience { get; set; }
        public long MaxExperience { get; set; }

        public int CharacterId { get; set; }
        public int AccountId { get; set; }

        public bool DirtyPos { get; set; }

        public float DistanceTravelled { get; set; }

        public PlayerEntity(string name, ModelType modelType, byte level, Vector3 position, NetConnection connection, AuthoryServer server, int characterId, int accountId)
        {
            this.Server = server;

            Mana = new Resource();
            Health = new Resource();

            this.EntityType = Entities.Enums.EntityType.Player;

            this.CharacterId = characterId;
            this.AccountId = accountId;

            this.Attributes = new int[Enum.GetValues(typeof(Attribute)).Length - 1];
            this.Buffs = new List<Buff>(MAX_BUFF_COUNT);
            this.Skills = new List<AbstractSkill>(20);

            this.Armor = 300;


            this.Name = name;
            this.ModelType = modelType;
            this.Position = position;
            this.Connection = connection;

            this.Level = level;

            if (connection != null)
                this.Uid = connection.RemoteUniqueIdentifier;

            SetStats();
            CalculateResources();
            Respawn();
        }

        public PlayerEntity(string name, ModelType modelType, Vector3 position, AuthoryServer server)
        {
            Server = server;


            Mana = new Resource();
            Health = new Resource();

            Armor = 300;
            Attributes = new int[Enum.GetValues(typeof(Attribute)).Length - 1];
            Buffs = new List<Buff>(MAX_BUFF_COUNT);
            Skills = new List<AbstractSkill>(20);

            EntityType = Entities.Enums.EntityType.Player;

            Name = name;
            Uid = new Random().Next(int.MaxValue);
            ModelType = modelType;

            Connection = null;

            Level = 99;
            SetStats();
            CalculateResources();
            Respawn();
        }

        public override void Tick()
        {
            if (EntityTick % 20 == 0)
            {
                DistanceTravelled = 0;
            }
            if (!isDead)
            {
                Regen();
                SkillTick();
                BuffTick();
            }
            else
            {
                if (Buffs.Count > 0)
                    RemoveAllBuffs();
                if (Skills.Count > 0)
                    Skills.Clear();
            }
            if (EntityTick > 50)
            {
                IsTargetable = true;
            }
            EntityTick++;
        }

        public override bool UseResource(SkillCostType costType, int value)
        {
            if (base.UseResource(costType, value))
                return true;
            else
                Server.OutgoingMessageHandler.SendSystemInfo(Connection, SystemMessageType.NotEnoughMana);
            return false;
        }

        public override void AddBuff(Buff buff)
        {
            buff.SetDurationBasedByTickRate(Server.PlayerTickRate);
            base.AddBuff(buff);
        }

        public override void AddSkill(AbstractSkill skill, Vector3 position)
        {
            Server.OutgoingMessageHandler.SendChannelingInfo(this);
            IsSkillCasting = true;
            Skills.Add(skill);
        }

        public override int TakeDamage(SkillSchool effectType, float damageMultiplier, Entity caster)
        {
            int damageDealt = base.TakeDamage(effectType, damageMultiplier, caster);
            Server.OutgoingMessageHandler.SendEntityUpdate(this);
            if (Health.Value <= 0)
            {
                isDead = true;
                Server.OutgoingMessageHandler.SendSystemInfo(Connection, SystemMessageType.YouAreDead);
            }
            return damageDealt;
        }

        public void Kill()
        {
            SetHealth(0);
            isDead = true;
            Server.OutgoingMessageHandler.SendSystemInfo(Connection, SystemMessageType.YouAreDead);

        }

        public override int TakeDamage(int value)
        {
            int damageDealt = base.TakeDamage(value);
            Server.OutgoingMessageHandler.SendEntityUpdate(this);
            return damageDealt;
        }


        public override void Respawn()
        {
            Action = ActionType.Idle;
            SetHealth(Health.MaxValue);
            SetMana(Mana.MaxValue);

            if (GridCell != null)
            {
                Position = new Vector3(GridCell.Area.X + 50, 0, GridCell.Area.Y + 50);

                Server.OutgoingMessageHandler.SendPlayerRespawn(this);
            }
        }

        public override void AddHealth(int health)
        {
            base.AddHealth(health);
            Server.OutgoingMessageHandler.SendEntityUpdate(this);

        }

        public override void SetHealth(int health = 1000)
        {
            Health.Value = health;
            //AddHealth(health);
            Server.OutgoingMessageHandler.SendEntityUpdate(this);
        }

        public override void SetStats(ushort END = 600, ushort STR = 10, ushort AGI = 10, ushort INT = 15, ushort KNW = 400, ushort LCK = 10)
        {
            GetAttribute(Attribute.Endurance) = END;
            GetAttribute(Attribute.Strength) = STR;
            GetAttribute(Attribute.Agility) = AGI;
            GetAttribute(Attribute.Intelligence) = INT;
            GetAttribute(Attribute.Knowledge) = KNW;
            GetAttribute(Attribute.Luck) = LCK;

            CalculateResources();
        }

        public void LevelUp()
        {
            Experience -= MaxExperience;
            MaxExperience = Level * Level * 100;
            Level++;


            Server.OutgoingMessageHandler.SendLevelUp(this);
            Server.OutgoingMessageHandler.SendLevelUpInfo(this);

            CalculateResources();
        }

        public override void AddExperience(long value)
        {
            Experience += value;
            if (Experience >= MaxExperience)
            {
                LevelUp();
            }
            else
            {
                Server.OutgoingMessageHandler.SendExperienceInfo(this, this.GetExperience());
            }
        }

        public override void CalculateResources()
        {
            Armor = (ushort)(100 * Level);
            MaxExperience = Level * Level * 100;

            GetAttribute(Attribute.Agility) = 20 + Level * 10;
            GetAttribute(Attribute.Intelligence) = 20 + Level * 10;
            GetAttribute(Attribute.Strength) = 20 + Level * 10;
            GetAttribute(Attribute.Endurance) = 20 + Level * 10;
            GetAttribute(Attribute.Knowledge) = 20 + Level * 10;
            GetAttribute(Attribute.Luck) = 20 + Level * 10;

            base.CalculateResources();

            Health.RegenValue = 10000;
            Server.OutgoingMessageHandler.SendAttributeUpdate(this);
        }

        public void SetConnection(NetConnection connection)
        {
            Connection = connection;
            Uid = connection.RemoteUniqueIdentifier;
        }

        private void Regen()
        {
            if (EntityTick % 60 == 0)
            {
                AddMana(Mana.RegenValue);
                AddHealth(Health.RegenValue);
            }
        }


        public override void AddMana(int mana)
        {
            base.AddMana(mana);

            Server.OutgoingMessageHandler.SendEntityUpdate(this);
        }

        public override NetConnection GetConnection()
        {
            return Connection;
        }

        public override long GetExperience()
        {
            return Experience;
        }

        public override void SetPosition(Vector3 position)
        {
            Position = position;
            DirtyPos = true;
            if (!GridCell.Area.Contains((int)Position.X, (int)Position.Z))
            {
                Server.Data.ReAdd(this);
            }
        }

        public void SetPositionWithoutGridCellCheck(Vector3 position)
        {
            Position = position;
            DirtyPos = true;
        }

        public override void Interact(PlayerEntity player)
        {
            Console.WriteLine($"{player.Name} interacted with PlayerEntity: {Id} {Name}");
        }

        public override string ToString()
        {
            string s = "";
            foreach (var buff in Buffs)
            {
                s += "\n" + buff;
            }
            return string.Format($"{Id}:\tName: {Name}\nHealth: {Health.Value}/{Health.MaxValue}\nMana: {Mana.Value}/{Mana.MaxValue}\nPos: {Position}\nGridCell: {GridCell}" +
                $"\nEntityTick: {EntityTick}\nPlayer Buffs:{s}");
        }
    }
}