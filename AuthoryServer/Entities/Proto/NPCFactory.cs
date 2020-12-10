using System.Collections.Generic;

namespace AuthoryServer.Entities
{
    /// <summary>
    /// Provides a factory for creating NPCs/Mobs
    /// </summary>
    public class NPCFactory
    {
        public Dictionary<ModelType, MobEntity> _npcBaseValues;

        private static NPCFactory _instance;
        public static NPCFactory Instance => _instance ??= new NPCFactory();

        public NPCFactory()
        {
            _npcBaseValues = new Dictionary<ModelType, MobEntity>();
            LoadNpcValues();
        }

        /// <summary>
        /// Returns a MobEntity of the requested ModelType.
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns>Null if no matching element in the dictionary, else returns the element.</returns>
        public MobEntity GetNpcValuesOf(ModelType modelType)
        {
            return _npcBaseValues.ContainsKey(modelType) ? _npcBaseValues[modelType] : null;
        }

        public void LoadNpcValues(Dictionary<ModelType, MobEntity> values = null)
        {
            if (values == null) _manualLoadTest();
            else _npcBaseValues = values;
        }

        /// <summary>
        /// Loading NPCFactory by MobEntities.
        /// </summary>
        private void _manualLoadTest()
        {
            _npcBaseValues.Add(ModelType.MeleeNPC, new MobEntity(ModelType.MeleeNPC, 25, 25, 25, 25, 100, 25, 1, SkillFactory.Instance.GetSkill(SkillID.MeleeAutoAttack)));

            _npcBaseValues.Add(ModelType.WizardNPC, new MobEntity(ModelType.WizardNPC, 20, 20, 20, 20, 600, 20, 1, SkillFactory.Instance.GetSkill(SkillID.Fireball)));
            _npcBaseValues.Add(ModelType.RangerNPC, new MobEntity(ModelType.RangerNPC, 20, 20, 20, 20, 600, 20, 1, SkillFactory.Instance.GetSkill(SkillID.Fireball)));
        }
    }
}
