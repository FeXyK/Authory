namespace AuthoryServer.Entities
{
    public class Skill_Buff : AbstractSkill
    {
        public Buff Buff { get; set; }

        public override void OnCasted()
        {
            State = SkillState.OnHit;
        }

        public override void OnApply()
        {
            Target.AddBuff(Buff);
        }

        public override AbstractSkill Copy()
        {
            Skill_Buff skill = new Skill_Buff();
            skill.Buff = this.Buff;
            CopyInto(skill);

            return skill;
        }
    }
}