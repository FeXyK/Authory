using Assets.Authory.Scripts.Enum;
using Lidgren.Network;
using Lidgren.Network.Shared;
using UnityEngine;

/// <summary>
/// Handles the incoming messages.
/// </summary>
public class AuthoryHandler
{
    public AuthoryData Data { get; private set; }

    private UIController uiController;

    private float MAX_VALUE_OF_POSITION = 2000;
    private float SMALLEST_FLOAT_STEP;

    public AuthoryHandler(AuthoryData data)
    {
        Data = data;
        uiController = GameObject.FindObjectOfType<UIController>();
        SMALLEST_FLOAT_STEP = (ushort.MaxValue / MAX_VALUE_OF_POSITION);
    }

    public void GridFullEntityUpdate(NetIncomingMessage msgIn)
    {
        ushort count = msgIn.ReadUInt16();
        for (int i = 0; i < count; i++)
        {
            FullEntityUpdate(msgIn);
        }
    }

    public void FullEntityUpdate(NetIncomingMessage msgIn)
    {

        ushort id = msgIn.ReadUInt16();
        string name = msgIn.ReadString();
        byte level = msgIn.ReadByte();
        ushort maxHealth = msgIn.ReadUInt16();
        ushort health = msgIn.ReadUInt16();

        float movementSpeed = QuantateUshortToFloat(msgIn.ReadUInt16());

        float x = QuantateUshortToFloat(msgIn.ReadUInt16());
        float z = QuantateUshortToFloat(msgIn.ReadUInt16());

        float endX = QuantateUshortToFloat(msgIn.ReadUInt16());
        float endZ = QuantateUshortToFloat(msgIn.ReadUInt16());

        ModelType modelType = (ModelType)msgIn.ReadByte();

        Entity entity = Data.GetEntity(id);

        if (entity == null)
        {
            entity = GetEntityObjectByModelType(modelType);
            entity.SetInfo(name, id);
            Debug.Log("Full Entity Update Spawn");
            Data.AddEntity(entity);
        }

        entity.SetInfo(name, id);
        entity.SetMaxHealth(maxHealth);
        entity.SetHealth(health);
        entity.SetPath(new Vector3(x, 0, z), new Vector3(endX, 0, endZ), movementSpeed);
        entity.Teleport(x, z);
        entity.Level = level;
    }

    public void Disconnect(NetIncomingMessage msgIn)
    {
        ushort id = msgIn.ReadUInt16();
        Entity entity = Data.GetEntity(id);
        if (entity != null)
        {
            Data.RemoveEntity(entity);
            GameObject.Destroy(entity.gameObject);
        }
    }

    public void Movement(NetIncomingMessage msgIn)
    {
        Entity entity;
        ushort count = msgIn.ReadUInt16();
        ushort id;
        for (int i = 0; i < count; i++)
        {
            id = msgIn.ReadUInt16();
            entity = Data.GetEntity(id);

            float x = QuantateUshortToFloat(msgIn.ReadUInt16());
            float z = QuantateUshortToFloat(msgIn.ReadUInt16());
            if (entity != null && entity.Id != Data.Player.Id)
            {
                entity.SetEndPosition(x, z);
            }
        }
    }

