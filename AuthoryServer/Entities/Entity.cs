using System;
using System.Linq;
using System.Collections.Generic;
using Lidgren.Network;
using Lidgren.Network.Shared;
using AuthoryServer.Entities.Enums;

namespace AuthoryServer.Entities
{

    /// <summary>
    /// Base of every entity in the server.
    /// </summary>
    public abstract class Entity : EntityBase
    {
        /// <summary>
        /// If something needs to be debugged according to the entity this can be used. 
        /// When an entity takes damage it's value set to true.
        /// </summary>
        public bool DEBUG { get; set; } = false;


        public byte Level { get; protected set; }

        public EntityType EntityType { get; protected set; }
        public ActionType ActionType { get; protected set; }


        public Resource Health { get; protected set; }
        public Resource Mana { get; protected set; }

        public ushort Armor { get; protected set; }
        public ushort MagicResist { get; protected set; }

        public float MovementSpeed { get; private set; } = 10.0f;

        /// <summary>
        /// The position of the entity where it is moving towards.
        /// </summary>
        public Vector3 EndPosition { get; protected set; }

        public Vector3 ChannelingPosition { get; protected set; }

        public int[] Attributes { get; protected set; }

        /// <summary>
        /// This skill contains the Skills that were casted and still active by the Entity.
        /// </summary>
        public List<AbstractSkill> Skills { get; protected set; }
        /// <summary>
        /// This list containst the Buffs that are active on the Entity.
        /// </summary>
        public List<Buff> Buffs { get; protected set; }


        /// <summary>
        /// If Entity is Casting returns true.
        /// </summary>
        public bool IsSkillCasting { get; set; }


        public abstract void SetHealth(int health);
        public abstract void Respawn();
        public abstract void SetStats(ushort END, ushort STR, ushort AGI, ushort INT, ushort KNW, ushort LCK);
        public abstract void AddSkill(AbstractSkill skill, Vector3 position);

        public void SetMovementSpeed(float mvSpeed)
        {
            MovementSpeed = mvSpeed;
            Server.OutgoingMessageHandler.SendMovementSpeedChange(this);
        }

