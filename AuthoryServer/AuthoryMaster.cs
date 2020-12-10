using System;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using Lidgren.Network.Shared;
using AuthoryServer.Entities;
using System.IO;
using System.Linq;

namespace AuthoryServer
{
    public class AuthoryMaster
    {
        /// <summary>
        /// Local map servers that are running under this ServerNode
        /// </summary>
        public List<AuthoryServer> MapServers { get; private set; }

        private const int NODE_TICK_RATE = 50;
        private const int MASTER_SERVER_CONNECTION_DELAY = 500;

        private NetPeerConfiguration config;
        private NetClient masterClient;

        private Thread masterThread;


        private string defaultConfigFilePath = "";
        private string defaultConfigFileName = "config.cfg";

        //The connection has to have the same auth string on the client and server side.
        private string defaultAuthoryServerAuthString = "AuthoryServer";
        long master_tick = 0;
        bool nodeUp = true;
        public bool NodeUp { get { return nodeUp; } }

        ///If false the server will get the outer IP of the network by URL call.
        private bool isServerRunningLocalhost = true;
        //Default port where the node will start opening the ports for the MapServers
        private int defaultServerPort = 0;
        //The folder where the config files for the NPCs found.
        private string defaultMapsFolderPath = "";

        //The connetion has to have the same auth string on the Node(this) and the MasterServer side.
        private string defaultMasterServerAuthString = "AuthoryMasterServer";
        //The IP adress where the master server running.
        private string defaultMasterServerHost="";
        //The port where the MasterServer is available.
        private int defaultMasterServerPort;


        private static AuthoryMaster _instance;
        public static AuthoryMaster Instance => _instance;


        /// <summary>
        /// Creates an AuthoryMaster instance
        /// </summary>
        /// <param name="ip">MasterServer IP address</param>
        /// <param name="port">MasterServer port</param>
        /// <param name="connName">The MasterServer will be notified from connections by this string</param>
        public AuthoryMaster()
        {
            LoadConfig();
            MapServers = new List<AuthoryServer>();

            CreateMaster(defaultMasterServerAuthString);

            _instance = this;
        }

