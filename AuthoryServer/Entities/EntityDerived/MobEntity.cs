using System;
using System.Linq;
using System.Collections.Generic;
using Lidgren.Network.Shared;

namespace AuthoryServer.Entities
{
    /// <summary>
    /// Entity that logic is simulated on the server
    /// </summary>
    public class MobEntity : Entity
    {
        private const int MIN_MOVEMENT_DISTANCE = 10;
        private const int MAX_MOVEMENT_DISTANCE = 20;
        private const int RESPAWN_TIME = 600;

        //Mobs will do something when their tick dividable by this
        private ushort ACTION_TICK = 300;
        private float MAX_DISTANCE_FROM_SPAWN = 100f * 100f;
        private float SQR_AGGRO_RANGE = 20f * 20f;
        private ushort ATK_CDR = 30;

        public Entity Target { get; protected set; }

        public int DeathTimer { get; private set; }
        public bool IsReseting { get; private set; } = false;

        public ushort AttackCooldown { get; private set; }

        public float DistanceFromTarget { get; private set; }
        public float DistanceFromSpawn { get; private set; }
        public Vector3 SpawnPosition { get; private set; }
        public Vector3 MovementDirection { get; private set; }

        public AbstractSkill MobSkill { get; private set; }

        public MobEntity(ModelType modelType, ushort s1, ushort s2, ushort s3, ushort s4, ushort s5, ushort s6, byte level, AbstractSkill mobSkill)
        {
            Health = new Resource();
            Mana = new Resource();

            MobSkill = mobSkill;
            Attributes = new int[6];
            Buffs = new List<Buff>(20);

            Target = null;
            ModelType = modelType;
            Name = ModelType.ToString();

            Level = level;

            SetStats(s1, s2, s3, s4, s5, s6);
        }

        public MobEntity(MobEntity entity, Vector3 position, AuthoryServer server)
        {
            Health = new Resource();
            Mana = new Resource();

            this.MobSkill = entity.MobSkill;
            this.Server = server;

            this.ACTION_TICK += (ushort)(new Random().Next(0, 40));

            this.Attributes = new int[entity.Attributes.Length];

            this.Buffs = new List<Buff>(20);
            this.Skills = new List<AbstractSkill>(20);

            this.EntityTick = 0;

            this.SetPosition(position);

            this.SpawnPosition = position;
            this.MovementDirection = new Vector3();


            Target = null;

            ModelType = entity.ModelType;
            Name = entity.Name;

            Level = entity.Level;

            entity.Attributes.CopyTo(Attributes, 0);

            CalculateResources();

            Health.MaxValue = entity.Health.MaxValue;
            Health.RegenValue = entity.Health.RegenValue;

            Respawn();
        }

        /// <summary>
        /// Called every MobUpdateTick() from the server loop.
        /// </summary>
        public override void Tick()
        {
            EntityTick++;
            //If the entity went too far from the spawn position, it will run back there.
            if (IsReseting)
            {
                MoveByDirection();
                Regen();
                SetTarget(null);
                if (Vector3.SqrDistance(Position, EndPosition) < 1f)
                {
                    IsReseting = false;
                }
                return;
            }
            //If the entity is dead, it waits for its respawn.
            else if (Health.Value <= 0)
            {
                DeathTimer++;
                if (DeathTimer > RESPAWN_TIME)
                {
                    Respawn();
                }
            }
            //If the entity is alive, it updates it's Buffs, Skills, and regenerates Health overtime.
            else
            {
                BuffTick();
                SkillTick();
                Regen();
                //If the entity is casting a skill it will not do anything else.
                if (IsSkillCasting) return;
                //If the entity has no target.
                if (Target == null)
                {
                    //Searching for nearby players.
                    Entity newTarget = QueryNearby();
                    if (newTarget == null)
                    {
                        //After a period of time the entity will change its poisition if it has no target.
                        if (EntityTick % ACTION_TICK == 0 && ActionType == ActionType.Idle)
                        {
                            ChangeRandomDirection();
                        }
                        //Move into the MoveDirection
                        MoveByDirection();
                    }
                    else
                    {
                        //If the entity is close to an entity it will Target it.
                        SetTarget(newTarget);
                    }
                }
                else
                {
                    //If the Entity's Target is dead it will set the Entity Target to null.
                    if (Target.Health.Value == 0)
                    {
                        SetTarget(null);
                        ChangeRandomDirection();
                    }
                    else
                    {
                        //Check for distance from target.
                        DistanceFromTarget = Vector3.SqrDistance(Position, Target.Position);

                        //If the distance from target is less than the MobSkill's attack range it will attack.
                        if (DistanceFromTarget < MobSkill.MaxTargetRange * MobSkill.MaxTargetRange) AttackTarget();
                        //If the distance from target is more than the attack range it will follow it.
                        else if (Target != null) Follow();
                    }
                }

                //If the target is too far from spawn position it will go reset to the spawn position.
                if (DistanceFromSpawn > MAX_DISTANCE_FROM_SPAWN) Reset();
            }
        }

