using AuthoryServer.Entities;
using Lidgren.Network;
using Lidgren.Network.Shared;
using System;
using System.Threading;

namespace AuthoryServer.Server.Handlers
{
    /// <summary>
    /// This handles the incoming messages from the clients.
    /// </summary>
    public class IncomingMessageHandler
    {
        private const float MAXIMUM_ERROR = 3f;
        AuthoryServer Server;
        Thread readerThread;

        /// <summary>
        /// Creates a thread for reading the messages.
        /// </summary>
        /// <param name="server"></param>
        public IncomingMessageHandler(AuthoryServer server)
        {
            Server = server;

            readerThread = new Thread(new ThreadStart(ReadLoop))
            {
                IsBackground = true
            };

            readerThread.Start();
        }

        /// <summary>
        /// Calls the Read() method every 50ms.
        /// </summary>
        private void ReadLoop()
        {
            while (true)
            {
                Read();
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Reads the incoming messages to the MapServer from the clients
        /// Based on NetMessageType it will call the appropiate method for them
        /// For NetIncomingMessageType.Data - HandleMessage()
        /// For NetIncominMessageType.StatusChanged - HandleConnection()
        /// 
        /// At the end of every message it will recycle the message, 
        /// this will remove the load from GarbageCollector
        /// </summary>
        public virtual void Read()
        {
            NetIncomingMessage msgIn;
            MessageType msgType;
            while ((msgIn = Server.NetServer.ReadMessage()) != null)
            {
                if (msgIn.MessageType == NetIncomingMessageType.Data)
                {
                    msgType = (MessageType)msgIn.ReadByte();
                    HandleMessage(msgType, msgIn);
                }
                else if (msgIn.MessageType == NetIncomingMessageType.StatusChanged)
                {
                    HandleConnection(msgIn);
                }
                Server.NetServer.Recycle(msgIn);
            }
        }
        /// <summary>
        /// The message read loop will call this for
        /// NetIncomingMessageType.Data typed NetIncomingMessages
        /// </summary>
        /// <param name="msgType">The MessageType of the message which the client sent !!!NOT THE SAME AS NetIncomingMessageType!!!</param>
        /// <param name="msgIn">The message from the client</param>
        private void HandleMessage(MessageType msgType, NetIncomingMessage msgIn)
        {
            // Based on the NetIncomingMessage RemoteUniqueIdentifier (UID) gets the PlayerEntity from the WorldData
            PlayerEntity entity = Server.Data.Get(msgIn.SenderConnection.RemoteUniqueIdentifier);
            if (entity != null)
            {
                switch (msgType)
                {
                    case MessageType.Respawn:
                        {
                            bool homeOrNearest = msgIn.ReadBoolean();
                            if (entity.isDead)
                            {
                                entity.isDead = false;
                                entity.Respawn();
                            }
                        }
                        break;
                    case MessageType.PlayerMovement:

                        if (!entity.isDead)
                        {
                            Vector3 newPosition = new Vector3(msgIn.ReadFloat(), 0, msgIn.ReadFloat());

                            if (CheckPosition(entity, newPosition))
                            {
                                return;
                            }

                            entity.SetPosition(newPosition);

                            if (entity.IsSkillCasting)
                            {
                                if (Vector3.SqrDistance(entity.ChannelingPosition, entity.Position) > 2f)
                                {
                                    entity.IsSkillCasting = false;
                                }
                            }
                        }
                        break;
                    case MessageType.Interact:
                        {
                            EntityBase interactedEntity = Server.Data.GetInteractedEntity(msgIn.ReadUInt16());
                            interactedEntity.Interact(entity);
                        }
                        break;
                    case MessageType.SkillRequestRawPosition:
                        {
                            Console.WriteLine("RAW SKILL");
                            SkillID skillId = (SkillID)msgIn.ReadByte();
                            Vector3 targetPosition = new Vector3(msgIn.ReadFloat(), 0, msgIn.ReadFloat());

                            if (entity != null)
                            {
                                AbstractSkill skill = SkillFactory.Instance.GetSkill(skillId);

                                skill.Create(entity, targetPosition, Server);

                                if (entity.AddSkill(skill))
                                {
                                    Console.WriteLine("ADDED");
                                }
                                else Console.WriteLine("FAILED TO ADD");
                            }
                        }
                        break;
                    case MessageType.SkillRequest:
                        {
                            Entity target = Server.Data.Get(msgIn.ReadUInt16());
                            SkillID skillId = (SkillID)msgIn.ReadByte();

                            Console.WriteLine("DEFAULT CHECK");
                            if (target != null && entity != null && target.Health.Value > 0 && !entity.IsSkillCasting)
                            {
                                Console.WriteLine("RANGE CHECK...");
                                AbstractSkill skill = SkillFactory.Instance.GetSkill(skillId);

                                if (Vector3.SqrDistance(entity.Position, target.Position) < skill.MaxTargetRange * skill.MaxTargetRange)
                                {
                                    Console.WriteLine("OK");
                                    skill.Create(entity, target, Server);
                                    float tickTime = 1000f / Server.PlayerTickRate;
                                    float lagCompensation = (int)((entity.Connection.AverageRoundtripTime * 1000f) / tickTime);
                                    skill.CastDuration -= (int)lagCompensation;

                                    if (entity.AddSkill(skill))
                                    {
                                        Console.WriteLine("ADDED");
                                    }
                                    else
                                    {
                                        Console.WriteLine("FAILED TO ADD");
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Checks if the client is moving as the allowed behaviour 
        /// Based on the travel distance of the past 1 second 
        /// the Server can check if they are moving with the allowed speed 
        /// DistanceTravelled calculated by the distance between the current and the newPosition
        /// </summary>
        /// <param name="newPosition"></param>
        /// <returns>true if the Distance is more than the MovementSpeed 
        /// false if the Distance is les than the MovementSpeed
        /// </returns>
        private bool CheckPosition(PlayerEntity player, Vector3 newPosition)
        {
            player.DistanceTravelled += Vector3.Distance(player.Position, newPosition);
            if (player.DistanceTravelled > player.MovementSpeed + MAXIMUM_ERROR)
            {
                Server.OutgoingMessageHandler.SendPositionCorrection(player);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called on new client connection 
        /// Approves every connection but drops them immediatly, 
        /// if the client sent the wrong UID. 
        /// UID provided by the MasterServer to the MapServer and the client at the same time! 
        /// If the UID that the client sent is in the AwaitingConnections, then it gets removed from the AwaitingConnections, 
        /// and it will be added to the the WorldData while setting it's connection to the msgIn.SenderConnection
        /// </summary>
        /// <param name="msgIn">The message that indicates the connection start</param>
        private void HandleConnection(NetIncomingMessage msgIn)
        {
            Console.WriteLine("New Conection");
            Console.WriteLine(msgIn.MessageType);

            msgIn.SenderConnection.Approve();
            if (msgIn.SenderConnection.Status == NetConnectionStatus.Connected)
            {
                if (msgIn.SenderConnection.RemoteHailMessage != null)
                {
                    NetIncomingMessage hailMsg = msgIn.SenderConnection.RemoteHailMessage;
                    Console.WriteLine("Remote hail message arrived");

                    long uid = hailMsg.ReadInt64();

                    //PlayerEntity newPlayer = new PlayerEntity(name, ModelType.FemaleHuman, msgIn.SenderConnection);

                    //Checks for AwaitingConnection this gets loaded by the MasterServer
                    if (Server.Data.AwaitingConnections.TryGetValue(uid, out PlayerEntity newPlayer))
                    {
                        Console.WriteLine("Client has the right uid connection aprroved");
                        newPlayer.SetConnection(msgIn.SenderConnection);
                        Server.Data.Add(newPlayer);
                        Server.OutgoingMessageHandler.SendPlayerInitializeInfo(newPlayer);
                        Console.WriteLine("{0}\n Connected from connection: {1}", newPlayer, newPlayer.Connection);


                        Console.WriteLine(newPlayer);

                        if (Server.Data.AwaitingConnections.TryRemove(uid, out PlayerEntity playerEntity))
                        {
                            Console.WriteLine($"{playerEntity} removed from awawiting connections");
                            playerEntity.CalculateResources();
                        }
                        else
                        {
                            Console.WriteLine($"Could not remove entity from AwaitingConnections with UID: {uid}");
                            foreach (var conn in Server.Data.AwaitingConnections)
                            {
                                Console.WriteLine("-------------------------");
                                Console.WriteLine(conn.Key);
                                Console.WriteLine(conn.Value);
                            }
                        }
                        Server.OutgoingMessageHandler.SendFullEntityUpdatesToPlayer(newPlayer);
                        Server.OutgoingMessageHandler.SendFullResourceEntitiesUpdate(newPlayer);
                    }
                }
            }
        }
    }
}
