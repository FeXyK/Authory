using System;

namespace AuthoryMasterServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing...");
            AuthoryMasterServer masterServer = new AuthoryMasterServer();
            Console.WriteLine("Initialized.");

            Console.WriteLine("Starting...");
            masterServer.Start();
            Console.WriteLine("Server up...");

            Console.WriteLine();
            ConsoleKey key;
            while (true)
            {
                key = Console.ReadKey().Key;
                Console.WriteLine();
                switch (key)
                {
                    case ConsoleKey.L:
                        masterServer.DataHandler.ListNodes();
                        break;
                }
            }
        }
    }
}

