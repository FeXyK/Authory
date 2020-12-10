using System;
using System.Diagnostics;
using System.Threading;
using Lidgren.Network;
using AuthoryServer.Entities;
using AuthoryServer.Server.Handlers;
using System.IO;
using AuthoryServer.Entities.EntityDerived;

namespace AuthoryServer
{
    /// <summary>
    /// MapServer clients connects to this server for playing on a map. 
    /// One MapServer provides one map.
    /// </summary>
    public class AuthoryServer
    {
        public bool ServerUp { get; private set; } = false;
        public int PlayerTickRate { get; private set; } = 50;
        public int MobTickRate { get; private set; }

        private const int MOB_UPDATE_AT_TICK = 1;
        private const int MOB_TICK_SLEEP = 50;
        private const int PLAYER_TICK_SLEEP = 50;

        public int ServerIndex { get; private set; }
        public int MapIndex { get; private set; }
        public string MapName { get; private set; }

        public NetServer NetServer { get; private set; }
        public WorldDataHandler Data { get; private set; }
        public OutgoingMessageHandler OutgoingMessageHandler { get; private set; }
        public IncomingMessageHandler IncomingMessageHandler { get; private set; }

        private NetPeerConfiguration _netPeerConfig;
        private Thread playerUpdateThread;
        private Thread mobUpdateThread;

        private static uint _updateTick;
        private bool serverRunning;

        private Stopwatch statisticsStopwatch;
        private Stopwatch mobStopwatch;

        private Statistics statistics;

        private string mapsFolderPath = "";

        /// <summary>
        /// Saves ServerIndex given by the ServerNode
        /// </summary>
        /// <param name="serverIndex"></param>
        public AuthoryServer(int serverIndex, string mapsFolderPath)
        {
            this.ServerIndex = serverIndex;
            this.mapsFolderPath = mapsFolderPath;
        }

        /// <summary>
        /// Initialize MapServer
        /// </summary>
        /// <param name="port">Port where the MapServer will run</param>
        /// <param name="connName">Connection string for connection</param>
        /// <param name="mapName">Name of the map</param>
        /// <param name="mapIndex">Index of the map</param>
        public void Init(int port, string connName, string mapName, int mapIndex)
        {
            MapName = mapName;
            MapIndex = mapIndex;

            statisticsStopwatch = new Stopwatch();
            mobStopwatch = new Stopwatch();

            statisticsStopwatch.Start();
            mobStopwatch.Start();

            statistics = Statistics.None;

            _netPeerConfig = new NetPeerConfiguration(connName);
            _netPeerConfig.SetMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval, true);
            _netPeerConfig.PingInterval = 3f;
            _netPeerConfig.ConnectionTimeout = 6.5f;
            //_netPeerConfig.SimulatedMinimumLatency = 0.1f;
            //_netPeerConfig.SimulatedRandomLatency = 0.01f;
            _netPeerConfig.Port = (int)port;

            NetServer = new NetServer(_netPeerConfig);
            Data = new WorldDataHandler(ServerIndex);
            OutgoingMessageHandler = new OutgoingMessageHandler(NetServer, Data);
            IncomingMessageHandler = new IncomingMessageHandler(this);

            InitTeleporter(new Vector3(450, 0, 450), 2.5f, mapIndex == 0 ? 1 : 0);

            InitSpawners(mapsFolderPath);

            Console.WriteLine("Server UP");
            ServerUp = true;
        }

        private void InitTeleporter(Vector3 position, float radius, int targetMapIndex)
        {
            TeleportEntity teleport = new TeleportEntity(this, targetMapIndex, position, radius);

            Data.Add(teleport);
        }


        /// <summary>
        /// Starts the NetServer, players will be able to connect after this
        /// Creates and Starts the PlayerUpdateThread
        /// Creates and Starts the MobUpdateThread
        /// </summary>
        public void Start()
        {
            NetServer.Start();

            serverRunning = true;

            playerUpdateThread = new Thread(new ThreadStart(PlayerUpdateLoop)) { IsBackground = true };
            playerUpdateThread.Start();

            mobUpdateThread = new Thread(new ThreadStart(MobUpdateLoop)) { IsBackground = true };
            mobUpdateThread.Start();

            //Mobs can run on multiple threads if needed
            //StartMobUpdateThreads();
        }

        /// <summary>
        /// Calls the PlayerUpdate() after every PLAYER_TICK_SLEEP ms
        /// Also calculates the time for the PlayerUpdate() call,
        /// so it will be able to Sleep the thread for the required amount
        /// With these it will be able to produce fix cycle counts per second
        /// </summary>
        private void PlayerUpdateLoop()
        {
            int processTime;
            int tickCounter = 0;
            while (serverRunning)
            {
                if (statisticsStopwatch.ElapsedMilliseconds > 1000)
                {
                    statisticsStopwatch.Restart();
                    if (statistics == Statistics.PlayerTicks || statistics == Statistics.AllTicks)
                        Console.WriteLine($"PlayerTicks: {tickCounter}/s");
                    PlayerTickRate = tickCounter;
                    tickCounter = 0;
                }
                tickCounter++;
                processTime = (int)PlayerUpdate();
                Thread.Sleep(PLAYER_TICK_SLEEP - (processTime >= PLAYER_TICK_SLEEP ? PLAYER_TICK_SLEEP : processTime));
            }
        }

