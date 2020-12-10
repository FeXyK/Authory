using System;
using System.Collections.Generic;
using Lidgren.Network;
using Lidgren.Network.Shared;
using AuthoryServer.Entities;

namespace AuthoryServer.Server.Handlers
{
    /// <summary>
    /// Messages need to be sent to the connected clients handled here. 
    /// </summary>
    public class OutgoingMessageHandler
    {
        /// <summary>
        /// Maximum size of a message in bytes that can be packed. 
        /// </summary>
        public const int MAX_PACKAGE_SIZE = 1000;

        /// <summary>
        /// Size of the virtual world.
        /// </summary>
        private float MAX_WORLD_SIZE;

        /// <summary>
        /// For transforming float into ushort. 
        /// Calculated by dividing the maximum value of ushort by the MAX_WORLD_SIZE.
        /// </summary>
        private readonly float SMALLEST_FLOAT_STEP;

        public NetServer Server { get; private set; }
        public WorldDataHandler Data { get; private set; }

        public long OverallBytesSent { get; private set; } = 0;

        public OutgoingMessageHandler(NetServer server, WorldDataHandler data)
        {
            MAX_WORLD_SIZE = data.WORLD_SIZE;
            SMALLEST_FLOAT_STEP = (ushort.MaxValue / MAX_WORLD_SIZE);

            Server = server;
            Data = data;
        }

