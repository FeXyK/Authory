namespace AuthoryServer.Entities
{
    internal class Skill_Rage : AbstractSkill
    {
        public Buff Buff { get; set; }

        public override void OnApply()
        {
            Caster.AddBuff(Buff);
        }

        public override AbstractSkill Copy()
        {
            Skill_Rage skill = new Skill_Rage();
            CopyInto(skill);

            skill.Buff = this.Buff;

            return skill;
        }

        public override void OnCasted()
        {
        }
    }
}