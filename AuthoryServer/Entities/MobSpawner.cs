using System;
using System.Collections.Generic;

namespace AuthoryServer.Entities
{
    public class MobSpawner
    {
        public Vector3 Center { get; private set; }
        public float Radius { get; private set; }
        public ushort MaxCount { get; private set; }
        public ushort Count { get; private set; }
        public ushort RespawnTime { get; private set; }

        public ModelType ModelType { get; private set; }


        public List<MobEntity> MobEntities { get; private set; }


        public MobSpawner(ModelType mobType, Vector3 center, float radius = 30f, ushort maxCount = 30, ushort respawnTime = 60)
        {
            MobEntities = new List<MobEntity>();

            ModelType = mobType;
            Center = center;
            Radius = radius;
            MaxCount = maxCount;
            RespawnTime = respawnTime;
            Count = 0;
        }

        public void SpawnAll(AuthoryServer server)
        {
            for (int i = 0; i < MaxCount; i++)
            {
                MobEntity mob = new MobEntity(NPCFactory.Instance.GetNpcValuesOf((new Random().Next(0, 20) > 10 ? ModelType.WizardNPC : ModelType.MeleeNPC)), Center + Vector3.RandomRangeSquare(-(int)Radius, (int)Radius), server);
                MobEntities.Add(mob);
                server.Data.Add(mob);
                Count++;
            }
        }
    }
}
