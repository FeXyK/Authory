using Lidgren.Network;
using Lidgren.Network.Shared;
using System;
using System.IO;
using System.Timers;

namespace AuthoryMasterServer
{
    /// <summary>
    /// Instance of the MasterServer.
    /// </summary>
    public class AuthoryMasterServer
    {
        public NetServer NetServer { get; private set; }
        public DatabaseHandler DatabaseHandler { get; set; }
        public DataHandler DataHandler { get; private set; }
        public OutgoingMessageHandler MessageHandler { get; private set; }
        public IncomingMessageHandler IncomingMessageHandler { get; private set; }

        private NetPeerConfiguration netPeerConfig;

        private string dbServer = "";
        private string dbName = "";
        private string dbUser = "";
        private string dbPassword = "";

        private string defaultServerAuthString = "";
        private int defaultServerPort = 55500;

        private string defaultFilePath = "";
        private string defaultFileName = "server_config.cfg";

        private Timer timer;
        private long tick = 0;

        public AuthoryMasterServer()
        {
            LoadConfig();

            Console.WriteLine($"Creating config on port: {defaultServerPort}...");
            netPeerConfig = new NetPeerConfiguration(defaultServerAuthString);
            netPeerConfig.SetMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval, true);
            netPeerConfig.PingInterval = 4f;
            netPeerConfig.ConnectionTimeout = 6.5f;
            netPeerConfig.Port = (int)defaultServerPort;
            Console.WriteLine("Config created.");

            Console.WriteLine("Creating NetServer...");
            NetServer = new NetServer(netPeerConfig);
            Console.WriteLine($"NetServer created on port: {defaultServerPort}.");

            DatabaseHandler = new DatabaseHandler(dbServer, dbName, dbUser, dbPassword);
            DataHandler = DataHandler.Instance;
            MessageHandler = OutgoingMessageHandler.Instance.Init(NetServer);

            IncomingMessageHandler = new IncomingMessageHandler(DataHandler, DatabaseHandler, NetServer);

            Console.WriteLine("Lodaing maps...");
            LoadMaps();
            Console.WriteLine("Maps loaded.");
        }

        /// <summary>
        /// Starts the server timer and the NetServer.
        /// </summary>
        /// <param name="period"></param>
        public void Start(int period = 100)
        {
            NetServer.Start();

            timer = new Timer(100);
            timer.Elapsed += new ElapsedEventHandler(TimerCallback);
            timer.Start();
        }

        /// <summary>
        /// Timer calls this.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerCallback(object sender, ElapsedEventArgs e)
        {
            CheckNodeConnections();
            Read();
            if (tick % 50 == 0)
                SearchDisconnection();
            tick++;
        }

        /// <summary>
        /// Every timercall calls it. 
        /// Reads the arrived messages from the NetServer.
        /// </summary>
        public void Read()
        {
            NetIncomingMessage msgIn;
            MasterMessageType msgType;
            while ((msgIn = NetServer.ReadMessage()) != null)
            {
                if (msgIn.MessageType == NetIncomingMessageType.Data)
                {
                    msgType = (MasterMessageType)msgIn.ReadByte();
                    HandleMessage(msgType, msgIn);
                }
                else if (msgIn.MessageType == NetIncomingMessageType.StatusChanged)
                {
                    HandleConnection(msgIn);
                }
                NetServer.Recycle(msgIn);
            }
        }

        /// <summary>
        /// Handles a given NetIncomingMessage by its MasterMessageType
        /// </summary>
        /// <param name="msgType">Type of the message to be handled.</param>
        /// <param name="msgIn">The arrived Message</param>
        private void HandleMessage(MasterMessageType msgType, NetIncomingMessage msgIn)
        {
            switch (msgType)
            {

                //Client
                case MasterMessageType.GlobalChat:
                    IncomingMessageHandler.GlobalChat(msgType, msgIn);
                    break;
                //Client
                case MasterMessageType.WorldChat:
                    IncomingMessageHandler.WorldChat(msgType, msgIn);
                    break;
                //Client
                case MasterMessageType.PrivateChat:
                    IncomingMessageHandler.PrivateChatMessage(msgIn);
                    break;
                //Client
                case MasterMessageType.ServerConnectionRequest:
                    IncomingMessageHandler.ServerConnectionRequest(msgIn);
                    break;
                //MapServer
                case MasterMessageType.RequestCharacterInfo:
                    IncomingMessageHandler.RequestCharacterInfoArrived(msgIn);
                    break;
                //MapServer
                case MasterMessageType.MapChangeRequest:
                    IncomingMessageHandler.MapChangeRequest(msgIn);
                    break;
                //Client
                case MasterMessageType.CreateCharacter:
                    IncomingMessageHandler.CreateCharacter(msgIn);
                    break;
                //Client
                case MasterMessageType.DeleteCharacter:
                    IncomingMessageHandler.DeleteCharacter(msgIn);
                    break;
                //MapServer
                case MasterMessageType.ConnectionApproved:
                    IncomingMessageHandler.ConnectionApproved(msgIn);
                    break;
                //MapServer
                case MasterMessageType.MapCreated:
                    IncomingMessageHandler.MapCreated(msgIn);
                    break;
                //MapServer
                case MasterMessageType.MapsRemoved:
                    IncomingMessageHandler.MapsRemoved(msgIn);
                    break;
                case MasterMessageType.LoadReport:
                    IncomingMessageHandler.LoadReport(msgIn);
                    break;
            }
        }