        /// <summary>
        ///Read messages from MasterServer
        /// </summary>
        private void ReadMaster()
        {
            NetIncomingMessage msgIn;
            MasterMessageType msgType;

            while ((msgIn = masterClient.ReadMessage()) != null)
            {
                if (msgIn.MessageType == NetIncomingMessageType.Data)
                {
                    msgType = (MasterMessageType)msgIn.ReadByte();
                    Console.WriteLine(msgType.ToString());
                    switch (msgType)
                    {
                        case MasterMessageType.RequestMap:
                            {
                                int latestPort = msgIn.ReadInt32();


                                if (latestPort < 1000)
                                    latestPort = (int)defaultServerPort++;

                                int mapIndex = msgIn.ReadInt32();
                                string mapName = msgIn.ReadString();
                                //int serverId = msgIn.ReadInt32();

                                AuthoryServer server = new AuthoryServer(MapServers.Count, defaultMapsFolderPath);
                                MapServers.Add(server);

                                Console.WriteLine("Requested port: " + latestPort);


                                server.Init(latestPort++, defaultAuthoryServerAuthString, mapName, mapIndex);
                                server.Start();



                                NetOutgoingMessage msgOut = masterClient.CreateMessage();

                                msgOut.Write((byte)MasterMessageType.MapCreated);

                                msgOut.Write(server.NetServer.Port);
                                msgOut.Write(server.MapIndex);
                                msgOut.Write(server.MapName);

                                masterClient.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);

                                nodeUp = true;
                            }

                            break;
                        case MasterMessageType.RequestMaps:
                            {
                                int latestPort = msgIn.ReadInt32();

                                int infoCount = msgIn.ReadInt32();
                                for (int i = 0; i < infoCount; i++)
                                {
                                    int mapIndex = msgIn.ReadInt32();
                                    string mapName = msgIn.ReadString();

                                    AuthoryServer server = new AuthoryServer(MapServers.Count, defaultMapsFolderPath);
                                    MapServers.Add(server);

                                    if (latestPort < 1000)
                                        latestPort = (int)defaultServerPort++;

                                    Console.WriteLine("Requested port: " + latestPort);

                                    server.Init(latestPort++, defaultAuthoryServerAuthString, mapName, mapIndex);
                                    server.Start();


                                    NetOutgoingMessage msgOut = masterClient.CreateMessage();

                                    msgOut.Write((byte)MasterMessageType.MapCreated);

                                    msgOut.Write(server.NetServer.Port);
                                    msgOut.Write(server.MapIndex);
                                    msgOut.Write(server.MapName);

                                    masterClient.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
                                }
                                nodeUp = true;
                            }
                            break;
                        case MasterMessageType.CharacterInfo:
                            {
                                int serverPort = msgIn.ReadInt32();

                                AuthoryServer mapServer = null;
                                foreach (var server in MapServers)
                                {
                                    if (server.NetServer.Port == serverPort)
                                    {
                                        mapServer = server;
                                    }
                                }

                                string accountName = msgIn.ReadString();
                                string characterName = msgIn.ReadString();

                                int accountId = msgIn.ReadInt32();
                                int characterId = msgIn.ReadInt32();

                                int health = msgIn.ReadInt32();
                                int mana = msgIn.ReadInt32();

                                long experience = msgIn.ReadInt64();
                                byte level = msgIn.ReadByte();
                                ModelType modelType = (ModelType)msgIn.ReadByte();
                                float posX = msgIn.ReadFloat();
                                float posZ = msgIn.ReadFloat();

                                long uid = msgIn.ReadInt64();

                                PlayerEntity playerEntity = new PlayerEntity(characterName, modelType, level, new Vector3(posX, 0, posZ), null, mapServer, characterId, accountId);
                                playerEntity.Experience = experience;
                                playerEntity.Health.Value = health;
                                playerEntity.Mana.Value = mana;

                                Console.WriteLine(playerEntity);

                                playerEntity.SetId((ushort)(mapServer.Data.GetNextPlayerId()));

                                mapServer.Data.AwaitingConnections.TryAdd(uid, playerEntity);

                                NetOutgoingMessage msgOut = masterClient.CreateMessage();

                                msgOut.Write((byte)MasterMessageType.ConnectionApproved);
                                msgOut.Write(serverPort);
                                msgOut.Write(accountId);
                                msgOut.Write(characterId);
                                msgOut.Write(uid);
                                msgOut.Write(playerEntity.Id);


                                masterClient.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
                            }
                            break;
                        case MasterMessageType.RequestCharacterInfo:
                            {
                                int port = msgIn.ReadInt32();
                                int characterId = msgIn.ReadInt32();

                                foreach (var server in MapServers)
                                {
                                    foreach (var player in server.Data.PlayersById.Values)
                                    {
                                        if (player.CharacterId == characterId)
                                        {
                                            SendBackCharacterInfo(server, player);
                                            return;
                                        }
                                    }
                                    if (server.NetServer.Port == port)
                                    {
                                        if (server.Data.RecentlyOnlinePlayers.TryRemove(characterId, out PlayerEntity player))
                                        {
                                            SendBackCharacterInfo(server, player);
                                            return;
                                        }
                                    }
                                }
                            }
                            break;
                        case MasterMessageType.Shutdown:
                            {
                                Shutdown();
                            }
                            break;
                    }
                }
            }
        }

        public void Shutdown()
        {
            foreach (var server in MapServers)
            {
                foreach (var character in server.Data.PlayersById.Values)
                {
                    SendBackCharacterInfo(server, character);
                    server.Data.RemovePlayer(character);
                }
                server.Shutdown();

            }
            nodeUp = false;
        }

