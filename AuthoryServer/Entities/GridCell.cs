using System.Drawing;
using System.Collections.Generic;
using System.Collections.Concurrent;
using AuthoryServer.Entities;
using AuthoryServer.Server.Handlers;
using AuthoryServer.Entities.EntityDerived;
using System;

namespace AuthoryServer
{
    public class GridCell
    {
        private static ushort _next_id;

        public static ushort AllPlayerCount;
        public static ushort AllEntityCount;
        public ushort PlayerCount { get; private set; }
        public ushort EntityCount { get; private set; }
        public ConcurrentDictionary<ushort, Entity> MobEntities { get; private set; }
        public ConcurrentDictionary<ushort, PlayerEntity> PlayersById { get; private set; }
        public ConcurrentDictionary<ushort, EntityBase> ResourceEntites { get; private set; }

        public Rectangle Area { get; private set; }

        public WorldDataHandler Data { get; private set; }
        public List<GridCell> Neighbours { get; private set; }

        public GridCell(int x, int y, int width, int height, WorldDataHandler data)
        {
            Data = data;
            _next_id = 0;

            AllPlayerCount = 0;
            AllEntityCount = 0;

            PlayerCount = 0;
            EntityCount = 0;

            Area = new Rectangle(x, y, width, height);
            Neighbours = new List<GridCell>();

            MobEntities = new ConcurrentDictionary<ushort, Entity>();
            PlayersById = new ConcurrentDictionary<ushort, PlayerEntity>(2, 1000);

            ResourceEntites = new ConcurrentDictionary<ushort, EntityBase>();
        }

        public bool GetEntity(ushort id, out EntityBase entity)
        {
            entity = null;

            if (ResourceEntites.ContainsKey(id))
            {
                entity = ResourceEntites[id];
                return true;
            }
            if (MobEntities.ContainsKey(id))
            {
                entity = MobEntities[id];
                return true;
            }
            if (PlayersById.ContainsKey(id))
            {
                entity = PlayersById[id];
                return true;
            }

            return false;
        }

        public void Add(PlayerEntity player)
        {
            if (!PlayersById.TryAdd(player.Id, player)) System.Console.WriteLine(player.Id + " failed to add to dictionary");
            else
            {
                _next_id++;

                player.SetGridCell(this);
                 
                AllPlayerCount++;
                AllEntityCount++;

                PlayerCount++;
                EntityCount++;
            }
        }

        public void Add(Entity mob)
        {
            mob.SetId(_next_id);

            if (MobEntities.TryAdd(mob.Id, mob))
            {
                _next_id++;

                mob.SetGridCell(this);
                EntityCount++;
                AllEntityCount++;
            }
        }

        public void Add(TeleportEntity teleport)
        {
            teleport.SetId(_next_id);

            if (ResourceEntites.TryAdd(teleport.Id, teleport))
            {
                _next_id++;

                teleport.SetGridCell(this);
                EntityCount++;
                AllEntityCount++;
            }
        }

        public void ReAdd(Entity mob)
        {
            if (MobEntities.TryAdd(mob.Id, mob))
            {
                GridCell cell = mob.GridCell;

                mob.SetGridCell(this);

                if (cell != null)
                    cell.Remove(mob);

                EntityCount++;
                AllEntityCount++;
            }
        }

        public void ReAdd(PlayerEntity player)
        {
            PlayersById.TryAdd(player.Id, player);
            if (this != player.GridCell)
            {
                GridCell cell = player.GridCell;
                player.SetGridCell(this);
                if (cell != null)
                {
                    cell.Remove(player);
                }

                PlayerCount++;
                EntityCount++;
            }
        }

        public void Remove(Entity mob)
        {
            MobEntities.TryRemove(mob.Id, out _);

            EntityCount--;
            AllEntityCount--;
        }

        public bool Remove(PlayerEntity player)
        {
            if (PlayersById.TryRemove(player.Id, out _))
            {
                PlayerCount--;

                return true;
            }
            return false;
        }

        public override string ToString() => string.Format("Z: {0,4} X:{1,4} ", Area.Y / WorldDataHandler.GRID_SIZE + 1, Area.X / WorldDataHandler.GRID_SIZE + 1);
    }
}