        /// <summary>
        /// Updates every active skill in the SkillList by calling their Tick() method. 
        /// If the skill's SkillState is Handled or Interrupted it removes the skill from the list.
        /// </summary>
        public void SkillTick()
        {
            for (int i = Skills.Count - 1; i >= 0; i--)
            {
                Skills[i].OnTick();

                if (Skills[i].State == SkillState.Handled || Skills[i].State == SkillState.Interrupted)
                {
                    Skills.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Updates every Buff in the BuffList by calling their Tick() method. 
        /// Checks for the buff expiration if it is expired it calls the OnEnd() method of the buff and removes it from the list.
        /// </summary>
        public void BuffTick()
        {
            for (int i = Buffs.Count - 1; i >= 0; i--)
            {
                Buffs[i].OnTick(this);
                BuffExpirationCheck(i);
            }
        }

        /// <summary>
        /// Checks for the buff expiration, if expired removes it from the buff list.
        /// </summary>
        /// <param name="i"></param>
        public void BuffExpirationCheck(int i)
        {
            if (Buffs[i].ExpirationTick < EntityTick)
            {
                RemoveBuff(i);
            }
        }

        /// <summary>
        /// Removes all buffs from the bufflist
        /// </summary>
        public void RemoveAllBuffs()
        {
            for (int i = Buffs.Count - 1; i >= 0; i--)
            {
                RemoveBuff(i);
            }
        }

        /// <summary>
        /// Calls the OnEnd() method of the buff at the given index from the buff list, and removes it from the buff list. 
        /// At the end Recalucates the Attributes of the Entity.
        /// </summary>
        /// <param name="buffIndex">The index of the buff in the buff list</param>
        public void RemoveBuff(int buffIndex)
        {
            //Remove buff effects
            Buffs[buffIndex].OnEnd(this);
            Buffs.RemoveAt(buffIndex);

            CalculateResources();
        }

        public ref int GetAttribute(Attribute attribute)
        {
            return ref Attributes[(int)attribute];
        }

        public virtual void AddExperience(long value) { }
        public virtual long GetExperience() { return 0; }

        /// <summary>
        /// When a skill is requested it gets added into the list by this method. 
        /// Checks if there is any skill by the same skill in the skill list if yes, 
        /// checks it's cooldown if it is still on cooldown, it will not put the skill into the list.
        /// </summary>
        /// <param name="skill">The skill that's requested for casting.</param>
        /// <returns></returns>
        public virtual bool AddSkill(AbstractSkill skill)
        {
            if (CooldownCheck(skill))
            {
                Console.WriteLine("ON CD");
                return false;
            }

            IsSkillCasting = true;
            ChannelingPosition = Position;

            Skills.Add(skill);
            Server.OutgoingMessageHandler.SendChannelingInfo(this);
            Server.OutgoingMessageHandler.SendEntityUpdate(this);
            return true;
        }

        /// <summary>
        /// Checks if the skill is still on cooldown.
        /// </summary>
        /// <param name="skill"></param>
        /// <returns>Returns false if skill list contains skill and  cooldown is less than 0. Returns true if skill list not contains skill. 
        /// Returns true if the skill is contained but cooldown is more than 0. </returns>
        private bool CooldownCheck(AbstractSkill skill)
        {
            if (Skills.Any(x => (x.SkillId == skill.SkillId && x.Cooldown > 0)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the gridCell of the entity, and informs the new gridCell's neighbours by the new entity.
        /// </summary>
        /// <param name="gridCell">The gridCell that contains the Entity.</param>
        public override void SetGridCell(GridCell gridCell)
        {
            GridCell = gridCell;
            Server.OutgoingMessageHandler.SendFullEntityUpdate(this);
        }

        /// <summary>
        /// Deals damage to this Entity. 
        /// The damage is calculated by the Skill's damageMultiplier and the according attributes for the SkillSchool. 
        /// The damage also affected by the Armor or MagicResist of the entity, depends on the SkillSchool. 
        /// Also the damage can be a critical hit, if it is the case the damage will be doubled. 
        /// At the end the damage will be sent to both the Caster and the Victim. 
        /// </summary>
        /// <param name="skillSchool">The school of the skill like elements: Fire, Arcane, Physical...</param>
        /// <param name="damageMultiplier">The damage multiplier of the skill.</param>
        /// <param name="caster">The caster of the skill, who is dealing the damage.</param>
        /// <returns></returns>
        public virtual int TakeDamage(SkillSchool skillSchool, float damageMultiplier, Entity caster)
        {
            DEBUG = true;

            int damageDealt = 0;
            bool isCrit = caster.GetAttribute(Attribute.Luck) >= new Random().Next(0, 100);
            switch (skillSchool)
            {
                case SkillSchool.Arcane:
                case SkillSchool.Water:
                case SkillSchool.Fire:
                case SkillSchool.Lightning:
                case SkillSchool.Earth:
                case SkillSchool.Dark:
                case SkillSchool.Light:
                    damageDealt = (int)(damageMultiplier * caster.GetAttribute(Attribute.Intelligence));
                    break;
                case SkillSchool.Physical:
                    damageDealt = (int)(damageMultiplier * caster.GetAttribute(Attribute.Strength));
                    damageDealt -= Armor;
                    break;
            }
            if (damageDealt <= 0)
                damageDealt = 10;
            damageDealt *= (isCrit ? 2 : 1);
            damageDealt = Health.Value < damageDealt ? Health.Value : damageDealt;
            Health.Value -= damageDealt;

            if (damageDealt > 0)
                Server.OutgoingMessageHandler.SendDamageInfo(caster, this, skillSchool, damageDealt, isCrit);

            if (Health.Value <= 0)
            {
                Server.OutgoingMessageHandler.SendDeathMessage(this);
            }

            return damageDealt;
        }

        public virtual int TakeDamage(int value)
        {
            int damageDealt = value;
            Health.Value -= Health.Value < damageDealt ? Health.Value : damageDealt;

            if (Health.Value <= 0)
            {
                Server.OutgoingMessageHandler.SendDeathMessage(this);
            }
            return damageDealt;
        }

        /// <summary>
        /// Adds buff to the buff list. 
        /// If the buff is already contained by the buff list, it refreshes its expiration time. 
        /// Else it calls the buffs OnApply() method and adds it to the buff list.
        /// </summary>
        /// <param name="buff">The buff that needs to be added.</param>
        public virtual void AddBuff(Buff buff)
        {
            buff.SetExpirationTick(EntityTick);
            if (!Buffs.Any(x => x.BuffId == buff.BuffId))
            {
                //Add buff and apply its effects
                buff.OnApply(this);
                Buffs.Add(buff);
                //Buffs.Last()
            }
            else
            {
                //Refresh buff expiration
                for (int i = 0; i < Buffs.Count; i++)
                {
                    if (Buffs[i].BuffId == buff.BuffId)
                    {
                        Buffs[i] = buff;

                        buff.RefreshExpirationTick(this, Buffs[i].ExpirationTick);
                    }
                }
            }

            //Calculate resources from the new attribute values    
            CalculateResources();
        }

        public virtual void SetMana(int mana = 1000)
        {
            AddMana(mana);
        }

        public virtual void AddHealth(int health)
        {
            Health.Value += health;
            if (Health.MaxValue < Health.Value) Health.SetFull();
        }

        public virtual void AddMana(int mana)
        {
            Mana.Value += mana;
            if (Mana.MaxValue < Mana.Value) Mana.SetFull();
        }

        /// <summary>
        /// When a skill is casted it calls this method, for using the resources of the Entity. 
        /// </summary>
        /// <param name="costType">The type of the resource, it can be Health, or Mana.</param>
        /// <param name="amount">The amount of the resource that is needed.</param>
        /// <returns>Returns true if the required resource's value was more then the required amount, else returns false</returns>
        public virtual bool UseResource(SkillCostType costType, int amount)
        {
            switch (costType)
            {
                case SkillCostType.Mana:
                    if (Mana.Value >= amount)
                    {
                        AddMana(-amount);
                    }
                    else return false;
                    break;
                case SkillCostType.Health:
                    if (Health.Value > amount) TakeDamage(amount);
                    else return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// Calculating resources. 
        /// Max Health/Mana, ManaRegen, HealthRegen are calculated here.
        /// </summary>
        public virtual void CalculateResources()
        {
            Health.MaxValue = (ushort)(500 + GetAttribute(Attribute.Endurance) * 20);
            Mana.MaxValue = (ushort)(500 + GetAttribute(Attribute.Knowledge) * 35);

            //if (Health.MaxValue < Health.Value) Health.SetFull();
            //if (Mana.MaxValue < Mana.Value) Mana.SetFull();

            Health.RegenValue = (ushort)(Health.MaxValue / 100);
            Mana.RegenValue = (ushort)(Mana.MaxValue / 100);
        }

        /// <summary>
        /// Sets the position of the Entity while checking for the GridCell containment.
        /// </summary>
        /// <param name="position"></param>
        public virtual void SetPosition(Vector3 position)
        {
            Position = position;

            if (GridCell != null && !GridCell.Area.Contains((int)Position.X, (int)Position.Z))
            {
                Server.Data.ReAdd(this);
            }
        }

        public virtual NetConnection GetConnection()
        {
            return null;
        }

        public override string ToString()
        {
            return string.Format($"{Id} {Name}");
        }
    }
}
