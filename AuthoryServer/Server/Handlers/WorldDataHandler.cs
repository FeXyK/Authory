using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AuthoryServer.Entities;
using AuthoryServer.Entities.EntityDerived;

namespace AuthoryServer.Server.Handlers
{
    /// <summary>
    /// Stores and handles MapServer Data. Like Players, Mobs, Grids etc...
    /// </summary>
    public class WorldDataHandler
    {
        /// <summary>
        /// The resolution where the virtual space will be divided
        /// </summary>
        public const int GRID_RESOLUTION = 10;

        /// <summary>
        /// The maximum size of a GridCell
        /// </summary>
        public const int GRID_SIZE = 200;

        /// <summary>
        /// Returns the virtual world size by GRID_SIZE * GRID_RESOLUTION
        /// </summary>
        public int WORLD_SIZE { get { return GRID_SIZE * GRID_RESOLUTION; } }

        public ushort EntityCount { get; private set; }
        public int ServerIndex { get; private set; }

        /// <summary>
        /// All GridCells in a 2D array
        /// </summary>
        public GridCell[,] Grid { get; private set; }

        public ConcurrentDictionary<int, PlayerEntity> RecentlyOnlinePlayers { get; private set; }

        public ConcurrentDictionary<long, PlayerEntity> AwaitingConnections { get; private set; }


        /// <summary>
        /// Player references stored by their UID
        /// </summary>
        public ConcurrentDictionary<long, PlayerEntity> PlayersByUid { get; private set; }

        /// <summary>
        /// Player references stored by their ID
        /// </summary>
        public ConcurrentDictionary<ushort, PlayerEntity> PlayersById { get; private set; }

