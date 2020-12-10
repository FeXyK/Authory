using AuthoryServer.Entities;
using AuthoryServer.Entities.Enums;

public struct Buff
{
    public BuffList BuffId { get; private set; }

    public long ExpirationTick { get; private set; }
    public int OnApplyStatusEffectValue { get; private set; }
    public StatusEffect OnApplyStatusEffect { get; private set; }

    public int OnTickStatusEffectValue { get; private set; }
    public StatusEffect OnTickStatusEffect { get; private set; }


    float duration;


    public Buff Init(BuffList id, float duration, int value, StatusEffect onApplyStateEffect, StatusEffect onTickStatEffect = StatusEffect.None, int onTickStatEffectValue = 0)
    {
        this.BuffId = id;
        this.duration = duration;
        this.OnApplyStatusEffectValue = value;
        this.OnApplyStatusEffect = onApplyStateEffect;

        this.OnTickStatusEffect = onTickStatEffect;
        this.OnTickStatusEffectValue = onTickStatEffectValue;

        return this;
    }

    public void OnApply(Entity entity)
    {
        switch (OnApplyStatusEffect)
        {
            case StatusEffect.MoveSpeed:
                entity.SetMovementSpeed(entity.MovementSpeed + OnApplyStatusEffectValue);
                break;
            case StatusEffect.Stun:
                entity.SetMovementSpeed(0);
                break;
            case StatusEffect.Root:
                entity.SetMovementSpeed(0);
                break;
            case StatusEffect.Snare:
                entity.SetMovementSpeed(0);
                break;
            case StatusEffect.Knokback:
                break;
            case StatusEffect.Cleanse:
                break;
            case StatusEffect.Interrupt:
                break;
            case StatusEffect.IncreaseEndurance:
                entity.GetAttribute(Attribute.Endurance) += OnApplyStatusEffectValue;
                break;
            case StatusEffect.IncreaseKnowledge:
                entity.GetAttribute(Attribute.Knowledge) += OnApplyStatusEffectValue;
                break;
            case StatusEffect.Rage:
                entity.GetAttribute(Attribute.Endurance) += OnApplyStatusEffectValue;
                entity.GetAttribute(Attribute.Strength) += OnApplyStatusEffectValue;
                entity.GetAttribute(Attribute.Agility) += OnApplyStatusEffectValue;
                entity.GetAttribute(Attribute.Intelligence) += OnApplyStatusEffectValue;
                entity.GetAttribute(Attribute.Knowledge) += OnApplyStatusEffectValue;
                entity.GetAttribute(Attribute.Luck) += OnApplyStatusEffectValue;
                //entity.SetMovementSpeed(30);
                //entity.AttackRange = 15 * 15;
                break;
        }
        entity.Server.OutgoingMessageHandler.SendBuffApply(entity, this);
    }

    public void RefreshExpirationTick(Entity entity, long expirationTick)
    {
        this.ExpirationTick = expirationTick;
        entity.Server.OutgoingMessageHandler.SendBuffRefresh(entity, this);
    }

    public void OnEnd(Entity entity)
    {
        switch (OnApplyStatusEffect)
        {
            case StatusEffect.MoveSpeed:
                entity.SetMovementSpeed(entity.MovementSpeed - OnApplyStatusEffectValue);
                break;
            case StatusEffect.Stun:
                entity.SetMovementSpeed(NPCFactory.Instance.GetNpcValuesOf(ModelType.MeleeNPC).MovementSpeed);
                break;
            case StatusEffect.Root:
                entity.SetMovementSpeed(NPCFactory.Instance.GetNpcValuesOf(ModelType.MeleeNPC).MovementSpeed);
                break;
            case StatusEffect.Snare:
                entity.SetMovementSpeed(NPCFactory.Instance.GetNpcValuesOf(ModelType.MeleeNPC).MovementSpeed);
                break;
            case StatusEffect.Knokback:
                break;
            case StatusEffect.Cleanse:
                break;
            case StatusEffect.Interrupt:
                break;
            case StatusEffect.IncreaseEndurance:
                entity.GetAttribute(Attribute.Endurance) -= OnApplyStatusEffectValue;
                break;
            case StatusEffect.IncreaseKnowledge:
                entity.GetAttribute(Attribute.Knowledge) -= OnApplyStatusEffectValue;
                break;
            case StatusEffect.Rage:
                entity.GetAttribute(Attribute.Endurance) -= OnApplyStatusEffectValue;
                entity.GetAttribute(Attribute.Strength) -= OnApplyStatusEffectValue;
                entity.GetAttribute(Attribute.Agility) -= OnApplyStatusEffectValue;
                entity.GetAttribute(Attribute.Intelligence) -= OnApplyStatusEffectValue;
                entity.GetAttribute(Attribute.Knowledge) -= OnApplyStatusEffectValue;
                entity.GetAttribute(Attribute.Luck) -= OnApplyStatusEffectValue;
                //entity.SetMovementSpeed(ProtoNpc.Instance.GetNpcValuesOf(entity.ModelType).MovementSpeed);
                //entity.AttackRange = ProtoNpc.Instance.GetNpcValuesOf(entity.ModelType).AttackRange;
                break;
        }
        entity.Server.OutgoingMessageHandler.SendBuffRemove(entity, this);
    }

    public void OnTick(Entity entity)
    {
        switch (OnTickStatusEffect)
        {
            case StatusEffect.Health:
                if (OnTickStatusEffectValue > 0)
                    entity.AddHealth(OnTickStatusEffectValue);
                else
                    entity.TakeDamage(-OnTickStatusEffectValue);
                break;
            case StatusEffect.Mana:
                entity.AddMana(OnTickStatusEffectValue);
                break;
        }
    }

    public void SetExpirationTick(long entityTick)
    {
        ExpirationTick = (long)(entityTick + duration);
    }

    public void SetDurationBasedByTickRate(float tickRate)
    {
        duration *= tickRate;
    }

    public override string ToString()
    {
        return string.Format($"{BuffId.ToString()}: Exp tick: {ExpirationTick} Attr: {OnApplyStatusEffect.ToString()} Value: {OnApplyStatusEffectValue}| {OnTickStatusEffect.ToString()}:{ OnTickStatusEffectValue}");
    }
}
