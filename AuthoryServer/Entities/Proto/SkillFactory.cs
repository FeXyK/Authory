using System.Collections.Generic;
using AuthoryServer.Entities.Enums;

namespace AuthoryServer.Entities
{
    /// <summary>
    /// Provides a factory for creating skills. 
    /// </summary>
    public class SkillFactory
    {
        private Dictionary<SkillID, AbstractSkill> _newSkillDictionary;

        private static SkillFactory _instance;
        public static SkillFactory Instance => _instance ??= new SkillFactory();

        /// <summary>
        /// If required, server can provide lag compensation, which will lower the cooldowns of every skill by default by its given value. Value is in seconds.
        /// </summary>
        private const float LAG_COMPENSATION = 0.3f;

        /// <summary>
        /// Returns a Copy of the requested skill by SkillID.
        /// </summary>
        /// <param name="skillId">Requested SkillID</param>
        /// <returns>Null if no matching element in the dictionary, else returns the element's copy.</returns>
        public AbstractSkill GetSkill(SkillID skillId)
        {
            return _newSkillDictionary.ContainsKey(skillId) ? _newSkillDictionary[skillId].Copy() : null;
        }

        public SkillFactory()
        {
            _newSkillDictionary = new Dictionary<SkillID, AbstractSkill>();
            _loadNewSkillValues();
        }

