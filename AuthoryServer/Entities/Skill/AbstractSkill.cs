namespace AuthoryServer.Entities
{
    public abstract class AbstractSkill
    {
        public AuthoryServer Server { get; set; }

        public Entity Caster { get; set; }
        public Entity Target { get; set; }

        public float MaxTargetRange { get; set; } = 40f;
        public float DamageMultiplier { get; set; } = 1f;

        public SkillID SkillId { get; set; }
        public SkillSchool School { get; set; }

        public SkillState State { get; set; }

        public SkillCostType CostType { get; set; }
        public int CostValue { get; set; }

        //In seconds
        public float CastDuration { get; set; }
        //In seconds
        public float Cooldown { get; set; }

        protected float TickRate;

        public abstract AbstractSkill Copy();

        public virtual AbstractSkill Create(Entity caster, Entity target, AuthoryServer server)
        {
            Caster = caster;
            Target = target;
            Server = server;

            TickRate = (caster.EntityType == Enums.EntityType.Player ? server.PlayerTickRate : server.MobTickRate);
            CastDuration *= TickRate;
            Cooldown *= TickRate;

            return this;
        }

        public virtual AbstractSkill Create(Entity caster, Vector3 targetPosition, AuthoryServer server)
        {
            Caster = caster;
            Server = server;
            TickRate = (caster.EntityType == Enums.EntityType.Player ? server.PlayerTickRate : server.MobTickRate);

            CastDuration *= TickRate;
            Cooldown *= TickRate;

            return this;
        }
        public void OnTick()
        {
            switch (State)
            {
                case SkillState.OnCasting:
                    OnCasting();
                    break;
                case SkillState.OnCasted:
                    Caster.IsSkillCasting = false;
                    if (Caster.UseResource(CostType, CostValue))
                    {
                        State = SkillState.OnHit;
                        OnCasted();
                        //Send skill info for clients to render the skill.
                        SendServerInfo();
                    }
                    else
                    {
                        //Send skill interrupt for caster to reset cooldown.
                        Server.OutgoingMessageHandler.SendSkillInterrupt(Caster, SkillId);
                        State = SkillState.Interrupted;
                    }
                    Cooldown--;
                    break;
                case SkillState.OnMoving:
                    Cooldown--;
                    OnMoving();
                    break;
                case SkillState.OnHit:
                    Cooldown--;
                    OnHit();
                    break;
                case SkillState.OnCooldown:
                    Cooldown--;
                    if (Cooldown < 1)
                    {
                        State = SkillState.Handled;
                    }
                    break;
                case SkillState.Interrupted:
                    OnInterrupt();
                    break;
                case SkillState.Handled:
                    OnHandled();
                    break;
            }
        }

        public virtual void SendServerInfo()
        {
            Server.OutgoingMessageHandler.SendSkillOnCasted(this);
        }

        public abstract void OnApply();
        public virtual void OnCasting()
        {
            if (Caster.IsSkillCasting)
            {
                CastDuration--;
                if (CastDuration < 1)
                {
                    State = SkillState.OnCasted;
                }
            }
            else
            {
                State = SkillState.Interrupted;
            }
        }

        public abstract void OnCasted();

        public virtual void OnMoving() { }

        public virtual void OnHit()
        {
            State = SkillState.OnCooldown;
            OnApply();
        }

        public virtual void OnInterrupt() { }
        public virtual void OnHandled() { }


        public void CopyInto(AbstractSkill skill)
        {
            skill.SkillId = this.SkillId;
            skill.State = this.State;

            skill.School = this.School;

            skill.CostType = this.CostType;
            skill.CostValue = this.CostValue;

            skill.CastDuration = this.CastDuration;

            skill.DamageMultiplier = this.DamageMultiplier;
            skill.Cooldown = this.Cooldown;
            skill.MaxTargetRange = this.MaxTargetRange;
        }

        public override string ToString()
        {
            return string.Format($"{SkillId.ToString()}: CD: {Cooldown} s");
        }
    }
}