        public void Reset()
        {
            IsReseting = true;
            EndPosition = SpawnPosition;

            MovementDirection = (SpawnPosition - Position).Normalize() / Server.MobTickRate;
            MovementDirection *= MovementSpeed;
            SetTarget(null);

            ActionType = ActionType.PathTo;
            Server.OutgoingMessageHandler.SendMobPathToPosition(this);
        }

        public override void Respawn()
        {
            if (Level < 10)
            {
                GetAttribute(Attribute.Endurance) += 10;
                GetAttribute(Attribute.Strength) += 10;
                GetAttribute(Attribute.Agility) += 10;
                GetAttribute(Attribute.Intelligence) += 10;
                GetAttribute(Attribute.Knowledge) += 10;
                GetAttribute(Attribute.Luck) += 10;

                Level++;
                CalculateResources();
            }
            Skills.Clear();
            RemoveAllBuffs();

            SetTarget(null);
            SetPosition(SpawnPosition);
            Health.SetFull();

            MovementDirection = new Vector3(0, 0, 0);

            DeathTimer = 0;

            Server.OutgoingMessageHandler.SendEntityRespawn(this);
        }

        public override void AddBuff(Buff buff)
        {
            buff.SetDurationBasedByTickRate(Server.MobTickRate);
            base.AddBuff(buff);
        }

        public override void SetHealth(int health)
        {
            Health.Value = health;
            Server.OutgoingMessageHandler.SendEntityUpdate(this);
        }

        public override void AddHealth(int health)
        {
            if (!Health.IsFull())
            {
                base.AddHealth(health);
                Server.OutgoingMessageHandler.SendEntityUpdate(this);
            }
        }

        /// <summary>
        /// If this Entity dies, adds experience to the damage dealer.
        /// </summary>
        /// <param name="effectType">Effect type of the skill.</param>
        /// <param name="damageMultiplier">Skill damage multiplier.</param>
        /// <param name="caster">Damage dealer.</param>
        /// <returns></returns>
        public override int TakeDamage(SkillSchool effectType, float damageMultiplier, Entity caster)
        {
            int damageDealt = 0;
            if (Health.Value > 0)
            {
                damageDealt = base.TakeDamage(effectType, damageMultiplier, caster);
                Server.OutgoingMessageHandler.SendEntityUpdate(this);
                if (Health.Value > 0)
                {
                    if (!IsReseting)
                        SetTarget(caster);
                    if (new Random().Next(0, 100) < 10)
                        AddSkill(SkillFactory.Instance.GetSkill(SkillID.Rage).Create(this, caster, Server));
                }
                else
                {
                    SetTarget(null);
                    caster.AddExperience(Level * 100);
                }
            }
            return damageDealt;
        }

        public override int TakeDamage(int value)
        {
            int damageDealt = base.TakeDamage(value);
            Server.OutgoingMessageHandler.SendEntityUpdate(this);
            return damageDealt;
        }

        public void AttackTarget()
        {
            if (AttackCooldown < EntityTick)
            {
                AttackCooldown = (ushort)(EntityTick + ATK_CDR);
                if (!Skills.Any(x => x.SkillId == MobSkill.SkillId))
                    AddSkill(MobSkill.Copy().Create(this, Target, Server));
            }
        }

        public override void AddSkill(AbstractSkill skill, Vector3 position)
        {
            Skills.Add(skill);
            Server.OutgoingMessageHandler.SendChannelingInfo(this);
        }

        public void SetTarget(Entity target)
        {
            Target = target;
        }