        /// <summary>
        /// Initializes the WorldDataHandler, by dividing the virtual world space into smaller GridCells, and setting the neighbours of every GridCell
        /// </summary>
        /// <param name="serverIndex"></param>
        public WorldDataHandler(int serverIndex)
        {
            ServerIndex = serverIndex;
            AwaitingConnections = new ConcurrentDictionary<long, PlayerEntity>();
            PlayersByUid = new ConcurrentDictionary<long, PlayerEntity>(2, 1000);
            PlayersById = new ConcurrentDictionary<ushort, PlayerEntity>(2, 1000);

            RecentlyOnlinePlayers = new ConcurrentDictionary<int, PlayerEntity>();

            Grid = new GridCell[GRID_RESOLUTION, GRID_RESOLUTION];

            for (int z = 0; z < GRID_RESOLUTION; z++)
            {
                for (int x = 0; x < GRID_RESOLUTION; x++)
                {
                    Grid[z, x] = new GridCell(x * GRID_SIZE, z * GRID_SIZE, GRID_SIZE, GRID_SIZE, this);
                    Console.Write("{0,4},{1,4}|", Grid[z, x].Area.X, Grid[z, x].Area.Y);
                }
                Console.WriteLine();
            }

            for (int z = 0; z < GRID_RESOLUTION; z++)
            {
                for (int x = 0; x < GRID_RESOLUTION; x++)
                {
                    Grid[z, x].Neighbours.Add(Grid[z, x]);
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            int zz = i + z;
                            int xx = j + x;

                            if (j != 0 || i != 0)
                                if (zz >= 0 && zz < GRID_RESOLUTION &&
                                    xx >= 0 && xx < GRID_RESOLUTION)
                                {
                                    Grid[z, x].Neighbours.Add(Grid[zz, xx]);
                                }
                        }
                    }
                }
            }
        }

        public void Add(TeleportEntity teleport)
        {
            GetGridCellByPosition(teleport.Position).Add(teleport);
        }

        /// <summary>
        /// Sets the ID of the PlayerEntity based on the last added entity ID.
        /// Adds a PlayerEntity to the PlayersByid, and PlayersByUid dictionary, also adds to the GridCell according to its position
        /// </summary>
        /// <param name="player"></param>
        /// <param name="position"></param>
        public void Add(PlayerEntity player, Vector3 position)
        {
            player.SetId((ushort)(PlayersById.Count + 20000));
            player.SetPositionWithoutGridCellCheck(position);

            GetGridCellByPosition(player.Position).Add(player);
            PlayersByUid.TryAdd(player.Uid, player);
            PlayersById.TryAdd(player.Id, player);
        }

        /// <summary>
        /// Adds a PlayerEntity to the PlayersByid, and PlayersByUid dictionary, also adds to the GridCell according to its position
        /// </summary>
        /// <param name="player">The PlayerEntity thats going to be added</param>
        public void Add(PlayerEntity player)
        {
            //player.SetId((ushort)(PlayersById.Count + 10000));
            GetGridCellByPosition(player.Position).Add(player);
            PlayersByUid.TryAdd(player.Uid, player);
            PlayersById.TryAdd(player.Id, player);
        }

        /// <summary>
        /// Adds an Entity to the WorldData, specifically to the GridCell based on its position
        /// </summary>
        /// <param name="mob"></param>
        public void Add(Entity mob)
        {

            GetGridCellByPosition(mob.Position).Add(mob);
        }


        /// <summary>
        /// Converts given position into Grid indexes, 
        /// and returns the GridCell from the Grid[,] based on these indexes
        /// </summary>
        /// <param name="position"></param>
        /// <returns>Returns null if the position is out of bounds</returns>
        public GridCell GetGridCellByPosition(Vector3 position)
        {
            int x = (int)position.X / GRID_SIZE;
            int z = (int)position.Z / GRID_SIZE;
            if (x >= 0 && z >= 0 && x < GRID_RESOLUTION && z < GRID_RESOLUTION)
            {
                return Grid[z, x];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns PlayerEntity by its UID
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public PlayerEntity Get(long uid)
        {
            return PlayersByUid.ContainsKey(uid) ? PlayersByUid[uid] : null;
        }

        /// <summary>
        /// Returns an Entity (Can be PlayerEntity or MobEntity too) by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Entity Get(ushort id)
        {
            if (PlayersById.ContainsKey(id))
            {
                return PlayersById[id];
            }

            foreach (var grid in Grid)
            {
                if (grid.MobEntities.ContainsKey(id))
                {
                    return grid.MobEntities[id];
                }
            }
            return null;
        }

        public EntityBase GetInteractedEntity(ushort id)
        {
            foreach (var grid in Grid)
            {
                if (grid.GetEntity(id, out EntityBase entity))
                {
                    return entity;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds player to a new GridCell, and removes it from its old GridCell. 
        /// Notifies the new GridCell and its neighbours players by it.
        /// </summary>
        /// <param name="player"></param>
        public void ReAdd(PlayerEntity player)
        {

            GridCell cell = GetGridCellByPosition(player.Position);
            if (cell == null)
            {
                player.Kill();
                return;
            }
            cell.ReAdd(player);
            Program.Master.MapServers[ServerIndex].OutgoingMessageHandler.SendFullEntityUpdatesToPlayer(player);
            Program.Master.MapServers[ServerIndex].OutgoingMessageHandler.SendFullResourceEntitiesUpdate(player);
        }

        /// <summary>
        /// Adds Entity to a new GridCell, and removes it from its old GridCell. 
        /// If position is out of bounds, it respawns the Entity
        /// </summary>
        /// <param name="entity"></param>
        public void ReAdd(Entity entity)
        {
            GridCell grid = GetGridCellByPosition(entity.Position);
            if (grid != null)
            {
                grid.ReAdd(entity);
            }
            else
            {
                entity.Respawn();
            }
        }

        /// <summary>
        /// Returns the next available ID for the Players
        /// </summary>
        /// <returns></returns>
        public int GetNextPlayerId()
        {
            return GridCell.AllPlayerCount++ + 10000;
        }

        /// <summary>
        /// Adds the PlayerEntity to the RecentlyOnlinePlayers dictionary by its CharacterID Note: CharacterId is the ID from the database, CharacterId not equals to ID. 
        /// Removes the requested PlayerEntity from the PlayersById, PlayersByUid dictionaries, and the containing GridCell.
        /// </summary>
        /// <param name="requestedCharacter"></param>
        /// <returns></returns>
        public bool RemovePlayer(PlayerEntity requestedCharacter)
        {
            if (requestedCharacter.Connection != null && requestedCharacter.Connection.Status != Lidgren.Network.NetConnectionStatus.Disconnected)
            {
                requestedCharacter.Connection.Disconnect("Removed");
            }
            RecentlyOnlinePlayers.TryAdd(requestedCharacter.CharacterId, requestedCharacter);

            if (PlayersByUid.TryRemove(requestedCharacter.Uid, out _))
            {
                Console.WriteLine("Player removed from PlayersById");
            }
            else
            {
                Console.WriteLine("Character can not be removed or not exists!");
            }

            if (PlayersById.TryRemove(requestedCharacter.Id, out _))
            {
                Console.WriteLine("Player removed from PlayersByUid");
            }
            else
            {
                Console.WriteLine("Character can not be removed or not exists!");
            }

            return requestedCharacter.GridCell.Remove(requestedCharacter);
        }


        public void FullRemovePlayer(PlayerEntity player)
        {
            if (PlayersByUid.TryRemove(player.Uid, out _))
            {
                Console.WriteLine("Player removed from PlayersById");
            }
            else
            {
                Console.WriteLine("Character can not be removed or not exists!");
            }

            if (PlayersById.TryRemove(player.Id, out _))
            {
                Console.WriteLine("Player removed from PlayersByUid");
            }
            else
            {
                Console.WriteLine("Character can not be removed or not exists!");
            }

            player.GridCell.Remove(player);
        }
    }
}