    public void ChannelInfo(NetIncomingMessage msgIn)
    {
        int count = msgIn.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            Channel channel = new Channel()
            {
                Name = msgIn.ReadString(),
                Index = msgIn.ReadInt32(),
                IP = msgIn.ReadString(),
                Port = msgIn.ReadInt32()
            };

            uiController.GetChannelSelector().AddChannel(channel);
        }
    }

    public void MobPathTo(NetIncomingMessage msgIn)
    {
        ushort id = msgIn.ReadUInt16();
        Entity entity = Data.GetEntity(id);

        float fromX = QuantateUshortToFloat(msgIn.ReadUInt16());
        float fromZ = QuantateUshortToFloat(msgIn.ReadUInt16());

        float endX = QuantateUshortToFloat(msgIn.ReadUInt16());
        float endZ = QuantateUshortToFloat(msgIn.ReadUInt16());

        float moveSpeed = QuantateUshortToFloat(msgIn.ReadUInt16());

        if (entity != null)
        {
            Vector3 startPos = new Vector3(fromX, 0, fromZ);
            Vector3 endPos = new Vector3(endX, 0, endZ);

            entity.SetPath(startPos, endPos, moveSpeed);
        }
    }

    public void SkillInterrupted(NetIncomingMessage msgIn)
    {
        int skillID = msgIn.ReadByte();

        uiController.SkillBarController.ResetSkillCooldown(skillID);
    }

    public void GridFullResourceEntityUpdate(NetIncomingMessage msgIn)
    {
        int count = msgIn.ReadUInt16();

        for (int i = 0; i < count; i++)
        {
            ushort id = msgIn.ReadUInt16();
            string name = msgIn.ReadString();

            ModelType modelType = (ModelType)msgIn.ReadByte();

            float x = QuantateUshortToFloat(msgIn.ReadUInt16());
            float z = QuantateUshortToFloat(msgIn.ReadUInt16());

            Entity entity = Data.GetEntity(id);
            if (entity == null)
            {
                entity = GetEntityObjectByModelType(modelType);
                entity.SetInfo(name, id);
                entity.transform.position = new Vector3(x, 0, z);
                Data.AddEntity(entity);
            }
        }
    }

    public void MobRespawn(NetIncomingMessage msgIn)
    {
        ushort id = msgIn.ReadUInt16();
        byte level = msgIn.ReadByte();
        ushort maxHealth = msgIn.ReadUInt16();

        float x = QuantateUshortToFloat(msgIn.ReadUInt16());
        float z = QuantateUshortToFloat(msgIn.ReadUInt16());

        Entity entity = Data.GetEntity(id);
        if (entity != null)
        {
            entity.Level = level;
            entity.SetMaxHealth(maxHealth);
            entity.SetHealth(maxHealth);

            entity.Teleport(x, z);
            entity.Buffs.Clear();
            entity.Respawn();
        }
    }

    public void MovementSpeedChange(NetIncomingMessage msgIn)
    {
        Entity entity = Data.GetEntity(msgIn.ReadUInt16());
        if (entity != null)
        {
            entity.SetMovementSpeed(msgIn.ReadFloat());
        }
    }

    public void PlayerUpdate(NetIncomingMessage msgIn)
    {
        Entity entity = Data.GetEntity(msgIn.ReadUInt16());

        if (entity != null)
        {
            entity.SetHealth(msgIn.ReadUInt16());
            entity.SetMana(msgIn.ReadUInt16());
        }
    }

    public void MobUpdate(NetIncomingMessage msgIn)
    {
        Entity entity = Data.GetEntity(msgIn.ReadUInt16());

        entity?.SetHealth(msgIn.ReadUInt16());
    }

    public void Skill(NetIncomingMessage msgIn)
    {
        {
            byte skillId = msgIn.ReadByte();
            ushort casterId = msgIn.ReadUInt16();
            ushort targetId = msgIn.ReadUInt16();
            Entity caster = Data.GetEntity(casterId);
            Entity target = Data.GetEntity(targetId);

            if (target == null || caster == null) return;
            if (skillId == 0)
            {
                if (caster != null)
                    caster.PlayHitVFX();
            }
            else
            {
                GameObject skillObject = GameObject.Instantiate(SkillCollection.Instance.SkillObjects[skillId]);
                skillObject.transform.position = target.transform.position;

                if (caster != null && target != null)
                {
                    Skill_Fireball moveTowards = skillObject.GetComponent<Skill_Fireball>();
                    if (moveTowards != null)
                    {
                        moveTowards.transform.position = caster.transform.position;
                        moveTowards.Caster = caster;
                        moveTowards.Target = target;
                    }
                }
            }
        }
    }

    public void SkillAlternativePosition(NetIncomingMessage msgIn)
    {
        ushort skillId = msgIn.ReadByte();
        ushort casterId = msgIn.ReadUInt16();
        ushort targetId = msgIn.ReadUInt16();
        ushort alternativeTargetId = msgIn.ReadUInt16();

        Entity caster = Data.GetEntity(casterId);
        Entity target = Data.GetEntity(targetId);
        Entity alternativeEntity = Data.GetEntity(alternativeTargetId);

        if (caster != null && target != null && alternativeEntity != null)
        {
            GameObject skillObject = GameObject.Instantiate(SkillCollection.Instance.SkillObjects[skillId]);
            Skill_Lightning lightning = skillObject.GetComponent<Skill_Lightning>();
            if (lightning != null)
            {
                if (caster != null && target != null)
                {
                    lightning.Target = target;

                    lightning.Caster = caster;
                    lightning.AlternativeEntity = alternativeEntity;
                }
            }
        }
    }

    public void Death(NetIncomingMessage msgIn)
    {
        Entity entity = Data.GetEntity(msgIn.ReadUInt16());
        if (entity != null)
        {
            entity.SetHealth(0);
        }
    }

    public void BuffApply(NetIncomingMessage msgIn)
    {
        ushort entityId = msgIn.ReadUInt16();

        byte buffId = msgIn.ReadByte();
        long duration = msgIn.ReadInt64();

        Entity entity = Data.GetEntity(entityId);

        if (entity != null)
        {
            Buff newBuff = GameObject.Instantiate(SkillCollection.Instance.BuffObjects[buffId]);
            newBuff.BuffLifetime = duration * (entity.IsPlayer ? 0.05f : 0.05f);

            entity.AddBuff(newBuff);

            if (uiController.SelectedTargetInfoController.CurrentTarget == entity)
            {
                uiController.SelectedTargetInfoController.RefreshBuffController();
            }
        }
    }

    public void BuffRefresh(NetIncomingMessage msgIn)
    {
        ushort entityId = msgIn.ReadUInt16();

        byte buffId = msgIn.ReadByte();
        long duration = msgIn.ReadInt64();

        Entity entity = Data.GetEntity(entityId);

        if (entity != null)
        {
            Buff newBuff = GameObject.Instantiate(SkillCollection.Instance.BuffObjects[buffId]);
            newBuff.BuffLifetime = duration * (entity.IsPlayer ? 0.05f : 0.05f);

            entity.AddBuff(newBuff);

            if (uiController.SelectedTargetInfoController.CurrentTarget == entity)
            {
                uiController.SelectedTargetInfoController.RefreshBuffController();
            }
        }
    }

    public void BuffRemove(NetIncomingMessage msgIn)
    {
        ushort entityId = msgIn.ReadUInt16();
        byte buffId = msgIn.ReadByte();

        Entity entity = Data.GetEntity(entityId);
        if (entity == null) return;
        entity.Buffs.RemoveAll(x => x.BuffId == buffId);
    }

    public void AttributeUpdate(NetIncomingMessage msgIn)
    {

        Entity entity = Data.GetEntity(msgIn.ReadUInt16());
        if (entity != null)
        {
            entity.SetMaxHealth(msgIn.ReadUInt16());
            entity.SetHealth(msgIn.ReadUInt16());
            entity.SetMaxMana(msgIn.ReadUInt16());
            entity.SetMana(msgIn.ReadUInt16());

            entity.Attributes.Endurance = msgIn.ReadUInt16();
            entity.Attributes.Strength = msgIn.ReadUInt16();
            entity.Attributes.Agility = msgIn.ReadUInt16();
            entity.Attributes.Intelligence = msgIn.ReadUInt16();
            entity.Attributes.Knowledge = msgIn.ReadUInt16();
            entity.Attributes.Luck = msgIn.ReadUInt16();
        }
    }

    public void Teleport(NetIncomingMessage msgIn)
    {
        ushort id = msgIn.ReadUInt16();
        float X = QuantateUshortToFloat(msgIn.ReadUInt16());
        float Z = QuantateUshortToFloat(msgIn.ReadUInt16());

        Data.GetEntity(id)?.Teleport(X, Z);
    }

    public void PositionCorrection(NetIncomingMessage msgIn)
    {
        float X = QuantateUshortToFloat(msgIn.ReadUInt16());
        float Z = QuantateUshortToFloat(msgIn.ReadUInt16());

        Data.Player.Teleport(X, Z);
    }

    public void UpdateMaxExperience(NetIncomingMessage msgIn)
    {
        long maxExperience = msgIn.ReadInt64();
        long experience = msgIn.ReadInt64();
        ushort level = msgIn.ReadByte();
        Data.Player.SetLevel(level);
        uiController.UpdateMaxExperience(maxExperience, experience, level);
    }

    public void LevelUp(NetIncomingMessage msgIn)
    {
        ushort id = msgIn.ReadUInt16();
        byte level = msgIn.ReadByte();

        Entity entity = Data.GetEntity(id);

        if (entity != null)
        {
            entity.SetLevel(level);
        }
        else { Debug.Log(id + " Not found!"); }
    }

    public void UpdateExperience(NetIncomingMessage msgIn)
    {
        long newExpTick = msgIn.ReadInt64();
        long experience = msgIn.ReadInt64();
        uiController.UpdateExperience(newExpTick, experience);
    }

    public void DamageInfo(NetIncomingMessage msgIn)
    {
        ushort id = msgIn.ReadUInt16();
        int damageValue = msgIn.ReadInt32();
        EffectType effectType = (EffectType)msgIn.ReadByte();
        bool crit = msgIn.ReadBoolean();

        Entity entity = Data.GetEntity(id);
        if (entity == null)
            Debug.LogWarning($"{id} not found");
        else
            entity.SpawnDamageInfo(effectType, damageValue, crit);
    }

    public void RespawnPlayer(NetIncomingMessage msgIn)
    {
        //ushort targetId = msgIn.ReadUInt16();
        float X = QuantateUshortToFloat(msgIn.ReadUInt16());
        float Z = QuantateUshortToFloat(msgIn.ReadUInt16());

        Data.Player.Teleport(X, Z);
        Data.Player.Buffs.Clear();
    }

    public Entity GetEntityObjectByModelType(ModelType modelType)
    {
        Entity entity = GameObject.Instantiate(SpawnCollection.Instance.EntityPrefabs[modelType]).GetComponent<Entity>();

        if (entity == null)
            Debug.LogError($"No model type of: {modelType}");

        return entity;
    }

    //Chat
    public void ChatMessage(NetIncomingMessage msgIn, MasterMessageType masterMsgType)
    {
        string username = msgIn.ReadString();
        string message = msgIn.ReadString();
        //string date = msgIn.ReadString();

        uiController.IncomingChatMessage(masterMsgType, username, message);
    }

    public void SystemInfo(NetIncomingMessage msgIn)
    {
        SystemMessageType msgType = (SystemMessageType)msgIn.ReadByte();

        switch (msgType)
        {
            case SystemMessageType.OutOfRange:
                uiController.SystemMessage("Targe out of range!");
                break;
            case SystemMessageType.NotEnoughMana:
                uiController.SystemMessage("Not enough mana!");
                break;
            case SystemMessageType.YouAreDead:
                uiController.SystemMessage("You piece of crap! You dead NOOB!");
                break;
        }
    }

    private float QuantateUshortToFloat(ushort val)
    {
        return val / SMALLEST_FLOAT_STEP;
    }

    private ushort QuantateFloatToUshort(float val)
    {
        return (ushort)(val * SMALLEST_FLOAT_STEP);
    }

    public void SetMapSize(float mapSize)
    {
        MAX_VALUE_OF_POSITION = mapSize;
        SMALLEST_FLOAT_STEP = (ushort.MaxValue / MAX_VALUE_OF_POSITION);
    }
}