        /// <summary>
        /// Sends PlayerEntity info to master server for database save
        /// </summary>
        /// <param name="server">The MapServer where the player is</param>
        /// <param name="character">The player which information will be sent</param>
        private void SendBackCharacterInfo(AuthoryServer server, PlayerEntity character)
        {
            NetOutgoingMessage msgOut = masterClient.CreateMessage();

            msgOut.Write((byte)MasterMessageType.RequestCharacterInfo);
            msgOut.Write(CreateCharacterPacket(character));

            masterClient.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Sends a message to MasterServer from character request to transfer to the specific map.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="character"></param>
        private void SendMapChangeRequest(AuthoryServer server, PlayerEntity character, int requestedMapIndex)
        {
            NetOutgoingMessage msgOut = masterClient.CreateMessage();

            msgOut.Write((byte)MasterMessageType.MapChangeRequest);

            msgOut.Write(requestedMapIndex);

            msgOut.Write(server.MapIndex);
            msgOut.Write(CreateCharacterPacket(character));

            masterClient.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Creates a packet from the given entity, that the MasterServer will be able to save.
        /// </summary>
        /// <param name="character">The serialized character</param>
        /// <returns>Returns a NetBuffer that contains the serialized data of the character</returns>
        private NetBuffer CreateCharacterPacket(PlayerEntity character)
        {
            NetBuffer buffer = new NetBuffer();

            buffer.Write(character.CharacterId);
            buffer.Write(character.AccountId);
            buffer.Write(character.Name);
            buffer.Write(character.Experience);
            buffer.Write(character.Level);
            buffer.Write((byte)character.ModelType);
            buffer.Write(character.Position.X);
            buffer.Write(character.Position.Z);

            buffer.Write(character.Health.Value);
            buffer.Write(character.Mana.Value);

            return buffer;
        }

        /// <summary>
        /// Creates the master client(peer) for network communication with MasterServer
        /// </summary>
        /// <param name="authString"></param>
        private void CreateMaster(string authString)
        {
            config = new NetPeerConfiguration(authString);
            masterClient = new NetClient(config);
            masterClient.Start();

            masterThread = new Thread(new ThreadStart(ReadMasterLoop))
            {
                IsBackground = true
            };
            masterThread.Start();
        }

        /// <summary>
        /// Connects to MasterServer
        /// </summary>
        /// <param name="ip">The IP address where the MasterServer is running</param>
        /// <param name="port">The port where the MasterServer is running</param>
        private void ConnectToMaster(string ip, int port)
        {
            NetOutgoingMessage hailMessage = masterClient.CreateMessage();

            hailMessage.Write((byte)MasterMessageType.NewNodeConnection);

            if (isServerRunningLocalhost)
            {
                string localIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.ToList().Find(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
                hailMessage.Write(localIP);
            }
            else
            {
                //Gets back the public IP address of the network where this node is running (the link will only give back an IP address as a string)
                string externalip = new WebClient().DownloadString("https://api.ipify.org/");
                hailMessage.Write(externalip);
            }
            Console.WriteLine($"Attempting connection to Master at {defaultMasterServerHost}:{defaultMasterServerPort}");

            masterClient.Connect(ip, port, hailMessage);
        }


        /// <summary>
        /// The loop where this nodes calls update functions
        /// </summary>
        private void ReadMasterLoop()
        {
            while (nodeUp)
            {
                if (masterClient.ServerConnection != null)
                {
                    ReadMaster();
                    if (master_tick % 50 == 0)
                    {
                        ReportLoad();
                    }
                    Thread.Sleep(NODE_TICK_RATE);
                }
                else
                {
                    ConnectToMaster(defaultMasterServerHost, defaultMasterServerPort);

                    Thread.Sleep(MASTER_SERVER_CONNECTION_DELAY);
                }
                master_tick++;
            }
        }

        private void ReportLoad()
        {
            NetOutgoingMessage msgOut = masterClient.CreateMessage();

            msgOut.Write((byte)MasterMessageType.LoadReport);
            msgOut.Write(MapServers.Count);

            foreach (var mapServer in MapServers)
            {
                msgOut.Write(mapServer.NetServer.Port);
                msgOut.Write(mapServer.Data.PlayersById.Count);
            }

            masterClient.SendMessage(msgOut, NetDeliveryMethod.ReliableSequenced);
        }

        public void ChangeMapServer(PlayerEntity player, AuthoryServer fromServer, int teleportToMapIndex)
        {
            fromServer.Data.FullRemovePlayer(player);
            SendMapChangeRequest(fromServer, player, teleportToMapIndex);
        }

        /// <summary>
        /// Loads the MasterServer configuration from the config file. 
        /// If the file is not found it will create one with the help of the user.
        /// </summary>
        private void LoadConfig()
        {
            //Console.Write("Enter path of config file (Press enter if you want to use default path): ");
            //string tempPath = Console.ReadLine();
            //if (tempPath.Length > 0)
            //{
            //    defaultFilePath = tempPath;
            //    Console.WriteLine("Default path");
            //}

            string path = defaultConfigFilePath + defaultConfigFileName;
            Console.WriteLine($"Reading config file from: \"{path}\"...");
            if (File.Exists(path))
            {
                Console.WriteLine("Config file found.");
                string[] configs = File.ReadAllLines(path);

                foreach (var config in configs)
                {
                    if (config.Length > 0 && config[0] != '#')
                    {
                        string[] configDetails = config.Trim().Split('=');
                        switch (configDetails[0].ToLower())
                        {

                            case "authoryserverauthstring":
                                defaultAuthoryServerAuthString = configDetails[1];
                                break;
                            case "runninglocalhost":
                                isServerRunningLocalhost = bool.Parse(configDetails[1]);
                                break;
                            case "serverport":
                                defaultServerPort = int.Parse(configDetails[1]);
                                break;
                            case "mapsfolderpath":
                                defaultMapsFolderPath = configDetails[1];
                                break;
                            case "masterserverauthstring":
                                defaultMasterServerAuthString = configDetails[1];
                                break;
                            case "masterserverhost":
                                defaultMasterServerHost = configDetails[1];
                                break;
                            case "masterserverport":
                                defaultMasterServerPort = int.Parse(configDetails[1]);
                                break;
                        }
                    }
                }

                Console.WriteLine("Config file read: OK.");
            }
            else
            {
                try
                {
                    Console.WriteLine("Config file not found.");

                    Console.WriteLine("Setting up server configuration...");

                    if (defaultMapsFolderPath.Length == 0)
                    {
                        Console.Write("Please enter Maps folder path: ");
                        defaultMapsFolderPath = Console.ReadLine();
                    }

                    if (defaultAuthoryServerAuthString.Length == 0)
                    {
                        Console.Write("Please enter the Server Auth String: ");
                        defaultAuthoryServerAuthString = Console.ReadLine();
                    }

                    Console.WriteLine("Will the MasterServer run on the same network(localhost)? (Y/y for yes)");
                    isServerRunningLocalhost = (Console.ReadLine()?.Trim().ToLower()[0] == 'y' ? true : false);

                    if (defaultServerPort == 0)
                    {
                        Console.Write("Please enter Server Port: ");
                        defaultServerPort = int.Parse(Console.ReadLine());
                    }

                    if (defaultMasterServerAuthString.Length == 0)
                    {
                        Console.Write("Please enter MasterServer Auth String: ");
                        defaultMasterServerHost = Console.ReadLine();
                    }

                    if (defaultMasterServerHost.Length == 0)
                    {
                        Console.Write("Please enter MasterServer Host: ");
                        defaultMasterServerHost = Console.ReadLine();
                    }

                    if (defaultMasterServerPort == 0)
                    {
                        Console.Write("Please enter MasterServer Port: ");
                        defaultMasterServerPort = int.Parse(Console.ReadLine());
                    }

                    Console.WriteLine("Save configuration? (Y/y for yes)");
                    if (Console.ReadLine()?.Trim().ToLower()[0] == 'y')
                    {
                        Console.WriteLine("Saving server configuration...");

                        string[] defaultContent = new string[1] { "" +
                            "#Server details:"+
                            $"\nMapsFolderPath={defaultMapsFolderPath}" +
                            $"\nServerAuthString={defaultAuthoryServerAuthString}"+
                            $"\nServerRunningLocalHost={isServerRunningLocalhost}"+
                            $"\nServerPort={defaultServerPort}"+
                            "\n#MasterServer details:" +
                            $"\nMasterServerAuthString={defaultMasterServerAuthString}" +
                            $"\nMasterServerHost={defaultMasterServerHost}" +
                            $"\nMasterServerPort={defaultMasterServerPort}"
                            };

                        File.WriteAllLines(path, defaultContent);

                        Console.WriteLine("Server configuration saved.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    LoadConfig();
                }
            }
        }
    }
}
