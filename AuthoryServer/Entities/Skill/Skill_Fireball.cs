namespace AuthoryServer.Entities
{
    public class Skill_Fireball : AbstractSkill
    {
        public float Speed { get; set; }
        public Vector3 Position { get; set; }
        public Buff Debuff { get; set; }

        public override AbstractSkill Create(Entity caster, Entity target, AuthoryServer server)
        {
            base.Create(caster, target, server);
            Position = Caster.Position;

            return this;
        }

        public override void OnApply()
        {
            Target.TakeDamage(School, DamageMultiplier, Caster);
            Target.AddBuff(Debuff);
        }

        public override void OnCasted()
        {
            State = SkillState.OnMoving;
        }

        public override void OnMoving()
        {
            MoveTowards();

            if (Vector3.SqrDistance(Position, Target.Position) < 1f)
            {
                State = SkillState.OnHit;
            }
        }

        public Vector3 MoveTowards()
        {
            Vector3 unitVector = (Target.Position - Position).Normalize();
            unitVector /= TickRate;
            unitVector *= Speed;
            //Console.WriteLine(unitVector);
            Position += unitVector;

            return unitVector;
        }

        public override AbstractSkill Copy()
        {
            Skill_Fireball skill = new Skill_Fireball();

            skill.Speed = this.Speed;
            skill.DamageMultiplier = this.DamageMultiplier;
            skill.Debuff = this.Debuff;
            CopyInto(skill);

            return skill;
        }
    }
}