        /// <summary>
        /// Sends system message to client, for information, gets written to the chat on client side.
        /// </summary>
        /// <param name="connection">Message will be sent to this connection</param>
        /// <param name="msgType">The information that can be described by a value, it will be transformed into a predefined message on the client side. Like "Out of range"</param>
        public void SendSystemInfo(NetConnection connection, SystemMessageType msgType)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MessageType.SystemInfo);
            msgOut.Write((byte)msgType);
            if (connection != null)
                connection.SendMessage(msgOut, NetDeliveryMethod.Unreliable, 0);
        }

        /// <summary>
        /// If skill was interrupted under casting this will inform the client to reset the cooldown of that skill.
        /// </summary>
        /// <param name="caster">The caster of the interrupted skill.</param>
        /// <param name="skillID">The ID of the skill that got interrupted.</param>
        public void SendSkillInterrupt(Entity caster, SkillID skillID)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MessageType.SkillInterrupt);
            msgOut.Write((byte)skillID);

            caster.GetConnection()?.SendMessage(msgOut, NetDeliveryMethod.Unreliable, 0);
        }

        /// <summary>
        /// Sends movement speed of an entity to 
        /// the players in the entity's GridCell and its neigbours.
        /// </summary>
        /// <param name="entity">The entity of the movement speed notification</param>
        public void SendMovementSpeedChange(Entity entity)
        {
            NetBuffer buffer = new NetBuffer();

            buffer.Write((byte)MessageType.MovementSpeed);
            buffer.Write(entity.Id);
            buffer.Write(entity.MovementSpeed);

            SendDataToNeigbours(buffer.Data, entity.GridCell, NetDeliveryMethod.Unreliable);
        }


        /// <summary>
        /// Creates a package from the entity and sends it to 
        /// the players in the entity's GridCell and its neighbours.
        /// </summary>
        /// <param name="entity">The entity that the message will be created by</param>
        public void SendFullEntityUpdate(Entity entity)
        {
            NetBuffer buffer = new NetBuffer();

            buffer.Write((byte)MessageType.FullEntityUpdate);
            buffer.Write(CreateFullEntityUpdatePackage(entity));

            SendDataToNeigbours(buffer.Data, entity.GridCell);
        }

        /// <summary>
        /// Creates an attribute update of the given entity and sends it to
        /// the players in the entity's GridCell and its neighbours.
        /// </summary>
        /// <param name="entity"></param>
        public void SendAttributeUpdate(Entity entity)
        {
            if (entity.GridCell == null) return;

            //Console.WriteLine("Entity full update");
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MessageType.AttributeUpdate);
            msgOut.Write(entity.Id, 16);
            msgOut.Write((ushort)entity.Health.MaxValue, 16);
            msgOut.Write((ushort)entity.Health.Value, 16);
            msgOut.Write((ushort)entity.Mana.MaxValue, 16);
            msgOut.Write((ushort)entity.Mana.Value, 16);

            for (int i = 0; i < 6; i++)
            {
                msgOut.Write((ushort)entity.Attributes[i], 16);
            }

            //Console.WriteLine("Attr update: " + entity.Name + " " + entity.Level);

            byte[] data = msgOut.Data;

            foreach (var otherPlayer in entity.GridCell.PlayersById.Values)
            {
                msgOut = Server.CreateMessage();
                msgOut.Write(data);
                if (otherPlayer.Connection != null)
                    otherPlayer.Connection.SendMessage(msgOut, NetDeliveryMethod.Unreliable, 0);
            }
        }

        /// <summary>
        /// Sends a buff apply message that will be sent to 
        /// the players in the entity's GridCell and its neighbours.
        /// </summary>
        /// <param name="entity">The buff applied on this entity.</param>
        /// <param name="buff">The buff that is applied.</param>
        public void SendBuffApply(Entity entity, Buff buff)
        {
            NetBuffer data = CreateBuffApplyPackage(entity, buff);

            SendDataToNeigbours(data.Data, entity.GridCell);
        }

        /// <summary>
        /// Sends a buff refresh message that will be sent to 
        /// the players in the entity's GridCell and its neighbours.
        /// </summary>
        /// <param name="entity">The buff refreshed on this entity</param>
        /// <param name="buff">The buff that is refreshed</param>
        public void SendBuffRefresh(Entity entity, Buff buff)
        {
            NetBuffer data = CreateBuffRefreshPackage(entity, buff);

            SendDataToNeigbours(data.Data, entity.GridCell);
        }

        /// <summary>
        /// Sends a buff remove message that will be sent to 
        /// the players in the entity's GridCell and its neighbours.
        /// </summary>
        /// <param name="entity">The buff removed from this entity</param>
        /// <param name="buff">The buff that is removed</param>

        public void SendBuffRemove(Entity entity, Buff buff)
        {
            NetBuffer data = CreateBuffRemovePackage(entity, buff);

            SendDataToNeigbours(data.Data, entity.GridCell);
        }

        /// <summary>
        /// Sends position of entity to 
        /// the players in the entity's GridCell and its neighbours. 
        /// On the client side the entity will be "teleported/rapidly repositioned" to the given position.
        /// </summary>
        /// <param name="entity">The entity that has been teleported.</param>
        public void SendTeleport(Entity entity)
        {
            NetBuffer buffer = new NetBuffer();

            buffer.Write((byte)MessageType.Teleport);

            buffer.Write(entity.Id);
            buffer.Write(TransformFloatToUshort(entity.Position.X));
            buffer.Write(TransformFloatToUshort(entity.Position.Z));

            SendDataToNeigbours(buffer.Data, entity.GridCell, NetDeliveryMethod.Unreliable);
        }


        /// <summary>
        /// Sends position correction to the client.
        /// Only the client that's position has been corrected will get the message
        /// </summary>
        /// <param name="entity"></param>
        public void SendPositionCorrection(Entity entity)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MessageType.PositionCorrection);
            msgOut.Write(TransformFloatToUshort(entity.Position.X));
            msgOut.Write(TransformFloatToUshort(entity.Position.Z));

            entity.GetConnection()?.SendMessage(msgOut, NetDeliveryMethod.Unreliable, 0);
        }

        /// <summary>
        /// Sends death notification of an entity to 
        /// the players in the entity's GridCell and its neighbours.
        /// </summary>
        /// <param name="entity"></param>
        public void SendDeathMessage(Entity entity)
        {
            NetBuffer buffer = new NetBuffer();
            buffer.Write((byte)MessageType.Death);
            buffer.Write(entity.Id);

            SendDataToNeigbours(buffer.Data, entity.GridCell, NetDeliveryMethod.ReliableUnordered);
        }


        //-----------------------------------------------------
        //Player message handlers
        //-----------------------------------------------------   

        /// <summary>
        /// Sends entity update(Health, Mana) to 
        /// the players in the entity's GridCell and its neighbours.
        /// </summary>
        /// <param name="entity">The entity that is updated.</param>
        public void SendEntityUpdate(Entity entity)
        {
            if (entity.GridCell == null) return;

            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MessageType.PlayerUpdate);
            msgOut.Write(entity.Id, 16);
            //msgOut.Write(player.EntityTick, 16);
            msgOut.Write(entity.Health.Value, 16);
            msgOut.Write(entity.Mana.Value, 16);
            byte[] data = msgOut.Data;

            SendDataToNeigbours(data, entity.GridCell);
        }

        /// <summary>
        /// Sends experience info to 
        /// the players in the entity's GridCell and its neighbours.
        /// </summary>
        /// <param name="player">The Player that gets the message and the experience is updated for.</param>
        /// <param name="experience">Current experience of the player</param>
        public void SendExperienceInfo(Entity player, long experience)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MessageType.UpdateExperience);
            msgOut.Write(player.EntityTick);
            msgOut.Write(experience);

            Server.SendMessage(msgOut, player.GetConnection(), NetDeliveryMethod.ReliableUnordered);
        }

        /// <summary>
        /// Sends player disconnected message to 
        /// the players in the entity's GridCell and its neighbours.
        /// </summary>
        /// <param name="player">Player that is disconnected.</param>
        public void SendDisconnect(PlayerEntity player)
        {
            NetBuffer buffer = new NetBuffer();

            buffer.Write((byte)MessageType.Disconnect);
            buffer.Write(player.Id);

            SendDataToAllPlayers(buffer.Data, NetDeliveryMethod.ReliableUnordered);
        }

        /// <summary>
        /// Sends entity level up information to the Player that leveled up.
        /// </summary>
        /// <param name="player">The entity that leveled up.</param>
        public void SendLevelUp(PlayerEntity player)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MessageType.UpdateMaxExperience);
            msgOut.Write(player.MaxExperience);
            msgOut.Write(player.Experience);
            msgOut.Write(player.Level);

            Server.SendMessage(msgOut, player.GetConnection(), NetDeliveryMethod.ReliableUnordered);
        }

        /// <summary>
        /// Sends entity level up to 
        /// the players in the entity's GridCell and its neighbours.
        /// </summary>
        /// <param name="entity">The entity that leveled up.</param>
        public void SendLevelUpInfo(PlayerEntity entity)
        {
            NetBuffer buffer = new NetBuffer();

            buffer.Write((byte)MessageType.LevelUp);
            buffer.Write(entity.Id);
            buffer.Write(entity.Level);

            SendDataToNeigbours(buffer.Data, entity.GridCell, NetDeliveryMethod.ReliableUnordered);
        }

        /// <summary>
        /// Sends player respawn message to the Player.
        /// </summary>
        /// <param name="entity">The player that respawned.</param>
        public void SendPlayerRespawn(PlayerEntity entity)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MessageType.Respawn);
            //msgOut.Write(entity.Id);
            msgOut.Write(TransformFloatToUshort(entity.Position.X), 16);
            msgOut.Write(TransformFloatToUshort(entity.Position.Z), 16);

            entity.GetConnection().SendMessage(msgOut, NetDeliveryMethod.ReliableUnordered, 0);
        }

        /// <summary>
        /// Creates messages to every GridCell from the players positions that has been changed and 
        /// sends them to the players in the GridCell and its neighbours.
        /// </summary>
        /// <returns>Returns the overall sent bytes by this call.</returns>
        public int SendPlayerMovementInfoByGridCell()
        {
            List<byte[]> gridData = new List<byte[]>();

            int sentBytes = 0;
            NetOutgoingMessage msgOut;
            NetBuffer buffer;

            //Serializing data for every GridCell at once.
            foreach (var grid in Data.Grid)
            {
                buffer = new NetBuffer();
                buffer.Write((byte)MessageType.PlayerMovement);
                buffer.Write((ushort)grid.PlayersById.Count, 16);
                ushort dataCount = 0;
                foreach (var player in grid.PlayersById.Values)
                {
                    if (player.DirtyPos)
                    {
                        buffer.Write(player.Id, 16);
                        buffer.Write(TransformFloatToUshort(player.Position.X), 16);
                        buffer.Write(TransformFloatToUshort(player.Position.Z), 16);

                        player.DirtyPos = false;
                        dataCount++;
                    }
                }
                buffer.WriteAt(8, dataCount);
                gridData.Add(buffer.Data);
            }

            int i = 0;
            int gridIndex;
            int index;
            int GRID_RESOLUTION = WorldDataHandler.GRID_RESOLUTION;
            //Iterating through every GridCell and sending the data from it to its neighbours players including itself.
            foreach (var grid in Data.Grid)
            {
                foreach (var player in grid.PlayersById.Values)
                {
                    index = 0;
                    gridIndex = i / GRID_RESOLUTION;

                    for (int y = gridIndex - 1; y < gridIndex + 2; y++)
                    {
                        for (int x = (i % GRID_RESOLUTION) - 1; x < (i % GRID_RESOLUTION) + 2; x++)
                        {
                            index = y * GRID_RESOLUTION + x;

                            if (index >= 0 && index < gridData.Count && x >= 0 && x < GRID_RESOLUTION)
                            {

                                msgOut = Server.CreateMessage();
                                msgOut.Write(gridData[index]);

                                if (player.GetConnection() != null)
                                {
                                    player.GetConnection().SendMessage(msgOut, NetDeliveryMethod.Unreliable, 0);
                                }
                                sentBytes += gridData[index].Length;
                            }
                        }
                    }
                }
                i++;
            }
            return sentBytes;
        }


        /// <summary>
        /// Sends the resurce entities information from the player's grid to the player.
        /// </summary>
        /// <param name="player">Message receiver.</param>
        public void SendFullResourceEntitiesUpdate(PlayerEntity player)
        {
            Console.WriteLine("RESOURFCE UJPDATE");
            if (player.GridCell.ResourceEntites.Count == 0) return;

            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MessageType.GridResourceEntityFullUpdate);
            msgOut.Write((ushort)player.GridCell.ResourceEntites.Count);

            foreach (var resourceEntity in player.GridCell.ResourceEntites.Values)
            {
                msgOut.Write(resourceEntity.Id);
                msgOut.Write(resourceEntity.Name);

                msgOut.Write((byte)resourceEntity.ModelType);
                msgOut.Write(TransformFloatToUshort(resourceEntity.Position.X));
                msgOut.Write(TransformFloatToUshort(resourceEntity.Position.Z));
            }

            player.GetConnection()?.SendMessage(msgOut, NetDeliveryMethod.ReliableUnordered, 0);
        }


        /// <summary>
        /// Creates messages from every entity in the player's GridCell and it's neighbours 
        /// and sends them to the player. 
        /// This method should be used when a player is added to a new GridCell!
        /// </summary>
        /// <param name="player">The Player that will receive the messages</param>
        public void SendFullEntityUpdatesToPlayer(PlayerEntity player)
        {
            NetOutgoingMessage msgOut;
            foreach (var grid in player.GridCell.Neighbours)
            {

                ushort count = 0;
                msgOut = Server.CreateMessage();
                msgOut.Write((byte)MessageType.GridFullEntityUpdate);
                msgOut.Write((ushort)grid.MobEntities.Count);
                foreach (var mob in grid.MobEntities.Values)
                {
                    if (msgOut.LengthBytes > MAX_PACKAGE_SIZE)
                    {
                        msgOut.WriteAt(8, count);
                        player.GetConnection().SendMessage(msgOut, NetDeliveryMethod.ReliableUnordered, 0);
                        count = 0;


                        msgOut = Server.CreateMessage();
                        msgOut.Write((byte)MessageType.GridFullEntityUpdate);
                        msgOut.Write((ushort)grid.MobEntities.Count);
                    }
                    else
                    {
                        msgOut.Write(CreateFullEntityUpdatePackage(mob));
                        count++;
                    }
                }
                foreach (var otherPlayer in grid.PlayersById.Values)
                {
                    if (msgOut.LengthBytes > MAX_PACKAGE_SIZE)
                    {
                        msgOut.WriteAt(8, count);
                        player.GetConnection().SendMessage(msgOut, NetDeliveryMethod.ReliableUnordered, 0);
                        count = 0;


                        msgOut = Server.CreateMessage();
                        msgOut.Write((byte)MessageType.GridFullEntityUpdate);
                        msgOut.Write((ushort)grid.MobEntities.Count);
                    }
                    else
                    {
                        msgOut.Write(CreateFullEntityUpdatePackage(otherPlayer));
                        count++;
                    }
                }
                Console.WriteLine($"Message size: {msgOut.LengthBytes}");
                msgOut.WriteAt(8, count);
                player.GetConnection().SendMessage(msgOut, NetDeliveryMethod.ReliableUnordered, 0);
            }
        }


        //------------------------------------------------------------
        //MOB message handling 
        //------------------------------------------------------------

        /// <summary>
        /// Sends entity respawn message to 
        /// the entity's GridCell and it's neighbours. 
        /// </summary>
        /// <param name="entity">The entity that respawned</param>
        public void SendEntityRespawn(MobEntity entity)
        {
            if (entity.GridCell == null) return;

            NetBuffer data = new NetBuffer();

            data.Write((byte)MessageType.MobRespawn);
            data.Write(CreateMobRespawnPackage(entity));

            SendDataToNeigbours(data.Data, entity.GridCell);
        }

        /// <summary>
        /// Sends a MobUpdate message(Health) to 
        /// the entity's GridCell and it's neighbours. 
        /// 
        /// </summary>
        /// <param name="entity">The updated entity.</param>
        public void SendEntityUpdate(MobEntity entity)
        {
            NetBuffer buffer = new NetBuffer();

            buffer.Write((byte)MessageType.MobUpdate);

            buffer.Write(entity.Id);
            buffer.Write((ushort)entity.Health.Value, 16);

            SendDataToNeigbours(buffer.Data, entity.GridCell, NetDeliveryMethod.UnreliableSequenced);

        }

        /// <summary>
        /// Sends a MobPathToPosition message to 
        /// the entity's GridCell and it's neighbours. 
        /// The client will interpolate the movement of the mobs based on this message.
        /// </summary>
        /// <param name="mob">The mob that is pathing</param>
        public void SendMobPathToPosition(Entity mob)
        {
            foreach (var grid in mob.GridCell.Neighbours)
            {
                foreach (var player in grid.PlayersById.Values)
                {
                    NetOutgoingMessage msgOut = Server.CreateMessage();

                    msgOut.Write((byte)MessageType.MobMovementToPosition);
                    msgOut.Write(mob.Id);

                    msgOut.Write(TransformFloatToUshort(mob.Position.X));
                    msgOut.Write(TransformFloatToUshort(mob.Position.Z));

                    msgOut.Write(TransformFloatToUshort(mob.EndPosition.X));
                    msgOut.Write(TransformFloatToUshort(mob.EndPosition.Z));

                    //msgOut.Write(mob.PathValue);

                    msgOut.Write(TransformFloatToUshort(mob.MovementSpeed));

                    if (player.Connection != null)
                    {
                        player.Connection.SendMessage(msgOut, NetDeliveryMethod.ReliableUnordered, 0);
                    }
                }
            }
        }


        //------------------------------------------------------------------------------------
        //SKILLS
        //------------------------------------------------------------------------------------
        public void SendChannelingInfo(Entity entity)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MessageType.Casting);
            msgOut.Write(entity.Id, 16);

            SendDataToNeigbours(msgOut.Data, entity.GridCell, NetDeliveryMethod.ReliableSequenced);
        }

        /// <summary>
        /// Sends a SkillInfo to 
        /// the entity's GridCell and it's neighbours. 
        /// Clients will simulate skills entity targeted skills based on this message. 
        /// </summary>
        /// <param name="skill">Skill that has been casted.</param>
        public void SendSkillOnCasted(AbstractSkill skill)
        {
            NetBuffer buffer = new NetBuffer();

            buffer.Write((byte)MessageType.SkillInfo);

            buffer.Write((byte)skill.SkillId);
            buffer.Write((ushort)skill.Caster.Id, 16);

            buffer.Write((ushort)skill.Target.Id, 16);


            SendDataToNeigbours(buffer.Data, skill.Caster.GridCell);
        }

        /// <summary>
        /// Sends SkillInfoAlternativePosition message to 
        /// the entity's GridCell and it's neighbours. 
        /// Clients will be able simulate skills that start position is not from their entity position based on this message.
        /// Skill Like: ChainLightning
        /// </summary>
        /// <param name="skill">Skill that has been casted</param>
        /// <param name="alternativeEntity">The entity the skills starts from.</param>
        public void SendSkillOnCastedAlternativePosition(AbstractSkill skill, Entity alternativeEntity)
        {
            NetOutgoingMessage msgOut;
            foreach (var gridCell in skill.Caster.GridCell.Neighbours)
            {
                if (gridCell != null)
                    foreach (var entity in gridCell.PlayersById.Values)
                    {

                        msgOut = Server.CreateMessage();

                        msgOut.Write((byte)MessageType.SkillInfoAlternativePosition);

                        msgOut.Write((byte)skill.SkillId);
                        msgOut.Write((ushort)skill.Caster.Id, 16);

                        msgOut.Write((ushort)skill.Target.Id, 16);
                        if (alternativeEntity != null)
                            msgOut.Write((ushort)alternativeEntity.Id, 16);
                        else
                            msgOut.Write(skill.Caster.Id);


                        if (entity.Connection != null)
                            entity.Connection.SendMessage(msgOut, NetDeliveryMethod.Unreliable, 0);
                    }
            }
        }

        /// <summary>
        /// Sends damage information to damage dealer, and damage getter entities, if they have a connection.
        /// </summary>
        /// <param name="damageDealer">The entity that is dealing the damage.</param>
        /// <param name="damageGetter">The entity that is getting the damage.</param>
        /// <param name="effectType">The effect type of the damage.</param>
        /// <param name="damageValue">The value of the damage.</param>
        /// <param name="crit">True if the damage is critical damage.</param>
        public void SendDamageInfo(Entity damageDealer, Entity damageGetter, SkillSchool effectType, int damageValue, bool crit = false)
        {
            NetOutgoingMessage msgOut;
            NetBuffer buffer = new NetBuffer();
            buffer.Write((byte)MessageType.DamageInfo);

            buffer.Write(damageGetter.Id);
            buffer.Write(damageValue);
            buffer.Write((byte)effectType);
            buffer.Write(crit);

            if (damageDealer.GetConnection() != null)
            {
                msgOut = Server.CreateMessage();
                msgOut.Write(buffer);
                damageDealer.GetConnection().SendMessage(msgOut, NetDeliveryMethod.Unreliable, 0);
            }
            if (damageGetter.GetConnection() != null)
            {
                msgOut = Server.CreateMessage();
                msgOut.Write(buffer);
                damageGetter.GetConnection().SendMessage(msgOut, NetDeliveryMethod.Unreliable, 0);
            }
        }

        /// <summary>
        /// When player connects this message will send the client's PlayerEntity to the client. 
        /// </summary>
        /// <param name="player">The client's PlayerEntity.</param>
        public void SendPlayerInitializeInfo(PlayerEntity player)
        {
            NetOutgoingMessage msgOut = Server.CreateMessage();

            msgOut.Write((byte)MessageType.PlayerID);
            msgOut.Write(player.Id, 16);

            msgOut.Write(player.Name);

            msgOut.Write(player.Health.MaxValue);
            msgOut.Write(player.Health.Value);

            msgOut.Write(player.Mana.MaxValue);
            msgOut.Write(player.Mana.Value);

            msgOut.Write(player.Level);
            msgOut.Write((byte)player.ModelType);

            msgOut.Write(player.Position.X);
            msgOut.Write(player.Position.Z);

            msgOut.Write(player.MovementSpeed);

            Console.WriteLine(player.Position);

            WriteAttributesToMessage(ref msgOut, player);

            msgOut.Write(player.Experience);
            msgOut.Write(player.MaxExperience);

            msgOut.Write(Data.WORLD_SIZE);

            player.Connection.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered, 0);
        }


        //-----------------------------------------------------
        //Helper functions
        //-----------------------------------------------------

        /// <summary>
        /// Creates and returns a NetBuffer that contains the given entity's respawn information.
        /// </summary>
        /// <param name="entity">The entity that's data is serialized into the NetBuffer.</param>
        /// <returns></returns>
        private NetBuffer CreateMobRespawnPackage(Entity entity)
        {
            NetBuffer buffer = new NetBuffer();
            buffer.Write(entity.Id, 16);
            buffer.Write(entity.Level);
            buffer.Write(entity.Health.MaxValue, 16);

            buffer.Write(TransformFloatToUshort(entity.Position.X), 16);
            buffer.Write(TransformFloatToUshort(entity.Position.Z), 16);

            return buffer;//.Data;
        }

        /// <summary>
        /// Creates and returns a NetBuffer that containst the given entity's full information.
        /// </summary>
        /// <param name="entity">The entity that's data is serialized into the NetBuffer.</param>
        /// <returns></returns>
        private NetBuffer CreateFullEntityUpdatePackage(Entity entity)
        {
            NetBuffer buffer = new NetBuffer();
            buffer.Write(entity.Id, 16);

            buffer.Write(entity.Name);
            buffer.Write(entity.Level);
            buffer.Write((ushort)entity.Health.MaxValue, 16);
            buffer.Write((ushort)entity.Health.Value, 16);


            buffer.Write(TransformFloatToUshort(entity.MovementSpeed), 16);

            buffer.Write(TransformFloatToUshort(entity.Position.X), 16);
            buffer.Write(TransformFloatToUshort(entity.Position.Z), 16);

            buffer.Write(TransformFloatToUshort(entity.EndPosition.X), 16);
            buffer.Write(TransformFloatToUshort(entity.EndPosition.Z), 16);


            buffer.Write((byte)entity.ModelType);
            return buffer;
        }

        /// <summary>
        /// Creates and returns a NetBuffer from the entity and a Buff that is applied.
        /// </summary>
        /// <param name="entity">The entity that has the buff applied.</param>
        /// <param name="buff">The applied buff.</param>
        /// <returns></returns>
        private NetBuffer CreateBuffApplyPackage(Entity entity, Buff buff)
        {
            NetBuffer buffer = new NetBuffer();

            buffer.Write((byte)MessageType.BuffApply);
            buffer.Write(entity.Id);

            buffer.Write((byte)buff.BuffId);
            buffer.Write(buff.ExpirationTick - entity.EntityTick);

            return buffer;//.Data;
        }

        /// <summary>
        /// Creates and returns a NetBuffer from the entity and a Buff that is refreshed.
        /// </summary>
        /// <param name="entity">The entity that has the buff refreshed.</param>
        /// <param name="buff">The refreshed buff.</param>
        /// <returns></returns>
        private NetBuffer CreateBuffRefreshPackage(Entity entity, Buff buff)
        {
            NetBuffer buffer = new NetBuffer();

            buffer.Write((byte)MessageType.BuffRefresh);
            buffer.Write(entity.Id);
            buffer.Write((byte)buff.BuffId);
            buffer.Write(buff.ExpirationTick - entity.EntityTick);

            return buffer;//.Data;
        }

        private NetBuffer CreateBuffRemovePackage(Entity entity, Buff buff)
        {
            NetBuffer buffer = new NetBuffer();

            buffer.Write((byte)MessageType.BuffRemove);

            buffer.Write(entity.Id);
            buffer.Write((byte)buff.BuffId);

            return buffer;//.Data;
        }

        //Writes attributes into given message
        /// <summary>
        /// Writes an entity's attributes into the given message.
        /// </summary>
        /// <param name="msgOut">The message that will be serialized into.</param>
        /// <param name="entity">The entity that attributes will be serialized.</param>
        private void WriteAttributesToMessage(ref NetOutgoingMessage msgOut, Entity entity)
        {
            foreach (var attribute in entity.Attributes)
            {
                msgOut.Write(attribute, 16);
            }
        }

        /// <summary>
        /// Creates a message from the given byte array, and sends it to every online client.
        /// </summary>
        /// <param name="data">The data that needs to be sent.</param>
        /// <param name="netDeliveryMethod">The delivery method of the message.</param>
        private void SendDataToAllPlayers(byte[] data, NetDeliveryMethod netDeliveryMethod = NetDeliveryMethod.Unreliable)
        {
            foreach (var player in Data.PlayersById.Values)
            {
                NetOutgoingMessage msgOut = Server.CreateMessage();
                msgOut.Write(data);

                OverallBytesSent += msgOut.LengthBytes;
                if (player.Connection != null) player.Connection.SendMessage(msgOut, netDeliveryMethod, 0);
            }
        }

        /// <summary>
        /// Creates a message from the given byte array, and sends it to the given GridCell, and its neighbours Players.
        /// </summary>
        /// <param name="data">The data that needs to be sent.</param>
        /// <param name="gridCell">The GridCell which neighbours (including itself) Players needs to get the message.</param>
        /// <param name="netDeliveryMethod">The delivery method of the message-</param>
        private void SendDataToNeigbours(byte[] data, GridCell gridCell, NetDeliveryMethod netDeliveryMethod = NetDeliveryMethod.Unreliable)
        {
            if (gridCell == null) return;

            foreach (var grid in gridCell.Neighbours)
            {
                if (grid != null)
                    foreach (var player in grid.PlayersById.Values)
                    {
                        NetOutgoingMessage msgOut = Server.CreateMessage();
                        msgOut.Write(data);

                        OverallBytesSent += msgOut.LengthBytes;
                        if (player.Connection != null) player.Connection.SendMessage(msgOut, netDeliveryMethod, 0);
                    }
            }
        }

        /// <summary>
        /// Quantizes/Transforms a float into ushort by the SMALLEST_FLOAT_STEP.
        /// </summary>
        /// <param name="val">The float that needs to be quantized/transformed into ushort.</param>
        /// <returns>The quantized/transformed ushort value of the float.</returns>
        private ushort TransformFloatToUshort(float val)
        {
            return (ushort)(val * SMALLEST_FLOAT_STEP);
        }
    }
}
