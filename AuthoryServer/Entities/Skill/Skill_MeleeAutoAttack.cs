namespace AuthoryServer.Entities
{
    public class Skill_MeleeAutoAttack : AbstractSkill
    {
        public override void OnApply()
        {
            Target.TakeDamage(School, DamageMultiplier, Caster);
        }

        public override AbstractSkill Copy()
        {
            Skill_MeleeAutoAttack skill = new Skill_MeleeAutoAttack();
            CopyInto(skill);
            return skill;
        }


        public override void OnCasted() { }
    }
}