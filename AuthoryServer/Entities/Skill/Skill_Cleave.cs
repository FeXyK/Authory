namespace AuthoryServer.Entities
{
    public class Skill_Cleave : AbstractSkill
    {
        public float Range { get; set; }

        public override void OnApply()
        {
            foreach (var mob in Caster.GridCell.MobEntities.Values)
            {
                if (Vector3.SqrDistance(Caster.Position, mob.Position) < Range)
                {
                    mob.TakeDamage(School, Caster.GetAttribute(Attribute.Strength), Caster);
                }
            }
            foreach (var player in Caster.GridCell.PlayersById.Values)
            {
                if (Vector3.SqrDistance(Caster.Position, player.Position) < Range)
                {
                    player.TakeDamage(School, Caster.GetAttribute(Attribute.Strength), Caster);
                }
            }
        }

        public override AbstractSkill Copy()
        {
            Skill_Cleave skill = new Skill_Cleave();
            CopyInto(skill);

            return skill;
        }

        public override void OnCasted()
        {
            State = SkillState.OnHit;
        }
    }
}