namespace AuthoryServer.Entities
{
    public class Skill_Explosion : AbstractSkill
    {
        public float SqrRange { get; set; }

        public override void OnApply()
        {
            foreach (var grid in Target.GridCell.Neighbours)
                foreach (var mob in grid.MobEntities.Values)
                {
                    if (Vector3.SqrDistance(Target.Position, mob.Position) < SqrRange)
                    {
                        mob.TakeDamage(School, Caster.GetAttribute(Attribute.Intelligence), Caster);
                    }
                }
        }

        public override AbstractSkill Copy()
        {
            Skill_Explosion skill = new Skill_Explosion();
            CopyInto(skill);
            skill.SqrRange = this.SqrRange;

            return skill;
        }

        public override void OnCasted()
        {
        }
    }
}