        /// <summary>
        /// Same as the PlayerUpdateLoop()
        /// </summary>
        private void MobUpdateLoop()
        {
            int processTime;
            int tickCounter = 0;
            long startTime;
            long bytesSent = 0;
            while (serverRunning)
            {
                if (mobStopwatch.ElapsedMilliseconds > 1000)
                {
                    mobStopwatch.Restart();
                    if (statistics == Statistics.MobTicks || statistics == Statistics.AllTicks)
                    {
                        Console.WriteLine($"MobTicks : {tickCounter}/s");
                    }
                    if (statistics == Statistics.SentBytes || statistics == Statistics.AllTicks)
                    {
                        Console.WriteLine($"SentMB: {(OutgoingMessageHandler.OverallBytesSent - bytesSent) / 1024f / 1024f:0.0} MB");
                        bytesSent = OutgoingMessageHandler.OverallBytesSent;
                        //Console.WriteLine($"OverallSentKB: {MessageHandler.OverallBytesSent} byte\n{MessageHandler.OverallBytesSent / 1024} KB");
                    }
                    MobTickRate = tickCounter;
                    tickCounter = 0;
                }
                startTime = mobStopwatch.ElapsedMilliseconds;

                tickCounter++;
                MobUpdate();

                processTime = (int)(mobStopwatch.ElapsedMilliseconds - startTime);
                Thread.Sleep(MOB_TICK_SLEEP - (processTime >= MOB_TICK_SLEEP ? MOB_TICK_SLEEP : processTime));
            }
        }



        /// <summary>
        /// Called every server frame in the PlayerUpdateThread
        /// Calls the Tick() methot for every online PlayerEntity
        /// Also checks for Disconnected clients
        /// 
        /// Sends the Movement info to clients of players who changed position in the frame
        /// </summary>
        /// <returns></returns>
        public long PlayerUpdate()
        {
            long startMils = statisticsStopwatch.ElapsedMilliseconds;

            //READ JOB FROM QUE AND HANDLE THAT
            //Update players
            foreach (var player in Data.PlayersByUid.Values)
            {
                player.Tick();

                if (player.Connection != null)
                    if (player.Connection.Status == NetConnectionStatus.Disconnected)
                    {
                        if (Data.RemovePlayer(player))
                        {
                            Console.WriteLine($"Player disconnected:\n{player}");
                            OutgoingMessageHandler.SendDisconnect(player);
                        }
                    }
            }

            OutgoingMessageHandler.SendPlayerMovementInfoByGridCell();

            //MessageSender.Instance.SendPlayerMovementInfoByPlayers();

            _updateTick++;
            return statisticsStopwatch.ElapsedMilliseconds - startMils;
        }

        /// <summary>
        /// Calls the Tick() of every Server side entities(mobs)
        /// </summary>
        public void MobUpdate()
        {

            if (_updateTick % MOB_UPDATE_AT_TICK == 0)
            {
                foreach (var grid in Data.Grid)
                //foreach (var grid in Data.Regions[(int)region])
                {
                    foreach (var mob in grid.MobEntities.Values)
                    {
                        mob.Tick();
                    }
                }
            }
        }



        /// <summary>
        /// For debugging purposes.  
        /// Rotates the statistics variable, 
        /// </summary>
        public void ShowNextStatistic()
        {
            if ((int)statistics < Enum.GetNames(typeof(Statistics)).Length - 1)
            {
                statistics++;
            }
            else statistics = 0;
            Console.WriteLine("\nShow statistics of: " + statistics.ToString());
        }

        /// <summary>
        /// For testing purposes. 
        /// Adds mobs to the WorldData
        /// </summary>
        /// <param name="vector3"></param>
        /// <param name="count"></param>
        public void AddMobsForTesting()
        {
            InitSpawners(mapsFolderPath);
        }

        /// <summary>
        /// For testing purposes. 
        /// Adds simulated players to the WorldData
        /// </summary>
        /// <param name="position"></param>
        public void AddPlayerForTesting(Vector3 position)
        {
            Data.Add(new PlayerEntity("TestPlayer" + Data.PlayersByUid.Count, ModelType.GreenPlayer, position, this), position);
        }

        /// <summary>
        /// Spawns the mobs on the MapServer, by reading the config for the current MapIndex
        /// </summary>
        private void InitSpawners(string mapsFolderPath)
        {
            //MobSpawner spawner = new MobSpawner(ModelType.MeleeNPC, new Vector3(1000, 0, 1000), 500f, 5000);
            //spawner.SpawnAll(this);
            try
            {
                Console.WriteLine("Reading spawner info...");
                Console.WriteLine("Spawner info read.");
                if (mapsFolderPath.Length > 0)
                    mapsFolderPath += "/";
                string[] spawnerInfos = File.ReadAllLines(mapsFolderPath + "Map" + MapIndex + ".spawner");
                byte mobType = byte.Parse(spawnerInfos[0]);

                Console.WriteLine("Creating spawners...");
                for (int i = 1; i < spawnerInfos.Length; i++)
                {
                    Console.WriteLine("Creating spawner...");
                    string[] spawnerInfo = spawnerInfos[i].Split(';');
                    float x = float.Parse(spawnerInfo[0]);
                    float z = float.Parse(spawnerInfo[1]);
                    float r = float.Parse(spawnerInfo[2]);
                    int count = int.Parse(spawnerInfo[3]);
                    MobSpawner spawner = new MobSpawner((ModelType)mobType, new Vector3(x, 0, z), r, (ushort)count);
                    Console.WriteLine("Spawner created.");
                    Console.WriteLine("Spawning mobs...");
                    spawner.SpawnAll(this);
                    Console.WriteLine("Spawner mobs spawned.");
                }
                Console.WriteLine("Mobs spawned.");

            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e);
            }
        }

        public void Shutdown()
        {
            serverRunning = false;
            playerUpdateThread.Interrupt();
            mobUpdateThread.Interrupt();
        }
    }
}