        /// <summary>
        /// Handles a new connection to the server.
        /// </summary>
        /// <param name="msgIn">Message that changed the connection status</param>
        private void HandleConnection(NetIncomingMessage msgIn)
        {
            Console.WriteLine("New Conection");
            Console.WriteLine(msgIn.MessageType);

            msgIn.SenderConnection.Approve();
            if (msgIn.SenderConnection.Status == NetConnectionStatus.Connected)
            {
                if (msgIn.SenderConnection.RemoteHailMessage != null)
                {
                    Console.WriteLine("Remote hail message arrived");
                    NetIncomingMessage hailMessage = msgIn.SenderConnection.RemoteHailMessage;
                    MasterMessageType msgType = (MasterMessageType)hailMessage.ReadByte();

                    switch (msgType)
                    {
                        case MasterMessageType.NewNodeConnection:
                            IncomingMessageHandler.NewServerNodeConnection(msgIn, hailMessage);
                            break;
                        case MasterMessageType.NewAccountConnection:
                            IncomingMessageHandler.NewAccountConnection(hailMessage, msgIn);
                            break;
                    }
                }
            }
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

            string path = defaultFilePath + defaultFileName;
            Console.WriteLine($"Reading config file from: \"{path}\"...");
            if (File.Exists(defaultFilePath + defaultFileName))
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
                            case "dbserver":
                                dbServer = configDetails[1];
                                break;
                            case "dbname":
                                dbName = configDetails[1];
                                break;
                            case "dbuser":
                                dbUser = configDetails[1];
                                break;
                            case "dbpassword":
                                dbPassword = configDetails[1];
                                break;

                            case "serverauthstring":
                                defaultServerAuthString = configDetails[1];
                                break;
                            case "serverport":
                                defaultServerPort = int.Parse(configDetails[1]);
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

                    if (defaultServerAuthString.Length == 0)
                    {
                        Console.Write("Please enter ServerAuthString: ");
                        defaultServerAuthString = Console.ReadLine();
                    }

                    if (defaultServerPort == 0)
                    {
                        Console.Write("Please enter ServerPort: ");
                        defaultServerPort = int.Parse(Console.ReadLine());
                    }
                    Console.WriteLine("Setting up database configuration...");

                    if (dbServer.Length == 0)
                    {
                        Console.Write("Please enter DatabaseServer: ");
                        dbServer = Console.ReadLine();
                    }

                    if (dbName.Length == 0)
                    {
                        Console.Write("Please enter DatabaseName: ");
                        dbName = Console.ReadLine();
                    }

                    if (dbUser.Length == 0)
                    {
                        Console.Write("Please enter DatabaseUser: ");
                        dbUser = Console.ReadLine();
                    }

                    if (dbPassword.Length == 0)
                    {
                        Console.Write("Please enter DatabasePassword: ");
                        dbPassword = Console.ReadLine();
                    }

                    Console.WriteLine("Saving server configuration...");

                    string[] defaultContent = new string[1] { "" +
                    "#Server details:"+
                    $"\nServerAuthString={defaultServerAuthString}"+
                    $"\nServerPort={defaultServerPort}" +
                    "\n#Database details:" +
                    $"\nDBServer={dbServer}" +
                    $"\nDBName={dbName}" +
                    $"\nDBUser={dbUser}" +
                    $"\nDBPassword={dbPassword}" };

                    File.WriteAllLines(path, defaultContent);

                    Console.WriteLine("Server configuration saved.");

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    LoadConfig();
                }
            }
        }

        public void LoadMaps()
        {
            DataHandler.Maps.Add(0, new AuthoryMap(0, "Map Zero"));
            DataHandler.Maps.Add(1, new AuthoryMap(1, "Map 1"));
        }
        private void SearchDisconnection()
        {
            foreach (var account in DataHandler.OnlineAccounts)
            {
                if (account.Connection.Status == NetConnectionStatus.Disconnected)
                {
                    if (account.ConnectedCharacter != null)
                    {
                        Console.WriteLine("Requesting character info...");
                        OutgoingMessageHandler.Instance.RequestCharacterInfo(account.ConnectedCharacter, account.ConnectedServerMap);
                    }
                }
            }
        }

        public void CheckNodeConnections()
        {
            for (int i = DataHandler.Nodes.Count - 1; i >= 0; i--)
            {
                if (DataHandler.Nodes[i].NodeMasterConnection.Status == NetConnectionStatus.Disconnected)
                {
                    AuthoryNode node = DataHandler.Nodes[i];
                    Console.WriteLine($"Removing node: {node}");
                    DataHandler.RemoveNode(node);


                }
            }
        }

    }
}