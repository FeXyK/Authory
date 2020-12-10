namespace AuthoryServer.Entities
{
    public class Skill_ChainLightning : AbstractSkill
    {
        public Entity previousTarget { get; set; }

        public int Depth { get; set; }
        public float SqrBounceDistance { get; set; }

        public Buff Debuff { get; set; }

        public override void OnApply()
        {
            Target.TakeDamage(School, DamageMultiplier, Caster);
            Target.AddBuff(Debuff);
            Depth--;

            if (Depth > 0)
            {
                foreach (var grid in Target.GridCell.Neighbours)
                    foreach (var mob in grid.MobEntities.Values)
                    {
                        if (Target != mob && mob.Health.Value > 0 && Vector3.SqrDistance(mob.Position, Target.Position) < SqrBounceDistance)
                        {
                            previousTarget = Target;
                            Target = mob;
                            State = SkillState.OnCasted;
                            break;
                        }
                    }
            }
        }

        public override void SendServerInfo()
        {
            Server.OutgoingMessageHandler.SendSkillOnCastedAlternativePosition(this, previousTarget);
        }

        public override AbstractSkill Copy()
        {
            Skill_ChainLightning skill = new Skill_ChainLightning();
            CopyInto(skill);
            skill.Depth = this.Depth;
            skill.SqrBounceDistance = this.SqrBounceDistance;

            skill.Debuff = this.Debuff;

            return skill;
        }

        public override void OnCasted()
        {
            State = SkillState.OnHit;
        }
    }
}