namespace AuthoryServer.Entities
{
    public class Skill_Blink : AbstractSkill
    {
        public Vector3 TargetPosition { get; set; }

        public override AbstractSkill Copy()
        {
            Skill_Blink skill = new Skill_Blink();

            CopyInto(skill);
            return skill;

        }

        public override AbstractSkill Create(Entity caster, Vector3 targetPosition, AuthoryServer server)
        {
            base.Create(caster, targetPosition, server);
            TargetPosition = targetPosition;

            return this;
        }

        public override void OnApply()
        {
            float Distance = Vector3.Distance(Caster.Position, TargetPosition);
            if (Distance > MaxTargetRange)
            {
                Vector3 direction = (TargetPosition - Caster.Position);
                direction = direction.Normalize() * MaxTargetRange;
                TargetPosition = Caster.Position + direction;

            }
            Caster.SetPosition(TargetPosition);
            Server.OutgoingMessageHandler.SendTeleport(Caster);
        }

        public override void OnCasted()
        {
            OnApply();
            State = SkillState.OnCooldown;
        }

        public override void SendServerInfo()
        {
        }
    }
}