        public void MoveByDirection()
        {
            Move(MovementDirection);
            if (GridCell != null)
                if (!GridCell.Area.Contains((int)Position.X, (int)Position.Z))
                {
                    Server.Data.ReAdd(this);
                }

            if (EntityTick % 10 == 0)
            {
                DistanceFromSpawn = Vector3.SqrDistance(Position, SpawnPosition);
            }

            if (Vector3.SqrDistance(Position, EndPosition) < 1f)
            {
                MovementDirection = new Vector3(0, 0, 0);
                ActionType = ActionType.Idle;
            }
        }

        public void Follow()
        {
            Vector3 newEndPosition = Target.Position + (Position - Target.Position).Normalize() * MobSkill.MaxTargetRange;

            MovementDirection = (newEndPosition - Position).Normalize() / Server.MobTickRate;
            MovementDirection *= MovementSpeed;

            if (Vector3.SqrDistance(EndPosition, newEndPosition) > 1f)
            {
                EndPosition = newEndPosition;
                Server.OutgoingMessageHandler.SendMobPathToPosition(this);
            }
            MoveByDirection();
        }

        public void Move(Vector3 unitVector)
        {
            Position += unitVector;
        }

        public void Regen()
        {
            if (EntityTick % 60 == 0)
            {
                AddHealth(Health.RegenValue);
                AddMana(Mana.RegenValue);
            }
        }

        public PlayerEntity QueryNearby()
        {
            if (GridCell != null)
                foreach (var player in GridCell.PlayersById.Values)
                {
                    if (player.IsTargetable && Vector3.SqrDistance(Position, player.Position) < SQR_AGGRO_RANGE && player.Health.Value > 0)
                    {
                        return player;
                    }
                }
            return null;
        }

        public void ChangeRandomDirection()
        {
            MovementDirection = Vector3.f_RandomRangeCircle() / Server.MobTickRate;
            MovementDirection *= MovementSpeed;
            EndPosition = SpawnPosition + MovementDirection * new Random().Next(MIN_MOVEMENT_DISTANCE, MAX_MOVEMENT_DISTANCE);

            MovementDirection = (EndPosition - Position).Normalize() / Server.MobTickRate;
            MovementDirection *= MovementSpeed;
            ActionType = ActionType.PathTo;

            Server.OutgoingMessageHandler.SendMobPathToPosition(this);
        }

        public void Calm()
        {
            GetAttribute(Attribute.Endurance) /= 2;
            GetAttribute(Attribute.Strength) /= 2;
            GetAttribute(Attribute.Agility) /= 2;
            GetAttribute(Attribute.Intelligence) /= 2;
            GetAttribute(Attribute.Knowledge) /= 2;
            GetAttribute(Attribute.Luck) /= 2;

            CalculateResources();
            Server.OutgoingMessageHandler.SendEntityUpdate(this);
        }

        public override void SetStats(ushort END, ushort STR, ushort AGI, ushort INT, ushort KNW, ushort LCK)
        {
            GetAttribute(Attribute.Endurance) = END;
            GetAttribute(Attribute.Strength) = STR;
            GetAttribute(Attribute.Agility) = AGI;
            GetAttribute(Attribute.Intelligence) = INT;
            GetAttribute(Attribute.Knowledge) = KNW;
            GetAttribute(Attribute.Luck) = LCK;
        }

        public override void CalculateResources()
        {
            base.CalculateResources();

            Server.OutgoingMessageHandler.SendAttributeUpdate(this);
        }

        public override void Interact(PlayerEntity player)
        {
            Console.WriteLine($"{player.Name} interacted with MobEntity: {Id} {Name}");
        }

        public override string ToString()
        {
            string s = "";
            foreach (var buff in Buffs)
            {
                s += "\n" + buff;
            }
            return string.Format($"{Id}:\tName: {Name}\nHealth: {Health.Value}/{Health.MaxValue}\nMana: {Mana.Value}/{Mana.MaxValue}\nPos: {Position}\nGridCell: {GridCell}" +
                $"\nEntityTick: {EntityTick}\nPlayer Buffs:{s}\nMovDir: {MovementDirection}\n MoveSpeed: {MovementSpeed} TargetId: {Target?.Id}");
        }

    }
}
