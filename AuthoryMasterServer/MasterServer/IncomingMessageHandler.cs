using Lidgren.Network;
using Lidgren.Network.Shared;
using System;
using System.Linq;

namespace AuthoryMasterServer
{
    /// <summary>
    /// Handles the incoming messages from the clients and the map servers
    /// </summary>
    public class IncomingMessageHandler
    {

        public DataHandler DataHandler { get; private set; }
        public DatabaseHandler DatabaseHandler { get; private set; }
        public NetServer NetServer { get; private set; }


        public IncomingMessageHandler(DataHandler dataHandler, DatabaseHandler databaseHandler, NetServer server)
        {
            DataHandler = dataHandler;
            DatabaseHandler = databaseHandler;
            NetServer = server;
        }

        /// <summary>
        /// When a character sends a message from the global chat.
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="msgIn"></param>
        public void GlobalChat(MasterMessageType msgType, NetIncomingMessage msgIn)
        {
            Account account = DataHandler.GetAccount(msgIn.SenderConnection);
            string messageContent = msgIn.ReadString();

            foreach (var map in DataHandler.Maps)
            {
                foreach (var mapServer in map.Value.OnlineChannels)
                {
                    mapServer.SendMessageToConnectedPlayers(account, msgType, messageContent);
                }
            }
        }

        /// <summary>
        /// When a character sends a message to the world chat.
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="msgIn"></param>
        public void WorldChat(MasterMessageType msgType, NetIncomingMessage msgIn)
        {
            Account account = DataHandler.GetAccount(msgIn.SenderConnection);
            string messageContent = msgIn.ReadString();

            if (account.ConnectedServerMap != null)
                account.ConnectedServerMap.SendMessageToConnectedPlayers(account, msgType, messageContent);
        }

        /// <summary>
        /// When a client sends a message to another client.
        /// </summary>
        /// <param name="msgIn"></param>
        public void PrivateChatMessage(NetIncomingMessage msgIn)
        {
            Account account = DataHandler.GetAccount(msgIn.SenderConnection);
            string messageContent = msgIn.ReadString();
            string receiverName = msgIn.ReadString();

            Account receiver = DataHandler.GetAccountByCharacterName(receiverName);

            if (receiver != null)
            {
                OutgoingMessageHandler.Instance.SendPrivateChatMessage(account, messageContent, receiver);
            }
            else Console.WriteLine("Receiver is null");
            if (account != null)
            {
                OutgoingMessageHandler.Instance.SendPrivateChatMessage(account, messageContent, account);
            }
        }

        /// <summary>
        /// Sends server connection request requested by the account, to the node server that is running the requested characters mapindex
        /// </summary>
        /// <param name="msgIn"></param>
        public void ServerConnectionRequest(NetIncomingMessage msgIn)
        {
            int requestedCharacterId = msgIn.ReadInt32();

            AuthoryMapServer channel;
            Account account = DataHandler.GetAccount(msgIn.SenderConnection);
            Character character = account.GetCharacter(requestedCharacterId);
            AuthoryMap requestedMap = DataHandler.GetMap(character.MapIndex);

            account.MapIndex = character.MapIndex;


            if (account == null)
            {
                Console.WriteLine($"Online Account with connection({msgIn.SenderConnection}) not found");
                return;
            }
            Console.WriteLine(account);


            if (requestedMap == null)
            {
                Console.WriteLine($"Map with index({character.MapIndex}) not found");
                return;
            }

            channel = requestedMap.GetLeastLoadedChannel();
            Console.WriteLine(channel.ToString());
            if (channel == null)
            {
                Console.WriteLine($"No online map servers!");
                foreach (var map in DataHandler.Maps.Values)
                {
                    foreach (var mapServer in map.OnlineChannels)
                    {
                        Console.WriteLine(mapServer);
                    }
                }
                return;
            }



            if (character == null)
            {
                Console.WriteLine($"Character with id({requestedCharacterId}) not found");
                return;
            }

            channel.SendNewCharacterInfo(character);
        }