        /// <summary>
        /// Loading SkillFactory by skills.
        /// </summary>
        private void _loadNewSkillValues()
        {
            //Cleave
            _newSkillDictionary.Add(SkillID.Cleave, new Skill_Cleave()
            {
                SkillId = SkillID.Cleave,
                CostType = SkillCostType.Mana,
                CostValue = 100,
                CastDuration = 0,
                State = SkillState.OnCasting,
                School = SkillSchool.Physical,
                Cooldown = 1f - LAG_COMPENSATION,
            });

            //Fireball
            _newSkillDictionary.Add(SkillID.Fireball, new Skill_Fireball()
            {
                DamageMultiplier = 5f,
                SkillId = SkillID.Fireball,
                CostType = SkillCostType.Mana,
                CostValue = 100,
                CastDuration = 0.5f,
                State = SkillState.OnCasting,
                School = SkillSchool.Fire,
                Speed = 20.0f,
                Debuff = new Buff().Init(BuffList.Ignite, 10f, 0, StatusEffect.None, StatusEffect.Health, -3),
                Cooldown = 2f - LAG_COMPENSATION,
            });

            //Explosion
            _newSkillDictionary.Add(SkillID.Explosion, new Skill_Explosion()
            {
                SkillId = SkillID.Explosion,
                CostType = SkillCostType.Mana,
                CostValue = 100,
                CastDuration = 1f,
                State = SkillState.OnCasting,
                School = SkillSchool.Fire,
                SqrRange = 900f,
                Cooldown = 2f - LAG_COMPENSATION,
            });

            //MeleeAutoAttack
            _newSkillDictionary.Add(SkillID.MeleeAutoAttack, new Skill_MeleeAutoAttack()
            {
                SkillId = SkillID.MeleeAutoAttack,
                CostType = SkillCostType.Mana,
                CostValue = 10,
                DamageMultiplier = 2f,
                State = SkillState.OnCasting,
                School = SkillSchool.Physical,
                Cooldown = 0.5f - LAG_COMPENSATION,
                MaxTargetRange = 5f
            });

            //Blink
            _newSkillDictionary.Add(SkillID.Blink, new Skill_Blink()
            {
                SkillId = SkillID.Blink,
                CostType = SkillCostType.Mana,
                CostValue = 100,
                CastDuration = 0f,
                State = SkillState.OnCasted,
                School = SkillSchool.Arcane,
                Cooldown = 3f - LAG_COMPENSATION,
                MaxTargetRange = 100f,
            });


            //ChainLightning
            _newSkillDictionary.Add(SkillID.ChainLightning, new Skill_ChainLightning()
            {
                SkillId = SkillID.ChainLightning,
                CostType = SkillCostType.Mana,
                CostValue = 100,
                CastDuration = 0.5f,
                State = SkillState.OnCasting,
                School = SkillSchool.Lightning,
                Depth = 10,
                SqrBounceDistance = 1200f,
                DamageMultiplier = 5f,
                Debuff = new Buff().Init(BuffList.Root, 3f, 0, StatusEffect.Root),
                Cooldown = 2f - LAG_COMPENSATION,
            });

            //Rage
            _newSkillDictionary.Add(SkillID.Rage, new Skill_Rage()
            {
                SkillId = SkillID.Rage,
                CostType = SkillCostType.Health,
                CostValue = 100,
                CastDuration = 0,
                State = SkillState.OnCasted,
                School = SkillSchool.Dark,
                Buff = new Buff().Init(BuffList.Rage, 10f, 200, StatusEffect.Rage),
                Cooldown = 60 - LAG_COMPENSATION,
            });

            //Blink
            //_newSkillDictionary.Add(SkillList.Blink, new Skill(SkillList.Blink, SkillType.Instant, SkillCastType.Instant, SkillCostType.Mana, SkillSchool.Lightning, BuffEffect.Teleport, 0, 100, 40, 0, 0, 0));
            //_newSkillDictionary[SkillList.Blink].NeedTarget = false;


            //BUFFS

            //Swiftness
            _newSkillDictionary.Add(SkillID.Swiftness, new Skill_Buff()
            {
                SkillId = SkillID.Swiftness,
                CostType = SkillCostType.Mana,
                CostValue = 100,
                CastDuration = 1f,
                State = SkillState.OnCasting,
                School = SkillSchool.Arcane,
                Buff = new Buff().Init(BuffList.Swiftness, 60f, 20, StatusEffect.MoveSpeed),
                Cooldown = 1 - LAG_COMPENSATION,
            });

            //HealthOverflow
            _newSkillDictionary.Add(SkillID.HealthOverflow, new Skill_Buff()
            {
                SkillId = SkillID.HealthOverflow,
                CostType = SkillCostType.Mana,
                CostValue = 100,
                CastDuration = 1f,
                State = SkillState.OnCasting,
                School = SkillSchool.Arcane,
                Buff = new Buff().Init(BuffList.HealthOverflow, 60f, 20, StatusEffect.IncreaseEndurance),
                Cooldown = 60 - LAG_COMPENSATION,
            });

            //HealthOverload
            _newSkillDictionary.Add(SkillID.HealthOverload, new Skill_Buff()
            {
                SkillId = SkillID.HealthOverload,
                CostType = SkillCostType.Mana,
                CostValue = 100,
                CastDuration = 1f,
                State = SkillState.OnCasting,
                School = SkillSchool.Arcane,
                Buff = new Buff().Init(BuffList.HealthOverload, 8f, 0, StatusEffect.None, StatusEffect.Health, 10),
                Cooldown = 30 - LAG_COMPENSATION,
            });


            //ManaOverflow
            _newSkillDictionary.Add(SkillID.ManaOverflow, new Skill_Buff()
            {
                SkillId = SkillID.ManaOverflow,
                CostType = SkillCostType.Mana,
                CostValue = 100,
                CastDuration = 1f,
                State = SkillState.OnCasting,
                School = SkillSchool.Arcane,
                Buff = new Buff().Init(BuffList.ManaOverflow, 60f, 20, StatusEffect.IncreaseKnowledge),
                Cooldown = 60 - LAG_COMPENSATION,
            });

            //ManaOverload
            _newSkillDictionary.Add(SkillID.ManaOverload, new Skill_Buff()
            {
                SkillId = SkillID.ManaOverload,
                CostType = SkillCostType.Mana,
                CostValue = 100,
                CastDuration = 1f,
                State = SkillState.OnCasting,
                School = SkillSchool.Arcane,
                Buff = new Buff().Init(BuffList.ManaOverload, 8f, 0, StatusEffect.None, StatusEffect.Mana, 10),
                Cooldown = 30 - LAG_COMPENSATION,
            });
        }
    }
}
