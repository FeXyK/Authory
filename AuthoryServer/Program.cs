using System;
using System.Diagnostics;
using AuthoryServer.Entities;

namespace AuthoryServer
{
    class Program
    {

        public static AuthoryMaster Master { get; set; }

        static void Main(string[] args)
        {
            //Enable HighPrecisionTimer to achieve 1ms internal clock = Thread.Sleep will be able to sleep for 1ms instead of 15.6ms
            HighPrecisionTimer.Enable();

            //Creating server node
            Master = new AuthoryMaster();



            //For debugging purposes
            Console.WriteLine("Is High Resolution timer enabled: " + Stopwatch.IsHighResolution);
            ConsoleKey key;
            while ((key = Console.ReadKey().Key) != ConsoleKey.Escape && AuthoryMaster.Instance.NodeUp)
            {
                try
                {

                    foreach (var map in Master.MapServers)
                    {
                        Console.WriteLine(map.MapIndex);
                    }

                    int mapIndex = Master.MapServers.Count > 1 ? int.Parse(Console.ReadLine()) : 0;

                    switch (key)
                    {
                        case ConsoleKey.G:
                            foreach (var map in AuthoryMaster.Instance.MapServers)
                            {
                                Console.WriteLine("------------------");
                                Console.WriteLine(map.MapName);
                                Console.WriteLine(map.MapIndex);
                                Console.WriteLine(map.Data.PlayersById.Count);
                                Console.WriteLine();
                            }
                            break;
                        case ConsoleKey.L:
                            Console.WriteLine();
                            Console.WriteLine("List by grid cells");
                            foreach (var entitiesContainer in Master.MapServers[mapIndex].Data.Grid)
                            {
                                foreach (var player in entitiesContainer.PlayersById.Values)
                                {
                                    Console.WriteLine(player);
                                }
                            }
                            Console.WriteLine("---------------------------");
                            Console.WriteLine("List by DataHandler");
                            foreach (var player in Master.MapServers[mapIndex].Data.PlayersById.Values)
                            {
                                Console.WriteLine(player);
                            }
                            Console.WriteLine("Player count: " + GridCell.AllPlayerCount);
                            Console.WriteLine("Entity count: " + GridCell.AllEntityCount);
                            break;
                        case ConsoleKey.Q:
                            foreach (var grid in Master.MapServers[mapIndex].Data.Grid)
                            {
                                foreach (var mob in grid.MobEntities.Values)
                                {
                                    Console.WriteLine(mob.Id + ": " + mob);
                                }
                            }
                            break;
                        case ConsoleKey.O:
                            Console.Write("\nEnter the ID of the entity: ");
                            int id = int.Parse(Console.ReadLine());
                            Entity entity = Master.MapServers[mapIndex].Data.Get((ushort)id);

                            Console.WriteLine("Id: {0}", entity.ToString());
                            break;
                        case ConsoleKey.P:
                            Console.WriteLine();
                            for (int i = 0; i < 100; i++)
                                Master.MapServers[mapIndex].AddPlayerForTesting(new Vector3(new Random().Next(100, 1900), 0, new Random().Next(100, 1900)));
                            break;
                        case ConsoleKey.M:
                            Console.WriteLine();
                            Master.MapServers[mapIndex].AddMobsForTesting();
                            break;
                        case ConsoleKey.S:
                            Console.WriteLine();
                            Master.MapServers[mapIndex].ShowNextStatistic();
                            break;
                        case ConsoleKey.V:
                            Console.WriteLine();
                            Console.WriteLine($"Number of players: {Master.MapServers[mapIndex].Data.PlayersById.Count} ");
                            Console.WriteLine($"Number of mobs: {GridCell.AllEntityCount - GridCell.AllPlayerCount} ");
                            break;
                        case ConsoleKey.X:
                            AuthoryMaster.Instance.Shutdown();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            //Server.Shutdown();
            Console.WriteLine("Press Enter to close the window!");
            Console.ReadLine();

            HighPrecisionTimer.Disable();

        }
    }
}