        /// <summary>
        /// Creates a character and inserts it into the database.
        /// </summary>
        /// <param name="msgIn"></param>
        public void CreateCharacter(NetIncomingMessage msgIn)
        {
            Account account = DataHandler.GetAccount(msgIn.SenderConnection);

            string name = msgIn.ReadString();
            byte modelType = msgIn.ReadByte();

            int characterId = DatabaseHandler.CreateCharacter(account.AccountId, name, modelType);

            if (characterId < 0)
            {
                OutgoingMessageHandler.Instance.SendInfo(account, SystemMessageType.InvalidCharacterName);
            }
            else
            {
                Character character = new Character(account, characterId, name, positionX: 450f, positionZ: 450f, modelType, level: 1, experience: 0, mapIndex: 0);
                account.Characters.Add(character);

                Console.WriteLine($"New Character: {character}");

                OutgoingMessageHandler.Instance.SendCharacterListRefresh(account);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Removes the requested character from the database.
        /// </summary>
        /// <param name="msgIn"></param>
        public void DeleteCharacter(NetIncomingMessage msgIn)
        {
            int characterId = msgIn.ReadInt32();
            string characterName = msgIn.ReadString();

            Account account = DataHandler.GetAccount(msgIn.SenderConnection);

            account.Characters.RemoveAll(x => x.CharacterId == characterId);

            DatabaseHandler.DeleteCharacter(account.AccountId, characterId, characterName);

            OutgoingMessageHandler.Instance.SendCharacterListRefresh(account);
        }

        /// <summary>
        /// If the map server approved the connection of the sent character. The server will send the map server information to the connected client.
        /// </summary>
        /// <param name="msgIn"></param>
        public void ConnectionApproved(NetIncomingMessage msgIn)
        {
            int port = msgIn.ReadInt32();
            int accountId = msgIn.ReadInt32();
            int characterId = msgIn.ReadInt32();
            long uid = msgIn.ReadInt64();
            ushort characterServerId = msgIn.ReadUInt16();
            Console.WriteLine("Connection Approved at uid: " + uid);

            Account account = DataHandler.GetAccount(accountId);
            account.SetConnectedCharacter(characterId);

            account.ConnectionApproved = true;

            AuthoryNode node = DataHandler.GetNode(msgIn.SenderConnection);
            if (node == null)
            {
                Console.WriteLine($"Node not found with Connection({msgIn.SenderConnection})");
                return;
            }
            AuthoryMapServer server = node.GetMapServerByPort(port);

            if (server == null)
            {
                Console.WriteLine($"Server not found with Port({port})");
                return;
            }

            account.ConnectedServerMap = server;

            if (server.GetCharacter(characterId) == null)
                server.OnlineCharacters.Add(account.ConnectedCharacter);

            OutgoingMessageHandler.Instance.SendConnectionApproved(account, server);
        }

        /// <summary>
        /// When a node creates a map server.
        /// </summary>
        /// <param name="msgIn"></param>
        public void MapCreated(NetIncomingMessage msgIn)
        {
            AuthoryNode node = DataHandler.GetNode(msgIn.SenderConnection);
            int mapPort = msgIn.ReadInt32();

            int mapIndex = msgIn.ReadInt32();
            string mapName = msgIn.ReadString();


            Console.WriteLine("Created map port: " + mapPort);
            Console.WriteLine("MAP NAME: " + mapName);
            Console.WriteLine("MAP IDNEX: " + mapIndex);

            AuthoryMapServer map = new AuthoryMapServer(node, mapPort, DataHandler.Maps.Values.Single(x => x.MapIndex == mapIndex));

            node.AddMapServer(map);
            DataHandler.GetMap(mapIndex).OnlineChannels.Add(map);


            Console.WriteLine("Map added:");
            Console.WriteLine(map);
        }

        /// <summary>
        /// When a map server closes on a node.
        /// </summary>
        /// <param name="msgIn"></param>
        public void MapsRemoved(NetIncomingMessage msgIn)
        {
            AuthoryNode node = DataHandler.GetNode(msgIn.SenderConnection);


            int msgInfoCount = msgIn.ReadInt32();
            for (int i = 0; i < msgInfoCount; i++)
            {
                int mapPort = msgIn.ReadInt32();

                int mapIndex = msgIn.ReadInt32();
                string mapName = msgIn.ReadString();

                node.RemoveMapServer(mapPort, mapIndex, mapName);
            }
        }

        /// <summary>
        /// When the requested character info arrived from the map server's node.
        /// </summary>
        /// <param name="msgIn"></param>
        public void RequestCharacterInfoArrived(NetIncomingMessage msgIn)
        {
            Character character = ReadCharacterDataFromMessage(msgIn);
            DatabaseHandler.UpdateCharacter(character);

            int cnt = DataHandler.OnlineAccounts.RemoveAll(x => (x.AccountId == character.Account.AccountId && x.Connection.Status == NetConnectionStatus.Disconnected));
            Console.WriteLine($"Removed accounts: {cnt}");
        }

        /// <summary>
        /// When a map server requests a map change for its connected client.
        /// </summary>
        /// <param name="msgIn"></param>
        public void MapChangeRequest(NetIncomingMessage msgIn)
        {
            Character character;
            AuthoryMap map;
            AuthoryMapServer serverChannel;


            int requestedMapIndex = msgIn.ReadInt32();
            int currentMapIndex = msgIn.ReadInt32();

            character = ReadCharacterDataFromMessage(msgIn);
            character.Account.ConnectedServerMap.OnlineCharacters.RemoveAll(x => x.CharacterId == character.CharacterId);


            map = DataHandler.GetMap(requestedMapIndex);

            if (map == null)
            {
                Console.WriteLine("No maps found with map index: " + requestedMapIndex);
                return;
            }

            serverChannel = map.GetLeastLoadedChannel();

            if (serverChannel == null)
            {
                Console.WriteLine("No running map server channel on map index: " + requestedMapIndex);
                return;
            }

            character.MapIndex = requestedMapIndex;
            DatabaseHandler.UpdateCharacter(character);

            if (requestedMapIndex != currentMapIndex)
            {
                Console.WriteLine("Sending new character info for teleport...");
                serverChannel.SendNewCharacterInfo(character);
            }
            else
            {
                Console.WriteLine("Teleport requested to the same map!");
            }
        }

        /// <summary>
        /// Nodes will report their map servers load every X seconds.
        /// </summary>
        /// <param name="msgIn"></param>
        public void LoadReport(NetIncomingMessage msgIn)
        {
            AuthoryNode node = DataHandler.GetNode(msgIn.SenderConnection);

            int count = msgIn.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                AuthoryMapServer channel = node.GetMapServerByPort(msgIn.ReadInt32());

                int channelLoad = msgIn.ReadInt32();

                channel.Load = channelLoad;
            }
        }

        /// <summary>
        /// When a new client connects to the map server.
        /// </summary>
        /// <param name="hailMessage"></param>
        /// <param name="msgIn"></param>
        public void NewAccountConnection(NetIncomingMessage hailMessage, NetIncomingMessage msgIn)
        {
            string username = hailMessage.ReadString();
            string password = hailMessage.ReadString();
            Console.WriteLine(username);

            Account onlineAccount = DataHandler.GetAccount(username);

            if (onlineAccount != null)
            {
                if (onlineAccount.ConnectedServerMap != null && onlineAccount.ConnectedCharacter != null)
                {
                    OutgoingMessageHandler.Instance.RequestCharacterInfo(onlineAccount.ConnectedCharacter, onlineAccount.ConnectedServerMap);
                    Console.WriteLine("Account logged in...");
                }
                DataHandler.OnlineAccounts.Remove(onlineAccount);
                Console.WriteLine("Account still online.");
                return;
            }

            Account account = DatabaseHandler.ReadAccount(username, password);
            if (account == null)
            {
                Console.WriteLine("Account not registered\nRegistering account");
                int id = DatabaseHandler.CreateAccount(username, password);
                if (id > 0)
                {
                    account = new Account(id, username, msgIn.SenderConnection);
                }
            }

            if (account != null)
            {
                account.Connection = msgIn.SenderConnection;
                account.Characters = DatabaseHandler.ReadCharactersOfAccount(account);
                DataHandler.OnlineAccounts.Add(account);

                OutgoingMessageHandler.Instance.SendNewConnectionMessage(account);
            }
            else
            {
                msgIn.SenderConnection.Disconnect("Bad credentials");
            }
        }


        /// <summary>
        /// When a new Node server connects.
        /// </summary>
        /// <param name="msgIn"></param>
        /// <param name="hailMessage"></param>
        public void NewServerNodeConnection(NetIncomingMessage msgIn, NetIncomingMessage hailMessage)
        {
            Console.WriteLine("New node connection...");
            string ip = hailMessage.ReadString();
            int port = msgIn.SenderConnection.RemoteEndPoint.Port;

            int latestPort = DataHandler.FindLatestPort();
            Console.WriteLine($"Latest port: {latestPort}");
            latestPort++;

            Console.WriteLine($"Latest port: {latestPort}");

            Console.WriteLine($"Connection: {ip}:{port}");
            AuthoryNode newNode = new AuthoryNode(msgIn.SenderConnection, ip, port);
            Console.WriteLine($"New node created:");
            Console.WriteLine(newNode);

            if (DataHandler.Nodes.Count == 0)
            {
                OutgoingMessageHandler.Instance.SendMapsRequest(newNode.NodeMasterConnection, DataHandler.Maps.Values.ToArray(), latestPort);
                Console.WriteLine($"Current node count {DataHandler.Nodes.Count} requesting all maps from new node");

                Console.WriteLine(newNode);
                DataHandler.ListNodes();
            }
            else
            {
                AuthoryNode overloadedNode = DataHandler.GetOverloadedNode();
                AuthoryMapServer overloadedMap = overloadedNode.GetOverloadedMap();
                if (overloadedMap == null)
                {
                    DataHandler.ListNodes();

                    throw new Exception("There aren't any nodes at the moment!");
                }
                else
                {
                    OutgoingMessageHandler.Instance.SendMapsRequest(newNode.NodeMasterConnection, overloadedMap.AuthoryMap, latestPort);
                    Console.WriteLine("Map request sent...");
                }
            }

            DataHandler.AddNode(newNode);
            Console.WriteLine($"New node added to Online Nodes.");
            Console.WriteLine();
        }

        /// <summary>
        /// Reads character data from message.
        /// </summary>
        /// <param name="msgIn"></param>
        /// <returns></returns>
        public Character ReadCharacterDataFromMessage(NetIncomingMessage msgIn)
        {
            int characterId = msgIn.ReadInt32();
            int accountId = msgIn.ReadInt32();
            Account account = DataHandler.GetAccount(accountId);
            Character character = account.Characters.Find(x => x.CharacterId == characterId);

            character.Name = msgIn.ReadString();
            character.Experience = msgIn.ReadInt64();
            character.Level = msgIn.ReadByte();
            character.ModelType = msgIn.ReadByte();
            character.PositionX = msgIn.ReadFloat();
            character.PositionZ = msgIn.ReadFloat();
            character.Health = msgIn.ReadInt32();
            character.Mana = msgIn.ReadInt32();

            return character;
        }
    }
}